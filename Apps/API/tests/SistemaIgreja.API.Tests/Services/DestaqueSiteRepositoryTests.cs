using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class DestaqueSiteRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByDataCriacaoThenId()
    {
        await using var context = await CreateContextAsync();
        context.Set<DestaqueSite>().AddRange(
            new DestaqueSite { Texto = "Mais recente", DataCriacao = new DateTime(2026, 4, 5) },
            new DestaqueSite { Texto = "Mais antigo", DataCriacao = new DateTime(2026, 4, 1) });
        await context.SaveChangesAsync();

        var repository = new DestaqueSiteRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result[0].Texto.Should().Be("Mais antigo");
        result[1].Texto.Should().Be("Mais recente");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistDestaque()
    {
        await using var context = await CreateContextAsync();
        var repository = new DestaqueSiteRepository(context);

        var created = await repository.CreateAsync(new DestaqueSite
        {
            Texto = "Culto domingo",
            Descricao = "Participe",
            Url = "https://exemplo.com"
        });
        created.Id.Should().BeGreaterThan(0);

        created.Texto = "Culto domingo atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Texto.Should().Be("Culto domingo atualizado");

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
