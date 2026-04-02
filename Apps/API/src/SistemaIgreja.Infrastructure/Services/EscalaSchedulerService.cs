using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.Infrastructure.Services;

public class EscalaSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EscalaSchedulerService> _logger;
    private readonly EscalaSchedulerSettings _settings;

    public EscalaSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<EscalaSchedulerService> logger,
        IOptions<EscalaSchedulerSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "EscalaSchedulerService iniciado. Ativo: {Enabled}, intervalo base: {BaseMin} min, jitter: 0–{Jitter}s, janela: D+{Inicio} a D+{Fim}",
            _settings.Enabled,
            _settings.BaseIntervalMinutes,
            _settings.JitterSecondsMax,
            _settings.DiasJanelaInicio,
            _settings.DiasJanelaFim);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_settings.Enabled)
                {
                    await GerarOcorrenciasRecorrentesAsync(stoppingToken);
                    if (_settings.EnviarLembretesAutomaticos)
                    {
                        await ProcessarLembretesEscalasAsync(stoppingToken);
                    }
                }
                else
                {
                    _logger.LogDebug("EscalaSchedulerService desativado por configuração.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento do EscalaSchedulerService");
            }

            var delay = ObterDelayComJitter();
            _logger.LogDebug("Próxima execução do EscalaSchedulerService em {Delay}", delay);
            await Task.Delay(delay, stoppingToken);
        }

        _logger.LogInformation("EscalaSchedulerService parado");
    }

    private TimeSpan ObterDelayComJitter()
    {
        var baseInterval = TimeSpan.FromMinutes(_settings.BaseIntervalMinutes);
        var jitterSec = Random.Shared.Next(0, _settings.JitterSecondsMax + 1);
        return baseInterval.Add(TimeSpan.FromSeconds(jitterSec));
    }

    private async Task GerarOcorrenciasRecorrentesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var eventoRepository = scope.ServiceProvider.GetRequiredService<IEventoRepository>();
        var eventoOcorrenciaService = scope.ServiceProvider.GetRequiredService<IEventoOcorrenciaService>();

        var hoje = DateTime.Today;
        var dataInicio = hoje.AddDays(_settings.DiasJanelaInicio);
        var dataFim = hoje.AddDays(_settings.DiasJanelaFim);

        var eventos = await eventoRepository.GetAllAsync();
        var eventosRecorrentesAtivos = eventos
            .Where(e => e.Ativo && e.EhRecorrente)
            .ToList();

        if (eventosRecorrentesAtivos.Count == 0)
        {
            _logger.LogDebug("Nenhum evento recorrente ativo encontrado para gerar ocorrências.");
            return;
        }

        var totalGeradas = 0;

        foreach (var evento in eventosRecorrentesAtivos)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                var geradas = await eventoOcorrenciaService.GerarPorRecorrenciaAsync(
                    evento.Id,
                    dataInicio,
                    dataFim);

                totalGeradas += geradas;

                if (geradas > 0)
                {
                    _logger.LogInformation(
                        "Evento {EventoId} ({EventoTitulo}): {Count} ocorrências geradas no período {DataInicio:yyyy-MM-dd} a {DataFim:yyyy-MM-dd}",
                        evento.Id,
                        evento.Titulo,
                        geradas,
                        dataInicio,
                        dataFim);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao gerar ocorrências do evento {EventoId} ({EventoTitulo})",
                    evento.Id,
                    evento.Titulo);
            }
        }

        _logger.LogInformation(
            "EscalaScheduler concluído. Eventos processados: {EventosCount}, ocorrências geradas: {OcorrenciasGeradas}",
            eventosRecorrentesAtivos.Count,
            totalGeradas);
    }

    private async Task ProcessarLembretesEscalasAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var escalaService = scope.ServiceProvider.GetRequiredService<IEscalaService>();
        var total = await escalaService.EnviarLembretesPendentesAsync(DateTime.Now);

        if (!stoppingToken.IsCancellationRequested && total > 0)
        {
            _logger.LogInformation("EscalaScheduler enviou {Total} lembrete(s) de escala.", total);
        }
    }
}
