using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class CategoriasDespesasControllerTests
{
    private readonly Mock<ICategoriaDespesaService> _serviceMock = new();
    private readonly CategoriasDespesasController _controller;

    public CategoriasDespesasControllerTests()
    {
        _controller = new CategoriasDespesasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CategoriaDespesaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarCategoriaDespesaDto>()))
            .ReturnsAsync(new CategoriaDespesaDto { Id = 3, Nome = "Infra" });

        var result = await _controller.Create(new CriarCategoriaDespesaDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }
}
