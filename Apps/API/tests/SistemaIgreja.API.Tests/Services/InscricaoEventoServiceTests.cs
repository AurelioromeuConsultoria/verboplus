using System.Text.Json;
using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class InscricaoEventoServiceTests
{
    private readonly Mock<IInscricaoEventoRepository> _repositoryMock = new();
    private readonly Mock<IEventoRepository> _eventoRepositoryMock = new();
    private readonly InscricaoEventoService _service;

    public InscricaoEventoServiceTests()
    {
        _service = new InscricaoEventoService(_repositoryMock.Object, _eventoRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEventoDoesNotExist()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Evento?)null);

        var act = () => _service.CreateAsync(new CriarInscricaoEventoDto
        {
            EventoId = 1,
            Nome = "Marco",
            WhatsApp = "5511999999999"
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Evento não encontrado");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEventoNaoAceitaInscricoes()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Evento
        {
            Id = 1,
            Titulo = "Evento",
            AceitaInscricoes = false,
            DataInicio = DateTime.Now.AddDays(5),
            DataFim = DateTime.Now.AddDays(5)
        });

        var act = () => _service.CreateAsync(new CriarInscricaoEventoDto
        {
            EventoId = 1,
            Nome = "Marco",
            WhatsApp = "5511999999999"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Este evento não aceita inscrições");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenJaExisteInscricaoNoEvento()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CriarEventoComInscricao());
        _repositoryMock.Setup(r => r.ExisteInscricaoAsync(1, "5511999999999")).ReturnsAsync(true);

        var act = () => _service.CreateAsync(new CriarInscricaoEventoDto
        {
            EventoId = 1,
            Nome = "Marco",
            WhatsApp = "5511999999999"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Já existe uma inscrição para este WhatsApp neste evento");
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenCampoObrigatorioDinamicoIsMissing()
    {
        var configuracao = JsonSerializer.Serialize(new List<EventoCampoFormularioDto>
        {
            new() { Slug = "cpf", Label = "CPF", Obrigatorio = true }
        });

        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Evento
        {
            Id = 1,
            Titulo = "Evento",
            AceitaInscricoes = true,
            DataInicio = DateTime.Now.AddDays(5),
            DataFim = DateTime.Now.AddDays(5),
            ConfiguracaoFormularioInscricao = configuracao
        });
        _repositoryMock.Setup(r => r.ExisteInscricaoAsync(1, "5511999999999")).ReturnsAsync(false);

        var act = () => _service.CreateAsync(new CriarInscricaoEventoDto
        {
            EventoId = 1,
            Nome = "Marco",
            WhatsApp = "5511999999999",
            Campos = new Dictionary<string, object?>()
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O campo \"CPF\" é obrigatório.");
    }

    [Fact]
    public async Task CreateAsync_CreatesInscricao_WhenDataIsValid()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CriarEventoComInscricao());
        _repositoryMock.Setup(r => r.ExisteInscricaoAsync(1, "5511999999999")).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<InscricaoEvento>()))
            .ReturnsAsync((InscricaoEvento inscricao) =>
            {
                inscricao.Id = 10;
                inscricao.Evento = CriarEventoComInscricao();
                return inscricao;
            });

        var result = await _service.CreateAsync(new CriarInscricaoEventoDto
        {
            EventoId = 1,
            Nome = "Marco",
            WhatsApp = "5511999999999",
            Email = "marco@app.com",
            Campos = new Dictionary<string, object?> { ["cpf"] = "123" }
        });

        result.Id.Should().Be(10);
        result.Status.Should().Be(StatusInscricao.Pendente);
        result.Nome.Should().Be("Marco");
    }

    [Fact]
    public async Task ConfirmarInscricaoAsync_UpdatesStatusAndDate()
    {
        var entity = new InscricaoEvento
        {
            Id = 3,
            EventoId = 1,
            Evento = CriarEventoComInscricao(),
            Nome = "Marco",
            WhatsApp = "5511999999999",
            Status = StatusInscricao.Pendente,
            DataInscricao = DateTime.Now
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(entity);

        var result = await _service.ConfirmarInscricaoAsync(3);

        result.Status.Should().Be(StatusInscricao.Confirmada);
        entity.DataConfirmacao.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterEstatisticasAsync_ReturnsAggregatedCounts()
    {
        _eventoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CriarEventoComInscricao());
        _repositoryMock.Setup(r => r.GetByEventoAsync(1)).ReturnsAsync(new List<InscricaoEvento>
        {
            new() { Status = StatusInscricao.Pendente, QuantidadeAcompanhantes = 0 },
            new() { Status = StatusInscricao.Confirmada, QuantidadeAcompanhantes = 2 },
            new() { Status = StatusInscricao.Cancelada, QuantidadeAcompanhantes = 1 }
        });

        var result = await _service.ObterEstatisticasAsync(1);

        result.TotalInscricoes.Should().Be(3);
        result.InscricoesConfirmadas.Should().Be(1);
        result.InscricoesPendentes.Should().Be(1);
        result.InscricoesCanceladas.Should().Be(1);
        result.TotalParticipantes.Should().Be(6);
    }

    private static Evento CriarEventoComInscricao()
    {
        return new Evento
        {
            Id = 1,
            Titulo = "Evento",
            AceitaInscricoes = true,
            DataInicio = DateTime.Now.AddDays(5),
            DataFim = DateTime.Now.AddDays(5),
            ConfiguracaoFormularioInscricao = JsonSerializer.Serialize(new List<EventoCampoFormularioDto>
            {
                new() { Slug = "cpf", Label = "CPF", Obrigatorio = false }
            })
        };
    }
}
