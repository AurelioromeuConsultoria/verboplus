using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class CampanhaAniversarioServiceTests
{
    private readonly Mock<IConfiguracaoCampanhaAniversarioRepository> _configuracaoRepositoryMock = new();
    private readonly Mock<IEnvioCampanhaAniversarioRepository> _envioRepositoryMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly Mock<IEvolutionApiService> _evolutionApiServiceMock = new();
    private readonly Mock<IComunicacaoAutomacaoService> _comunicacaoAutomacaoServiceMock = new();
    private readonly Mock<ILogger<CampanhaAniversarioService>> _loggerMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly CampanhaAniversarioService _service;

    public CampanhaAniversarioServiceTests()
    {
        _service = new CampanhaAniversarioService(
            _configuracaoRepositoryMock.Object,
            _envioRepositoryMock.Object,
            _pessoaRepositoryMock.Object,
            _evolutionApiServiceMock.Object,
            Options.Create(new BirthdayCampaignSchedulerSettings
            {
                MaxPessoasPorExecucao = 10,
                MaxTentativasPorPessoa = 3,
                TimeZoneId = "America/Sao_Paulo"
            }),
            Options.Create(new PublicAppUrlSettings
            {
                ApiBaseUrl = "https://publico.appigreja.com"
            }),
            _comunicacaoAutomacaoServiceMock.Object,
            _loggerMock.Object,
            _auditLogServiceMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenMensagemTemplateIsEmpty()
    {
        var act = () => _service.UpdateAsync(new AtualizarCampanhaAniversarioDto
        {
            MensagemTemplate = "   ",
            HorarioEnvio = "09:00"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A mensagem da campanha é obrigatória.");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesConfiguration_AndAudits()
    {
        ConfiguracaoCampanhaAniversario? configuracaoAtualizada = null;
        _configuracaoRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<ConfiguracaoCampanhaAniversario>()))
            .ReturnsAsync((ConfiguracaoCampanhaAniversario configuracao) =>
            {
                configuracaoAtualizada = configuracao;
                configuracao.Id = 1;
                return configuracao;
            });
        _envioRepositoryMock.Setup(r => r.GetHistoricoAsync(null, null, 50)).ReturnsAsync(new List<EnvioCampanhaAniversario>());
        ConfigurarMetricasZeradas();

        var result = await _service.UpdateAsync(new AtualizarCampanhaAniversarioDto
        {
            Ativo = true,
            ImagemUrl = " /uploads/campanha.png ",
            MensagemTemplate = " Feliz aniversario, {Nome}! ",
            HorarioEnvio = "10:30"
        });

        configuracaoAtualizada.Should().NotBeNull();
        configuracaoAtualizada!.ImagemUrl.Should().Be("/uploads/campanha.png");
        configuracaoAtualizada.MensagemTemplate.Should().Be("Feliz aniversario, {Nome}!");
        configuracaoAtualizada.HorarioEnvio.Should().Be(new TimeSpan(10, 30, 0));
        result.HorarioEnvio.Should().Be("10:30");
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("CampanhaAniversario", "1", "AtualizarConfiguracao", It.IsAny<object?>()),
            Times.Once);
    }

    [Fact]
    public async Task EnviarTesteAsync_Throws_WhenWhatsAppIsEmpty()
    {
        var act = () => _service.EnviarTesteAsync(new EnviarTesteCampanhaAniversarioDto
        {
            Nome = "Teste",
            WhatsApp = "   "
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("O WhatsApp para teste é obrigatório.");
    }

    [Fact]
    public async Task EnviarTesteAsync_ReturnsSuccess_WhenEvolutionApiSucceeds()
    {
        _configuracaoRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(CriarConfiguracaoAtiva());
        _evolutionApiServiceMock
            .Setup(s => s.EnviarMensagemImagemAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse
            {
                Sucesso = true,
                MessageId = "msg-123"
            });

        var result = await _service.EnviarTesteAsync(new EnviarTesteCampanhaAniversarioDto
        {
            Nome = "Maria",
            WhatsApp = "5511999999999"
        });

        result.Sucesso.Should().BeTrue();
        result.MessageId.Should().Be("msg-123");
        result.Mensagem.Should().Be("Mensagem de teste enviada com sucesso.");
        _evolutionApiServiceMock.Verify(
            s => s.EnviarMensagemImagemAsync(
                "5511999999999",
                "/uploads/campanha.png",
                It.Is<string>(legenda => legenda.Contains("Maria")),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("CampanhaAniversario", "teste", "EnviarTeste", It.IsAny<object?>()),
            Times.Once);
    }

    [Fact]
    public async Task EnviarTesteAsync_ReturnsFailureMessage_WhenEvolutionApiFails()
    {
        _configuracaoRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(CriarConfiguracaoAtiva());
        _evolutionApiServiceMock
            .Setup(s => s.EnviarMensagemImagemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse
            {
                Sucesso = false,
                MensagemErro = "Falha na Evolution",
                RespostaCompleta = "{\"error\":\"boom\"}"
            });

        var result = await _service.EnviarTesteAsync(new EnviarTesteCampanhaAniversarioDto
        {
            Nome = "Maria",
            WhatsApp = "5511999999999"
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Falha na Evolution");
        result.Detalhes.Should().Be("{\"error\":\"boom\"}");
    }

    [Fact]
    public async Task ReenviarAsync_Throws_WhenEnvioDoesNotExist()
    {
        _configuracaoRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(CriarConfiguracaoAtiva());
        _envioRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((EnvioCampanhaAniversario?)null);

        var act = () => _service.ReenviarAsync(99);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Registro de envio não encontrado.");
    }

    [Fact]
    public async Task ReenviarAsync_UpdatesEnvio_WhenSendSucceeds()
    {
        var pessoa = CriarPessoa(1, "Maria", "5511999999999");
        var envio = new EnvioCampanhaAniversario
        {
            Id = 5,
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            AnoReferencia = DateTime.Now.Year,
            DataAniversario = DateTime.Now.Date,
            Tentativas = 1,
            Status = StatusEnvioCampanhaAniversario.Erro
        };

        _configuracaoRepositoryMock.Setup(r => r.GetAsync()).ReturnsAsync(CriarConfiguracaoAtiva());
        _envioRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(envio);
        _envioRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<EnvioCampanhaAniversario>()))
            .ReturnsAsync((EnvioCampanhaAniversario item) => item);
        _evolutionApiServiceMock
            .Setup(s => s.EnviarMensagemImagemAsync(pessoa.WhatsApp!, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse
            {
                Sucesso = true,
                MessageId = "reenvio-1"
            });

        var result = await _service.ReenviarAsync(5);

        result.Sucesso.Should().BeTrue();
        result.EnvioId.Should().Be(5);
        envio.Status.Should().Be(StatusEnvioCampanhaAniversario.Enviado);
        envio.Tentativas.Should().Be(2);
        envio.DataEnvioSucesso.Should().NotBeNull();
        _envioRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<EnvioCampanhaAniversario>()), Times.Exactly(2));
        _auditLogServiceMock.Verify(
            a => a.RecordAsync("CampanhaAniversarioEnvio", "5", "Reenviar", It.IsAny<object?>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessarAniversariantesDoDiaAsync_ReturnsEmpty_WhenCampaignIsInactive()
    {
        _comunicacaoAutomacaoServiceMock
            .Setup(s => s.ExecutarAniversariosDoDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampanhaAniversarioProcessamentoResultadoDto());

        var result = await _service.ProcessarAniversariantesDoDiaAsync();

        result.TotalElegiveis.Should().Be(0);
        result.TotalEnviados.Should().Be(0);
        result.TotalFalhas.Should().Be(0);
        result.TotalIgnorados.Should().Be(0);
        _comunicacaoAutomacaoServiceMock.Verify(r => r.ExecutarAniversariosDoDiaAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarAniversariantesDoDiaAsync_ProcessesEligiblePeople_WithSuccessFailureAndIgnored()
    {
        _comunicacaoAutomacaoServiceMock
            .Setup(s => s.ExecutarAniversariosDoDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampanhaAniversarioProcessamentoResultadoDto
            {
                TotalElegiveis = 3,
                TotalEnviados = 1,
                TotalFalhas = 1,
                TotalIgnorados = 1
            });

        var result = await _service.ProcessarAniversariantesDoDiaAsync();

        result.TotalElegiveis.Should().Be(3);
        result.TotalEnviados.Should().Be(1);
        result.TotalFalhas.Should().Be(1);
        result.TotalIgnorados.Should().Be(1);
        _comunicacaoAutomacaoServiceMock.Verify(
            s => s.ExecutarAniversariosDoDiaAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void ConfigurarMetricasZeradas()
    {
        _envioRepositoryMock.Setup(r => r.CountAsync()).ReturnsAsync(0);
        _envioRepositoryMock.Setup(r => r.CountByStatusAnoAsync(It.IsAny<StatusEnvioCampanhaAniversario>(), It.IsAny<int>())).ReturnsAsync(0);
        _envioRepositoryMock.Setup(r => r.CountPendentesAnoAsync(It.IsAny<int>())).ReturnsAsync(0);
        _envioRepositoryMock.Setup(r => r.CountByStatusDataAsync(It.IsAny<StatusEnvioCampanhaAniversario>(), It.IsAny<DateTime>())).ReturnsAsync(0);
    }

    private static ConfiguracaoCampanhaAniversario CriarConfiguracaoAtiva()
    {
        return new ConfiguracaoCampanhaAniversario
        {
            Id = 1,
            Ativo = true,
            ImagemUrl = "/uploads/campanha.png",
            MensagemTemplate = "Ola, {Nome}!",
            HorarioEnvio = TimeSpan.Zero
        };
    }

    private static Pessoa CriarPessoa(int id, string nome, string whatsApp, DateTime? dataNascimento = null)
    {
        return new Pessoa
        {
            Id = id,
            Nome = nome,
            WhatsApp = whatsApp,
            DataNascimento = dataNascimento ?? DateTime.Now.Date.AddYears(-30),
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
    }
}
