using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsPreCheckinServiceTests
{
    private readonly Mock<IKidsPreCheckinRepository> _preCheckinRepository = new();
    private readonly Mock<IResponsavelCriancaRepository> _responsavelRepository = new();
    private readonly Mock<ICriancaDetalheRepository> _criancaDetalheRepository = new();
    private readonly Mock<IKidsCheckinRepository> _checkinRepository = new();
    private readonly Mock<IEventoOcorrenciaRepository> _eventoOcorrenciaRepository = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();
    private readonly Mock<IPessoaRepository> _pessoaRepository = new();
    private readonly Mock<IKidsService> _kidsService = new();
    private readonly Mock<ILogger<KidsPreCheckinService>> _logger = new();

    private readonly KidsPreCheckinService _service;

    public KidsPreCheckinServiceTests()
    {
        _authorizationService.Setup(a => a.GetCurrentContextAsync())
            .ReturnsAsync(new KidsAuthorizationContext
            {
                UsuarioId = 15,
                PessoaId = 30,
                TenantId = 1,
                TipoUsuario = TipoUsuario.Portal,
                PerfisAtivos = Array.Empty<PerfilPessoa>()
            });
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);

        _service = new KidsPreCheckinService(
            _preCheckinRepository.Object,
            _responsavelRepository.Object,
            _criancaDetalheRepository.Object,
            _checkinRepository.Object,
            _eventoOcorrenciaRepository.Object,
            _authorizationService.Object,
            _pessoaRepository.Object,
            _kidsService.Object,
            _logger.Object);
    }

    [Fact]
    public async Task CriarMeuPreCheckinAsync_DeveCriarPreCheckinQuandoCriancaElegivel()
    {
        const int criancaPessoaId = 10;

        _responsavelRepository.Setup(r => r.ExisteVinculoAtivoAsync(criancaPessoaId, 30)).ReturnsAsync(true);
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(criancaPessoaId))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = criancaPessoaId, DataCadastro = DateTime.UtcNow });
        _pessoaRepository.Setup(r => r.GetByIdAsync(criancaPessoaId))
            .ReturnsAsync(new Pessoa { Id = criancaPessoaId, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow });
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(criancaPessoaId)).ReturnsAsync((KidsCheckin?)null);
        _preCheckinRepository.Setup(r => r.GetAtivoPorCriancaESessaoAsync(criancaPessoaId, null)).ReturnsAsync((KidsPreCheckin?)null);
        _preCheckinRepository.Setup(r => r.CreateAsync(It.IsAny<KidsPreCheckin>()))
            .ReturnsAsync((KidsPreCheckin item) =>
            {
                item.Id = 77;
                item.Crianca = new Pessoa { Id = criancaPessoaId, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow };
                item.Responsavel = new Pessoa { Id = 30, Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
                return item;
            });

        var result = await _service.CriarMeuPreCheckinAsync(new CreateKidsPreCheckinRequest
        {
            CriancaPessoaId = criancaPessoaId,
            Observacoes = "Vai chegar com a avó"
        });

        result.Id.Should().Be(77);
        result.CriancaPessoaId.Should().Be(criancaPessoaId);
        result.ResponsavelPessoaId.Should().Be(30);
        result.Status.Should().Be("Pending");
        result.QrToken.Should().NotBeNullOrWhiteSpace();
        result.CodigoCurto.Should().HaveLength(8);
        _preCheckinRepository.Verify(r => r.CreateAsync(It.Is<KidsPreCheckin>(item =>
            item.CriancaPessoaId == criancaPessoaId &&
            item.ResponsavelPessoaId == 30 &&
            item.Status == "Pending" &&
            item.ObservacoesResponsavel == "Vai chegar com a avó")), Times.Once);
    }

    [Fact]
    public async Task CriarMeuPreCheckinAsync_DeveRetornarExistenteQuandoJaHouverPreCheckinAtivo()
    {
        const int criancaPessoaId = 10;
        var existente = new KidsPreCheckin
        {
            Id = 22,
            CriancaPessoaId = criancaPessoaId,
            ResponsavelPessoaId = 30,
            Status = "Pending",
            QrToken = "TOKEN123",
            CodigoCurto = "ABCD2345",
            ExpiraEm = DateTime.UtcNow.AddMinutes(5),
            CriadoEm = DateTime.UtcNow.AddMinutes(-1),
            Crianca = new Pessoa { Id = criancaPessoaId, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
            Responsavel = new Pessoa { Id = 30, Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
        };

        _responsavelRepository.Setup(r => r.ExisteVinculoAtivoAsync(criancaPessoaId, 30)).ReturnsAsync(true);
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(criancaPessoaId))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = criancaPessoaId, DataCadastro = DateTime.UtcNow });
        _pessoaRepository.Setup(r => r.GetByIdAsync(criancaPessoaId))
            .ReturnsAsync(new Pessoa { Id = criancaPessoaId, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow });
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(criancaPessoaId)).ReturnsAsync((KidsCheckin?)null);
        _preCheckinRepository.Setup(r => r.GetAtivoPorCriancaESessaoAsync(criancaPessoaId, null)).ReturnsAsync(existente);

        var result = await _service.CriarMeuPreCheckinAsync(new CreateKidsPreCheckinRequest
        {
            CriancaPessoaId = criancaPessoaId
        });

        result.Id.Should().Be(22);
        result.CodigoCurto.Should().Be("ABCD2345");
        _preCheckinRepository.Verify(r => r.CreateAsync(It.IsAny<KidsPreCheckin>()), Times.Never);
    }

    [Fact]
    public async Task CriarMeuPreCheckinAsync_DeveBloquearQuandoCriancaJaEstiverEmCheckin()
    {
        const int criancaPessoaId = 10;

        _responsavelRepository.Setup(r => r.ExisteVinculoAtivoAsync(criancaPessoaId, 30)).ReturnsAsync(true);
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(criancaPessoaId))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = criancaPessoaId, DataCadastro = DateTime.UtcNow });
        _pessoaRepository.Setup(r => r.GetByIdAsync(criancaPessoaId))
            .ReturnsAsync(new Pessoa { Id = criancaPessoaId, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow });
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(criancaPessoaId))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 99,
                CriancaPessoaId = criancaPessoaId,
                Status = "CheckedIn"
            });

        await _service.Invoking(s => s.CriarMeuPreCheckinAsync(new CreateKidsPreCheckinRequest
        {
            CriancaPessoaId = criancaPessoaId
        }))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*check-in ativo*");
    }

    [Fact]
    public async Task GetPendentesAsync_DeveBloquearQuandoUsuarioNaoForOperador()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de operador, líder ou administrativo."));

        await _service.Invoking(s => s.GetPendentesAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*operador, líder ou administrativo*");
    }

    [Fact]
    public async Task ConfirmarAsync_DeveGerarCheckinEConfirmarPreCheckin()
    {
        var item = new KidsPreCheckin
        {
            Id = 5,
            CriancaPessoaId = 10,
            ResponsavelPessoaId = 30,
            Status = "Pending",
            QrToken = "TOKEN123",
            CodigoCurto = "ABCD1234",
            ExpiraEm = DateTime.UtcNow.AddMinutes(10),
            CriadoEm = DateTime.UtcNow.AddMinutes(-2),
            Crianca = new Pessoa { Id = 10, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
            Responsavel = new Pessoa { Id = 30, Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
        };

        _preCheckinRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(item);
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(10)).ReturnsAsync((KidsCheckin?)null);
        _kidsService.Setup(s => s.CheckinAsync(It.IsAny<CheckinRequest>()))
            .ReturnsAsync(new CheckinResponse
            {
                CheckinId = 91,
                CodigoSessao = "SESSAO123",
                CheckinTime = DateTime.UtcNow
            });
        _preCheckinRepository.Setup(r => r.UpdateAsync(It.IsAny<KidsPreCheckin>()))
            .ReturnsAsync((KidsPreCheckin preCheckin) => preCheckin);

        var result = await _service.ConfirmarAsync(5, new ConfirmKidsPreCheckinRequest
        {
            SalaId = "SALA-BERCARIO",
            TurmaId = "TURMA-1",
            ObservacoesEquipe = "Recepção liberada"
        });

        result.Status.Should().Be("Confirmed");
        result.CheckinId.Should().Be(91);
        _kidsService.Verify(s => s.CheckinAsync(It.Is<CheckinRequest>(request =>
            request.CriancaPessoaId == 10 &&
            request.Metodo == "PRECHECKIN" &&
            request.CheckinByPessoaId == 30 &&
            request.Observacoes != null &&
            request.Observacoes.Contains("Equipe: Recepção liberada"))), Times.Once);
        _preCheckinRepository.Verify(r => r.UpdateAsync(It.Is<KidsPreCheckin>(preCheckin =>
            preCheckin.Id == 5 &&
            preCheckin.Status == "Confirmed" &&
            preCheckin.CheckinId == 91 &&
            preCheckin.ConfirmadoPorPessoaId == 30 &&
            preCheckin.SalaId == "SALA-BERCARIO" &&
            preCheckin.TurmaId == "TURMA-1")), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_DeveCancelarPreCheckinPendente()
    {
        var item = new KidsPreCheckin
        {
            Id = 8,
            CriancaPessoaId = 10,
            ResponsavelPessoaId = 30,
            Status = "Pending",
            QrToken = "TOKEN888",
            CodigoCurto = "ZXCV1234",
            ExpiraEm = DateTime.UtcNow.AddMinutes(8),
            CriadoEm = DateTime.UtcNow.AddMinutes(-1),
            Crianca = new Pessoa { Id = 10, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
            Responsavel = new Pessoa { Id = 30, Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
        };

        _preCheckinRepository.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(item);
        _preCheckinRepository.Setup(r => r.UpdateAsync(It.IsAny<KidsPreCheckin>()))
            .ReturnsAsync((KidsPreCheckin preCheckin) => preCheckin);

        var result = await _service.CancelarAsync(8, new CancelKidsPreCheckinRequest
        {
            Motivo = "Família não veio"
        });

        result.Status.Should().Be("Cancelled");
        result.CancelamentoMotivo.Should().Be("Família não veio");
    }

    [Fact]
    public async Task CancelarMeuPreCheckinAsync_DeveCancelarPreCheckinDoResponsavel()
    {
        var item = new KidsPreCheckin
        {
            Id = 18,
            CriancaPessoaId = 10,
            ResponsavelPessoaId = 30,
            Status = "Pending",
            QrToken = "TOKEN999",
            CodigoCurto = "QWER1234",
            ExpiraEm = DateTime.UtcNow.AddMinutes(8),
            CriadoEm = DateTime.UtcNow.AddMinutes(-1),
            Crianca = new Pessoa { Id = 10, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
            Responsavel = new Pessoa { Id = 30, Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
        };

        _preCheckinRepository.Setup(r => r.GetByIdAsync(18)).ReturnsAsync(item);
        _preCheckinRepository.Setup(r => r.UpdateAsync(It.IsAny<KidsPreCheckin>()))
            .ReturnsAsync((KidsPreCheckin preCheckin) => preCheckin);

        var result = await _service.CancelarMeuPreCheckinAsync(18, new CancelKidsPreCheckinRequest());

        result.Status.Should().Be("Cancelled");
        result.CancelamentoMotivo.Should().Be("Cancelado pela família no AppKids.");
    }
}
