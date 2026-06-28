using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoCanaisContextuaisTests
{
    [Fact]
    public async Task PushProvider_EnviaParaPessoaDoKids()
    {
        var pushServiceMock = new Mock<IKidsPushNotificationService>();
        pushServiceMock
            .Setup(x => x.SendToPessoasAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        var provider = new ComunicacaoPushCanalProvider(
            pushServiceMock.Object,
            Options.Create(new FirebaseKidsPushOptions { CredentialsPath = "/tmp/firebase.json" }),
            Mock.Of<ILogger<ComunicacaoPushCanalProvider>>());

        var diagnostico = await provider.ValidarConfiguracaoAsync();
        var resultado = await provider.EnviarAsync(new ComunicacaoEntrega
        {
            Id = 12,
            Canal = CanalComunicacao.Push,
            DestinatarioPessoaId = 88,
            RemetenteResolvido = "Aviso Kids",
            ConteudoFinal = "Seu filho foi direcionado para outra sala."
        });

        diagnostico.Configurado.Should().BeTrue();
        resultado.Sucesso.Should().BeTrue();
        pushServiceMock.Verify(x => x.SendToPessoasAsync(
            It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(new[] { 88 })),
            "Aviso Kids",
            "Seu filho foi direcionado para outra sala.",
            It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task NotificacaoInternaProvider_CriaNotificacaoParaUsuarioAtivo()
    {
        var usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        var notificacaoRepositoryMock = new Mock<INotificacaoUsuarioRepository>();

        usuarioRepositoryMock.Setup(x => x.GetByPessoaIdAsync(55)).ReturnsAsync(new Usuario
        {
            Id = 9,
            PessoaId = 55,
            Ativo = true
        });
        notificacaoRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<NotificacaoUsuario>()))
            .ReturnsAsync((NotificacaoUsuario notificacao) => notificacao);

        var provider = new ComunicacaoNotificacaoInternaCanalProvider(
            usuarioRepositoryMock.Object,
            notificacaoRepositoryMock.Object);

        var resultado = await provider.EnviarAsync(new ComunicacaoEntrega
        {
            Id = 21,
            Canal = CanalComunicacao.NotificacaoInterna,
            DestinatarioPessoaId = 55,
            RemetenteResolvido = "Comunicado administrativo",
            ConteudoFinal = "Sua equipe recebeu uma atualização."
        });

        resultado.Sucesso.Should().BeTrue();
        notificacaoRepositoryMock.Verify(x => x.CreateAsync(It.Is<NotificacaoUsuario>(n =>
            n.UsuarioId == 9 &&
            n.Titulo == "Comunicado administrativo" &&
            n.Mensagem == "Sua equipe recebeu uma atualização.")), Times.Once);
    }

    [Fact]
    public async Task NotificacaoInternaProvider_FalhaQuandoPessoaNaoTemUsuarioAtivo()
    {
        var usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        var notificacaoRepositoryMock = new Mock<INotificacaoUsuarioRepository>();
        usuarioRepositoryMock.Setup(x => x.GetByPessoaIdAsync(55)).ReturnsAsync((Usuario?)null);

        var provider = new ComunicacaoNotificacaoInternaCanalProvider(
            usuarioRepositoryMock.Object,
            notificacaoRepositoryMock.Object);

        var resultado = await provider.EnviarAsync(new ComunicacaoEntrega
        {
            Canal = CanalComunicacao.NotificacaoInterna,
            DestinatarioPessoaId = 55,
            ConteudoFinal = "Mensagem"
        });

        resultado.Sucesso.Should().BeFalse();
        resultado.Mensagem.Should().Contain("Nenhum usuário ativo");
        notificacaoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<NotificacaoUsuario>()), Times.Never);
    }
}
