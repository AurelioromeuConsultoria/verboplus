using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class CargosControllerTests
{
    private readonly Mock<ICargoService> _serviceMock = new();
    private readonly CargosController _controller;

    public CargosControllerTests()
    {
        _controller = new CargosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CargoDto>());
        var result = await _controller.GetAll();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_WhenNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CargoDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CriarCargoDto { Nome = "Cargo" };
        var created = new CargoDto { Id = 1, Nome = dto.Nome, DataCriacao = DateTime.UtcNow };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);
        var result = await _controller.Create(dto);
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_OnArgumentException()
    {
        var dto = new AtualizarCargoDto { Nome = "X" };
        _serviceMock.Setup(s => s.UpdateAsync(9, dto)).ThrowsAsync(new ArgumentException());
        var result = await _controller.Update(9, dto);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(3)).Returns(Task.CompletedTask);
        var result = await _controller.Delete(3);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new CargoDto { Id = 2, Nome = "Líder", DataCriacao = DateTime.UtcNow };
        _serviceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync(dto);

        var result = await _controller.GetById(2);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCargoDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarCargoDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarCargoDto { Nome = "Atualizado" };
        var updated = new CargoDto { Id = 9, Nome = "Atualizado", DataCriacao = DateTime.UtcNow };
        _serviceMock.Setup(s => s.UpdateAsync(9, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(9, dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(9, It.IsAny<AtualizarCargoDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(9, new AtualizarCargoDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
