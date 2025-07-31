using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Visitante
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;
    
    public DateTime DataVisita { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MaxLength(500)]
    public string? Observacoes { get; set; }
    
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    
    // Relacionamento com mensagens agendadas
    public virtual ICollection<MensagemAgendada> MensagensAgendadas { get; set; } = new List<MensagemAgendada>();
}

