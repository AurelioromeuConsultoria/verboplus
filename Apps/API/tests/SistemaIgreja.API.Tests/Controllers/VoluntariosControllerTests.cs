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

public class VoluntariosControllerTests
{
    private readonly Mock<IVoluntarioService> _serviceMock = new();
    private readonly VoluntariosController _controller;

    public VoluntariosControllerTests()
    {
        _controller = new VoluntariosController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<VoluntarioDto>());
        var result = await _controller.GetAll();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByPessoa_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetVoluntariosPorPessoaAsync(1)).ReturnsAsync([new VoluntarioDto { Id = 2, PessoaId = 1, Nome = "Voluntário" }]);

        var result = await _controller.GetByPessoa(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByEquipe_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetVoluntariosPorEquipeAsync(3)).ReturnsAsync([new VoluntarioDto { Id = 2, EquipeId = 3, Nome = "Voluntário" }]);

        var result = await _controller.GetByEquipe(3);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_WhenNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((VoluntarioDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        SetUser((int)TipoUsuario.Admin);
        var dto = new CriarVoluntarioDto { PessoaId = 1, WhatsApp = "11999999999", Email = "v@x.com", EquipeId = 1, CargoId = 1 };
        var created = new VoluntarioDto { Id = 2, PessoaId = 1, Nome = "V1", WhatsApp = dto.WhatsApp, Email = dto.Email, EquipeId = 1, CargoId = 1, NomeEquipe = "Equipe", NomeCargo = "Cargo", DataCadastro = DateTime.UtcNow };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);
        var result = await _controller.Create(dto);
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);
        var result = await _controller.Create(new CriarVoluntarioDto());
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_OnArgumentException()
    {
        SetUser((int)TipoUsuario.Admin);
        var dto = new AtualizarVoluntarioDto { PessoaId = 1, WhatsApp = "11888", Email = null, EquipeId = 2, CargoId = 3 };
        _serviceMock.Setup(s => s.UpdateAsync(9, dto)).ThrowsAsync(new ArgumentException());
        var result = await _controller.Update(9, dto);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        var dto = new AtualizarVoluntarioDto { PessoaId = 1, WhatsApp = "11888", Email = "v@x.com", EquipeId = 2, CargoId = 3 };
        _serviceMock.Setup(s => s.UpdateAsync(9, dto)).ReturnsAsync(new VoluntarioDto { Id = 9, PessoaId = 1, Nome = "V1", EquipeId = 2, CargoId = 3 });

        var result = await _controller.Update(9, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.DeleteAsync(3)).Returns(Task.CompletedTask);
        var result = await _controller.Delete(3);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenVoluntarioHasEscalasRelacionadas()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock
            .Setup(s => s.DeleteAsync(3))
            .ThrowsAsync(new InvalidOperationException("Não é possível excluir este voluntário porque ele já possui escalas vinculadas."));

        var result = await _controller.Delete(3);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Não é possível excluir este voluntário porque ele já possui escalas vinculadas.");
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);
        var result = await _controller.Delete(3);
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
