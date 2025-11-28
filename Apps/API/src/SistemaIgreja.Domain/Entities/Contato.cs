using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Contato
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

    [Required]
    public bool Membro { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Mensagem { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}

