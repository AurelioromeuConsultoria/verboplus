using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IVoluntarioRepository
{
    Task<IEnumerable<Voluntario>> GetAllAsync();
    Task<Voluntario?> GetByIdAsync(int id);
    Task<Voluntario> CreateAsync(Voluntario voluntario);
    Task<Voluntario> UpdateAsync(Voluntario voluntario);
    Task DeleteAsync(int id);
}
