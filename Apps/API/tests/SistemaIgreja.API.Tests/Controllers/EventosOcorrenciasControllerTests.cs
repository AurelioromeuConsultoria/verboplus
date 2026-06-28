using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class EventosOcorrenciasControllerTests
{
    private readonly Mock<IEventoOcorrenciaService> _serviceMock = new();
    private readonly EventosOcorrenciasController _controller;

    public EventosOcorrenciasControllerTests()
    {
        _controller = new EventosOcorrenciasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenOcorrenciaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EventoOcorrenciaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarEventoOcorrenciaDto>()))
            .ReturnsAsync(new EventoOcorrenciaDto { Id = 4, EventoId = 1 });

        var result = await _controller.Create(new CriarEventoOcorrenciaDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task GerarPorRecorrencia_ReturnsOk_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.GerarPorRecorrenciaAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(3);

        var result = await _controller.GerarPorRecorrencia(1, DateTime.Today, DateTime.Today.AddDays(30));

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
