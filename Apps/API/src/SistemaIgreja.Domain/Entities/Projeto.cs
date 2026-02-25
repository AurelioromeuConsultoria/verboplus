using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Projeto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    public DateTime? DataInicio { get; set; }

    public DateTime? DataFim { get; set; }

    public decimal? Orcamento { get; set; }

    [Required]
    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamentos
    public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    public virtual ICollection<Receita> Receitas { get; set; } = new List<Receita>();
}
