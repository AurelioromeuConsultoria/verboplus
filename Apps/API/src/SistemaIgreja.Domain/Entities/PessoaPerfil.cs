using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class PessoaPerfil
{
    public int Id { get; set; }

    [Required]
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;

    [Required]
    public PerfilPessoa Perfil { get; set; }

    [Required]
    public DateTime DataInicio { get; set; } = DateTime.Now;

    public DateTime? DataFim { get; set; }
}



