using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs.MensagensAgendadas;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class MensagemAgendadaRepositoryTests
{
    [Fact]
    public async Task GetStatsAsync_CalculatesCountsByStatus()
    {
        await using var context = await CreateContextAsync();
        var deps = await SeedBaseDependenciasAsync(context);
        context.MensagensAgendadas.AddRange(
            CriarMensagem(StatusMensagem.Agendada, new DateTime(2026, 4, 5), deps.VisitanteId, deps.ConfiguracaoId),
            CriarMensagem(StatusMensagem.ProntaParaEnvio, new DateTime(2026, 4, 6), deps.VisitanteId, deps.ConfiguracaoId),
            CriarMensagem(StatusMensagem.Enviada, new DateTime(2026, 4, 7), deps.VisitanteId, deps.ConfiguracaoId),
            CriarMensagem(StatusMensagem.Erro, new DateTime(2026, 4, 8), deps.VisitanteId, deps.ConfiguracaoId));
        await context.SaveChangesAsync();

        var repository = new MensagemAgendadaRepository(context);

        var result = await repository.GetStatsAsync();

        result.Total.Should().Be(4);
        result.Agendadas.Should().Be(2);
        result.Enviadas.Should().Be(1);
        result.Erro.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByTextAndStatus()
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

        var visitante = new Visitante
        {
            PessoaId = pessoa.Id,
            DataVisita = new DateTime(2026, 4, 1),
            DataCadastro = new DateTime(2026, 4, 1)
        };
        var config = new ConfiguracaoMensagem
        {
            Nome = "Boas-vindas",
            TextoMensagem = "Mensagem",
            DiasAposVisita = 1,
            HorarioEnvio = new TimeSpan(9, 0, 0),
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Visitantes.Add(visitante);
        context.ConfiguracoesMensagens.Add(config);
        await context.SaveChangesAsync();

        context.MensagensAgendadas.AddRange(
            new MensagemAgendada
            {
                VisitanteId = visitante.Id,
                ConfiguracaoMensagemId = config.Id,
                DataAgendamento = new DateTime(2026, 4, 2),
                DataEnvio = new DateTime(2026, 4, 3),
                Status = StatusMensagem.Agendada,
                TextoFinal = "Olá Marco",
                DataCriacao = new DateTime(2026, 4, 2)
            },
            new MensagemAgendada
            {
                VisitanteId = visitante.Id,
                ConfiguracaoMensagemId = config.Id,
                DataAgendamento = new DateTime(2026, 4, 4),
                DataEnvio = new DateTime(2026, 4, 5),
                Status = StatusMensagem.Enviada,
                TextoFinal = "Outra mensagem",
                DataCriacao = new DateTime(2026, 4, 4)
            });
        await context.SaveChangesAsync();

        var repository = new MensagemAgendadaRepository(context);

        var result = await repository.GetPagedAsync(new MensagemAgendadaPagedQuery
        {
            Texto = "marco",
            Status = StatusMensagem.Agendada
        });

        result.Total.Should().Be(1);
        result.Items[0].TextoFinal.Should().Be("Olá Marco");
    }

    [Fact]
    public async Task CancelarPendentesPorVisitanteAsync_UpdatesOnlyNonSentMessages()
    {
        await using var context = await CreateContextAsync();
        var deps = await SeedBaseDependenciasAsync(context);
        context.MensagensAgendadas.AddRange(
            CriarMensagem(StatusMensagem.Agendada, new DateTime(2026, 4, 5), deps.VisitanteId, deps.ConfiguracaoId),
            CriarMensagem(StatusMensagem.Enviada, new DateTime(2026, 4, 6), deps.VisitanteId, deps.ConfiguracaoId));
        await context.SaveChangesAsync();

        var repository = new MensagemAgendadaRepository(context);

        var affected = await repository.CancelarPendentesPorVisitanteAsync(deps.VisitanteId, "Regenerada");

        affected.Should().Be(1);
        context.ChangeTracker.Clear();
        context.MensagensAgendadas.Single(m => m.Status == StatusMensagem.Cancelada).LogErro.Should().Be("Regenerada");
        context.MensagensAgendadas.Single(m => m.Status == StatusMensagem.Enviada).LogErro.Should().BeNull();
    }

    private static MensagemAgendada CriarMensagem(StatusMensagem status, DateTime dataEnvio, int visitanteId, int configuracaoId)
    {
        return new MensagemAgendada
        {
            VisitanteId = visitanteId,
            ConfiguracaoMensagemId = configuracaoId,
            DataAgendamento = dataEnvio.AddDays(-1),
            DataEnvio = dataEnvio,
            Status = status,
            TextoFinal = $"Mensagem {status}",
            DataCriacao = dataEnvio.AddDays(-1)
        };
    }

    private static async Task<(int VisitanteId, int ConfiguracaoId)> SeedBaseDependenciasAsync(SistemaIgrejaDbContext context)
    {
        var pessoa = new Pessoa
        {
            Nome = "Pessoa Base",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        var visitante = new Visitante
        {
            PessoaId = pessoa.Id,
            DataVisita = new DateTime(2026, 4, 1),
            DataCadastro = new DateTime(2026, 4, 1)
        };

        var configuracao = new ConfiguracaoMensagem
        {
            Nome = "Base",
            TextoMensagem = "Mensagem base",
            DiasAposVisita = 1,
            HorarioEnvio = new TimeSpan(9, 0, 0),
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        context.Visitantes.Add(visitante);
        context.ConfiguracoesMensagens.Add(configuracao);

        await context.SaveChangesAsync();
        return (visitante.Id, configuracao.Id);
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
