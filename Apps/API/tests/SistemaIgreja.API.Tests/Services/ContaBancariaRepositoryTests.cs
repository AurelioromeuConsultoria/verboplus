using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ContaBancariaRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.Set<ContaBancaria>().AddRange(
            new ContaBancaria { Nome = "Conta Operacional", SaldoInicial = 100, Ativo = true },
            new ContaBancaria { Nome = "Banco Caixa", SaldoInicial = 50, Ativo = true });
        await context.SaveChangesAsync();

        var repository = new ContaBancariaRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Banco Caixa", "Conta Operacional");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistConta()
    {
        await using var context = await CreateContextAsync();
        var repository = new ContaBancariaRepository(context);

        var created = await repository.CreateAsync(new ContaBancaria
        {
            Nome = "Bradesco",
            Banco = "Bradesco",
            Agencia = "1234",
            Conta = "0001-9",
            SaldoInicial = 250,
            Ativo = true
        });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Bradesco Principal";
        created.SaldoInicial = 500;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Bradesco Principal");
        loaded.SaldoInicial.Should().Be(500);

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
