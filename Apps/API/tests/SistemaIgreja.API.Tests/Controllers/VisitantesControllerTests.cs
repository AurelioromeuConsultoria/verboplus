using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class VisitantesControllerTests
{
    private readonly Mock<IVisitanteService> _serviceMock;
    private readonly VisitantesController _controller;

    public VisitantesControllerTests()
    {
        _serviceMock = new Mock<IVisitanteService>();
        _controller = new VisitantesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<VisitanteDto>
        {
            new() { Id = 1, Nome = "A", Telefone = "123", DataVisita = DateTime.UtcNow },
            new() { Id = 2, Nome = "B", Telefone = "456", DataVisita = DateTime.UtcNow }
        });

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().BeAssignableTo<IEnumerable<VisitanteDto>>();
        (ok.Value as IEnumerable<VisitanteDto>)!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((VisitanteDto?)null);

        var result = await _controller.GetById(99);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenOk()
    {
        var dto = new CriarVisitanteDto { Nome = "A", Telefone = "123", DataVisita = DateTime.UtcNow };
        var created = new VisitanteDto { Id = 10, Nome = dto.Nome, Telefone = dto.Telefone, DataVisita = dto.DataVisita };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdAt = result.Result as CreatedAtActionResult;
        createdAt!.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_OnArgumentException()
    {
        var dto = new AtualizarVisitanteDto { Nome = "X", Telefone = "999", DataVisita = DateTime.UtcNow };
        _serviceMock.Setup(s => s.UpdateAsync(42, dto)).ThrowsAsync(new ArgumentException("Visitante não encontrado"));

        var result = await _controller.Update(42, dto);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(5)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(5);

        result.Should().BeOfType<NoContentResult>();
    }
}
