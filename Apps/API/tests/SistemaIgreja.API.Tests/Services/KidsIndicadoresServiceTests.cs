using FluentAssertions;
using Moq;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsIndicadoresServiceTests
{
    private readonly Mock<IPessoaRepository> _pessoaRepository = new();
    private readonly Mock<IResponsavelCriancaRepository> _responsavelRepository = new();
    private readonly Mock<IKidsEstruturaRepository> _estruturaRepository = new();
    private readonly Mock<IKidsCheckinRepository> _checkinRepository = new();
    private readonly Mock<IKidsOcorrenciaRepository> _ocorrenciaRepository = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();

    private readonly KidsIndicadoresService _service;

    public KidsIndicadoresServiceTests()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.EnsureLiderAsync()).Returns(Task.CompletedTask);
        _service = new KidsIndicadoresService(
            _pessoaRepository.Object,
            _responsavelRepository.Object,
            _estruturaRepository.Object,
            _checkinRepository.Object,
            _ocorrenciaRepository.Object,
            _authorizationService.Object);
    }

    [Fact]
    public async Task GetIndicadoresAsync_DeveConsolidarMetricasDoPeriodo()
    {
        _pessoaRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Pessoa>
            {
                new() { Id = 1, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
                new() { Id = 2, Nome = "Bia", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
                new() { Id = 3, Nome = "Carlos", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _responsavelRepository.Setup(r => r.GetResponsavelIdsAtivosAsync())
            .ReturnsAsync(new[] { 10, 11, 10 });
        _estruturaRepository.Setup(r => r.GetSalasAsync(false))
            .ReturnsAsync(new[]
            {
                new KidsSala { Id = "SALA_1", Nome = "Sala 1", Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _estruturaRepository.Setup(r => r.GetTurmasAsync(null, false))
            .ReturnsAsync(new[]
            {
                new KidsTurma { Id = "TURMA_1", SalaId = "SALA_1", Nome = "Maternal", Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _checkinRepository.Setup(r => r.GetByPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new[]
            {
                new KidsCheckin { Id = 1, RetiradaMetodo = "QR" },
                new KidsCheckin { Id = 2, RetiradaMetodo = "PIN" },
                new KidsCheckin { Id = 3, RetiradaMetodo = "ADMIN", RetiradaEmModoExcecao = true }
            });
        _checkinRepository.Setup(r => r.GetCheckinsAtivosAsync())
            .ReturnsAsync(new[]
            {
                new KidsCheckin { Id = 1, CriancaPessoaId = 1, Status = "CheckedIn" }
            });
        _ocorrenciaRepository.Setup(r => r.GetAbertasAsync())
            .ReturnsAsync(new[]
            {
                new KidsOcorrencia { Id = 1, CriancaPessoaId = 1, Status = "Aberta", DataCriacao = DateTime.UtcNow },
                new KidsOcorrencia { Id = 2, CriancaPessoaId = 2, Status = "Em andamento", DataCriacao = DateTime.UtcNow }
            });

        var result = await _service.GetIndicadoresAsync(7);

        result.DiasAnalisados.Should().Be(7);
        result.TotalCriancasAtivas.Should().Be(2);
        result.TotalResponsaveisAtivos.Should().Be(2);
        result.TotalSalasAtivas.Should().Be(1);
        result.TotalTurmasAtivas.Should().Be(1);
        result.TotalCheckinsPeriodo.Should().Be(3);
        result.TotalRetiradasQr.Should().Be(1);
        result.TotalRetiradasPin.Should().Be(1);
        result.TotalRetiradasExcecao.Should().Be(1);
        result.TotalOcorrenciasAbertas.Should().Be(2);
        result.TotalCriancasPresentesAgora.Should().Be(1);
        result.MediaCheckinsPorDia.Should().Be(0.43m);
    }

    [Fact]
    public async Task GetIndicadoresAsync_DeveBloquearQuandoUsuarioNaoForOperador()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de operador, líder ou administrativo."));

        await _service.Invoking(s => s.GetIndicadoresAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*operador, líder ou administrativo*");
    }
}
