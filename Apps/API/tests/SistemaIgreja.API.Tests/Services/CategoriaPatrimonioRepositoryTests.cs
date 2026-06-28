using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaPatrimonioRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<CategoriaPatrimonio>().AddRange(
            new CategoriaPatrimonio { Nome = "Veículos", Ativo = true },
            new CategoriaPatrimonio { Nome = "Equipamentos", Ativo = true });
        await context.SaveChangesAsync();

        var repository = new CategoriaPatrimonioRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Equipamentos", "Veículos");
    }

    [Fact]
    public async Task GetByNomeAsync_IsCaseInsensitive()
    {
        await using var context = await CreateContextAsync();
        context.Set<CategoriaPatrimonio>().Add(new CategoriaPatrimonio { Nome = "Audio", Ativo = true });
        await context.SaveChangesAsync();

        var repository = new CategoriaPatrimonioRepository(context);

        var result = await repository.GetByNomeAsync("audio");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Audio");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistCategoria()
    {
        await using var context = await CreateContextAsync();
        var repository = new CategoriaPatrimonioRepository(context);

        var created = await repository.CreateAsync(new CategoriaPatrimonio { Nome = "Mobiliário", Ativo = true });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Mobiliário Atualizado";
        created.Ativo = false;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Mobiliário Atualizado");
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
