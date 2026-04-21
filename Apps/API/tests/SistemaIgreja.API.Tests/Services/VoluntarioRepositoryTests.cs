using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class VoluntarioRepositoryTests
{
    [Fact]
    public async Task GetByEquipeAndGetByPessoaId_ReturnExpectedOrdering()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Marco");
        var equipeA = await SeedEquipeAsync(context, "Audio");
        var equipeB = await SeedEquipeAsync(context, "Louvor");
        var cargoA = await SeedCargoAsync(context, "Tecnico");
        var cargoB = await SeedCargoAsync(context, "Vocal");

        context.Set<Voluntario>().AddRange(
            new Voluntario { PessoaId = pessoa.Id, EquipeId = equipeB.Id, CargoId = cargoB.Id, DataCadastro = DateTime.UtcNow },
            new Voluntario { PessoaId = pessoa.Id, EquipeId = equipeA.Id, CargoId = cargoA.Id, DataCadastro = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var repository = new VoluntarioRepository(context);

        var byEquipe = (await repository.GetByEquipeAsync(equipeA.Id)).ToList();
        byEquipe.Should().ContainSingle();
        byEquipe[0].Equipe.Nome.Should().Be("Audio");

        var byPessoa = (await repository.GetByPessoaIdAsync(pessoa.Id)).ToList();
        byPessoa.Should().HaveCount(2);
        byPessoa[0].Equipe.Nome.Should().Be("Audio");
        byPessoa[1].Equipe.Nome.Should().Be("Louvor");
    }

    [Fact]
    public async Task ExistsByPessoaEquipeCargoAsync_RespectsIgnoreId()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Aline");
        var equipe = await SeedEquipeAsync(context, "Recepcao");
        var cargo = await SeedCargoAsync(context, "Lider");
        var voluntario = new Voluntario { PessoaId = pessoa.Id, EquipeId = equipe.Id, CargoId = cargo.Id, DataCadastro = DateTime.UtcNow };
        context.Set<Voluntario>().Add(voluntario);
        await context.SaveChangesAsync();

        var repository = new VoluntarioRepository(context);

        (await repository.ExistsByPessoaEquipeCargoAsync(pessoa.Id, equipe.Id, cargo.Id)).Should().BeTrue();
        (await repository.ExistsByPessoaEquipeCargoAsync(pessoa.Id, equipe.Id, cargo.Id, voluntario.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_PersistVoluntario()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Joao");
        var equipe = await SeedEquipeAsync(context, "Kids");
        var cargo = await SeedCargoAsync(context, "Apoio");
        var repository = new VoluntarioRepository(context);

        var created = await repository.CreateAsync(new Voluntario
        {
            PessoaId = pessoa.Id,
            EquipeId = equipe.Id,
            CargoId = cargo.Id,
            DataCadastro = DateTime.UtcNow,
            MaxEscalasPorMes = 2
        });
        created.Id.Should().BeGreaterThan(0);

        created.MaxEscalasPorMes = 4;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.MaxEscalasPorMes.Should().Be(4);

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

    private static async Task<Equipe> SeedEquipeAsync(SistemaIgrejaDbContext context, string nome)
    {
        var equipe = new Equipe { Nome = nome, Area = AreaEquipe.Verde };
        context.Set<Equipe>().Add(equipe);
        await context.SaveChangesAsync();
        return equipe;
    }

    private static async Task<Cargo> SeedCargoAsync(SistemaIgrejaDbContext context, string nome)
    {
        var cargo = new Cargo { Nome = nome };
        context.Set<Cargo>().Add(cargo);
        await context.SaveChangesAsync();
        return cargo;
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
