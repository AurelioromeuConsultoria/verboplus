using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IComunicacaoCampanhaRepository
{
    Task<(IReadOnlyList<ComunicacaoCampanha> Items, int Total)> GetPagedAsync(ComunicacaoCampanhaPagedQueryDto query);
    Task<ComunicacaoCampanha?> GetByIdAsync(int id);
    Task<ComunicacaoCampanha> CreateAsync(ComunicacaoCampanha campanha);
    Task<ComunicacaoCampanha> UpdateAsync(ComunicacaoCampanha campanha);
    Task<ComunicacaoStatsDto> GetStatsAsync();
}
