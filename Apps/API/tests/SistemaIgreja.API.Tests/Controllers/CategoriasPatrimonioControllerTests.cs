using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class CategoriasPatrimonioControllerTests
{
    private readonly Mock<ICategoriaPatrimonioService> _serviceMock = new();
    private readonly CategoriasPatrimonioController _controller;

    public CategoriasPatrimonioControllerTests()
    {
        _controller = new CategoriasPatrimonioController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CategoriaPatrimonioDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaPatrimonioDto>()))
            .ReturnsAsync(new CategoriaPatrimonioDto { Id = 4, Nome = "Audio" });

        var result = await _controller.Create(new CriarCategoriaPatrimonioDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenCategoriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarCategoriaPatrimonioDto>()))
            .ThrowsAsync(new ArgumentException("Categoria de patrimônio não encontrada"));

        var result = await _controller.Update(5, new AtualizarCategoriaPatrimonioDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<CategoriaPatrimonioDto> { new() { Id = 2, Nome = "Instrumentos" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenCategoriaExists()
    {
        var dto = new CategoriaPatrimonioDto { Id = 3, Nome = "Som" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaPatrimonioDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarCategoriaPatrimonioDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarCategoriaPatrimonioDto { Nome = "Atualizada" };
        var updated = new CategoriaPatrimonioDto { Id = 5, Nome = "Atualizada" };
        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(5, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarCategoriaPatrimonioDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(5, new AtualizarCategoriaPatrimonioDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(6);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(6), Times.Once);
    }
}
