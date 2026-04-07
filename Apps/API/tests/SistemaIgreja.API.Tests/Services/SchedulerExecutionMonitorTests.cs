using FluentAssertions;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class SchedulerExecutionMonitorTests
{
    [Fact]
    public void RecordSuccess_StoresHealthyStatus()
    {
        var monitor = new SchedulerExecutionMonitor();
        var startedAt = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc);
        var finishedAt = startedAt.AddSeconds(12);

        monitor.RecordSuccess("MessageScheduler", startedAt, finishedAt, "Processou lote");

        var result = monitor.GetAll().Single();
        result.SchedulerName.Should().Be("MessageScheduler");
        result.Status.Should().Be("Healthy");
        result.LastDurationMs.Should().Be(12000);
        result.Details.Should().Be("Processou lote");
    }

    [Fact]
    public void RecordFailure_StoresUnhealthyStatusWithError()
    {
        var monitor = new SchedulerExecutionMonitor();
        var startedAt = new DateTime(2026, 4, 6, 11, 0, 0, DateTimeKind.Utc);
        var finishedAt = startedAt.AddSeconds(3);

        monitor.RecordFailure("EscalaScheduler", startedAt, finishedAt, "Timeout", "Falha na janela");

        var result = monitor.GetAll().Single();
        result.SchedulerName.Should().Be("EscalaScheduler");
        result.Status.Should().Be("Unhealthy");
        result.Error.Should().Be("Timeout");
        result.Details.Should().Be("Falha na janela");
    }
}
