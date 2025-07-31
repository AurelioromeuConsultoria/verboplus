using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IConfiguracaoMensagemRepository
{
    Task<IEnumerable<ConfiguracaoMensagem>> GetAllAsync();
    Task<ConfiguracaoMensagem?> GetByIdAsync(int id);
    Task<ConfiguracaoMensagem> CreateAsync(ConfiguracaoMensagem configuracao);
    Task<ConfiguracaoMensagem> UpdateAsync(ConfiguracaoMensagem configuracao);
    Task DeleteAsync(int id);
    Task<IEnumerable<ConfiguracaoMensagem>> GetAtivasAsync();
}

