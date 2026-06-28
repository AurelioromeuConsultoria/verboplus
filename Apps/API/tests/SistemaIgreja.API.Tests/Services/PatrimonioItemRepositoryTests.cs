using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class PatrimonioItemRepositoryTests
{
    [Fact]
    public async Task GetAllAndGetByCodigoAsync_ReturnExpectedItemsWithIncludes()
    {
        await using var context = await CreateContextAsync();
        var categoria = await SeedCategoriaAsync(context, "Audio");
        var pessoa = await SeedPessoaAsync(context, "Marco");
        var fornecedor = await SeedFornecedorAsync(context, "Loja Som");
        var centroCusto = await SeedCentroCustoAsync(context, "Eventos");
        var projeto = await SeedProjetoAsync(context, "Campus");

        context.PatrimonioItens.AddRange(
            new PatrimonioItem
            {
                Nome = "Mesa de Som",
                Codigo = "PAT-002",
                CategoriaPatrimonioId = categoria.Id
            },
            new PatrimonioItem
            {
                Nome = "Caixa Line",
                Codigo = "PAT-001",
                CategoriaPatrimonioId = categoria.Id,
                ResponsavelPessoaId = pessoa.Id,
                FornecedorId = fornecedor.Id,
                CentroCustoId = centroCusto.Id,
                ProjetoId = projeto.Id
            });
        await context.SaveChangesAsync();

        var repository = new PatrimonioItemRepository(context);

        var all = (await repository.GetAllAsync()).ToList();
        all.Select(x => x.Nome).Should().ContainInOrder("Caixa Line", "Mesa de Som");
        all[0].CategoriaPatrimonio.Should().NotBeNull();

        var byCodigo = await repository.GetByCodigoAsync("pat-001");
        byCodigo.Should().NotBeNull();
        byCodigo!.FornecedorId.Should().Be(fornecedor.Id);
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistItem()
    {
        await using var context = await CreateContextAsync();
        var categoria = await SeedCategoriaAsync(context, "Instrumentos");
        var repository = new PatrimonioItemRepository(context);

        var created = await repository.CreateAsync(new PatrimonioItem
        {
            Nome = "Violao",
            Codigo = "PAT-010",
            CategoriaPatrimonioId = categoria.Id
        });
        created.Id.Should().BeGreaterThan(0);

        created.Nome = "Violao Atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Nome.Should().Be("Violao Atualizado");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    private static async Task<CategoriaPatrimonio> SeedCategoriaAsync(SistemaIgrejaDbContext context, string nome)
    {
        var categoria = new CategoriaPatrimonio { Nome = nome };
        context.CategoriasPatrimonio.Add(categoria);
        await context.SaveChangesAsync();
        return categoria;
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context, string nome)
    {
        var pessoa = new Pessoa { Nome = nome, TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        return pessoa;
    }

    private static async Task<Fornecedor> SeedFornecedorAsync(SistemaIgrejaDbContext context, string nome)
    {
        var fornecedor = new Fornecedor { Nome = nome };
        context.Fornecedores.Add(fornecedor);
        await context.SaveChangesAsync();
        return fornecedor;
    }

    private static async Task<CentroCusto> SeedCentroCustoAsync(SistemaIgrejaDbContext context, string nome)
    {
        var centro = new CentroCusto { Nome = nome };
        context.CentrosCustos.Add(centro);
        await context.SaveChangesAsync();
        return centro;
    }

    private static async Task<Projeto> SeedProjetoAsync(SistemaIgrejaDbContext context, string nome)
    {
        var projeto = new Projeto { Nome = nome };
        context.Projetos.Add(projeto);
        await context.SaveChangesAsync();
        return projeto;
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
