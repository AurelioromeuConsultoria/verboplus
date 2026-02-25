using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IDespesaService
{
    Task<IEnumerable<DespesaDto>> GetAllAsync();
    Task<DespesaDto?> GetByIdAsync(int id);
    Task<DespesaDto> CreateAsync(CriarDespesaDto dto);
    Task<DespesaDto> UpdateAsync(int id, AtualizarDespesaDto dto);
    Task DeleteAsync(int id);
}

public class DespesaService : IDespesaService
{
    private readonly IDespesaRepository _repository;

    public DespesaService(IDespesaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DespesaDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<DespesaDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<DespesaDto> CreateAsync(CriarDespesaDto dto)
    {
        var entity = new Despesa
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            DataVencimento = dto.DataVencimento,
            DataPagamento = dto.DataPagamento,
            Status = dto.Status,
            Observacoes = dto.Observacoes,
            ComprovanteUrl = dto.ComprovanteUrl,
            FornecedorId = dto.FornecedorId,
            CategoriaDespesaId = dto.CategoriaDespesaId,
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

    public async Task<DespesaDto> UpdateAsync(int id, AtualizarDespesaDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Despesa não encontrada");

        entity.Descricao = dto.Descricao;
        entity.Valor = dto.Valor;
        entity.DataVencimento = dto.DataVencimento;
        entity.DataPagamento = dto.DataPagamento;
        entity.Status = dto.Status;
        entity.Observacoes = dto.Observacoes;
        entity.ComprovanteUrl = dto.ComprovanteUrl;
        entity.FornecedorId = dto.FornecedorId;
        entity.CategoriaDespesaId = dto.CategoriaDespesaId;
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

    private static DespesaDto MapToDto(Despesa d)
    {
        return new DespesaDto
        {
            Id = d.Id,
            Descricao = d.Descricao,
            Valor = d.Valor,
            DataVencimento = d.DataVencimento,
            DataPagamento = d.DataPagamento,
            Status = d.Status,
            StatusDescricao = GetStatusDescricao(d.Status),
            Observacoes = d.Observacoes,
            ComprovanteUrl = d.ComprovanteUrl,
            FornecedorId = d.FornecedorId,
            FornecedorNome = d.Fornecedor?.Nome,
            CategoriaDespesaId = d.CategoriaDespesaId,
            CategoriaDespesaNome = d.CategoriaDespesa?.Nome,
            ContaBancariaId = d.ContaBancariaId,
            ContaBancariaNome = d.ContaBancaria?.Nome,
            CentroCustoId = d.CentroCustoId,
            CentroCustoNome = d.CentroCusto?.Nome,
            ProjetoId = d.ProjetoId,
            ProjetoNome = d.Projeto?.Nome,
            UsuarioId = d.UsuarioId,
            UsuarioNome = d.Usuario?.Pessoa?.Nome,
            DataCriacao = d.DataCriacao,
        };
    }

    private static string GetStatusDescricao(StatusDespesa status)
    {
        return status switch
        {
            StatusDespesa.Pendente => "Pendente",
            StatusDespesa.Paga => "Paga",
            StatusDespesa.Cancelada => "Cancelada",
            _ => status.ToString()
        };
    }
}
