using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public enum AreaEquipe
{
    Verde = 1,
    Vermelha = 2,
    Laranja = 3
}

public class Equipe
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public AreaEquipe Area { get; set; }

    public int? LiderUsuarioId { get; set; }
    public virtual Usuario? LiderUsuario { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public virtual ICollection<Voluntario> Voluntarios { get; set; } = new List<Voluntario>();
}
