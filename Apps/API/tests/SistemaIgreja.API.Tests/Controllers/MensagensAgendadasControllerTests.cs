using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
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
    public async Task MarcarComoEnviada_ReturnsOk()
    {
        _serviceMock.Setup(s => s.MarcarComoEnviadaAsync(6)).Returns(Task.CompletedTask);
        var result = await _controller.MarcarComoEnviada(6);
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task MarcarComoErro_ReturnsOk()
    {
        _serviceMock.Setup(s => s.MarcarComoErroAsync(7, It.IsAny<string>())).Returns(Task.CompletedTask);
        var result = await _controller.MarcarComoErro(7, "erro");
        result.Should().BeOfType<OkResult>();
    }
}
