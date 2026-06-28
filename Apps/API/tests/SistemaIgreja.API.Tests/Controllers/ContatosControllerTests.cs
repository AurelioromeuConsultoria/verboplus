using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ContatosControllerTests
{
    private readonly Mock<IContatoService> _serviceMock = new();
    private readonly ContatosController _controller;

    public ContatosControllerTests()
    {
        _controller = new ContatosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllAndGetById_ReturnExpectedResponses()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ContatoDto>());
        _serviceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync((ContatoDto?)null);

        (await _controller.GetAll()).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await _controller.GetById(2)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task CreateUpdateDelete_ReturnExpectedResponses()
    {
        var createDto = new CriarContatoDto { Nome = "Ana", WhatsApp = "11999999999", Mensagem = "Oi" };
        _serviceMock.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(new ContatoDto { Id = 4, Nome = "Ana", WhatsApp = "11999999999", Mensagem = "Oi" });

        var updateDto = new AtualizarContatoDto { Nome = "Ana 2", WhatsApp = "11999999999", Mensagem = "Oi" };
        _serviceMock.Setup(s => s.UpdateAsync(4, updateDto)).ReturnsAsync(new ContatoDto { Id = 4, Nome = "Ana 2", WhatsApp = "11999999999", Mensagem = "Oi" });
        _serviceMock.Setup(s => s.DeleteAsync(4)).Returns(Task.CompletedTask);

        (await _controller.Create(createDto)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
        (await _controller.Update(4, updateDto)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await _controller.Delete(4)).Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
    }
}
