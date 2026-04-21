using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EnvioCampanhaAniversarioRepositoryTests
{
    [Fact]
    public async Task QueryMethods_FilterOrderAndCountExpectedItems()
    {
        await using var context = await CreateContextAsync();
        var pessoaAna = await SeedPessoaAsync(context, "Ana Souza");
        var pessoaBruno = await SeedPessoaAsync(context, "Bruno Lima");

        context.EnviosCampanhaAniversario.AddRange(
            new EnvioCampanhaAniversario
            {
                PessoaId = pessoaAna.Id,
                AnoReferencia = 2026,
                DataAniversario = new DateTime(2026, 4, 8),
                Status = StatusEnvioCampanhaAniversario.Enviado,
                WhatsAppUtilizado = "5511999991111",
                DataCriacao = new DateTime(2026, 4, 7),
                DataUltimaTentativa = new DateTime(2026, 4, 8, 10, 0, 0)
            },
            new EnvioCampanhaAniversario
            {
                PessoaId = pessoaBruno.Id,
                AnoReferencia = 2026,
                DataAniversario = new DateTime(2026, 4, 8),
                Status = StatusEnvioCampanhaAniversario.Pendente,
                WhatsAppUtilizado = "5511999992222",
                DataCriacao = new DateTime(2026, 4, 7),
                DataUltimaTentativa = new DateTime(2026, 4, 8, 9, 0, 0)
            });
        await context.SaveChangesAsync();

        var repository = new EnvioCampanhaAniversarioRepository(context);

        var byPessoaAno = await repository.GetByPessoaAnoAsync(pessoaAna.Id, 2026);
        byPessoaAno.Should().NotBeNull();
        byPessoaAno!.Pessoa.Nome.Should().Be("Ana Souza");

        var recentes = await repository.GetRecentesAsync(1);
        recentes.Should().ContainSingle();
        recentes[0].Pessoa.Nome.Should().Be("Ana Souza");

        var historico = await repository.GetHistoricoAsync("bruno", "Pendente", 10);
        historico.Should().ContainSingle();
        historico[0].Pessoa.Nome.Should().Be("Bruno Lima");

        (await repository.CountAsync()).Should().Be(2);
        (await repository.CountByStatusAnoAsync(StatusEnvioCampanhaAniversario.Enviado, 2026)).Should().Be(1);
        (await repository.CountByStatusDataAsync(StatusEnvioCampanhaAniversario.Pendente, new DateTime(2026, 4, 8))).Should().Be(1);
        (await repository.CountPendentesAnoAsync(2026)).Should().Be(1);
    }

    [Fact]
    public async Task CreateUpdateAsync_PersistEnvio()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Carla");
        var repository = new EnvioCampanhaAniversarioRepository(context);

        var created = await repository.CreateAsync(new EnvioCampanhaAniversario
        {
            PessoaId = pessoa.Id,
            AnoReferencia = 2026,
            DataAniversario = new DateTime(2026, 4, 9),
            Status = StatusEnvioCampanhaAniversario.Pendente
        });
        created.Id.Should().BeGreaterThan(0);

        created.Status = StatusEnvioCampanhaAniversario.Enviado;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(StatusEnvioCampanhaAniversario.Enviado);
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
