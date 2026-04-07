using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class EventosControllerTests
{
    private readonly Mock<IEventoService> _serviceMock = new();
    private readonly EventosController _controller;

    public EventosControllerTests()
    {
        _controller = new EventosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEventoDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EventoDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarEventoDto>()))
            .ReturnsAsync(new EventoDto { Id = 8, Titulo = "Conferencia" });

        var result = await _controller.Create(new CriarEventoDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.DeleteAsync(8)).ThrowsAsync(new InvalidOperationException("Falha"));

        var result = await _controller.Delete(8);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
