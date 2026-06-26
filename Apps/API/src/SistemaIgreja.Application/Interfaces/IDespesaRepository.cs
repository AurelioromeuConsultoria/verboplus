using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IDespesaRepository
{
    Task<IEnumerable<Despesa>> GetAllAsync();
    Task<IEnumerable<Despesa>> GetPendentesAteDataAsync(DateTime ate);
    Task<IEnumerable<Despesa>> GetPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<Despesa?> GetByIdAsync(int id);
    Task<Despesa> CreateAsync(Despesa despesa);
    Task<Despesa> UpdateAsync(Despesa despesa);
    Task DeleteAsync(int id);
}
