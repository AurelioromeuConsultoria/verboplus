using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IComunicacaoPreferenciaRepository
{
    Task<IReadOnlyList<ComunicacaoPreferencia>> GetByPessoaIdAsync(int pessoaId);
    Task<ComunicacaoPreferencia?> GetByPessoaCanalAsync(int pessoaId, CanalComunicacao canal);
    Task<ComunicacaoPreferencia> CreateAsync(ComunicacaoPreferencia preferencia);
    Task<ComunicacaoPreferencia> UpdateAsync(ComunicacaoPreferencia preferencia);
}
