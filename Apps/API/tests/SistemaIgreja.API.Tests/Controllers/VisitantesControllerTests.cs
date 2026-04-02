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

public class VisitantesControllerTests
{
    private readonly Mock<IVisitanteService> _serviceMock;
    private readonly Mock<IMensagemAgendadaService> _mensagemServiceMock;
    private readonly VisitantesController _controller;

    public VisitantesControllerTests()
    {
        _serviceMock = new Mock<IVisitanteService>();
        _mensagemServiceMock = new Mock<IMensagemAgendadaService>();
        _controller = new VisitantesController(_serviceMock.Object, _mensagemServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<VisitanteDto>
        {
            new() { Id = 1, Nome = "A", Telefone = "123", DataVisita = DateTime.UtcNow },
            new() { Id = 2, Nome = "B", Telefone = "456", DataVisita = DateTime.UtcNow }
        });

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = result.Result as OkObjectResult;
        ok!.Value.Should().BeAssignableTo<IEnumerable<VisitanteDto>>();
        (ok.Value as IEnumerable<VisitanteDto>)!.Should().HaveCount(2);
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
    public async Task GetById_ReturnsNotFound_WhenNull()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((VisitanteDto?)null);

        var result = await _controller.GetById(99);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenOk()
    {
        SetUser((int)TipoUsuario.Admin);
        var request = new CreateVisitanteRequest { Nome = "A", Telefone = "123", DataVisita = DateTime.UtcNow };
        var response = new VisitanteResponse { VisitanteId = 10, Nome = request.Nome, Telefone = request.Telefone, DataVisita = request.DataVisita ?? DateTime.UtcNow };
        _serviceMock.Setup(s => s.CreateVisitanteAsync(request)).ReturnsAsync(response);

        var result = await _controller.Create(request);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdAt = result.Result as CreatedAtActionResult;
        createdAt!.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Create(new CreateVisitanteRequest());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_OnArgumentException()
    {
        SetUser((int)TipoUsuario.Admin);
        var dto = new AtualizarVisitanteDto { DataVisita = DateTime.UtcNow, Observacoes = "Teste" };
        _serviceMock.Setup(s => s.UpdateAsync(42, dto)).ThrowsAsync(new ArgumentException("Visitante não encontrado"));

        var result = await _controller.Update(42, dto);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.DeleteAsync(5)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(5);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.Delete(5);

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
