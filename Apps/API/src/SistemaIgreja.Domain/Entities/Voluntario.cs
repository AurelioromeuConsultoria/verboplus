using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Voluntario
{
    public int Id { get; set; }

    [Required]
    public int PessoaId { get; set; }
    public virtual Pessoa Pessoa { get; set; } = null!;

    [Required]
    public int EquipeId { get; set; }
    public virtual Equipe Equipe { get; set; } = null!;

    [Required]
    public int CargoId { get; set; }
    public virtual Cargo Cargo { get; set; } = null!;

    [Required]
    public DateTime DataCadastro { get; set; } = DateTime.Now;
}
