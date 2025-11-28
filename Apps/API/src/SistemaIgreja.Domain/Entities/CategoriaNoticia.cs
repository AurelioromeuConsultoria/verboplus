using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class CategoriaNoticia
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamento com notícias
    public virtual ICollection<Noticia> Noticias { get; set; } = new List<Noticia>();
}



