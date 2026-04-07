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
}
