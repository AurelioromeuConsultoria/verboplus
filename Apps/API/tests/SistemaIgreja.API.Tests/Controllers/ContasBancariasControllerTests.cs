using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ContasBancariasControllerTests
{
    private readonly Mock<IContaBancariaService> _serviceMock = new();
    private readonly ContasBancariasController _controller;

    public ContasBancariasControllerTests()
    {
        _controller = new ContasBancariasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ContaBancariaDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarContaBancariaDto>()))
            .ThrowsAsync(new ArgumentException("Conta bancária não encontrada"));

        var result = await _controller.Update(5, new AtualizarContaBancariaDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
