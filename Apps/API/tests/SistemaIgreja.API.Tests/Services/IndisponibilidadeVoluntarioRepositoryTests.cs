using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class IndisponibilidadeVoluntarioRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedIndisponibilidades()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Lucas");
        var equipe = await SeedEquipeAsync(context, "Recepcao");
        var cargo = await SeedCargoAsync(context, "Apoio");
        var voluntario = new Voluntario { PessoaId = pessoa.Id, EquipeId = equipe.Id, CargoId = cargo.Id, DataCadastro = DateTime.UtcNow };
        context.Voluntarios.Add(voluntario);
        await context.SaveChangesAsync();

        context.IndisponibilidadesVoluntarios.AddRange(
            new IndisponibilidadeVoluntario { VoluntarioId = voluntario.Id, Data = new DateTime(2026, 4, 9), Motivo = "Viagem" },
            new IndisponibilidadeVoluntario { VoluntarioId = voluntario.Id, Data = new DateTime(2026, 4, 8), Motivo = "Saude" });
        await context.SaveChangesAsync();

        var repository = new IndisponibilidadeVoluntarioRepository(context);

        var byVoluntario = (await repository.GetByVoluntarioAsync(voluntario.Id, new DateTime(2026, 4, 8), new DateTime(2026, 4, 9))).ToList();
        byVoluntario.Should().HaveCount(2);
        byVoluntario[0].Motivo.Should().Be("Saude");
        byVoluntario[0].Voluntario.Pessoa.Nome.Should().Be("Lucas");

        var indisponiveis = await repository.GetVoluntarioIdsIndisponiveisNaDataAsync([voluntario.Id], new DateTime(2026, 4, 8));
        indisponiveis.Should().Contain(voluntario.Id);
    }

    [Fact]
    public async Task CreateAndDeleteAsync_PersistIndisponibilidade()
    {
        await using var context = await CreateContextAsync();
        var pessoa = await SeedPessoaAsync(context, "Laura");
        var equipe = await SeedEquipeAsync(context, "Kids");
        var cargo = await SeedCargoAsync(context, "Apoio");
        var voluntario = new Voluntario { PessoaId = pessoa.Id, EquipeId = equipe.Id, CargoId = cargo.Id, DataCadastro = DateTime.UtcNow };
        context.Voluntarios.Add(voluntario);
        await context.SaveChangesAsync();

        var repository = new IndisponibilidadeVoluntarioRepository(context);

        var created = await repository.CreateAsync(new IndisponibilidadeVoluntario
        {
            VoluntarioId = voluntario.Id,
            Data = new DateTime(2026, 4, 10),
            Motivo = "Compromisso"
        });

        (await repository.GetByIdAsync(created.Id))!.Motivo.Should().Be("Compromisso");

        await repository.DeleteAsync(created.Id);
        (await repository.GetByIdAsync(created.Id)).Should().BeNull();
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context, string nome)
    {
        var pessoa = new Pessoa { Nome = nome, TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        return pessoa;
    }

    private static async Task<Equipe> SeedEquipeAsync(SistemaIgrejaDbContext context, string nome)
    {
        var equipe = new Equipe { Nome = nome, Area = AreaEquipe.Verde };
        context.Equipes.Add(equipe);
        await context.SaveChangesAsync();
        return equipe;
    }

    private static async Task<Cargo> SeedCargoAsync(SistemaIgrejaDbContext context, string nome)
    {
        var cargo = new Cargo { Nome = nome };
        context.Cargos.Add(cargo);
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
