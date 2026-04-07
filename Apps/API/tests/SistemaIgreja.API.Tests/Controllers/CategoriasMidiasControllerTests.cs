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
}
