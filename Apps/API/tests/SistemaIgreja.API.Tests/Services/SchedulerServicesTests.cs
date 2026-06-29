using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class SchedulerServicesTests
{
    [Fact]
    public async Task MessageSchedulerService_ProcessesReservedMessages_AndRecordsSuccess()
    {
        var mensagemServiceMock = new Mock<IMensagemAgendadaService>();
        var evolutionServiceMock = new Mock<IEvolutionApiService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource();

        mensagemServiceMock.Setup(s => s.ReservarProntasParaEnvioAsync(10))
            .ReturnsAsync(new[]
            {
                new MensagemAgendadaDto
                {
                    Id = 5,
                    NomeVisitante = "Marco",
                    TelefoneVisitante = "11999999999",
                    TextoFinal = "Olá"
                }
            });
        mensagemServiceMock.Setup(s => s.MarcarComoEnviadaAsync(5))
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());
        evolutionServiceMock.Setup(s => s.ValidarInstanciaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        evolutionServiceMock.Setup(s => s.EnviarMensagemTextoAsync("11999999999", "Olá", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EvolutionApiResponse { Sucesso = true, MessageId = "msg-1" });

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(mensagemServiceMock.Object);
            services.AddSingleton(evolutionServiceMock.Object);
        });

        var service = new TestableMessageSchedulerService(
            provider,
            Mock.Of<ILogger<MessageSchedulerService>>(),
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                BatchSizeReserva = 10
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        mensagemServiceMock.Verify(s => s.MarcarComoEnviadaAsync(5), Times.Once);
        monitor.GetAll().Single().Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task MessageSchedulerService_WhenMessageHasNoPhone_MarksErrorAndStillRecordsSuccess()
    {
        var mensagemServiceMock = new Mock<IMensagemAgendadaService>();
        var evolutionServiceMock = new Mock<IEvolutionApiService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource();

        mensagemServiceMock.Setup(s => s.ReservarProntasParaEnvioAsync(10))
            .ReturnsAsync(new[]
            {
                new MensagemAgendadaDto
                {
                    Id = 6,
                    NomeVisitante = "Aline",
                    TelefoneVisitante = "",
                    TextoFinal = "Olá"
                }
            });
        mensagemServiceMock.Setup(s => s.MarcarComoErroAsync(6, It.Is<string>(msg => msg.Contains("número de telefone"))))
            .Returns(Task.CompletedTask)
            .Callback(() => cts.Cancel());
        evolutionServiceMock.Setup(s => s.ValidarInstanciaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(mensagemServiceMock.Object);
            services.AddSingleton(evolutionServiceMock.Object);
        });

        var service = new TestableMessageSchedulerService(
            provider,
            Mock.Of<ILogger<MessageSchedulerService>>(),
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                BatchSizeReserva = 10
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        mensagemServiceMock.Verify(s => s.MarcarComoErroAsync(6, It.IsAny<string>()), Times.Once);
        evolutionServiceMock.Verify(s => s.EnviarMensagemTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        monitor.GetAll().Single().Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task MessageSchedulerService_ProcessesComunicacaoDeliveries_AndRecordsSuccess()
    {
        var mensagemServiceMock = new Mock<IMensagemAgendadaService>();
        var comunicacaoProcessamentoMock = new Mock<IComunicacaoProcessamentoService>();
        var evolutionServiceMock = new Mock<IEvolutionApiService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource();

        mensagemServiceMock.Setup(s => s.ReservarProntasParaEnvioAsync(10))
            .ReturnsAsync([]);
        comunicacaoProcessamentoMock.Setup(s => s.ProcessarPendentesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2)
            .Callback(() => cts.Cancel());
        evolutionServiceMock.Setup(s => s.ValidarInstanciaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(mensagemServiceMock.Object);
            services.AddSingleton(comunicacaoProcessamentoMock.Object);
            services.AddSingleton(evolutionServiceMock.Object);
        });

        var service = new TestableMessageSchedulerService(
            provider,
            Mock.Of<ILogger<MessageSchedulerService>>(),
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                BatchSizeReserva = 10
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        comunicacaoProcessamentoMock.Verify(s => s.ProcessarPendentesAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        monitor.GetAll().Single().Status.Should().Be("Healthy");
        monitor.GetAll().Single().Details.Should().Contain("comunicacao: 2");
    }

    [Fact]
    public async Task MessageSchedulerService_WhenResolutionFails_RecordsFailure()
    {
        var evolutionServiceMock = new Mock<IEvolutionApiService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource(50);

        evolutionServiceMock.Setup(s => s.ValidarInstanciaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(evolutionServiceMock.Object);
        });

        var service = new TestableMessageSchedulerService(
            provider,
            Mock.Of<ILogger<MessageSchedulerService>>(),
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                BatchSizeReserva = 10
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        monitor.GetAll().Single().Status.Should().Be("Unhealthy");
    }

    [Fact]
    public async Task EscalaSchedulerService_GeneratesOccurrencesAndProcessesReminders()
    {
        var eventoRepositoryMock = new Mock<IEventoRepository>();
        var ocorrenciaServiceMock = new Mock<IEventoOcorrenciaService>();
        var escalaServiceMock = new Mock<IEscalaService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource();

        eventoRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new[]
            {
                new Evento { Id = 3, Titulo = "Culto", Ativo = true, EhRecorrente = true }
            });
        ocorrenciaServiceMock.Setup(s => s.GerarPorRecorrenciaAsync(3, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
            .ReturnsAsync(new GerarOcorrenciasResultadoDto { TotalCriadas = 2 });
        escalaServiceMock.Setup(s => s.EnviarLembretesPendentesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(1)
            .Callback(() => cts.Cancel());

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(eventoRepositoryMock.Object);
            services.AddSingleton(ocorrenciaServiceMock.Object);
            services.AddSingleton(escalaServiceMock.Object);
        });

        var service = new TestableEscalaSchedulerService(
            provider,
            Mock.Of<ILogger<EscalaSchedulerService>>(),
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = true,
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                DiasJanelaInicio = 0,
                DiasJanelaFim = 7,
                EnviarLembretesAutomaticos = true
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        ocorrenciaServiceMock.Verify(s => s.GerarPorRecorrenciaAsync(3, It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
        escalaServiceMock.Verify(s => s.EnviarLembretesPendentesAsync(It.IsAny<DateTime>()), Times.Once);
        monitor.GetAll().Single().Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task EscalaSchedulerService_WhenDisabled_DoesNotResolveDependenciesAndStillRecordsSuccess()
    {
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource(50);

        var provider = BuildProvider(_ => { });

        var service = new TestableEscalaSchedulerService(
            provider,
            Mock.Of<ILogger<EscalaSchedulerService>>(),
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = false,
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                DiasJanelaInicio = 0,
                DiasJanelaFim = 7,
                EnviarLembretesAutomaticos = true
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        monitor.GetAll().Single().Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task EscalaSchedulerService_WhenReminderProcessingFails_RecordsFailure()
    {
        var eventoRepositoryMock = new Mock<IEventoRepository>();
        var ocorrenciaServiceMock = new Mock<IEventoOcorrenciaService>();
        var escalaServiceMock = new Mock<IEscalaService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource(50);

        eventoRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new[]
            {
                new Evento { Id = 3, Titulo = "Culto", Ativo = true, EhRecorrente = true }
            });
        ocorrenciaServiceMock.Setup(s => s.GerarPorRecorrenciaAsync(3, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
            .ReturnsAsync(new GerarOcorrenciasResultadoDto { TotalCriadas = 0 });
        escalaServiceMock.Setup(s => s.EnviarLembretesPendentesAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("falha lembretes"));

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(eventoRepositoryMock.Object);
            services.AddSingleton(ocorrenciaServiceMock.Object);
            services.AddSingleton(escalaServiceMock.Object);
        });

        var service = new TestableEscalaSchedulerService(
            provider,
            Mock.Of<ILogger<EscalaSchedulerService>>(),
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = true,
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                DiasJanelaInicio = 0,
                DiasJanelaFim = 7,
                EnviarLembretesAutomaticos = true
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        monitor.GetAll().Single().Status.Should().Be("Unhealthy");
    }

    [Fact]
    public async Task BirthdayCampaignSchedulerService_ProcessesCampaign_AndRecordsSuccess()
    {
        var campanhaServiceMock = new Mock<ICampanhaAniversarioService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource();

        campanhaServiceMock.Setup(s => s.ProcessarAniversariantesDoDiaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CampanhaAniversarioProcessamentoResultadoDto
            {
                TotalElegiveis = 3,
                TotalEnviados = 2,
                TotalIgnorados = 1,
                TotalFalhas = 0
            })
            .Callback(() => cts.Cancel());

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(campanhaServiceMock.Object);
        });

        var service = new TestableBirthdayCampaignSchedulerService(
            provider,
            Mock.Of<ILogger<BirthdayCampaignSchedulerService>>(),
            Options.Create(new BirthdayCampaignSchedulerSettings
            {
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                MaxPessoasPorExecucao = 5,
                TimeZoneId = "America/Sao_Paulo"
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        campanhaServiceMock.Verify(s => s.ProcessarAniversariantesDoDiaAsync(It.IsAny<CancellationToken>()), Times.Once);
        monitor.GetAll().Single().Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task BirthdayCampaignSchedulerService_WhenServiceFails_RecordsFailure()
    {
        var campanhaServiceMock = new Mock<ICampanhaAniversarioService>();
        var monitor = new SchedulerExecutionMonitor();
        using var cts = new CancellationTokenSource(50);

        campanhaServiceMock.Setup(s => s.ProcessarAniversariantesDoDiaAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("falha campanha"));

        var provider = BuildProvider(services =>
        {
            services.AddSingleton(campanhaServiceMock.Object);
        });

        var service = new TestableBirthdayCampaignSchedulerService(
            provider,
            Mock.Of<ILogger<BirthdayCampaignSchedulerService>>(),
            Options.Create(new BirthdayCampaignSchedulerSettings
            {
                BaseIntervalMinutes = 1,
                JitterSecondsMax = 0,
                MaxPessoasPorExecucao = 5,
                TimeZoneId = "America/Sao_Paulo"
            }),
            monitor);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.RunAsync(cts.Token));

        monitor.GetAll().Single().Status.Should().Be("Unhealthy");
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        return services.BuildServiceProvider();
    }

    private sealed class TestableMessageSchedulerService : MessageSchedulerService
    {
        public TestableMessageSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<MessageSchedulerService> logger,
            IOptions<MessageSchedulerSettings> settings,
            ISchedulerExecutionMonitor executionMonitor)
            : base(serviceProvider, logger, settings, executionMonitor)
        {
        }

        public Task RunAsync(CancellationToken cancellationToken) => base.ExecuteAsync(cancellationToken);
    }

    private sealed class TestableEscalaSchedulerService : EscalaSchedulerService
    {
        public TestableEscalaSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<EscalaSchedulerService> logger,
            IOptions<EscalaSchedulerSettings> settings,
            ISchedulerExecutionMonitor executionMonitor)
            : base(serviceProvider, logger, settings, executionMonitor)
        {
        }

        public Task RunAsync(CancellationToken cancellationToken) => base.ExecuteAsync(cancellationToken);
    }

    private sealed class TestableBirthdayCampaignSchedulerService : BirthdayCampaignSchedulerService
    {
        public TestableBirthdayCampaignSchedulerService(
            IServiceProvider serviceProvider,
            ILogger<BirthdayCampaignSchedulerService> logger,
            IOptions<BirthdayCampaignSchedulerSettings> settings,
            ISchedulerExecutionMonitor executionMonitor)
            : base(serviceProvider, logger, settings, executionMonitor)
        {
        }

        public Task RunAsync(CancellationToken cancellationToken) => base.ExecuteAsync(cancellationToken);
    }
}
