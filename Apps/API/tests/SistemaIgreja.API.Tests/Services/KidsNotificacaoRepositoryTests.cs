using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class KidsNotificacaoRepositoryTests
{
    [Fact]
    public async Task QueryMethods_FilterFeedAndAdministrativeViews()
    {
        await using var context = await CreateContextAsync();
        var crianca = await SeedPessoaAsync(context, "Luna", TipoPessoa.Crianca);
        var responsavel = await SeedPessoaAsync(context, "Patricia", TipoPessoa.Adulto);

        context.KidsNotificacoes.AddRange(
            new KidsNotificacao
            {
                CriancaPessoaId = crianca.Id,
                ResponsavelPessoaId = responsavel.Id,
                Titulo = "Check-in",
                Tipo = "CHECKIN",
                Origem = "AUTOMATICA",
                Mensagem = "Entrada registrada",
                Status = "Enviado",
                DataCriacao = new DateTime(2026, 4, 8, 10, 0, 0)
            },
            new KidsNotificacao
            {
                CriancaPessoaId = crianca.Id,
                ResponsavelPessoaId = responsavel.Id,
                Titulo = "Aviso",
                Tipo = "ALERTA",
                Origem = "MANUAL",
                Mensagem = "Trazer casaco",
                Status = "Pendente",
                DataCriacao = new DateTime(2026, 4, 8, 9, 0, 0)
            });
        await context.SaveChangesAsync();

        var repository = new KidsNotificacaoRepository(context);

        var byCrianca = (await repository.GetByCriancaIdAsync(crianca.Id)).ToList();
        byCrianca.Should().HaveCount(2);
        byCrianca[0].Titulo.Should().Be("Check-in");

        var feed = (await repository.GetFeedByResponsavelIdAsync(responsavel.Id, tipo: "checkin", criancaPessoaId: crianca.Id, limit: 5)).ToList();
        feed.Should().ContainSingle();
        feed[0].Tipo.Should().Be("CHECKIN");

        var administrativos = (await repository.GetAdministrativosAsync(tipo: "alerta", responsavelPessoaId: responsavel.Id)).ToList();
        administrativos.Should().ContainSingle();
        administrativos[0].Origem.Should().Be("MANUAL");

        var pendentes = (await repository.GetPendentesAsync()).ToList();
        pendentes.Should().ContainSingle();
        pendentes[0].Status.Should().Be("Pendente");
    }

    [Fact]
    public async Task CreateRangeAndUpdateAsync_PersistNotifications()
    {
        await using var context = await CreateContextAsync();
        var responsavel = await SeedPessoaAsync(context, "Daniela", TipoPessoa.Adulto);
        var repository = new KidsNotificacaoRepository(context);

        await repository.CreateRangeAsync(
        [
            new KidsNotificacao
            {
                ResponsavelPessoaId = responsavel.Id,
                Titulo = "Aviso 1",
                Tipo = "AVISO_GERAL",
                Origem = "MANUAL",
                Mensagem = "Mensagem 1",
                Status = "Pendente"
            },
            new KidsNotificacao
            {
                ResponsavelPessoaId = responsavel.Id,
                Titulo = "Aviso 2",
                Tipo = "AVISO_GERAL",
                Origem = "MANUAL",
                Mensagem = "Mensagem 2",
                Status = "Pendente"
            }
        ]);

        var created = await repository.CreateAsync(new KidsNotificacao
        {
            ResponsavelPessoaId = responsavel.Id,
            Titulo = "Aviso 3",
            Tipo = "AVISO_GERAL",
            Origem = "MANUAL",
            Mensagem = "Mensagem 3",
            Status = "Pendente"
        });

        created.LidoEm = new DateTime(2026, 4, 8, 12, 0, 0);
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.LidoEm.Should().NotBeNull();
        context.KidsNotificacoes.Should().HaveCount(3);
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
