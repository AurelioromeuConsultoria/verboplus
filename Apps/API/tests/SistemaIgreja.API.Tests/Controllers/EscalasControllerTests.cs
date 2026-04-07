using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class EscalasControllerTests
{
    private readonly Mock<IEscalaService> _serviceMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly EscalasController _controller;

    public EscalasControllerTests()
    {
        _controller = new EscalasController(_serviceMock.Object, _usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task GetById_ReturnsForbidden_WhenServiceThrowsUnauthorized()
    {
        SetUser((int)TipoUsuario.Portal, 10);
        _serviceMock.Setup(s => s.GetByIdAsync(5, 10, false))
            .ThrowsAsync(new UnauthorizedAccessException("Sem acesso"));

        var result = await _controller.GetById(5);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetMinhas_ReturnsUnauthorized_WhenUsuarioHasNoPessoa()
    {
        SetUser((int)TipoUsuario.Portal, 10);
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync((Usuario?)null);

        var result = await _controller.GetMinhas();

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenServiceSucceeds()
    {
        SetUser((int)TipoUsuario.Admin, 7);
        var dto = new CriarEscalaDto { EventoOcorrenciaId = 12, EquipeId = 3 };
        var created = new EscalaDto { Id = 20, EventoOcorrenciaId = 12, EquipeId = 3 };
        _serviceMock.Setup(s => s.CreateAsync(dto, 7, true)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().Be(created);
    }

    [Fact]
    public async Task ConfirmarItem_ReturnsOk_UsingUsuarioPessoaId()
    {
        SetUser((int)TipoUsuario.Portal, 15);
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 33 });
        var item = new EscalaItemDto { Id = 99, EscalaId = 5 };
        _serviceMock.Setup(s => s.ConfirmarItemAsync(5, 99, 15, false, 33))
            .ReturnsAsync(item);

        var result = await _controller.ConfirmarItem(5, 99);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(item);
    }

    [Fact]
    public async Task ProcessarLembretes_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal, 10);

        var result = await _controller.ProcessarLembretes();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task ProcessarLembretes_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin, 10);
        _serviceMock.Setup(s => s.EnviarLembretesPendentesAsync()).ReturnsAsync(4);

        var result = await _controller.ProcessarLembretes();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    private void SetUser(int tipoUsuarioId, int usuarioId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new("TipoUsuarioId", tipoUsuarioId.ToString())
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
