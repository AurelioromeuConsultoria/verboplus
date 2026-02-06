using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class HubCasa
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public int AbertoPorId { get; set; }
    public virtual Usuario AbertoPor { get; set; } = null!;

    [Required]
    public int LiderId { get; set; }
    public virtual Usuario Lider { get; set; } = null!;

    [Required]
    public int TimoteoId { get; set; }
    public virtual Usuario Timoteo { get; set; } = null!;

    [Required]
    [MaxLength(300)]
    public string EnderecoCompleto { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Anfitriao { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}
