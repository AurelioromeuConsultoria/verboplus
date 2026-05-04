using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IVoluntarioRepository
{
    Task<IEnumerable<Voluntario>> GetAllAsync();
    Task<IEnumerable<Voluntario>> GetByEquipeAsync(int equipeId);
    Task<IEnumerable<Voluntario>> GetByPessoaIdAsync(int pessoaId);
    Task<Voluntario?> GetByIdAsync(int id);
    Task<bool> ExistsByPessoaEquipeCargoAsync(int pessoaId, int equipeId, int cargoId, int? ignoreVoluntarioId = null);
    Task<bool> HasEscalasRelacionadasAsync(int id);
    Task<Voluntario> CreateAsync(Voluntario voluntario);
    Task<Voluntario> UpdateAsync(Voluntario voluntario);
    Task DeleteAsync(int id);
}
