using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IEventoRecorrenciaRepository
{
    Task<IEnumerable<EventoRecorrencia>> GetByEventoAsync(int eventoId);
    Task<EventoRecorrencia?> GetByIdAsync(int id);
    Task<EventoRecorrencia> CreateAsync(EventoRecorrencia recorrencia);
    Task<EventoRecorrencia> UpdateAsync(EventoRecorrencia recorrencia);
    Task DeleteAsync(int id);
}
