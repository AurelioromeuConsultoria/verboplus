using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Evento
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [MaxLength(500)]
    public string? ImagemDestaque { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    [Required]
    public TipoEvento Tipo { get; set; } = TipoEvento.Evento;

    [Required]
    public bool EhRecorrente { get; set; } = false;

    [Required]
    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamento com inscrições
    public virtual ICollection<InscricaoEvento> Inscricoes { get; set; } = new List<InscricaoEvento>();
    public virtual ICollection<EventoRecorrencia> Recorrencias { get; set; } = new List<EventoRecorrencia>();
    public virtual ICollection<EventoOcorrencia> Ocorrencias { get; set; } = new List<EventoOcorrencia>();
}

public enum TipoEvento
{
    Evento = 1,
    Culto = 2,
    Reuniao = 3,
    Outro = 4
}



