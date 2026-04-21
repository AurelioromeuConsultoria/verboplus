using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class IndisponibilidadesVoluntariosControllerTests
{
    private readonly Mock<IIndisponibilidadeVoluntarioService> _serviceMock = new();
    private readonly IndisponibilidadesVoluntariosController _controller;

    public IndisponibilidadesVoluntariosControllerTests()
    {
        _controller = new IndisponibilidadesVoluntariosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((IndisponibilidadeVoluntarioDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByVoluntario_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByVoluntarioAsync(2, null, null)).ReturnsAsync(new List<IndisponibilidadeVoluntarioDto>());
        var result = await _controller.GetByVoluntario(2, null, null);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CriarIndisponibilidadeVoluntarioDto { VoluntarioId = 3, Data = new DateTime(2026, 4, 10) };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(new IndisponibilidadeVoluntarioDto { Id = 9, VoluntarioId = 3 });

        var result = await _controller.Create(dto);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenServiceThrowsKnownArgumentException()
    {
        _serviceMock.Setup(s => s.DeleteAsync(4)).ThrowsAsync(new ArgumentException("não encontrada"));
        var result = await _controller.Delete(4);
        result.Should().BeOfType<NotFoundResult>();
    }
}
