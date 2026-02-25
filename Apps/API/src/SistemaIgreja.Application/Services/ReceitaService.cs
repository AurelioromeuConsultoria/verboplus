using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IReceitaService
{
    Task<IEnumerable<ReceitaDto>> GetAllAsync();
    Task<ReceitaDto?> GetByIdAsync(int id);
    Task<ReceitaDto> CreateAsync(CriarReceitaDto dto);
    Task<ReceitaDto> UpdateAsync(int id, AtualizarReceitaDto dto);
    Task DeleteAsync(int id);
}

public class ReceitaService : IReceitaService
{
    private readonly IReceitaRepository _repository;

    public ReceitaService(IReceitaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ReceitaDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<ReceitaDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<ReceitaDto> CreateAsync(CriarReceitaDto dto)
    {
        var entity = new Receita
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            DataRecebimento = dto.DataRecebimento,
            DataConfirmacao = dto.DataConfirmacao,
            Status = dto.Status,
            Observacoes = dto.Observacoes,
            ComprovanteUrl = dto.ComprovanteUrl,
            CategoriaReceitaId = dto.CategoriaReceitaId,
            ContaBancariaId = dto.ContaBancariaId,
            CentroCustoId = dto.CentroCustoId,
            ProjetoId = dto.ProjetoId,
            UsuarioId = dto.UsuarioId,
            DataCriacao = DateTime.Now,
        };

        var created = await _repository.CreateAsync(entity);
        // Recarregar com relacionamentos para o DTO
        var createdWithRelations = await _repository.GetByIdAsync(created.Id);
        return MapToDto(createdWithRelations!);
    }

    public async Task<ReceitaDto> UpdateAsync(int id, AtualizarReceitaDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Receita não encontrada");

        entity.Descricao = dto.Descricao;
        entity.Valor = dto.Valor;
        entity.DataRecebimento = dto.DataRecebimento;
        entity.DataConfirmacao = dto.DataConfirmacao;
        entity.Status = dto.Status;
        entity.Observacoes = dto.Observacoes;
        entity.ComprovanteUrl = dto.ComprovanteUrl;
        entity.CategoriaReceitaId = dto.CategoriaReceitaId;
        entity.ContaBancariaId = dto.ContaBancariaId;
        entity.CentroCustoId = dto.CentroCustoId;
        entity.ProjetoId = dto.ProjetoId;
        entity.UsuarioId = dto.UsuarioId;

        var updated = await _repository.UpdateAsync(entity);
        // Recarregar com relacionamentos para o DTO
        var updatedWithRelations = await _repository.GetByIdAsync(updated.Id);
        return MapToDto(updatedWithRelations!);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private static ReceitaDto MapToDto(Receita r)
    {
        return new ReceitaDto
        {
            Id = r.Id,
            Descricao = r.Descricao,
            Valor = r.Valor,
            DataRecebimento = r.DataRecebimento,
            DataConfirmacao = r.DataConfirmacao,
            Status = r.Status,
            StatusDescricao = GetStatusDescricao(r.Status),
            Observacoes = r.Observacoes,
            ComprovanteUrl = r.ComprovanteUrl,
            CategoriaReceitaId = r.CategoriaReceitaId,
            CategoriaReceitaNome = r.CategoriaReceita?.Nome,
            ContaBancariaId = r.ContaBancariaId,
            ContaBancariaNome = r.ContaBancaria?.Nome,
            CentroCustoId = r.CentroCustoId,
            CentroCustoNome = r.CentroCusto?.Nome,
            ProjetoId = r.ProjetoId,
            ProjetoNome = r.Projeto?.Nome,
            UsuarioId = r.UsuarioId,
            UsuarioNome = r.Usuario?.Pessoa?.Nome,
            DataCriacao = r.DataCriacao,
        };
    }

    private static string GetStatusDescricao(StatusReceita status)
    {
        return status switch
        {
            StatusReceita.Pendente => "Pendente",
            StatusReceita.Recebida => "Recebida",
            StatusReceita.Cancelada => "Cancelada",
            _ => status.ToString()
        };
    }
}
