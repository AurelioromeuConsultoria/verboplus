using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class EscalaItem
{
    public int Id { get; set; }

    [Required]
    public int EscalaId { get; set; }
    public virtual Escala Escala { get; set; } = null!;

    [Required]
    public int EquipeId { get; set; }
    public virtual Equipe Equipe { get; set; } = null!;

    public int? CargoId { get; set; }
    public virtual Cargo? Cargo { get; set; }

    [Required]
    public int VoluntarioId { get; set; }
    public virtual Voluntario Voluntario { get; set; } = null!;

    [Required]
    public int Ordem { get; set; } = 0;

    [Required]
    public bool ConflitoAprovado { get; set; } = false;

    [MaxLength(500)]
    public string? MotivoExcecao { get; set; }

    public int? AprovadoPorUsuarioId { get; set; }
    public virtual Usuario? AprovadoPorUsuario { get; set; }

    public DateTime? AprovadoEm { get; set; }
    [Required]
    public StatusEscalaItem Status { get; set; } = StatusEscalaItem.Pendente;
    public DateTime? DataConvite { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public DateTime? DataRecusa { get; set; }
    public DateTime? DataLembrete7DiasEnviado { get; set; }
    public DateTime? DataLembrete24HorasEnviado { get; set; }

    [MaxLength(500)]
    public string? MotivoRecusa { get; set; }

    public int? RespondidoPorUsuarioId { get; set; }
    public virtual Usuario? RespondidoPorUsuario { get; set; }

    [MaxLength(500)]
    public string? ObservacaoOperacional { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}

public enum StatusEscalaItem
{
    Pendente = 1,
    Confirmado = 2,
    Recusado = 3,
    Substituido = 4,
    Serviu = 5,
    Faltou = 6
}
