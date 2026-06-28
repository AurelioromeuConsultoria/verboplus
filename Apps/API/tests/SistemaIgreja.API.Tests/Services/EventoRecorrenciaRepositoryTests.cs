using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EventoRecorrenciaRepositoryTests
{
    [Fact]
    public async Task GetByEventoAsync_OnSqlite_ThrowsBecauseTimeSpanOrderingIsNotSupported()
    {
        await using var context = await CreateContextAsync();
        var evento1 = new Evento { Titulo = "Culto 1", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Culto };
        var evento2 = new Evento { Titulo = "Culto 2", DataInicio = new DateTime(2026, 4, 2), DataFim = new DateTime(2026, 4, 2), Tipo = TipoEvento.Culto };
        context.Set<Evento>().AddRange(evento1, evento2);
        await context.SaveChangesAsync();

        context.EventosRecorrencias.AddRange(
            new EventoRecorrencia
            {
                EventoId = evento1.Id,
                DiaSemana = DayOfWeek.Sunday,
                HoraInicio = new TimeSpan(18, 0, 0),
                Periodicidade = PeriodicidadeRecorrencia.Semanal,
                DataInicioVigencia = new DateTime(2026, 4, 1),
                Ativo = true
            },
            new EventoRecorrencia
            {
                EventoId = evento2.Id,
                DiaSemana = DayOfWeek.Monday,
                HoraInicio = new TimeSpan(19, 0, 0),
                Periodicidade = PeriodicidadeRecorrencia.Semanal,
                DataInicioVigencia = new DateTime(2026, 4, 1),
                Ativo = true
            });
        await context.SaveChangesAsync();

        var repository = new EventoRecorrenciaRepository(context);

        var action = () => repository.GetByEventoAsync(evento1.Id);

        await action.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPersistedRecorrencia()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Culto", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Culto };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();

        var recorrencia = new EventoRecorrencia
        {
            EventoId = evento.Id,
            DiaSemana = DayOfWeek.Sunday,
            HoraInicio = new TimeSpan(20, 0, 0),
            Periodicidade = PeriodicidadeRecorrencia.Semanal,
            DataInicioVigencia = new DateTime(2026, 4, 1),
            Ativo = true
        };
        context.EventosRecorrencias.Add(recorrencia);
        await context.SaveChangesAsync();

        var repository = new EventoRecorrenciaRepository(context);

        var result = await repository.GetByIdAsync(recorrencia.Id);

        result.Should().NotBeNull();
        result!.EventoId.Should().Be(evento.Id);
        result.DiaSemana.Should().Be(DayOfWeek.Sunday);
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistRecorrencia()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Evento", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Evento };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();
        var repository = new EventoRecorrenciaRepository(context);

        var created = await repository.CreateAsync(new EventoRecorrencia
        {
            EventoId = evento.Id,
            DiaSemana = DayOfWeek.Wednesday,
            HoraInicio = new TimeSpan(19, 0, 0),
            Periodicidade = PeriodicidadeRecorrencia.Semanal,
            DataInicioVigencia = new DateTime(2026, 4, 1),
            Ativo = true
        });
        created.Id.Should().BeGreaterThan(0);

        created.Ativo = false;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Ativo.Should().BeFalse();

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_IgnoresMissingEntity()
    {
        await using var context = await CreateContextAsync();
        var repository = new EventoRecorrenciaRepository(context);

        var action = () => repository.DeleteAsync(321);

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
