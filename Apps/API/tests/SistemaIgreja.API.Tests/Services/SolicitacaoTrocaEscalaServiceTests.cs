using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class SolicitacaoTrocaEscalaServiceTests
{
    private readonly Mock<ISolicitacaoTrocaEscalaRepository> _repositoryMock = new();
    private readonly Mock<IEscalaRepository> _escalaRepositoryMock = new();
    private readonly Mock<IEquipeRepository> _equipeRepositoryMock = new();
    private readonly Mock<IVoluntarioRepository> _voluntarioRepositoryMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<INotificacaoUsuarioService> _notificacaoUsuarioServiceMock = new();
    private readonly Mock<ILogger<SolicitacaoTrocaEscalaService>> _loggerMock = new();
    private readonly SolicitacaoTrocaEscalaService _service;

    public SolicitacaoTrocaEscalaServiceTests()
    {
        _service = new SolicitacaoTrocaEscalaService(
            _repositoryMock.Object,
            _escalaRepositoryMock.Object,
            _equipeRepositoryMock.Object,
            _voluntarioRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _notificacaoUsuarioServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenUsuarioNaoPodeGerenciarNemEhProprioVoluntario()
    {
        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            VoluntarioId = 8,
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            }
        });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(false);

        var act = () => _service.CreateAsync(10, 70, new CriarSolicitacaoTrocaEscalaDto
        {
            Motivo = "Preciso trocar"
        }, 15, false, 202);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não pode solicitar troca*");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenJaExisteSolicitacaoPendente()
    {
        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            VoluntarioId = 8,
            Status = StatusEscalaItem.Pendente,
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            }
        });
        _repositoryMock.Setup(r => r.GetPendenteByEscalaItemAsync(70)).ReturnsAsync(new SolicitacaoTrocaEscala
        {
            Id = 50,
            EscalaItemId = 70,
            Status = StatusSolicitacaoTrocaEscala.Pendente
        });

        var act = () => _service.CreateAsync(10, 70, new CriarSolicitacaoTrocaEscalaDto
        {
            Motivo = "Preciso trocar"
        }, 15, false, 101);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Já existe uma solicitação de troca pendente para este item.");
    }

    [Fact]
    public async Task AprovarAsync_Throws_WhenSubstitutoJaEstaEscaladoNoEvento()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(new SolicitacaoTrocaEscala
        {
            Id = 50,
            EscalaItemId = 70,
            Status = StatusSolicitacaoTrocaEscala.Pendente
        });
        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            VoluntarioId = 8,
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            }
        });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(true);
        _voluntarioRepositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(new Voluntario
        {
            Id = 9,
            PessoaId = 202,
            EquipeId = 3,
            Pessoa = new Pessoa { Id = 202, Nome = "Carlos Lima" }
        });
        _escalaRepositoryMock.Setup(r => r.GetConflitoPessoaNaEscalaAsync(10, 9, null)).ReturnsAsync(new EscalaItem
        {
            Id = 88,
            EscalaId = 10,
            VoluntarioId = 9
        });

        var act = () => _service.AprovarAsync(50, new AprovarSolicitacaoTrocaEscalaDto
        {
            VoluntarioSubstitutoId = 9,
            ObservacaoResposta = "Aprovado"
        }, 15, false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O voluntário substituto já está escalado neste evento.");
    }

    [Fact]
    public async Task AprovarAsync_MarcaOriginalComoSubstituidoECriaNovoItem()
    {
        var solicitacao = new SolicitacaoTrocaEscala
        {
            Id = 50,
            EscalaItemId = 70,
            Status = StatusSolicitacaoTrocaEscala.Pendente,
            Motivo = "Viagem",
            VoluntarioSolicitante = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            }
        };
        var itemOriginal = new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            CargoId = 5,
            Ordem = 2,
            Status = StatusEscalaItem.Pendente,
            VoluntarioId = 8,
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            }
        };

        _repositoryMock.SetupSequence(r => r.GetByIdAsync(50))
            .ReturnsAsync(solicitacao)
            .ReturnsAsync(() =>
            {
                solicitacao.EscalaItem = itemOriginal;
                itemOriginal.Equipe = new Equipe { Id = 3, Nome = "Louvor" };
                itemOriginal.Escala = new Escala
                {
                    Id = 10,
                    EventoOcorrenciaId = 500,
                    EventoOcorrencia = new EventoOcorrencia
                    {
                        Id = 500,
                        DataHoraInicio = new DateTime(2026, 4, 10, 19, 0, 0),
                        Evento = new Evento { Id = 11, Titulo = "Culto de Domingo" }
                    }
                };
                return solicitacao;
            });
        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(itemOriginal);
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(true);
        _voluntarioRepositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(new Voluntario
        {
            Id = 9,
            PessoaId = 202,
            EquipeId = 3,
            Pessoa = new Pessoa { Id = 202, Nome = "Carlos Lima" }
        });
        _escalaRepositoryMock.Setup(r => r.GetConflitoPessoaNaEscalaAsync(10, 9, null)).ReturnsAsync((EscalaItem?)null);
        _escalaRepositoryMock.Setup(r => r.UpdateItemAsync(It.IsAny<EscalaItem>()))
            .ReturnsAsync((EscalaItem x) => x);
        _escalaRepositoryMock.Setup(r => r.AddItemAsync(It.IsAny<EscalaItem>()))
            .ReturnsAsync((EscalaItem x) =>
            {
                x.Id = 99;
                return x;
            });
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<SolicitacaoTrocaEscala>()))
            .ReturnsAsync((SolicitacaoTrocaEscala x) => x);
        var result = await _service.AprovarAsync(50, new AprovarSolicitacaoTrocaEscalaDto
        {
            VoluntarioSubstitutoId = 9,
            ObservacaoResposta = "Tudo certo"
        }, 15, false);

        result.Status.Should().Be(StatusSolicitacaoTrocaEscala.Aprovada);
        result.VoluntarioSubstitutoId.Should().Be(9);
        itemOriginal.Status.Should().Be(StatusEscalaItem.Substituido);
        itemOriginal.RespondidoPorUsuarioId.Should().Be(15);

        _escalaRepositoryMock.Verify(r => r.AddItemAsync(It.Is<EscalaItem>(x =>
            x.EscalaId == 10 &&
            x.EquipeId == 3 &&
            x.CargoId == 5 &&
            x.VoluntarioId == 9 &&
            x.Ordem == 2 &&
            x.Status == StatusEscalaItem.Pendente)), Times.Once);
    }
}
