using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IConfiguracaoPortalRepository
{
    Task<ConfiguracaoPortal?> GetAsync();
    Task<ConfiguracaoPortal> UpdateAsync(ConfiguracaoPortal configuracao);
}
