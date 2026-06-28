using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ComunicacaoPreferenciaRepositoryTests
{
    [Fact]
    public async Task GetByPessoaIdAsync_ReturnsOrderedByCanal()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context);
        context.ComunicacaoPreferencias.AddRange(
            new ComunicacaoPreferencia { PessoaId = pessoa.Id, Canal = CanalComunicacao.Push, Status = StatusPreferenciaCanal.Permitido },
            new ComunicacaoPreferencia { PessoaId = pessoa.Id, Canal = CanalComunicacao.Email, Status = StatusPreferenciaCanal.Bloqueado });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoPreferenciaRepository(context);

        var result = await repository.GetByPessoaIdAsync(pessoa.Id);

        result.Should().HaveCount(2);
        result.Select(x => x.Canal).Should().ContainInOrder(CanalComunicacao.Email, CanalComunicacao.Push);
    }

    [Fact]
    public async Task GetByPessoaCanalAsync_ReturnsMatchingPreference()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context);
        context.ComunicacaoPreferencias.Add(new ComunicacaoPreferencia
        {
            PessoaId = pessoa.Id,
            Canal = CanalComunicacao.WhatsApp,
            Status = StatusPreferenciaCanal.Bloqueado
        });
        await context.SaveChangesAsync();

        var repository = new ComunicacaoPreferenciaRepository(context);

        var result = await repository.GetByPessoaCanalAsync(pessoa.Id, CanalComunicacao.WhatsApp);

        result.Should().NotBeNull();
        result!.Status.Should().Be(StatusPreferenciaCanal.Bloqueado);
    }

    [Fact]
    public async Task CreateAndUpdateAsync_PersistPreference()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context);
        var repository = new ComunicacaoPreferenciaRepository(context);

        var created = await repository.CreateAsync(new ComunicacaoPreferencia
        {
            PessoaId = pessoa.Id,
            Canal = CanalComunicacao.Email,
            Status = StatusPreferenciaCanal.Permitido
        });

        created.Id.Should().BeGreaterThan(0);

        created.Status = StatusPreferenciaCanal.Bloqueado;
        created.DataAtualizacao = DateTime.UtcNow;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByPessoaCanalAsync(pessoa.Id, CanalComunicacao.Email);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(StatusPreferenciaCanal.Bloqueado);
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context)
    {
        var pessoa = new Pessoa
        {
            Nome = "Marco",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
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
