using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class FornecedorRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<Fornecedor>().AddRange(
            new Fornecedor { Nome = "Zeta Serviços" },
            new Fornecedor { Nome = "Alpha Gráfica" });
        await context.SaveChangesAsync();

        var repository = new FornecedorRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Alpha Gráfica", "Zeta Serviços");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistFornecedor()
    {
        await using var context = await CreateContextAsync();
        var repository = new FornecedorRepository(context);

        var created = await repository.CreateAsync(new Fornecedor
        {
            Nome = "Fornecedor Teste",
            RazaoSocial = "Fornecedor Teste LTDA",
            ContatoEmail = "teste@fornecedor.com"
        });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Fornecedor Atualizado";
        created.ContatoNome = "Marco";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Fornecedor Atualizado");
        loaded.ContatoNome.Should().Be("Marco");

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
