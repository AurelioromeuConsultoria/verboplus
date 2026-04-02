using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.Infrastructure.Services;

public class BirthdayCampaignSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BirthdayCampaignSchedulerService> _logger;
    private readonly BirthdayCampaignSchedulerSettings _settings;

    public BirthdayCampaignSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<BirthdayCampaignSchedulerService> logger,
        IOptions<BirthdayCampaignSchedulerSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "BirthdayCampaignSchedulerService iniciado. Intervalo base: {BaseMin} min, jitter: 0–{Jitter}s, limite por execução: {Max}.",
            _settings.BaseIntervalMinutes,
            _settings.JitterSecondsMax,
            _settings.MaxPessoasPorExecucao);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ICampanhaAniversarioService>();
                var resultado = await service.ProcessarAniversariantesDoDiaAsync(stoppingToken);

                if (resultado.TotalElegiveis > 0)
                {
                    _logger.LogInformation(
                        "Campanha de aniversário processada. Elegíveis: {Elegiveis}, Enviados: {Enviados}, Ignorados: {Ignorados}, Falhas: {Falhas}.",
                        resultado.TotalElegiveis,
                        resultado.TotalEnviados,
                        resultado.TotalIgnorados,
                        resultado.TotalFalhas);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar campanha de aniversário.");
            }

            await Task.Delay(ObterDelayComJitter(), stoppingToken);
        }
    }

    private TimeSpan ObterDelayComJitter()
    {
        var baseInterval = TimeSpan.FromMinutes(_settings.BaseIntervalMinutes);
        var jitter = Random.Shared.Next(0, _settings.JitterSecondsMax + 1);
        return baseInterval.Add(TimeSpan.FromSeconds(jitter));
    }
}
