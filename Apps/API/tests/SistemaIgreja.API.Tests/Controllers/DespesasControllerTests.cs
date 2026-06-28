using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class DespesasControllerTests
{
    private readonly Mock<IDespesaService> _serviceMock = new();
    private readonly DespesasController _controller;

    public DespesasControllerTests()
    {
        _controller = new DespesasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((DespesaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarDespesaDto>()))
            .ThrowsAsync(new InvalidOperationException("Falha"));

        var result = await _controller.Create(new CriarDespesaDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
