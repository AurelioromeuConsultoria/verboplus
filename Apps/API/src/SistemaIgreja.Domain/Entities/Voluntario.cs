using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Voluntario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string WhatsApp { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    public int EquipeId { get; set; }
    public virtual Equipe Equipe { get; set; } = null!;

    public int CargoId { get; set; }
    public virtual Cargo Cargo { get; set; } = null!;

    public DateTime DataCadastro { get; set; } = DateTime.Now;
}
