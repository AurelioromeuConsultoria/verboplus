using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class PerfilAcessoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome_WithPermissoesIncluded()
    {
        await using var context = await CreateContextAsync();
        context.Set<PerfilAcesso>().AddRange(
            new PerfilAcesso
            {
                Nome = "Lider",
                Permissoes = [new PerfilAcessoPermissao { Recurso = "VOLUNTARIADO", PodeVer = true }]
            },
            new PerfilAcesso
            {
                Nome = "Admin",
                Permissoes = [new PerfilAcessoPermissao { Recurso = "USUARIOS", PodeVer = true, PodeEditar = true }]
            });
        await context.SaveChangesAsync();

        var repository = new PerfilAcessoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Admin", "Lider");
        result[0].Permissoes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistPerfil()
    {
        await using var context = await CreateContextAsync();
        var repository = new PerfilAcessoRepository(context);

        var created = await repository.CreateAsync(new PerfilAcesso { Nome = "Operador" });
        created.Id.Should().BeGreaterThan(0);

        created.Descricao = "Perfil operacional";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Descricao.Should().Be("Perfil operacional");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SetsTenantIdOnPerfilAndPermissoes_FromCurrentTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new SistemaIgrejaDbContext(options, new FixedTenantContext(2));
        await context.Database.EnsureCreatedAsync();
        context.IgnoreTenantFilters = true;
        context.Tenants.Add(new Tenant
        {
            Id = 2,
            Nome = "Outra Igreja",
            Slug = "outra-igreja",
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        context.IgnoreTenantFilters = false;

        var repository = new PerfilAcessoRepository(context, new FixedTenantContext(2));
        var created = await repository.CreateAsync(new PerfilAcesso
        {
            Nome = "Operador",
            Permissoes = [new PerfilAcessoPermissao { Recurso = "dashboard", PodeVer = true }]
        });

        created.TenantId.Should().Be(2);
        created.Permissoes.Should().OnlyContain(p => p.TenantId == 2);
    }

    private static async Task<SistemaIgrejaDbContext> CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new SistemaIgrejaDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = null;
        public bool IsResolved => TenantId.HasValue;
    }
}
