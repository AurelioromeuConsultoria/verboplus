using FluentAssertions;
using Moq;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoNotificacaoInternaCanalProviderTests
{
    [Fact]
    public async Task ValidarConfiguracaoAsync_IsAlwaysConfigured()
    {
        var sut = new ComunicacaoNotificacaoInternaCanalProvider(
            Mock.Of<IUsuarioRepository>(),
            Mock.Of<INotificacaoUsuarioRepository>());

        var result = await sut.ValidarConfiguracaoAsync();

        result.Configurado.Should().BeTrue();
    }

    [Fact]
    public async Task EnviarAsync_ReturnsFailureWhenPessoaIsNotResolved()
    {
        var sut = new ComunicacaoNotificacaoInternaCanalProvider(
            Mock.Of<IUsuarioRepository>(),
            Mock.Of<INotificacaoUsuarioRepository>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Contain("Destinatário pessoa");
    }

    [Fact]
    public async Task EnviarAsync_ReturnsFailureWhenNoActiveUserExists()
    {
        var usuarioRepository = new Mock<IUsuarioRepository>();
        usuarioRepository.Setup(r => r.GetByPessoaIdAsync(15)).ReturnsAsync((Usuario?)null);

        var sut = new ComunicacaoNotificacaoInternaCanalProvider(
            usuarioRepository.Object,
            Mock.Of<INotificacaoUsuarioRepository>());

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            DestinatarioPessoaId = 15,
            ConteudoFinal = "Mensagem"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Contain("Nenhum usuário ativo");
    }

    [Fact]
    public async Task EnviarAsync_CreatesNotificationForActiveUser()
    {
        var usuarioRepository = new Mock<IUsuarioRepository>();
        usuarioRepository.Setup(r => r.GetByPessoaIdAsync(15))
            .ReturnsAsync(new Usuario
            {
                Id = 9,
                Ativo = true,
                TipoUsuario = TipoUsuario.Portal,
                EmailLogin = "usuario@igreja.com"
            });

        var notificacaoRepository = new Mock<INotificacaoUsuarioRepository>();
        notificacaoRepository.Setup(r => r.CreateAsync(It.Is<NotificacaoUsuario>(n =>
                n.UsuarioId == 9 &&
                n.Tipo == TipoNotificacaoUsuario.Geral &&
                n.Titulo == "Comunicacao AppIgreja" &&
                n.Mensagem == "Mensagem interna")))
            .ReturnsAsync(new NotificacaoUsuario());

        var sut = new ComunicacaoNotificacaoInternaCanalProvider(
            usuarioRepository.Object,
            notificacaoRepository.Object);

        var result = await sut.EnviarAsync(new ComunicacaoEntrega
        {
            DestinatarioPessoaId = 15,
            ConteudoFinal = "Mensagem interna",
            RemetenteResolvido = " "
        });

        result.Sucesso.Should().BeTrue();
        notificacaoRepository.VerifyAll();
    }
}
