using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string EmailLogin { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    public TipoUsuario TipoUsuario { get; set; } = TipoUsuario.Portal;

    [Required]
    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? UltimoAcesso { get; set; }
}






