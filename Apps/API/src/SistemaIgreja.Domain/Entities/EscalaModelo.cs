using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

/// <summary>Modelo de escala: quantas pessoas (por cargo) uma equipe precisa para um evento.</summary>
public class EscalaModelo
{
    public int Id { get; set; }

    /// <summary>Null = modelo padrão da equipe para qualquer evento.</summary>
    public int? EventoId { get; set; }
    public virtual Evento? Evento { get; set; }

    [Required]
    public int EquipeId { get; set; }
    public virtual Equipe Equipe { get; set; } = null!;

    [MaxLength(200)]
    public string? Nome { get; set; }

    /// <summary>Dias de folga após ser escalado (não sugerir de novo nesse período).</summary>
    public int? DiasFolgaAposEscala { get; set; }

    [Required]
    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public virtual ICollection<EscalaModeloItem> Itens { get; set; } = new List<EscalaModeloItem>();
}
