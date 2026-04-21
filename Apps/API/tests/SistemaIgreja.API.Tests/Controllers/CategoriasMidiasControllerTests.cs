using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;

namespace SistemaIgreja.API.Tests.Controllers;

public class CategoriasMidiasControllerTests
{
    private readonly Mock<ICategoriaMidiaService> _serviceMock = new();
    private readonly CategoriasMidiasController _controller;

    public CategoriasMidiasControllerTests()
    {
        _controller = new CategoriasMidiasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CategoriaMidiaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenServiceReturnsFalse()
    {
        _serviceMock.Setup(s => s.DeleteAsync(4)).ReturnsAsync(false);

        var result = await _controller.Delete(4);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<CategoriaMidiaDto> { new() { Id = 2, Nome = "Vídeos" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenCategoriaExists()
    {
        var dto = new CategoriaMidiaDto { Id = 3, Nome = "Fotos" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var created = new CategoriaMidiaDto { Id = 5, Nome = "Podcasts" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaMidiaDto>())).ReturnsAsync(created);

        var result = await _controller.Create(new CriarCategoriaMidiaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaMidiaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarCategoriaMidiaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarCategoriaMidiaDto { Nome = "Atualizada" };
        var updated = new CategoriaMidiaDto { Id = 6, Nome = "Atualizada" };
        _serviceMock.Setup(s => s.UpdateAsync(6, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(6, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(6, It.IsAny<AtualizarCategoriaMidiaDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(6, new AtualizarCategoriaMidiaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(6, It.IsAny<AtualizarCategoriaMidiaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(6, new AtualizarCategoriaMidiaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceReturnsTrue()
    {
        _serviceMock.Setup(s => s.DeleteAsync(7)).ReturnsAsync(true);

        var result = await _controller.Delete(7);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
    }
}
