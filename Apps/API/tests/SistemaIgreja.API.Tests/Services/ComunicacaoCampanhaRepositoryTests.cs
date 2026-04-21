using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoCampanhaRepositoryTests
{
    [Fact]
    public async Task CreateAsyncAndUpdateAsync_PersistCampaignChanges()
    {
        await using var context = await CreateContextAsync();
        var repository = new ComunicacaoCampanhaRepository(context);

        var campanha = new ComunicacaoCampanha
        {
            Nome = "Campanha inicial",
            Objetivo = "Objetivo inicial",
            PublicoAlvo = "visitantes",
            Status = StatusComunicacaoCampanha.Rascunho,
            Origem = TipoOrigemComunicacao.Manual,
            DataCriacao = new DateTime(2026, 4, 7, 10, 0, 0)
        };

        var created = await repository.CreateAsync(campanha);
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Campanha atualizada";
        created.Objetivo = "Objetivo atualizado";
        created.Status = StatusComunicacaoCampanha.Agendada;

        await repository.UpdateAsync(created);

        var persisted = await repository.GetByIdAsync(created.Id);
        persisted.Should().NotBeNull();
        persisted!.Nome.Should().Be("Campanha atualizada");
        persisted.Objetivo.Should().Be("Objetivo atualizado");
        persisted.Status.Should().Be(StatusComunicacaoCampanha.Agendada);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByStatusPublicoAndText()
    {
        await using var context = await CreateContextAsync();
        context.ComunicacaoCampanhas.AddRange(
            new ComunicacaoCampanha
            {
                Nome = "Boas-vindas visitantes",
                Objetivo = "Onboarding",
                PublicoAlvo = "visitantes",
                Status = StatusComunicacaoCampanha.Agendada,
                Origem = TipoOrigemComunicacao.Manual,
                DataCriacao = new DateTime(2026, 4, 6, 10, 0, 0)
            },
            new ComunicacaoCampanha
            {
                Nome = "Aviso membros",
                Objetivo = "Engajamento",
                PublicoAlvo = "membros",
                Status = StatusComunicacaoCampanha.Rascunho,
                Origem = TipoOrigemComunicacao.Manual,
                DataCriacao = new DateTime(2026, 4, 6, 11, 0, 0)
            });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoCampanhaRepository(context);

        var result = await repository.GetPagedAsync(new ComunicacaoCampanhaPagedQueryDto
        {
            Status = StatusComunicacaoCampanha.Agendada,
            PublicoAlvo = "VISITANTES",
            Texto = "vindas"
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Nome.Should().Be("Boas-vindas visitantes");
    }

    [Fact]
    public async Task GetByIdAsync_LoadsChannelsTemplatesAndRecentDeliveries()
    {
        await using var context = await CreateContextAsync();
        var template = new ComunicacaoTemplate
        {
            Nome = "Template base",
            Objetivo = "Onboarding",
            Canal = CanalComunicacao.WhatsApp,
            Corpo = "Conteudo",
            VariaveisPermitidas = "nome",
            Status = StatusComunicacaoTemplate.Ativo
        };
        var campanha = new ComunicacaoCampanha
        {
            Nome = "Campanha 1",
            Objetivo = "Onboarding",
            PublicoAlvo = "visitantes",
            Status = StatusComunicacaoCampanha.Rascunho,
            Origem = TipoOrigemComunicacao.Manual,
            DataCriacao = DateTime.UtcNow,
            Canais =
            [
                new ComunicacaoCampanhaCanal
                {
                    Canal = CanalComunicacao.WhatsApp,
                    Prioridade = 1,
                    Template = template
                }
            ],
            Entregas =
            [
                new ComunicacaoEntrega
                {
                    Canal = CanalComunicacao.WhatsApp,
                    DestinoResolvido = "5511999999999",
                    ConteudoFinal = "Mensagem 1",
                    Status = StatusComunicacaoEntrega.Pendente,
                    DataCriacao = DateTime.UtcNow.AddMinutes(-10)
                },
                new ComunicacaoEntrega
                {
                    Canal = CanalComunicacao.WhatsApp,
                    DestinoResolvido = "5511888888888",
                    ConteudoFinal = "Mensagem 2",
                    Status = StatusComunicacaoEntrega.Enviado,
                    DataCriacao = DateTime.UtcNow
                }
            ]
        };
        context.ComunicacaoCampanhas.Add(campanha);
        await context.SaveChangesAsync();

        var repository = new ComunicacaoCampanhaRepository(context);

        var result = await repository.GetByIdAsync(campanha.Id);

        result.Should().NotBeNull();
        result!.Canais.Should().ContainSingle();
        result.Canais.First().Template.Should().NotBeNull();
        result.Entregas.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStatsAsync_AggregatesCampaignsAndDeliveries()
    {
        await using var context = await CreateContextAsync();
        var campanha1 = new ComunicacaoCampanha
        {
            Nome = "Campanha Rascunho",
            Objetivo = "Obj",
            PublicoAlvo = "visitantes",
            Status = StatusComunicacaoCampanha.Rascunho,
            Origem = TipoOrigemComunicacao.Manual,
            DataCriacao = DateTime.UtcNow
        };
        var campanha2 = new ComunicacaoCampanha
        {
            Nome = "Campanha Agendada",
            Objetivo = "Obj",
            PublicoAlvo = "membros",
            Status = StatusComunicacaoCampanha.Agendada,
            Origem = TipoOrigemComunicacao.Manual,
            DataCriacao = DateTime.UtcNow
        };
        context.ComunicacaoCampanhas.AddRange(campanha1, campanha2);
        await context.SaveChangesAsync();

        context.ComunicacaoEntregas.AddRange(
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha1.Id,
                Canal = CanalComunicacao.WhatsApp,
                DestinoResolvido = "1",
                ConteudoFinal = "a",
                Status = StatusComunicacaoEntrega.Pendente
            },
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha2.Id,
                Canal = CanalComunicacao.WhatsApp,
                DestinoResolvido = "2",
                ConteudoFinal = "b",
                Status = StatusComunicacaoEntrega.Enviado
            },
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha2.Id,
                Canal = CanalComunicacao.Email,
                DestinoResolvido = "3",
                ConteudoFinal = "c",
                Status = StatusComunicacaoEntrega.Falhou
            });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoCampanhaRepository(context);

        var stats = await repository.GetStatsAsync();

        stats.TotalCampanhas.Should().Be(2);
        stats.CampanhasRascunho.Should().Be(1);
        stats.CampanhasAgendadas.Should().Be(1);
        stats.EntregasPendentes.Should().Be(1);
        stats.EntregasEnviadas.Should().Be(1);
        stats.EntregasComFalha.Should().Be(1);
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
