using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface INoticiaService
{
    Task<IEnumerable<NoticiaDto>> GetAllAsync();
    Task<NoticiaDto?> GetByIdAsync(int id);
    Task<IEnumerable<NoticiaDto>> GetByCategoriaAsync(int categoriaId);
    Task<NoticiaDto> CreateAsync(CriarNoticiaDto dto);
    Task<NoticiaDto> UpdateAsync(int id, AtualizarNoticiaDto dto);
    Task DeleteAsync(int id);
}

public class NoticiaService : INoticiaService
{
    private readonly INoticiaRepository _repository;
    private readonly ICategoriaNoticiaRepository _categoriaRepository;

    public NoticiaService(INoticiaRepository repository, ICategoriaNoticiaRepository categoriaRepository)
    {
        _repository = repository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<NoticiaDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<NoticiaDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<NoticiaDto>> GetByCategoriaAsync(int categoriaId)
    {
        var entities = await _repository.GetByCategoriaAsync(categoriaId);
        return entities.Select(MapToDto);
    }

    public async Task<NoticiaDto> CreateAsync(CriarNoticiaDto dto)
    {
        // Verificar se a categoria existe
        var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaNoticiaId);
        if (categoria == null) throw new ArgumentException("Categoria não encontrada");

        var entity = new Noticia
        {
            Titulo = dto.Titulo,
            Descricao = dto.Descricao,
            Texto = dto.Texto,
            Data = dto.Data,
            Url = dto.Url,
            Imagem = dto.Imagem,
            CategoriaNoticiaId = dto.CategoriaNoticiaId,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<NoticiaDto> UpdateAsync(int id, AtualizarNoticiaDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Notícia não encontrada");

        // Verificar se a categoria existe
        var categoria = await _categoriaRepository.GetByIdAsync(dto.CategoriaNoticiaId);
        if (categoria == null) throw new ArgumentException("Categoria não encontrada");

        entity.Titulo = dto.Titulo;
        entity.Descricao = dto.Descricao;
        entity.Texto = dto.Texto;
        entity.Data = dto.Data;
        entity.Url = dto.Url;
        entity.Imagem = dto.Imagem;
        entity.CategoriaNoticiaId = dto.CategoriaNoticiaId;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static NoticiaDto MapToDto(Noticia n)
    {
        return new NoticiaDto
        {
            Id = n.Id,
            Titulo = n.Titulo,
            Descricao = n.Descricao,
            Texto = n.Texto,
            Data = n.Data,
            Url = n.Url,
            Imagem = n.Imagem,
            CategoriaNoticiaId = n.CategoriaNoticiaId,
            CategoriaNoticiaNome = n.CategoriaNoticia?.Nome,
            DataCriacao = n.DataCriacao
        };
    }
}



