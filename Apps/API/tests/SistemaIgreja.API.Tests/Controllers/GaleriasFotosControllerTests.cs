using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;

namespace SistemaIgreja.API.Tests.Controllers;

public class GaleriasFotosControllerTests
{
    private readonly Mock<IGaleriaFotoService> _serviceMock = new();
    private readonly Mock<IWebHostEnvironment> _environmentMock = new();
    private readonly GaleriasFotosController _controller;

    public GaleriasFotosControllerTests()
    {
        _environmentMock.SetupGet(e => e.ContentRootPath).Returns("/tmp/appigreja");
        _controller = new GaleriasFotosController(_serviceMock.Object, _environmentMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenGaleriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((GaleriaFotoDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task ListarFotos_ReturnsNotFound_WhenGaleriaDoesNotExistAndNoPhotos()
    {
        _serviceMock.Setup(s => s.ListarFotosAsync(2, It.IsAny<string>())).ReturnsAsync([]);
        _serviceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync((GaleriaFotoDto?)null);

        var result = await _controller.ListarFotos(2);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>();
    }

    [Fact]
    public async Task UploadFotos_ReturnsBadRequest_WhenNoFilesAreSent()
    {
        var result = await _controller.UploadFotos(5, []);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<GaleriaFotoDto> { new() { Id = 2, Nome = "Retiro" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetAtivas_ReturnsOkWithItems()
    {
        var items = new List<GaleriaFotoDto> { new() { Id = 3, Nome = "Conferência" } };
        _serviceMock.Setup(s => s.GetAtivasAsync()).ReturnsAsync(items);

        var result = await _controller.GetAtivas();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenGaleriaExists()
    {
        var dto = new GaleriaFotoDto { Id = 4, Nome = "Batismo" };
        _serviceMock.Setup(s => s.GetByIdAsync(4)).ReturnsAsync(dto);

        var result = await _controller.GetById(4);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetByEvento_ReturnsOkWithItems()
    {
        var items = new List<GaleriaFotoDto> { new() { Id = 5, Nome = "Evento" } };
        _serviceMock.Setup(s => s.GetByEventoIdAsync(7)).ReturnsAsync(items);

        var result = await _controller.GetByEvento(7);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetByCategoria_ReturnsOkWithItems()
    {
        var items = new List<GaleriaFotoDto> { new() { Id = 6, Nome = "Categoria" } };
        _serviceMock.Setup(s => s.GetByCategoriaMidiaIdAsync(8)).ReturnsAsync(items);

        var result = await _controller.GetByCategoria(8);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var created = new GaleriaFotoDto { Id = 9, Nome = "Nova Galeria" };
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarGaleriaFotoDto>())).ReturnsAsync(created);

        var result = await _controller.Create(new CriarGaleriaFotoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarGaleriaFotoDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarGaleriaFotoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarGaleriaFotoDto { Nome = "Atualizada" };
        var updated = new GaleriaFotoDto { Id = 10, Nome = "Atualizada" };
        _serviceMock.Setup(s => s.UpdateAsync(10, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(10, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(10, It.IsAny<AtualizarGaleriaFotoDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(10, new AtualizarGaleriaFotoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(10, It.IsAny<AtualizarGaleriaFotoDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(10, new AtualizarGaleriaFotoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenGaleriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(11)).ReturnsAsync((GaleriaFotoDto?)null);

        var result = await _controller.Delete(11);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task DefinirDestaque_ReturnsOk_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.DefinirImagemDestaqueAsync(12, "foto.jpg")).ReturnsAsync(true);

        var result = await _controller.DefinirDestaque(12, "foto.jpg");

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task DefinirDestaque_ReturnsNotFound_WhenServiceFails()
    {
        _serviceMock.Setup(s => s.DefinirImagemDestaqueAsync(12, "foto.jpg")).ReturnsAsync(false);

        var result = await _controller.DefinirDestaque(12, "foto.jpg");

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>();
    }

    [Fact]
    public async Task SyncItens_ReturnsNotFound_WhenGaleriaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(13)).ReturnsAsync((GaleriaFotoDto?)null);

        var result = await _controller.SyncItens(13);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundObjectResult>();
    }
}
