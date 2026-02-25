using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IReceitaRepository
{
    Task<IEnumerable<Receita>> GetAllAsync();
    Task<Receita?> GetByIdAsync(int id);
    Task<Receita> CreateAsync(Receita receita);
    Task<Receita> UpdateAsync(Receita receita);
    Task DeleteAsync(int id);
}
