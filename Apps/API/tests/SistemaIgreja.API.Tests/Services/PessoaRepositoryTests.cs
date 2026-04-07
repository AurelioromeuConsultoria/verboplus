using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.DTOs.Pessoas;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class PessoaRepositoryTests
{
    [Fact]
    public async Task GetByWhatsAppAsync_NormalizesDigits()
    {
        await using var context = await CreateContextAsync();
        context.Pessoas.Add(new Pessoa
        {
            Nome = "Marco",
            WhatsApp = "(11) 99999-9999",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repository = new PessoaRepository(context);

        var result = await repository.GetByWhatsAppAsync("11999999999");

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Marco");
    }

    [Fact]
    public async Task GetByTelefoneAsync_ReturnsNull_WhenNormalizedPhoneIsEmpty()
    {
        await using var context = await CreateContextAsync();
        var repository = new PessoaRepository(context);

        var result = await repository.GetByTelefoneAsync("()");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByPerfilAndSortsByNome()
    {
        await using var context = await CreateContextAsync();
        var pessoa1 = new Pessoa
        {
            Nome = "Zeca",
            Email = "zeca@example.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            Perfis =
            [
                new PessoaPerfil
                {
                    Perfil = PerfilPessoa.Membro,
                    DataInicio = DateTime.UtcNow
                }
            ]
        };
        var pessoa2 = new Pessoa
        {
            Nome = "Aline",
            Email = "aline@example.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow,
            Perfis =
            [
                new PessoaPerfil
                {
                    Perfil = PerfilPessoa.Visitante,
                    DataInicio = DateTime.UtcNow
                }
            ]
        };
        context.Pessoas.AddRange(pessoa1, pessoa2);
        await context.SaveChangesAsync();

        var repository = new PessoaRepository(context);

        var result = await repository.GetPagedAsync(new PessoaPagedQuery
        {
            Perfil = PerfilPessoa.Membro,
            Sort = "nome",
            Direction = "asc"
        });

        result.Total.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items[0].Nome.Should().Be("Zeca");
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
