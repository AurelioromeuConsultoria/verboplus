using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class NotificacoesControllerTests
{
    private readonly Mock<INotificacaoUsuarioService> _serviceMock = new();
    private readonly Mock<ICurrentUserContext> _currentUserMock = new();
    private readonly NotificacoesController _controller;

    public NotificacoesControllerTests()
    {
        _controller = new NotificacoesController(_serviceMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task GetMinhas_ReturnsUnauthorized_WhenUserIsMissing()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns((int?)null);

        var result = await _controller.GetMinhas();

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetMinhas_ReturnsOk_WhenUserExists()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(15);
        _serviceMock.Setup(x => x.GetMinhasAsync(15, true, 5)).ReturnsAsync(
        [
            new NotificacaoUsuarioDto
            {
                Id = 1,
                Tipo = TipoNotificacaoUsuario.Geral,
                Titulo = "Alerta",
                Mensagem = "Teste",
                DataCriacao = DateTime.UtcNow
            }
        ]);

        var result = await _controller.GetMinhas(true, 5);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsUnauthorized_WhenUserIsMissing()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns((int?)null);

        var result = await _controller.GetUnreadCount();

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsOk_WhenUserExists()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(15);
        _serviceMock.Setup(x => x.GetUnreadCountAsync(15)).ReturnsAsync(4);

        var result = await _controller.GetUnreadCount();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task MarcarComoLida_ReturnsUnauthorized_WhenUserIsMissing()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns((int?)null);

        var result = await _controller.MarcarComoLida(1);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task MarcarComoLida_ReturnsOk_WhenSuccessful()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(15);
        _serviceMock.Setup(x => x.MarcarComoLidaAsync(1, 15)).ReturnsAsync(new NotificacaoUsuarioDto
        {
            Id = 1,
            Tipo = TipoNotificacaoUsuario.Geral,
            Titulo = "Lida",
            Mensagem = "Teste",
            DataCriacao = DateTime.UtcNow,
            DataLeitura = DateTime.UtcNow
        });

        var result = await _controller.MarcarComoLida(1);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task MarcarComoLida_ReturnsBadRequest_OnArgumentException()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(15);
        _serviceMock.Setup(x => x.MarcarComoLidaAsync(1, 15)).ThrowsAsync(new ArgumentException("erro"));

        var result = await _controller.MarcarComoLida(1);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task MarcarTodasComoLidas_ReturnsUnauthorized_WhenUserIsMissing()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns((int?)null);

        var result = await _controller.MarcarTodasComoLidas();

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task MarcarTodasComoLidas_ReturnsOk_WhenSuccessful()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(15);
        _serviceMock.Setup(x => x.MarcarTodasComoLidasAsync(15)).ReturnsAsync(6);

        var result = await _controller.MarcarTodasComoLidas();

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
