using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.API.Tests.Services;

public class BillingModelTests
{
    [Fact]
    public async Task Planos_SaoSeedadosComOsTresTiers()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var planos = await context.Planos.OrderBy(p => p.Ordem).ToListAsync();

        planos.Should().HaveCount(3);
        planos.Select(p => p.Slug).Should().ContainInOrder("essencial", "organizacao", "crescimento");
        planos.Should().OnlyContain(p => p.Ativo && p.PrecoMensal > 0);
    }

    [Fact]
    public async Task Assinatura_EhIsoladaPorTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var seed = CreateContext(connection))
        {
            await seed.Database.EnsureCreatedAsync();
            seed.IgnoreTenantFilters = true;

            seed.Tenants.Add(new Tenant { Id = 2, Nome = "Outra Igreja", Slug = "outra-igreja", Ativo = true, DataCriacao = DateTime.UtcNow });

            seed.Assinaturas.AddRange(
                new Assinatura { TenantId = Tenant.InitialTenantId, PlanoId = 1, Status = StatusAssinatura.Trial, Ciclo = CicloCobranca.Mensal, Valor = 49.90m },
                new Assinatura { TenantId = 2, PlanoId = 2, Status = StatusAssinatura.Ativa, Ciclo = CicloCobranca.Mensal, Valor = 99.90m });
            await seed.SaveChangesAsync();
        }

        await using var tenant2 = CreateContext(connection, tenantId: 2);

        var assinaturas = await tenant2.Assinaturas.ToListAsync();

        assinaturas.Should().ContainSingle();
        assinaturas[0].TenantId.Should().Be(2);
        assinaturas[0].Status.Should().Be(StatusAssinatura.Ativa);
    }

    private static SistemaIgrejaDbContext CreateContext(SqliteConnection connection, int tenantId = Tenant.InitialTenantId)
    {
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        return new SistemaIgrejaDbContext(options, new FixedTenantContext(tenantId));
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = null;
        public bool IsResolved => TenantId.HasValue;
    }
}
