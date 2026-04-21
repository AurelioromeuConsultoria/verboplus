using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class NotificacaoUsuarioServiceTests
{
    private readonly Mock<INotificacaoUsuarioRepository> _repositoryMock = new();
    private readonly NotificacaoUsuarioService _service;

    public NotificacaoUsuarioServiceTests()
    {
        _service = new NotificacaoUsuarioService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetMinhasAndGetUnreadCount_ReturnExpectedValues()
    {
        _repositoryMock.Setup(r => r.GetByUsuarioAsync(7, true, 5))
            .ReturnsAsync(
            [
                new NotificacaoUsuario
                {
                    Id = 1,
                    UsuarioId = 7,
                    Titulo = "Aviso",
                    Mensagem = "Mensagem",
                    Tipo = TipoNotificacaoUsuario.Geral,
                    DataCriacao = DateTime.Now
                }
            ]);
        _repositoryMock.Setup(r => r.GetUnreadCountAsync(7)).ReturnsAsync(3);

        var items = (await _service.GetMinhasAsync(7, true, 5)).ToList();
        items.Should().ContainSingle();
        items[0].Titulo.Should().Be("Aviso");
        (await _service.GetUnreadCountAsync(7)).Should().Be(3);
    }

    [Fact]
    public async Task MarcarComoLidaAsync_ThrowsWhenNotificationDoesNotBelongToUser()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new NotificacaoUsuario { Id = 1, UsuarioId = 8 });

        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarcarComoLidaAsync(1, 7));
    }

    [Fact]
    public async Task MarcarComoLidaAsync_UpdatesWhenUnread()
    {
        var entity = new NotificacaoUsuario
        {
            Id = 2,
            UsuarioId = 7,
            Titulo = "Alerta",
            Mensagem = "Nova mensagem",
            Tipo = TipoNotificacaoUsuario.Geral,
            DataCriacao = DateTime.Now
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(entity);

        var result = await _service.MarcarComoLidaAsync(2, 7);

        result.DataLeitura.Should().NotBeNull();
        _repositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_IgnoresInvalidData_AndPersistsValid()
    {
        await _service.CriarAsync(new CriarNotificacaoUsuarioDto { UsuarioId = 0, Titulo = "", Mensagem = "" });
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<NotificacaoUsuario>()), Times.Never);

        await _service.CriarAsync(new CriarNotificacaoUsuarioDto
        {
            UsuarioId = 3,
            Titulo = " Titulo ",
            Mensagem = " Mensagem ",
            Tipo = TipoNotificacaoUsuario.Geral
        });

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<NotificacaoUsuario>(n =>
            n.UsuarioId == 3 &&
            n.Titulo == "Titulo" &&
            n.Mensagem == "Mensagem")), Times.Once);
    }

    [Fact]
    public async Task CriarParaUsuariosAsync_DeduplicatesAndMarksAllRead()
    {
        await _service.CriarParaUsuariosAsync(
        [
            new CriarNotificacaoUsuarioDto { UsuarioId = 5, Titulo = "Aviso", Mensagem = "Msg", Tipo = TipoNotificacaoUsuario.Geral },
            new CriarNotificacaoUsuarioDto { UsuarioId = 5, Titulo = "Aviso", Mensagem = "Msg", Tipo = TipoNotificacaoUsuario.Geral }
        ]);

        _repositoryMock.Verify(r => r.CreateRangeAsync(It.Is<IEnumerable<NotificacaoUsuario>>(items => items.Count() == 1)), Times.Once);

        _repositoryMock.Setup(r => r.MarcarTodasComoLidasAsync(5, It.IsAny<DateTime>())).ReturnsAsync(4);
        (await _service.MarcarTodasComoLidasAsync(5)).Should().Be(4);
    }
}
