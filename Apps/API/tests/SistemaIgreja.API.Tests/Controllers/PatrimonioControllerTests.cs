using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class PatrimonioControllerTests
{
    private readonly Mock<IPatrimonioItemService> _serviceMock = new();
    private readonly PatrimonioController _controller;

    public PatrimonioControllerTests()
    {
        _controller = new PatrimonioController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((PatrimonioItemDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarPatrimonioItemDto>()))
            .ReturnsAsync(new PatrimonioItemDto { Id = 3, Nome = "Projetor" });

        var result = await _controller.Create(new CriarPatrimonioItemDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsInvalidOperation()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarPatrimonioItemDto>()))
            .ThrowsAsync(new InvalidOperationException("Código duplicado"));

        var result = await _controller.Update(2, new AtualizarPatrimonioItemDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }
}
