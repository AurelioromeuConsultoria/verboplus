using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IEquipeService
{
    Task<IEnumerable<EquipeDto>> GetAllAsync();
    Task<EquipeDto?> GetByIdAsync(int id);
    Task<EquipeDto> CreateAsync(CriarEquipeDto dto);
    Task<EquipeDto> UpdateAsync(int id, AtualizarEquipeDto dto);
    Task DeleteAsync(int id);
}

public class EquipeService : IEquipeService
{
    private readonly IEquipeRepository _repository;

    public EquipeService(IEquipeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EquipeDto>> GetAllAsync()
    {
        var equipes = await _repository.GetAllAsync();
        return equipes.Select(MapToDto);
    }

    public async Task<EquipeDto?> GetByIdAsync(int id)
    {
        var equipe = await _repository.GetByIdAsync(id);
        return equipe != null ? MapToDto(equipe) : null;
    }

    public async Task<EquipeDto> CreateAsync(CriarEquipeDto dto)
    {
        var equipe = new Equipe
        {
            Nome = dto.Nome,
            Area = (AreaEquipe)dto.Area,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(equipe);
        return MapToDto(created);
    }

    public async Task<EquipeDto> UpdateAsync(int id, AtualizarEquipeDto dto)
    {
        var equipe = await _repository.GetByIdAsync(id);
        if (equipe == null) throw new ArgumentException("Equipe não encontrada");

        equipe.Nome = dto.Nome;
        equipe.Area = (AreaEquipe)dto.Area;

        var updated = await _repository.UpdateAsync(equipe);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static EquipeDto MapToDto(Equipe e)
    {
        return new EquipeDto
        {
            Id = e.Id,
            Nome = e.Nome,
            Area = (int)e.Area,
            DataCriacao = e.DataCriacao
        };
    }
}
