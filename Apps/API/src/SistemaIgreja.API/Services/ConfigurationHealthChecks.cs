using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;

namespace SistemaIgreja.API.Services;

public class EvolutionApiConfigurationHealthCheck : IHealthCheck
{
    private readonly EvolutionApiSettings _settings;

    public EvolutionApiConfigurationHealthCheck(IOptions<EvolutionApiSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl)) missing.Add(nameof(_settings.BaseUrl));
        if (string.IsNullOrWhiteSpace(_settings.ApiKey)) missing.Add(nameof(_settings.ApiKey));
        if (string.IsNullOrWhiteSpace(_settings.InstanceName)) missing.Add(nameof(_settings.InstanceName));

        if (missing.Count == 0)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Evolution API configuration OK."));
        }

        return Task.FromResult(HealthCheckResult.Degraded(
            $"Evolution API configuration incomplete. Missing: {string.Join(", ", missing)}."));
    }
}

public class MessageSchedulerConfigurationHealthCheck : IHealthCheck
{
    private readonly MessageSchedulerSettings _settings;

    public MessageSchedulerConfigurationHealthCheck(IOptions<MessageSchedulerSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_settings.BaseIntervalMinutes <= 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("MessageScheduler BaseIntervalMinutes must be greater than zero."));
        }

        if (_settings.BatchSizeReserva <= 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("MessageScheduler BatchSizeReserva must be greater than zero."));
        }

        return Task.FromResult(HealthCheckResult.Healthy("MessageScheduler configuration OK."));
    }
}

public class EscalaSchedulerConfigurationHealthCheck : IHealthCheck
{
    private readonly EscalaSchedulerSettings _settings;

    public EscalaSchedulerConfigurationHealthCheck(IOptions<EscalaSchedulerSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_settings.BaseIntervalMinutes <= 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("EscalaScheduler BaseIntervalMinutes must be greater than zero."));
        }

        if (_settings.DiasJanelaFim < _settings.DiasJanelaInicio)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("EscalaScheduler window is invalid: DiasJanelaFim must be greater than or equal to DiasJanelaInicio."));
        }

        if (!_settings.Enabled)
        {
            return Task.FromResult(HealthCheckResult.Degraded("EscalaScheduler is disabled by configuration."));
        }

        return Task.FromResult(HealthCheckResult.Healthy("EscalaScheduler configuration OK."));
    }
}
