using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class SolicitacaoTitularServiceTests
{
    [Fact]
    public async Task CriarAsync_DefineStatusAbertaEPrazoDe15Dias()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var service = new SolicitacaoTitularService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var criada = await service.CriarAsync(new CriarSolicitacaoTitularDto
        {
            NomeSolicitante = "João",
            ContatoSolicitante = "joao@x.com",
            Tipo = TipoSolicitacaoTitular.Exportacao,
            Canal = "email"
        });

        criada.Status.Should().Be(nameof(StatusSolicitacaoTitular.Aberta));
        criada.Tipo.Should().Be(nameof(TipoSolicitacaoTitular.Exportacao));
        (criada.PrazoLimite - criada.SolicitadoEm).TotalDays.Should().BeApproximately(15, 0.01);
        criada.PrazoVencido.Should().BeFalse();
        criada.DiasRestantes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConcluirAsync_MarcaConcluidaComAtendente()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var service = new SolicitacaoTitularService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var criada = await service.CriarAsync(new CriarSolicitacaoTitularDto { Tipo = TipoSolicitacaoTitular.Acesso });

        var concluida = await service.ConcluirAsync(criada.Id, "Dados enviados por e-mail.");

        concluida.Should().NotBeNull();
        concluida!.Status.Should().Be(nameof(StatusSolicitacaoTitular.Concluida));
        concluida.AtendidoEm.Should().NotBeNull();
        concluida.AtendidoPorUsuarioId.Should().Be(7);
        concluida.ResultadoObservacao.Should().Be("Dados enviados por e-mail.");
        concluida.PrazoVencido.Should().BeFalse(); // concluída não conta como vencida
    }

    [Fact]
    public async Task RecusarAsync_MarcaRecusadaComMotivo()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var service = new SolicitacaoTitularService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var criada = await service.CriarAsync(new CriarSolicitacaoTitularDto { Tipo = TipoSolicitacaoTitular.Eliminacao });

        var recusada = await service.RecusarAsync(criada.Id, "Retenção exigida por obrigação legal.");

        recusada.Should().NotBeNull();
        recusada!.Status.Should().Be(nameof(StatusSolicitacaoTitular.Recusada));
        recusada.ResultadoObservacao.Should().Be("Retenção exigida por obrigação legal.");
    }

    [Fact]
    public async Task ListarAsync_MarcaPrazoVencido_ParaSolicitacaoAbertaVencida()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var seed = CreateContext(connection))
        {
            await seed.Database.EnsureCreatedAsync();
            seed.Set<SolicitacaoTitular>().Add(new SolicitacaoTitular
            {
                TenantId = Tenant.InitialTenantId,
                Tipo = TipoSolicitacaoTitular.Acesso,
                Status = StatusSolicitacaoTitular.Aberta,
                SolicitadoEm = DateTime.UtcNow.AddDays(-20),
                PrazoLimite = DateTime.UtcNow.AddDays(-5)
            });
            await seed.SaveChangesAsync();
        }

        await using var context = CreateContext(connection);
        var service = new SolicitacaoTitularService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var itens = (await service.ListarAsync()).ToList();

        itens.Should().ContainSingle();
        itens[0].PrazoVencido.Should().BeTrue();
        itens[0].DiasRestantes.Should().BeLessThan(0);
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

    private sealed class StubCurrentUser : ICurrentUserContext
    {
        public int? UserId => 7;
        public int? TenantId => Tenant.InitialTenantId;
        public string? TenantSlug => null;
        public string? UserName => "Admin Teste";
        public string? UserEmail => "admin@teste.com";
        public string? IpAddress => "203.0.113.1";
    }
}
