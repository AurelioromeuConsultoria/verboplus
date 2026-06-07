using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class EventoOcorrenciaServiceTests
{
    private readonly Mock<IEventoOcorrenciaRepository> _repositoryMock = new();
    private readonly Mock<IEventoRepository> _eventoRepositoryMock = new();
    private readonly Mock<IEscalaRepository> _escalaRepositoryMock = new();
    private readonly EventoOcorrenciaService _service;

    public EventoOcorrenciaServiceTests()
    {
        _service = new EventoOcorrenciaService(
            _repositoryMock.Object,
            _eventoRepositoryMock.Object,
            _escalaRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEventoDoesNotExist()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Evento?)null);

        var act = () => _service.CreateAsync(new CriarEventoOcorrenciaDto
        {
            EventoId = 1,
            DataHoraInicio = new DateTime(2026, 5, 10, 19, 0, 0)
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Evento não encontrado");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenHorarioConflicts()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Evento { Id = 1, Titulo = "Culto" });
        _repositoryMock.Setup(r => r.ExistsOcorrenciaNoHorarioAsync(1, It.IsAny<DateTime>())).ReturnsAsync(true);

        var act = () => _service.CreateAsync(new CriarEventoOcorrenciaDto
        {
            EventoId = 1,
            DataHoraInicio = new DateTime(2026, 5, 10, 19, 0, 0)
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Já existe ocorrência para este evento neste mesmo horário");
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenOcorrenciaHasEscalas()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new EventoOcorrencia { Id = 5, EventoId = 1 });
        _escalaRepositoryMock.Setup(r => r.GetAllByEventoOcorrenciaAsync(5))
            .ReturnsAsync(new List<Escala> { new() { Id = 1, EventoOcorrenciaId = 5, EquipeId = 2 } });

        var act = () => _service.DeleteAsync(5);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Não é possível remover ocorrência que já possui escala(s)");
    }

    [Fact]
    public async Task GetCoberturaVoluntariadoAsync_FiltersByNivelRisco()
    {
        var ocorrencia = new EventoOcorrencia
        {
            Id = 9,
            EventoId = 1,
            Evento = new Evento { Id = 1, Titulo = "Culto" },
            DataHoraInicio = new DateTime(2026, 5, 10, 19, 0, 0),
            Status = StatusEventoOcorrencia.Confirmado
        };
        var escala = new Escala
        {
            Id = 1,
            EventoOcorrenciaId = 9,
            EquipeId = 2,
            Equipe = new Equipe { Id = 2, Nome = "Louvor", Area = AreaEquipe.Verde },
            Status = StatusEscala.Publicada,
            Itens = new List<EscalaItem>
            {
                new() { Status = StatusEscalaItem.Recusado }
            }
        };

        _repositoryMock.Setup(r => r.GetByPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
            .ReturnsAsync(new List<EventoOcorrencia> { ocorrencia });
        _escalaRepositoryMock.Setup(r => r.GetAllByEventoOcorrenciaAsync(9))
            .ReturnsAsync(new List<Escala> { escala });

        var result = await _service.GetCoberturaVoluntariadoAsync(DateTime.Today, DateTime.Today.AddDays(7), null, "high");

        result.Should().ContainSingle();
        result.First().NivelRisco.Should().Be("high");
    }

    [Fact]
    public async Task GerarPorRecorrenciaAsync_CreatesOccurrencesForDatesWithoutConflict()
    {
        var evento = new Evento { Id = 1, Titulo = "Culto" };
        var recorrencia = new EventoRecorrencia
        {
            Id = 3,
            EventoId = 1,
            DiaSemana = new DateTime(2026, 5, 11).DayOfWeek,
            HoraInicio = new TimeSpan(19, 0, 0),
            HoraFim = new TimeSpan(20, 0, 0),
            Periodicidade = PeriodicidadeRecorrencia.Semanal,
            DataInicioVigencia = new DateTime(2026, 5, 1),
            Ativo = true
        };

        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(evento);
        _repositoryMock.Setup(r => r.GetRecorrenciasAtivasByEventoAsync(1))
            .ReturnsAsync(new List<EventoRecorrencia> { recorrencia });
        _repositoryMock.Setup(r => r.ExistsOcorrenciaNoHorarioAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<EventoOcorrencia>()))
            .ReturnsAsync((EventoOcorrencia ocorrencia) => ocorrencia);

        var total = await _service.GerarPorRecorrenciaAsync(1, new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));

        total.Should().BeGreaterThan(0);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<EventoOcorrencia>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GerarPorRecorrenciaAsync_MonthlyUsesSameWeekdayOrdinalFromVigencia()
    {
        var evento = new Evento { Id = 1, Titulo = "Ceia do Senhor" };
        var recorrencia = new EventoRecorrencia
        {
            Id = 3,
            EventoId = 1,
            DiaSemana = DayOfWeek.Sunday,
            HoraInicio = new TimeSpan(19, 30, 0),
            Periodicidade = PeriodicidadeRecorrencia.Mensal,
            DataInicioVigencia = new DateTime(2026, 5, 10),
            Ativo = true
        };
        var criadas = new List<EventoOcorrencia>();

        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(evento);
        _repositoryMock.Setup(r => r.GetRecorrenciasAtivasByEventoAsync(1))
            .ReturnsAsync(new List<EventoRecorrencia> { recorrencia });
        _repositoryMock.Setup(r => r.ExistsOcorrenciaNoHorarioAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<EventoOcorrencia>()))
            .ReturnsAsync((EventoOcorrencia ocorrencia) =>
            {
                criadas.Add(ocorrencia);
                return ocorrencia;
            });

        var total = await _service.GerarPorRecorrenciaAsync(1, new DateTime(2026, 6, 1), new DateTime(2026, 7, 31));

        total.Should().Be(2);
        criadas.Select(o => o.DataHoraInicio).Should().BeEquivalentTo(new[]
        {
            new DateTime(2026, 6, 14, 19, 30, 0),
            new DateTime(2026, 7, 12, 19, 30, 0)
        }, options => options.WithStrictOrdering());
    }
}
