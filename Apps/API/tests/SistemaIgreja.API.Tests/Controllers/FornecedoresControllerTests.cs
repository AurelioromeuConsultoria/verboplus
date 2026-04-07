using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class FornecedoresControllerTests
{
    private readonly Mock<IFornecedorService> _serviceMock = new();
    private readonly FornecedoresController _controller;

    public FornecedoresControllerTests()
    {
        _controller = new FornecedoresController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((FornecedorDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarFornecedorDto>()))
            .ReturnsAsync(new FornecedorDto { Id = 4, Nome = "Papelaria" });

        var result = await _controller.Create(new CriarFornecedorDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }
}
