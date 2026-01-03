using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface ICriancaDetalheRepository
{
    Task<CriancaDetalhe?> GetByPessoaIdAsync(int pessoaId);
    Task<CriancaDetalhe> CreateAsync(CriancaDetalhe detalhe);
    Task<CriancaDetalhe> CreateWithoutSaveAsync(CriancaDetalhe detalhe);
    Task<CriancaDetalhe> UpdateAsync(CriancaDetalhe detalhe);
    Task DeleteAsync(int pessoaId);
}


