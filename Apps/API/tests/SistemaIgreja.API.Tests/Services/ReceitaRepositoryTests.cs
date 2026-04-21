using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class ReceitaRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByDataRecebimentoDesc_WithRelations()
    {
        await using var context = await CreateContextAsync();
        var categoria = await SeedCategoriaReceitaAsync(context);
        var conta = await SeedContaAsync(context);
        var centro = await SeedCentroAsync(context);
        var projeto = await SeedProjetoAsync(context);
        var usuario = await SeedUsuarioAsync(context);

        context.Set<Receita>().AddRange(
            new Receita { Descricao = "Mais antiga", Valor = 10, DataRecebimento = new DateTime(2026, 4, 1), CategoriaReceitaId = categoria.Id, ContaBancariaId = conta.Id, CentroCustoId = centro.Id, ProjetoId = projeto.Id, UsuarioId = usuario.Id },
            new Receita { Descricao = "Mais nova", Valor = 20, DataRecebimento = new DateTime(2026, 4, 10), CategoriaReceitaId = categoria.Id, ContaBancariaId = conta.Id, CentroCustoId = centro.Id, ProjetoId = projeto.Id, UsuarioId = usuario.Id });
        await context.SaveChangesAsync();

        var repository = new ReceitaRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result[0].Descricao.Should().Be("Mais nova");
        result[0].CategoriaReceita.Should().NotBeNull();
        result[0].Usuario!.Pessoa.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistReceita()
    {
        await using var context = await CreateContextAsync();
        var repository = new ReceitaRepository(context);

        var created = await repository.CreateAsync(new Receita
        {
            Descricao = "Oferta",
            Valor = 100,
            DataRecebimento = new DateTime(2026, 4, 8)
        });
        created.Id.Should().BeGreaterThan(0);

        created.Descricao = "Oferta Atualizada";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Descricao.Should().Be("Oferta Atualizada");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    private static async Task<CategoriaReceita> SeedCategoriaReceitaAsync(SistemaIgrejaDbContext context)
    {
        var item = new CategoriaReceita { Nome = "Dízimo", Ativo = true };
        context.Set<CategoriaReceita>().Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    private static async Task<ContaBancaria> SeedContaAsync(SistemaIgrejaDbContext context)
    {
        var item = new ContaBancaria { Nome = "Conta", SaldoInicial = 0, Ativo = true };
        context.Set<ContaBancaria>().Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    private static async Task<CentroCusto> SeedCentroAsync(SistemaIgrejaDbContext context)
    {
        var item = new CentroCusto { Nome = "Centro", Ativo = true };
        context.Set<CentroCusto>().Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    private static async Task<Projeto> SeedProjetoAsync(SistemaIgrejaDbContext context)
    {
        var item = new Projeto { Nome = "Projeto", Ativo = true };
        context.Set<Projeto>().Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    private static async Task<Usuario> SeedUsuarioAsync(SistemaIgrejaDbContext context)
    {
        var pessoa = new Pessoa { Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        var usuario = new Usuario { PessoaId = pessoa.Id, EmailLogin = "marco@app.com", SenhaHash = "hash", TipoUsuario = TipoUsuario.Admin, Ativo = true };
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
