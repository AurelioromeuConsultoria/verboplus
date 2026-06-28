using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class PessoaPerfilRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnActiveAndOrderedPerfis()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Marina");
        var antigo = new PessoaPerfil
        {
            PessoaId = pessoa.Id,
            Perfil = PerfilPessoa.Membro,
            DataInicio = new DateTime(2025, 1, 1),
            DataFim = new DateTime(2025, 12, 31)
        };
        var ativo = new PessoaPerfil
        {
            PessoaId = pessoa.Id,
            Perfil = PerfilPessoa.Voluntario,
            DataInicio = new DateTime(2026, 1, 1)
        };

        context.PessoasPerfis.AddRange(antigo, ativo);
        await context.SaveChangesAsync();

        var repository = new PessoaPerfilRepository(context);

        var perfilAtivo = await repository.GetPerfilAtivoAsync(pessoa.Id, PerfilPessoa.Voluntario);
        perfilAtivo.Should().NotBeNull();
        perfilAtivo!.DataFim.Should().BeNull();

        var perfis = (await repository.GetPerfisPorPessoaAsync(pessoa.Id)).ToList();
        perfis.Should().HaveCount(2);
        perfis[0].Perfil.Should().Be(PerfilPessoa.Voluntario);
        perfis[1].Perfil.Should().Be(PerfilPessoa.Membro);
        perfis[0].Pessoa.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateWithoutSaveAndDeleteAsync_PersistPerfil()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Rafaela");
        var repository = new PessoaPerfilRepository(context);

        var pending = await repository.CreateWithoutSaveAsync(new PessoaPerfil
        {
            PessoaId = pessoa.Id,
            Perfil = PerfilPessoa.Visitante,
            DataInicio = new DateTime(2026, 4, 1)
        });
        pending.Id.Should().Be(0);

        await context.SaveChangesAsync();
        pending.Id.Should().BeGreaterThan(0);

        pending.DataFim = new DateTime(2026, 4, 30);
        await repository.UpdateAsync(pending);

        var loaded = await repository.GetByIdAsync(pending.Id);
        loaded.Should().NotBeNull();
        loaded!.DataFim.Should().Be(new DateTime(2026, 4, 30));

        await repository.DeleteAsync(pending.Id);
        (await repository.GetByIdAsync(pending.Id)).Should().BeNull();
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context, string nome)
    {
        var pessoa = new Pessoa
        {
            Nome = nome,
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
