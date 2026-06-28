using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoEmailCanalProviderTests
{
    [Fact]
    public async Task ValidarConfiguracaoAsync_ReturnsFailureWhenSettingsAreIncomplete()
    {
        var sut = new ComunicacaoEmailCanalProvider(
            Mock.Of<IEmailService>(),
            Options.Create(new EmailSettings { Enabled = false }),
            Mock.Of<ILogger<ComunicacaoEmailCanalProvider>>());

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeFalse();
        result.Mensagem.Should().Contain("Email:Enabled");
        result.Mensagem.Should().Contain("Email:Host");
        result.Mensagem.Should().Contain("Email:FromAddress");
    }

    [Fact]
    public async Task ValidarConfiguracaoAsync_ReturnsFailureWhenPasswordIsMissingForUsername()
    {
        var sut = new ComunicacaoEmailCanalProvider(
            Mock.Of<IEmailService>(),
            Options.Create(new EmailSettings
            {
                Enabled = true,
                Host = "smtp.exemplo.com",
                FromAddress = "noreply@igreja.com",
                Username = "usuario"
            }),
            Mock.Of<ILogger<ComunicacaoEmailCanalProvider>>());

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeFalse();
        result.Mensagem.Should().Contain("Email:Password");
    }

    [Fact]
    public async Task EnviarAsync_ReturnsFailureWhenDestinationIsMissing()
    {
        var sut = new ComunicacaoEmailCanalProvider(
            Mock.Of<IEmailService>(),
            Options.Create(new EmailSettings()),
            Mock.Of<ILogger<ComunicacaoEmailCanalProvider>>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Contain("Destino de e-mail");
    }

    [Fact]
    public async Task EnviarAsync_SendsEmailUsingResolvedSubjectAndHtml()
    {
        var emailService = new Mock<IEmailService>();
        emailService.Setup(s => s.SendAsync(
                It.Is<EmailMessage>(m =>
                    m.To == "usuario@igreja.com" &&
                    m.Subject == "Assunto" &&
                    m.TextBody == "Mensagem" &&
                    m.HtmlBody == "<b>Mensagem</b>"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new ComunicacaoEmailCanalProvider(
            emailService.Object,
            Options.Create(new EmailSettings
            {
                Enabled = true,
                Host = "smtp.exemplo.com",
                FromAddress = "noreply@igreja.com"
            }),
            Mock.Of<ILogger<ComunicacaoEmailCanalProvider>>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            DestinoResolvido = "usuario@igreja.com",
            RemetenteResolvido = "Assunto",
            ConteudoFinal = "Mensagem",
            ConteudoHtmlFinal = "<b>Mensagem</b>"
        });

        result.Sucesso.Should().BeTrue();
        emailService.VerifyAll();
    }
}
