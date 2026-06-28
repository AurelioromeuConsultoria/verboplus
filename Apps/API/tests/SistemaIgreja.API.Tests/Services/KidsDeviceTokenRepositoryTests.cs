using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class KidsDeviceTokenRepositoryTests
{
    [Fact]
    public async Task UpsertAsync_CreatesAndUpdatesNormalizedToken()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Adriana");
        var repository = new KidsDeviceTokenRepository(context);

        await repository.UpsertAsync(pessoa.Id, "token-1", "ios");
        await repository.UpsertAsync(pessoa.Id, "token-1", "web");

        context.KidsDeviceTokens.Should().ContainSingle();
        var token = context.KidsDeviceTokens.Single();
        token.Platform.Should().Be("ANDROID");
    }

    [Fact]
    public async Task GetTokensByPessoaIdsAsync_ReturnsDistinctMatchingTokens()
    {
        await using var context = await CreateContextAsync();
        var pessoaA = await SeedPessoaAsync(context, "A");
        var pessoaB = await SeedPessoaAsync(context, "B");
        context.KidsDeviceTokens.AddRange(
            new KidsDeviceToken { PessoaId = pessoaA.Id, FcmToken = "token-a", Platform = "ANDROID" },
            new KidsDeviceToken { PessoaId = pessoaB.Id, FcmToken = "token-b", Platform = "IOS" });
        await context.SaveChangesAsync();

        var repository = new KidsDeviceTokenRepository(context);

        var tokens = (await repository.GetTokensByPessoaIdsAsync([pessoaA.Id, pessoaA.Id, pessoaB.Id])).ToList();
        tokens.Should().BeEquivalentTo(["token-a", "token-b"]);
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context, string nome)
    {
        var pessoa = new Pessoa { Nome = nome, TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        return pessoa;
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
