using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class SolicitacaoTrocaEscalaRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedScopedResults()
    {
        await using var context = await CreateContextAsync();
        var leader = await SeedUsuarioAsync(context, "leader@app.com");
        var responder = await SeedUsuarioAsync(context, "responder@app.com");
        var equipe = new Equipe { Nome = "Audio", Area = AreaEquipe.Verde, LiderUsuarioId = leader.Id };
        context.Set<Equipe>().Add(equipe);
        await context.SaveChangesAsync();
        var cargo = new Cargo { Nome = "Tecnico" };
        context.Set<Cargo>().Add(cargo);
        await context.SaveChangesAsync();
        var solicitante = await SeedVoluntarioAsync(context, "Marco", equipe.Id, cargo.Id);
        var substituto = await SeedVoluntarioAsync(context, "Aline", equipe.Id, cargo.Id);
        var evento = new Evento { Titulo = "Culto", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Culto };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();
        var ocorrencia = new EventoOcorrencia { EventoId = evento.Id, DataHoraInicio = new DateTime(2026, 4, 8, 19, 0, 0), Status = StatusEventoOcorrencia.Confirmado };
        context.EventosOcorrencias.Add(ocorrencia);
        await context.SaveChangesAsync();
        var escala = new Escala { EventoOcorrenciaId = ocorrencia.Id, EquipeId = equipe.Id, Status = StatusEscala.Publicada };
        context.Set<Escala>().Add(escala);
        await context.SaveChangesAsync();
        var item = new EscalaItem { EscalaId = escala.Id, EquipeId = equipe.Id, CargoId = cargo.Id, VoluntarioId = solicitante.Id, Ordem = 1, Status = StatusEscalaItem.Pendente };
        context.Set<EscalaItem>().Add(item);
        await context.SaveChangesAsync();

        var solicitacao = new SolicitacaoTrocaEscala
        {
            EscalaItemId = item.Id,
            VoluntarioSolicitanteId = solicitante.Id,
            VoluntarioSubstitutoId = substituto.Id,
            RespondidoPorUsuarioId = responder.Id,
            Status = StatusSolicitacaoTrocaEscala.Pendente,
            Motivo = "Viagem"
        };
        context.Set<SolicitacaoTrocaEscala>().Add(solicitacao);
        await context.SaveChangesAsync();

        var repository = new SolicitacaoTrocaEscalaRepository(context);

        (await repository.GetByIdAsync(solicitacao.Id)).Should().NotBeNull();
        (await repository.GetPendenteByEscalaItemAsync(item.Id)).Should().NotBeNull();
        (await repository.GetGerenciaveisAsync(leader.Id, false, equipe.Id, StatusSolicitacaoTrocaEscala.Pendente)).Should().ContainSingle();
        (await repository.GetByEscalaAsync(escala.Id)).Should().ContainSingle();
        (await repository.GetByPessoaAsync(solicitante.PessoaId)).Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAndUpdateAsync_PersistSolicitacao()
    {
        await using var context = await CreateContextAsync();
        var usuario = await SeedUsuarioAsync(context, "leader@app.com");
        var equipe = new Equipe { Nome = "Recepcao", Area = AreaEquipe.Laranja, LiderUsuarioId = usuario.Id };
        context.Set<Equipe>().Add(equipe);
        await context.SaveChangesAsync();
        var cargo = new Cargo { Nome = "Lider" };
        context.Set<Cargo>().Add(cargo);
        await context.SaveChangesAsync();
        var solicitante = await SeedVoluntarioAsync(context, "Joao", equipe.Id, cargo.Id);
        var evento = new Evento { Titulo = "Evento", DataInicio = new DateTime(2026, 4, 1), DataFim = new DateTime(2026, 4, 1), Tipo = TipoEvento.Evento };
        context.Set<Evento>().Add(evento);
        await context.SaveChangesAsync();
        var ocorrencia = new EventoOcorrencia { EventoId = evento.Id, DataHoraInicio = new DateTime(2026, 4, 9, 19, 0, 0), Status = StatusEventoOcorrencia.Confirmado };
        context.EventosOcorrencias.Add(ocorrencia);
        await context.SaveChangesAsync();
        var escala = new Escala { EventoOcorrenciaId = ocorrencia.Id, EquipeId = equipe.Id, Status = StatusEscala.Publicada };
        context.Set<Escala>().Add(escala);
        await context.SaveChangesAsync();
        var item = new EscalaItem { EscalaId = escala.Id, EquipeId = equipe.Id, CargoId = cargo.Id, VoluntarioId = solicitante.Id, Ordem = 1, Status = StatusEscalaItem.Pendente };
        context.Set<EscalaItem>().Add(item);
        await context.SaveChangesAsync();
        var repository = new SolicitacaoTrocaEscalaRepository(context);

        var created = await repository.CreateAsync(new SolicitacaoTrocaEscala
        {
            EscalaItemId = item.Id,
            VoluntarioSolicitanteId = solicitante.Id,
            Status = StatusSolicitacaoTrocaEscala.Pendente,
            Motivo = "Teste"
        });
        created.Id.Should().BeGreaterThan(0);

        created.Status = StatusSolicitacaoTrocaEscala.Aprovada;
        await repository.UpdateAsync(created);

        var loaded = await repository.GetByIdAsync(created.Id);
        loaded.Should().NotBeNull();
        loaded!.Status.Should().Be(StatusSolicitacaoTrocaEscala.Aprovada);
    }

    private static async Task<Usuario> SeedUsuarioAsync(SistemaIgrejaDbContext context, string email)
    {
        var pessoa = new Pessoa { Nome = email, TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        var usuario = new Usuario { PessoaId = pessoa.Id, EmailLogin = email, SenhaHash = "hash", TipoUsuario = TipoUsuario.Admin, Ativo = true };
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private static async Task<Voluntario> SeedVoluntarioAsync(SistemaIgrejaDbContext context, string nome, int equipeId, int cargoId)
    {
        var pessoa = new Pessoa { Nome = nome, TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        var voluntario = new Voluntario { PessoaId = pessoa.Id, EquipeId = equipeId, CargoId = cargoId, DataCadastro = DateTime.UtcNow };
        context.Set<Voluntario>().Add(voluntario);
        await context.SaveChangesAsync();
        return voluntario;
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
