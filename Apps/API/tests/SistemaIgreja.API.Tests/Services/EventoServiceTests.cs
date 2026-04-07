using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class EventoServiceTests
{
    private readonly Mock<IEventoRepository> _repositoryMock = new();
    private readonly EventoService _service;

    public EventoServiceTests()
    {
        _service = new EventoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_UsesFallbackTipo_WhenDtoTipoIsInvalid()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Evento>()))
            .ReturnsAsync((Evento evento) =>
            {
                evento.Id = 9;
                return evento;
            });

        var result = await _service.CreateAsync(new CriarEventoDto
        {
            Titulo = "Conferencia",
            DataInicio = new DateTime(2026, 5, 10),
            DataFim = new DateTime(2026, 5, 10),
            Tipo = 999
        });

        result.Id.Should().Be(9);
        result.Tipo.Should().Be((int)TipoEvento.Evento);
        result.TipoDescricao.Should().Be("Evento");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEventoDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync((Evento?)null);

        var act = () => _service.UpdateAsync(3, new AtualizarEventoDto());

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Evento não encontrado");
    }
}
