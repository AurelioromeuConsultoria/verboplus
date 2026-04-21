using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EventoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByStartDate()
    {
        await using var context = await CreateContextAsync();
        context.Set<Evento>().AddRange(
            new Evento { Titulo = "Mais tarde", DataInicio = new DateTime(2026, 4, 10), DataFim = new DateTime(2026, 4, 10), Tipo = TipoEvento.Evento },
            new Evento { Titulo = "Mais cedo", DataInicio = new DateTime(2026, 4, 2), DataFim = new DateTime(2026, 4, 2), Tipo = TipoEvento.Culto });
        await context.SaveChangesAsync();

        var repository = new EventoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Titulo.Should().Be("Mais cedo");
        result[1].Titulo.Should().Be("Mais tarde");
    }

    [Fact]
    public async Task GetByPeriodoAsync_FiltersOverlappingEvents_AndOrdersByStart()
    {
        await using var context = await CreateContextAsync();
        context.Set<Evento>().AddRange(
            new Evento { Titulo = "Fora", DataInicio = new DateTime(2026, 3, 1), DataFim = new DateTime(2026, 3, 1), Tipo = TipoEvento.Evento },
            new Evento { Titulo = "Dentro 1", DataInicio = new DateTime(2026, 4, 2), DataFim = new DateTime(2026, 4, 2), Tipo = TipoEvento.Culto },
            new Evento { Titulo = "Dentro 2", DataInicio = new DateTime(2026, 4, 10), DataFim = new DateTime(2026, 4, 10), Tipo = TipoEvento.Reuniao });
        await context.SaveChangesAsync();

        var repository = new EventoRepository(context);

        var result = (await repository.GetByPeriodoAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30))).ToList();

        result.Select(x => x.Titulo).Should().ContainInOrder("Dentro 1", "Dentro 2");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistEvento()
    {
        await using var context = await CreateContextAsync();
        var repository = new EventoRepository(context);

        var created = await repository.CreateAsync(new Evento
        {
            Titulo = "Culto Jovem",
            DataInicio = new DateTime(2026, 4, 8, 19, 0, 0),
            DataFim = new DateTime(2026, 4, 8, 21, 0, 0),
            Tipo = TipoEvento.Culto
        });
        created.Id.Should().BeGreaterThan(0);

        created.Titulo = "Culto Jovem Atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Titulo.Should().Be("Culto Jovem Atualizado");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_IgnoresMissingEntity()
    {
        await using var context = await CreateContextAsync();
        var repository = new EventoRepository(context);

        var action = () => repository.DeleteAsync(999);

        await action.Should().NotThrowAsync();
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
