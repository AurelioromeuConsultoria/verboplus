using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
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
    private readonly Dictionary<string, string> _refreshTokens = new(); // Em produção, usar Redis ou banco

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        if (!usuario.Ativo)
        {
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Atualizar último acesso
        usuario.UltimoAcesso = DateTime.Now;
        await _usuarioRepository.UpdateAsync(usuario);

        var token = GenerateJwtToken(usuario);
        var refreshToken = GenerateRefreshToken();

        // Armazenar refresh token (em produção, usar banco ou Redis)
        _refreshTokens[refreshToken] = usuario.Id.ToString();

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
            throw new UnauthorizedAccessException("Refresh token inválido");
        }

        var usuarioId = int.Parse(usuarioIdStr);
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);

        if (usuario == null || !usuario.Ativo)
        {
            _refreshTokens.Remove(refreshToken);
            throw new UnauthorizedAccessException("Usuário não encontrado ou inativo");
        }

        var newToken = GenerateJwtToken(usuario);
        var newRefreshToken = GenerateRefreshToken();

        // Remover token antigo e adicionar novo
        _refreshTokens.Remove(refreshToken);
        _refreshTokens[newRefreshToken] = usuarioId.ToString();

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
            throw new UnauthorizedAccessException("Senha atual incorreta");
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
        await _usuarioRepository.UpdateAsync(usuario);
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres");
        var issuer = _configuration["Jwt:Issuer"] ?? "SistemaIgreja";
        var audience = _configuration["Jwt:Audience"] ?? "SistemaIgreja";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
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
            Nome = u.Nome,
            Email = u.Email,
            TipoUsuario = u.TipoUsuario,
            TipoUsuarioDescricao = GetTipoUsuarioDescricao(u.TipoUsuario),
            Ativo = u.Ativo,
            DataCriacao = u.DataCriacao,
            UltimoAcesso = u.UltimoAcesso
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

