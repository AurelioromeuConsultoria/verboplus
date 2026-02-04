using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class ConfiguracaoPortal
{
    public int Id { get; set; } = 1; // Sempre será 1 (singleton)

    /// <summary>
    /// Tempo de transição do carrossel de destaques em segundos (padrão: 5 segundos)
    /// </summary>
    public int TempoTransicaoCarrossel { get; set; } = 5;

    public DateTime DataAtualizacao { get; set; } = DateTime.Now;
}
