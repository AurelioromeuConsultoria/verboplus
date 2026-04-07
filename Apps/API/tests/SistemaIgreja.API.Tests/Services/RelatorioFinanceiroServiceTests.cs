using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class RelatorioFinanceiroServiceTests
{
    private readonly Mock<IFinanceiroQueryService> _queryServiceMock = new();
    private readonly RelatorioFinanceiroService _service;

    public RelatorioFinanceiroServiceTests()
    {
        _service = new RelatorioFinanceiroService(_queryServiceMock.Object);
    }

    [Fact]
    public async Task GetFluxoCaixaAsync_CalculatesSaldo()
    {
        var inicio = new DateTime(2026, 4, 1);
        var fim = new DateTime(2026, 4, 30);
        _queryServiceMock.Setup(s => s.GetMovimentacoesDiariasAsync(inicio, fim))
            .ReturnsAsync(new List<MovimentacaoDiariaDto> { new() { Data = inicio, Receitas = 100, Despesas = 40, SaldoDia = 60 } });
        _queryServiceMock.Setup(s => s.GetTotalReceitasAsync(inicio, fim, StatusReceita.Recebida)).ReturnsAsync(1000);
        _queryServiceMock.Setup(s => s.GetTotalDespesasAsync(inicio, fim, StatusDespesa.Paga)).ReturnsAsync(400);

        var result = await _service.GetFluxoCaixaAsync(inicio, fim);

        result.TotalReceitas.Should().Be(1000);
        result.TotalDespesas.Should().Be(400);
        result.Saldo.Should().Be(600);
        result.MovimentacoesDiarias.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRelatorioPorCategoriaAsync_ReturnsReceitasAndDespesas()
    {
        var inicio = new DateTime(2026, 4, 1);
        var fim = new DateTime(2026, 4, 30);
        _queryServiceMock.Setup(s => s.GetRelatorioReceitasPorCategoriaAsync(inicio, fim))
            .ReturnsAsync(new List<RelatorioPorCategoriaDto>
            {
                new() { CategoriaId = 1, CategoriaNome = "Dizimos", Valor = 500, Quantidade = 2, Percentual = 50 }
            });
        _queryServiceMock.Setup(s => s.GetRelatorioDespesasPorCategoriaAsync(inicio, fim))
            .ReturnsAsync(new List<RelatorioPorCategoriaDto>
            {
                new() { CategoriaId = 2, CategoriaNome = "Infra", Valor = 200, Quantidade = 1, Percentual = 100 }
            });

        var result = await _service.GetRelatorioPorCategoriaAsync(inicio, fim);

        result.Receitas.Should().HaveCount(1);
        result.Despesas.Should().HaveCount(1);
        result.Receitas[0].CategoriaNome.Should().Be("Dizimos");
    }
}
