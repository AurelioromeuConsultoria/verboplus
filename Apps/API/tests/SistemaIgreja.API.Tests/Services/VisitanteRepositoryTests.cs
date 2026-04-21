using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs.Visitantes;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class VisitanteRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_FiltersByNomeAndDateRange()
    {
        await using var context = await CreateContextAsync();
        var pessoa1 = new Pessoa
        {
            Nome = "Marco Aurelio",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        var pessoa2 = new Pessoa
        {
            Nome = "Aline Souza",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Pessoas.AddRange(pessoa1, pessoa2);
        await context.SaveChangesAsync();

        context.Visitantes.AddRange(
            new Visitante
            {
                PessoaId = pessoa1.Id,
                DataVisita = new DateTime(2026, 4, 5),
                DataCadastro = new DateTime(2026, 4, 5)
            },
            new Visitante
            {
                PessoaId = pessoa2.Id,
                DataVisita = new DateTime(2026, 4, 1),
                DataCadastro = new DateTime(2026, 4, 1)
            });
        await context.SaveChangesAsync();

        var repository = new VisitanteRepository(context);

        var result = await repository.GetPagedAsync(new VisitantePagedQuery
        {
            Nome = "marco",
            DataVisitaFrom = new DateTime(2026, 4, 4),
            DataVisitaTo = new DateTime(2026, 4, 6)
        });

        result.Total.Should().Be(1);
        result.Items[0].Pessoa!.Nome.Should().Be("Marco Aurelio");
    }

    [Fact]
    public async Task GetVisitantesPorPessoaAsync_ReturnsOrderedByMostRecentVisit()
    {
        await using var context = await CreateContextAsync();
        var pessoa = new Pessoa
        {
            Nome = "Marco",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        context.Visitantes.AddRange(
            new Visitante
            {
                PessoaId = pessoa.Id,
                DataVisita = new DateTime(2026, 4, 1),
                DataCadastro = new DateTime(2026, 4, 1)
            },
            new Visitante
            {
                PessoaId = pessoa.Id,
                DataVisita = new DateTime(2026, 4, 10),
                DataCadastro = new DateTime(2026, 4, 10)
            });
        await context.SaveChangesAsync();

        var repository = new VisitanteRepository(context);

        var result = (await repository.GetVisitantesPorPessoaAsync(pessoa.Id)).ToList();

        result.Should().HaveCount(2);
        result[0].DataVisita.Should().Be(new DateTime(2026, 4, 10));
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByEmailTelefoneWhatsAppAndSortsByNome()
    {
        await using var context = await CreateContextAsync();
        var pessoa1 = new Pessoa
        {
            Nome = "Marco Aurelio",
            Email = "marco@example.com",
            Telefone = "1133334444",
            WhatsApp = "11999999999",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        var pessoa2 = new Pessoa
        {
            Nome = "Aline Souza",
            Email = "aline@example.com",
            Telefone = "1144445555",
            WhatsApp = "11888888888",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Pessoas.AddRange(pessoa1, pessoa2);
        await context.SaveChangesAsync();

        context.Visitantes.AddRange(
            new Visitante
            {
                PessoaId = pessoa1.Id,
                DataVisita = new DateTime(2026, 4, 5),
                DataCadastro = new DateTime(2026, 4, 5)
            },
            new Visitante
            {
                PessoaId = pessoa2.Id,
                DataVisita = new DateTime(2026, 4, 6),
                DataCadastro = new DateTime(2026, 4, 6)
            });
        await context.SaveChangesAsync();

        var repository = new VisitanteRepository(context);

        var result = await repository.GetPagedAsync(new VisitantePagedQuery
        {
            Email = "marco",
            Telefone = "3333",
            WhatsApp = "9999",
            Sort = "nome",
            Direction = "asc"
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Pessoa!.Nome.Should().Be("Marco Aurelio");
    }

    [Fact]
    public async Task GetAllCreateUpdateDeleteAndPeriodQueries_WorkAsExpected()
    {
        await using var context = await CreateContextAsync();
        var pessoa = new Pessoa
        {
            Nome = "Marco",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        var repository = new VisitanteRepository(context);

        var created = await repository.CreateAsync(new Visitante
        {
            PessoaId = pessoa.Id,
            DataVisita = new DateTime(2026, 4, 7),
            DataCadastro = new DateTime(2026, 4, 7)
        });

        var semSalvar = await repository.CreateWithoutSaveAsync(new Visitante
        {
            PessoaId = pessoa.Id,
            DataVisita = new DateTime(2026, 4, 8),
            DataCadastro = new DateTime(2026, 4, 8)
        });
        semSalvar.Id.Should().Be(0);
        await context.SaveChangesAsync();

        var all = (await repository.GetAllAsync()).ToList();
        all.Should().HaveCount(2);

        created.DataVisita = new DateTime(2026, 4, 9);
        await repository.UpdateAsync(created);

        var byId = await repository.GetByIdAsync(created.Id);
        byId.Should().NotBeNull();
        byId!.DataVisita.Should().Be(new DateTime(2026, 4, 9));

        var porPeriodo = (await repository.GetVisitantesPorPeriodoAsync(new DateTime(2026, 4, 8), new DateTime(2026, 4, 10))).ToList();
        porPeriodo.Should().ContainSingle(v => v.Id == created.Id);

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
