using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<UsuarioDto> GetUsuarioLogadoAsync(int usuarioId);
    Task AlterarSenhaAsync(int usuarioId, AlterarSenhaDto dto);
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly Dictionary<string, string> _refreshTokens = new(); // Em produção, usar Redis ou banco

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Falha de login. Email={Email}", dto.Email);
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        if (!usuario.Ativo)
        {
            _logger.LogWarning("Tentativa de login com usuário inativo. UsuarioId={UsuarioId} Email={EmailLogin}", usuario.Id, usuario.EmailLogin);
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Atualizar último acesso
        usuario.UltimoAcesso = DateTime.Now;
        await _usuarioRepository.UpdateAsync(usuario);

        var token = GenerateJwtToken(usuario);
        var refreshToken = GenerateRefreshToken();

        // Armazenar refresh token (em produção, usar banco ou Redis)
        _refreshTokens[refreshToken] = usuario.Id.ToString();

        _logger.LogInformation(
            "Login realizado com sucesso. UsuarioId={UsuarioId} PessoaId={PessoaId} TipoUsuario={TipoUsuario}",
            usuario.Id,
            usuario.PessoaId,
            usuario.TipoUsuario);

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hora
            Usuario = MapToUsuarioDto(usuario)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var usuarioIdStr))
        {
            _logger.LogWarning("Refresh token inválido.");
            throw new UnauthorizedAccessException("Refresh token inválido");
        }

        var usuarioId = int.Parse(usuarioIdStr);
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario == null || !usuario.Ativo)
        {
            _refreshTokens.Remove(refreshToken);
            _logger.LogWarning("Refresh token rejeitado por usuário ausente ou inativo. UsuarioId={UsuarioId}", usuarioId);
            throw new UnauthorizedAccessException("Usuário não encontrado ou inativo");
        }

        var newToken = GenerateJwtToken(usuario);
        var newRefreshToken = GenerateRefreshToken();

        // Remover token antigo e adicionar novo
        _refreshTokens.Remove(refreshToken);
        _refreshTokens[newRefreshToken] = usuarioId.ToString();

        _logger.LogInformation("Refresh token renovado com sucesso. UsuarioId={UsuarioId}", usuario.Id);

        return new LoginResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
            Usuario = MapToUsuarioDto(usuario)
        };
    }

    public async Task<UsuarioDto> GetUsuarioLogadoAsync(int usuarioId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null) throw new ArgumentException("Usuário não encontrado");

        return MapToUsuarioDto(usuario);
    }

    public async Task AlterarSenhaAsync(int usuarioId, AlterarSenhaDto dto)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null) throw new ArgumentException("Usuário não encontrado");

        if (!BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, usuario.SenhaHash))
        {
            _logger.LogWarning("Falha ao alterar senha por senha atual inválida. UsuarioId={UsuarioId}", usuarioId);
            throw new UnauthorizedAccessException("Senha atual incorreta");
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
        await _usuarioRepository.UpdateAsync(usuario);
        _logger.LogInformation("Senha alterada com sucesso. UsuarioId={UsuarioId}", usuarioId);
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres");
        var issuer = _configuration["Jwt:Issuer"] ?? "SistemaIgreja";
        var audience = _configuration["Jwt:Audience"] ?? "SistemaIgreja";

        var nome = usuario.Pessoa?.Nome ?? string.Empty;
        var email = usuario.Pessoa?.Email ?? usuario.EmailLogin;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, nome),
            new Claim(ClaimTypes.Email, email),
            new Claim("TipoUsuario", usuario.TipoUsuario.ToString()),
            new Claim("TipoUsuarioId", ((int)usuario.TipoUsuario).ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static UsuarioDto MapToUsuarioDto(Usuario u)
    {
        return new UsuarioDto
        {
            Id = u.Id,
            PessoaId = u.PessoaId,
            Nome = u.Pessoa?.Nome ?? string.Empty,
            Email = u.Pessoa?.Email ?? string.Empty,
            EmailLogin = u.EmailLogin,
            TipoUsuario = u.TipoUsuario,
            TipoUsuarioDescricao = GetTipoUsuarioDescricao(u.TipoUsuario),
            Ativo = u.Ativo,
            DataCriacao = u.DataCriacao,
            UltimoAcesso = u.UltimoAcesso,
            PerfilAcessoId = u.PerfilAcessoId,
            PerfilAcessoNome = u.PerfilAcesso?.Nome,
            Permissoes = u.PerfilAcesso?.Permissoes.Select(p => new PermissaoPerfilDto
            {
                Id = p.Id,
                Recurso = p.Recurso,
                PodeVer = p.PodeVer,
                PodeEditar = p.PodeEditar,
                PodeExcluir = p.PodeExcluir
            }).ToList() ?? new List<PermissaoPerfilDto>()
        };
    }

    private static string GetTipoUsuarioDescricao(TipoUsuario tipo)
    {
        return tipo switch
        {
            TipoUsuario.Admin => "Administrador",
            TipoUsuario.Portal => "Portal",
            TipoUsuario.Ambos => "Administrador e Portal",
            _ => "Desconhecido"
        };
    }
}
