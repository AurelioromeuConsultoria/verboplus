using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Pessoas;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class PessoasControllerTests
{
    private readonly Mock<IPessoaService> _serviceMock = new();
    private readonly Mock<ICurrentUserContext> _currentUserMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<ILogger<PessoasController>> _loggerMock = new();
    private readonly PessoasController _controller;

    public PessoasControllerTests()
    {
        _controller = new PessoasController(_serviceMock.Object, _currentUserMock.Object, _usuarioRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenAuthenticated()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<PessoaDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WithPagedResult()
    {
        var query = new PessoaPagedQueryDto { Page = 2, PageSize = 5, Nome = "mar" };
        _serviceMock.Setup(s => s.GetPagedAsync(query)).ReturnsAsync(new PagedResultDto<PessoaDto>
        {
            Items = [new PessoaDto { Id = 1, Nome = "Marco" }],
            Total = 1,
            Page = 2,
            PageSize = 5
        });

        var result = await _controller.GetPaged(query);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenPessoaExists()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new PessoaDto { Id = 1, Nome = "Marco" });

        var result = await _controller.GetById(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenPessoaDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((PessoaDto?)null);

        var result = await _controller.GetById(99);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Get360_ReturnsOk_WhenPessoaExists()
    {
        _serviceMock.Setup(s => s.Get360Async(1)).ReturnsAsync(new Pessoa360Dto { Pessoa = new PessoaDto { Id = 1, Nome = "Marco" } });

        var result = await _controller.Get360(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Get360_ReturnsNotFound_WhenPessoaDoesNotExist()
    {
        _serviceMock.Setup(s => s.Get360Async(1)).ReturnsAsync((Pessoa360Dto?)null);

        var result = await _controller.Get360(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Create(new CriarPessoaDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarPessoaDto>())).ReturnsAsync(new PessoaDto { Id = 1 });

        var result = await _controller.Create(new CriarPessoaDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<AtualizarPessoaDto>())).ReturnsAsync(new PessoaDto { Id = 1, Nome = "Atualizada" });

        var result = await _controller.Update(1, new AtualizarPessoaDto());

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenPessoaDoesNotExist()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<AtualizarPessoaDto>())).ThrowsAsync(new ArgumentException());

        var result = await _controller.Update(1, new AtualizarPessoaDto());

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

    [Fact]
    public async Task GetMe_ReturnsOk_WhenCurrentUserHasPessoa()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(10);
        _usuarioRepositoryMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new Usuario { Id = 10, PessoaId = 44 });
        _serviceMock.Setup(x => x.GetByIdAsync(44)).ReturnsAsync(new PessoaDto { Id = 44, Nome = "Marco" });

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateMe_ReturnsOk_WhenCurrentUserHasPessoa()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(10);
        _usuarioRepositoryMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new Usuario { Id = 10, PessoaId = 44 });
        _serviceMock.Setup(x => x.UpdateMinhaPessoaAsync(44, It.IsAny<AtualizarMinhaPessoaDto>()))
            .ReturnsAsync(new PessoaDto { Id = 44, Nome = "Marco Atualizado" });

        var result = await _controller.UpdateMe(new AtualizarMinhaPessoaDto { Nome = "Marco Atualizado" });

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMe_ReturnsUnauthorized_WhenCurrentUserHasNoPessoa()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(10);
        _usuarioRepositoryMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync((Usuario?)null);

        var result = await _controller.GetMe();

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task UpdateMe_ReturnsUnauthorized_WhenCurrentUserHasNoPessoa()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(10);
        _usuarioRepositoryMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync((Usuario?)null);

        var result = await _controller.UpdateMe(new AtualizarMinhaPessoaDto { Nome = "Marco" });

        result.Result.Should().BeOfType<UnauthorizedResult>();
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
        _currentUserMock.SetupGet(x => x.UserId).Returns(10);
    }
}
