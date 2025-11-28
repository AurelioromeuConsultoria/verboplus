using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IVoluntarioService
{
    Task<IEnumerable<VoluntarioDto>> GetAllAsync();
    Task<VoluntarioDto?> GetByIdAsync(int id);
    Task<VoluntarioDto> CreateAsync(CriarVoluntarioDto dto);
    Task<VoluntarioDto> UpdateAsync(int id, AtualizarVoluntarioDto dto);
    Task DeleteAsync(int id);
}

public class VoluntarioService : IVoluntarioService
{
    private readonly IVoluntarioRepository _repository;
    private readonly IEquipeRepository _equipeRepository;
    private readonly ICargoRepository _cargoRepository;

    public VoluntarioService(IVoluntarioRepository repository, IEquipeRepository equipeRepository, ICargoRepository cargoRepository)
    {
        _repository = repository;
        _equipeRepository = equipeRepository;
        _cargoRepository = cargoRepository;
    }

    public async Task<IEnumerable<VoluntarioDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<VoluntarioDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<VoluntarioDto> CreateAsync(CriarVoluntarioDto dto)
    {
        // Garantir referencias válidas
        var equipe = await _equipeRepository.GetByIdAsync(dto.EquipeId) ?? throw new ArgumentException("Equipe inválida");
        var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId) ?? throw new ArgumentException("Cargo inválido");

        var entity = new Voluntario
        {
            Nome = dto.Nome,
            WhatsApp = dto.WhatsApp,
            Email = dto.Email,
            EquipeId = dto.EquipeId,
            CargoId = dto.CargoId,
            DataCadastro = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        // Recarregar para obter navegações
        var loaded = await _repository.GetByIdAsync(created.Id) ?? created;
        loaded.Equipe = equipe;
        loaded.Cargo = cargo;
        return MapToDto(loaded);
    }

    public async Task<VoluntarioDto> UpdateAsync(int id, AtualizarVoluntarioDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Voluntário não encontrado");

        // Validar novas referencias
        var equipe = await _equipeRepository.GetByIdAsync(dto.EquipeId) ?? throw new ArgumentException("Equipe inválida");
        var cargo = await _cargoRepository.GetByIdAsync(dto.CargoId) ?? throw new ArgumentException("Cargo inválido");

        entity.Nome = dto.Nome;
        entity.WhatsApp = dto.WhatsApp;
        entity.Email = dto.Email;
        entity.EquipeId = dto.EquipeId;
        entity.CargoId = dto.CargoId;

        var updated = await _repository.UpdateAsync(entity);
        updated.Equipe = equipe;
        updated.Cargo = cargo;
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static VoluntarioDto MapToDto(Voluntario v)
    {
        return new VoluntarioDto
        {
            Id = v.Id,
            Nome = v.Nome,
            WhatsApp = v.WhatsApp,
            Email = v.Email,
            EquipeId = v.EquipeId,
            NomeEquipe = v.Equipe?.Nome ?? string.Empty,
            CargoId = v.CargoId,
            NomeCargo = v.Cargo?.Nome ?? string.Empty,
            DataCadastro = v.DataCadastro
        };
    }
}
