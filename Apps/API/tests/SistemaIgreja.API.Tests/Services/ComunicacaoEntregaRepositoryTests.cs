using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoEntregaRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_FiltersByCampaignStatusAndText()
    {
        await using var context = await CreateContextAsync();
        var campanha = new ComunicacaoCampanha
        {
            Nome = "Campanha",
            Objetivo = "Obj",
            PublicoAlvo = "visitantes",
            Status = StatusComunicacaoCampanha.Rascunho,
            Origem = TipoOrigemComunicacao.Manual,
            DataCriacao = DateTime.UtcNow
        };
        context.ComunicacaoCampanhas.Add(campanha);
        await context.SaveChangesAsync();

        context.ComunicacaoEntregas.AddRange(
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha.Id,
                Canal = CanalComunicacao.WhatsApp,
                DestinoResolvido = "5511999999999",
                ConteudoFinal = "Mensagem visitante",
                Status = StatusComunicacaoEntrega.Pendente,
                DataCriacao = DateTime.UtcNow
            },
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha.Id,
                Canal = CanalComunicacao.Email,
                DestinoResolvido = "teste@example.com",
                ConteudoFinal = "Outro conteudo",
                Status = StatusComunicacaoEntrega.Falhou,
                Erro = "SMTP",
                DataCriacao = DateTime.UtcNow.AddMinutes(-5)
            });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoEntregaRepository(context);

        var result = await repository.GetPagedAsync(new ComunicacaoEntregaPagedQueryDto
        {
            CampanhaId = campanha.Id,
            Status = StatusComunicacaoEntrega.Pendente,
            Texto = "visitante"
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Canal.Should().Be(CanalComunicacao.WhatsApp);
    }

    [Fact]
    public async Task GetByCampanhaIdAsync_ReturnsNewestFirst()
    {
        await using var context = await CreateContextAsync();
        var campanha = new ComunicacaoCampanha
        {
            Nome = "Campanha",
            Objetivo = "Obj",
            PublicoAlvo = "visitantes",
            Status = StatusComunicacaoCampanha.Rascunho,
            Origem = TipoOrigemComunicacao.Manual,
            DataCriacao = DateTime.UtcNow
        };
        context.ComunicacaoCampanhas.Add(campanha);
        await context.SaveChangesAsync();

        context.ComunicacaoEntregas.AddRange(
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha.Id,
                Canal = CanalComunicacao.WhatsApp,
                DestinoResolvido = "1",
                ConteudoFinal = "Antiga",
                Status = StatusComunicacaoEntrega.Pendente,
                DataCriacao = new DateTime(2026, 4, 6, 10, 0, 0)
            },
            new ComunicacaoEntrega
            {
                ComunicacaoCampanhaId = campanha.Id,
                Canal = CanalComunicacao.WhatsApp,
                DestinoResolvido = "2",
                ConteudoFinal = "Recente",
                Status = StatusComunicacaoEntrega.Enviado,
                DataCriacao = new DateTime(2026, 4, 6, 11, 0, 0)
            });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoEntregaRepository(context);

        var result = await repository.GetByCampanhaIdAsync(campanha.Id);

        result.Should().HaveCount(2);
        result[0].ConteudoFinal.Should().Be("Recente");
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
