using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class CategoriaReceitaRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<CategoriaReceita>().AddRange(
            new CategoriaReceita { Nome = "Doações", Ativo = true },
            new CategoriaReceita { Nome = "Dízimo", Ativo = true });
        await context.SaveChangesAsync();

        var repository = new CategoriaReceitaRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Nome == "Dízimo");
        result.Should().Contain(x => x.Nome == "Doações");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistCategoria()
    {
        await using var context = await CreateContextAsync();
        var repository = new CategoriaReceitaRepository(context);

        var created = await repository.CreateAsync(new CategoriaReceita { Nome = "Oferta", Descricao = "Receita eventual", Ativo = true });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Oferta Atualizada";
        created.Ativo = false;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Oferta Atualizada");
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
