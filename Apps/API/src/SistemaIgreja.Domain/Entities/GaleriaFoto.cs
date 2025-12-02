using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIgreja.Domain.Entities;

public class GaleriaFoto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descricao { get; set; }

    [Required]
    public DateTime Data { get; set; }

    [Required]
    [MaxLength(500)]
    public string CaminhoDiretorio { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImagemDestaque { get; set; }

    public int QuantidadeFotos { get; set; } = 0;

    public bool Ativo { get; set; } = true;

    // Relacionamento opcional com Evento
    public int? EventoId { get; set; }
    [ForeignKey("EventoId")]
    public virtual Evento? Evento { get; set; }

    // Relacionamento com CategoriaMidia
    public int? CategoriaMidiaId { get; set; }
    [ForeignKey("CategoriaMidiaId")]
    public virtual CategoriaMidia? CategoriaMidia { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}

