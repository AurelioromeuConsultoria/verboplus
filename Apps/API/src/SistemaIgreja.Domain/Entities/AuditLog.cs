using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string EntityName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Action { get; set; } = string.Empty; // Create | Update | Delete

    public int? UserId { get; set; }

    [MaxLength(200)]
    public string? UserName { get; set; }

    [MaxLength(200)]
    public string? UserEmail { get; set; }

    [MaxLength(60)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ChangesJson { get; set; }
}

