using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoSegmentoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_OrdersByPadraoThenNome()
    {
        await using var context = await CreateContextAsync();
        context.ComunicacaoSegmentos.AddRange(
            new ComunicacaoSegmento { Nome = "Visitantes", PublicoAlvo = "visitantes", Padrao = false, Ativo = true },
            new ComunicacaoSegmento { Nome = "Membros", PublicoAlvo = "membros", Padrao = true, Ativo = true },
            new ComunicacaoSegmento { Nome = "Líderes", PublicoAlvo = "lideres", Padrao = true, Ativo = true });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoSegmentoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Take(2).Should().ContainInOrder("Líderes", "Membros");
        result.Last().Nome.Should().Be("Visitantes");
    }

    [Fact]
    public async Task CreateAndUpdateAsync_PersistSegmento()
    {
        await using var context = await CreateContextAsync();
        var repository = new ComunicacaoSegmentoRepository(context);

        var created = await repository.CreateAsync(new ComunicacaoSegmento
        {
            Nome = "Jovens",
            PublicoAlvo = "jovens",
            Ativo = true,
            Padrao = false
        });

        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Jovens Atualizado";
        created.Padrao = true;
        created.DataAtualizacao = DateTime.UtcNow;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Jovens Atualizado");
        loaded.Padrao.Should().BeTrue();
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
