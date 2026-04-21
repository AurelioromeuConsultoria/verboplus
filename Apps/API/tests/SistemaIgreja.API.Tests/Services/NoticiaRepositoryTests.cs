using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class NoticiaRepositoryTests
{
    [Fact]
    public async Task GetAllAndGetByCategoriaAsync_ReturnOrderedByDataDesc_WithCategoryIncluded()
    {
        await using var context = await CreateContextAsync();
        var categoriaA = new CategoriaNoticia { Nome = "Avisos" };
        var categoriaB = new CategoriaNoticia { Nome = "Eventos" };
        context.Set<CategoriaNoticia>().AddRange(categoriaA, categoriaB);
        await context.SaveChangesAsync();

        context.Set<Noticia>().AddRange(
            new Noticia { Titulo = "Mais antiga", CategoriaNoticiaId = categoriaA.Id, Data = new DateTime(2026, 4, 1) },
            new Noticia { Titulo = "Mais nova", CategoriaNoticiaId = categoriaA.Id, Data = new DateTime(2026, 4, 10) },
            new Noticia { Titulo = "Outra categoria", CategoriaNoticiaId = categoriaB.Id, Data = new DateTime(2026, 4, 5) });
        await context.SaveChangesAsync();

        var repository = new NoticiaRepository(context);

        var all = (await repository.GetAllAsync()).ToList();
        all[0].Titulo.Should().Be("Mais nova");
        all[0].CategoriaNoticia.Should().NotBeNull();

        var byCategoria = (await repository.GetByCategoriaAsync(categoriaA.Id)).ToList();
        byCategoria.Should().HaveCount(2);
        byCategoria[0].Titulo.Should().Be("Mais nova");
        byCategoria[1].Titulo.Should().Be("Mais antiga");
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistNoticia()
    {
        await using var context = await CreateContextAsync();
        var categoria = new CategoriaNoticia { Nome = "Comunicados" };
        context.Set<CategoriaNoticia>().Add(categoria);
        await context.SaveChangesAsync();
        var repository = new NoticiaRepository(context);

        var created = await repository.CreateAsync(new Noticia
        {
            Titulo = "Título",
            CategoriaNoticiaId = categoria.Id,
            Data = new DateTime(2026, 4, 8)
        });
        created.Id.Should().BeGreaterThan(0);

        created.Titulo = "Título atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Titulo.Should().Be("Título atualizado");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
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
