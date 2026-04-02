using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface ISolicitacaoTrocaEscalaRepository
{
    Task<SolicitacaoTrocaEscala?> GetByIdAsync(int id);
    Task<SolicitacaoTrocaEscala?> GetPendenteByEscalaItemAsync(int escalaItemId);
    Task<IEnumerable<SolicitacaoTrocaEscala>> GetGerenciaveisAsync(int usuarioId, bool isAdmin, int? equipeId, StatusSolicitacaoTrocaEscala? status);
    Task<IEnumerable<SolicitacaoTrocaEscala>> GetByEscalaAsync(int escalaId);
    Task<IEnumerable<SolicitacaoTrocaEscala>> GetByPessoaAsync(int pessoaId);
    Task<SolicitacaoTrocaEscala> CreateAsync(SolicitacaoTrocaEscala solicitacao);
    Task<SolicitacaoTrocaEscala> UpdateAsync(SolicitacaoTrocaEscala solicitacao);
}
