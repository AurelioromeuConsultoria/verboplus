using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class PessoasPerfisControllerTests
{
    private readonly Mock<IPessoaPerfilService> _serviceMock = new();
    private readonly PessoasPerfisController _controller;

    public PessoasPerfisControllerTests()
    {
        _controller = new PessoasPerfisController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenAuthenticated()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<PessoaPerfilDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenPerfilExists()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new PessoaPerfilDto { Id = 1 });

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenPerfilDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((PessoaPerfilDto?)null);

        var result = await _controller.GetById(99);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByPessoa_ReturnsOk_WithPerfis()
    {
        _serviceMock.Setup(s => s.GetPerfisPorPessoaAsync(7)).ReturnsAsync([new PessoaPerfilDto { Id = 1, PessoaId = 7 }]);

        var result = await _controller.GetByPessoa(7);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Create(new CriarPessoaPerfilDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarPessoaPerfilDto>())).ReturnsAsync(new PessoaPerfilDto { Id = 1 });

        var result = await _controller.Create(new CriarPessoaPerfilDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<AtualizarPessoaPerfilDto>())).ReturnsAsync(new PessoaPerfilDto { Id = 1 });

        var result = await _controller.Update(1, new AtualizarPessoaPerfilDto());

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenPerfilDoesNotExist()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<AtualizarPessoaPerfilDto>())).ThrowsAsync(new ArgumentException());

        var result = await _controller.Update(1, new AtualizarPessoaPerfilDto());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    private void SetUser(int tipoUsuarioId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "10"),
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
