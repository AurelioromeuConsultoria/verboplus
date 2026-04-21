using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.Configuration;

namespace SistemaIgreja.API.Tests.Services;

public class ConfigurationHealthChecksTests
{
    [Fact]
    public async Task EvolutionApiConfigurationHealthCheck_ReturnsDegraded_WhenFieldsAreMissing()
    {
        var check = new EvolutionApiConfigurationHealthCheck(
            Options.Create(new EvolutionApiSettings()));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("BaseUrl");
    }

    [Fact]
    public async Task EvolutionApiConfigurationHealthCheck_ReturnsHealthy_WhenSettingsAreComplete()
    {
        var check = new EvolutionApiConfigurationHealthCheck(
            Options.Create(new EvolutionApiSettings
            {
                BaseUrl = "https://api.example.com",
                ApiKey = "token",
                InstanceName = "igreja"
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task MessageSchedulerConfigurationHealthCheck_ReturnsUnhealthy_WhenBatchSizeIsInvalid()
    {
        var check = new MessageSchedulerConfigurationHealthCheck(
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 5,
                BatchSizeReserva = 0
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task MessageSchedulerConfigurationHealthCheck_ReturnsUnhealthy_WhenBaseIntervalIsInvalid()
    {
        var check = new MessageSchedulerConfigurationHealthCheck(
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 0,
                BatchSizeReserva = 10
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("BaseIntervalMinutes");
    }

    [Fact]
    public async Task MessageSchedulerConfigurationHealthCheck_ReturnsHealthy_WhenConfigurationIsValid()
    {
        var check = new MessageSchedulerConfigurationHealthCheck(
            Options.Create(new MessageSchedulerSettings
            {
                BaseIntervalMinutes = 5,
                BatchSizeReserva = 10
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task EmailConfigurationHealthCheck_ReturnsHealthy_WhenSettingsAreComplete()
    {
        var check = new EmailConfigurationHealthCheck(
            Options.Create(new EmailSettings
            {
                Enabled = true,
                Host = "smtp.example.com",
                FromAddress = "noreply@example.com",
                Username = "mailer",
                Password = "secret"
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task EmailConfigurationHealthCheck_ReturnsDegraded_WhenPasswordIsMissingForConfiguredUsername()
    {
        var check = new EmailConfigurationHealthCheck(
            Options.Create(new EmailSettings
            {
                Enabled = true,
                Host = "smtp.example.com",
                FromAddress = "noreply@example.com",
                Username = "mailer"
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Password");
    }

    [Fact]
    public async Task PushConfigurationHealthCheck_ReturnsDegraded_WhenCredentialsPathIsMissing()
    {
        var check = new PushConfigurationHealthCheck(
            Options.Create(new FirebaseKidsPushOptions()));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("CredentialsPath");
    }

    [Fact]
    public async Task PushConfigurationHealthCheck_ReturnsHealthy_WhenCredentialsPathIsPresent()
    {
        var check = new PushConfigurationHealthCheck(
            Options.Create(new FirebaseKidsPushOptions
            {
                CredentialsPath = "/tmp/firebase.json"
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task EscalaSchedulerConfigurationHealthCheck_ReturnsDegraded_WhenDisabled()
    {
        var check = new EscalaSchedulerConfigurationHealthCheck(
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = false,
                BaseIntervalMinutes = 30,
                DiasJanelaInicio = 0,
                DiasJanelaFim = 15
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task EscalaSchedulerConfigurationHealthCheck_ReturnsUnhealthy_WhenWindowIsInvalid()
    {
        var check = new EscalaSchedulerConfigurationHealthCheck(
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = true,
                BaseIntervalMinutes = 30,
                DiasJanelaInicio = 10,
                DiasJanelaFim = 5
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("DiasJanelaFim");
    }

    [Fact]
    public async Task EscalaSchedulerConfigurationHealthCheck_ReturnsUnhealthy_WhenBaseIntervalIsInvalid()
    {
        var check = new EscalaSchedulerConfigurationHealthCheck(
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = true,
                BaseIntervalMinutes = 0,
                DiasJanelaInicio = 0,
                DiasJanelaFim = 15
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("BaseIntervalMinutes");
    }

    [Fact]
    public async Task EscalaSchedulerConfigurationHealthCheck_ReturnsHealthy_WhenConfigurationIsValid()
    {
        var check = new EscalaSchedulerConfigurationHealthCheck(
            Options.Create(new EscalaSchedulerSettings
            {
                Enabled = true,
                BaseIntervalMinutes = 30,
                DiasJanelaInicio = 0,
                DiasJanelaFim = 15
            }));

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }
}
