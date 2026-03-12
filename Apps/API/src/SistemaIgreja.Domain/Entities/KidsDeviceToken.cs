using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

/// <summary>
/// Token FCM (Firebase Cloud Messaging) para envio de push ao responsável.
/// Um responsável pode ter vários dispositivos (vários tokens).
/// </summary>
public class KidsDeviceToken
{
    public int Id { get; set; }

    [Required]
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string FcmToken { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Platform { get; set; } = "Android"; // "Android", "iOS"

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
