using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class BillingServiceTests
{
    [Fact]
    public async Task AssinarAsync_CriaTrial_ComIdsDoGateway()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var asaas = MockAsaas(configurado: true);
        var service = CreateService(context, asaas.Object);

        var dto = new AssinarTenantDto { TenantId = Tenant.InitialTenantId, PlanoId = 2, Ciclo = CicloCobranca.Mensal, NomeCliente = "Igreja Teste", Email = "t@x.com" };
        var result = await service.AssinarAsync(dto);

        result.Status.Should().Be(nameof(StatusAssinatura.Trial));
        result.EmTrial.Should().BeTrue();
        result.DiasTrialRestantes.Should().BeInRange(13, 14);
        result.TrialFim.Should().NotBeNull();
        result.GatewaySubscriptionId.Should().Be("sub_1");
        result.Valor.Should().Be(99.90m);
    }

    [Fact]
    public async Task AssinarAsync_SemGateway_CriaTrialLocalSemIds()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var asaas = MockAsaas(configurado: false);
        var service = CreateService(context, asaas.Object);

        var result = await service.AssinarAsync(new AssinarTenantDto { TenantId = Tenant.InitialTenantId, PlanoId = 1, NomeCliente = "Igreja" });

        result.Status.Should().Be(nameof(StatusAssinatura.Trial));
        result.GatewaySubscriptionId.Should().BeNull();
        asaas.Verify(a => a.CreateSubscriptionAsync(It.IsAny<AsaasSubscriptionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssinarAsync_TenantJaAssinado_LancaErro()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        context.Assinaturas.Add(new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 1, Status = StatusAssinatura.Ativa, Ciclo = CicloCobranca.Mensal, Valor = 49.90m });
        await context.SaveChangesAsync();

        var service = CreateService(context, MockAsaas(true).Object);

        await service.Invoking(s => s.AssinarAsync(new AssinarTenantDto { TenantId = Tenant.InitialTenantId, PlanoId = 2, NomeCliente = "X" }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProcessarWebhook_PagamentoConfirmado_AtivaEAcumulaFaturaPaga()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        SeedAssinatura(context, "sub_1", StatusAssinatura.Trial);

        var service = CreateService(context, MockAsaas(true).Object);

        const string json = "{\"event\":\"PAYMENT_CONFIRMED\",\"payment\":{\"id\":\"pay_1\",\"subscription\":\"sub_1\",\"value\":99.90,\"dueDate\":\"2026-07-01\"}}";
        using (var doc = JsonDocument.Parse(json))
        {
            var ok = await service.ProcessarWebhookAsync(doc.RootElement, accessToken: null);
            ok.Should().BeTrue();
        }

        await using var verify = CreateContext(connection);
        var assinatura = await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_1");
        assinatura.Status.Should().Be(StatusAssinatura.Ativa);
        assinatura.VigenciaInicio.Should().NotBeNull();

        var faturas = await verify.Faturas.Where(f => f.GatewayPaymentId == "pay_1").ToListAsync();
        faturas.Should().ContainSingle();
        faturas[0].Status.Should().Be(StatusFatura.Paga);
        faturas[0].Valor.Should().Be(99.90m);
    }

    [Fact]
    public async Task ProcessarWebhook_EhIdempotente()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        SeedAssinatura(context, "sub_1", StatusAssinatura.Trial);
        var service = CreateService(context, MockAsaas(true).Object);

        const string json = "{\"event\":\"PAYMENT_RECEIVED\",\"payment\":{\"id\":\"pay_9\",\"subscription\":\"sub_1\",\"value\":99.90}}";
        for (var i = 0; i < 2; i++)
        {
            using var doc = JsonDocument.Parse(json);
            await service.ProcessarWebhookAsync(doc.RootElement, null);
        }

        await using var verify = CreateContext(connection);
        (await verify.Faturas.CountAsync(f => f.GatewayPaymentId == "pay_9")).Should().Be(1);
    }

    [Fact]
    public async Task ProcessarWebhook_PagamentoVencido_MarcaInadimplente()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        SeedAssinatura(context, "sub_1", StatusAssinatura.Ativa);
        var service = CreateService(context, MockAsaas(true).Object);

        const string json = "{\"event\":\"PAYMENT_OVERDUE\",\"payment\":{\"id\":\"pay_2\",\"subscription\":\"sub_1\"}}";
        using (var doc = JsonDocument.Parse(json))
        {
            await service.ProcessarWebhookAsync(doc.RootElement, null);
        }

        await using var verify = CreateContext(connection);
        var assinatura = await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_1");
        assinatura.Status.Should().Be(StatusAssinatura.Inadimplente);
        assinatura.InadimplenteDesde.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessarWebhook_TokenInvalido_RetornaFalse()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var service = CreateService(context, MockAsaas(true).Object, webhookToken: "segredo-correto");

        const string json = "{\"event\":\"PAYMENT_RECEIVED\",\"payment\":{\"id\":\"p\",\"subscription\":\"sub_1\"}}";
        using var doc = JsonDocument.Parse(json);
        var ok = await service.ProcessarWebhookAsync(doc.RootElement, accessToken: "token-errado");

        ok.Should().BeFalse();
    }

    [Theory]
    [InlineData(StatusAssinatura.Ativa, false)]
    [InlineData(StatusAssinatura.Trial, false)]
    [InlineData(StatusAssinatura.Inadimplente, false)]
    [InlineData(StatusAssinatura.Suspensa, true)]
    public async Task TenantBloqueadoAsync_SegueOStatus(StatusAssinatura status, bool esperaBloqueio)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        context.Assinaturas.Add(new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 2, Status = status, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, ProximaCobranca = DateTime.UtcNow.AddDays(20) });
        await context.SaveChangesAsync();
        var service = CreateService(context, MockAsaas(true).Object);

        (await service.TenantBloqueadoAsync(Tenant.InitialTenantId)).Should().Be(esperaBloqueio);
    }

    [Fact]
    public async Task TenantBloqueadoAsync_SemAssinatura_NaoBloqueia()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var service = CreateService(context, MockAsaas(true).Object);

        (await service.TenantBloqueadoAsync(Tenant.InitialTenantId)).Should().BeFalse();
    }

    [Fact]
    public async Task TenantBloqueadoAsync_Cancelada_BloqueiaAposFimDoPeriodo()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        context.IgnoreTenantFilters = true;
        context.Tenants.Add(new Tenant { Id = 2, Nome = "T2", Slug = "t2", Ativo = true, DataCriacao = DateTime.UtcNow });
        await context.SaveChangesAsync();
        // cancelada com período já encerrado → bloqueia
        context.Assinaturas.Add(new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 2, Status = StatusAssinatura.Cancelada, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, ProximaCobranca = DateTime.UtcNow.AddDays(-1) });
        // outra cancelada com período futuro (tenant 2) → não bloqueia
        context.Assinaturas.Add(new Assinatura { TenantId = 2, PlanoId = 2, Status = StatusAssinatura.Cancelada, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, ProximaCobranca = DateTime.UtcNow.AddDays(15) });
        await context.SaveChangesAsync();
        var service = CreateService(context, MockAsaas(true).Object);

        (await service.TenantBloqueadoAsync(Tenant.InitialTenantId)).Should().BeTrue();
        (await service.TenantBloqueadoAsync(2)).Should().BeFalse();
    }

    // ---- helpers ----

    private static void SeedAssinatura(SistemaIgrejaDbContext context, string subId, StatusAssinatura status)
    {
        context.Assinaturas.Add(new Assinatura
        {
            TenantId = Tenant.InitialTenantId,
            PlanoId = 2,
            Status = status,
            Ciclo = CicloCobranca.Mensal,
            Valor = 99.90m,
            GatewaySubscriptionId = subId,
            TrialFim = DateTime.UtcNow.AddDays(10)
        });
        context.SaveChanges();
    }

    private static Mock<IAsaasBillingClient> MockAsaas(bool configurado)
    {
        var mock = new Mock<IAsaasBillingClient>();
        mock.SetupGet(a => a.Configurado).Returns(configurado);
        mock.Setup(a => a.CreateCustomerAsync(It.IsAny<AsaasCustomerRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AsaasCustomerResult { Success = true, CustomerId = "cus_1" });
        mock.Setup(a => a.CreateSubscriptionAsync(It.IsAny<AsaasSubscriptionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AsaasSubscriptionResult { Success = true, SubscriptionId = "sub_1", Status = "ACTIVE" });
        return mock;
    }

    private static BillingService CreateService(SistemaIgrejaDbContext context, IAsaasBillingClient asaas, string webhookToken = "")
    {
        return new BillingService(
            context,
            asaas,
            Options.Create(new BillingSettings { TrialDias = 14, CarenciaDias = 7 }),
            Options.Create(new AsaasBillingSettings { WebhookToken = webhookToken, Environment = "Sandbox" }),
            new FixedTenantContext(Tenant.InitialTenantId),
            new Mock<IEmailService>().Object,
            new Mock<ILogger<BillingService>>().Object);
    }

    private static SistemaIgrejaDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;
        return new SistemaIgrejaDbContext(options, new FixedTenantContext(Tenant.InitialTenantId));
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = null;
        public bool IsResolved => TenantId.HasValue;
    }
}
