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

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<FornecedorDto> { new() { Id = 2, Nome = "Gráfica" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new FornecedorDto { Id = 3, Nome = "Som & Luz" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarFornecedorDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarFornecedorDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarFornecedorDto { Nome = "Fornecedor Atualizado" };
        var updated = new FornecedorDto { Id = 5, Nome = "Fornecedor Atualizado" };
        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(5, dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarFornecedorDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(5, new AtualizarFornecedorDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarFornecedorDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(5, new AtualizarFornecedorDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.Delete(6);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(6), Times.Once);
    }
}
