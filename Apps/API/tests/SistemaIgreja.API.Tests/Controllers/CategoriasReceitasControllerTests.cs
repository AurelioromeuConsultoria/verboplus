using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class CategoriasReceitasControllerTests
{
    private readonly Mock<ICategoriaReceitaService> _serviceMock = new();
    private readonly CategoriasReceitasController _controller;

    public CategoriasReceitasControllerTests()
    {
        _controller = new CategoriasReceitasController(_serviceMock.Object);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarCategoriaReceitaDto>()))
            .ThrowsAsync(new ArgumentException("Categoria de receita não encontrada"));

        var result = await _controller.Update(2, new AtualizarCategoriaReceitaDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<CategoriaReceitaDto> { new() { Id = 1, Nome = "Dízimos" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync((CategoriaReceitaDto?)null);

        var result = await _controller.GetById(3);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new CategoriaReceitaDto { Id = 4, Nome = "Ofertas" };
        _serviceMock.Setup(s => s.GetByIdAsync(4)).ReturnsAsync(dto);

        var result = await _controller.GetById(4);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var created = new CategoriaReceitaDto { Id = 6, Nome = "Eventos" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaReceitaDto>())).ReturnsAsync(created);

        var result = await _controller.Create(new CriarCategoriaReceitaDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaReceitaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarCategoriaReceitaDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarCategoriaReceitaDto { Nome = "Atualizada" };
        var updated = new CategoriaReceitaDto { Id = 2, Nome = "Atualizada" };
        _serviceMock.Setup(s => s.UpdateAsync(2, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(2, dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarCategoriaReceitaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(2, new AtualizarCategoriaReceitaDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(7);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(7), Times.Once);
    }
}
