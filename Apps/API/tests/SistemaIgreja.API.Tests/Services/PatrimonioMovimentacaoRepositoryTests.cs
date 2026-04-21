using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class PatrimonioMovimentacaoRepositoryTests
{
    [Fact]
    public async Task GetByPatrimonioIdAsync_ReturnsOrderedDescending()
    {
        await using var context = await CreateContextAsync();
        var categoria = new CategoriaPatrimonio { Nome = "Audio" };
        context.CategoriasPatrimonio.Add(categoria);
        await context.SaveChangesAsync();

        var item = new PatrimonioItem { Nome = "Microfone", Codigo = "PAT-100", CategoriaPatrimonioId = categoria.Id };
        context.PatrimonioItens.Add(item);
        await context.SaveChangesAsync();

        context.PatrimonioMovimentacoes.AddRange(
            new PatrimonioMovimentacao { PatrimonioItemId = item.Id, TipoMovimentacao = "Transferencia", DataMovimentacao = new DateTime(2026, 4, 2) },
            new PatrimonioMovimentacao { PatrimonioItemId = item.Id, TipoMovimentacao = "Manutencao", DataMovimentacao = new DateTime(2026, 4, 3) });
        await context.SaveChangesAsync();

        var repository = new PatrimonioMovimentacaoRepository(context);

        var result = (await repository.GetByPatrimonioIdAsync(item.Id)).ToList();

        result.Should().HaveCount(2);
        result[0].TipoMovimentacao.Should().Be("Manutencao");
        result[1].TipoMovimentacao.Should().Be("Transferencia");
    }

    [Fact]
    public async Task CreateAsync_PersistMovimentacao()
    {
        await using var context = await CreateContextAsync();
        var categoria = new CategoriaPatrimonio { Nome = "Instrumentos" };
        context.CategoriasPatrimonio.Add(categoria);
        await context.SaveChangesAsync();

        var item = new PatrimonioItem { Nome = "Teclado", Codigo = "PAT-200", CategoriaPatrimonioId = categoria.Id };
        context.PatrimonioItens.Add(item);
        await context.SaveChangesAsync();

        var repository = new PatrimonioMovimentacaoRepository(context);

        var created = await repository.CreateAsync(new PatrimonioMovimentacao
        {
            PatrimonioItemId = item.Id,
            TipoMovimentacao = "Aquisicao",
            DataMovimentacao = new DateTime(2026, 4, 1)
        });

        created.Id.Should().BeGreaterThan(0);
        context.PatrimonioMovimentacoes.Should().ContainSingle(x => x.Id == created.Id);
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
