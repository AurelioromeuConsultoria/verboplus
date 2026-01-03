using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class KidsNotificacao
{
    public int Id { get; set; }

    [Required]
    public int CriancaPessoaId { get; set; }
    public virtual Pessoa Crianca { get; set; } = null!;

    [Required]
    public int ResponsavelPessoaId { get; set; }
    public virtual Pessoa Responsavel { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Tipo { get; set; } = string.Empty; // "CHECKIN", "CHECKOUT", "ALERTA"

    [Required]
    [MaxLength(1000)]
    public string Mensagem { get; set; } = string.Empty;

    public DateTime? EnviadoEm { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pendente"; // "Pendente", "Enviado", "Falhou"

    [Required]
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}


