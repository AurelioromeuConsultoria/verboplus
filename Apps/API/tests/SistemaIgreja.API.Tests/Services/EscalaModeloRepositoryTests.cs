using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EscalaModeloRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedModelsAndFallback()
    {
        await using var context = await CreateContextAsync();
        var evento = await SeedEventoAsync(context, "Culto de Domingo");
        var outraEquipe = await SeedEquipeAsync(context, "Audio");
        var equipe = await SeedEquipeAsync(context, "Louvor");
        var cargo = await SeedCargoAsync(context, "Vocal");

        context.EscalasModelos.AddRange(
            new EscalaModelo
            {
                EquipeId = equipe.Id,
                EventoId = null,
                Nome = "Padrao",
                Ativo = true,
                Itens =
                [
                    new EscalaModeloItem { CargoId = cargo.Id, Quantidade = 2, Ordem = 2 }
                ]
            },
            new EscalaModelo
            {
                EquipeId = equipe.Id,
                EventoId = evento.Id,
                Nome = "Especial",
                Ativo = true,
                Itens =
                [
                    new EscalaModeloItem { CargoId = cargo.Id, Quantidade = 1, Ordem = 1 }
                ]
            },
            new EscalaModelo
            {
                EquipeId = outraEquipe.Id,
                EventoId = evento.Id,
                Nome = "Audio Especial",
                Ativo = true
            });
        await context.SaveChangesAsync();

        var repository = new EscalaModeloRepository(context);

        var byEventoEquipe = await repository.GetByEventoAndEquipeAsync(evento.Id, equipe.Id);
        byEventoEquipe.Should().NotBeNull();
        byEventoEquipe!.Nome.Should().Be("Especial");
        byEventoEquipe.Itens.Should().ContainSingle();
        byEventoEquipe.Itens.First().Cargo.Should().NotBeNull();

        var fallback = await repository.GetByEventoAndEquipeAsync(9999, equipe.Id);
        fallback.Should().NotBeNull();
        fallback!.Nome.Should().Be("Padrao");

        var byEquipe = (await repository.GetByEquipeAsync(equipe.Id)).ToList();
        byEquipe.Should().HaveCount(2);
        byEquipe[0].EventoId.Should().BeNull();
        byEquipe[1].EventoId.Should().Be(evento.Id);

        var byEvento = (await repository.GetByEventoAsync(evento.Id)).ToList();
        byEvento.Should().HaveCount(2);
        byEvento.Select(x => x.Equipe.Nome).Should().ContainInOrder("Audio", "Louvor");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistModelo()
    {
        await using var context = await CreateContextAsync();
        var equipe = await SeedEquipeAsync(context, "Recepcao");
        var repository = new EscalaModeloRepository(context);

        var created = await repository.CreateAsync(new EscalaModelo
        {
            EquipeId = equipe.Id,
            Nome = "Modelo Inicial",
            Ativo = true
        });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Modelo Atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Modelo Atualizado");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    private static async Task<Evento> SeedEventoAsync(SistemaIgrejaDbContext context, string titulo)
    {
        var evento = new Evento
        {
            Titulo = titulo,
            DataInicio = new DateTime(2026, 4, 1),
            DataFim = new DateTime(2026, 4, 1),
            Tipo = TipoEvento.Culto
        };
        context.Eventos.Add(evento);
        await context.SaveChangesAsync();
        return evento;
    }

    private static async Task<Equipe> SeedEquipeAsync(SistemaIgrejaDbContext context, string nome)
    {
        var equipe = new Equipe { Nome = nome, Area = AreaEquipe.Verde };
        context.Equipes.Add(equipe);
        await context.SaveChangesAsync();
        return equipe;
    }

    private static async Task<Cargo> SeedCargoAsync(SistemaIgrejaDbContext context, string nome)
    {
        var cargo = new Cargo { Nome = nome };
        context.Cargos.Add(cargo);
        await context.SaveChangesAsync();
        return cargo;
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
