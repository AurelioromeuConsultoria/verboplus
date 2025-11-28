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
        repo.Setup(r => r.CreateAsync(It.IsAny<Equipe>())).ReturnsAsync((Equipe e) => { e.Id = 1; return e; });
        var service = new EquipeService(repo.Object);

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

        eqRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Equipe { Id = 1, Nome = "Recepcao", Area = AreaEquipe.Verde });
        cgRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Cargo { Id = 2, Nome = "Recepcionista" });

        volRepo.Setup(r => r.CreateAsync(It.IsAny<Voluntario>()))
               .ReturnsAsync((Voluntario v) => { v.Id = 10; return v; });
        volRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Voluntario { Id = 10, Nome = "Maria", WhatsApp = "111", Email = "m@x.com", EquipeId = 1, CargoId = 2, Equipe = new Equipe { Id = 1, Nome = "Recepcao" }, Cargo = new Cargo { Id = 2, Nome = "Recepcionista" } });

        var service = new VoluntarioService(volRepo.Object, eqRepo.Object, cgRepo.Object);

        var result = await service.CreateAsync(new CriarVoluntarioDto { Nome = "Maria", WhatsApp = "111", Email = "m@x.com", EquipeId = 1, CargoId = 2 });

        result.Id.Should().Be(10);
        result.NomeEquipe.Should().Be("Recepcao");
        result.NomeCargo.Should().Be("Recepcionista");
    }
}
