using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class PatrimonioMovimentacoesControllerTests
{
    private readonly Mock<IPatrimonioMovimentacaoService> _serviceMock = new();
    private readonly PatrimonioMovimentacoesController _controller;

    public PatrimonioMovimentacoesControllerTests()
    {
        _controller = new PatrimonioMovimentacoesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByPatrimonioIdAsync(4))
            .ReturnsAsync(new List<PatrimonioMovimentacaoDto>());

        var result = await _controller.GetAll(4);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.CreateAsync(4, It.IsAny<CriarPatrimonioMovimentacaoDto>()))
            .ThrowsAsync(new ArgumentException("Item patrimonial não encontrado"));

        var result = await _controller.Create(4, new CriarPatrimonioMovimentacaoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrowsUnexpectedError()
    {
        _serviceMock.Setup(s => s.CreateAsync(4, It.IsAny<CriarPatrimonioMovimentacaoDto>()))
            .ThrowsAsync(new InvalidOperationException("Falha"));

        var result = await _controller.Create(4, new CriarPatrimonioMovimentacaoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }
}
