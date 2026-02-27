using System.ComponentModel.DataAnnotations;

namespace SistemaIgreja.Application.DTOs;

/// <summary>
/// DTO para cadastro público de membros (formulário web sem autenticação)
/// </summary>
public class CadastroMembroDto
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "WhatsApp é obrigatório")]
    [MaxLength(20)]
    public string WhatsApp { get; set; } = string.Empty;

    public DateTime? DataNascimento { get; set; }
}
