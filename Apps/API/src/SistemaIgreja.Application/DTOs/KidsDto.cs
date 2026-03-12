using System.ComponentModel.DataAnnotations;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

// DTOs para Criança
public class CriancaDto
{
    public int PessoaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    
    // Detalhes
    public string? Alergias { get; set; }
    public string? RestricoesAlimentares { get; set; }
    public string? Observacoes { get; set; }
    public string? SalaId { get; set; }
    public DateTime DataCadastro { get; set; }
    
    // Relacionamentos
    public List<ResponsavelCriancaDto> Responsaveis { get; set; } = new();
    public bool EstaCheckedIn { get; set; }
    public KidsCheckinDto? CheckinAtual { get; set; }
}

public class CreateCriancaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(20)]
    public string? WhatsApp { get; set; }

    [MaxLength(500)]
    public string? Alergias { get; set; }

    [MaxLength(500)]
    public string? RestricoesAlimentares { get; set; }

    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    [MaxLength(50)]
    public string? SalaId { get; set; }

    public List<ResponsavelRequest>? Responsaveis { get; set; }
}

public class ResponsavelRequest
{
    // Se o responsável já existe, fornecer o ID
    public int? ResponsavelPessoaId { get; set; }

    // Se o responsável não existe, fornecer dados para criar
    [MaxLength(100)]
    public string? Nome { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(20)]
    public string? WhatsApp { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "PodeRetirar é obrigatório")]
    public bool PodeRetirar { get; set; } = true;

    [MaxLength(50)]
    public string? Parentesco { get; set; }
}

public class UpdateCriancaRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(20)]
    public string? WhatsApp { get; set; }

    [MaxLength(500)]
    public string? Alergias { get; set; }

    [MaxLength(500)]
    public string? RestricoesAlimentares { get; set; }

    [MaxLength(1000)]
    public string? Observacoes { get; set; }

    [MaxLength(50)]
    public string? SalaId { get; set; }
}

// DTOs para Responsáveis
public class ResponsavelCriancaDto
{
    public int Id { get; set; }
    public int CriancaPessoaId { get; set; }
    public string CriancaNome { get; set; } = string.Empty;
    public int ResponsavelPessoaId { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string? ResponsavelTelefone { get; set; }
    public string? ResponsavelWhatsApp { get; set; }
    public string? ResponsavelEmail { get; set; }
    public bool PodeRetirar { get; set; }
    public string? Parentesco { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
}

public class CreateResponsavelRequest
{
    [Required(ErrorMessage = "ResponsavelPessoaId é obrigatório")]
    public int ResponsavelPessoaId { get; set; }

    [Required(ErrorMessage = "PodeRetirar é obrigatório")]
    public bool PodeRetirar { get; set; } = true;

    [MaxLength(50)]
    public string? Parentesco { get; set; }
}

public class UpdateResponsavelRequest
{
    public bool? PodeRetirar { get; set; }

    [MaxLength(50)]
    public string? Parentesco { get; set; }

    public bool? Ativo { get; set; }
}

// DTOs para Check-in/Check-out
public class CheckinRequest
{
    [Required(ErrorMessage = "CriancaPessoaId é obrigatório")]
    public int CriancaPessoaId { get; set; }

    [Required(ErrorMessage = "Método é obrigatório")]
    [MaxLength(20)]
    public string Metodo { get; set; } = "ADMIN"; // "QR", "PIN", "ADMIN"

    public int? CheckinByPessoaId { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }
}

public class CheckoutRequest
{
    [Required(ErrorMessage = "CriancaPessoaId é obrigatório")]
    public int CriancaPessoaId { get; set; }

    [Required(ErrorMessage = "CodigoSessao é obrigatório")]
    [MaxLength(50)]
    public string CodigoSessao { get; set; } = string.Empty;

    [Required(ErrorMessage = "CheckoutByPessoaId é obrigatório")]
    public int CheckoutByPessoaId { get; set; }

    [MaxLength(20)]
    public string? Metodo { get; set; } // "QR", "PIN", "ADMIN"
}

public class CheckinResponse
{
    public int CheckinId { get; set; }
    public string CodigoSessao { get; set; } = string.Empty;
    public DateTime CheckinTime { get; set; }
    public List<NotificacaoCriadaDto> Notificacoes { get; set; } = new();
}

public class NotificacaoCriadaDto
{
    public int ResponsavelPessoaId { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class KidsCheckinDto
{
    public int Id { get; set; }
    public int CriancaPessoaId { get; set; }
    public string CriancaNome { get; set; } = string.Empty;
    public DateTime CheckinTime { get; set; }
    public DateTime? CheckoutTime { get; set; }
    public int? CheckinByPessoaId { get; set; }
    public string? CheckinByNome { get; set; }
    public int? CheckoutByPessoaId { get; set; }
    public string? CheckoutByNome { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public string CodigoSessao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
}

// DTOs para Notificações
public class KidsNotificacaoDto
{
    public int Id { get; set; }
    public int CriancaPessoaId { get; set; }
    public string CriancaNome { get; set; } = string.Empty;
    public int ResponsavelPessoaId { get; set; }
    public string ResponsavelNome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime? EnviadoEm { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
}

// Registro de token FCM para push
public class RegisterDeviceTokenRequest
{
    [Required(ErrorMessage = "Token é obrigatório")]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Platform { get; set; } = "Android"; // "Android" ou "iOS"
}

