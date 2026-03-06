using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

/// <summary>Indica que o voluntário não está disponível em uma data específica.</summary>
public class IndisponibilidadeVoluntario
{
    public int Id { get; set; }

    [Required]
    public int VoluntarioId { get; set; }
    public virtual Voluntario Voluntario { get; set; } = null!;

    [Required]
    public DateTime Data { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}
