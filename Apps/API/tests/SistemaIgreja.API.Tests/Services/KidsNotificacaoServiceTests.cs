using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsNotificacaoServiceTests
{
    private readonly Mock<IKidsNotificacaoRepository> _repository = new();
    private readonly Mock<IResponsavelCriancaRepository> _responsavelCriancaRepository = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<ICurrentUserContext> _currentUserContext = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();
    private readonly Mock<IKidsPushNotificationService> _pushService = new();

    private readonly KidsNotificacaoService _service;

    public KidsNotificacaoServiceTests()
    {
        _currentUserContext.Setup(c => c.UserId).Returns(15);
        _usuarioRepository.Setup(r => r.GetByIdAsync(15))
            .ReturnsAsync(new Usuario { Id = 15, PessoaId = 30, Ativo = true, TipoUsuario = TipoUsuario.Admin });
        _authorizationService.Setup(a => a.EnsureLiderAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);

        _service = new KidsNotificacaoService(
            _repository.Object,
            _responsavelCriancaRepository.Object,
            _usuarioRepository.Object,
            _currentUserContext.Object,
            _authorizationService.Object,
            _pushService.Object);
    }

    [Fact]
    public async Task CriarAvisoAsync_DeveCriarAvisoGeralParaTodosResponsaveisAtivos()
    {
        _responsavelCriancaRepository.Setup(r => r.GetResponsavelIdsAtivosAsync())
            .ReturnsAsync(new List<int> { 30, 31, 31 });
        _repository.Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<KidsNotificacao>>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((KidsNotificacao?)null);

        await _service.CriarAvisoAsync(new CreateKidsAvisoRequest
        {
            Titulo = "Culto especial",
            Mensagem = "Hoje teremos programação especial.",
            Destino = "GERAL",
            Tipo = "AVISO_GERAL"
        });

        _repository.Verify(r => r.CreateRangeAsync(It.Is<IEnumerable<KidsNotificacao>>(items =>
            items.Count() == 2 &&
            items.All(i => i.CriancaPessoaId == null) &&
            items.All(i => i.Origem == "MANUAL") &&
            items.All(i => i.Titulo == "Culto especial"))), Times.Once);
        _pushService.Verify(p => p.SendToPessoasAsync(
            It.Is<IEnumerable<int>>(ids => ids.OrderBy(x => x).SequenceEqual(new[] { 30, 31 })),
            "Culto especial",
            "Hoje teremos programação especial.",
            It.IsAny<IReadOnlyDictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task CriarAvisoAsync_DeveCriarAvisoPorCriancaParaResponsaveisVinculados()
    {
        _responsavelCriancaRepository.Setup(r => r.GetByCriancaIdAsync(10))
            .ReturnsAsync(new List<ResponsavelCrianca>
            {
                new() { CriancaPessoaId = 10, ResponsavelPessoaId = 30, Ativo = true },
                new() { CriancaPessoaId = 10, ResponsavelPessoaId = 31, Ativo = true }
            });
        _repository.Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<KidsNotificacao>>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((KidsNotificacao?)null);

        await _service.CriarAvisoAsync(new CreateKidsAvisoRequest
        {
            Titulo = "Sala mudou",
            Mensagem = "Sua criança foi direcionada para outra sala.",
            Destino = "CRIANCA",
            Tipo = "AVISO_CRIANCA",
            CriancaPessoaIds = new List<int> { 10 }
        });

        _repository.Verify(r => r.CreateRangeAsync(It.Is<IEnumerable<KidsNotificacao>>(items =>
            items.Count() == 2 &&
            items.All(i => i.CriancaPessoaId == 10) &&
            items.All(i => i.Tipo == "AVISO_CRIANCA"))), Times.Once);
    }

    [Fact]
    public async Task CriarAvisoAsync_DeveCriarAvisoPorResponsavelSemCriancaAssociada()
    {
        _repository.Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<KidsNotificacao>>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((KidsNotificacao?)null);

        await _service.CriarAvisoAsync(new CreateKidsAvisoRequest
        {
            Titulo = "Retirada antecipada",
            Mensagem = "Procure a equipe ao final da reunião.",
            Destino = "RESPONSAVEL",
            Tipo = "AVISO_RESPONSAVEL",
            ResponsavelPessoaIds = new List<int> { 50 }
        });

        _repository.Verify(r => r.CreateRangeAsync(It.Is<IEnumerable<KidsNotificacao>>(items =>
            items.Count() == 1 &&
            items.Single().ResponsavelPessoaId == 50 &&
            items.Single().CriancaPessoaId == null)), Times.Once);
    }

    [Fact]
    public async Task MarcarComoLidoAsync_DeveRegistrarLeituraQuandoAvisoPertenceAoResponsavel()
    {
        var aviso = new KidsNotificacao
        {
            Id = 99,
            ResponsavelPessoaId = 30,
            Titulo = "Aviso",
            Tipo = "AVISO_GERAL",
            Origem = "MANUAL",
            Mensagem = "Teste",
            Status = "Enviado",
            DataCriacao = DateTime.UtcNow
        };

        _repository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(aviso);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<KidsNotificacao>()))
            .ReturnsAsync((KidsNotificacao item) => item);

        var result = await _service.MarcarComoLidoAsync(99);

        result.FoiLido.Should().BeTrue();
        result.LidoEm.Should().NotBeNull();
        _repository.Verify(r => r.UpdateAsync(It.Is<KidsNotificacao>(item => item.Id == 99 && item.LidoEm.HasValue)), Times.Once);
    }

    [Fact]
    public async Task CriarAvisoAsync_DeveBloquearQuandoUsuarioNaoForAdministrativo()
    {
        _authorizationService.Setup(a => a.EnsureLiderAsync())
            .ThrowsAsync(new UnauthorizedAccessException("Esta operação de Kids exige perfil de liderança ou administrativo."));

        await _service.Invoking(s => s.CriarAvisoAsync(new CreateKidsAvisoRequest
        {
            Titulo = "Aviso",
            Mensagem = "Teste",
            Destino = "GERAL"
        }))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*liderança ou administrativo*");
    }
}
