using System.ComponentModel.DataAnnotations;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

public class DespesaDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusDespesa Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public string? ComprovanteUrl { get; set; }
    public int? FornecedorId { get; set; }
    public string? FornecedorNome { get; set; }
    public int? CategoriaDespesaId { get; set; }
    public string? CategoriaDespesaNome { get; set; }
    public int? ContaBancariaId { get; set; }
    public string? ContaBancariaNome { get; set; }
    public int? CentroCustoId { get; set; }
    public string? CentroCustoNome { get; set; }
    public int? ProjetoId { get; set; }
    public string? ProjetoNome { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public DateTime DataCriacao { get; set; }
}

public class CriarDespesaDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Data de vencimento é obrigatória")]
    public DateTime DataVencimento { get; set; }

    public DateTime? DataPagamento { get; set; }

    public StatusDespesa Status { get; set; } = StatusDespesa.Pendente;

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? ComprovanteUrl { get; set; }

    public int? FornecedorId { get; set; }

    public int? CategoriaDespesaId { get; set; }

    public int? ContaBancariaId { get; set; }

    public int? CentroCustoId { get; set; }

    public int? ProjetoId { get; set; }

    public int? UsuarioId { get; set; }
}

public class AtualizarDespesaDto
{
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "Data de vencimento é obrigatória")]
    public DateTime DataVencimento { get; set; }

    public DateTime? DataPagamento { get; set; }

    public StatusDespesa Status { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? ComprovanteUrl { get; set; }

    public int? FornecedorId { get; set; }

    public int? CategoriaDespesaId { get; set; }

    public int? ContaBancariaId { get; set; }

    public int? CentroCustoId { get; set; }

    public int? ProjetoId { get; set; }

    public int? UsuarioId { get; set; }
}
