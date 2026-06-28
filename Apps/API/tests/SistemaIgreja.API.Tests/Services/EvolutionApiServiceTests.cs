using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class EvolutionApiServiceTests
{
    private readonly Mock<ILogger<EvolutionApiService>> _loggerMock = new();

    [Fact]
    public async Task EnviarMensagemTextoAsync_ReturnsBadRequest_WhenNumeroIsEmpty()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await service.EnviarMensagemTextoAsync("", "teste");

        result.Sucesso.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task EnviarMensagemTextoAsync_ReturnsBadRequest_WhenMensagemIsEmpty()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var result = await service.EnviarMensagemTextoAsync("11999999999", "");

        result.Sucesso.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task EnviarMensagemTextoAsync_ReturnsSuccessAndMessageId_WhenApiReturnsOk()
    {
        HttpRequestMessage? capturedRequest = null;
        var service = CreateService(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"key":{"id":"msg-123"}}""", Encoding.UTF8, "application/json")
            };
        });

        var result = await service.EnviarMensagemTextoAsync("11999999999", "Olá mundo");

        result.Sucesso.Should().BeTrue();
        result.MessageId.Should().Be("msg-123");
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("message/sendText/instancia-teste");
    }

    [Fact]
    public async Task ValidarInstanciaAsync_ReturnsTrue_WhenInstanceExistsInResponse()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"instance":{"instanceName":"instancia-teste"}}]""", Encoding.UTF8, "application/json")
        });

        var result = await service.ValidarInstanciaAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarInstanciaAsync_ReturnsFalse_WhenApiFails()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("erro")
        });

        var result = await service.ValidarInstanciaAsync();

        result.Should().BeFalse();
    }

    private EvolutionApiService CreateService(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHttpMessageHandler(responder);
        var client = new HttpClient(handler);
        var settings = Options.Create(new EvolutionApiSettings
        {
            BaseUrl = "https://evolution.example.com",
            ApiKey = "api-key",
            InstanceName = "instancia-teste",
            TimeoutSeconds = 5,
            MaxRetries = 1,
            RetryDelaySeconds = 1,
            CodigoPaisPadrao = "55"
        });

        return new EvolutionApiService(client, settings, _loggerMock.Object);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}
