using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class DashboardFinanceiroServiceTests
{
    private readonly Mock<IFinanceiroQueryService> _queryServiceMock = new();
    private readonly DashboardFinanceiroService _service;

    public DashboardFinanceiroServiceTests()
    {
        _service = new DashboardFinanceiroService(_queryServiceMock.Object);
    }

    [Fact]
    public async Task GetDashboardAsync_ReturnsCalculatedDashboard()
    {
        _queryServiceMock.Setup(s => s.GetTotalReceitasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), StatusReceita.Recebida))
            .ReturnsAsync(5000);
        _queryServiceMock.Setup(s => s.GetTotalDespesasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), StatusDespesa.Paga))
            .ReturnsAsync(3000);
        _queryServiceMock.Setup(s => s.GetFluxoCaixaMensalAsync(12))
            .ReturnsAsync(new List<FluxoCaixaMensalDto> { new() { Mes = 4, Ano = 2026, MesAno = "Abr/2026", TotalReceitas = 5000, TotalDespesas = 3000, Saldo = 2000 } });
        _queryServiceMock.Setup(s => s.GetReceitasPorCategoriaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReceitaPorCategoriaDto> { new() { CategoriaId = 1, CategoriaNome = "Dizimos", Total = 5000 } });
        _queryServiceMock.Setup(s => s.GetDespesasPorCategoriaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<DespesaPorCategoriaDto> { new() { CategoriaId = 2, CategoriaNome = "Infra", Total = 3000 } });
        _queryServiceMock.Setup(s => s.GetUltimasMovimentacoesAsync(10))
            .ReturnsAsync(new List<UltimaMovimentacaoDto> { new() { Tipo = "Receita", Descricao = "Oferta", Valor = 200 } });

        var result = await _service.GetDashboardAsync();

        result.TotalReceitasMes.Should().Be(5000);
        result.TotalDespesasMes.Should().Be(3000);
        result.SaldoMes.Should().Be(2000);
        result.TotalReceitasAno.Should().Be(5000);
        result.TotalDespesasAno.Should().Be(3000);
        result.SaldoAno.Should().Be(2000);
        result.FluxoCaixaMensal.Should().HaveCount(1);
        result.ReceitasPorCategoria.Should().HaveCount(1);
        result.DespesasPorCategoria.Should().HaveCount(1);
        result.UltimasMovimentacoes.Should().HaveCount(1);
    }
}
