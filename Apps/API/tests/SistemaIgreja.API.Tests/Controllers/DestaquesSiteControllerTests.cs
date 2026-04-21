using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class DestaquesSiteControllerTests
{
    private readonly Mock<IDestaqueSiteService> _serviceMock = new();
    private readonly DestaquesSiteController _controller;

    public DestaquesSiteControllerTests()
    {
        _controller = new DestaquesSiteController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDestaqueDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((DestaqueSiteDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarDestaqueSiteDto>()))
            .ReturnsAsync(new DestaqueSiteDto { Id = 3, Texto = "Topo" });

        var result = await _controller.Create(new CriarDestaqueSiteDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<DestaqueSiteDto> { new() { Id = 2, Texto = "Banner" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenDestaqueExists()
    {
        var dto = new DestaqueSiteDto { Id = 4, Texto = "Home" };
        _serviceMock.Setup(s => s.GetByIdAsync(4)).ReturnsAsync(dto);

        var result = await _controller.GetById(4);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarDestaqueSiteDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarDestaqueSiteDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarDestaqueSiteDto { Texto = "Atualizado" };
        var updated = new DestaqueSiteDto { Id = 5, Texto = "Atualizado" };
        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(5, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarDestaqueSiteDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(5, new AtualizarDestaqueSiteDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(5, It.IsAny<AtualizarDestaqueSiteDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(5, new AtualizarDestaqueSiteDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceSucceeds()
    {
        var result = await _controller.Delete(6);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(6), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.DeleteAsync(6)).ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Delete(6);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }
}
