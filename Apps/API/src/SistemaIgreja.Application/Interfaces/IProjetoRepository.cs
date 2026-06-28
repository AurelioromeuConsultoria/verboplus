using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IProjetoRepository
{
    Task<IEnumerable<Projeto>> GetAllAsync();
    Task<Projeto?> GetByIdAsync(int id);
    Task<Projeto> CreateAsync(Projeto projeto);
    Task<Projeto> UpdateAsync(Projeto projeto);
    Task DeleteAsync(int id);
}
