using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ComunicacaoSegmentosControllerTests
{
    private readonly Mock<IComunicacaoSegmentoService> _serviceMock = new();
    private readonly ComunicacaoSegmentosController _controller;

    public ComunicacaoSegmentosControllerTests()
    {
        _controller = new ComunicacaoSegmentosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAllAndEstimativa_ReturnOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ComunicacaoSegmentoResumoDto>());
        _serviceMock.Setup(s => s.GetEstimativaAsync("MEMBROS", 3)).ReturnsAsync(new ComunicacaoEstimativaAudienciaDto());

        (await _controller.GetAll()).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
        (await _controller.GetEstimativa("MEMBROS", 3)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync((ComunicacaoSegmentoDetalheDto?)null);
        var result = await _controller.GetById(2);
        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }

    [Fact]
    public async Task CreateAndUpdate_ReturnExpectedResponses()
    {
        var createDto = new CriarComunicacaoSegmentoDto { Nome = "Visitantes", PublicoAlvo = "VISITANTES" };
        _serviceMock.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(new ComunicacaoSegmentoDetalheDto { Id = 4, Nome = "Visitantes", PublicoAlvo = "VISITANTES" });

        var updateDto = new AtualizarComunicacaoSegmentoDto { Nome = "Visitantes 2", PublicoAlvo = "VISITANTES", Ativo = true };
        _serviceMock.Setup(s => s.UpdateAsync(4, updateDto))
            .ReturnsAsync(new ComunicacaoSegmentoDetalheDto { Id = 4, Nome = "Visitantes 2", PublicoAlvo = "VISITANTES" });

        (await _controller.Create(createDto)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.CreatedAtActionResult>();
        (await _controller.Update(4, updateDto)).Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new ComunicacaoSegmentoDetalheDto { Id = 5, Nome = "Membros", PublicoAlvo = "MEMBROS" };
        _serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(dto);

        var result = await _controller.GetById(5);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetEstimativa_ReturnsPayload()
    {
        var dto = new ComunicacaoEstimativaAudienciaDto { TotalDestinatarios = 42, PublicoAlvo = "TODOS" };
        _serviceMock.Setup(s => s.GetEstimativaAsync(null, null)).ReturnsAsync(dto);

        var result = await _controller.GetEstimativa();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenServiceThrowsArgumentException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(9, It.IsAny<AtualizarComunicacaoSegmentoDto>()))
            .ThrowsAsync(new ArgumentException("não encontrado"));

        var result = await _controller.Update(9, new AtualizarComunicacaoSegmentoDto());

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NotFoundResult>();
    }
}
