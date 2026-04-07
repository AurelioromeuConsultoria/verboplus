using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class CriancaDetalhe
{
    [Required]
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;

    [MaxLength(500)]
    public string? Alergias { get; set; }

    [MaxLength(500)]
    public string? RestricoesAlimentares { get; set; }

    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    [MaxLength(50)]
    public string? SalaId { get; set; }

    [MaxLength(50)]
    public string? TurmaId { get; set; }

    [Required]
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}

