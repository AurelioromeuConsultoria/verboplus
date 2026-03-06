using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IGaleriaFotoItemRepository
{
    Task<List<GaleriaFotoItem>> GetByGaleriaIdAsync(int galeriaId);
    Task AddRangeAsync(IEnumerable<GaleriaFotoItem> items);
    Task SetDestaqueAsync(int galeriaId, string nomeArquivoDestaque);
}
