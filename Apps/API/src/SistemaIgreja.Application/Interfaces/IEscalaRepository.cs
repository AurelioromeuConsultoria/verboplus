using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IEscalaRepository
{
    Task<Escala?> GetByIdAsync(int id);
    Task<Escala?> GetByEventoOcorrenciaIdAsync(int eventoOcorrenciaId);
    Task<Escala?> GetByEventoOcorrenciaAndEquipeAsync(int eventoOcorrenciaId, int equipeId);
    Task<IEnumerable<Escala>> GetAllByEventoOcorrenciaAsync(int eventoOcorrenciaId);
    Task<Escala> CreateAsync(Escala escala);
    Task<Escala> UpdateAsync(Escala escala);
    Task DeleteAsync(int id);

    Task<EscalaItem?> GetItemByIdAsync(int escalaItemId);
    Task<EscalaItem> AddItemAsync(EscalaItem item);
    Task<EscalaItem> UpdateItemAsync(EscalaItem item);
    Task DeleteItemAsync(int escalaItemId);

    Task<EscalaItem?> GetConflitoPessoaNaEscalaAsync(int escalaId, int voluntarioId, int? ignorarEscalaItemId = null);
    Task<HashSet<int>> GetPessoaIdsJaEscaladasAsync(int escalaId);
    Task<Dictionary<int, int>> GetCargaRecentePorVoluntarioAsync(int equipeId, DateTime dataMinima);
    Task<Dictionary<int, int>> GetQuantidadeEscalasNoMesPorVoluntarioAsync(int equipeId, int ano, int mes);
    Task<Dictionary<int, int>> GetQuantidadeEscalasEmPeriodoPorVoluntarioAsync(int equipeId, DateTime dataInicio, DateTime dataFim);
}
