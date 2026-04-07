using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IComunicacaoSegmentoRepository
{
    Task<IReadOnlyList<ComunicacaoSegmento>> GetAllAsync();
    Task<ComunicacaoSegmento?> GetByIdAsync(int id);
    Task<ComunicacaoSegmento> CreateAsync(ComunicacaoSegmento segmento);
    Task<ComunicacaoSegmento> UpdateAsync(ComunicacaoSegmento segmento);
}
