using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class ContaBancaria
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Banco { get; set; }

    [MaxLength(20)]
    public string? Agencia { get; set; }

    [MaxLength(20)]
    public string? Conta { get; set; }

    [MaxLength(10)]
    public string? TipoConta { get; set; } // Corrente, Poupança, etc.

    [Required]
    public decimal SaldoInicial { get; set; } = 0;

    [Required]
    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamentos
    public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    public virtual ICollection<Receita> Receitas { get; set; } = new List<Receita>();
}
