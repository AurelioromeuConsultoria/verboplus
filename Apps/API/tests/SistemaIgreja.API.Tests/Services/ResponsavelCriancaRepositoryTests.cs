using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ResponsavelCriancaRepositoryTests
{
    [Fact]
    public async Task GetByIdAndCreateAsync_ReturnExpectedRelationship()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Lia", TipoPessoa.Crianca);
        var responsavel = await SeedPessoaAsync(context, "Marina", TipoPessoa.Adulto);
        var repository = new ResponsavelCriancaRepository(context);

        var created = await repository.CreateAsync(new ResponsavelCrianca
        {
            CriancaPessoaId = crianca.Id,
            ResponsavelPessoaId = responsavel.Id,
            PodeRetirar = true,
            Ativo = true
        });

        var loaded = await repository.GetByIdAsync(created.Id);

        loaded.Should().NotBeNull();
        loaded!.Crianca.Nome.Should().Be("Lia");
        loaded.Responsavel.Nome.Should().Be("Marina");
    }

    [Fact]
    public async Task QueryMethods_ReturnExpectedActiveRelationships()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Lia", TipoPessoa.Crianca);
        var mae = await SeedPessoaAsync(context, "Marina", TipoPessoa.Adulto);
        var pai = await SeedPessoaAsync(context, "Carlos", TipoPessoa.Adulto);

        context.ResponsaveisCriancas.AddRange(
            new ResponsavelCrianca { CriancaPessoaId = crianca.Id, ResponsavelPessoaId = mae.Id, PodeRetirar = true, Ativo = true },
            new ResponsavelCrianca { CriancaPessoaId = crianca.Id, ResponsavelPessoaId = pai.Id, PodeRetirar = false, Ativo = false });
        await context.SaveChangesAsync();

        var repository = new ResponsavelCriancaRepository(context);

        var byCrianca = (await repository.GetByCriancaIdAsync(crianca.Id)).ToList();
        byCrianca.Should().ContainSingle();
        byCrianca[0].Responsavel.Nome.Should().Be("Marina");

        var byResponsavel = (await repository.GetByResponsavelIdAsync(mae.Id)).ToList();
        byResponsavel.Should().ContainSingle();
        byResponsavel[0].Crianca.Nome.Should().Be("Lia");

        (await repository.GetResponsavelIdsAtivosAsync()).Should().ContainSingle().Which.Should().Be(mae.Id);
        (await repository.GetResponsavelIdsAtivosByCriancaIdsAsync([crianca.Id])).Should().ContainSingle().Which.Should().Be(mae.Id);
        (await repository.GetCriancaIdsAtivosByResponsavelIdAsync(mae.Id)).Should().ContainSingle().Which.Should().Be(crianca.Id);
        (await repository.ExisteVinculoAtivoAsync(crianca.Id, mae.Id)).Should().BeTrue();
        (await repository.PodeRetirarAsync(crianca.Id, mae.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task CreateWithoutSaveUpdateAndDeleteAsync_WorkAsExpected()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Noah", TipoPessoa.Crianca);
        var responsavel = await SeedPessoaAsync(context, "Julia", TipoPessoa.Adulto);
        var repository = new ResponsavelCriancaRepository(context);

        var pending = await repository.CreateWithoutSaveAsync(new ResponsavelCrianca
        {
            CriancaPessoaId = crianca.Id,
            ResponsavelPessoaId = responsavel.Id,
            PodeRetirar = false
        });
        await context.SaveChangesAsync();

        var byPair = await repository.GetByCriancaAndResponsavelAsync(crianca.Id, responsavel.Id);
        byPair.Should().NotBeNull();

        pending.PodeRetirar = true;
        await repository.UpdateAsync(pending);
        (await repository.PodeRetirarAsync(crianca.Id, responsavel.Id)).Should().BeTrue();

        await repository.DeleteAsync(pending.Id);
        (await repository.ExisteVinculoAtivoAsync(crianca.Id, responsavel.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task EmptyIdsAndInactiveRelationships_ReturnSafeDefaults()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Noah", TipoPessoa.Crianca);
        var responsavel = await SeedPessoaAsync(context, "Julia", TipoPessoa.Adulto);
        context.ResponsaveisCriancas.Add(new ResponsavelCrianca
        {
            CriancaPessoaId = crianca.Id,
            ResponsavelPessoaId = responsavel.Id,
            PodeRetirar = false,
            Ativo = false
        });
        await context.SaveChangesAsync();

        var repository = new ResponsavelCriancaRepository(context);

        (await repository.GetResponsavelIdsAtivosByCriancaIdsAsync([0, -1])).Should().BeEmpty();
        (await repository.PodeRetirarAsync(crianca.Id, responsavel.Id)).Should().BeFalse();
        (await repository.ExisteVinculoAtivoAsync(crianca.Id, responsavel.Id)).Should().BeFalse();
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
