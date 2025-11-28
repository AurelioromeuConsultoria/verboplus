using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IEquipeRepository
{
    Task<IEnumerable<Equipe>> GetAllAsync();
    Task<Equipe?> GetByIdAsync(int id);
    Task<Equipe> CreateAsync(Equipe equipe);
    Task<Equipe> UpdateAsync(Equipe equipe);
    Task DeleteAsync(int id);
}
