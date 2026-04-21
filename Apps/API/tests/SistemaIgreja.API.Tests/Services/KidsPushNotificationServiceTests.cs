using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.Interfaces;

namespace SistemaIgreja.API.Tests.Services;

public class KidsPushNotificationServiceTests
{
    [Fact]
    public async Task SendToPessoasAsync_ReturnsWhenThereAreNoTokens()
    {
        var tokenRepository = new Mock<IKidsDeviceTokenRepository>();
        tokenRepository.Setup(r => r.GetTokensByPessoaIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(Array.Empty<string>());

        var sut = new KidsPushNotificationService(
            Options.Create(new FirebaseKidsPushOptions { CredentialsPath = null }),
            tokenRepository.Object,
            Mock.Of<ILogger<KidsPushNotificationService>>());

        var action = () => sut.SendToPessoasAsync([1, 2], "Titulo", "Mensagem");

        await action.Should().NotThrowAsync();
        tokenRepository.Verify(r => r.GetTokensByPessoaIdsAsync(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 1, 2 }))), Times.Once);
    }

    [Fact]
    public async Task SendToPessoasAsync_ReturnsWhenFirebaseIsNotConfigured()
    {
        var tokenRepository = new Mock<IKidsDeviceTokenRepository>();
        tokenRepository.Setup(r => r.GetTokensByPessoaIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(["token-1"]);

        var sut = new KidsPushNotificationService(
            Options.Create(new FirebaseKidsPushOptions { CredentialsPath = "/caminho/inexistente/firebase.json" }),
            tokenRepository.Object,
            Mock.Of<ILogger<KidsPushNotificationService>>());

        var action = () => sut.SendToPessoasAsync([3], "Titulo", "Mensagem", new Dictionary<string, string> { ["origem"] = "kids" });

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendToPessoasAsync_AllowsNullOptionsWrapperValue()
    {
        var tokenRepository = new Mock<IKidsDeviceTokenRepository>();
        tokenRepository.Setup(r => r.GetTokensByPessoaIdsAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(["token-1"]);

        var sut = new KidsPushNotificationService(
            Options.Create<FirebaseKidsPushOptions>(null!),
            tokenRepository.Object,
            Mock.Of<ILogger<KidsPushNotificationService>>());

        var action = () => sut.SendToPessoasAsync([7], "Titulo", "Mensagem");

        await action.Should().NotThrowAsync();
    }
}
