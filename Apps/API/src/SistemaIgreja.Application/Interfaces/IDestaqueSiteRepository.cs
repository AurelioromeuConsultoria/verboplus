using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IDestaqueSiteRepository
{
    Task<IEnumerable<DestaqueSite>> GetAllAsync();
    Task<DestaqueSite?> GetByIdAsync(int id);
    Task<DestaqueSite> CreateAsync(DestaqueSite destaqueSite);
    Task<DestaqueSite> UpdateAsync(DestaqueSite destaqueSite);
    Task DeleteAsync(int id);
}



