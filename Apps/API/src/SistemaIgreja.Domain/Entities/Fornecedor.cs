using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Domain.Entities;

public class Fornecedor
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? RazaoSocial { get; set; }

    [MaxLength(20)]
    public string? CnpjCpf { get; set; }

    [MaxLength(30)]
    public string? InscricaoEstadual { get; set; }

    [MaxLength(300)]
    public string? Endereco { get; set; }

    [MaxLength(30)]
    public string? Telefone { get; set; }

    [MaxLength(200)]
    public string? Site { get; set; }

    [MaxLength(150)]
    public string? ContatoNome { get; set; }

    [MaxLength(20)]
    public string? ContatoCpf { get; set; }

    [MaxLength(30)]
    public string? ContatoWhatsApp { get; set; }

    [MaxLength(150)]
    public string? ContatoEmail { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.Now;
}
