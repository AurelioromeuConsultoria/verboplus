using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsEstruturaServiceTests
{
    private readonly Mock<IKidsEstruturaRepository> _repository = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();
    private readonly KidsEstruturaService _service;

    public KidsEstruturaServiceTests()
    {
        _authorizationService.Setup(a => a.EnsureLiderAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);
        _service = new KidsEstruturaService(_repository.Object, _authorizationService.Object);
    }

    [Fact]
    public async Task CreateSalaAsync_DeveCriarSalaComIdNormalizado()
    {
        _repository.Setup(r => r.GetSalaByIdAsync("SALA_BERCARIO")).ReturnsAsync((KidsSala?)null);
        _repository.Setup(r => r.CreateSalaAsync(It.IsAny<KidsSala>()))
            .ReturnsAsync((KidsSala item) => item);

        var result = await _service.CreateSalaAsync(new CreateKidsSalaRequest
        {
            Id = " sala_bercario ",
            Nome = "Berçário",
            CapacidadeMaxima = 12,
            Ativo = true
        });

        result.Id.Should().Be("SALA_BERCARIO");
        result.Nome.Should().Be("Berçário");
        result.CapacidadeMaxima.Should().Be(12);
    }

    [Fact]
    public async Task CreateTurmaAsync_DeveExigirSalaAtiva()
    {
        _repository.Setup(r => r.GetTurmaByIdAsync("TURMA_1")).ReturnsAsync((KidsTurma?)null);
        _repository.Setup(r => r.GetSalaByIdAsync("SALA_1"))
            .ReturnsAsync(new KidsSala
            {
                Id = "SALA_1",
                Nome = "Sala 1",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
        _repository.Setup(r => r.CreateTurmaAsync(It.IsAny<KidsTurma>()))
            .ReturnsAsync((KidsTurma item) => item);

        var result = await _service.CreateTurmaAsync(new CreateKidsTurmaRequest
        {
            Id = "turma_1",
            SalaId = "sala_1",
            Nome = "Maternal A",
            CapacidadeMaxima = 8,
            Ativo = true
        });

        result.Id.Should().Be("TURMA_1");
        result.SalaId.Should().Be("SALA_1");
        result.Nome.Should().Be("Maternal A");
    }

    [Fact]
    public async Task UpdateTurmaAsync_DeveFalharQuandoSalaNaoExiste()
    {
        _repository.Setup(r => r.GetTurmaByIdAsync("TURMA_1"))
            .ReturnsAsync(new KidsTurma
            {
                Id = "TURMA_1",
                SalaId = "SALA_ANTIGA",
                Nome = "Maternal",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
        _repository.Setup(r => r.GetSalaByIdAsync("SALA_NOVA"))
            .ReturnsAsync((KidsSala?)null);

        var action = async () => await _service.UpdateTurmaAsync("TURMA_1", new UpdateKidsTurmaRequest
        {
            SalaId = "sala_nova",
            Nome = "Maternal B",
            CapacidadeMaxima = 10,
            Ativo = true
        });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Sala não encontrada ou inativa.");
    }

    [Fact]
    public async Task CreateSalaAsync_DeveBloquearQuandoUsuarioNaoForLider()
    {
        _authorizationService.Setup(a => a.EnsureLiderAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de liderança ou administrativo."));

        await _service.Invoking(s => s.CreateSalaAsync(new CreateKidsSalaRequest
        {
            Id = "SALA_TESTE",
            Nome = "Sala Teste",
            CapacidadeMaxima = 10,
            Ativo = true
        }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*liderança ou administrativo*");
    }
}
