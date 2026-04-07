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
}
