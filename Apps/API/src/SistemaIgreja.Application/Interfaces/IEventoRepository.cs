using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IEventoRepository
{
    Task<IEnumerable<Evento>> GetAllAsync();
    Task<Evento?> GetByIdAsync(int id);
    Task<IEnumerable<Evento>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<Evento> CreateAsync(Evento evento);
    Task<Evento> UpdateAsync(Evento evento);
    Task DeleteAsync(int id);
}



