using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.DTOs;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class AlterarSenhaDto
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public int PessoaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmailLogin { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public string TipoUsuarioDescricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoAcesso { get; set; }
}

public class CriarUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string EmailLogin { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
}

public class AtualizarUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string EmailLogin { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public bool Ativo { get; set; }
}






