using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class CriancaDetalheRepositoryTests
{
    [Fact]
    public async Task GetByPessoaIdAsync_ReturnsPersistedDetail()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Bia", TipoPessoa.Crianca);
        context.CriancasDetalhes.Add(new CriancaDetalhe
        {
            PessoaId = crianca.Id,
            Alergias = "Lactose",
            SalaId = "Sala-1"
        });
        await context.SaveChangesAsync();

        var repository = new CriancaDetalheRepository(context);

        var result = await repository.GetByPessoaIdAsync(crianca.Id);

        result.Should().NotBeNull();
        result!.Pessoa.Nome.Should().Be("Bia");
        result.Alergias.Should().Be("Lactose");
    }

    [Fact]
    public async Task CreateAsync_PersistsDetailImmediately()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Isa", TipoPessoa.Crianca);
        var repository = new CriancaDetalheRepository(context);

        var created = await repository.CreateAsync(new CriancaDetalhe
        {
            PessoaId = crianca.Id,
            SalaId = "Sala-2",
            RestricoesAlimentares = "Sem leite"
        });

        created.PessoaId.Should().Be(crianca.Id);
        var loaded = await repository.GetByPessoaIdAsync(crianca.Id);
        loaded.Should().NotBeNull();
        loaded!.SalaId.Should().Be("Sala-2");
    }

    [Fact]
    public async Task CreateWithoutSaveUpdateAndDeleteAsync_WorkAsExpected()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Theo", TipoPessoa.Crianca);
        var repository = new CriancaDetalheRepository(context);

        var detalhe = await repository.CreateWithoutSaveAsync(new CriancaDetalhe
        {
            PessoaId = crianca.Id,
            RestricoesAlimentares = "Sem gluten"
        });
        await context.SaveChangesAsync();

        detalhe.Observacoes = "Observacao atualizada";
        await repository.UpdateAsync(detalhe);

        var loaded = await repository.GetByPessoaIdAsync(crianca.Id);
        loaded.Should().NotBeNull();
        loaded!.Observacoes.Should().Be("Observacao atualizada");

        await repository.DeleteAsync(crianca.Id);
        (await repository.GetByPessoaIdAsync(crianca.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_IgnoresMissingEntity()
    {
        await using var context = await CreateContextAsync();
        var repository = new CriancaDetalheRepository(context);

        var action = () => repository.DeleteAsync(999);

        await action.Should().NotThrowAsync();
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context, string nome, TipoPessoa tipoPessoa)
    {
        var pessoa = new Pessoa { Nome = nome, TipoPessoa = tipoPessoa, Ativo = true, DataCriacao = DateTime.UtcNow };
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
