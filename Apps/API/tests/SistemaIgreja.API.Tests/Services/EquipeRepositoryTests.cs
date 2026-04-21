using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EquipeRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome_WithLeaderIncluded()
    {
        await using var context = await CreateContextAsync();
        var lider = await SeedUsuarioAsync(context, "Marco");
        context.Set<Equipe>().AddRange(
            new Equipe { Nome = "Louvor", Area = AreaEquipe.Verde },
            new Equipe { Nome = "Audio", Area = AreaEquipe.Laranja, LiderUsuarioId = lider.Id });
        await context.SaveChangesAsync();

        var repository = new EquipeRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("Audio", "Louvor");
        result.First(x => x.Nome == "Audio").LiderUsuario.Should().NotBeNull();
    }

    [Fact]
    public async Task IsLiderUsuarioDaEquipeAsync_ReturnsExpectedValue()
    {
        await using var context = await CreateContextAsync();
        var lider = await SeedUsuarioAsync(context, "Lider");
        var equipe = new Equipe { Nome = "Recepcao", Area = AreaEquipe.Vermelha, LiderUsuarioId = lider.Id };
        context.Set<Equipe>().Add(equipe);
        await context.SaveChangesAsync();

        var repository = new EquipeRepository(context);

        (await repository.IsLiderUsuarioDaEquipeAsync(lider.Id, equipe.Id)).Should().BeTrue();
        (await repository.IsLiderUsuarioDaEquipeAsync(999, equipe.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistEquipe()
    {
        await using var context = await CreateContextAsync();
        var repository = new EquipeRepository(context);

        var created = await repository.CreateAsync(new Equipe { Nome = "Kids", Area = AreaEquipe.Verde });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Kids Atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Kids Atualizado");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    private static async Task<Usuario> SeedUsuarioAsync(SistemaIgrejaDbContext context, string nome)
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

        var usuario = new Usuario
        {
            PessoaId = pessoa.Id,
            EmailLogin = $"{nome.ToLower()}@app.com",
            SenhaHash = "hash",
            TipoUsuario = TipoUsuario.Admin,
            Ativo = true
        };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
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
