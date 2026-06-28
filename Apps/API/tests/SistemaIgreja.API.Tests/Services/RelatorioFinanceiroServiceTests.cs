using FluentAssertions;
using Moq;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class RelatorioFinanceiroServiceTests
{
    private readonly Mock<IReceitaRepository> _receitaRepositoryMock = new();
    private readonly Mock<IDespesaRepository> _despesaRepositoryMock = new();
    private readonly RelatorioFinanceiroService _service;

    public RelatorioFinanceiroServiceTests()
    {
        _service = new RelatorioFinanceiroService(_receitaRepositoryMock.Object, _despesaRepositoryMock.Object);
    }

    [Fact]
    public async Task GetFluxoCaixaAsync_CalculatesSaldo()
    {
        var inicio = new DateTime(2026, 4, 1);
        var fim = new DateTime(2026, 4, 30);

        _receitaRepositoryMock
            .Setup(r => r.GetPorPeriodoAsync(inicio, fim))
            .ReturnsAsync([new Receita { Valor = 1000m, DataRecebimento = inicio, Status = StatusReceita.Recebida }]);

        _despesaRepositoryMock
            .Setup(r => r.GetPorPeriodoAsync(inicio, fim))
            .ReturnsAsync([new Despesa { Valor = 400m, DataVencimento = inicio, Status = StatusDespesa.Paga }]);

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
        var catReceita = new CategoriaReceita { Id = 1, Nome = "Dizimos" };
        var catDespesa = new CategoriaDespesa { Id = 2, Nome = "Infra" };

        _receitaRepositoryMock
            .Setup(r => r.GetPorPeriodoAsync(inicio, fim))
            .ReturnsAsync([new Receita { Valor = 500m, DataRecebimento = inicio, CategoriaReceitaId = 1, CategoriaReceita = catReceita }]);

        _despesaRepositoryMock
            .Setup(r => r.GetPorPeriodoAsync(inicio, fim))
            .ReturnsAsync([new Despesa { Valor = 200m, DataVencimento = inicio, CategoriaDespesaId = 2, CategoriaDespesa = catDespesa }]);

        var result = await _service.GetRelatorioPorCategoriaAsync(inicio, fim);

        result.Receitas.Should().HaveCount(1);
        result.Despesas.Should().HaveCount(1);
        result.Receitas[0].CategoriaNome.Should().Be("Dizimos");
    }
}
