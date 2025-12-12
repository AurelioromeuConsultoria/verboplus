using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Pessoa
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(20)]
    public string? WhatsApp { get; set; }

    public DateTime? DataNascimento { get; set; }

    [Required]
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Adulto;

    [Required]
    public bool Ativo { get; set; } = true;

    [Required]
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    // Relacionamentos
    public virtual Usuario? Usuario { get; set; }
    public virtual ICollection<Visitante> Visitantes { get; set; } = new List<Visitante>();
    public virtual ICollection<Voluntario> Voluntarios { get; set; } = new List<Voluntario>();
    public virtual ICollection<PessoaPerfil> Perfis { get; set; } = new List<PessoaPerfil>();
}



