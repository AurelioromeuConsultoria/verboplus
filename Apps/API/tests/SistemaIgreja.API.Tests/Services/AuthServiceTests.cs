using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "uma-chave-super-segura-com-pelo-menos-32-caracteres",
                ["Jwt:Issuer"] = "SistemaIgreja.Tests",
                ["Jwt:Audience"] = "SistemaIgreja.Tests"
            })
            .Build();

        _service = new AuthService(
            _usuarioRepositoryMock.Object,
            configuration,
            _loggerMock.Object,
            _auditLogServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenUserDoesNotExist()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("inexistente@app.com"))
            .ReturnsAsync((Usuario?)null);

        var act = () => _service.LoginAsync(new LoginDto
        {
            Email = "inexistente@app.com",
            Senha = "123456"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email ou senha inválidos");
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenUserIsInactive()
    {
        var usuario = CriarUsuario(ativo: false);
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(usuario.EmailLogin))
            .ReturnsAsync(usuario);

        var act = () => _service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "123456"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Usuário inativo");
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_UpdatesLastAccess_AndAudits()
    {
        var usuario = CriarUsuario();
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(usuario.EmailLogin))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock.Setup(r => r.UpdateAsync(usuario))
            .ReturnsAsync(usuario);

        var result = await _service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "123456"
        });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.Usuario.Id.Should().Be(usuario.Id);
        usuario.UltimoAcesso.Should().NotBeNull();
        _usuarioRepositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("Auth", usuario.Id.ToString(), "Login", It.IsAny<object?>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ThrowsUnauthorized_WhenTokenIsInvalid()
    {
        var act = () => _service.RefreshTokenAsync("token-invalido");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token inválido");
    }

    [Fact]
    public async Task RefreshTokenAsync_ReturnsNewTokens_WhenRefreshTokenIsValid()
    {
        var usuario = CriarUsuario();
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(usuario.EmailLogin))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock.Setup(r => r.UpdateAsync(usuario))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        var login = await _service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "123456"
        });

        var refreshed = await _service.RefreshTokenAsync(login.RefreshToken);

        refreshed.Token.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(login.RefreshToken);
        refreshed.Usuario.Id.Should().Be(usuario.Id);
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("Auth", usuario.Id.ToString(), "RefreshToken", It.IsAny<object?>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUsuarioLogadoAsync_ReturnsMappedUser()
    {
        var usuario = CriarUsuario();
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        var result = await _service.GetUsuarioLogadoAsync(usuario.Id);

        result.Id.Should().Be(usuario.Id);
        result.Nome.Should().Be(usuario.Pessoa.Nome);
        result.Email.Should().Be(usuario.Pessoa.Email);
        result.PerfilAcessoNome.Should().Be(usuario.PerfilAcesso!.Nome);
        result.Permissoes.Should().HaveCount(1);
    }

    [Fact]
    public async Task AlterarSenhaAsync_ThrowsUnauthorized_WhenCurrentPasswordIsInvalid()
    {
        var usuario = CriarUsuario();
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);

        var act = () => _service.AlterarSenhaAsync(usuario.Id, new AlterarSenhaDto
        {
            SenhaAtual = "senha-errada",
            NovaSenha = "nova-senha"
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Senha atual incorreta");
    }

    [Fact]
    public async Task AlterarSenhaAsync_UpdatesPassword_AndAudits()
    {
        var usuario = CriarUsuario();
        var senhaAnterior = usuario.SenhaHash;
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(usuario.Id))
            .ReturnsAsync(usuario);
        _usuarioRepositoryMock.Setup(r => r.UpdateAsync(usuario))
            .ReturnsAsync(usuario);

        await _service.AlterarSenhaAsync(usuario.Id, new AlterarSenhaDto
        {
            SenhaAtual = "123456",
            NovaSenha = "nova-senha"
        });

        usuario.SenhaHash.Should().NotBe(senhaAnterior);
        BCrypt.Net.BCrypt.Verify("nova-senha", usuario.SenhaHash).Should().BeTrue();
        _usuarioRepositoryMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("Usuario", usuario.Id.ToString(), "AlterarSenha", It.IsAny<object?>()),
            Times.Once);
    }

    private static Usuario CriarUsuario(bool ativo = true)
    {
        return new Usuario
        {
            Id = 10,
            PessoaId = 20,
            EmailLogin = "admin@app.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            TipoUsuario = TipoUsuario.Admin,
            Ativo = ativo,
            Pessoa = new Pessoa
            {
                Id = 20,
                Nome = "Administrador Teste",
                Email = "admin@app.com",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true
            },
            PerfilAcessoId = 30,
            PerfilAcesso = new PerfilAcesso
            {
                Id = 30,
                Nome = "Administradores",
                Permissoes = new List<PerfilAcessoPermissao>
                {
                    new()
                    {
                        Id = 1,
                        Recurso = "Usuarios",
                        PodeVer = true,
                        PodeEditar = true,
                        PodeExcluir = true
                    }
                }
            }
        };
    }
}
