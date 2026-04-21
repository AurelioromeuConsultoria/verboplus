using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoTemplateRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome()
    {
        await using var context = await CreateContextAsync();
        context.ComunicacaoTemplates.AddRange(
            new ComunicacaoTemplate { Nome = "Zeta", Canal = CanalComunicacao.WhatsApp, Corpo = "Oi" },
            new ComunicacaoTemplate { Nome = "Alpha", Canal = CanalComunicacao.Email, Corpo = "Oi", Assunto = "Assunto" });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoTemplateRepository(context);

        var result = await repository.GetAllAsync();

        result.Select(x => x.Nome).Should().ContainInOrder("Alpha", "Zeta");
    }

    [Fact]
    public async Task CreateAndUpdateAsync_PersistTemplate()
    {
        await using var context = await CreateContextAsync();
        var repository = new ComunicacaoTemplateRepository(context);

        var created = await repository.CreateAsync(new ComunicacaoTemplate
        {
            Nome = "Boas-vindas",
            Canal = CanalComunicacao.WhatsApp,
            Corpo = "Olá {Nome}"
        });

        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Boas-vindas Atualizada";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Boas-vindas Atualizada");
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
