using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ContatoRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByMostRecentFirst()
    {
        await using var context = await CreateContextAsync();
        context.Set<Contato>().AddRange(
            new Contato { Nome = "Primeiro", WhatsApp = "111", Membro = true, Mensagem = "Oi", DataCriacao = new DateTime(2026, 4, 1) },
            new Contato { Nome = "Segundo", WhatsApp = "222", Membro = false, Mensagem = "Olá", DataCriacao = new DateTime(2026, 4, 10) });
        await context.SaveChangesAsync();

        var repository = new ContatoRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result[0].Nome.Should().Be("Segundo");
        result[1].Nome.Should().Be("Primeiro");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistContato()
    {
        await using var context = await CreateContextAsync();
        var repository = new ContatoRepository(context);

        var created = await repository.CreateAsync(new Contato
        {
            Nome = "Marco",
            WhatsApp = "5511999999999",
            Membro = true,
            Mensagem = "Preciso de oração"
        });
        created.Id.Should().BeGreaterThan(0);

        created.Mensagem = "Mensagem atualizada";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Mensagem.Should().Be("Mensagem atualizada");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
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
