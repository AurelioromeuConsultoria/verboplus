using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class EventoRecorrenciaServiceTests
{
    private readonly Mock<IEventoRecorrenciaRepository> _repositoryMock = new();
    private readonly Mock<IEventoRepository> _eventoRepositoryMock = new();
    private readonly EventoRecorrenciaService _service;

    public EventoRecorrenciaServiceTests()
    {
        _service = new EventoRecorrenciaService(_repositoryMock.Object, _eventoRepositoryMock.Object);
    }

    [Fact]
    public async Task GetByEventoAsync_Throws_WhenEventoDoesNotExist()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Evento?)null);

        var act = () => _service.GetByEventoAsync(1);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Evento não encontrado");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenHoraFimIsBeforeHoraInicio()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Evento { Id = 1, Titulo = "Culto" });

        var act = () => _service.CreateAsync(new CriarEventoRecorrenciaDto
        {
            EventoId = 1,
            DiaSemana = 1,
            HoraInicio = "19:00",
            HoraFim = "18:00",
            Periodicidade = 1,
            DataInicioVigencia = new DateTime(2026, 5, 1)
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Hora fim deve ser maior que hora início");
    }

    [Fact]
    public async Task CreateAsync_CreatesRecorrencia_WhenDataIsValid()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Evento { Id = 1, Titulo = "Culto" });
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<EventoRecorrencia>()))
            .ReturnsAsync((EventoRecorrencia recorrencia) =>
            {
                recorrencia.Id = 8;
                return recorrencia;
            });

        var result = await _service.CreateAsync(new CriarEventoRecorrenciaDto
        {
            EventoId = 1,
            DiaSemana = 1,
            HoraInicio = "19:00",
            HoraFim = "20:00",
            Periodicidade = 1,
            DataInicioVigencia = new DateTime(2026, 5, 1),
            Ativo = true
        });

        result.Id.Should().Be(8);
        result.HoraInicio.Should().Be("19:00");
        result.HoraFim.Should().Be("20:00");
        result.PeriodicidadeDescricao.Should().Be("Semanal");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenRecorrenciaDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync((EventoRecorrencia?)null);

        var act = () => _service.UpdateAsync(8, new AtualizarEventoRecorrenciaDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Recorrência não encontrada");
    }
}
