using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IContatoRepository
{
    Task<IEnumerable<Contato>> GetAllAsync();
    Task<Contato?> GetByIdAsync(int id);
    Task<Contato> CreateAsync(Contato contato);
    Task<Contato> UpdateAsync(Contato contato);
    Task DeleteAsync(int id);
}



