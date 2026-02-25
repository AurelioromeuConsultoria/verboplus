using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public enum StatusDespesa
{
    Pendente = 1,
    Paga = 2,
    Cancelada = 3
}

public class Despesa
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public decimal Valor { get; set; }

    [Required]
    public DateTime DataVencimento { get; set; }

    public DateTime? DataPagamento { get; set; }

    [Required]
    public StatusDespesa Status { get; set; } = StatusDespesa.Pendente;

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? ComprovanteUrl { get; set; }

    // Relacionamentos
    public int? FornecedorId { get; set; }
    public virtual Fornecedor? Fornecedor { get; set; }

    public int? CategoriaDespesaId { get; set; }
    public virtual CategoriaDespesa? CategoriaDespesa { get; set; }

    public int? ContaBancariaId { get; set; }
    public virtual ContaBancaria? ContaBancaria { get; set; }

    public int? CentroCustoId { get; set; }
    public virtual CentroCusto? CentroCusto { get; set; }

    public int? ProjetoId { get; set; }
    public virtual Projeto? Projeto { get; set; }

    public int? UsuarioId { get; set; }
    public virtual Usuario? Usuario { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}
