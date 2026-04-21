using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class RelatoriosFinanceirosControllerTests
{
    private readonly Mock<IRelatorioFinanceiroService> _serviceMock = new();
    private readonly RelatoriosFinanceirosController _controller;

    public RelatoriosFinanceirosControllerTests()
    {
        _controller = new RelatoriosFinanceirosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetFluxoCaixa_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetFluxoCaixaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new RelatorioFluxoCaixaDto());

        var result = await _controller.GetFluxoCaixa(DateTime.Today.AddDays(-30), DateTime.Today);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRelatorioPorCategoria_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetRelatorioPorCategoriaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new RelatorioPorCategoriaCompletoDto());

        var result = await _controller.GetRelatorioPorCategoria(DateTime.Today.AddDays(-30), DateTime.Today);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRelatorioPorCentroCusto_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetRelatorioPorCentroCustoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<RelatorioPorCentroCustoDto>());

        var result = await _controller.GetRelatorioPorCentroCusto(DateTime.Today.AddDays(-30), DateTime.Today);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRelatorioPorProjeto_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetRelatorioPorProjetoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<RelatorioPorProjetoDto>());

        var result = await _controller.GetRelatorioPorProjeto(DateTime.Today.AddDays(-30), DateTime.Today);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFluxoCaixa_ReturnsPayloadFromService()
    {
        var dto = new RelatorioFluxoCaixaDto { TotalReceitas = 300, TotalDespesas = 50, Saldo = 250 };
        _serviceMock.Setup(s => s.GetFluxoCaixaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetFluxoCaixa(DateTime.Today.AddDays(-7), DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetRelatorioPorCategoria_ReturnsPayloadFromService()
    {
        var dto = new RelatorioPorCategoriaCompletoDto();
        _serviceMock.Setup(s => s.GetRelatorioPorCategoriaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetRelatorioPorCategoria(DateTime.Today.AddDays(-7), DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetRelatorioPorCentroCusto_ReturnsPayloadFromService()
    {
        var dto = new List<RelatorioPorCentroCustoDto> { new() { CentroCusto = "Operação" } };
        _serviceMock.Setup(s => s.GetRelatorioPorCentroCustoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetRelatorioPorCentroCusto(DateTime.Today.AddDays(-7), DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetRelatorioPorProjeto_ReturnsPayloadFromService()
    {
        var dto = new List<RelatorioPorProjetoDto> { new() { Projeto = "Construção" } };
        _serviceMock.Setup(s => s.GetRelatorioPorProjetoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dto);

        var result = await _controller.GetRelatorioPorProjeto(DateTime.Today.AddDays(-7), DateTime.Today);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }
}
