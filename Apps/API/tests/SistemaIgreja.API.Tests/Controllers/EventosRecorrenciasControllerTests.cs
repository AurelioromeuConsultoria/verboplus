using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class EventosRecorrenciasControllerTests
{
    private readonly Mock<IEventoRecorrenciaService> _serviceMock = new();
    private readonly EventosRecorrenciasController _controller;

    public EventosRecorrenciasControllerTests()
    {
        _controller = new EventosRecorrenciasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetByEvento_ReturnsNotFound_WhenEventoDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByEventoAsync(1))
            .ThrowsAsync(new ArgumentException("Evento não encontrado"));

        var result = await _controller.GetByEvento(1);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEventoIdDoesNotMatchUrl()
    {
        var result = await _controller.Create(1, new CriarEventoRecorrenciaDto { EventoId = 2 });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenRecorrenciaBelongsToAnotherEvento()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(5))
            .ReturnsAsync(new EventoRecorrenciaDto { Id = 5, EventoId = 2 });

        var result = await _controller.Update(1, 5, new AtualizarEventoRecorrenciaDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenRecorrenciaBelongsToEvento()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(5))
            .ReturnsAsync(new EventoRecorrenciaDto { Id = 5, EventoId = 1 });

        var result = await _controller.Delete(1, 5);

        result.Should().BeOfType<NoContentResult>();
    }
}
