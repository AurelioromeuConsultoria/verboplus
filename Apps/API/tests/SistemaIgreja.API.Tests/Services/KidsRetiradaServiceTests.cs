using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsRetiradaServiceTests
{
    private readonly Mock<IKidsCheckinRepository> _checkinRepository = new();
    private readonly Mock<IResponsavelCriancaRepository> _responsavelRepository = new();
    private readonly Mock<ICriancaDetalheRepository> _criancaDetalheRepository = new();
    private readonly Mock<IPessoaRepository> _pessoaRepository = new();
    private readonly Mock<IKidsNotificacaoRepository> _notificacaoRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<ICurrentUserContext> _currentUserContext = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();
    private readonly Mock<IKidsPushNotificationService> _pushService = new();
    private readonly Mock<ILogger<KidsRetiradaService>> _logger = new();

    private readonly KidsRetiradaService _service;

    public KidsRetiradaServiceTests()
    {
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());
        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 50, Ativo = true, TipoUsuario = TipoUsuario.Admin });
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);

        _service = new KidsRetiradaService(
            _checkinRepository.Object,
            _responsavelRepository.Object,
            _criancaDetalheRepository.Object,
            _pessoaRepository.Object,
            _notificacaoRepository.Object,
            _unitOfWork.Object,
            _usuarioRepository.Object,
            _currentUserContext.Object,
            _authorizationService.Object,
            _logger.Object,
            _pushService.Object);
    }

    [Fact]
    public async Task ValidarAsync_DeveRetornarContextoQuandoTokenValido()
    {
        _checkinRepository.Setup(r => r.GetByTokenRetiradaAsync("TOKEN123"))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                CheckinTime = DateTime.UtcNow.AddMinutes(-30),
                TokenRetirada = "TOKEN123",
                PinRetirada = "123456",
                TokenRetiradaExpiraEm = DateTime.UtcNow.AddHours(2),
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe { PessoaId = 10, SalaId = "Sala 2", DataCadastro = DateTime.UtcNow });
        _responsavelRepository.Setup(r => r.GetByCriancaIdAsync(10))
            .ReturnsAsync(new List<ResponsavelCrianca>
            {
                new()
                {
                    CriancaPessoaId = 10,
                    ResponsavelPessoaId = 30,
                    PodeRetirar = true,
                    Ativo = true,
                    Parentesco = "Mãe",
                    Responsavel = new Pessoa { Id = 30, Nome = "Maria", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
                }
            });

        var result = await _service.ValidarAsync(new ValidarRetiradaRequest { Token = "TOKEN123" });

        result.CheckinId.Should().Be(1);
        result.CriancaNome.Should().Be("Ana");
        result.SalaId.Should().Be("Sala 2");
        result.MetodoValidado.Should().Be("QR");
        result.ResponsaveisAutorizados.Should().ContainSingle();
    }

    [Fact]
    public async Task ConfirmarAsync_DeveBloquearQuandoResponsavelNaoAutorizado()
    {
        _checkinRepository.Setup(r => r.GetByTokenRetiradaAsync("TOKEN123"))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                TokenRetirada = "TOKEN123",
                TokenRetiradaExpiraEm = DateTime.UtcNow.AddHours(1),
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _responsavelRepository.Setup(r => r.PodeRetirarAsync(10, 99)).ReturnsAsync(false);

        await _service.Invoking(s => s.ConfirmarAsync(new ConfirmarRetiradaRequest
        {
            CheckinId = 1,
            Token = "TOKEN123",
            ResponsavelPessoaId = 99,
            Metodo = "QR"
        }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não autorizado*");
    }

    [Fact]
    public async Task ConfirmarAsync_DeveRealizarCheckoutQuandoResponsavelAutorizado()
    {
        _checkinRepository.Setup(r => r.GetByTokenRetiradaAsync("TOKEN123"))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                TokenRetirada = "TOKEN123",
                TokenRetiradaExpiraEm = DateTime.UtcNow.AddHours(1),
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _responsavelRepository.Setup(r => r.PodeRetirarAsync(10, 30)).ReturnsAsync(true);
        _responsavelRepository.Setup(r => r.GetByCriancaIdAsync(10))
            .ReturnsAsync(new List<ResponsavelCrianca>
            {
                new() { CriancaPessoaId = 10, ResponsavelPessoaId = 30, Ativo = true, PodeRetirar = true }
            });
        _notificacaoRepository.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<KidsNotificacao>()))
            .ReturnsAsync((KidsNotificacao item) => item);

        await _service.ConfirmarAsync(new ConfirmarRetiradaRequest
        {
            CheckinId = 1,
            Token = "TOKEN123",
            ResponsavelPessoaId = 30,
            Metodo = "QR"
        });

        _checkinRepository.Verify(r => r.UpdateWithoutSaveAsync(It.Is<KidsCheckin>(c =>
            c.Id == 1 &&
            c.Status == "CheckedOut" &&
            c.CheckoutByPessoaId == 30 &&
            c.RetiradaConfirmadaPorPessoaId == 50 &&
            c.RetiradaMetodo == "QR" &&
            !c.RetiradaEmModoExcecao)), Times.Once);
        _pushService.Verify(p => p.SendToPessoasAsync(
            It.Is<IEnumerable<int>>(ids => ids.Single() == 30),
            "App Kids - Check-out",
            It.IsAny<string>(),
            It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarExcecaoAsync_DeveRealizarCheckoutEmModoExcecao()
    {
        _checkinRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new KidsCheckin
            {
                Id = 1,
                CriancaPessoaId = 10,
                Status = "CheckedIn",
                Crianca = new Pessoa { Id = 10, Nome = "Ana", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow }
            });
        _responsavelRepository.Setup(r => r.GetByCriancaIdAsync(10))
            .ReturnsAsync(new List<ResponsavelCrianca>
            {
                new() { CriancaPessoaId = 10, ResponsavelPessoaId = 30, Ativo = true, PodeRetirar = true }
            });
        _notificacaoRepository.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<KidsNotificacao>()))
            .ReturnsAsync((KidsNotificacao item) => item);

        await _service.RegistrarExcecaoAsync(new RetiradaExcecaoRequest
        {
            CheckinId = 1,
            PessoaRetirandoNome = "Tia Joana",
            PessoaRetirandoDocumento = "DOC123",
            Motivo = "Responsável sem celular",
            Observacoes = "Liberado pela coordenação"
        });

        _checkinRepository.Verify(r => r.UpdateWithoutSaveAsync(It.Is<KidsCheckin>(c =>
            c.Id == 1 &&
            c.Status == "CheckedOut" &&
            c.RetiradaEmModoExcecao &&
            c.RetiradaMetodo == "EXCECAO" &&
            c.RetiradaPessoaNome == "Tia Joana" &&
            c.RetiradaMotivoExcecao == "Responsável sem celular")), Times.Once);
    }

    [Fact]
    public async Task ValidarAsync_DeveBloquearQuandoUsuarioNaoForAdministrativo()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de operador, líder ou administrativo."));

        await _service.Invoking(s => s.ValidarAsync(new ValidarRetiradaRequest { Token = "TOKEN123" }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*operador, líder ou administrativo*");
    }
}
