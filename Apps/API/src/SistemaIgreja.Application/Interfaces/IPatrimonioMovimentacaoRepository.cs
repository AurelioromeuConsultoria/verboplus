using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IPatrimonioMovimentacaoRepository
{
    Task<IEnumerable<PatrimonioMovimentacao>> GetByPatrimonioIdAsync(int patrimonioItemId);
    Task<PatrimonioMovimentacao> CreateAsync(PatrimonioMovimentacao movimentacao);
}
