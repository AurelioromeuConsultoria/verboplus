using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoPushCanalProviderTests
{
    [Fact]
    public async Task ValidarConfiguracaoAsync_ReturnsConfigured_WhenCredentialsPathExists()
    {
        var sut = CreateSut("/tmp/firebase.json");

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeTrue();
        result.Mensagem.Should().BeNull();
    }

    [Fact]
    public async Task ValidarConfiguracaoAsync_ReturnsWarning_WhenCredentialsPathIsMissing()
    {
        var sut = CreateSut(null);

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeFalse();
        result.Mensagem.Should().Contain("Firebase:CredentialsPath");
    }

    [Fact]
    public async Task EnviarAsync_ReturnsFailure_WhenPessoaIsNotResolved()
    {
        var kidsPushNotificationService = new Mock<IKidsPushNotificationService>(MockBehavior.Strict);
        var sut = CreateSut("/tmp/firebase.json", kidsPushNotificationService.Object);

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            Id = 10,
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Contain("Destinatário pessoa");
    }

    [Fact]
    public async Task EnviarAsync_SendsNotificationToResolvedPessoa()
    {
        var kidsPushNotificationService = new Mock<IKidsPushNotificationService>();
        kidsPushNotificationService.Setup(s => s.SendToPessoasAsync(
                It.Is<IEnumerable<int>>(ids => ids.Single() == 25),
                "Avisos",
                "Mensagem",
                It.Is<IReadOnlyDictionary<string, string>>(data =>
                    data["origem"] == "COMUNICACAO_CENTRAL" &&
                    data["entregaId"] == "11")))
            .Returns(Task.CompletedTask);

        var sut = CreateSut("/tmp/firebase.json", kidsPushNotificationService.Object);

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            Id = 11,
            DestinatarioPessoaId = 25,
            RemetenteResolvido = "Avisos",
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeTrue();
        kidsPushNotificationService.VerifyAll();
    }

    [Fact]
    public async Task EnviarAsync_UsesDefaultSender_WhenRemetenteIsBlank()
    {
        var kidsPushNotificationService = new Mock<IKidsPushNotificationService>();
        kidsPushNotificationService.Setup(s => s.SendToPessoasAsync(
                It.Is<IEnumerable<int>>(ids => ids.Single() == 30),
                "Comunicacao AppIgreja",
                "Mensagem padrao",
                It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut("/tmp/firebase.json", kidsPushNotificationService.Object);

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            Id = 12,
            DestinatarioPessoaId = 30,
            RemetenteResolvido = " ",
            ConteudoFinal = "Mensagem padrao"
        });

        result.Sucesso.Should().BeTrue();
        kidsPushNotificationService.VerifyAll();
    }

    private static ComunicacaoPushCanalProvider CreateSut(
        string? credentialsPath,
        IKidsPushNotificationService? kidsPushNotificationService = null)
    {
        return new ComunicacaoPushCanalProvider(
            kidsPushNotificationService ?? Mock.Of<IKidsPushNotificationService>(),
            Options.Create(new FirebaseKidsPushOptions { CredentialsPath = credentialsPath }),
            Mock.Of<ILogger<ComunicacaoPushCanalProvider>>());
    }
}
