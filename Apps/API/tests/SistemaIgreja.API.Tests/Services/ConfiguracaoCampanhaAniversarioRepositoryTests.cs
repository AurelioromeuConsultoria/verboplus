using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ConfiguracaoCampanhaAniversarioRepositoryTests
{
    [Fact]
    public async Task GetAsync_CreatesDefaultConfiguration_WhenDatabaseIsEmpty()
    {
        await using var context = await CreateContextAsync();
        var repository = new ConfiguracaoCampanhaAniversarioRepository(context);

        var result = await repository.GetAsync();

        result.Id.Should().Be(1);
        result.Ativo.Should().BeTrue();
        result.MensagemTemplate.Should().NotBeNullOrWhiteSpace();
        context.ConfiguracoesCampanhaAniversario.Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateAsync_CreatesConfiguration_WhenDatabaseIsEmpty()
    {
        await using var context = await CreateContextAsync();
        var repository = new ConfiguracaoCampanhaAniversarioRepository(context);

        var result = await repository.UpdateAsync(new()
        {
            Ativo = false,
            ImagemUrl = "/uploads/teste.png",
            MensagemTemplate = "Parabens, {Nome}",
            HorarioEnvio = new TimeSpan(8, 30, 0)
        });

        result.Id.Should().Be(1);
        result.Ativo.Should().BeFalse();
        result.ImagemUrl.Should().Be("/uploads/teste.png");
        result.HorarioEnvio.Should().Be(new TimeSpan(8, 30, 0));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingConfiguration()
    {
        await using var context = await CreateContextAsync();
        context.ConfiguracoesCampanhaAniversario.Add(new()
        {
            Ativo = true,
            ImagemUrl = "/assets/original.png",
            MensagemTemplate = "Original",
            HorarioEnvio = new TimeSpan(9, 0, 0),
            DataAtualizacao = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var repository = new ConfiguracaoCampanhaAniversarioRepository(context);

        var result = await repository.UpdateAsync(new()
        {
            Ativo = false,
            ImagemUrl = "/assets/novo.png",
            MensagemTemplate = "Atualizada",
            HorarioEnvio = new TimeSpan(10, 15, 0)
        });

        result.Ativo.Should().BeFalse();
        result.ImagemUrl.Should().Be("/assets/novo.png");
        result.MensagemTemplate.Should().Be("Atualizada");
        result.HorarioEnvio.Should().Be(new TimeSpan(10, 15, 0));
        context.ConfiguracoesCampanhaAniversario.Should().ContainSingle();
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
