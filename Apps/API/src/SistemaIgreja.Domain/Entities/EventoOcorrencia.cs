using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class EventoOcorrencia
{
    public int Id { get; set; }

    [Required]
    public int EventoId { get; set; }
    public virtual Evento Evento { get; set; } = null!;

    public int? EventoRecorrenciaId { get; set; }
    public virtual EventoRecorrencia? EventoRecorrencia { get; set; }

    [Required]
    public DateTime DataHoraInicio { get; set; }

    public DateTime? DataHoraFim { get; set; }

    [Required]
    public StatusEventoOcorrencia Status { get; set; } = StatusEventoOcorrencia.Confirmado;

    [Required]
    public bool GeradaAutomaticamente { get; set; } = false;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public virtual Escala? Escala { get; set; }
}

public enum StatusEventoOcorrencia
{
    Confirmado = 1,
    Cancelado = 2,
    Realizado = 3
}
