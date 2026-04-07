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
}
