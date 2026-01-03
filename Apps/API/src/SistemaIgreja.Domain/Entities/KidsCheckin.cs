using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class KidsCheckin
{
    public int Id { get; set; }

    [Required]
    public int CriancaPessoaId { get; set; }
    public virtual Pessoa Crianca { get; set; } = null!;

    [Required]
    public DateTime CheckinTime { get; set; } = DateTime.UtcNow;

    public DateTime? CheckoutTime { get; set; }

    public int? CheckinByPessoaId { get; set; }
    public virtual Pessoa? CheckinBy { get; set; }

    public int? CheckoutByPessoaId { get; set; }
    public virtual Pessoa? CheckoutBy { get; set; }

    [Required]
    [MaxLength(20)]
    public string Metodo { get; set; } = "ADMIN"; // "QR", "PIN", "ADMIN"

    [Required]
    [MaxLength(50)]
    public string CodigoSessao { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "CheckedIn"; // "CheckedIn", "CheckedOut"

    [MaxLength(500)]
    public string? Observacoes { get; set; }
}


