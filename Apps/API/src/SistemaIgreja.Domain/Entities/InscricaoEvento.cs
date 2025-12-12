using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class InscricaoEvento
{
    public int Id { get; set; }

    // Relacionamento com Evento
    [Required]
    public int EventoId { get; set; }
    public virtual Evento Evento { get; set; } = null!;

    // Dados do Participante
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string WhatsApp { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    // Status e Informações Adicionais
    [Required]
    public StatusInscricao Status { get; set; } = StatusInscricao.Pendente;

    public int QuantidadeAcompanhantes { get; set; } = 0;

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(500)]
    public string? ObservacoesInternas { get; set; }

    public DateTime DataInscricao { get; set; } = DateTime.Now;
    public DateTime? DataConfirmacao { get; set; }
    public DateTime? DataCancelamento { get; set; }
}



