using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Visitante
{
    public int Id { get; set; }
    
    [Required]
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;
    
    [Required]
    public DateTime DataVisita { get; set; }
    
    [MaxLength(500)]
    public string? Observacoes { get; set; }
    
    [Required]
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    
    // Relacionamento com mensagens agendadas
    public virtual ICollection<MensagemAgendada> MensagensAgendadas { get; set; } = new List<MensagemAgendada>();
}

