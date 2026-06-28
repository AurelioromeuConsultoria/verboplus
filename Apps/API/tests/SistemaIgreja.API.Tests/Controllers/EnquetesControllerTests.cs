using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class EnquetesControllerTests
{
    private readonly Mock<IEnqueteService> _serviceMock = new();
    private readonly EnquetesController _controller;

    public EnquetesControllerTests()
    {
        _controller = new EnquetesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEnqueteDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((EnqueteDto?)null);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenOptionsAreMissing()
    {
        var result = await _controller.Create(new CriarEnqueteDto { Opcoes = [] });

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarEnqueteDto>()))
            .ThrowsAsync(new ArgumentException("Enquete não encontrada"));

        var result = await _controller.Update(2, new AtualizarEnqueteDto
        {
            Opcoes = [new AtualizarEnqueteOpcaoDto { Texto = "Opcao", Ordem = 1 }]
        });

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var items = new List<EnqueteDto> { new() { Id = 2, Titulo = "Pesquisa" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetAtivas_ReturnsOkWithItems()
    {
        var items = new List<EnqueteDto> { new() { Id = 3, Titulo = "Ativa" } };
        _serviceMock.Setup(s => s.GetAtivasAsync()).ReturnsAsync(items);

        var result = await _controller.GetAtivas();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenEnqueteExists()
    {
        var dto = new EnqueteDto { Id = 4, Titulo = "Comunicação" };
        _serviceMock.Setup(s => s.GetByIdAsync(4)).ReturnsAsync(dto);

        var result = await _controller.GetById(4);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        var dto = new CriarEnqueteDto
        {
            Titulo = "Nova enquete",
            Opcoes = [new CriarEnqueteOpcaoDto { Texto = "Sim", Ordem = 1 }]
        };
        var created = new EnqueteDto { Id = 5, Titulo = "Nova enquete" };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenOptionsAreMissing()
    {
        var result = await _controller.Update(2, new AtualizarEnqueteDto { Opcoes = [] });

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarEnqueteDto
        {
            Titulo = "Atualizada",
            Opcoes = [new AtualizarEnqueteOpcaoDto { Texto = "Sim", Ordem = 1 }]
        };
        var updated = new EnqueteDto { Id = 2, Titulo = "Atualizada" };
        _serviceMock.Setup(s => s.UpdateAsync(2, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(2, dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarEnqueteDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(2, new AtualizarEnqueteDto
        {
            Opcoes = [new AtualizarEnqueteOpcaoDto { Texto = "Opcao", Ordem = 1 }]
        });

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
