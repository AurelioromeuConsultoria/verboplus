using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IVisitanteRepository
{
    Task<IEnumerable<Visitante>> GetAllAsync();
    Task<Visitante?> GetByIdAsync(int id);
    Task<Visitante> CreateAsync(Visitante visitante);
    Task<Visitante> CreateWithoutSaveAsync(Visitante visitante); // Para uso em transações
    Task<Visitante> UpdateAsync(Visitante visitante);
    Task DeleteAsync(int id);
    Task<IEnumerable<Visitante>> GetVisitantesPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<IEnumerable<Visitante>> GetVisitantesPorPessoaAsync(int pessoaId);
}

