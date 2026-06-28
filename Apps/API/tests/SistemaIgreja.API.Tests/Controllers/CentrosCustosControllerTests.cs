using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class CentrosCustosControllerTests
{
    private readonly Mock<ICentroCustoService> _serviceMock = new();
    private readonly CentrosCustosController _controller;

    public CentrosCustosControllerTests()
    {
        _controller = new CentrosCustosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CentroCustoDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<CentroCustoDto> { new() { Id = 2, Nome = "Operação" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new CentroCustoDto { Id = 3, Nome = "Projetos" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var created = new CentroCustoDto { Id = 4, Nome = "Missões" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCentroCustoDto>())).ReturnsAsync(created);

        var result = await _controller.Create(new CriarCentroCustoDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCentroCustoDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarCentroCustoDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarCentroCustoDto { Nome = "Atualizado" };
        var updated = new CentroCustoDto { Id = 5, Nome = "Atualizado" };
        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(5, dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarCentroCustoDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(5, new AtualizarCentroCustoDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarCentroCustoDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(5, new AtualizarCentroCustoDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(6);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(6), Times.Once);
    }
}
