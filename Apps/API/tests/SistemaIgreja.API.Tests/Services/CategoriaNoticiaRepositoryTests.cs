using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaNoticiaRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<CategoriaNoticia>().AddRange(
            new CategoriaNoticia { Nome = "Eventos" },
            new CategoriaNoticia { Nome = "Avisos" });
        await context.SaveChangesAsync();

        var repository = new CategoriaNoticiaRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Avisos", "Eventos");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistCategoria()
    {
        await using var context = await CreateContextAsync();
        var repository = new CategoriaNoticiaRepository(context);

        var created = await repository.CreateAsync(new CategoriaNoticia { Nome = "Comunicados" });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Comunicados Gerais";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Comunicados Gerais");

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
