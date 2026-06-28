using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class AuditLogServiceTests
{
    [Fact]
    public async Task GetPagedAsync_FiltersBySearchEntityAndUser()
    {
        await using var context = await CreateContextAsync();
        context.AuditLogs.AddRange(
            new AuditLog
            {
                EntityName = "Escala",
                EntityId = "10",
                Action = "Publicar",
                UserId = 1,
                UserName = "Marco",
                UserEmail = "marco@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Pessoa",
                EntityId = "20",
                Action = "Create",
                UserId = 2,
                UserName = "Aline",
                UserEmail = "aline@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 11, 0, 0, DateTimeKind.Utc)
            });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetPagedAsync(new AuditLogPagedQueryDto
        {
            Search = "escala",
            UserEmail = "marco",
            Page = 1,
            PageSize = 10
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].EntityName.Should().Be("Escala");
    }

    [Fact]
    public async Task GetMetricsAsync_CalculatesCriticalFailureAndTopValues()
    {
        await using var context = await CreateContextAsync();
        context.AuditLogs.AddRange(
            new AuditLog
            {
                EntityName = "Escala",
                EntityId = "1",
                Action = "Publicar",
                UserName = "Marco",
                UserEmail = "marco@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Escala",
                EntityId = "2",
                Action = "Recusar",
                UserName = "Marco",
                UserEmail = "marco@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 11, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Pessoa",
                EntityId = "3",
                Action = "Create",
                UserName = "Aline",
                UserEmail = "aline@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 12, 0, 0, DateTimeKind.Utc)
            });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetMetricsAsync(new AuditLogPagedQueryDto());

        result.TotalLogs.Should().Be(3);
        result.CriticalActions.Should().Be(2);
        result.FailureActions.Should().Be(1);
        result.DistinctUsers.Should().Be(2);
        result.TopUserLabel.Should().Be("marco@example.com");
        result.TopEntityName.Should().Be("Escala");
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByEntityActionUserIdAndPeriod()
    {
        await using var context = await CreateContextAsync();
        context.AuditLogs.AddRange(
            new AuditLog
            {
                EntityName = "MensagemAgendada",
                EntityId = "11",
                Action = "ErroEnvio",
                UserId = 10,
                UserName = "Marco",
                UserEmail = "marco@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "MensagemAgendada",
                EntityId = "12",
                Action = "Enviado",
                UserId = 10,
                UserName = "Marco",
                UserEmail = "marco@example.com",
                CreatedAt = new DateTime(2026, 4, 6, 14, 0, 0, DateTimeKind.Utc)
            },
            new AuditLog
            {
                EntityName = "Escala",
                EntityId = "11",
                Action = "ErroEnvio",
                UserId = 11,
                UserName = "Aline",
                UserEmail = "aline@example.com",
                CreatedAt = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc)
            });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetPagedAsync(new AuditLogPagedQueryDto
        {
            EntityName = "MensagemAgendada",
            EntityId = "11",
            Action = "ErroEnvio",
            UserId = 10,
            From = new DateTime(2026, 4, 6, 0, 0, 0),
            To = new DateTime(2026, 4, 6, 12, 0, 0)
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].EntityId.Should().Be("11");
        result.Items[0].Action.Should().Be("ErroEnvio");
    }

    [Fact]
    public async Task RecordAsync_PersistsCurrentUserDataAndChangesJson()
    {
        await using var context = await CreateContextAsync();
        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.SetupGet(c => c.UserId).Returns(15);
        currentUser.SetupGet(c => c.UserName).Returns("Marco");
        currentUser.SetupGet(c => c.UserEmail).Returns("marco@example.com");
        currentUser.SetupGet(c => c.IpAddress).Returns("127.0.0.1");

        var service = new AuditLogService(context, currentUser.Object);

        await service.RecordAsync("ComunicacaoCampanha", "8", "CriarCampanha", new { Nome = "Páscoa" });

        var log = await context.AuditLogs.SingleAsync();
        log.EntityName.Should().Be("ComunicacaoCampanha");
        log.EntityId.Should().Be("8");
        log.UserId.Should().Be(15);
        log.UserEmail.Should().Be("marco@example.com");
        log.ChangesJson.Should().Contain("\"Nome\"");
        log.ChangesJson.Should().Contain("P\\u00E1scoa");
    }

    [Fact]
    public async Task RecordAsync_AllowsNullChanges()
    {
        await using var context = await CreateContextAsync();
        var service = CreateService(context);

        await service.RecordAsync("Usuario", "3", "Delete");

        var log = await context.AuditLogs.SingleAsync();
        log.EntityName.Should().Be("Usuario");
        log.EntityId.Should().Be("3");
        log.Action.Should().Be("Delete");
        log.ChangesJson.Should().BeNull();
    }

    [Fact]
    public async Task GetMetricsAsync_WhenThereAreNoLogs_ReturnsZeroedMetrics()
    {
        await using var context = await CreateContextAsync();
        var service = CreateService(context);

        var result = await service.GetMetricsAsync(new AuditLogPagedQueryDto());

        result.TotalLogs.Should().Be(0);
        result.CriticalActions.Should().Be(0);
        result.FailureActions.Should().Be(0);
        result.DistinctUsers.Should().Be(0);
        result.TopUserLabel.Should().BeNull();
        result.TopEntityName.Should().BeNull();
        result.TopActionName.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_UsesPaginationDefaultsAndMaxPageSize()
    {
        await using var context = await CreateContextAsync();
        context.AuditLogs.AddRange(
            Enumerable.Range(1, 25).Select(i => new AuditLog
            {
                EntityName = "Escala",
                EntityId = i.ToString(),
                Action = "Publicar",
                CreatedAt = new DateTime(2026, 4, 6, 10, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            }));
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetPagedAsync(new AuditLogPagedQueryDto
        {
            Page = 0,
            PageSize = 500
        });

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(200);
        result.Total.Should().Be(25);
        result.Items.Should().HaveCount(25);
        result.Items.First().EntityId.Should().Be("25");
    }

    private static AuditLogService CreateService(SistemaIgrejaDbContext context)
    {
        var currentUser = new Mock<ICurrentUserContext>();
        return new AuditLogService(context, currentUser.Object);
    }

    private static async Task<SistemaIgrejaDbContext> CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new SistemaIgrejaDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
