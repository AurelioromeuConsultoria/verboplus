using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class HubCasasControllerTests
{
    private readonly Mock<IHubCasaService> _serviceMock = new();
    private readonly HubCasasController _controller;

    public HubCasasControllerTests()
    {
        _controller = new HubCasasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllAndGetById_ReturnExpectedResponses()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<HubCasaDto>());
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((HubCasaDto?)null);

        (await _controller.GetAll()).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await _controller.GetById(1)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task CreateUpdateDelete_ReturnExpectedResponses()
    {
        var createDto = new CriarHubCasaDto { Nome = "Casa Centro", AbertoPorId = 1, LiderId = 2, TimoteoId = 3, EnderecoCompleto = "Rua A", Anfitriao = "Jose" };
        _serviceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(new HubCasaDto { Id = 5, Nome = "Casa Centro" });

        var updateDto = new AtualizarHubCasaDto { Nome = "Casa Atualizada", AbertoPorId = 1, LiderId = 2, TimoteoId = 3, EnderecoCompleto = "Rua B", Anfitriao = "Jose" };
        _serviceMock.Setup(s => s.UpdateAsync(5, updateDto)).ReturnsAsync(new HubCasaDto { Id = 5, Nome = "Casa Atualizada" });
        _serviceMock.Setup(s => s.DeleteAsync(5)).Returns(Task.CompletedTask);

        (await _controller.Create(createDto)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
        (await _controller.Update(5, updateDto)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await _controller.Delete(5)).Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new HubCasaDto { Id = 3, Nome = "Casa Norte" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarHubCasaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarHubCasaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(10, It.IsAny<AtualizarHubCasaDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(10, new AtualizarHubCasaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(10, It.IsAny<AtualizarHubCasaDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(10, new AtualizarHubCasaDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }
}
