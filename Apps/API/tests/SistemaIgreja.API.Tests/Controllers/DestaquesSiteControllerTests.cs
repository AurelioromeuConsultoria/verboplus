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
}
