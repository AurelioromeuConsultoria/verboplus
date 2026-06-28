using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class InscricaoEventoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsItemsOrderedByMostRecentInscricao()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Conferência", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Evento };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();

        context.Set<InscricaoEvento>().AddRange(
            new InscricaoEvento { EventoId = evento.Id, Nome = "Marco", WhatsApp = "111", Status = StatusInscricao.Pendente, DataInscricao = new DateTime(2026, 4, 1) },
            new InscricaoEvento { EventoId = evento.Id, Nome = "Aline", WhatsApp = "222", Status = StatusInscricao.Confirmada, DataInscricao = new DateTime(2026, 4, 3) });
        await context.SaveChangesAsync();

        var repository = new InscricaoEventoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].Nome.Should().Be("Aline");
        result[1].Nome.Should().Be("Marco");
    }

    [Fact]
    public async Task QueryHelpers_FilterAndCountExpectedItems()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Conferência", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Evento };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();

        context.Set<InscricaoEvento>().AddRange(
            new InscricaoEvento { EventoId = evento.Id, Nome = "Marco", WhatsApp = "111", Status = StatusInscricao.Pendente, DataInscricao = new DateTime(2026, 4, 1) },
            new InscricaoEvento { EventoId = evento.Id, Nome = "Aline", WhatsApp = "222", Status = StatusInscricao.Confirmada, DataInscricao = new DateTime(2026, 4, 2) });
        await context.SaveChangesAsync();

        var repository = new InscricaoEventoRepository(context);

        (await repository.GetByEventoAsync(evento.Id)).Should().HaveCount(2);
        (await repository.GetByStatusAsync(StatusInscricao.Confirmada)).Should().ContainSingle();
        (await repository.ContarInscricoesPorEventoAsync(evento.Id)).Should().Be(2);
        (await repository.ContarInscricoesConfirmadasPorEventoAsync(evento.Id)).Should().Be(1);
        (await repository.ExisteInscricaoAsync(evento.Id, "111")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistInscricao()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Encontro", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Evento };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();
        var repository = new InscricaoEventoRepository(context);

        var created = await repository.CreateAsync(new InscricaoEvento
        {
            EventoId = evento.Id,
            Nome = "João",
            WhatsApp = "333",
            Status = StatusInscricao.Pendente
        });
        created.Id.Should().BeGreaterThan(0);

        created.Status = StatusInscricao.Confirmada;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(StatusInscricao.Confirmada);

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_IgnoresMissingEntity()
    {
        await using var context = await CreateContextAsync();
        var repository = new InscricaoEventoRepository(context);

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
