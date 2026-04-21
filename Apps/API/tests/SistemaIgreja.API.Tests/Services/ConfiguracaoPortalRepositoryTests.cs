using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ConfiguracaoPortalRepositoryTests
{
    [Fact]
    public async Task GetAsync_CreatesDefaultConfiguration_WhenEmpty()
    {
        await using var context = await CreateContextAsync();
        var repository = new ConfiguracaoPortalRepository(context);

        var result = await repository.GetAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.TempoTransicaoCarrossel.Should().Be(5);
        context.ConfiguracoesPortal.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingConfiguration()
    {
        await using var context = await CreateContextAsync();
        var repository = new ConfiguracaoPortalRepository(context);
        await repository.GetAsync();

        var result = await repository.UpdateAsync(new ConfiguracaoPortal { TempoTransicaoCarrossel = 9 });

        result.Id.Should().Be(1);
        result.TempoTransicaoCarrossel.Should().Be(9);
        context.ConfiguracoesPortal.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateAsync_CreatesConfiguration_WhenMissing()
    {
        await using var context = await CreateContextAsync();
        var repository = new ConfiguracaoPortalRepository(context);

        var result = await repository.UpdateAsync(new ConfiguracaoPortal
        {
            TempoTransicaoCarrossel = 12
        });

        result.Id.Should().Be(1);
        result.TempoTransicaoCarrossel.Should().Be(12);
        context.ConfiguracoesPortal.Should().ContainSingle();
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
