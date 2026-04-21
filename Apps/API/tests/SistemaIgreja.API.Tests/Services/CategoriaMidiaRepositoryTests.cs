using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaMidiaRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<CategoriaMidia>().AddRange(
            new CategoriaMidia { Nome = "Videos" },
            new CategoriaMidia { Nome = "Fotos" });
        await context.SaveChangesAsync();

        var repository = new CategoriaMidiaRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Fotos", "Videos");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_ReturnsExpectedDeleteFlag()
    {
        await using var context = await CreateContextAsync();
        var repository = new CategoriaMidiaRepository(context);

        var created = await repository.CreateAsync(new CategoriaMidia { Nome = "Podcast", Descricao = "Audio" });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Podcast Atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Podcast Atualizado");

        (await repository.DeleteAsync(created.Id)).Should().BeTrue();
        (await repository.DeleteAsync(9999)).Should().BeFalse();
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
