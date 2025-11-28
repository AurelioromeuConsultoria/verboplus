using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IEventoService
{
    Task<IEnumerable<EventoDto>> GetAllAsync();
    Task<EventoDto?> GetByIdAsync(int id);
    Task<IEnumerable<EventoDto>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
    Task<EventoDto> CreateAsync(CriarEventoDto dto);
    Task<EventoDto> UpdateAsync(int id, AtualizarEventoDto dto);
    Task DeleteAsync(int id);
}

public class EventoService : IEventoService
{
    private readonly IEventoRepository _repository;

    public EventoService(IEventoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EventoDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<EventoDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<EventoDto>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
    {
        var entities = await _repository.GetByPeriodoAsync(dataInicio, dataFim);
        return entities.Select(MapToDto);
    }

    public async Task<EventoDto> CreateAsync(CriarEventoDto dto)
    {
        var entity = new Evento
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            ImagemDestaque = dto.ImagemDestaque,
            Url = dto.Url,
            DataInicio = dto.DataInicio,
            DataFim = dto.DataFim,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<EventoDto> UpdateAsync(int id, AtualizarEventoDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Evento não encontrado");

        entity.Titulo = dto.Titulo;
        entity.Descricao = dto.Descricao;
        entity.ImagemDestaque = dto.ImagemDestaque;
        entity.Url = dto.Url;
        entity.DataInicio = dto.DataInicio;
        entity.DataFim = dto.DataFim;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static EventoDto MapToDto(Evento e)
    {
        return new EventoDto
        {
            Id = e.Id,
            Titulo = e.Titulo,
            Descricao = e.Descricao,
            ImagemDestaque = e.ImagemDestaque,
            Url = e.Url,
            DataInicio = e.DataInicio,
            DataFim = e.DataFim,
            DataCriacao = e.DataCriacao
        };
    }
}



