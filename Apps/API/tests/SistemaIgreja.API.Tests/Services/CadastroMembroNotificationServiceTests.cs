using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class CadastroMembroNotificationServiceTests
{
    private readonly Mock<IEvolutionApiService> _evolutionApiServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<CadastroMembroNotificationService>> _loggerMock = new();

    [Fact]
    public async Task NotifySuccessAsync_SkipsChannels_WhenContatoIsMissing()
    {
        var service = CreateService(emailEnabled: true);

        var result = await service.NotifySuccessAsync(new CadastroMembroNotification
        {
            Nome = "Marco"
        });

        result.WhatsApp.Status.Should().Be("skipped");
        result.Email.Status.Should().Be("skipped");
    }

    [Fact]
    public async Task NotifySuccessAsync_MarksWhatsappAsFailed_WhenProviderReturnsFailure()
    {
        _evolutionApiServiceMock.Setup(s => s.EnviarMensagemTextoAsync("11999999999", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse
            {
                Sucesso = false,
                MensagemErro = "Instância offline"
            });

        var service = CreateService(emailEnabled: false);

        var result = await service.NotifySuccessAsync(new CadastroMembroNotification
        {
            Nome = "Marco",
            WhatsApp = "11999999999",
            Email = "marco@example.com"
        });

        result.WhatsApp.Status.Should().Be("failed");
        result.Email.Status.Should().Be("skipped");
    }

    [Fact]
    public async Task NotifySuccessAsync_SendsEmail_WhenEnabled()
    {
        _evolutionApiServiceMock.Setup(s => s.EnviarMensagemTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse { Sucesso = true });

        var service = CreateService(emailEnabled: true);

        var result = await service.NotifySuccessAsync(new CadastroMembroNotification
        {
            Nome = "Marco",
            Email = "marco@example.com",
            WhatsApp = "11999999999"
        });

        result.WhatsApp.Status.Should().Be("sent");
        result.Email.Status.Should().Be("sent");
        _emailServiceMock.Verify(
            s => s.SendAsync(It.Is<EmailMessage>(m =>
                m.To == "marco@example.com" &&
                m.Subject == "Cadastro recebido com sucesso" &&
                m.HtmlBody != null &&
                m.TextBody != null), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifySuccessAsync_MarksEmailAsFailed_WhenEmailThrows()
    {
        _emailServiceMock.Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("smtp error"));

        var service = CreateService(emailEnabled: true);

        var result = await service.NotifySuccessAsync(new CadastroMembroNotification
        {
            Nome = "Marco",
            Email = "marco@example.com"
        });

        result.Email.Status.Should().Be("failed");
    }

    private CadastroMembroNotificationService CreateService(bool emailEnabled)
    {
        return new CadastroMembroNotificationService(
            _evolutionApiServiceMock.Object,
            _emailServiceMock.Object,
            Options.Create(new EmailSettings
            {
                Enabled = emailEnabled
            }),
            _loggerMock.Object);
    }
}
