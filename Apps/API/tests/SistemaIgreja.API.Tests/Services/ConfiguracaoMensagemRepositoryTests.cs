using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ConfiguracaoMensagemRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByDiasAposVisita()
    {
        await using var context = await CreateContextAsync();
        context.ConfiguracoesMensagens.AddRange(
            new ConfiguracaoMensagem { TextoMensagem = "D+5", DiasAposVisita = 5, HorarioEnvio = new TimeSpan(9, 0, 0), Ativo = true },
            new ConfiguracaoMensagem { TextoMensagem = "D+1", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(9, 0, 0), Ativo = true });
        await context.SaveChangesAsync();

        var repository = new ConfiguracaoMensagemRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Should().Contain(x => x.TextoMensagem == "D+1");
        result.Should().Contain(x => x.TextoMensagem == "D+5");
        result.Select(x => x.DiasAposVisita).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAtivasAsync_ReturnsOnlyActiveConfigurations()
    {
        await using var context = await CreateContextAsync();
        context.ConfiguracoesMensagens.AddRange(
            new ConfiguracaoMensagem { TextoMensagem = "Ativa", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(9, 0, 0), Ativo = true },
            new ConfiguracaoMensagem { TextoMensagem = "Inativa", DiasAposVisita = 2, HorarioEnvio = new TimeSpan(9, 0, 0), Ativo = false });
        await context.SaveChangesAsync();

        var repository = new ConfiguracaoMensagemRepository(context);

        var result = (await repository.GetAtivasAsync()).ToList();

        result.Should().Contain(x => x.TextoMensagem == "Ativa");
        result.Should().NotContain(x => x.TextoMensagem == "Inativa");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistChanges()
    {
        await using var context = await CreateContextAsync();
        var repository = new ConfiguracaoMensagemRepository(context);

        var created = await repository.CreateAsync(new ConfiguracaoMensagem
        {
            TextoMensagem = "Boas-vindas",
            DiasAposVisita = 3,
            HorarioEnvio = new TimeSpan(8, 0, 0),
            Ativo = true
        });

        created.Id.Should().BeGreaterThan(0);

        created.TextoMensagem = "Atualizada";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.TextoMensagem.Should().Be("Atualizada");

        await repository.DeleteAsync(created.Id);

        var deleted = await repository.GetByIdAsync(created.Id);
        deleted.Should().BeNull();
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
