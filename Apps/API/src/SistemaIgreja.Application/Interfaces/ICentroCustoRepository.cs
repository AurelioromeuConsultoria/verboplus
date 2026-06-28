using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface ICentroCustoRepository
{
    Task<IEnumerable<CentroCusto>> GetAllAsync();
    Task<CentroCusto?> GetByIdAsync(int id);
    Task<CentroCusto> CreateAsync(CentroCusto centroCusto);
    Task<CentroCusto> UpdateAsync(CentroCusto centroCusto);
    Task DeleteAsync(int id);
}
