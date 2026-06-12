using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class BillingCycleServiceTests
{
    [Fact]
    public async Task ExpiraTrialVencido_ParaInadimplente_EMantemTrialVigente()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        // trial vencido
        context.Assinaturas.Add(new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 2, Status = StatusAssinatura.Trial, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, TrialFim = DateTime.UtcNow.AddDays(-1), GatewaySubscriptionId = "sub_venc" });
        await context.SaveChangesAsync();
        // trial vigente (tenant 2)
        context.IgnoreTenantFilters = true;
        context.Tenants.Add(new Tenant { Id = 2, Nome = "T2", Slug = "t2", Ativo = true, DataCriacao = DateTime.UtcNow });
        context.Assinaturas.Add(new Assinatura { TenantId = 2, PlanoId = 2, Status = StatusAssinatura.Trial, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, TrialFim = DateTime.UtcNow.AddDays(5), GatewaySubscriptionId = "sub_vig" });
        await context.SaveChangesAsync();

        var service = CreateService(context, carenciaDias: 7);
        var resultado = await service.ExecutarTransicoesAutomaticasAsync();

        resultado.TrialsExpirados.Should().Be(1);

        await using var verify = CreateContext(connection);
        verify.IgnoreTenantFilters = true;
        var vencida = await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_venc");
        vencida.Status.Should().Be(StatusAssinatura.Inadimplente);
        vencida.InadimplenteDesde.Should().NotBeNull();
        var vigente = await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_vig");
        vigente.Status.Should().Be(StatusAssinatura.Trial);
    }

    [Fact]
    public async Task SuspendeInadimplente_SomenteAposCarencia()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        context.IgnoreTenantFilters = true;
        context.Tenants.Add(new Tenant { Id = 2, Nome = "T2", Slug = "t2", Ativo = true, DataCriacao = DateTime.UtcNow });
        // inadimplente além da carência (8 dias, carência 7) → suspende
        context.Assinaturas.Add(new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 2, Status = StatusAssinatura.Inadimplente, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, InadimplenteDesde = DateTime.UtcNow.AddDays(-8), GatewaySubscriptionId = "sub_fora" });
        // inadimplente dentro da carência (3 dias) → permanece
        context.Assinaturas.Add(new Assinatura { TenantId = 2, PlanoId = 2, Status = StatusAssinatura.Inadimplente, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, InadimplenteDesde = DateTime.UtcNow.AddDays(-3), GatewaySubscriptionId = "sub_dentro" });
        await context.SaveChangesAsync();

        var service = CreateService(context, carenciaDias: 7);
        var resultado = await service.ExecutarTransicoesAutomaticasAsync();

        resultado.Suspensos.Should().Be(1);

        await using var verify = CreateContext(connection);
        verify.IgnoreTenantFilters = true;
        (await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_fora")).Status.Should().Be(StatusAssinatura.Suspensa);
        (await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_dentro")).Status.Should().Be(StatusAssinatura.Inadimplente);
    }

    [Fact]
    public async Task EnviaAvisoTrialAcabando_UmaUnicaVez()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        // trial terminando em 2 dias (janela de aviso = 3), sem aviso ainda
        context.Assinaturas.Add(new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 2, Status = StatusAssinatura.Trial, Ciclo = CicloCobranca.Mensal, Valor = 99.90m, TrialFim = DateTime.UtcNow.AddDays(2), GatewaySubscriptionId = "sub_aviso" });
        await context.SaveChangesAsync();

        var service = CreateService(context, carenciaDias: 7);

        var primeira = await service.ExecutarTransicoesAutomaticasAsync();
        primeira.AvisosTrialEnviados.Should().Be(1);

        var segunda = await service.ExecutarTransicoesAutomaticasAsync();
        segunda.AvisosTrialEnviados.Should().Be(0); // não reenvia

        await using var verify = CreateContext(connection);
        (await verify.Assinaturas.FirstAsync(a => a.GatewaySubscriptionId == "sub_aviso")).TrialAvisoEnviadoEm.Should().NotBeNull();
    }

    private static BillingCycleService CreateService(SistemaIgrejaDbContext context, int carenciaDias)
    {
        return new BillingCycleService(
            context,
            Options.Create(new BillingSettings { TrialDias = 14, CarenciaDias = carenciaDias }),
            new Mock<IEmailService>().Object,
            new Mock<ILogger<BillingCycleService>>().Object);
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
