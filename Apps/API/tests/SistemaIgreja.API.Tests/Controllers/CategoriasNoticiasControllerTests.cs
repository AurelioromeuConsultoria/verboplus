using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class CategoriasNoticiasControllerTests
{
    private readonly Mock<ICategoriaNoticiaService> _serviceMock = new();
    private readonly CategoriasNoticiasController _controller;

    public CategoriasNoticiasControllerTests()
    {
        _controller = new CategoriasNoticiasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CategoriaNoticiaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }
}
