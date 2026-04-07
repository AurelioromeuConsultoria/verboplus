using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class SearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsTooShort()
    {
        await using var context = await CreateContextAsync();
        var service = new SearchService(context);

        var result = await service.SearchAsync("a", 20);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchesAcrossMultipleTypes()
    {
        await using var context = await CreateContextAsync();
        var categoria = new CategoriaNoticia { Nome = "Geral" };
        context.CategoriasNoticias.Add(categoria);
        await context.SaveChangesAsync();

        var pessoa = new Pessoa
        {
            Nome = "Marco Aurelio",
            Email = "marco@app.com",
            WhatsApp = "5511999999999",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true
        };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        context.Visitantes.Add(new Visitante
        {
            PessoaId = pessoa.Id,
            DataVisita = new DateTime(2026, 4, 1),
            DataCadastro = DateTime.UtcNow
        });
        context.Eventos.Add(new Evento
        {
            Titulo = "Conferencia Marco",
            DataInicio = new DateTime(2026, 5, 1),
            DataFim = new DateTime(2026, 5, 1),
            Tipo = TipoEvento.Evento,
            Ativo = true
        });
        context.Noticias.Add(new Noticia
        {
            Titulo = "Marco no encontro",
            Data = new DateTime(2026, 4, 2),
            CategoriaNoticiaId = categoria.Id
        });
        context.Usuarios.Add(new Usuario
        {
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            EmailLogin = "marco@app.com",
            SenhaHash = "hash",
            TipoUsuario = TipoUsuario.Admin,
            Ativo = true
        });
        await context.SaveChangesAsync();

        var service = new SearchService(context);

        var result = await service.SearchAsync("Marco", 20);

        result.Should().NotBeEmpty();
        result.Select(x => x.Type).Should().Contain(["Pessoa", "Visitante", "Evento", "Noticia", "Usuario"]);
    }

    [Fact]
    public async Task SearchAsync_RespectsLimit()
    {
        await using var context = await CreateContextAsync();
        for (var i = 0; i < 10; i++)
        {
            context.Pessoas.Add(new Pessoa
            {
                Nome = $"Maria {i}",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true
            });
        }

        await context.SaveChangesAsync();
        var service = new SearchService(context);

        var result = await service.SearchAsync("Maria", 3);

        result.Count.Should().BeLessThanOrEqualTo(3);
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
