using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.API.Tests.Services;

public class GaleriaFotoRepositoryTests
{
    [Fact]
    public async Task GetAtivasAndFilters_ReturnExpectedItems()
    {
        await using var context = await CreateContextAsync();
        var categoria = new CategoriaMidia { Nome = "Fotos" };
        var evento = new Evento { Titulo = "Conferência", Tipo = TipoEvento.Culto, DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1) };
        context.Set<CategoriaMidia>().Add(categoria);
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();

        context.Set<GaleriaFoto>().AddRange(
            new GaleriaFoto { Nome = "Ativa", Data = new DateTime(2026, 4, 10), CaminhoDiretorio = "/galeria/a", Ativo = true, EventoId = evento.Id, CategoriaMidiaId = categoria.Id },
            new GaleriaFoto { Nome = "Inativa", Data = new DateTime(2026, 4, 5), CaminhoDiretorio = "/galeria/b", Ativo = false, EventoId = evento.Id, CategoriaMidiaId = categoria.Id });
        await context.SaveChangesAsync();

        var repository = new GaleriaFotoRepository(context);

        var ativas = (await repository.GetAtivasAsync()).ToList();
        ativas.Should().ContainSingle();
        ativas[0].Nome.Should().Be("Ativa");

        var byEvento = (await repository.GetByEventoIdAsync(evento.Id)).ToList();
        byEvento.Should().HaveCount(2);
        byEvento[0].Nome.Should().Be("Ativa");

        var byCategoria = (await repository.GetByCategoriaMidiaIdAsync(categoria.Id)).ToList();
        byCategoria.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_ReturnsDeleteFlag()
    {
        await using var context = await CreateContextAsync();
        var repository = new GaleriaFotoRepository(context);

        var created = await repository.CreateAsync(new GaleriaFoto
        {
            Nome = "Galeria",
            Data = new DateTime(2026, 4, 8),
            CaminhoDiretorio = "/galeria/teste",
            Ativo = true
        });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Galeria Atualizada";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Galeria Atualizada");

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
