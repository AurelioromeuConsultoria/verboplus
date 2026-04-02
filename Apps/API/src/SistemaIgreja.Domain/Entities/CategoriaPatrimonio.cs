using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class CategoriaPatrimonio
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Descricao { get; set; }

    [Required]
    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public virtual ICollection<PatrimonioItem> Itens { get; set; } = new List<PatrimonioItem>();
}
