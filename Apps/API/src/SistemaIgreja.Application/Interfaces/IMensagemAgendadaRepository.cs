using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IMensagemAgendadaRepository
{
    Task<IEnumerable<MensagemAgendada>> GetAllAsync();
    Task<MensagemAgendada?> GetByIdAsync(int id);
    Task<MensagemAgendada> CreateAsync(MensagemAgendada mensagem);
    Task<MensagemAgendada> UpdateAsync(MensagemAgendada mensagem);
    Task DeleteAsync(int id);
    Task<IEnumerable<MensagemAgendada>> GetMensagensProntasParaEnvioAsync();
    Task<IEnumerable<MensagemAgendada>> GetMensagensPorVisitanteAsync(int visitanteId);
    Task<IEnumerable<MensagemAgendada>> GetMensagensPorStatusAsync(StatusMensagem status);
}

