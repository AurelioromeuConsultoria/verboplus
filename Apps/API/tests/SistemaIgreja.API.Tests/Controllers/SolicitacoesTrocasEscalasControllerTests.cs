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

public class SolicitacoesTrocasEscalasControllerTests
{
    private readonly Mock<ISolicitacaoTrocaEscalaService> _serviceMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly SolicitacoesTrocasEscalasController _controller;

    public SolicitacoesTrocasEscalasControllerTests()
    {
        _controller = new SolicitacoesTrocasEscalasController(_serviceMock.Object, _usuarioRepositoryMock.Object);
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
    public async Task Create_ReturnsOk_WhenServiceSucceeds()
    {
        SetUser((int)TipoUsuario.Portal, 10);
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Usuario { Id = 10, PessoaId = 20 });
        var dto = new CriarSolicitacaoTrocaEscalaDto { Motivo = "Viagem" };
        var created = new SolicitacaoTrocaEscalaDto { Id = 7, EscalaId = 3, EscalaItemId = 4 };
        _serviceMock.Setup(s => s.CreateAsync(3, 4, dto, 10, false, 20)).ReturnsAsync(created);

        var result = await _controller.Create(3, 4, dto);

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(created);
    }

    [Fact]
    public async Task Aprovar_ReturnsForbidden_WhenServiceThrowsUnauthorized()
    {
        SetUser((int)TipoUsuario.Portal, 10);
        _serviceMock.Setup(s => s.AprovarAsync(5, It.IsAny<AprovarSolicitacaoTrocaEscalaDto>(), 10, false))
            .ThrowsAsync(new UnauthorizedAccessException("Sem acesso"));

        var result = await _controller.Aprovar(5, new AprovarSolicitacaoTrocaEscalaDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetGerenciaveis_ReturnsOk()
    {
        SetUser((int)TipoUsuario.Admin, 1);
        _serviceMock.Setup(s => s.GetGerenciaveisAsync(1, true, 2, StatusSolicitacaoTrocaEscala.Pendente))
            .ReturnsAsync(new List<SolicitacaoTrocaEscalaDto>());

        var result = await _controller.GetGerenciaveis(2, StatusSolicitacaoTrocaEscala.Pendente);

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
