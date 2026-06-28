using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ProjetosControllerTests
{
    private readonly Mock<IProjetoService> _serviceMock = new();
    private readonly ProjetosController _controller;

    public ProjetosControllerTests()
    {
        _controller = new ProjetosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ProjetoDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenProjetoDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync((ProjetoDto?)null);

        var result = await _controller.GetById(2);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarProjetoDto>()))
            .ReturnsAsync(new ProjetoDto { Id = 3, Nome = "Construção" });

        var result = await _controller.Create(new CriarProjetoDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarProjetoDto>()))
            .ThrowsAsync(new ArgumentException("Projeto não encontrado"));

        var result = await _controller.Update(2, new AtualizarProjetoDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(2);

        result.Should().BeOfType<NoContentResult>();
    }
}
