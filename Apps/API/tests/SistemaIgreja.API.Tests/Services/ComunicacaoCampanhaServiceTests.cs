using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoCampanhaServiceTests
{
    private readonly Mock<IComunicacaoCampanhaRepository> _repositoryMock = new();
    private readonly Mock<IComunicacaoEntregaRepository> _entregaRepositoryMock = new();
    private readonly Mock<IComunicacaoTemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IComunicacaoPreferenciaService> _preferenciaServiceMock = new();
    private readonly Mock<IComunicacaoAudienceResolver> _audienceResolverMock = new();
    private readonly Mock<ICurrentUserContext> _currentUserMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly Mock<ILogger<ComunicacaoCampanhaService>> _loggerMock = new();
    private readonly ComunicacaoCampanhaService _service;

    public ComunicacaoCampanhaServiceTests()
    {
        _currentUserMock.SetupGet(x => x.UserId).Returns(42);
        _auditLogServiceMock
            .Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        _service = new ComunicacaoCampanhaService(
            _repositoryMock.Object,
            _entregaRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _preferenciaServiceMock.Object,
            _audienceResolverMock.Object,
            _currentUserMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_GeraEntregasComTemplatesEVariaveis()
    {
        var dto = new CriarComunicacaoCampanhaDto
        {
            Nome = "  Boas-vindas Visitantes  ",
            Objetivo = " relacionamento ",
            PublicoAlvo = " visitantes ",
            Canais =
            [
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.WhatsApp,
                    TemplateId = 10,
                    Prioridade = 1
                },
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.Email,
                    TemplateId = 11,
                    Prioridade = 2
                }
            ]
        };

        var createdCampaign = new ComunicacaoCampanha
        {
            Id = 7,
            Nome = "Boas-vindas Visitantes",
            Objetivo = "relacionamento",
            PublicoAlvo = "visitantes",
            Status = StatusComunicacaoCampanha.Rascunho,
            Origem = TipoOrigemComunicacao.Manual,
            CriadoPorUsuarioId = 42,
            DataCriacao = DateTime.UtcNow,
            Canais =
            [
                new ComunicacaoCampanhaCanal { Id = 1, ComunicacaoCampanhaId = 7, Canal = CanalComunicacao.WhatsApp, TemplateId = 10, Prioridade = 1 },
                new ComunicacaoCampanhaCanal { Id = 2, ComunicacaoCampanhaId = 7, Canal = CanalComunicacao.Email, TemplateId = 11, Prioridade = 2 }
            ]
        };

        List<ComunicacaoEntrega>? entregasCriadas = null;

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>()))
            .ReturnsAsync(createdCampaign);
        _repositoryMock
            .Setup(x => x.GetByIdAsync(createdCampaign.Id))
            .ReturnsAsync(() =>
            {
                createdCampaign.Entregas = entregasCriadas ?? [];
                return createdCampaign;
            });

        _audienceResolverMock
            .Setup(x => x.ResolveAsync("visitantes"))
            .ReturnsAsync(
            [
                new ComunicacaoDestinatario
                {
                    PessoaId = 101,
                    VisitanteId = 201,
                    Nome = "Maria Souza",
                    PrimeiroNome = "Maria",
                    WhatsApp = "5511999999999",
                    Email = "maria@email.com"
                }
            ]);

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(new ComunicacaoTemplate
        {
            Id = 10,
            Nome = "WA boas-vindas",
            Canal = CanalComunicacao.WhatsApp,
            Corpo = "Olá {PrimeiroNome}, campanha {Campanha}",
            DataCriacao = DateTime.UtcNow
        });
        _templateRepositoryMock.Setup(x => x.GetByIdAsync(11)).ReturnsAsync(new ComunicacaoTemplate
        {
            Id = 11,
            Nome = "Email boas-vindas",
            Canal = CanalComunicacao.Email,
            Assunto = "Bem-vinda, {PrimeiroNome}",
            Corpo = "Oi {Nome}, público {PublicoAlvo}",
            CorpoHtml = "<p>Oi {PrimeiroNome}</p>",
            DataCriacao = DateTime.UtcNow
        });

        _entregaRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<ComunicacaoEntrega>>()))
            .ReturnsAsync((IEnumerable<ComunicacaoEntrega> entregas) =>
            {
                entregasCriadas = entregas.ToList();
                return entregasCriadas;
            });

        var result = await _service.CreateAsync(dto);

        entregasCriadas.Should().NotBeNull();
        entregasCriadas.Should().HaveCount(2);

        var whatsapp = entregasCriadas!.Single(x => x.Canal == CanalComunicacao.WhatsApp);
        whatsapp.DestinoResolvido.Should().Be("5511999999999");
        whatsapp.ConteudoFinal.Should().Be("Olá Maria, campanha Boas-vindas Visitantes");
        whatsapp.ChaveDedupe.Should().Be("7:WhatsApp:101:201");

        var email = entregasCriadas.Single(x => x.Canal == CanalComunicacao.Email);
        email.DestinoResolvido.Should().Be("maria@email.com");
        email.RemetenteResolvido.Should().Be("Bem-vinda, Maria");
        email.ConteudoFinal.Should().Be("Oi Maria Souza, público visitantes");
        email.ConteudoHtmlFinal.Should().Be("<p>Oi Maria</p>");

        result.TotalEntregas.Should().Be(2);
        result.CriadoPorUsuarioId.Should().Be(42);

        _auditLogServiceMock.Verify(
            x => x.RecordAsync(
                "ComunicacaoCampanha",
                "7",
                "CriarCampanha",
                It.IsAny<object?>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RegistraBloqueioQuandoDestinoDoCanalNaoEstaDisponivel()
    {
        var dto = new CriarComunicacaoCampanhaDto
        {
            Nome = "Convite",
            Objetivo = "engajamento",
            PublicoAlvo = "visitantes",
            Canais =
            [
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.WhatsApp,
                    Prioridade = 1
                },
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.Email,
                    Prioridade = 2
                }
            ]
        };

        var createdCampaign = new ComunicacaoCampanha
        {
            Id = 9,
            Nome = "Convite",
            Objetivo = "engajamento",
            PublicoAlvo = "visitantes",
            CriadoPorUsuarioId = 42,
            DataCriacao = DateTime.UtcNow,
            Canais =
            [
                new ComunicacaoCampanhaCanal { Id = 1, ComunicacaoCampanhaId = 9, Canal = CanalComunicacao.WhatsApp, Prioridade = 1 },
                new ComunicacaoCampanhaCanal { Id = 2, ComunicacaoCampanhaId = 9, Canal = CanalComunicacao.Email, Prioridade = 2 }
            ]
        };

        List<ComunicacaoEntrega>? entregasCriadas = null;

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>())).ReturnsAsync(createdCampaign);
        _repositoryMock.Setup(x => x.GetByIdAsync(createdCampaign.Id)).ReturnsAsync(createdCampaign);

        _audienceResolverMock
            .Setup(x => x.ResolveAsync("visitantes"))
            .ReturnsAsync(
            [
                new ComunicacaoDestinatario
                {
                    PessoaId = 1,
                    VisitanteId = 10,
                    Nome = "Pessoa Sem WhatsApp",
                    PrimeiroNome = "Pessoa",
                    Email = "pessoa1@email.com"
                },
                new ComunicacaoDestinatario
                {
                    PessoaId = 2,
                    VisitanteId = 20,
                    Nome = "Pessoa Sem Email",
                    PrimeiroNome = "Outra",
                    WhatsApp = "5511888888888"
                }
            ]);

        _entregaRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<ComunicacaoEntrega>>()))
            .ReturnsAsync((IEnumerable<ComunicacaoEntrega> entregas) =>
            {
                entregasCriadas = entregas.ToList();
                return entregasCriadas;
            });

        await _service.CreateAsync(dto);

        entregasCriadas.Should().NotBeNull();
        entregasCriadas.Should().HaveCount(4);
        entregasCriadas!.Should().ContainSingle(x =>
            x.Canal == CanalComunicacao.Email &&
            x.DestinoResolvido == "pessoa1@email.com" &&
            x.Status == StatusComunicacaoEntrega.Pendente);
        entregasCriadas.Should().ContainSingle(x =>
            x.Canal == CanalComunicacao.WhatsApp &&
            x.DestinoResolvido == "5511888888888" &&
            x.Status == StatusComunicacaoEntrega.Pendente);
        entregasCriadas.Should().ContainSingle(x =>
            x.Canal == CanalComunicacao.WhatsApp &&
            x.DestinatarioPessoaId == 1 &&
            x.Status == StatusComunicacaoEntrega.Falhou &&
            x.Erro == "Entrega bloqueada: Pessoa Sem WhatsApp não possui WhatsApp válido.");
        entregasCriadas.Should().ContainSingle(x =>
            x.Canal == CanalComunicacao.Email &&
            x.DestinatarioPessoaId == 2 &&
            x.Status == StatusComunicacaoEntrega.Falhou &&
            x.Erro == "Entrega bloqueada: Pessoa Sem Email não possui e-mail válido.");
    }

    [Fact]
    public async Task CreateAsync_IgnoraEntregaQuandoPessoaBloqueouCanal()
    {
        var dto = new CriarComunicacaoCampanhaDto
        {
            Nome = "Aviso",
            Objetivo = "operacao",
            PublicoAlvo = "membros",
            Canais =
            [
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.Email,
                    Prioridade = 1
                }
            ]
        };

        var createdCampaign = new ComunicacaoCampanha
        {
            Id = 15,
            Nome = "Aviso",
            Objetivo = "operacao",
            PublicoAlvo = "membros",
            CriadoPorUsuarioId = 42,
            DataCriacao = DateTime.UtcNow,
            Canais = [new ComunicacaoCampanhaCanal { Id = 1, ComunicacaoCampanhaId = 15, Canal = CanalComunicacao.Email, Prioridade = 1 }]
        };

        List<ComunicacaoEntrega>? entregasCriadas = null;
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>())).ReturnsAsync(createdCampaign);
        _repositoryMock.Setup(x => x.GetByIdAsync(createdCampaign.Id)).ReturnsAsync(createdCampaign);
        _audienceResolverMock.Setup(x => x.ResolveAsync("membros")).ReturnsAsync(
        [
            new ComunicacaoDestinatario
            {
                PessoaId = 101,
                Nome = "Maria",
                PrimeiroNome = "Maria",
                Email = "maria@email.com"
            }
        ]);
        _preferenciaServiceMock.Setup(x => x.EstaBloqueadoAsync(101, CanalComunicacao.Email)).ReturnsAsync(true);
        _entregaRepositoryMock.Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<ComunicacaoEntrega>>()))
            .ReturnsAsync((IEnumerable<ComunicacaoEntrega> entregas) =>
            {
                entregasCriadas = entregas.ToList();
                return entregasCriadas;
            });

        await _service.CreateAsync(dto);

        entregasCriadas.Should().ContainSingle(x =>
            x.Status == StatusComunicacaoEntrega.IgnoradoPorPreferencia &&
            x.Canal == CanalComunicacao.Email &&
            x.Erro == "Entrega ignorada: Maria bloqueou o canal Email.");
    }

    [Fact]
    public async Task CreateAsync_GeraEntregasContextuaisParaPushENotificacaoInterna()
    {
        var dto = new CriarComunicacaoCampanhaDto
        {
            Nome = "Aviso interno",
            Objetivo = "operacao",
            PublicoAlvo = "membros",
            Canais =
            [
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.Push,
                    Prioridade = 1
                },
                new CriarComunicacaoCampanhaCanalDto
                {
                    Canal = CanalComunicacao.NotificacaoInterna,
                    Prioridade = 2
                }
            ]
        };

        var createdCampaign = new ComunicacaoCampanha
        {
            Id = 10,
            Nome = "Aviso interno",
            Objetivo = "operacao",
            PublicoAlvo = "membros",
            CriadoPorUsuarioId = 42,
            DataCriacao = DateTime.UtcNow,
            Canais =
            [
                new ComunicacaoCampanhaCanal { Id = 1, ComunicacaoCampanhaId = 10, Canal = CanalComunicacao.Push, Prioridade = 1 },
                new ComunicacaoCampanhaCanal { Id = 2, ComunicacaoCampanhaId = 10, Canal = CanalComunicacao.NotificacaoInterna, Prioridade = 2 }
            ]
        };

        List<ComunicacaoEntrega>? entregasCriadas = null;

        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>())).ReturnsAsync(createdCampaign);
        _repositoryMock.Setup(x => x.GetByIdAsync(createdCampaign.Id)).ReturnsAsync(createdCampaign);
        _audienceResolverMock
            .Setup(x => x.ResolveAsync("membros"))
            .ReturnsAsync(
            [
                new ComunicacaoDestinatario
                {
                    PessoaId = 77,
                    Nome = "João Silva",
                    PrimeiroNome = "João"
                }
            ]);

        _entregaRepositoryMock
            .Setup(x => x.CreateManyAsync(It.IsAny<IEnumerable<ComunicacaoEntrega>>()))
            .ReturnsAsync((IEnumerable<ComunicacaoEntrega> entregas) =>
            {
                entregasCriadas = entregas.ToList();
                return entregasCriadas;
            });

        await _service.CreateAsync(dto);

        entregasCriadas.Should().NotBeNull();
        entregasCriadas.Should().HaveCount(2);
        entregasCriadas!.Should().ContainSingle(x =>
            x.Canal == CanalComunicacao.Push &&
            x.DestinoResolvido == "pessoa:77" &&
            x.RemetenteResolvido == "Aviso interno");
        entregasCriadas.Should().ContainSingle(x =>
            x.Canal == CanalComunicacao.NotificacaoInterna &&
            x.DestinoResolvido == "pessoa:77" &&
            x.RemetenteResolvido == "Aviso interno");
    }
}
