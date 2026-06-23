using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class EquipeCargoVoluntarioServicesTests
{
    [Fact]
    public async Task EquipeService_Create_MapsEnum()
    {
        var repo = new Mock<IEquipeRepository>();
        var usuarioRepo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.CreateAsync(It.IsAny<Equipe>())).ReturnsAsync((Equipe e) => { e.Id = 1; return e; });
        var service = new EquipeService(repo.Object, usuarioRepo.Object);

        var dto = new CriarEquipeDto { Nome = "Recepcao", Area = 1 };
        var result = await service.CreateAsync(dto);

        result.Area.Should().Be(1);
        result.Nome.Should().Be("Recepcao");
    }

    [Fact]
    public async Task CargoService_Update_Throws_WhenNotFound()
    {
        var repo = new Mock<ICargoRepository>();
        repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Cargo?)null);
        var service = new CargoService(repo.Object);

        await service.Invoking(s => s.UpdateAsync(99, new AtualizarCargoDto { Nome = "X" }))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task VoluntarioService_Create_ValidatesReferences_AndMaps()
    {
        var volRepo = new Mock<IVoluntarioRepository>();
        var eqRepo = new Mock<IEquipeRepository>();
        var cgRepo = new Mock<ICargoRepository>();
        var pessoaRepo = new Mock<IPessoaRepository>();

        eqRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Equipe { Id = 1, Nome = "Recepcao", Area = AreaEquipe.Verde });
        cgRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Cargo { Id = 2, Nome = "Recepcionista" });

        var pessoa = new Pessoa { Id = 1, Nome = "Maria", WhatsApp = "111", Email = "m@x.com", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        pessoaRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pessoa);

        volRepo.Setup(r => r.CreateAsync(It.IsAny<Voluntario>()))
               .ReturnsAsync((Voluntario v) => { v.Id = 10; return v; });

        var equipe = new Equipe { Id = 1, Nome = "Recepcao" };
        var cargo = new Cargo { Id = 2, Nome = "Recepcionista" };
        var voluntario = new Voluntario
        {
            Id = 10,
            PessoaId = 1,
            Pessoa = pessoa,
            EquipeId = 1,
            CargoId = 2,
            Equipe = equipe,
            Cargo = cargo,
            DataCadastro = DateTime.UtcNow
        };
        volRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(voluntario);

        var service = new VoluntarioService(volRepo.Object, eqRepo.Object, cgRepo.Object, pessoaRepo.Object);

        var result = await service.CreateAsync(new CriarVoluntarioDto { PessoaId = 1, WhatsApp = "111", Email = "m@x.com", EquipeId = 1, CargoId = 2 });

        result.Id.Should().Be(10);
        result.NomeEquipe.Should().Be("Recepcao");
        result.NomeCargo.Should().Be("Recepcionista");
    }

    [Fact]
    public async Task VoluntarioService_Delete_Succeeds_EvenWhenHasEscalasRelacionadas()
    {
        // EscalaItem.VoluntarioId agora é nullable (ON DELETE SET NULL), então remover
        // um voluntário da equipe é sempre permitido. O histórico de escalas é preservado
        // via EscalaItem.PessoaId, que permanece intacto.
        var volRepo = new Mock<IVoluntarioRepository>();
        var eqRepo = new Mock<IEquipeRepository>();
        var cgRepo = new Mock<ICargoRepository>();
        var pessoaRepo = new Mock<IPessoaRepository>();

        volRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Voluntario
        {
            Id = 10,
            PessoaId = 1,
            EquipeId = 1,
            CargoId = 2,
            Pessoa = new Pessoa { Id = 1, Nome = "Maria", Ativo = true, TipoPessoa = TipoPessoa.Adulto, DataCriacao = DateTime.UtcNow },
            Equipe = new Equipe { Id = 1, Nome = "Recepcao" },
            Cargo = new Cargo { Id = 2, Nome = "Recepcionista" }
        });
        volRepo.Setup(r => r.DeleteAsync(10)).Returns(Task.CompletedTask);

        var service = new VoluntarioService(volRepo.Object, eqRepo.Object, cgRepo.Object, pessoaRepo.Object);

        await service.Invoking(s => s.DeleteAsync(10)).Should().NotThrowAsync();

        volRepo.Verify(r => r.DeleteAsync(10), Times.Once);
    }
}
