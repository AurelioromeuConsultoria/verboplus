using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class EscalasModelosControllerTests
{
    private readonly Mock<IEscalaModeloService> _serviceMock = new();
    private readonly EscalasModelosController _controller;

    public EscalasModelosControllerTests()
    {
        _controller = new EscalasModelosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EscalaModeloDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByEquipeAndEvento_ReturnOk()
    {
        _serviceMock.Setup(s => s.GetByEquipeAsync(2)).ReturnsAsync(new List<EscalaModeloDto>());
        _serviceMock.Setup(s => s.GetByEventoAsync(3)).ReturnsAsync(new List<EscalaModeloDto>());

        (await _controller.GetByEquipe(2)).Result.Should().BeOfType<OkObjectResult>();
        (await _controller.GetByEvento(3)).Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByEventoAndEquipe_ReturnsOk_WhenFound()
    {
        _serviceMock.Setup(s => s.GetByEventoAndEquipeAsync(5, 7))
            .ReturnsAsync(new EscalaModeloDto { Id = 11, EquipeId = 7, EquipeNome = "Louvor" });

        var result = await _controller.GetByEventoAndEquipe(5, 7);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateUpdateDelete_ReturnExpectedResponses()
    {
        var createDto = new CriarEscalaModeloDto { EquipeId = 3 };
        _serviceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(new EscalaModeloDto { Id = 12, EquipeId = 3, EquipeNome = "Audio" });

        var updateDto = new AtualizarEscalaModeloDto { Ativo = true };
        _serviceMock.Setup(s => s.UpdateAsync(12, updateDto)).ReturnsAsync(new EscalaModeloDto { Id = 12, EquipeId = 3, EquipeNome = "Audio" });
        _serviceMock.Setup(s => s.DeleteAsync(12)).Returns(Task.CompletedTask);

        (await _controller.Create(createDto)).Result.Should().BeOfType<CreatedAtActionResult>();
        (await _controller.Update(12, updateDto)).Result.Should().BeOfType<OkObjectResult>();
        (await _controller.Delete(12)).Should().BeOfType<NoContentResult>();
    }
}
