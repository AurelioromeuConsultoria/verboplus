using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IHubCasaRepository
{
    Task<IEnumerable<HubCasa>> GetAllAsync();
    Task<HubCasa?> GetByIdAsync(int id);
    Task<HubCasa> CreateAsync(HubCasa casa);
    Task<HubCasa> UpdateAsync(HubCasa casa);
    Task DeleteAsync(int id);
}
