using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class ConfiguracaoCampanhaAniversario
{
    public int Id { get; set; } = 1;

    public bool Ativo { get; set; } = true;

    [MaxLength(500)]
    public string? ImagemUrl { get; set; }

    [Required]
    [MaxLength(4000)]
    public string MensagemTemplate { get; set; } = string.Empty;

    public TimeSpan HorarioEnvio { get; set; } = new(9, 0, 0);

    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}
