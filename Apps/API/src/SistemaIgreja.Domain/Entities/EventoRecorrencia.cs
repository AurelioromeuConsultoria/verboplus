using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class EventoRecorrencia : ITenantEntity
{
    public int Id { get; set; }

    [Required]
    public int TenantId { get; set; } = Tenant.InitialTenantId;

    public virtual Tenant Tenant { get; set; } = null!;

    [Required]
    public int EventoId { get; set; }
    public virtual Evento Evento { get; set; } = null!;

    [Required]
    public DayOfWeek DiaSemana { get; set; }

    [Required]
    public TimeSpan HoraInicio { get; set; }

    public TimeSpan? HoraFim { get; set; }

    [Required]
    public PeriodicidadeRecorrencia Periodicidade { get; set; } = PeriodicidadeRecorrencia.Semanal;

    [Required]
    public DateTime DataInicioVigencia { get; set; }

    public DateTime? DataFimVigencia { get; set; }

    [Required]
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Semanas do mês (1 a 5) em que esta recorrência NÃO deve gerar ocorrências,
    /// armazenadas como CSV (ex.: "2" ou "1,3"). Null/vazio = não pula nenhuma.
    /// Útil para regras como "todo domingo, exceto o 2º domingo do mês".
    /// </summary>
    [MaxLength(20)]
    public string? SemanasDoMesExcluidas { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public virtual ICollection<EventoOcorrencia> Ocorrencias { get; set; } = new List<EventoOcorrencia>();
}

public enum PeriodicidadeRecorrencia
{
    Semanal = 1,
    Quinzenal = 2,
    Mensal = 3
}
