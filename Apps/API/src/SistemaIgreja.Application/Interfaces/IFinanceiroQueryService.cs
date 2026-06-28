using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IFinanceiroQueryService
{
    Task<decimal> GetTotalReceitasAsync(DateTime dataInicio, DateTime dataFim, StatusReceita? status = null);
    Task<decimal> GetTotalDespesasAsync(DateTime dataInicio, DateTime dataFim, StatusDespesa? status = null);
    Task<List<FluxoCaixaMensalDto>> GetFluxoCaixaMensalAsync(int meses);
    Task<List<ReceitaPorCategoriaDto>> GetReceitasPorCategoriaAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<DespesaPorCategoriaDto>> GetDespesasPorCategoriaAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<UltimaMovimentacaoDto>> GetUltimasMovimentacoesAsync(int quantidade);
    Task<List<MovimentacaoDiariaDto>> GetMovimentacoesDiariasAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<RelatorioPorCategoriaDto>> GetRelatorioReceitasPorCategoriaAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<RelatorioPorCategoriaDto>> GetRelatorioDespesasPorCategoriaAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<RelatorioPorCentroCustoDto>> GetRelatorioPorCentroCustoAsync(DateTime dataInicio, DateTime dataFim);
    Task<List<RelatorioPorProjetoDto>> GetRelatorioPorProjetoAsync(DateTime dataInicio, DateTime dataFim);
}
