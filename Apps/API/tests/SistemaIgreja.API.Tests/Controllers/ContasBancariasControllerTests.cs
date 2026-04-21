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

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(9)).ReturnsAsync((ContaBancariaDto?)null);

        var result = await _controller.GetById(9);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new ContaBancariaDto { Id = 3, Nome = "Conta Teste", Banco = "Banco Teste" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenServiceSucceeds()
    {
        var dto = new CriarContaBancariaDto { Nome = "Conta Principal", Banco = "Banco", Agencia = "0001", Conta = "12345" };
        var created = new ContaBancariaDto { Id = 12, Nome = "Conta Principal", Banco = "Banco" };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ContasBancariasController.GetById));
        createdResult.Value.Should().Be(created);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        var dto = new CriarContaBancariaDto();
        _serviceMock.Setup(s => s.CreateAsync(dto)).ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(dto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarContaBancariaDto { Nome = "Conta Alterada", Banco = "Banco Alterado" };
        var updated = new ContaBancariaDto { Id = 5, Nome = "Conta Alterada", Banco = "Banco Alterado" };
        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(5, dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarContaBancariaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(5, new AtualizarContaBancariaDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(7);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(7), Times.Once);
    }
}
