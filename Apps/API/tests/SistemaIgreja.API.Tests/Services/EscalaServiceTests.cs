using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class EscalaServiceTests
{
    private readonly Mock<IEscalaRepository> _escalaRepositoryMock = new();
    private readonly Mock<IEventoOcorrenciaRepository> _eventoOcorrenciaRepositoryMock = new();
    private readonly Mock<IVoluntarioRepository> _voluntarioRepositoryMock = new();
    private readonly Mock<IEscalaModeloRepository> _escalaModeloRepositoryMock = new();
    private readonly Mock<IIndisponibilidadeVoluntarioRepository> _indisponibilidadeRepositoryMock = new();
    private readonly Mock<IEquipeRepository> _equipeRepositoryMock = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<INotificacaoUsuarioService> _notificacaoUsuarioServiceMock = new();
    private readonly Mock<IComunicacaoAutomacaoService> _comunicacaoAutomacaoServiceMock = new();
    private readonly Mock<ILogger<EscalaService>> _loggerMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly EscalaService _service;

    public EscalaServiceTests()
    {
        _service = new EscalaService(
            _escalaRepositoryMock.Object,
            _eventoOcorrenciaRepositoryMock.Object,
            _voluntarioRepositoryMock.Object,
            _escalaModeloRepositoryMock.Object,
            _indisponibilidadeRepositoryMock.Object,
            _equipeRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _notificacaoUsuarioServiceMock.Object,
            _comunicacaoAutomacaoServiceMock.Object,
            _loggerMock.Object,
            _auditLogServiceMock.Object);
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenUsuarioNaoGerenciaEquipe()
    {
        var escala = new Escala
        {
            Id = 10,
            EquipeId = 3,
            Status = StatusEscala.Rascunho,
            Itens = new List<EscalaItem>()
        };

        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(escala);
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(99, 3)).ReturnsAsync(false);

        var act = () => _service.AddItemAsync(10, new CriarEscalaItemDto
        {
            EquipeId = 3,
            VoluntarioId = 8
        }, 99, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenUsuarioNaoGerenciaEquipeDaEscala()
    {
        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Escala
        {
            Id = 10,
            EquipeId = 3
        });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(false);

        var act = () => _service.GetByIdAsync(10, 15, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task GetAllByEventoOcorrenciaAsync_RetornaSomenteEscalasDasEquipesGeridas()
    {
        _escalaRepositoryMock.Setup(r => r.GetAllByEventoOcorrenciaAsync(500)).ReturnsAsync(new List<Escala>
        {
            new() { Id = 1, EquipeId = 3, Equipe = new Equipe { Id = 3, Nome = "Louvor" } },
            new() { Id = 2, EquipeId = 4, Equipe = new Equipe { Id = 4, Nome = "Recepção" } }
        });
        _equipeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Equipe>
        {
            new() { Id = 3, Nome = "Louvor", LiderUsuarioId = 15 },
            new() { Id = 4, Nome = "Recepção", LiderUsuarioId = 99 }
        });

        var result = (await _service.GetAllByEventoOcorrenciaAsync(500, 15, false)).ToList();

        result.Should().HaveCount(1);
        result[0].EquipeId.Should().Be(3);
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenHaConflitoESemForcar()
    {
        var escala = new Escala
        {
            Id = 10,
            EquipeId = 3,
            Status = StatusEscala.Rascunho,
            Itens = new List<EscalaItem>()
        };

        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(escala);
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(true);
        _voluntarioRepositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new Voluntario
        {
            Id = 8,
            PessoaId = 101,
            EquipeId = 3
        });
        _escalaRepositoryMock.Setup(r => r.GetConflitoPessoaNaEscalaAsync(10, 8, null)).ReturnsAsync(new EscalaItem
        {
            Id = 99,
            Voluntario = new Voluntario
            {
                Id = 8,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            },
            Equipe = new Equipe { Id = 7, Nome = "Louvor" }
        });

        var act = () => _service.AddItemAsync(10, new CriarEscalaItemDto
        {
            EquipeId = 3,
            VoluntarioId = 8,
            ForcarConflito = false
        }, 15, false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Ana Souza já está escalado neste evento pela equipe 'Louvor'.");
    }

    [Fact]
    public async Task AddItemAsync_Throws_WhenForcaConflitoSemMotivo()
    {
        var escala = new Escala
        {
            Id = 10,
            EquipeId = 3,
            Status = StatusEscala.Rascunho,
            Itens = new List<EscalaItem>()
        };

        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(escala);
        _voluntarioRepositoryMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(new Voluntario
        {
            Id = 8,
            PessoaId = 101,
            EquipeId = 3
        });
        _escalaRepositoryMock.Setup(r => r.GetConflitoPessoaNaEscalaAsync(10, 8, null)).ReturnsAsync(new EscalaItem
        {
            Id = 99,
            Voluntario = new Voluntario { Id = 8, Pessoa = new Pessoa { Nome = "Ana Souza" } },
            Equipe = new Equipe { Id = 7, Nome = "Louvor" }
        });

        var act = () => _service.AddItemAsync(10, new CriarEscalaItemDto
        {
            EquipeId = 3,
            VoluntarioId = 8,
            ForcarConflito = true,
            MotivoExcecao = "   "
        }, 1, true);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Motivo da exceção é obrigatório ao forçar conflito.");
    }

    [Fact]
    public async Task GetSugestoesAsync_Throws_WhenUsuarioNaoGerenciaEquipe()
    {
        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Escala
        {
            Id = 10,
            EquipeId = 3
        });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(false);

        var act = () => _service.GetSugestoesAsync(10, 3, 15, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task GetSugestoesAsync_Throws_WhenEquipeNaoPertenceAEscala()
    {
        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Escala
        {
            Id = 10,
            EquipeId = 3
        });

        var act = () => _service.GetSugestoesAsync(10, 4, 15, true);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Equipe inválida para esta escala");
    }

    [Fact]
    public async Task ConfirmarItemAsync_Throws_WhenUsuarioNaoEhLiderAdminNemOProprioVoluntario()
    {
        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            }
        });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(false);

        var act = () => _service.ConfirmarItemAsync(10, 70, 15, false, 202);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*responder esta escala*");
    }

    [Fact]
    public async Task PublicarAsync_Throws_WhenEscalaNaoTemItens()
    {
        _escalaRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Escala
        {
            Id = 10,
            EquipeId = 3,
            Status = StatusEscala.Rascunho,
            Itens = new List<EscalaItem>()
        });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(true);

        var act = () => _service.PublicarAsync(10, 15, false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Não é possível publicar escala sem itens");
    }

    [Fact]
    public async Task PublicarAsync_AtualizaStatusEResetaItensNaoConfirmados()
    {
        var itemConfirmado = new EscalaItem
        {
            Id = 1,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Confirmado,
            DataConfirmacao = new DateTime(2026, 4, 1, 10, 0, 0),
            Voluntario = new Voluntario { Id = 20, PessoaId = 100, Pessoa = new Pessoa { Id = 100, Nome = "Ana" } }
        };
        var itemRecusado = new EscalaItem
        {
            Id = 2,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Recusado,
            DataRecusa = new DateTime(2026, 4, 1, 11, 0, 0),
            MotivoRecusa = "Sem disponibilidade",
            RespondidoPorUsuarioId = 55,
            Voluntario = new Voluntario { Id = 21, PessoaId = 101, Pessoa = new Pessoa { Id = 101, Nome = "Bruno" } }
        };
        var escala = new Escala
        {
            Id = 10,
            EquipeId = 3,
            Status = StatusEscala.Rascunho,
            Itens = new List<EscalaItem> { itemConfirmado, itemRecusado }
        };

        _escalaRepositoryMock.SetupSequence(r => r.GetByIdAsync(10))
            .ReturnsAsync(escala)
            .ReturnsAsync(new Escala
            {
                Id = 10,
                EquipeId = 3,
                Status = StatusEscala.Publicada,
                DataPublicacao = escala.DataPublicacao,
                Itens = new List<EscalaItem> { itemConfirmado, itemRecusado }
            });
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(true);
        _escalaRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Escala>()))
            .ReturnsAsync((Escala x) => x);

        var result = await _service.PublicarAsync(10, 15, false);

        result.Status.Should().Be(StatusEscala.Publicada);
        escala.Status.Should().Be(StatusEscala.Publicada);
        escala.DataPublicacao.Should().NotBeNull();
        itemConfirmado.Status.Should().Be(StatusEscalaItem.Confirmado);
        itemConfirmado.DataConfirmacao.Should().NotBeNull();
        itemRecusado.Status.Should().Be(StatusEscalaItem.Pendente);
        itemRecusado.DataConvite.Should().NotBeNull();
        itemRecusado.DataConfirmacao.Should().BeNull();
        itemRecusado.DataRecusa.Should().BeNull();
        itemRecusado.MotivoRecusa.Should().BeNull();
        itemRecusado.RespondidoPorUsuarioId.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmarItemAsync_AtualizaStatusEDisparaNotificacaoParaLider()
    {
        var item = new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Pendente,
            Equipe = new Equipe { Id = 3, Nome = "Louvor", LiderUsuarioId = 40 },
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            },
            Escala = new Escala
            {
                Id = 10,
                EventoOcorrenciaId = 500,
                EventoOcorrencia = new EventoOcorrencia
                {
                    Id = 500,
                    DataHoraInicio = new DateTime(2026, 4, 20, 19, 0, 0),
                    Evento = new Evento { Id = 11, Titulo = "Culto" }
                }
            }
        };

        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(item);
        _escalaRepositoryMock.Setup(r => r.UpdateItemAsync(It.IsAny<EscalaItem>()))
            .ReturnsAsync((EscalaItem x) => x);
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(false);

        var result = await _service.ConfirmarItemAsync(10, 70, 15, false, 101);

        result.Status.Should().Be(StatusEscalaItem.Confirmado);
        item.Status.Should().Be(StatusEscalaItem.Confirmado);
        item.DataConfirmacao.Should().NotBeNull();
        item.DataRecusa.Should().BeNull();
        item.MotivoRecusa.Should().BeNull();
        item.RespondidoPorUsuarioId.Should().Be(15);

        _notificacaoUsuarioServiceMock.Verify(n => n.CriarAsync(It.Is<CriarNotificacaoUsuarioDto>(dto =>
            dto.UsuarioId == 40 &&
            dto.Tipo == TipoNotificacaoUsuario.Escala &&
            dto.Titulo == "Escala confirmada" &&
            dto.Link != null &&
            dto.Link.Contains("/voluntariado/escalas/ocorrencia/500/equipe/3"))), Times.Once);
    }

    [Fact]
    public async Task RecusarItemAsync_AtualizaStatusEMotivoEDisparaNotificacaoParaLider()
    {
        var item = new EscalaItem
        {
            Id = 70,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Pendente,
            Equipe = new Equipe { Id = 3, Nome = "Louvor", LiderUsuarioId = 40 },
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            },
            Escala = new Escala
            {
                Id = 10,
                EventoOcorrenciaId = 500,
                EventoOcorrencia = new EventoOcorrencia
                {
                    Id = 500,
                    DataHoraInicio = new DateTime(2026, 4, 20, 19, 0, 0),
                    Evento = new Evento { Id = 11, Titulo = "Culto" }
                }
            }
        };

        _escalaRepositoryMock.Setup(r => r.GetItemByIdAsync(70)).ReturnsAsync(item);
        _escalaRepositoryMock.Setup(r => r.UpdateItemAsync(It.IsAny<EscalaItem>()))
            .ReturnsAsync((EscalaItem x) => x);
        _equipeRepositoryMock.Setup(r => r.IsLiderUsuarioDaEquipeAsync(15, 3)).ReturnsAsync(false);

        var result = await _service.RecusarItemAsync(10, 70, "Tenho compromisso", 15, false, 101);

        result.Status.Should().Be(StatusEscalaItem.Recusado);
        item.Status.Should().Be(StatusEscalaItem.Recusado);
        item.DataRecusa.Should().NotBeNull();
        item.DataConfirmacao.Should().BeNull();
        item.MotivoRecusa.Should().Be("Tenho compromisso");
        item.RespondidoPorUsuarioId.Should().Be(15);

        _notificacaoUsuarioServiceMock.Verify(n => n.CriarAsync(It.Is<CriarNotificacaoUsuarioDto>(dto =>
            dto.UsuarioId == 40 &&
            dto.Tipo == TipoNotificacaoUsuario.Escala &&
            dto.Titulo == "Escala recusada" &&
            dto.Mensagem.Contains("Tenho compromisso"))), Times.Once);
    }

    [Fact]
    public async Task EnviarLembretesPendentesAsync_EnviaLembrete24HorasEApenasParaItensElegiveis()
    {
        var referencia = new DateTime(2026, 4, 2, 12, 0, 0);
        var itemElegivel = new EscalaItem
        {
            Id = 1,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Pendente,
            Voluntario = new Voluntario
            {
                Id = 8,
                PessoaId = 101,
                Pessoa = new Pessoa { Id = 101, Nome = "Ana Souza" }
            },
            Escala = new Escala
            {
                Id = 10,
                EventoOcorrencia = new EventoOcorrencia
                {
                    Id = 500,
                    DataHoraInicio = referencia.AddHours(24),
                    Evento = new Evento { Id = 11, Titulo = "Culto" }
                }
            }
        };
        var itemSemUsuario = new EscalaItem
        {
            Id = 2,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Pendente,
            Voluntario = new Voluntario
            {
                Id = 9,
                PessoaId = 202,
                Pessoa = new Pessoa { Id = 202, Nome = "Bruno Lima" }
            },
            Escala = new Escala
            {
                Id = 10,
                EventoOcorrencia = new EventoOcorrencia
                {
                    Id = 501,
                    DataHoraInicio = referencia.AddHours(24),
                    Evento = new Evento { Id = 12, Titulo = "Ensaio" }
                }
            }
        };
        var itemRecusado = new EscalaItem
        {
            Id = 3,
            EscalaId = 10,
            EquipeId = 3,
            Status = StatusEscalaItem.Recusado,
            Voluntario = new Voluntario
            {
                Id = 10,
                PessoaId = 303,
                Pessoa = new Pessoa { Id = 303, Nome = "Carla" }
            },
            Escala = new Escala
            {
                Id = 10,
                EventoOcorrencia = new EventoOcorrencia
                {
                    Id = 502,
                    DataHoraInicio = referencia.AddHours(24),
                    Evento = new Evento { Id = 13, Titulo = "Reunião" }
                }
            }
        };

        _escalaRepositoryMock.Setup(r => r.GetItensComOcorrenciaNoPeriodoAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                null,
                null))
            .ReturnsAsync(new List<EscalaItem> { itemElegivel, itemSemUsuario, itemRecusado });
        _usuarioRepositoryMock.Setup(r => r.GetByPessoaIdAsync(101)).ReturnsAsync(new Usuario
        {
            Id = 40,
            PessoaId = 101,
            Ativo = true
        });
        _usuarioRepositoryMock.Setup(r => r.GetByPessoaIdAsync(202)).ReturnsAsync((Usuario?)null);
        _escalaRepositoryMock.Setup(r => r.UpdateItemAsync(It.IsAny<EscalaItem>()))
            .ReturnsAsync((EscalaItem x) => x);
        _comunicacaoAutomacaoServiceMock
            .Setup(s => s.ExecutarLembretesOperacionaisAsync(It.IsAny<IEnumerable<ComunicacaoLembreteOperacionalRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var total = await _service.EnviarLembretesPendentesAsync(referencia);

        total.Should().Be(1);
        itemElegivel.DataLembrete24HorasEnviado.Should().Be(referencia);
        itemRecusado.DataLembrete24HorasEnviado.Should().BeNull();
        _comunicacaoAutomacaoServiceMock.Verify(n => n.ExecutarLembretesOperacionaisAsync(It.Is<IEnumerable<ComunicacaoLembreteOperacionalRequest>>(lista =>
            lista.Count() == 1 &&
            lista.First().PessoaId == 101 &&
            lista.First().Titulo == "Lembrete: escala em 24 horas"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPlanejamentoMensalAsync_OrdenaVoluntariosPorNomeMesmoComTotaisDiferentes()
    {
        var equipe = new Equipe { Id = 3, Nome = "Hospitalidade" };
        var cargo = new Cargo { Id = 7, Nome = "Membro" };
        var ocorrencia1 = new EventoOcorrencia
        {
            Id = 501,
            EventoId = 11,
            Evento = new Evento { Id = 11, Titulo = "Culto" },
            DataHoraInicio = new DateTime(2026, 6, 7, 19, 30, 0)
        };
        var ocorrencia2 = new EventoOcorrencia
        {
            Id = 502,
            EventoId = 11,
            Evento = new Evento { Id = 11, Titulo = "Culto" },
            DataHoraInicio = new DateTime(2026, 6, 14, 19, 30, 0)
        };
        var ana = new Voluntario
        {
            Id = 10,
            PessoaId = 100,
            Pessoa = new Pessoa { Id = 100, Nome = "Ana Souza" },
            EquipeId = 3,
            Equipe = equipe,
            CargoId = 7,
            Cargo = cargo
        };
        var bruno = new Voluntario
        {
            Id = 11,
            PessoaId = 101,
            Pessoa = new Pessoa { Id = 101, Nome = "Bruno Lima" },
            EquipeId = 3,
            Equipe = equipe,
            CargoId = 7,
            Cargo = cargo
        };
        var itens = new List<EscalaItem>
        {
            new()
            {
                Id = 1,
                EscalaId = 20,
                EquipeId = 3,
                Equipe = equipe,
                CargoId = 7,
                Cargo = cargo,
                PessoaId = bruno.PessoaId,
                Pessoa = bruno.Pessoa,
                Voluntario = bruno,
                Escala = new Escala { Id = 20, EventoOcorrenciaId = 501, EventoOcorrencia = ocorrencia1 }
            },
            new()
            {
                Id = 2,
                EscalaId = 21,
                EquipeId = 3,
                Equipe = equipe,
                CargoId = 7,
                Cargo = cargo,
                PessoaId = bruno.PessoaId,
                Pessoa = bruno.Pessoa,
                Voluntario = bruno,
                Escala = new Escala { Id = 21, EventoOcorrenciaId = 502, EventoOcorrencia = ocorrencia2 }
            },
            new()
            {
                Id = 3,
                EscalaId = 20,
                EquipeId = 3,
                Equipe = equipe,
                CargoId = 7,
                Cargo = cargo,
                PessoaId = ana.PessoaId,
                Pessoa = ana.Pessoa,
                Voluntario = ana,
                Escala = new Escala { Id = 20, EventoOcorrenciaId = 501, EventoOcorrencia = ocorrencia1 }
            }
        };

        _eventoOcorrenciaRepositoryMock.Setup(r => r.GetByPeriodoAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                null))
            .ReturnsAsync(new List<EventoOcorrencia> { ocorrencia1, ocorrencia2 });
        _escalaRepositoryMock.Setup(r => r.GetItensComOcorrenciaNoPeriodoAsync(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                null,
                null))
            .ReturnsAsync(itens);
        _voluntarioRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Voluntario> { bruno, ana });

        var result = await _service.GetPlanejamentoMensalAsync(1, true, 2026, 6);

        result.Voluntarios.Select(v => v.Nome).Should().Equal("Ana Souza", "Bruno Lima");
        result.Voluntarios.Select(v => v.TotalEscalas).Should().Equal(1, 2);
    }
}
