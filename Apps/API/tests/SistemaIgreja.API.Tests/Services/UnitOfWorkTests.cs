using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.API.Tests.Services;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_PersistsTrackedChanges()
    {
        await using var context = await CreateContextAsync();
        var unitOfWork = new UnitOfWork(context);

        context.Pessoas.Add(new Pessoa
        {
            Nome = "Marco",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });

        var affected = await unitOfWork.SaveChangesAsync();

        affected.Should().BeGreaterThan(0);
        context.Pessoas.Should().ContainSingle();
    }

    [Fact]
    public async Task BeginCommitAndRollbackTransactionAsync_WorkAsExpected()
    {
        await using var context = await CreateContextAsync();
        var unitOfWork = new UnitOfWork(context);

        await unitOfWork.BeginTransactionAsync();
        context.Pessoas.Add(new Pessoa
        {
            Nome = "Commitado",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.CommitTransactionAsync();

        context.Pessoas.Should().ContainSingle(p => p.Nome == "Commitado");

        await unitOfWork.BeginTransactionAsync();
        context.Pessoas.Add(new Pessoa
        {
            Nome = "Descartado",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.RollbackTransactionAsync();

        context.ChangeTracker.Clear();
        context.Pessoas.Should().ContainSingle(p => p.Nome == "Commitado");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_CommitsOnSuccessAndRollsBackOnFailure()
    {
        await using var context = await CreateContextAsync();
        var unitOfWork = new UnitOfWork(context);

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            context.Pessoas.Add(new Pessoa
            {
                Nome = "Dentro da transacao",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        });

        context.ChangeTracker.Clear();
        context.Pessoas.Should().ContainSingle(p => p.Nome == "Dentro da transacao");

        Func<Task> action = async () =>
        {
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                context.Pessoas.Add(new Pessoa
                {
                    Nome = "Vai falhar",
                    TipoPessoa = TipoPessoa.Adulto,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                throw new InvalidOperationException("falhou");
            });
        };

        await action.Should().ThrowAsync<InvalidOperationException>();

        context.ChangeTracker.Clear();
        context.Pessoas.Should().ContainSingle(p => p.Nome == "Dentro da transacao");
    }

    [Fact]
    public async Task ExecuteInTransactionAsyncOfT_ReturnsResult()
    {
        await using var context = await CreateContextAsync();
        var unitOfWork = new UnitOfWork(context);

        var result = await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var pessoa = new Pessoa
            {
                Nome = "Com retorno",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();
            return pessoa.Nome;
        });

        result.Should().Be("Com retorno");
        context.ChangeTracker.Clear();
        context.Pessoas.Should().ContainSingle(p => p.Nome == "Com retorno");
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
