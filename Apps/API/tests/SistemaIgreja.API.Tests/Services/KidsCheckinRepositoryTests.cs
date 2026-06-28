using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class KidsCheckinRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedCheckins()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Luca", TipoPessoa.Crianca);
        var operador = await SeedPessoaAsync(context, "Paulo", TipoPessoa.Adulto);

        context.KidsCheckins.AddRange(
            new KidsCheckin
            {
                CriancaPessoaId = crianca.Id,
                CheckinByPessoaId = operador.Id,
                Metodo = "ADMIN",
                CodigoSessao = "S2",
                TokenRetirada = "TK2",
                PinRetirada = "2222",
                Status = "CheckedOut",
                CheckinTime = new DateTime(2026, 4, 8, 9, 0, 0)
            },
            new KidsCheckin
            {
                CriancaPessoaId = crianca.Id,
                CheckinByPessoaId = operador.Id,
                Metodo = "PIN",
                CodigoSessao = "S1",
                TokenRetirada = "TK1",
                PinRetirada = "1111",
                Status = "CheckedIn",
                CheckinTime = new DateTime(2026, 4, 8, 10, 0, 0)
            });
        await context.SaveChangesAsync();

        var repository = new KidsCheckinRepository(context);

        (await repository.GetCheckinAtivoPorCriancaAsync(crianca.Id))!.CodigoSessao.Should().Be("S1");
        (await repository.GetByCodigoSessaoAsync("S1"))!.Crianca.Nome.Should().Be("Luca");
        (await repository.GetByTokenRetiradaAsync("TK1"))!.CodigoSessao.Should().Be("S1");
        (await repository.GetByPinRetiradaAsync("1111"))!.CodigoSessao.Should().Be("S1");

        var historico = (await repository.GetHistoricoPorCriancaAsync(crianca.Id, 1)).ToList();
        historico.Should().ContainSingle();
        historico[0].CodigoSessao.Should().Be("S1");

        var ativos = (await repository.GetCheckinsAtivosAsync()).ToList();
        ativos.Should().ContainSingle();

        var periodo = (await repository.GetByPeriodoAsync(new DateTime(2026, 4, 8, 8, 0, 0), new DateTime(2026, 4, 8, 11, 0, 0))).ToList();
        periodo.Should().HaveCount(2);
        periodo[0].CodigoSessao.Should().Be("S1");
    }

    [Fact]
    public async Task CreateWithoutSaveAndUpdateAsync_PersistCheckin()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Mia", TipoPessoa.Crianca);
        var operador = await SeedPessoaAsync(context, "Sara", TipoPessoa.Adulto);
        var repository = new KidsCheckinRepository(context);

        var pending = await repository.CreateWithoutSaveAsync(new KidsCheckin
        {
            CriancaPessoaId = crianca.Id,
            CheckinByPessoaId = operador.Id,
            Metodo = "ADMIN",
            CodigoSessao = "S3",
            Status = "CheckedIn"
        });
        await context.SaveChangesAsync();

        pending.Status = "CheckedOut";
        await repository.UpdateAsync(pending);

        var loaded = await repository.GetByIdAsync(pending.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be("CheckedOut");
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
