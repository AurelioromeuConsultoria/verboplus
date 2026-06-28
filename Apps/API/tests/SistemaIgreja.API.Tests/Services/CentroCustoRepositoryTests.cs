using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class CentroCustoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<CentroCusto>().AddRange(
            new CentroCusto { Nome = "Obras", Ativo = true },
            new CentroCusto { Nome = "Administrativo", Ativo = true });
        await context.SaveChangesAsync();

        var repository = new CentroCustoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Administrativo", "Obras");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistCentroCusto()
    {
        await using var context = await CreateContextAsync();
        var repository = new CentroCustoRepository(context);

        var created = await repository.CreateAsync(new CentroCusto { Nome = "Missões", Descricao = "Ações externas", Ativo = true });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Missões Atualizado";
        created.Ativo = false;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Missões Atualizado");
        loaded.Ativo.Should().BeFalse();

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
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
}
