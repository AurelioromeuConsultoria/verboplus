using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

public class InscricaoEventoDto
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string? EventoTitulo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string? Email { get; set; }
    public StatusInscricao Status { get; set; }
    public string StatusDescricao { get; set; } = string.Empty;
    public int QuantidadeAcompanhantes { get; set; }
    public string? Observacoes { get; set; }
    public string? ObservacoesInternas { get; set; }
    public DateTime DataInscricao { get; set; }
    public DateTime? DataConfirmacao { get; set; }
    public DateTime? DataCancelamento { get; set; }
}

public class CriarInscricaoEventoDto
{
    public int EventoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int QuantidadeAcompanhantes { get; set; } = 0;
    public string? Observacoes { get; set; }
}

public class AtualizarInscricaoEventoDto
{
    public StatusInscricao Status { get; set; }
    public int QuantidadeAcompanhantes { get; set; }
    public string? Observacoes { get; set; }
    public string? ObservacoesInternas { get; set; }
}

public class EstatisticasInscricaoDto
{
    public int EventoId { get; set; }
    public string EventoTitulo { get; set; } = string.Empty;
    public int TotalInscricoes { get; set; }
    public int InscricoesConfirmadas { get; set; }
    public int InscricoesPendentes { get; set; }
    public int InscricoesCanceladas { get; set; }
    public int TotalParticipantes { get; set; }
}






