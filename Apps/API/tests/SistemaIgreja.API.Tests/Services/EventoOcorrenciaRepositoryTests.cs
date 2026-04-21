using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EventoOcorrenciaRepositoryTests
{
    [Fact]
    public async Task GetByEventoAsync_ReturnsOrderedOccurrences()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Culto", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Culto };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();

        context.EventosOcorrencias.AddRange(
            new EventoOcorrencia
            {
                EventoId = evento.Id,
                DataHoraInicio = new DateTime(2026, 4, 12, 19, 0, 0),
                DataHoraFim = new DateTime(2026, 4, 12, 21, 0, 0),
                Status = StatusEventoOcorrencia.Confirmado
            },
            new EventoOcorrencia
            {
                EventoId = evento.Id,
                DataHoraInicio = new DateTime(2026, 4, 8, 19, 0, 0),
                DataHoraFim = new DateTime(2026, 4, 8, 21, 0, 0),
                Status = StatusEventoOcorrencia.Confirmado
            });
        await context.SaveChangesAsync();

        var repository = new EventoOcorrenciaRepository(context);

        var result = (await repository.GetByEventoAsync(evento.Id)).ToList();

        result.Should().HaveCount(2);
        result[0].DataHoraInicio.Should().Be(new DateTime(2026, 4, 8, 19, 0, 0));
        result[1].DataHoraInicio.Should().Be(new DateTime(2026, 4, 12, 19, 0, 0));
    }

    [Fact]
    public async Task GetByPeriodoAndHelpers_ReturnExpectedResults()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Culto", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Culto };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();

        var recorrencia = new EventoRecorrencia
        {
            EventoId = evento.Id,
            DiaSemana = DayOfWeek.Sunday,
            HoraInicio = new TimeSpan(19, 0, 0),
            Periodicidade = PeriodicidadeRecorrencia.Semanal,
            DataInicioVigencia = new DateTime(2026, 4, 1),
            Ativo = true
        };
        context.EventosRecorrencias.Add(recorrencia);
        await context.SaveChangesAsync();

        var ocorrencia = new EventoOcorrencia
        {
            EventoId = evento.Id,
            EventoRecorrenciaId = recorrencia.Id,
            DataHoraInicio = new DateTime(2026, 4, 8, 19, 0, 0),
            DataHoraFim = new DateTime(2026, 4, 8, 21, 0, 0),
            Status = StatusEventoOcorrencia.Confirmado
        };
        context.EventosOcorrencias.Add(ocorrencia);
        await context.SaveChangesAsync();

        var repository = new EventoOcorrenciaRepository(context);

        (await repository.ExistsAsync(ocorrencia.Id)).Should().BeTrue();
        (await repository.ExistsOcorrenciaNoHorarioAsync(evento.Id, ocorrencia.DataHoraInicio)).Should().BeTrue();

        var byPeriodo = (await repository.GetByPeriodoAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), evento.Id)).ToList();
        byPeriodo.Should().ContainSingle();
        byPeriodo[0].Evento.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistOcorrencia()
    {
        await using var context = await CreateContextAsync();
        var evento = new Evento { Titulo = "Evento", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Evento };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();
        var repository = new EventoOcorrenciaRepository(context);

        var created = await repository.CreateAsync(new EventoOcorrencia
        {
            EventoId = evento.Id,
            DataHoraInicio = new DateTime(2026, 4, 12, 10, 0, 0),
            DataHoraFim = new DateTime(2026, 4, 12, 12, 0, 0),
            Status = StatusEventoOcorrencia.Confirmado
        });
        created.Id.Should().BeGreaterThan(0);

        created.Status = StatusEventoOcorrencia.Realizado;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(StatusEventoOcorrencia.Realizado);

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_IgnoresMissingEntity()
    {
        await using var context = await CreateContextAsync();
        var repository = new EventoOcorrenciaRepository(context);

        var action = () => repository.DeleteAsync(456);

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
