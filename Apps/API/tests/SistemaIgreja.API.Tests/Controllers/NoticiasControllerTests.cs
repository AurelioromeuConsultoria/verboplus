using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class NoticiasControllerTests
{
    private readonly Mock<INoticiaService> _serviceMock = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly NoticiasController _controller;

    public NoticiasControllerTests()
    {
        _controller = new NoticiasController(_serviceMock.Object, new NoticiaUrlExtractorService(_httpClientFactoryMock.Object));
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNoticiaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((NoticiaDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync([new NoticiaDto { Id = 1, Titulo = "Portal" }]);

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByCategoria_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetByCategoriaAsync(2)).ReturnsAsync([new NoticiaDto { Id = 1, Titulo = "Portal" }]);

        var result = await _controller.GetByCategoria(2);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExtrairDeUrl_ReturnsBadRequest_WhenUrlIsMissing()
    {
        var result = await _controller.ExtrairDeUrl(new ExtrairNoticiaUrlRequest { Url = "" });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarNoticiaDto>()))
            .ReturnsAsync(new NoticiaDto { Id = 4, Titulo = "Portal" });

        var result = await _controller.Create(new CriarNoticiaDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.UpdateAsync(4, It.IsAny<AtualizarNoticiaDto>()))
            .ReturnsAsync(new NoticiaDto { Id = 4, Titulo = "Atualizada" });

        var result = await _controller.Update(4, new AtualizarNoticiaDto());

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(4, It.IsAny<AtualizarNoticiaDto>()))
            .ThrowsAsync(new ArgumentException());

        var result = await _controller.Update(4, new AtualizarNoticiaDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenServiceSucceeds()
    {
        var result = await _controller.Delete(4);

        result.Should().BeOfType<NoContentResult>();
    }
}
