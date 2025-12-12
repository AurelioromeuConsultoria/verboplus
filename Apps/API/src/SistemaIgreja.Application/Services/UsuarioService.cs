using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto?> GetByIdAsync(int id);
    Task<UsuarioDto> CreateAsync(CriarUsuarioDto dto);
    Task<UsuarioDto> UpdateAsync(int id, AtualizarUsuarioDto dto);
    Task DeleteAsync(int id);
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<UsuarioDto> CreateAsync(CriarUsuarioDto dto)
    {
        // Verificar se email já existe
        var existe = await _repository.GetByEmailAsync(dto.Email);
        if (existe != null) throw new ArgumentException("Email já cadastrado");

        var entity = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            TipoUsuario = dto.TipoUsuario,
            Ativo = true,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<UsuarioDto> UpdateAsync(int id, AtualizarUsuarioDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Usuário não encontrado");

        // Verificar se email já existe em outro usuário
        var existe = await _repository.GetByEmailAsync(dto.Email);
        if (existe != null && existe.Id != id) throw new ArgumentException("Email já cadastrado");

        entity.Nome = dto.Nome;
        entity.Email = dto.Email;
        entity.TipoUsuario = dto.TipoUsuario;
        entity.Ativo = dto.Ativo;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static string GetTipoUsuarioDescricao(TipoUsuario tipo)
    {
        return tipo switch
        {
            TipoUsuario.Admin => "Administrador",
            TipoUsuario.Portal => "Portal",
            TipoUsuario.Ambos => "Administrador e Portal",
            _ => "Desconhecido"
        };
    }

    private static UsuarioDto MapToDto(Usuario u)
    {
        return new UsuarioDto
        {
            Id = u.Id,
            Nome = u.Nome,
            Email = u.Email,
            TipoUsuario = u.TipoUsuario,
            TipoUsuarioDescricao = GetTipoUsuarioDescricao(u.TipoUsuario),
            Ativo = u.Ativo,
            DataCriacao = u.DataCriacao,
            UltimoAcesso = u.UltimoAcesso
        };
    }
}



