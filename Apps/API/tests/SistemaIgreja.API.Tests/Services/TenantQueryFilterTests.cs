using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.API.Tests.Services;

public class TenantQueryFilterTests
{
    [Fact]
    public async Task TenantizedEntities_AreAutomaticallyFilteredByCurrentTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var seedContext = CreateContext(connection, new FixedTenantContext(Tenant.InitialTenantId)))
        {
            await seedContext.Database.EnsureCreatedAsync();
            seedContext.IgnoreTenantFilters = true;

            seedContext.Tenants.Add(new Tenant
            {
                Id = 2,
                Nome = "Outra Igreja",
                Slug = "outra-igreja",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });

            var pessoaTenant1 = new Pessoa
            {
                TenantId = Tenant.InitialTenantId,
                Nome = "Pessoa Tenant 1",
                Email = "tenant1@app.com",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            var pessoaTenant2 = new Pessoa
            {
                TenantId = 2,
                Nome = "Pessoa Tenant 2",
                Email = "tenant2@app.com",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            seedContext.Pessoas.AddRange(pessoaTenant1, pessoaTenant2);
            await seedContext.SaveChangesAsync();

            seedContext.Usuarios.AddRange(
                new Usuario
                {
                    TenantId = Tenant.InitialTenantId,
                    PessoaId = pessoaTenant1.Id,
                    EmailLogin = "user1@app.com",
                    SenhaHash = "hash",
                    TipoUsuario = TipoUsuario.Admin,
                    Ativo = true
                },
                new Usuario
                {
                    TenantId = 2,
                    PessoaId = pessoaTenant2.Id,
                    EmailLogin = "user2@app.com",
                    SenhaHash = "hash",
                    TipoUsuario = TipoUsuario.Admin,
                    Ativo = true
                });

            seedContext.Visitantes.AddRange(
                new Visitante
                {
                    TenantId = Tenant.InitialTenantId,
                    PessoaId = pessoaTenant1.Id,
                    DataVisita = DateTime.UtcNow,
                    DataCadastro = DateTime.UtcNow
                },
                new Visitante
                {
                    TenantId = 2,
                    PessoaId = pessoaTenant2.Id,
                    DataVisita = DateTime.UtcNow,
                    DataCadastro = DateTime.UtcNow
                });

            await seedContext.SaveChangesAsync();
        }

        await using var tenant2Context = CreateContext(connection, new FixedTenantContext(2));

        var pessoas = await tenant2Context.Pessoas.ToListAsync();
        var usuarios = await tenant2Context.Usuarios.ToListAsync();
        var visitantes = await tenant2Context.Visitantes.ToListAsync();

        pessoas.Should().ContainSingle();
        pessoas[0].TenantId.Should().Be(2);
        usuarios.Should().ContainSingle();
        usuarios[0].TenantId.Should().Be(2);
        visitantes.Should().ContainSingle();
        visitantes[0].TenantId.Should().Be(2);
    }

    private static SistemaIgrejaDbContext CreateContext(SqliteConnection connection, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        return new SistemaIgrejaDbContext(options, tenantContext);
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = null;
        public bool IsResolved => TenantId.HasValue;
    }
}
