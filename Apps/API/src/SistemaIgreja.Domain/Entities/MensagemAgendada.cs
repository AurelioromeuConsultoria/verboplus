using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class MensagemAgendada
{
    public int Id { get; set; }
    
    public int VisitanteId { get; set; }
    public virtual Visitante Visitante { get; set; } = null!;
    
    public int ConfiguracaoMensagemId { get; set; }
    public virtual ConfiguracaoMensagem ConfiguracaoMensagem { get; set; } = null!;
    
    public DateTime DataAgendamento { get; set; }
    
    public DateTime DataEnvio { get; set; }
    
    public StatusMensagem Status { get; set; } = StatusMensagem.Agendada;
    
    [MaxLength(1000)]
    public string TextoFinal { get; set; } = string.Empty;
    
    public DateTime? DataProcessamento { get; set; }
    
    [MaxLength(500)]
    public string? LogErro { get; set; }
    
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}

public enum StatusMensagem
{
    Agendada = 1,
    ProntaParaEnvio = 2,
    Enviada = 3,
    Erro = 4,
    Cancelada = 5
}

