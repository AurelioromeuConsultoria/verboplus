using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class UsuarioRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedUsersWithRelationships()
    {
        await using var context = await CreateContextAsync();
        var perfil = new PerfilAcesso
        {
            Nome = "Admin",
            Permissoes = [new PerfilAcessoPermissao { Recurso = "USUARIOS", PodeVer = true, PodeEditar = true }]
        };
        context.PerfisAcesso.Add(perfil);
        await context.SaveChangesAsync();

        var pessoaA = await SeedPessoaAsync(context, "Bruna");
        var pessoaB = await SeedPessoaAsync(context, "Ana");
        var usuarioA = new Usuario
        {
            PessoaId = pessoaA.Id,
            PerfilAcessoId = perfil.Id,
            EmailLogin = "bruna@app.com",
            SenhaHash = "hash",
            TipoUsuario = TipoUsuario.Admin,
            Ativo = true
        };
        var usuarioB = new Usuario
        {
            PessoaId = pessoaB.Id,
            PerfilAcessoId = perfil.Id,
            EmailLogin = "ana@app.com",
            SenhaHash = "hash",
            TipoUsuario = TipoUsuario.Portal,
            Ativo = true
        };
        context.Usuarios.AddRange(usuarioA, usuarioB);
        await context.SaveChangesAsync();

        var repository = new UsuarioRepository(context);

        var all = (await repository.GetAllAsync()).ToList();
        all.Select(x => x.Pessoa?.Nome).Should().ContainInOrder("Ana", "Bruna");
        all[0].PerfilAcesso.Should().NotBeNull();
        all[0].PerfilAcesso!.Permissoes.Should().NotBeEmpty();

        var byEmail = await repository.GetByEmailAsync("ANA@app.com");
        byEmail.Should().NotBeNull();
        byEmail!.PessoaId.Should().Be(pessoaB.Id);

        var byPessoa = await repository.GetByPessoaIdAsync(pessoaA.Id);
        byPessoa.Should().NotBeNull();
        byPessoa!.EmailLogin.Should().Be("bruna@app.com");

        (await repository.ExisteAlgumUsuarioAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistUsuario()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Carlos");
        var perfil = new PerfilAcesso { Nome = "Operador" };
        context.PerfisAcesso.Add(perfil);
        await context.SaveChangesAsync();

        var repository = new UsuarioRepository(context);

        var created = await repository.CreateAsync(new Usuario
        {
            PessoaId = pessoa.Id,
            PerfilAcessoId = perfil.Id,
            EmailLogin = "carlos@app.com",
            SenhaHash = "hash",
            TipoUsuario = TipoUsuario.Admin,
            Ativo = true
        });
        created.Id.Should().BeGreaterThan(0);

        created.Ativo = false;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Ativo.Should().BeFalse();

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
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
