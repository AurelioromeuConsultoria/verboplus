using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class EquipesControllerTests
{
    private readonly Mock<IEquipeService> _serviceMock = new();
    private readonly EquipesController _controller;

    public EquipesControllerTests()
    {
        _controller = new EquipesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<EquipeDto>());
        var result = await _controller.GetAll();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_WhenNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EquipeDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CriarEquipeDto { Nome = "Equipe", Area = 1 };
        var created = new EquipeDto { Id = 1, Nome = dto.Nome, Area = dto.Area, DataCriacao = DateTime.UtcNow };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);
        var result = await _controller.Create(dto);
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_OnArgumentException()
    {
        var dto = new AtualizarEquipeDto { Nome = "X", Area = 2 };
        _serviceMock.Setup(s => s.UpdateAsync(9, dto)).ThrowsAsync(new ArgumentException());
        var result = await _controller.Update(9, dto);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(3)).Returns(Task.CompletedTask);
        var result = await _controller.Delete(3);
        result.Should().BeOfType<NoContentResult>();
    }
}
