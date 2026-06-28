using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.API.Tests.Services;

public class AuditSaveChangesInterceptorTests
{
    [Fact]
    public async Task SaveChanges_CreatesAuditLog_ForAddedEntity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.SetupGet(c => c.UserId).Returns(42);
        currentUser.SetupGet(c => c.UserName).Returns("Marco");
        currentUser.SetupGet(c => c.UserEmail).Returns("marco@example.com");
        currentUser.SetupGet(c => c.IpAddress).Returns("127.0.0.1");

        var interceptor = new AuditSaveChangesInterceptor(currentUser.Object);
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new SistemaIgrejaDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.Pessoas.Add(new Pessoa
        {
            Nome = "Pessoa Auditada",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var logs = await context.AuditLogs
            .Where(l => l.EntityName == nameof(Pessoa))
            .OrderBy(l => l.Id)
            .ToListAsync();

        logs.Should().ContainSingle();
        logs[0].Action.Should().Be("Create");
        logs[0].UserId.Should().Be(42);
        logs[0].UserEmail.Should().Be("marco@example.com");
        logs[0].CreatedAt.Kind.Should().Be(DateTimeKind.Unspecified);
        logs[0].ChangesJson.Should().Contain("Pessoa Auditada");
    }

    [Fact]
    public async Task SaveChanges_DoesNotAudit_AuditLogEntityItself()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var currentUser = new Mock<ICurrentUserContext>();
        var interceptor = new AuditSaveChangesInterceptor(currentUser.Object);
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new SistemaIgrejaDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = "Manual",
            EntityId = "1",
            Action = "Seed",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var logs = await context.AuditLogs.ToListAsync();

        logs.Should().HaveCount(1);
        logs[0].EntityName.Should().Be("Manual");
    }

    [Fact]
    public async Task SaveChanges_CreatesAuditLog_ForModifiedAndDeletedEntity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var currentUser = new Mock<ICurrentUserContext>();
        currentUser.SetupGet(c => c.UserId).Returns(7);
        currentUser.SetupGet(c => c.UserName).Returns("Aline");
        currentUser.SetupGet(c => c.UserEmail).Returns("aline@example.com");

        var interceptor = new AuditSaveChangesInterceptor(currentUser.Object);
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new SistemaIgrejaDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var pessoa = new Pessoa
        {
            Nome = "Pessoa Original",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        pessoa.Nome = "Pessoa Alterada";
        await context.SaveChangesAsync();

        context.Pessoas.Remove(pessoa);
        await context.SaveChangesAsync();

        var logs = await context.AuditLogs
            .Where(l => l.EntityName == nameof(Pessoa))
            .OrderBy(l => l.Id)
            .ToListAsync();

        logs.Should().HaveCount(3);
        logs.Select(l => l.Action).Should().ContainInOrder("Create", "Update", "Delete");
        logs[1].ChangesJson.Should().Contain("Pessoa Alterada");
        logs[2].ChangesJson.Should().Contain("Pessoa Alterada");
    }
}
