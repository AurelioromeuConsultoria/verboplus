using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class KidsEstruturaRepositoryTests
{
    [Fact]
    public async Task GetSalasAndTurmasAsync_FilterAndOrderCorrectly()
    {
        await using var context = await CreateContextAsync();
        context.KidsSalas.AddRange(
            new KidsSala { Id = "S2", Nome = "Berçario", Ativo = false },
            new KidsSala { Id = "S1", Nome = "Infantil", Ativo = true });
        context.KidsTurmas.AddRange(
            new KidsTurma { Id = "T2", SalaId = "S1", Nome = "4-5 anos", Ativo = true },
            new KidsTurma { Id = "T1", SalaId = "S1", Nome = "2-3 anos", Ativo = true },
            new KidsTurma { Id = "T3", SalaId = "S2", Nome = "Bebes", Ativo = false });
        await context.SaveChangesAsync();

        var repository = new KidsEstruturaRepository(context);

        var salas = (await repository.GetSalasAsync()).ToList();
        salas.Should().ContainSingle();
        salas[0].Id.Should().Be("S1");

        var todasSalas = (await repository.GetSalasAsync(true)).ToList();
        todasSalas.Should().HaveCount(2);
        todasSalas.Select(x => x.Nome).Should().ContainInOrder("Berçario", "Infantil");

        var turmasSala = (await repository.GetTurmasAsync("S1")).ToList();
        turmasSala.Should().HaveCount(2);
        turmasSala.Select(x => x.Nome).Should().ContainInOrder("2-3 anos", "4-5 anos");

        var todasTurmas = (await repository.GetTurmasAsync(incluirInativas: true)).ToList();
        todasTurmas.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateAndUpdateSalaAndTurma_PersistData()
    {
        await using var context = await CreateContextAsync();
        var repository = new KidsEstruturaRepository(context);

        var sala = await repository.CreateSalaAsync(new KidsSala { Id = "S10", Nome = "Sala A", Ativo = true });
        sala.Nome = "Sala Atualizada";
        await repository.UpdateSalaAsync(sala);

        var turma = await repository.CreateTurmaAsync(new KidsTurma { Id = "T10", SalaId = sala.Id, Nome = "Turma A", Ativo = true });
        turma.Nome = "Turma Atualizada";
        await repository.UpdateTurmaAsync(turma);

        (await repository.GetSalaByIdAsync("S10"))!.Nome.Should().Be("Sala Atualizada");
        (await repository.GetTurmaByIdAsync("T10"))!.Nome.Should().Be("Turma Atualizada");
        (await repository.GetTurmaByIdAsync("T10"))!.Sala.Should().NotBeNull();
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
