using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoProcessamentoServiceTests
{
    private readonly Mock<IComunicacaoEntregaService> _entregaServiceMock = new();
    private readonly Mock<IComunicacaoEntregaRepository> _entregaRepositoryMock = new();
    private readonly Mock<IComunicacaoCanalProvider> _whatsAppProviderMock = new();
    private readonly Mock<IComunicacaoCanalProvider> _emailProviderMock = new();
    private readonly Mock<IComunicacaoCanalProvider> _pushProviderMock = new();
    private readonly Mock<IComunicacaoCanalProvider> _notificacaoInternaProviderMock = new();
    private readonly Mock<ILogger<ComunicacaoProcessamentoService>> _loggerMock = new();
    private readonly ComunicacaoProcessamentoService _service;

    public ComunicacaoProcessamentoServiceTests()
    {
        _whatsAppProviderMock.SetupGet(x => x.Canal).Returns(CanalComunicacao.WhatsApp);
        _whatsAppProviderMock.SetupGet(x => x.Nome).Returns("WhatsApp");
        _whatsAppProviderMock
            .Setup(x => x.ValidarConfiguracaoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalDiagnostico { Configurado = true });

        _emailProviderMock.SetupGet(x => x.Canal).Returns(CanalComunicacao.Email);
        _emailProviderMock.SetupGet(x => x.Nome).Returns("E-mail");
        _emailProviderMock
            .Setup(x => x.ValidarConfiguracaoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalDiagnostico { Configurado = true });

        _pushProviderMock.SetupGet(x => x.Canal).Returns(CanalComunicacao.Push);
        _pushProviderMock.SetupGet(x => x.Nome).Returns("Push");
        _pushProviderMock
            .Setup(x => x.ValidarConfiguracaoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalDiagnostico { Configurado = true });

        _notificacaoInternaProviderMock.SetupGet(x => x.Canal).Returns(CanalComunicacao.NotificacaoInterna);
        _notificacaoInternaProviderMock.SetupGet(x => x.Nome).Returns("Notificação interna");
        _notificacaoInternaProviderMock
            .Setup(x => x.ValidarConfiguracaoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalDiagnostico { Configurado = true });

        _service = new ComunicacaoProcessamentoService(
            _entregaServiceMock.Object,
            _entregaRepositoryMock.Object,
            [_whatsAppProviderMock.Object, _emailProviderMock.Object, _pushProviderMock.Object, _notificacaoInternaProviderMock.Object],
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessarPendentesAsync_EnviaWhatsAppEMarcaComoEnviado()
    {
        var resumo = new ComunicacaoEntregaResumoDto
        {
            Id = 1,
            Canal = CanalComunicacao.WhatsApp,
            DestinoResolvido = "5511999999999",
            Status = StatusComunicacaoEntrega.Reservado
        };
        var entrega = new ComunicacaoEntrega
        {
            Id = 1,
            Canal = CanalComunicacao.WhatsApp,
            DestinoResolvido = "5511999999999",
            ConteudoFinal = "Olá pelo WhatsApp",
            Status = StatusComunicacaoEntrega.Reservado
        };

        _entregaServiceMock.Setup(x => x.ReservarPendentesAsync(10)).ReturnsAsync([resumo]);
        _entregaRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(entrega);
        _whatsAppProviderMock
            .Setup(x => x.EnviarAsync(entrega, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalEnvioResultado { Sucesso = true });
        _entregaServiceMock.Setup(x => x.MarcarComoEnviadaAsync(1)).Returns(Task.CompletedTask);

        var processadas = await _service.ProcessarPendentesAsync(10);

        processadas.Should().Be(1);
        _entregaServiceMock.Verify(x => x.MarcarComoEnviadaAsync(1), Times.Once);
        _entregaServiceMock.Verify(x => x.MarcarComoFalhaAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarPendentesAsync_EnviaEmailEMarcaComoEnviado()
    {
        var resumo = new ComunicacaoEntregaResumoDto
        {
            Id = 2,
            Canal = CanalComunicacao.Email,
            DestinoResolvido = "destino@email.com",
            Status = StatusComunicacaoEntrega.Reservado
        };
        var entrega = new ComunicacaoEntrega
        {
            Id = 2,
            Canal = CanalComunicacao.Email,
            DestinoResolvido = "destino@email.com",
            RemetenteResolvido = "Assunto especial",
            ConteudoFinal = "Texto puro",
            ConteudoHtmlFinal = "<p>Texto puro</p>",
            Status = StatusComunicacaoEntrega.Reservado
        };

        _entregaServiceMock.Setup(x => x.ReservarPendentesAsync(10)).ReturnsAsync([resumo]);
        _entregaRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(entrega);
        _emailProviderMock
            .Setup(x => x.EnviarAsync(entrega, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalEnvioResultado { Sucesso = true });
        _entregaServiceMock.Setup(x => x.MarcarComoEnviadaAsync(2)).Returns(Task.CompletedTask);

        var processadas = await _service.ProcessarPendentesAsync(10);

        processadas.Should().Be(1);
        _emailProviderMock.Verify(x => x.EnviarAsync(entrega, It.IsAny<CancellationToken>()), Times.Once);
        _entregaServiceMock.Verify(x => x.MarcarComoEnviadaAsync(2), Times.Once);
    }

    [Fact]
    public async Task ProcessarPendentesAsync_MarcaFalhaQuandoCanalNaoEstaHabilitado()
    {
        var resumo = new ComunicacaoEntregaResumoDto
        {
            Id = 3,
            Canal = (CanalComunicacao)99,
            DestinoResolvido = "token-123",
            Status = StatusComunicacaoEntrega.Reservado
        };
        var entrega = new ComunicacaoEntrega
        {
            Id = 3,
            Canal = (CanalComunicacao)99,
            DestinoResolvido = "token-123",
            ConteudoFinal = "Push pendente",
            Status = StatusComunicacaoEntrega.Reservado
        };

        _entregaServiceMock.Setup(x => x.ReservarPendentesAsync(10)).ReturnsAsync([resumo]);
        _entregaRepositoryMock.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(entrega);
        _entregaServiceMock.Setup(x => x.MarcarComoFalhaAsync(3, It.IsAny<string>())).Returns(Task.CompletedTask);

        var processadas = await _service.ProcessarPendentesAsync(10);

        processadas.Should().Be(0);
        _entregaServiceMock.Verify(
            x => x.MarcarComoFalhaAsync(3, It.Is<string>(erro => erro.Contains("ainda não está habilitado"))),
            Times.Once);
        _entregaServiceMock.Verify(x => x.MarcarComoEnviadaAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarPendentesAsync_MarcaFalhaQuandoConfiguracaoDoCanalEstaIncompleta()
    {
        var resumo = new ComunicacaoEntregaResumoDto
        {
            Id = 4,
            Canal = CanalComunicacao.Email,
            DestinoResolvido = "destino@email.com",
            Status = StatusComunicacaoEntrega.Reservado
        };
        var entrega = new ComunicacaoEntrega
        {
            Id = 4,
            Canal = CanalComunicacao.Email,
            DestinoResolvido = "destino@email.com",
            ConteudoFinal = "Texto puro",
            Status = StatusComunicacaoEntrega.Reservado
        };

        _entregaServiceMock.Setup(x => x.ReservarPendentesAsync(10)).ReturnsAsync([resumo]);
        _entregaRepositoryMock.Setup(x => x.GetByIdAsync(4)).ReturnsAsync(entrega);
        _emailProviderMock
            .Setup(x => x.ValidarConfiguracaoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalDiagnostico
            {
                Configurado = false,
                Mensagem = "Configuração do canal E-mail incompleta. Campos obrigatórios: Email:FromAddress."
            });
        _entregaServiceMock.Setup(x => x.MarcarComoFalhaAsync(4, It.IsAny<string>())).Returns(Task.CompletedTask);

        var processadas = await _service.ProcessarPendentesAsync(10);

        processadas.Should().Be(0);
        _entregaServiceMock.Verify(
            x => x.MarcarComoFalhaAsync(4, It.Is<string>(erro => erro.Contains("Configuração do canal E-mail incompleta"))),
            Times.Once);
        _emailProviderMock.Verify(x => x.EnviarAsync(It.IsAny<ComunicacaoEntrega>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarEntregaAsync_ProcessaEntregaEspecifica()
    {
        var entrega = new ComunicacaoEntrega
        {
            Id = 9,
            Canal = CanalComunicacao.WhatsApp,
            DestinoResolvido = "5511999999999",
            ConteudoFinal = "Olá individual",
            MidiaUrl = "/uploads/card.png",
            Status = StatusComunicacaoEntrega.Pendente
        };

        _entregaRepositoryMock.Setup(x => x.GetByIdAsync(9)).ReturnsAsync(entrega);
        _whatsAppProviderMock
            .Setup(x => x.EnviarAsync(entrega, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComunicacaoCanalEnvioResultado { Sucesso = true });
        _entregaServiceMock.Setup(x => x.MarcarComoEnviadaAsync(9)).Returns(Task.CompletedTask);

        var processada = await _service.ProcessarEntregaAsync(9);

        processada.Should().BeTrue();
        _entregaServiceMock.Verify(x => x.MarcarComoEnviadaAsync(9), Times.Once);
        _whatsAppProviderMock.Verify(x => x.EnviarAsync(entrega, It.IsAny<CancellationToken>()), Times.Once);
    }
}
