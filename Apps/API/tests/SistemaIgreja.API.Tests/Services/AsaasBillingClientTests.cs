using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class AsaasBillingClientTests
{
    [Fact]
    public void Configurado_EhFalse_QuandoSemApiKey()
    {
        var client = Build(new StubHandler((_) => (HttpStatusCode.OK, "{}")), apiKey: "");
        client.Configurado.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCustomerAsync_SemApiKey_RetornaErro_SemChamarHttp()
    {
        var handler = new StubHandler((_) => (HttpStatusCode.OK, "{\"id\":\"cus_x\"}"));
        var client = Build(handler, apiKey: "");

        var result = await client.CreateCustomerAsync(new AsaasCustomerRequest { Nome = "Igreja X" });

        result.Success.Should().BeFalse();
        handler.LastRequest.Should().BeNull(); // nem tentou chamar
    }

    [Fact]
    public async Task CreateCustomerAsync_RetornaCustomerId_EEnviaAccessToken()
    {
        var handler = new StubHandler((_) => (HttpStatusCode.OK, "{\"id\":\"cus_123\"}"));
        var client = Build(handler, apiKey: "key_test");

        var result = await client.CreateCustomerAsync(new AsaasCustomerRequest { Nome = "Igreja X", Email = "x@y.com" });

        result.Success.Should().BeTrue();
        result.CustomerId.Should().Be("cus_123");
        handler.LastRequest!.Headers.Contains("access_token").Should().BeTrue();
        handler.LastRequest.RequestUri!.ToString().Should().EndWith("customers");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_EnviaCicloEVencimento_ERetornaId()
    {
        var handler = new StubHandler((_) => (HttpStatusCode.OK, "{\"id\":\"sub_1\",\"status\":\"ACTIVE\"}"));
        var client = Build(handler, apiKey: "key_test");

        var result = await client.CreateSubscriptionAsync(new AsaasSubscriptionRequest
        {
            CustomerId = "cus_123",
            Valor = 99.90m,
            Ciclo = CicloCobranca.Mensal,
            PrimeiroVencimento = new DateTime(2026, 7, 1),
            Descricao = "Plano Organização"
        });

        result.Success.Should().BeTrue();
        result.SubscriptionId.Should().Be("sub_1");
        result.Status.Should().Be("ACTIVE");

        handler.LastRequest!.RequestUri!.ToString().Should().EndWith("subscriptions");
        handler.LastBody.Should().Contain("\"cycle\":\"MONTHLY\"");
        handler.LastBody.Should().Contain("\"nextDueDate\":\"2026-07-01\"");
        handler.LastBody.Should().Contain("\"customer\":\"cus_123\"");
    }

    [Fact]
    public async Task CancelSubscriptionAsync_RetornaDeleted()
    {
        var handler = new StubHandler((_) => (HttpStatusCode.OK, "{\"deleted\":true,\"id\":\"sub_1\"}"));
        var client = Build(handler, apiKey: "key_test");

        var result = await client.CancelSubscriptionAsync("sub_1");

        result.Success.Should().BeTrue();
        result.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubscriptionAsync_ErroHttp_RetornaFalha()
    {
        var handler = new StubHandler((_) => (HttpStatusCode.BadRequest, "{\"errors\":[{\"description\":\"inválido\"}]}"));
        var client = Build(handler, apiKey: "key_test");

        var result = await client.CreateSubscriptionAsync(new AsaasSubscriptionRequest { CustomerId = "cus_1", Valor = 10m, PrimeiroVencimento = new DateTime(2026, 7, 1) });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    private static AsaasBillingClient Build(StubHandler handler, string? apiKey)
    {
        var http = new HttpClient(handler);
        var settings = Options.Create(new AsaasBillingSettings { ApiKey = apiKey, Environment = "Sandbox" });
        return new AsaasBillingClient(http, settings, new Mock<ILogger<AsaasBillingClient>>().Object);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode, string)> _responder;

        public StubHandler(Func<HttpRequestMessage, (HttpStatusCode, string)> responder) => _responder = responder;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            var (code, json) = _responder(request);
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
