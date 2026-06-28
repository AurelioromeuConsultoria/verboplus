using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class KidsOcorrenciaRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedOccurrences()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Nina", TipoPessoa.Crianca);
        var operador = await SeedPessoaAsync(context, "Marcos", TipoPessoa.Adulto);
        var checkin = new KidsCheckin { CriancaPessoaId = crianca.Id, CodigoSessao = "KS1", Metodo = "ADMIN", Status = "CheckedIn" };
        context.KidsCheckins.Add(checkin);
        await context.SaveChangesAsync();

        context.KidsOcorrencias.AddRange(
            new KidsOcorrencia
            {
                CriancaPessoaId = crianca.Id,
                CheckinId = checkin.Id,
                Tipo = "ALERTA",
                Titulo = "Febre",
                Descricao = "Temperatura alta",
                Status = "Aberta",
                RegistradoPorPessoaId = operador.Id,
                DataCriacao = new DateTime(2026, 4, 8, 10, 0, 0)
            },
            new KidsOcorrencia
            {
                CriancaPessoaId = crianca.Id,
                CheckinId = checkin.Id,
                Tipo = "OBSERVACAO",
                Titulo = "Resolvida",
                Descricao = "Ocorrencia encerrada",
                Status = "Encerrada",
                RegistradoPorPessoaId = operador.Id,
                DataCriacao = new DateTime(2026, 4, 8, 9, 0, 0)
            });
        await context.SaveChangesAsync();

        var repository = new KidsOcorrenciaRepository(context);

        var byCrianca = (await repository.GetByCriancaIdAsync(crianca.Id)).ToList();
        byCrianca.Should().HaveCount(2);
        byCrianca[0].Titulo.Should().Be("Febre");

        var abertas = (await repository.GetAbertasAsync()).ToList();
        abertas.Should().ContainSingle();
        abertas[0].Status.Should().NotBe("Encerrada");
    }

    [Fact]
    public async Task CreateUpdateAsync_PersistOccurrence()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Theo", TipoPessoa.Crianca);
        var operador = await SeedPessoaAsync(context, "Paula", TipoPessoa.Adulto);
        var repository = new KidsOcorrenciaRepository(context);

        var created = await repository.CreateAsync(new KidsOcorrencia
        {
            CriancaPessoaId = crianca.Id,
            Tipo = "ALERTA",
            Titulo = "Queda",
            Descricao = "Queda leve",
            Status = "Aberta",
            RegistradoPorPessoaId = operador.Id
        });
        created.Id.Should().BeGreaterThan(0);

        created.Status = "Encerrada";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be("Encerrada");
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
