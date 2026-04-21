using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ProjetoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<Projeto>().AddRange(
            new Projeto { Nome = "Reforma", Ativo = true },
            new Projeto { Nome = "Ação Social", Ativo = true });
        await context.SaveChangesAsync();

        var repository = new ProjetoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Ação Social", "Reforma");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistProjeto()
    {
        await using var context = await CreateContextAsync();
        var repository = new ProjetoRepository(context);

        var created = await repository.CreateAsync(new Projeto { Nome = "Construção", Ativo = true });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Construção Atualizada";
        created.Ativo = false;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Construção Atualizada");
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
