using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IPessoaService
{
    Task<IEnumerable<PessoaDto>> GetAllAsync();
    Task<PessoaDto?> GetByIdAsync(int id);
    Task<PessoaDto> CreateAsync(CriarPessoaDto dto);
    Task<PessoaDto> UpdateAsync(int id, AtualizarPessoaDto dto);
    Task DeleteAsync(int id);
}

public class PessoaService : IPessoaService
{
    private readonly IPessoaRepository _repository;
    private readonly IPessoaPerfilRepository _perfilRepository;

    public PessoaService(IPessoaRepository repository, IPessoaPerfilRepository perfilRepository)
    {
        _repository = repository;
        _perfilRepository = perfilRepository;
    }

    public async Task<IEnumerable<PessoaDto>> GetAllAsync()
    {
        var pessoas = await _repository.GetAllAsync();
        var pessoasDto = new List<PessoaDto>();

        foreach (var pessoa in pessoas)
        {
            var perfis = await _perfilRepository.GetPerfisPorPessoaAsync(pessoa.Id);
            pessoasDto.Add(MapToDto(pessoa, perfis));
        }

        return pessoasDto;
    }

    public async Task<PessoaDto?> GetByIdAsync(int id)
    {
        var pessoa = await _repository.GetByIdAsync(id);
        if (pessoa == null) return null;

        var perfis = await _perfilRepository.GetPerfisPorPessoaAsync(pessoa.Id);
        return MapToDto(pessoa, perfis);
    }

    public async Task<PessoaDto> CreateAsync(CriarPessoaDto dto)
    {
        // Validar email único se fornecido
        if (!string.IsNullOrEmpty(dto.Email))
        {
            var existe = await _repository.GetByEmailAsync(dto.Email);
            if (existe != null) throw new ArgumentException("Email já cadastrado");
        }

        var pessoa = new Pessoa
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            WhatsApp = dto.WhatsApp,
            DataNascimento = dto.DataNascimento,
            TipoPessoa = dto.TipoPessoa,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(pessoa);
        var perfis = await _perfilRepository.GetPerfisPorPessoaAsync(created.Id);
        return MapToDto(created, perfis);
    }

    public async Task<PessoaDto> UpdateAsync(int id, AtualizarPessoaDto dto)
    {
        var pessoa = await _repository.GetByIdAsync(id);
        if (pessoa == null) throw new ArgumentException("Pessoa não encontrada");

        // Validar email único se alterado
        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != pessoa.Email)
        {
            var existe = await _repository.GetByEmailAsync(dto.Email);
            if (existe != null && existe.Id != id) throw new ArgumentException("Email já cadastrado para outra pessoa");
        }

        pessoa.Nome = dto.Nome;
        pessoa.Email = dto.Email;
        pessoa.Telefone = dto.Telefone;
        pessoa.WhatsApp = dto.WhatsApp;
        pessoa.DataNascimento = dto.DataNascimento;
        pessoa.TipoPessoa = dto.TipoPessoa;
        pessoa.Ativo = dto.Ativo;

        var updated = await _repository.UpdateAsync(pessoa);
        var perfis = await _perfilRepository.GetPerfisPorPessoaAsync(updated.Id);
        return MapToDto(updated, perfis);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static PessoaDto MapToDto(Pessoa pessoa, IEnumerable<PessoaPerfil> perfis)
    {
        return new PessoaDto
        {
            Id = pessoa.Id,
            Nome = pessoa.Nome,
            Email = pessoa.Email,
            Telefone = pessoa.Telefone,
            WhatsApp = pessoa.WhatsApp,
            DataNascimento = pessoa.DataNascimento,
            TipoPessoa = pessoa.TipoPessoa,
            TipoPessoaDescricao = GetTipoPessoaDescricao(pessoa.TipoPessoa),
            Ativo = pessoa.Ativo,
            DataCriacao = pessoa.DataCriacao,
            Perfis = perfis.Select(p => new PessoaPerfilDto
            {
                Id = p.Id,
                PessoaId = p.PessoaId,
                NomePessoa = pessoa.Nome,
                Perfil = p.Perfil,
                PerfilDescricao = GetPerfilDescricao(p.Perfil),
                DataInicio = p.DataInicio,
                DataFim = p.DataFim,
                Ativo = p.DataFim == null
            }).ToList()
        };
    }

    private static string GetTipoPessoaDescricao(TipoPessoa tipo)
    {
        return tipo switch
        {
            TipoPessoa.Adulto => "Adulto",
            TipoPessoa.Crianca => "Criança",
            _ => "Desconhecido"
        };
    }

    private static string GetPerfilDescricao(PerfilPessoa perfil)
    {
        return perfil switch
        {
            PerfilPessoa.Visitante => "Visitante",
            PerfilPessoa.Membro => "Membro",
            PerfilPessoa.Voluntario => "Voluntário",
            PerfilPessoa.Lider => "Líder",
            PerfilPessoa.Kids => "Kids",
            PerfilPessoa.Admin => "Administrador",
            _ => "Desconhecido"
        };
    }
}



