using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Cargo
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public virtual ICollection<Voluntario> Voluntarios { get; set; } = new List<Voluntario>();
}
