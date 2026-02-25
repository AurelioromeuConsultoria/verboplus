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
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IPerfilAcessoRepository _perfilAcessoRepository;

    public UsuarioService(IUsuarioRepository repository, IPessoaRepository pessoaRepository, IPerfilAcessoRepository perfilAcessoRepository)
    {
        _repository = repository;
        _pessoaRepository = pessoaRepository;
        _perfilAcessoRepository = perfilAcessoRepository;
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
        // Verificar se EmailLogin já existe
        var existeUsuario = await _repository.GetByEmailAsync(dto.EmailLogin);
        if (existeUsuario != null) throw new ArgumentException("Email de login já cadastrado");

        Pessoa? pessoa;
        if (dto.PessoaId.HasValue)
        {
            pessoa = await _pessoaRepository.GetByIdAsync(dto.PessoaId.Value)
                ?? throw new ArgumentException("Pessoa não encontrada");
        }
        else
        {
            // Compatibilidade: sem PessoaId, tenta reaproveitar por email (se informado), senão cria nova pessoa.
            pessoa = null;
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                pessoa = await _pessoaRepository.GetByEmailAsync(dto.Email);
            }

            if (pessoa == null)
            {
                if (string.IsNullOrWhiteSpace(dto.Nome))
                {
                    throw new ArgumentException("Nome é obrigatório ao criar usuário sem pessoa vinculada");
                }

                pessoa = new Pessoa
                {
                    Nome = dto.Nome,
                    Email = dto.Email,
                    Telefone = dto.Telefone,
                    WhatsApp = dto.WhatsApp,
                    DataNascimento = dto.DataNascimento,
                    TipoPessoa = TipoPessoa.Adulto,
                    Ativo = true,
                    DataCriacao = DateTime.Now
                };
                pessoa = await _pessoaRepository.CreateAsync(pessoa);
            }
        }

        var usuarioDaPessoa = await _repository.GetByPessoaIdAsync(pessoa.Id);
        if (usuarioDaPessoa != null)
        {
            throw new ArgumentException("Esta pessoa já possui usuário");
        }

        if (dto.PerfilAcessoId == null)
            throw new ArgumentException("Perfil de acesso é obrigatório");

        var perfil = await _perfilAcessoRepository.GetByIdAsync(dto.PerfilAcessoId.Value);
        if (perfil == null) throw new ArgumentException("Perfil de acesso inválido");

        // Criar usuário
        var entity = new Usuario
        {
            PessoaId = pessoa.Id,
            EmailLogin = dto.EmailLogin,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            TipoUsuario = dto.TipoUsuario,
            PerfilAcessoId = dto.PerfilAcessoId,
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

        // Verificar se EmailLogin já existe em outro usuário
        var existeUsuario = await _repository.GetByEmailAsync(dto.EmailLogin);
        if (existeUsuario != null && existeUsuario.Id != id) throw new ArgumentException("Email de login já cadastrado");

        // Atualizar pessoa
        var pessoa = await _pessoaRepository.GetByIdAsync(entity.PessoaId);
        if (pessoa == null) throw new ArgumentException("Pessoa não encontrada");

        // Verificar se email da pessoa já existe em outra pessoa (se fornecido)
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != pessoa.Email)
        {
            var existePessoa = await _pessoaRepository.GetByEmailAsync(dto.Email);
            if (existePessoa != null && existePessoa.Id != pessoa.Id) throw new ArgumentException("Email já cadastrado para outra pessoa");
        }

        pessoa.Nome = dto.Nome;
        pessoa.Email = dto.Email;
        pessoa.Telefone = dto.Telefone;
        pessoa.WhatsApp = dto.WhatsApp;
        pessoa.DataNascimento = dto.DataNascimento;
        await _pessoaRepository.UpdateAsync(pessoa);

        if (dto.PerfilAcessoId == null)
            throw new ArgumentException("Perfil de acesso é obrigatório");

        var perfil = await _perfilAcessoRepository.GetByIdAsync(dto.PerfilAcessoId.Value);
        if (perfil == null) throw new ArgumentException("Perfil de acesso inválido");

        // Atualizar usuário
        entity.EmailLogin = dto.EmailLogin;
        entity.TipoUsuario = dto.TipoUsuario;
        entity.Ativo = dto.Ativo;
        entity.PerfilAcessoId = dto.PerfilAcessoId;

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
            PessoaId = u.PessoaId,
            Nome = u.Pessoa?.Nome ?? string.Empty,
            Email = u.Pessoa?.Email ?? string.Empty,
            EmailLogin = u.EmailLogin,
            TipoUsuario = u.TipoUsuario,
            TipoUsuarioDescricao = GetTipoUsuarioDescricao(u.TipoUsuario),
            Ativo = u.Ativo,
            DataCriacao = u.DataCriacao,
            UltimoAcesso = u.UltimoAcesso,
            PerfilAcessoId = u.PerfilAcessoId,
            PerfilAcessoNome = u.PerfilAcesso?.Nome,
            Permissoes = u.PerfilAcesso?.Permissoes.Select(p => new PermissaoPerfilDto
            {
                Id = p.Id,
                Recurso = p.Recurso,
                PodeVer = p.PodeVer,
                PodeEditar = p.PodeEditar,
                PodeExcluir = p.PodeExcluir
            }).ToList() ?? new List<PermissaoPerfilDto>()
        };
    }
}





