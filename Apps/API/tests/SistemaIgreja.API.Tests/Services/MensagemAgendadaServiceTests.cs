using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class MensagemAgendadaServiceTests
{
    private readonly Mock<IMensagemAgendadaRepository> _repoMock = new();
    private readonly Mock<IVisitanteRepository> _visitRepoMock = new();
    private readonly Mock<IConfiguracaoMensagemRepository> _cfgRepoMock = new();
    private readonly MensagemAgendadaService _service;

    public MensagemAgendadaServiceTests()
    {
        _service = new MensagemAgendadaService(_repoMock.Object, _visitRepoMock.Object, _cfgRepoMock.Object);
    }

    [Fact]
    public async Task AgendarMensagensParaVisitanteAsync_CriaUmaPorConfiguracaoAtiva()
    {
        var visitante = new Visitante { Id = 10, Nome = "Maria", Telefone = "111", DataVisita = new DateTime(2025, 1, 1) };
        _visitRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(visitante);
        _cfgRepoMock.Setup(r => r.GetAtivasAsync()).ReturnsAsync(new List<ConfiguracaoMensagem>
        {
            new() { Id = 1, Nome = "Boas", TextoMensagem = "Olá {Nome}", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(10,0,0), Ativo = true, DataCriacao = DateTime.UtcNow },
            new() { Id = 2, Nome = "Volte", TextoMensagem = "Oi {Nome}", DiasAposVisita = 7, HorarioEnvio = new TimeSpan(18,0,0), Ativo = true, DataCriacao = DateTime.UtcNow },
        });
        var created = new List<MensagemAgendada>();
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<MensagemAgendada>())).ReturnsAsync((MensagemAgendada m) => { created.Add(m); return m; });

        await _service.AgendarMensagensParaVisitanteAsync(10);

        created.Should().HaveCount(2);
        created[0].TextoFinal.Should().Contain("Maria");
        created[1].TextoFinal.Should().Contain("Maria");
    }

    [Fact]
    public async Task MarcarComoProntaParaEnvio_AtualizaStatusEData()
    {
        var msg = new MensagemAgendada { Id = 5, Status = StatusMensagem.Agendada };
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(msg);
        _repoMock.Setup(r => r.UpdateAsync(msg)).ReturnsAsync(msg);

        await _service.MarcarComoProntaParaEnvioAsync(5);

        msg.Status.Should().Be(StatusMensagem.ProntaParaEnvio);
        msg.DataProcessamento.Should().NotBeNull();
    }

    [Fact]
    public async Task MarcarComoEnviada_AtualizaStatusEData()
    {
        var msg = new MensagemAgendada { Id = 6, Status = StatusMensagem.ProntaParaEnvio };
        _repoMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(msg);
        _repoMock.Setup(r => r.UpdateAsync(msg)).ReturnsAsync(msg);

        await _service.MarcarComoEnviadaAsync(6);

        msg.Status.Should().Be(StatusMensagem.Enviada);
        msg.DataProcessamento.Should().NotBeNull();
    }

    [Fact]
    public async Task MarcarComoErro_AtualizaStatusLogEData()
    {
        var msg = new MensagemAgendada { Id = 7, Status = StatusMensagem.ProntaParaEnvio };
        _repoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(msg);
        _repoMock.Setup(r => r.UpdateAsync(msg)).ReturnsAsync(msg);

        await _service.MarcarComoErroAsync(7, "erro");

        msg.Status.Should().Be(StatusMensagem.Erro);
        msg.LogErro.Should().Be("erro");
        msg.DataProcessamento.Should().NotBeNull();
    }
}
