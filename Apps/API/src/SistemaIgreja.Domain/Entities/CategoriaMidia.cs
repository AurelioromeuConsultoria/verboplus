using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class CategoriaMidia
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descricao { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamento com galerias
    public virtual ICollection<GaleriaFoto> Galerias { get; set; } = new List<GaleriaFoto>();
}

