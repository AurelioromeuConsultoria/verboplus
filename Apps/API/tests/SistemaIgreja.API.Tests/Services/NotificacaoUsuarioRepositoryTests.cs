using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class NotificacaoUsuarioRepositoryTests
{
    [Fact]
    public async Task GetByUsuarioAsync_FiltersUnreadAndOrdersUnreadFirstThenRecent()
    {
        await using var context = await CreateContextAsync();
        var usuario = await SeedUsuarioAsync(context);
        context.NotificacoesUsuarios.AddRange(
            new NotificacaoUsuario
            {
                UsuarioId = usuario.Id,
                Titulo = "Lida",
                Mensagem = "Mensagem",
                Tipo = TipoNotificacaoUsuario.Geral,
                DataCriacao = new DateTime(2026, 4, 1),
                DataLeitura = new DateTime(2026, 4, 2)
            },
            new NotificacaoUsuario
            {
                UsuarioId = usuario.Id,
                Titulo = "Nao lida nova",
                Mensagem = "Mensagem",
                Tipo = TipoNotificacaoUsuario.Geral,
                DataCriacao = new DateTime(2026, 4, 5)
            },
            new NotificacaoUsuario
            {
                UsuarioId = usuario.Id,
                Titulo = "Nao lida antiga",
                Mensagem = "Mensagem",
                Tipo = TipoNotificacaoUsuario.Geral,
                DataCriacao = new DateTime(2026, 4, 3)
            });
        await context.SaveChangesAsync();

        var repository = new NotificacaoUsuarioRepository(context);

        var all = (await repository.GetByUsuarioAsync(usuario.Id)).ToList();
        all[0].Titulo.Should().Be("Nao lida nova");
        all[1].Titulo.Should().Be("Nao lida antiga");
        all[2].Titulo.Should().Be("Lida");

        var unread = (await repository.GetByUsuarioAsync(usuario.Id, true, 1)).ToList();
        unread.Should().ContainSingle();
        unread[0].Titulo.Should().Be("Nao lida nova");
    }

    [Fact]
    public async Task CreateUpdateAndMarkAllAsRead_PersistNotifications()
    {
        await using var context = await CreateContextAsync();
        var usuario = await SeedUsuarioAsync(context);
        var repository = new NotificacaoUsuarioRepository(context);

        var created = await repository.CreateAsync(new NotificacaoUsuario
        {
            UsuarioId = usuario.Id,
            Titulo = "Alerta",
            Mensagem = "Teste",
            Tipo = TipoNotificacaoUsuario.Escala
        });
        created.Id.Should().BeGreaterThan(0);

        created.DataLeitura = new DateTime(2026, 4, 8);
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.DataLeitura.Should().Be(new DateTime(2026, 4, 8));

        await repository.CreateRangeAsync(
        [
            new NotificacaoUsuario { UsuarioId = usuario.Id, Titulo = "1", Mensagem = "A", Tipo = TipoNotificacaoUsuario.Geral },
            new NotificacaoUsuario { UsuarioId = usuario.Id, Titulo = "2", Mensagem = "B", Tipo = TipoNotificacaoUsuario.Geral }
        ]);

        var totalUnread = await repository.GetUnreadCountAsync(usuario.Id);
        totalUnread.Should().Be(2);

        var marked = await repository.MarcarTodasComoLidasAsync(usuario.Id, new DateTime(2026, 4, 9));
        marked.Should().Be(2);
        (await repository.GetUnreadCountAsync(usuario.Id)).Should().Be(0);
    }

    private static async Task<Usuario> SeedUsuarioAsync(SistemaIgrejaDbContext context)
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

        var usuario = new Usuario
        {
            PessoaId = pessoa.Id,
            EmailLogin = "marco@app.com",
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
