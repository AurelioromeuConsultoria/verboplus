namespace SistemaIgreja.Application.DTOs;

public class EventoRecorrenciaDto
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    /// <summary>0=Domingo, 1=Segunda, ... 6=Sábado</summary>
    public int DiaSemana { get; set; }
    public string DiaSemanaDescricao { get; set; } = string.Empty;
    /// <summary>Horário no formato "HH:mm" (ex: "10:00")</summary>
    public string HoraInicio { get; set; } = string.Empty;
    /// <summary>Horário no formato "HH:mm" ou null</summary>
    public string? HoraFim { get; set; }
    /// <summary>1=Semanal, 2=Quinzenal, 3=Mensal</summary>
    public int Periodicidade { get; set; }
    public string PeriodicidadeDescricao { get; set; } = string.Empty;
    public DateTime DataInicioVigencia { get; set; }
    public DateTime? DataFimVigencia { get; set; }
    public bool Ativo { get; set; }
    /// <summary>Semanas do mês (1 a 5) que esta recorrência pula. Vazio = não pula nenhuma.</summary>
    public List<int> SemanasDoMesExcluidas { get; set; } = new();
    public DateTime DataCriacao { get; set; }
}

public class CriarEventoRecorrenciaDto
{
    public int EventoId { get; set; }
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string? HoraFim { get; set; }
    public int Periodicidade { get; set; }
    public DateTime DataInicioVigencia { get; set; }
    public DateTime? DataFimVigencia { get; set; }
    public bool Ativo { get; set; } = true;
    /// <summary>Semanas do mês (1 a 5) a pular. Ex.: [2] = não gera no 2º dia-da-semana do mês.</summary>
    public List<int>? SemanasDoMesExcluidas { get; set; }
}

public class AtualizarEventoRecorrenciaDto
{
    public int DiaSemana { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string? HoraFim { get; set; }
    public int Periodicidade { get; set; }
    public DateTime DataInicioVigencia { get; set; }
    public DateTime? DataFimVigencia { get; set; }
    public bool Ativo { get; set; }
    /// <summary>Semanas do mês (1 a 5) a pular. Ex.: [2] = não gera no 2º dia-da-semana do mês.</summary>
    public List<int>? SemanasDoMesExcluidas { get; set; }
}
