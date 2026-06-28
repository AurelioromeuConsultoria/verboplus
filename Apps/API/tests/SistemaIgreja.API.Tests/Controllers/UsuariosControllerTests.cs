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

public class UsuariosControllerTests
{
    private readonly Mock<IUsuarioService> _serviceMock = new();
    private readonly Mock<IUsuarioRepository> _repositoryMock = new();
    private readonly UsuariosController _controller;

    public UsuariosControllerTests()
    {
        _controller = new UsuariosController(_serviceMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UsuarioDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new UsuarioDto { Id = 1, EmailLogin = "admin@app.com" });

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenUsersExistAndUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);
        _repositoryMock.Setup(r => r.ExisteAlgumUsuarioAsync()).ReturnsAsync(true);

        var result = await _controller.Create(new CriarUsuarioDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Create_AllowsBootstrapWithoutAuthenticatedUser_WhenNoUsersExist()
    {
        _repositoryMock.Setup(r => r.ExisteAlgumUsuarioAsync()).ReturnsAsync(false);
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarUsuarioDto>())).ReturnsAsync(new UsuarioDto { Id = 1 });

        var result = await _controller.Create(new CriarUsuarioDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Update(10, new AtualizarUsuarioDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.UpdateAsync(10, It.IsAny<AtualizarUsuarioDto>())).ReturnsAsync(new UsuarioDto { Id = 10, EmailLogin = "admin@app.com" });

        var result = await _controller.Update(10, new AtualizarUsuarioDto());

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Delete(10);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);

        var result = await _controller.Delete(10);

        result.Should().BeOfType<NoContentResult>();
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
