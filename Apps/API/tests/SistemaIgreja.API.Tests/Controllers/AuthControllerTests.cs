using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        var response = new LoginResponseDto
        {
            Token = "token",
            RefreshToken = "refresh",
            Usuario = new UsuarioDto { Id = 1 }
        };
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(response);

        var result = await _controller.Login(new LoginDto { Email = "admin@app.com", Senha = "123456" });

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Email ou senha inválidos"));

        var result = await _controller.Login(new LoginDto());

        result.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Email ou senha inválidos");
    }

    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        _authServiceMock.Setup(s => s.RefreshTokenAsync("invalido"))
            .ThrowsAsync(new UnauthorizedAccessException("Refresh token inválido"));

        var result = await _controller.RefreshToken(new RefreshTokenDto { RefreshToken = "invalido" });

        result.Result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Refresh token inválido");
    }

    [Fact]
    public async Task GetMe_ReturnsOk_WithAuthenticatedUser()
    {
        SetUser(15);
        var usuario = new UsuarioDto { Id = 15, Nome = "Usuario Logado" };
        _authServiceMock.Setup(s => s.GetUsuarioLogadoAsync(15))
            .ReturnsAsync(usuario);

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(usuario);
    }

    [Fact]
    public async Task AlterarSenha_ReturnsNoContent_WhenServiceSucceeds()
    {
        SetUser(22);

        var result = await _controller.AlterarSenha(new AlterarSenhaDto
        {
            SenhaAtual = "123456",
            NovaSenha = "nova"
        });

        result.Should().BeOfType<NoContentResult>();
        _authServiceMock.Verify(
            s => s.AlterarSenhaAsync(22, It.IsAny<AlterarSenhaDto>()),
            Times.Once);
    }

    [Fact]
    public async Task AlterarSenha_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        SetUser(22);
        _authServiceMock.Setup(s => s.AlterarSenhaAsync(22, It.IsAny<AlterarSenhaDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Senha atual incorreta"));

        var result = await _controller.AlterarSenha(new AlterarSenhaDto());

        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().Be("Senha atual incorreta");
    }

    private void SetUser(int usuarioId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
