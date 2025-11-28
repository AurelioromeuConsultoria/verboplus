using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class DestaqueSite
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Texto { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    [MaxLength(500)]
    public string? Imagem { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}



