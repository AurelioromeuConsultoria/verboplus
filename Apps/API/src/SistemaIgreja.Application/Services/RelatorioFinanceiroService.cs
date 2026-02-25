using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IRelatorioFinanceiroService
{
    Task<RelatorioFluxoCaixaDto> GetFluxoCaixaAsync(DateTime dataInicio, DateTime dataFim);
    Task<RelatorioPorCategoriaCompletoDto> GetRelatorioPorCategoriaAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<RelatorioPorCentroCustoDto>> GetRelatorioPorCentroCustoAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<RelatorioPorProjetoDto>> GetRelatorioPorProjetoAsync(DateTime dataInicio, DateTime dataFim);
}

public class RelatorioFinanceiroService : IRelatorioFinanceiroService
{
    private readonly IFinanceiroQueryService _queryService;

    public RelatorioFinanceiroService(IFinanceiroQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<RelatorioFluxoCaixaDto> GetFluxoCaixaAsync(DateTime dataInicio, DateTime dataFim)
    {
        var movimentacoesDiarias = await _queryService.GetMovimentacoesDiariasAsync(dataInicio, dataFim);
        var totalReceitas = await _queryService.GetTotalReceitasAsync(dataInicio, dataFim, StatusReceita.Recebida);
        var totalDespesas = await _queryService.GetTotalDespesasAsync(dataInicio, dataFim, StatusDespesa.Paga);

        return new RelatorioFluxoCaixaDto
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            TotalReceitas = totalReceitas,
            TotalDespesas = totalDespesas,
            Saldo = totalReceitas - totalDespesas,
            MovimentacoesDiarias = movimentacoesDiarias
        };
    }

    public async Task<RelatorioPorCategoriaCompletoDto> GetRelatorioPorCategoriaAsync(DateTime dataInicio, DateTime dataFim)
    {
        var receitasPorCategoria = await _queryService.GetRelatorioReceitasPorCategoriaAsync(dataInicio, dataFim);
        var despesasPorCategoria = await _queryService.GetRelatorioDespesasPorCategoriaAsync(dataInicio, dataFim);
        var totalReceitas = receitasPorCategoria.Sum(r => r.Valor);
        var totalDespesas = despesasPorCategoria.Sum(d => d.Valor);

        return new RelatorioPorCategoriaCompletoDto
        {
            Receitas = receitasPorCategoria.Select(r => new RelatorioPorCategoriaDto
            {
                CategoriaId = r.CategoriaId,
                CategoriaNome = r.CategoriaNome,
                Valor = r.Valor,
                Quantidade = r.Quantidade,
                Percentual = r.Percentual
            }).ToList(),
            Despesas = despesasPorCategoria
        };
    }

    public async Task<List<RelatorioPorCentroCustoDto>> GetRelatorioPorCentroCustoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _queryService.GetRelatorioPorCentroCustoAsync(dataInicio, dataFim);
    }

    public async Task<List<RelatorioPorProjetoDto>> GetRelatorioPorProjetoAsync(DateTime dataInicio, DateTime dataFim)
    {
        return await _queryService.GetRelatorioPorProjetoAsync(dataInicio, dataFim);
    }
}
