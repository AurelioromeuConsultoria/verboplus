using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoWhatsAppCanalProviderTests
{
    [Fact]
    public async Task ValidarConfiguracaoAsync_ReturnsFailureWhenSettingsAreIncomplete()
    {
        var sut = new ComunicacaoWhatsAppCanalProvider(
            Mock.Of<IEvolutionApiService>(),
            Options.Create(new EvolutionApiSettings()),
            Mock.Of<ILogger<ComunicacaoWhatsAppCanalProvider>>());

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeFalse();
        result.Mensagem.Should().Contain("EvolutionApi:BaseUrl");
        result.Mensagem.Should().Contain("EvolutionApi:ApiKey");
        result.Mensagem.Should().Contain("EvolutionApi:InstanceName");
    }

    [Fact]
    public async Task ValidarConfiguracaoAsync_ReturnsConfiguredWhenSettingsAreComplete()
    {
        var sut = new ComunicacaoWhatsAppCanalProvider(
            Mock.Of<IEvolutionApiService>(),
            Options.Create(new EvolutionApiSettings
            {
                BaseUrl = "https://api.exemplo.com",
                ApiKey = "token",
                InstanceName = "igreja"
            }),
            Mock.Of<ILogger<ComunicacaoWhatsAppCanalProvider>>());

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeTrue();
    }

    [Fact]
    public async Task EnviarAsync_ReturnsFailureWhenDestinationIsMissing()
    {
        var sut = new ComunicacaoWhatsAppCanalProvider(
            Mock.Of<IEvolutionApiService>(),
            Options.Create(new EvolutionApiSettings()),
            Mock.Of<ILogger<ComunicacaoWhatsAppCanalProvider>>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Contain("Destino de WhatsApp");
    }

    [Fact]
    public async Task EnviarAsync_UsesTextEndpointWhenThereIsNoMedia()
    {
        var evolutionService = new Mock<IEvolutionApiService>();
        evolutionService.Setup(s => s.EnviarMensagemTextoAsync("5511999999999", "Mensagem", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse { Sucesso = true, MessageId = "1" });

        var sut = new ComunicacaoWhatsAppCanalProvider(
            evolutionService.Object,
            Options.Create(new EvolutionApiSettings()),
            Mock.Of<ILogger<ComunicacaoWhatsAppCanalProvider>>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            DestinoResolvido = "5511999999999",
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeTrue();
        evolutionService.VerifyAll();
    }

    [Fact]
    public async Task EnviarAsync_UsesImageEndpointAndReturnsErrorMessage()
    {
        var evolutionService = new Mock<IEvolutionApiService>();
        evolutionService.Setup(s => s.EnviarMensagemImagemAsync("5511999999999", "https://img", "Legenda", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse { Sucesso = false, MensagemErro = "falhou", StatusCode = 500 });

        var sut = new ComunicacaoWhatsAppCanalProvider(
            evolutionService.Object,
            Options.Create(new EvolutionApiSettings()),
            Mock.Of<ILogger<ComunicacaoWhatsAppCanalProvider>>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            DestinoResolvido = "5511999999999",
            ConteudoFinal = "Legenda",
            MidiaUrl = "https://img"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Contain("Evolution API");
        result.Mensagem.Should().Contain("500");
    }
}
