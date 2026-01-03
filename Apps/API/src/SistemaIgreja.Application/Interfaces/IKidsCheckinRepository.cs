using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IKidsCheckinRepository
{
    Task<KidsCheckin?> GetByIdAsync(int id);
    Task<KidsCheckin?> GetCheckinAtivoPorCriancaAsync(int criancaPessoaId);
    Task<KidsCheckin?> GetByCodigoSessaoAsync(string codigoSessao);
    Task<IEnumerable<KidsCheckin>> GetHistoricoPorCriancaAsync(int criancaPessoaId, int? limit = null);
    Task<IEnumerable<KidsCheckin>> GetCheckinsAtivosAsync();
    Task<KidsCheckin> CreateAsync(KidsCheckin checkin);
    Task<KidsCheckin> CreateWithoutSaveAsync(KidsCheckin checkin);
    Task<KidsCheckin> UpdateAsync(KidsCheckin checkin);
    Task UpdateWithoutSaveAsync(KidsCheckin checkin);
}


