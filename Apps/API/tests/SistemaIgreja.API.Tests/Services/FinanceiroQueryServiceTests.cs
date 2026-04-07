using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class FinanceiroQueryServiceTests
{
    [Fact]
    public async Task GetTotalReceitasAsync_FiltersByPeriodAndStatus()
    {
        await using var context = await CreateContextAsync();
        context.Receitas.AddRange(
            new Receita
            {
                Descricao = "Oferta 1",
                Valor = 100,
                DataRecebimento = new DateTime(2026, 4, 5),
                Status = StatusReceita.Recebida
            },
            new Receita
            {
                Descricao = "Oferta 2",
                Valor = 50,
                DataRecebimento = new DateTime(2026, 4, 7),
                Status = StatusReceita.Pendente
            },
            new Receita
            {
                Descricao = "Oferta 3",
                Valor = 999,
                DataRecebimento = new DateTime(2026, 3, 30),
                Status = StatusReceita.Recebida
            });
        await context.SaveChangesAsync();

        var service = new FinanceiroQueryService(context);

        var total = await service.GetTotalReceitasAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), StatusReceita.Recebida);

        total.Should().Be(100);
    }

    [Fact]
    public async Task GetReceitasPorCategoriaAsync_GroupsAndCalculatesPercentual()
    {
        await using var context = await CreateContextAsync();
        var categoriaA = new CategoriaReceita { Nome = "Dizimos", Ativo = true };
        var categoriaB = new CategoriaReceita { Nome = "Ofertas", Ativo = true };
        context.CategoriasReceitas.AddRange(categoriaA, categoriaB);
        await context.SaveChangesAsync();

        context.Receitas.AddRange(
            new Receita
            {
                Descricao = "R1",
                Valor = 300,
                DataRecebimento = new DateTime(2026, 4, 2),
                Status = StatusReceita.Recebida,
                CategoriaReceitaId = categoriaA.Id
            },
            new Receita
            {
                Descricao = "R2",
                Valor = 100,
                DataRecebimento = new DateTime(2026, 4, 3),
                Status = StatusReceita.Recebida,
                CategoriaReceitaId = categoriaB.Id
            });
        await context.SaveChangesAsync();

        var service = new FinanceiroQueryService(context);

        var result = await service.GetReceitasPorCategoriaAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

        result.Should().HaveCount(2);
        result[0].CategoriaNome.Should().Be("Dizimos");
        result[0].Total.Should().Be(300);
        result[0].Percentual.Should().Be(75);
    }

    [Fact]
    public async Task GetMovimentacoesDiariasAsync_ReturnsSaldoAcumulado()
    {
        await using var context = await CreateContextAsync();
        context.Receitas.Add(new Receita
        {
            Descricao = "Entrada",
            Valor = 200,
            DataRecebimento = new DateTime(2026, 4, 1),
            Status = StatusReceita.Recebida
        });
        context.Despesas.Add(new Despesa
        {
            Descricao = "Saida",
            Valor = 80,
            DataVencimento = new DateTime(2026, 4, 2),
            Status = StatusDespesa.Paga
        });
        await context.SaveChangesAsync();

        var service = new FinanceiroQueryService(context);

        var result = await service.GetMovimentacoesDiariasAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

        result.Should().HaveCount(2);
        result[0].SaldoAcumulado.Should().Be(200);
        result[1].SaldoAcumulado.Should().Be(120);
    }

    [Fact]
    public async Task GetRelatorioPorCentroCustoAsync_AggregatesReceitasAndDespesas()
    {
        await using var context = await CreateContextAsync();
        var centro = new CentroCusto { Nome = "Administrativo", Ativo = true };
        context.CentrosCustos.Add(centro);
        await context.SaveChangesAsync();

        context.Receitas.Add(new Receita
        {
            Descricao = "Entrada",
            Valor = 500,
            DataRecebimento = new DateTime(2026, 4, 5),
            Status = StatusReceita.Recebida,
            CentroCustoId = centro.Id
        });
        context.Despesas.Add(new Despesa
        {
            Descricao = "Saida",
            Valor = 150,
            DataVencimento = new DateTime(2026, 4, 6),
            Status = StatusDespesa.Paga,
            CentroCustoId = centro.Id
        });
        await context.SaveChangesAsync();

        var service = new FinanceiroQueryService(context);

        var result = await service.GetRelatorioPorCentroCustoAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

        result.Should().ContainSingle();
        result[0].CentroCusto.Should().Be("Administrativo");
        result[0].TotalReceitas.Should().Be(500);
        result[0].TotalDespesas.Should().Be(150);
        result[0].Saldo.Should().Be(350);
    }

    [Fact]
    public async Task GetRelatorioPorProjetoAsync_CalculatesPercentualUtilizado()
    {
        await using var context = await CreateContextAsync();
        var projeto = new Projeto { Nome = "Reforma", Orcamento = 1000, Ativo = true };
        context.Projetos.Add(projeto);
        await context.SaveChangesAsync();

        context.Receitas.Add(new Receita
        {
            Descricao = "Entrada",
            Valor = 400,
            DataRecebimento = new DateTime(2026, 4, 8),
            Status = StatusReceita.Recebida,
            ProjetoId = projeto.Id
        });
        context.Despesas.Add(new Despesa
        {
            Descricao = "Saida",
            Valor = 250,
            DataVencimento = new DateTime(2026, 4, 9),
            Status = StatusDespesa.Paga,
            ProjetoId = projeto.Id
        });
        await context.SaveChangesAsync();

        var service = new FinanceiroQueryService(context);

        var result = await service.GetRelatorioPorProjetoAsync(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30));

        result.Should().ContainSingle();
        result[0].Projeto.Should().Be("Reforma");
        result[0].Saldo.Should().Be(150);
        result[0].PercentualUtilizado.Should().Be(25);
    }

    [Fact]
    public async Task GetUltimasMovimentacoesAsync_MergesReceitasAndDespesasOrderedByDate()
    {
        await using var context = await CreateContextAsync();
        context.Receitas.Add(new Receita
        {
            Descricao = "Receita recente",
            Valor = 100,
            DataRecebimento = new DateTime(2026, 4, 10),
            Status = StatusReceita.Recebida
        });
        context.Despesas.Add(new Despesa
        {
            Descricao = "Despesa mais recente",
            Valor = 70,
            DataVencimento = new DateTime(2026, 4, 11),
            Status = StatusDespesa.Paga
        });
        await context.SaveChangesAsync();

        var service = new FinanceiroQueryService(context);

        var result = await service.GetUltimasMovimentacoesAsync(4);

        result.Should().HaveCount(2);
        result[0].Tipo.Should().Be("Despesa");
        result[1].Tipo.Should().Be("Receita");
    }

    private static async Task<SistemaIgrejaDbContext> CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new SistemaIgrejaDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
