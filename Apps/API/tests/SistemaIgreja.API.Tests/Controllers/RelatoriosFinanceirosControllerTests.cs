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
}
