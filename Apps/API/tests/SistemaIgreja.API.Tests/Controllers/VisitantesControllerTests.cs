using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Visitantes;
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
        _controller = new VisitantesController(
            _serviceMock.Object,
            _mensagemServiceMock.Object,
            Mock.Of<ILogger<VisitantesController>>());
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
    public async Task GetPaged_ReturnsOk_WithPagedResult()
    {
        SetUser((int)TipoUsuario.Admin);
        var query = new VisitantePagedQueryDto { Page = 1, PageSize = 10, Nome = "mar" };
        _serviceMock.Setup(s => s.GetPagedAsync(query)).ReturnsAsync(new PagedResultDto<VisitanteDto>
        {
            Items = [new VisitanteDto { Id = 1, Nome = "Marco", Telefone = "123", DataVisita = DateTime.UtcNow }],
            Total = 1,
            Page = 1,
            PageSize = 10
        });

        var result = await _controller.GetPaged(query);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByPessoa_ReturnsOk_WithItems()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetVisitantesPorPessoaAsync(7)).ReturnsAsync(
        [
            new VisitanteDto { Id = 1, Nome = "Marco", Telefone = "123", DataVisita = DateTime.UtcNow }
        ]);

        var result = await _controller.GetByPessoa(7);

        result.Result.Should().BeOfType<OkObjectResult>();
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
    public async Task Create_ReturnsRootCause_WhenUnexpectedError()
    {
        SetUser((int)TipoUsuario.Admin);
        var request = new CreateVisitanteRequest { Nome = "A", Telefone = "123", DataVisita = DateTime.UtcNow };
        var exception = new InvalidOperationException(
            "An error occurred while saving the entity changes. See the inner exception for details.",
            new Exception("duplicate key value violates unique constraint"));
        _serviceMock.Setup(s => s.CreateVisitanteAsync(request)).ThrowsAsync(exception);

        var result = await _controller.Create(request);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().BeEquivalentTo(new
        {
            message = "Erro ao criar visitante",
            error = exception.Message,
            detail = "duplicate key value violates unique constraint"
        });
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
    public async Task Update_ReturnsOk_WhenVisitanteExists()
    {
        SetUser((int)TipoUsuario.Admin);
        var dto = new AtualizarVisitanteDto { DataVisita = DateTime.UtcNow, Observacoes = "Atualizado" };
        _serviceMock.Setup(s => s.UpdateAsync(42, dto)).ReturnsAsync(new VisitanteDto
        {
            Id = 42,
            Nome = "Marco",
            Telefone = "123",
            DataVisita = dto.DataVisita
        });

        var result = await _controller.Update(42, dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegerarMensagens_ReturnsOk_WhenSuccessful()
    {
        SetUser((int)TipoUsuario.Admin);
        _mensagemServiceMock.Setup(s => s.RegerarMensagensParaVisitanteAsync(5)).ReturnsAsync(new RegerarMensagensResultDto
        {
            MensagensCanceladas = 2,
            MensagensCriadas = 3
        });

        var result = await _controller.RegerarMensagens(5);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegerarMensagens_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.RegerarMensagens(5);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
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
