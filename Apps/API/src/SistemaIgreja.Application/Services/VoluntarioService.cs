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
    private readonly IPessoaRepository _pessoaRepository;

    public VoluntarioService(IVoluntarioRepository repository, IEquipeRepository equipeRepository, ICargoRepository cargoRepository, IPessoaRepository pessoaRepository)
    {
        _repository = repository;
        _equipeRepository = equipeRepository;
        _cargoRepository = cargoRepository;
        _pessoaRepository = pessoaRepository;
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

        // Verificar se pessoa já existe pelo email (se fornecido)
        Pessoa? pessoa = null;
        if (!string.IsNullOrEmpty(dto.Email))
        {
            pessoa = await _pessoaRepository.GetByEmailAsync(dto.Email);
        }

        // Criar pessoa se não existir
        if (pessoa == null)
        {
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
        else
        {
            // Atualizar dados da pessoa se necessário
            pessoa.Nome = dto.Nome;
            pessoa.Telefone = dto.Telefone ?? pessoa.Telefone;
            pessoa.WhatsApp = dto.WhatsApp ?? pessoa.WhatsApp;
            pessoa.DataNascimento = dto.DataNascimento ?? pessoa.DataNascimento;
            await _pessoaRepository.UpdateAsync(pessoa);
        }

        var entity = new Voluntario
        {
            PessoaId = pessoa.Id,
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

        // Atualizar pessoa
        var pessoa = await _pessoaRepository.GetByIdAsync(entity.PessoaId);
        if (pessoa == null) throw new ArgumentException("Pessoa não encontrada");

        // Verificar se email já existe em outra pessoa (se fornecido)
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

        // Atualizar voluntário
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
            PessoaId = v.PessoaId,
            Nome = v.Pessoa?.Nome ?? string.Empty,
            WhatsApp = v.Pessoa?.WhatsApp,
            Email = v.Pessoa?.Email,
            Telefone = v.Pessoa?.Telefone,
            EquipeId = v.EquipeId,
            NomeEquipe = v.Equipe?.Nome ?? string.Empty,
            CargoId = v.CargoId,
            NomeCargo = v.Cargo?.Nome ?? string.Empty,
            DataCadastro = v.DataCadastro
        };
    }
}
