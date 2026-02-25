using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

public class EventoOcorrenciaDto
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string EventoTitulo { get; set; } = string.Empty;
    public int? EventoRecorrenciaId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public StatusEventoOcorrencia Status { get; set; }
    public bool GeradaAutomaticamente { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool PossuiEscala { get; set; }
    public int? EscalaId { get; set; }
}

public class CriarEventoOcorrenciaDto
{
    public int EventoId { get; set; }
    public int? EventoRecorrenciaId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public StatusEventoOcorrencia Status { get; set; } = StatusEventoOcorrencia.Confirmado;
    public bool GeradaAutomaticamente { get; set; } = false;
}

public class AtualizarEventoOcorrenciaDto
{
    public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public StatusEventoOcorrencia Status { get; set; } = StatusEventoOcorrencia.Confirmado;
}
