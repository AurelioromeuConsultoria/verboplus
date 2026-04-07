using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class EnquetesControllerTests
{
    private readonly Mock<IEnqueteService> _serviceMock = new();
    private readonly EnquetesController _controller;

    public EnquetesControllerTests()
    {
        _controller = new EnquetesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEnqueteDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EnqueteDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenOptionsAreMissing()
    {
        var result = await _controller.Create(new CriarEnqueteDto { Opcoes = [] });

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarEnqueteDto>()))
            .ThrowsAsync(new ArgumentException("Enquete não encontrada"));

        var result = await _controller.Update(2, new AtualizarEnqueteDto
        {
            Opcoes = [new AtualizarEnqueteOpcaoDto { Texto = "Opcao", Ordem = 1 }]
        });

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }
}
