using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoAutomacaoServiceTests
{
    private readonly Mock<IVisitanteRepository> _visitanteRepositoryMock = new();
    private readonly Mock<IConfiguracaoMensagemRepository> _configuracaoMensagemRepositoryMock = new();
    private readonly Mock<IConfiguracaoCampanhaAniversarioRepository> _configuracaoCampanhaAniversarioRepositoryMock = new();
    private readonly Mock<IEnvioCampanhaAniversarioRepository> _envioCampanhaAniversarioRepositoryMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly Mock<IComunicacaoCampanhaRepository> _campanhaRepositoryMock = new();
    private readonly Mock<IComunicacaoEntregaRepository> _entregaRepositoryMock = new();
    private readonly Mock<IComunicacaoPreferenciaService> _preferenciaServiceMock = new();
    private readonly Mock<IComunicacaoProcessamentoService> _processamentoServiceMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly Mock<ILogger<ComunicacaoAutomacaoService>> _loggerMock = new();
    private readonly ComunicacaoAutomacaoService _service;

    public ComunicacaoAutomacaoServiceTests()
    {
        _service = new ComunicacaoAutomacaoService(
            _visitanteRepositoryMock.Object,
            _configuracaoMensagemRepositoryMock.Object,
            _configuracaoCampanhaAniversarioRepositoryMock.Object,
            _envioCampanhaAniversarioRepositoryMock.Object,
            _pessoaRepositoryMock.Object,
            _campanhaRepositoryMock.Object,
            _entregaRepositoryMock.Object,
            _preferenciaServiceMock.Object,
            _processamentoServiceMock.Object,
            _auditLogServiceMock.Object,
            Options.Create(new BirthdayCampaignSchedulerSettings
            {
                MaxPessoasPorExecucao = 10,
                MaxTentativasPorPessoa = 3,
                TimeZoneId = "America/Sao_Paulo"
            }),
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecutarNovoVisitanteAsync_CriaCampanhasCentralizadasParaCadaConfiguracaoAtiva()
    {
        var visitante = new Visitante
        {
            Id = 15,
            PessoaId = 9,
            Pessoa = new Pessoa
            {
                Id = 9,
                Nome = "Maria",
                WhatsApp = "5511999999999",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            },
            DataVisita = new DateTime(2026, 4, 6, 10, 0, 0),
            DataCadastro = DateTime.UtcNow
        };

        _visitanteRepositoryMock.Setup(x => x.GetByIdAsync(15)).ReturnsAsync(visitante);
        _configuracaoMensagemRepositoryMock.Setup(x => x.GetAtivasAsync()).ReturnsAsync(
        [
            new ConfiguracaoMensagem { Id = 1, TextoMensagem = "Olá {Nome}", DiasAposVisita = 0, HorarioEnvio = new TimeSpan(10, 0, 0), Ativo = true },
            new ConfiguracaoMensagem { Id = 2, TextoMensagem = "Estamos com você, {Nome}", DiasAposVisita = 2, HorarioEnvio = new TimeSpan(9, 0, 0), Ativo = true }
        ]);
        _campanhaRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>()))
            .ReturnsAsync((ComunicacaoCampanha campanha) =>
            {
                campanha.Id = campanha.Nome.Contains("D+0", StringComparison.Ordinal) ? 101 : 102;
                return campanha;
            });
        _entregaRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoEntrega>()))
            .ReturnsAsync((ComunicacaoEntrega entrega) => entrega);

        var result = await _service.ExecutarNovoVisitanteAsync(15);

        result.Gatilho.Should().Be("novo-visitante");
        result.TotalCriadas.Should().Be(2);
        _campanhaRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>()), Times.Exactly(2));
        _entregaRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<ComunicacaoEntrega>(e =>
                e.DestinoResolvido == "5511999999999" &&
                e.ConteudoFinal.Contains("Maria"))),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecutarAniversariosDoDiaAsync_ProcessaHistoricoPeloFluxoCentral()
    {
        var hoje = DateTime.Now.Date;
        var pessoa = new Pessoa
        {
            Id = 22,
            Nome = "Bruno",
            WhatsApp = "5511888888888",
            DataNascimento = hoje.AddYears(-30),
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        var envio = new EnvioCampanhaAniversario
        {
            Id = 70,
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            AnoReferencia = hoje.Year,
            DataAniversario = hoje,
            Status = StatusEnvioCampanhaAniversario.Pendente
        };

        _configuracaoCampanhaAniversarioRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(new ConfiguracaoCampanhaAniversario
        {
            Id = 1,
            Ativo = true,
            ImagemUrl = "/uploads/aniversario.png",
            MensagemTemplate = "Feliz aniversário, {Nome}!",
            HorarioEnvio = TimeSpan.Zero
        });
        _pessoaRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync([pessoa]);
        _envioCampanhaAniversarioRepositoryMock.Setup(x => x.GetByPessoaAnoAsync(pessoa.Id, It.IsAny<int>()))
            .ReturnsAsync((EnvioCampanhaAniversario?)null);
        _envioCampanhaAniversarioRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<EnvioCampanhaAniversario>()))
            .ReturnsAsync((EnvioCampanhaAniversario item) =>
            {
                item.Id = envio.Id;
                return item;
            });
        _envioCampanhaAniversarioRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<EnvioCampanhaAniversario>()))
            .ReturnsAsync((EnvioCampanhaAniversario item) => item);
        _campanhaRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>()))
            .ReturnsAsync((ComunicacaoCampanha campanha) =>
            {
                campanha.Id = 501;
                return campanha;
            });
        _entregaRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoEntrega>()))
            .ReturnsAsync((ComunicacaoEntrega entrega) =>
            {
                entrega.Id = 801;
                return entrega;
            });
        _processamentoServiceMock.Setup(x => x.ProcessarEntregaAsync(801, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _service.ExecutarAniversariosDoDiaAsync();

        result.TotalElegiveis.Should().Be(1);
        result.TotalEnviados.Should().Be(1);
        result.TotalFalhas.Should().Be(0);
        result.TotalIgnorados.Should().Be(0);
        _entregaRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<ComunicacaoEntrega>(e =>
                e.MidiaUrl == "/uploads/aniversario.png" &&
                e.ConteudoFinal.Contains("Bruno"))),
            Times.Once);
        _processamentoServiceMock.Verify(x => x.ProcessarEntregaAsync(801, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecutarAvisoContextualKidsAsync_NaoDuplicaQuandoChaveEventoJaFoiProcessada()
    {
        _auditLogServiceMock.Setup(x => x.GetPagedAsync(It.Is<AuditLogPagedQueryDto>(q =>
                q.EntityName == "ComunicacaoAutomacaoEvento" &&
                q.EntityId == "kids:checkin:10:99" &&
                q.Action == "ExecutarAvisoContextualKids")))
            .ReturnsAsync(new PagedResultDto<AuditLogDto>
            {
                Items = [new AuditLogDto { Id = 1, EntityName = "ComunicacaoAutomacaoEvento", EntityId = "kids:checkin:10:99", Action = "ExecutarAvisoContextualKids", CreatedAt = DateTime.UtcNow }],
                Total = 1,
                Page = 1,
                PageSize = 1
            });

        var total = await _service.ExecutarAvisoContextualKidsAsync(new ComunicacaoAvisoContextualKidsRequest
        {
            ChaveEvento = "kids:checkin:10:99",
            CriancaPessoaId = 10,
            ResponsavelPessoaIds = [30],
            Titulo = "App Kids - Check-in",
            Mensagem = "Check-in realizado",
            Tipo = "CHECKIN"
        });

        total.Should().Be(0);
        _campanhaRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>()), Times.Never);
    }

    [Fact]
    public async Task GetHistoricoAsync_RetornaExecucoesDaAutomacao()
    {
        _auditLogServiceMock.Setup(x => x.GetPagedAsync(It.Is<AuditLogPagedQueryDto>(q =>
                q.EntityName == "ComunicacaoAutomacaoEvento" &&
                q.Action == "ExecutarLembreteOperacional")))
            .ReturnsAsync(new PagedResultDto<AuditLogDto>
            {
                Items =
                [
                    new AuditLogDto
                    {
                        Id = 10,
                        EntityName = "ComunicacaoAutomacaoEvento",
                        EntityId = "escala:1:24h",
                        Action = "ExecutarLembreteOperacional",
                        CreatedAt = DateTime.UtcNow,
                        ChangesJson = "{\"PessoaId\":101}"
                    }
                ],
                Total = 1,
                Page = 1,
                PageSize = 20
            });

        var result = await _service.GetHistoricoAsync(new ComunicacaoAutomacaoHistoricoQueryDto
        {
            Gatilho = "ExecutarLembreteOperacional"
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].ChaveEvento.Should().Be("escala:1:24h");
        result.Items[0].Gatilho.Should().Be("ExecutarLembreteOperacional");
    }

    [Fact]
    public async Task ExecutarNovoVisitanteAsync_GeraEntregaIgnoradaQuandoCanalEstaBloqueado()
    {
        var visitante = new Visitante
        {
            Id = 15,
            PessoaId = 9,
            Pessoa = new Pessoa
            {
                Id = 9,
                Nome = "Maria",
                WhatsApp = "5511999999999",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            },
            DataVisita = DateTime.UtcNow,
            DataCadastro = DateTime.UtcNow
        };

        _visitanteRepositoryMock.Setup(x => x.GetByIdAsync(15)).ReturnsAsync(visitante);
        _configuracaoMensagemRepositoryMock.Setup(x => x.GetAtivasAsync()).ReturnsAsync(
        [
            new ConfiguracaoMensagem { Id = 1, TextoMensagem = "Olá {Nome}", DiasAposVisita = 0, HorarioEnvio = TimeSpan.Zero, Ativo = true }
        ]);
        _preferenciaServiceMock.Setup(x => x.EstaBloqueadoAsync(9, CanalComunicacao.WhatsApp)).ReturnsAsync(true);
        _campanhaRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoCampanha>()))
            .ReturnsAsync((ComunicacaoCampanha campanha) =>
            {
                campanha.Id = 101;
                return campanha;
            });
        _entregaRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoEntrega>()))
            .ReturnsAsync((ComunicacaoEntrega entrega) => entrega);

        await _service.ExecutarNovoVisitanteAsync(15);

        _entregaRepositoryMock.Verify(x => x.CreateAsync(It.Is<ComunicacaoEntrega>(e =>
            e.Status == StatusComunicacaoEntrega.IgnoradoPorPreferencia &&
            e.Erro == "Entrega ignorada: Maria bloqueou o canal WhatsApp.")), Times.Once);
    }

    [Fact]
    public async Task ComunicacaoPreferenciaService_AtualizaPreferenciaERegistraAuditoria()
    {
        var repository = new Mock<IComunicacaoPreferenciaRepository>();
        var audit = new Mock<IAuditLogService>();
        var logger = new Mock<ILogger<ComunicacaoPreferenciaService>>();
        repository.Setup(x => x.GetByPessoaCanalAsync(10, CanalComunicacao.Email)).ReturnsAsync((ComunicacaoPreferencia?)null);
        repository.Setup(x => x.CreateAsync(It.IsAny<ComunicacaoPreferencia>()))
            .ReturnsAsync((ComunicacaoPreferencia item) =>
            {
                item.Id = 1;
                return item;
            });

        var service = new ComunicacaoPreferenciaService(repository.Object, audit.Object, logger.Object);

        var result = await service.UpsertAsync(10, CanalComunicacao.Email, new AtualizarComunicacaoPreferenciaDto
        {
            Status = StatusPreferenciaCanal.Bloqueado,
            OrigemConsentimento = "portal"
        });

        result.Status.Should().Be(StatusPreferenciaCanal.Bloqueado);
        audit.Verify(x => x.RecordAsync(
            "ComunicacaoPreferencia",
            "10:Email",
            "AtualizarPreferenciaCanal",
            It.IsAny<object?>()), Times.Once);
    }
}
