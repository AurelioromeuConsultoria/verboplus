using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EnqueteRepositoryTests
{
    [Fact]
    public async Task GetAtivasAsync_RespectsDateWindow_AndIncludesOrderedOptions()
    {
        await using var context = await CreateContextAsync();
        var agora = DateTime.Now;
        var ativa = new Enquete
        {
            Titulo = "Ativa",
            DataInicio = agora.AddDays(-1),
            DataFim = agora.AddDays(1),
            Ativo = true,
            Opcoes =
            [
                new EnqueteOpcao { Texto = "Segunda", Ordem = 2 },
                new EnqueteOpcao { Texto = "Primeira", Ordem = 1 }
            ]
        };
        var encerrada = new Enquete
        {
            Titulo = "Encerrada",
            DataInicio = agora.AddDays(-5),
            DataFim = agora.AddDays(-1),
            Ativo = true
        };
        context.Set<Enquete>().AddRange(ativa, encerrada);
        await context.SaveChangesAsync();

        var repository = new EnqueteRepository(context);

        var result = (await repository.GetAtivasAsync()).ToList();

        result.Should().ContainSingle();
        result[0].Titulo.Should().Be("Ativa");

        var byId = await repository.GetByIdAsync(ativa.Id);
        byId.Should().NotBeNull();
        byId!.Opcoes.Should().HaveCount(2);
        byId.Opcoes.OrderBy(x => x.Ordem).Select(x => x.Texto).Should().ContainInOrder("Primeira", "Segunda");
    }

    [Fact]
    public async Task OpcaoAndVoteOperations_WorkAsExpected()
    {
        await using var context = await CreateContextAsync();
        var usuario = await SeedUsuarioAsync(context);
        var enquete = new Enquete
        {
            Titulo = "Escolha",
            DataInicio = DateTime.Now.AddDays(-1),
            DataFim = DateTime.Now.AddDays(1),
            Ativo = true
        };
        context.Set<Enquete>().Add(enquete);
        await context.SaveChangesAsync();

        var repository = new EnqueteRepository(context);

        var opcao = await repository.CreateOpcaoAsync(new EnqueteOpcao { EnqueteId = enquete.Id, Texto = "Opção A", Ordem = 1 });
        var voto = await repository.CreateVotoAsync(new EnqueteVoto { EnqueteId = enquete.Id, EnqueteOpcaoId = opcao.Id, UsuarioId = usuario.Id });

        opcao.Id.Should().BeGreaterThan(0);
        voto.Id.Should().BeGreaterThan(0);
        (await repository.UsuarioJaVotouAsync(enquete.Id, usuario.Id)).Should().BeTrue();
        (await repository.UsuarioJaVotouAsync(enquete.Id, null)).Should().BeFalse();
        (await repository.GetVotosPorEnqueteAsync(enquete.Id)).Should().ContainSingle();

        opcao.Texto = "Opção A Atualizada";
        await repository.UpdateOpcaoAsync(opcao);
        (await repository.GetOpcaoByIdAsync(opcao.Id))!.Texto.Should().Be("Opção A Atualizada");

        await repository.DeleteOpcaoAsync(opcao.Id);
        (await repository.GetOpcaoByIdAsync(opcao.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesEnqueteWithChildren()
    {
        await using var context = await CreateContextAsync();
        var enquete = new Enquete
        {
            Titulo = "Excluir",
            DataInicio = DateTime.Now.AddDays(-1),
            DataFim = DateTime.Now.AddDays(1),
            Ativo = true
        };
        context.Set<Enquete>().Add(enquete);
        await context.SaveChangesAsync();

        var opcao = new EnqueteOpcao
        {
            EnqueteId = enquete.Id,
            Texto = "A",
            Ordem = 1
        };
        context.Set<EnqueteOpcao>().Add(opcao);
        await context.SaveChangesAsync();

        context.Set<EnqueteVoto>().Add(new EnqueteVoto
        {
            EnqueteId = enquete.Id,
            EnqueteOpcaoId = opcao.Id,
            DataVoto = DateTime.Now
        });
        await context.SaveChangesAsync();

        var repository = new EnqueteRepository(context);
        await repository.DeleteAsync(enquete.Id);

        (await repository.GetByIdAsync(enquete.Id)).Should().BeNull();
        context.Set<EnqueteOpcao>().Should().BeEmpty();
        context.Set<EnqueteVoto>().Should().BeEmpty();
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
