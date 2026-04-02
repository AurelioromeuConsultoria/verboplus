using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IPatrimonioItemRepository
{
    Task<IEnumerable<PatrimonioItem>> GetAllAsync();
    Task<PatrimonioItem?> GetByIdAsync(int id);
    Task<PatrimonioItem?> GetByCodigoAsync(string codigo);
    Task<PatrimonioItem> CreateAsync(PatrimonioItem item);
    Task<PatrimonioItem> UpdateAsync(PatrimonioItem item);
    Task DeleteAsync(int id);
}
