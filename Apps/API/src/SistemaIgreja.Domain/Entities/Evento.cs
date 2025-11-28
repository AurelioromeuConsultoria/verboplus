using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Evento
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [MaxLength(500)]
    public string? ImagemDestaque { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamento com inscrições
    public virtual ICollection<InscricaoEvento> Inscricoes { get; set; } = new List<InscricaoEvento>();
}



