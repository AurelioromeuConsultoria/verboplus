using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class HubCasaRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOrderedByNome_WithUsersIncluded()
    {
        await using var context = await CreateContextAsync();
        var abertoPor = await SeedUsuarioAsync(context, "Aberto");
        var lider = await SeedUsuarioAsync(context, "Lider");
        var timoteo = await SeedUsuarioAsync(context, "Timoteo");

        context.Set<HubCasa>().AddRange(
            new HubCasa { Nome = "Z Norte", AbertoPorId = abertoPor.Id, LiderId = lider.Id, TimoteoId = timoteo.Id, EnderecoCompleto = "Rua 1", Anfitriao = "Ana" },
            new HubCasa { Nome = "A Sul", AbertoPorId = abertoPor.Id, LiderId = lider.Id, TimoteoId = timoteo.Id, EnderecoCompleto = "Rua 2", Anfitriao = "Bia" });
        await context.SaveChangesAsync();

        var repository = new HubCasaRepository(context);

        var result = (await repository.GetAllAsync()).ToList();

        result.Select(x => x.Nome).Should().ContainInOrder("A Sul", "Z Norte");
        result[0].Lider.Pessoa.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistCasa()
    {
        await using var context = await CreateContextAsync();
        var abertoPor = await SeedUsuarioAsync(context, "Aberto");
        var lider = await SeedUsuarioAsync(context, "Lider");
        var timoteo = await SeedUsuarioAsync(context, "Timoteo");
        var repository = new HubCasaRepository(context);

        var created = await repository.CreateAsync(new HubCasa
        {
            Nome = "Casa Centro",
            AbertoPorId = abertoPor.Id,
            LiderId = lider.Id,
            TimoteoId = timoteo.Id,
            EnderecoCompleto = "Rua Central, 100",
            Anfitriao = "Carlos"
        });
        created.Id.Should().BeGreaterThan(0);

        created.Anfitriao = "Carlos Atualizado";
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Anfitriao.Should().Be("Carlos Atualizado");

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
