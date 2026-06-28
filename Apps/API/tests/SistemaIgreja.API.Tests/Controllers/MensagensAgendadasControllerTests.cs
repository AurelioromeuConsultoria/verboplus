using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.MensagensAgendadas;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class MensagensAgendadasControllerTests
{
    private readonly Mock<IMensagemAgendadaService> _serviceMock = new();
    private readonly MensagensAgendadasController _controller;

    public MensagensAgendadasControllerTests()
    {
        _controller = new MensagensAgendadasController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<MensagemAgendadaDto>());
        var result = await _controller.GetAll();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_WhenNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((MensagemAgendadaDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPaged_ReturnsOk()
    {
        var query = new MensagemAgendadaPagedQueryDto { Page = 1, PageSize = 10, Texto = "visita" };
        _serviceMock.Setup(s => s.GetPagedAsync(query)).ReturnsAsync(new PagedResultDto<MensagemAgendadaDto>
        {
            Items = [new MensagemAgendadaDto { Id = 1, TextoFinal = "Olá" }],
            Total = 1,
            Page = 1,
            PageSize = 10
        });

        var result = await _controller.GetPaged(query);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStats_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetStatsAsync()).ReturnsAsync(new MensagemAgendadaStatsDto
        {
            Total = 5,
            Agendadas = 2,
            Enviadas = 2,
            Erro = 1
        });

        var result = await _controller.GetStats();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetProntasParaEnvio_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetMensagensProntasParaEnvioAsync()).ReturnsAsync(new List<MensagemAgendadaDto>());
        var result = await _controller.GetProntasParaEnvio();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPorVisitante_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetMensagensPorVisitanteAsync(10)).ReturnsAsync(new List<MensagemAgendadaDto>());
        var result = await _controller.GetPorVisitante(10);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task MarcarComoPronta_ReturnsOk()
    {
        _serviceMock.Setup(s => s.MarcarComoProntaParaEnvioAsync(5)).Returns(Task.CompletedTask);
        var result = await _controller.MarcarComoPronta(5);
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarcarComoPronta_ReturnsNotFound_OnArgumentException()
    {
        _serviceMock.Setup(s => s.MarcarComoProntaParaEnvioAsync(5)).ThrowsAsync(new ArgumentException());

        var result = await _controller.MarcarComoPronta(5);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MarcarComoEnviada_ReturnsOk()
    {
        _serviceMock.Setup(s => s.MarcarComoEnviadaAsync(6)).Returns(Task.CompletedTask);
        var result = await _controller.MarcarComoEnviada(6);
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarcarComoEnviada_ReturnsNotFound_OnArgumentException()
    {
        _serviceMock.Setup(s => s.MarcarComoEnviadaAsync(6)).ThrowsAsync(new ArgumentException());

        var result = await _controller.MarcarComoEnviada(6);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task MarcarComoErro_ReturnsOk()
    {
        _serviceMock.Setup(s => s.MarcarComoErroAsync(7, It.IsAny<string>())).Returns(Task.CompletedTask);
        var result = await _controller.MarcarComoErro(7, "erro");
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarcarComoErro_ReturnsNotFound_OnArgumentException()
    {
        _serviceMock.Setup(s => s.MarcarComoErroAsync(7, It.IsAny<string>())).ThrowsAsync(new ArgumentException());

        var result = await _controller.MarcarComoErro(7, "erro");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetAll_ReturnsPayloadFromService()
    {
        var items = new List<MensagemAgendadaDto> { new() { Id = 2, TextoFinal = "Mensagem" } };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

        var result = await _controller.GetAll();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(items);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenMensagemExists()
    {
        var dto = new MensagemAgendadaDto { Id = 3, TextoFinal = "Olá" };
        _serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync(dto);

        var result = await _controller.GetById(3);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetPaged_ReturnsPayloadFromService()
    {
        var query = new MensagemAgendadaPagedQueryDto();
        var dto = new PagedResultDto<MensagemAgendadaDto> { Items = [new MensagemAgendadaDto { Id = 4 }], Total = 1, Page = 1, PageSize = 20 };
        _serviceMock.Setup(s => s.GetPagedAsync(query)).ReturnsAsync(dto);

        var result = await _controller.GetPaged(query);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetStats_ReturnsPayloadFromService()
    {
        var dto = new MensagemAgendadaStatsDto { Total = 8 };
        _serviceMock.Setup(s => s.GetStatsAsync()).ReturnsAsync(dto);

        var result = await _controller.GetStats();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task MarcarComoPronta_ReturnsBadRequest_OnGenericException()
    {
        _serviceMock.Setup(s => s.MarcarComoProntaParaEnvioAsync(5)).ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.MarcarComoPronta(5);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MarcarComoEnviada_ReturnsBadRequest_OnGenericException()
    {
        _serviceMock.Setup(s => s.MarcarComoEnviadaAsync(6)).ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.MarcarComoEnviada(6);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MarcarComoErro_ReturnsBadRequest_OnGenericException()
    {
        _serviceMock.Setup(s => s.MarcarComoErroAsync(7, It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.MarcarComoErro(7, "erro");

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
