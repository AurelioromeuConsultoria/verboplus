using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IKidsNotificacaoRepository
{
    Task<IEnumerable<KidsNotificacao>> GetByCriancaIdAsync(int criancaPessoaId);
    Task<IEnumerable<KidsNotificacao>> GetByResponsavelIdAsync(int responsavelPessoaId);
    Task<IEnumerable<KidsNotificacao>> GetPendentesAsync();
    Task<KidsNotificacao> CreateAsync(KidsNotificacao notificacao);
    Task<KidsNotificacao> CreateWithoutSaveAsync(KidsNotificacao notificacao);
    Task<KidsNotificacao> UpdateAsync(KidsNotificacao notificacao);
}


