using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class GaleriaFotoItemRepositoryTests
{
    [Fact]
    public async Task GetByGaleriaIdAndSetDestaqueAsync_WorkAsExpected()
    {
        await using var context = await CreateContextAsync();
        var galeria = new GaleriaFoto { Nome = "Retiro", Data = new DateTime(2026, 4, 1), CaminhoDiretorio = "/galerias/retiro" };
        context.GaleriasFotos.Add(galeria);
        await context.SaveChangesAsync();

        context.GaleriasFotosItens.AddRange(
            new GaleriaFotoItem { GaleriaFotoId = galeria.Id, NomeArquivo = "b.jpg", Ordem = 2 },
            new GaleriaFotoItem { GaleriaFotoId = galeria.Id, NomeArquivo = "a.jpg", Ordem = 1 });
        await context.SaveChangesAsync();

        var repository = new GaleriaFotoItemRepository(context);

        var ordered = await repository.GetByGaleriaIdAsync(galeria.Id);
        ordered.Select(x => x.NomeArquivo).Should().ContainInOrder("a.jpg", "b.jpg");

        await repository.SetDestaqueAsync(galeria.Id, "B.jpg");

        var after = await repository.GetByGaleriaIdAsync(galeria.Id);
        after.Should().ContainSingle(x => x.Destaque && x.NomeArquivo == "b.jpg");
    }

    [Fact]
    public async Task AddRangeAsync_PersistItems()
    {
        await using var context = await CreateContextAsync();
        var galeria = new GaleriaFoto { Nome = "Conferencia", Data = new DateTime(2026, 4, 2), CaminhoDiretorio = "/galerias/conferencia" };
        context.GaleriasFotos.Add(galeria);
        await context.SaveChangesAsync();

        var repository = new GaleriaFotoItemRepository(context);

        await repository.AddRangeAsync(
        [
            new GaleriaFotoItem { GaleriaFotoId = galeria.Id, NomeArquivo = "1.jpg", Ordem = 1 },
            new GaleriaFotoItem { GaleriaFotoId = galeria.Id, NomeArquivo = "2.jpg", Ordem = 2 }
        ]);

        context.GaleriasFotosItens.Should().HaveCount(2);
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
