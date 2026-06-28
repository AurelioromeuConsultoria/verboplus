using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IConfiguracaoCampanhaAniversarioRepository
{
    Task<ConfiguracaoCampanhaAniversario> GetAsync();
    Task<ConfiguracaoCampanhaAniversario> UpdateAsync(ConfiguracaoCampanhaAniversario configuracao);
}
