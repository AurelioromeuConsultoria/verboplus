using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class EscalaRepositoryTests
{
    [Fact]
    public async Task QueryMethods_ReturnExpectedScaleShapes()
    {
        await using var context = await CreateContextAsync();
        var setup = await SeedEscalaSetupAsync(context);
        var repository = new EscalaRepository(context);

        var byId = await repository.GetByIdAsync(setup.EscalaLouvor.Id);
        byId.Should().NotBeNull();
        byId!.Equipe.Nome.Should().Be("Louvor");
        byId.Itens.Should().ContainSingle();

        var byOcorrencia = await repository.GetByEventoOcorrenciaIdAsync(setup.Ocorrencia.Id);
        byOcorrencia.Should().NotBeNull();

        var byOcorrenciaEquipe = await repository.GetByEventoOcorrenciaAndEquipeAsync(setup.Ocorrencia.Id, setup.EquipeLouvor.Id);
        byOcorrenciaEquipe.Should().NotBeNull();
        byOcorrenciaEquipe!.EquipeId.Should().Be(setup.EquipeLouvor.Id);

        var allByOcorrencia = (await repository.GetAllByEventoOcorrenciaAsync(setup.Ocorrencia.Id)).ToList();
        allByOcorrencia.Should().HaveCount(2);
        allByOcorrencia.Select(x => x.EquipeId).Should().BeInAscendingOrder();
        allByOcorrencia.Select(x => x.EquipeId).Should().BeEquivalentTo([setup.EquipeLouvor.Id, setup.EquipeAudio.Id]);
    }

    [Fact]
    public async Task PessoaAndCargaQueries_ReturnExpectedResults()
    {
        await using var context = await CreateContextAsync();
        var setup = await SeedEscalaSetupAsync(context);
        var repository = new EscalaRepository(context);

        var byPessoa = (await repository.GetByPessoaIdAsync(setup.PessoaVoluntario.Id, false)).ToList();
        byPessoa.Should().HaveCount(2);

        var futuras = (await repository.GetByPessoaIdAsync(setup.PessoaVoluntario.Id, true)).ToList();
        futuras.Should().HaveCount(1);
        futuras[0].EventoOcorrenciaId.Should().Be(setup.Ocorrencia.Id);

        var conflito = await repository.GetConflitoPessoaNaEscalaAsync(setup.EscalaAudio.Id, setup.VoluntarioLouvor.Id);
        conflito.Should().NotBeNull();
        conflito!.PessoaId.Should().Be(setup.PessoaVoluntario.Id);

        var pessoaIds = await repository.GetPessoaIdsJaEscaladasAsync(setup.EscalaAudio.Id);
        pessoaIds.Should().Contain(setup.PessoaVoluntario.Id);

        var primeiroDiaMes = new DateTime(setup.Ocorrencia.DataHoraInicio.Year, setup.Ocorrencia.DataHoraInicio.Month, 1);
        var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

        var cargaRecente = await repository.GetCargaRecentePorVoluntarioAsync(setup.EquipeLouvor.Id, primeiroDiaMes);
        cargaRecente.Should().ContainKey(setup.VoluntarioLouvor.Id);
        cargaRecente[setup.VoluntarioLouvor.Id].Should().Be(1);

        var noMes = await repository.GetQuantidadeEscalasNoMesPorVoluntarioAsync(setup.EquipeLouvor.Id, primeiroDiaMes.Year, primeiroDiaMes.Month);
        noMes[setup.VoluntarioLouvor.Id].Should().Be(1);

        var noPeriodo = await repository.GetQuantidadeEscalasEmPeriodoPorVoluntarioAsync(setup.EquipeLouvor.Id, primeiroDiaMes, ultimoDiaMes);
        noPeriodo[setup.VoluntarioLouvor.Id].Should().Be(1);
    }

    [Fact]
    public async Task ItemAndPeriodoOperations_PersistAndFilterCorrectly()
    {
        await using var context = await CreateContextAsync();
        var setup = await SeedEscalaSetupAsync(context);
        var repository = new EscalaRepository(context);

        var item = await repository.AddItemAsync(new EscalaItem
        {
            EscalaId = setup.EscalaAudio.Id,
            EquipeId = setup.EquipeAudio.Id,
            CargoId = setup.CargoAudio.Id,
            PessoaId = setup.PessoaAudio.Id,
            VoluntarioId = setup.VoluntarioAudio.Id,
            Ordem = 2
        });
        item.Id.Should().BeGreaterThan(0);

        item.Status = StatusEscalaItem.Confirmado;
        await repository.UpdateItemAsync(item);

        var loadedItem = await repository.GetItemByIdAsync(item.Id);
        loadedItem.Should().NotBeNull();
        loadedItem!.Status.Should().Be(StatusEscalaItem.Confirmado);

        var primeiroDiaMes = new DateTime(setup.Ocorrencia.DataHoraInicio.Year, setup.Ocorrencia.DataHoraInicio.Month, 1);
        var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);
        var itensPeriodo = (await repository.GetItensComOcorrenciaNoPeriodoAsync(primeiroDiaMes, ultimoDiaMes, equipeId: setup.EquipeAudio.Id)).ToList();
        itensPeriodo.Should().Contain(x => x.Id == item.Id);

        await repository.DeleteItemAsync(item.Id);
        (await repository.GetItemByIdAsync(item.Id)).Should().BeNull();
    }

    private static async Task<EscalaSetup> SeedEscalaSetupAsync(SistemaIgrejaDbContext context)
    {
        var dataFutura = new DateTime(2030, 7, 25);
        var dataPassada = new DateTime(2020, 7, 10);

        var pessoaVoluntario = await SeedPessoaAsync(context, "Mateus", TipoPessoa.Adulto);
        var pessoaAudio = await SeedPessoaAsync(context, "Joao", TipoPessoa.Adulto);
        var pessoaCriador = await SeedPessoaAsync(context, "Lider", TipoPessoa.Adulto);

        var usuarioCriador = new Usuario
        {
            PessoaId = pessoaCriador.Id,
            EmailLogin = "lider@app.com",
            SenhaHash = "hash",
            TipoUsuario = TipoUsuario.Admin,
            Ativo = true
        };
        context.Usuarios.Add(usuarioCriador);
        await context.SaveChangesAsync();

        var equipeLouvor = new Equipe { Nome = "Louvor", Area = AreaEquipe.Verde };
        var equipeAudio = new Equipe { Nome = "Audio", Area = AreaEquipe.Laranja };
        context.Equipes.AddRange(equipeLouvor, equipeAudio);
        await context.SaveChangesAsync();

        var cargoLouvor = new Cargo { Nome = "Vocal" };
        var cargoAudio = new Cargo { Nome = "Tecnico" };
        context.Cargos.AddRange(cargoLouvor, cargoAudio);
        await context.SaveChangesAsync();

        var voluntarioLouvor = new Voluntario { PessoaId = pessoaVoluntario.Id, EquipeId = equipeLouvor.Id, CargoId = cargoLouvor.Id, DataCadastro = DateTime.UtcNow };
        var voluntarioAudio = new Voluntario { PessoaId = pessoaAudio.Id, EquipeId = equipeAudio.Id, CargoId = cargoAudio.Id, DataCadastro = DateTime.UtcNow };
        context.Voluntarios.AddRange(voluntarioLouvor, voluntarioAudio);
        await context.SaveChangesAsync();

        var evento = new Evento
        {
            Titulo = "Culto",
            DataInicio = dataFutura,
            DataFim = dataFutura,
            Tipo = TipoEvento.Culto
        };
        context.Eventos.Add(evento);
        await context.SaveChangesAsync();

        var ocorrencia = new EventoOcorrencia
        {
            EventoId = evento.Id,
            DataHoraInicio = new DateTime(dataFutura.Year, dataFutura.Month, dataFutura.Day, 19, 0, 0),
            Status = StatusEventoOcorrencia.Confirmado
        };
        var ocorrenciaPassada = new EventoOcorrencia
        {
            EventoId = evento.Id,
            DataHoraInicio = new DateTime(dataPassada.Year, dataPassada.Month, dataPassada.Day, 19, 0, 0),
            Status = StatusEventoOcorrencia.Confirmado
        };
        context.EventosOcorrencias.AddRange(ocorrencia, ocorrenciaPassada);
        await context.SaveChangesAsync();

        var escalaLouvor = new Escala
        {
            EventoOcorrenciaId = ocorrencia.Id,
            EquipeId = equipeLouvor.Id,
            CriadoPorUsuarioId = usuarioCriador.Id,
            Status = StatusEscala.Publicada
        };
        var escalaAudio = new Escala
        {
            EventoOcorrenciaId = ocorrencia.Id,
            EquipeId = equipeAudio.Id,
            CriadoPorUsuarioId = usuarioCriador.Id,
            Status = StatusEscala.Publicada
        };
        var escalaPassada = new Escala
        {
            EventoOcorrenciaId = ocorrenciaPassada.Id,
            EquipeId = equipeLouvor.Id,
            CriadoPorUsuarioId = usuarioCriador.Id,
            Status = StatusEscala.Publicada
        };
        context.Escalas.AddRange(escalaLouvor, escalaAudio, escalaPassada);
        await context.SaveChangesAsync();

        context.EscalasItens.AddRange(
            new EscalaItem
            {
                EscalaId = escalaLouvor.Id,
                EquipeId = equipeLouvor.Id,
                CargoId = cargoLouvor.Id,
                PessoaId = pessoaVoluntario.Id,
                VoluntarioId = voluntarioLouvor.Id,
                Ordem = 1
            },
            new EscalaItem
            {
                EscalaId = escalaPassada.Id,
                EquipeId = equipeLouvor.Id,
                CargoId = cargoLouvor.Id,
                PessoaId = pessoaVoluntario.Id,
                VoluntarioId = voluntarioLouvor.Id,
                Ordem = 1
            },
            new EscalaItem
            {
                EscalaId = escalaAudio.Id,
                EquipeId = equipeAudio.Id,
                CargoId = cargoAudio.Id,
                PessoaId = pessoaAudio.Id,
                VoluntarioId = voluntarioAudio.Id,
                Ordem = 1
            });
        await context.SaveChangesAsync();

        return new EscalaSetup(
            pessoaVoluntario,
            pessoaAudio,
            equipeLouvor,
            equipeAudio,
            cargoAudio,
            voluntarioLouvor,
            voluntarioAudio,
            ocorrencia,
            escalaLouvor,
            escalaAudio);
    }

    private static async Task<Pessoa> SeedPessoaAsync(SistemaIgrejaDbContext context, string nome, TipoPessoa tipoPessoa)
    {
        var pessoa = new Pessoa { Nome = nome, TipoPessoa = tipoPessoa, Ativo = true, DataCriacao = DateTime.UtcNow };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        return pessoa;
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

    private sealed record EscalaSetup(
        Pessoa PessoaVoluntario,
        Pessoa PessoaAudio,
        Equipe EquipeLouvor,
        Equipe EquipeAudio,
        Cargo CargoAudio,
        Voluntario VoluntarioLouvor,
        Voluntario VoluntarioAudio,
        EventoOcorrencia Ocorrencia,
        Escala EscalaLouvor,
        Escala EscalaAudio);
}
