using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsServiceTests
{
    private readonly Mock<IPessoaRepository> _pessoaRepository = new();
    private readonly Mock<ICriancaDetalheRepository> _criancaDetalheRepository = new();
    private readonly Mock<IKidsEstruturaRepository> _kidsEstruturaRepository = new();
    private readonly Mock<IResponsavelCriancaRepository> _responsavelRepository = new();
    private readonly Mock<IKidsCheckinRepository> _checkinRepository = new();
    private readonly Mock<IKidsNotificacaoRepository> _notificacaoRepository = new();
    private readonly Mock<IPessoaPerfilRepository> _perfilRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<ICurrentUserContext> _currentUserContext = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();
    private readonly Mock<IComunicacaoAutomacaoService> _comunicacaoAutomacaoService = new();
    private readonly Mock<ILogger<KidsService>> _logger = new();
    private readonly Mock<IKidsPushNotificationService> _pushService = new();

    private readonly KidsService _service;

    public KidsServiceTests()
    {
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<Task<(CheckinResponse response, List<int> responsavelIds, string msg, string tipo)>>>()))        
            .Returns<Func<Task<(CheckinResponse response, List<int> responsavelIds, string msg, string tipo)>>>(
                async action => await action());
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.EnsureLiderAsync()).Returns(Task.CompletedTask);

        _service = new KidsService(
            _pessoaRepository.Object,
            _criancaDetalheRepository.Object,
            _kidsEstruturaRepository.Object,
            _responsavelRepository.Object,
            _checkinRepository.Object,
            _notificacaoRepository.Object,
            _perfilRepository.Object,
            _unitOfWork.Object,
            _usuarioRepository.Object,
            _currentUserContext.Object,
            _authorizationService.Object,
            _comunicacaoAutomacaoService.Object,
            _logger.Object,
            _pushService.Object);
    }

    [Fact]
    public async Task GetCriancasAsync_DeveBloquearQuandoUsuarioNaoForAdministrativo()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de operador, líder ou administrativo."));

        await _service.Invoking(s => s.GetCriancasAsync())
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*operador, líder ou administrativo*");
    }

    [Fact]
    public async Task GetMinhasCriancasAsync_DeveRetornarSomenteCriancasDoResponsavel()
    {
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 30, Ativo = true });
        _responsavelRepository.Setup(r => r.GetCriancaIdsAtivosByResponsavelIdAsync(30))
            .ReturnsAsync(new List<int> { 10 });
        _pessoaRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Pessoa>
            {
                new() { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow },
                new() { Id = 11, Nome = "Bia", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = 10, SalaId = "Sala 1", DataCadastro = DateTime.UtcNow });
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(10)).ReturnsAsync((KidsCheckin?)null);

        var result = (await _service.GetMinhasCriancasAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].PessoaId.Should().Be(10);
        result[0].Nome.Should().Be("Ana");
    }

    [Fact]
    public async Task GetMinhaCriancaByIdAsync_DeveBloquearQuandoCriancaNaoPertenceAoResponsavel()
    {
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 30, Ativo = true });
        _responsavelRepository.Setup(r => r.ExisteVinculoAtivoAsync(10, 30))
            .ReturnsAsync(false);

        await _service.Invoking(s => s.GetMinhaCriancaByIdAsync(10))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não tem acesso*");
    }

    [Fact]
    public async Task GetMeusCheckinsAsync_DeveRetornarApenasCheckinsDasCriancasDoResponsavel()
    {
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 30, Ativo = true });
        _responsavelRepository.Setup(r => r.GetCriancaIdsAtivosByResponsavelIdAsync(30))
            .ReturnsAsync(new List<int> { 10 });
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = 10, SalaId = "Sala 1", DataCadastro = DateTime.UtcNow });
        _checkinRepository.Setup(r => r.GetHistoricoPorCriancaAsync(10, 10))
            .ReturnsAsync(new List<KidsCheckin>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    CheckinTime = DateTime.UtcNow,
                    Status = "CheckedIn",
                    Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
                }
            });

        var result = (await _service.GetMeusCheckinsAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].CriancaPessoaId.Should().Be(10);
        result[0].CriancaNome.Should().Be("Ana");
    }

    [Fact]
    public async Task CheckinAsync_DeveRealizarCheckin_QuandoCriancaValidaESemCheckinAtivo()
    {
        var request = new CheckinRequest
        {
            CriancaPessoaId = 10,
            Metodo = "ADMIN",
            CheckinByPessoaId = 20
        };

        _currentUserContext.Setup(c => c.UserId).Returns(20);
        _usuarioRepository.Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(new Usuario { Id = 20, PessoaId = 20, Ativo = true, TipoUsuario = TipoUsuario.Admin });

        _pessoaRepository.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Pessoa
            {
                Id = 10,
                Nome = "Ana",
                TipoPessoa = TipoPessoa.Crianca,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(10))
            .ReturnsAsync((KidsCheckin?)null);
        _checkinRepository.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<KidsCheckin>()))
            .ReturnsAsync((KidsCheckin c) =>
            {
                c.Id = 99;
                return c;
            });
        _responsavelRepository.Setup(r => r.GetByCriancaIdAsync(10))
            .ReturnsAsync(new List<ResponsavelCrianca>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    ResponsavelPessoaId = 30,
                    Ativo = true,
                    PodeRetirar = true,
                    Responsavel = new Pessoa { Id = 30, Nome = "Maria", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
                }
            });
        _notificacaoRepository.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<KidsNotificacao>()))
            .ReturnsAsync((KidsNotificacao n) => n);

        var result = await _service.CheckinAsync(request);

        result.CheckinId.Should().Be(99);
        result.CodigoSessao.Should().NotBeNullOrWhiteSpace();
        result.Notificacoes.Should().ContainSingle();
        _comunicacaoAutomacaoService.Verify(
            p => p.ExecutarAvisoContextualKidsAsync(
                It.Is<ComunicacaoAvisoContextualKidsRequest>(req =>
                    req.CriancaPessoaId == 10 &&
                    req.ResponsavelPessoaIds.Single() == 30 &&
                    req.Titulo == "App Kids - Check-in"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckinAsync_DeveBloquear_QuandoCriancaJaPossuiCheckinAtivo()
    {
        var request = new CheckinRequest
        {
            CriancaPessoaId = 10,
            Metodo = "ADMIN",
            CheckinByPessoaId = 20
        };

        _currentUserContext.Setup(c => c.UserId).Returns(20);
        _usuarioRepository.Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(new Usuario { Id = 20, PessoaId = 20, Ativo = true, TipoUsuario = TipoUsuario.Admin });

        _pessoaRepository.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Pessoa
            {
                Id = 10,
                Nome = "Ana",
                TipoPessoa = TipoPessoa.Crianca,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
        _checkinRepository.Setup(r => r.GetCheckinAtivoPorCriancaAsync(10))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                CodigoSessao = "ABC123"
            });

        await _service.Invoking(s => s.CheckinAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*check-in ativo*");
    }

    [Fact]
    public async Task CheckoutAsync_DeveRealizarCheckout_QuandoResponsavelAutorizado()
    {
        var request = new CheckoutRequest
        {
            CriancaPessoaId = 10,
            CodigoSessao = "ABC123",
            CheckoutByPessoaId = 30,
            Metodo = "ADMIN"
        };

        _currentUserContext.Setup(c => c.UserId).Returns(20);
        _usuarioRepository.Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(new Usuario { Id = 20, PessoaId = 20, Ativo = true, TipoUsuario = TipoUsuario.Admin });

        _checkinRepository.Setup(r => r.GetByCodigoSessaoAsync("ABC123"))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                CodigoSessao = "ABC123",
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _responsavelRepository.Setup(r => r.PodeRetirarAsync(10, 30)).ReturnsAsync(true);
        _responsavelRepository.Setup(r => r.GetByCriancaIdAsync(10))
            .ReturnsAsync(new List<ResponsavelCrianca>
            {
                new()
                {
                    Id = 1,
                    CriancaPessoaId = 10,
                    ResponsavelPessoaId = 30,
                    Ativo = true,
                    PodeRetirar = true
                }
            });
        _pessoaRepository.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Pessoa
            {
                Id = 10,
                Nome = "Ana",
                TipoPessoa = TipoPessoa.Crianca,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
        _notificacaoRepository.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<KidsNotificacao>()))
            .ReturnsAsync((KidsNotificacao n) => n);

        await _service.CheckoutAsync(request);

        _checkinRepository.Verify(r => r.UpdateWithoutSaveAsync(It.Is<KidsCheckin>(c =>
            c.CheckoutByPessoaId == 30 &&
            c.Status == "CheckedOut" &&
            c.Metodo == "ADMIN")), Times.Once);
        _comunicacaoAutomacaoService.Verify(
            p => p.ExecutarAvisoContextualKidsAsync(
                It.Is<ComunicacaoAvisoContextualKidsRequest>(req =>
                    req.CriancaPessoaId == 10 &&
                    req.ResponsavelPessoaIds.Single() == 30 &&
                    req.Titulo == "App Kids - Check-out"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateCriancaAsync_DeveBloquearQuandoTurmaNaoPertenceASala()
    {
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 30, Ativo = true, TipoUsuario = TipoUsuario.Admin });
        _kidsEstruturaRepository.Setup(r => r.GetSalaByIdAsync("SALA_A"))
            .ReturnsAsync(new KidsSala { Id = "SALA_A", Nome = "Sala A", Ativo = true, DataCriacao = DateTime.UtcNow });
        _kidsEstruturaRepository.Setup(r => r.GetTurmaByIdAsync("TURMA_B"))
            .ReturnsAsync(new KidsTurma { Id = "TURMA_B", SalaId = "SALA_B", Nome = "Turma B", Ativo = true, DataCriacao = DateTime.UtcNow });

        await _service.Invoking(s => s.CreateCriancaAsync(new CreateCriancaRequest
        {
            Nome = "Ana",
            DataNascimento = new DateTime(2020, 1, 1),
            SalaId = "sala_a",
            TurmaId = "turma_b"
        }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*não pertence à sala*");
    }

    [Fact]
    public async Task CheckoutAsync_DeveBloquear_QuandoResponsavelNaoAutorizado()
    {
        var request = new CheckoutRequest
        {
            CriancaPessoaId = 10,
            CodigoSessao = "ABC123",
            CheckoutByPessoaId = 99,
            Metodo = "ADMIN"
        };

        _currentUserContext.Setup(c => c.UserId).Returns(20);
        _usuarioRepository.Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(new Usuario { Id = 20, PessoaId = 20, Ativo = true, TipoUsuario = TipoUsuario.Admin });

        _checkinRepository.Setup(r => r.GetByCodigoSessaoAsync("ABC123"))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                CodigoSessao = "ABC123"
            });
        _responsavelRepository.Setup(r => r.PodeRetirarAsync(10, 99)).ReturnsAsync(false);
        _pessoaRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync(new Pessoa
            {
                Id = 99,
                Nome = "Nao Autorizado",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = false,
                DataCriacao = DateTime.UtcNow
            });

        await _service.Invoking(s => s.CheckoutAsync(request))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não tem autorização*");
    }
}
