using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class ConfiguracaoMensagemServiceTests
{
    private readonly Mock<IConfiguracaoMensagemRepository> _repoMock = new();
    private readonly ConfiguracaoMensagemService _service;

    public ConfiguracaoMensagemServiceTests()
    {
        _service = new ConfiguracaoMensagemService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Creates_WithFields()
    {
        var dto = new CriarConfiguracaoMensagemDto { Nome = "Cfg", TextoMensagem = "Olá {Nome}", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(10,0,0), Ativo = true };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<ConfiguracaoMensagem>())).ReturnsAsync((ConfiguracaoMensagem e) => { e.Id = 3; return e; });

        var result = await _service.CreateAsync(dto);

        result.Id.Should().Be(3);
        result.Nome.Should().Be("Cfg");
        result.DiasAposVisita.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((ConfiguracaoMensagem?)null);
        var dto = new AtualizarConfiguracaoMensagemDto { Nome = "X", TextoMensagem = "Y", DiasAposVisita = 2, HorarioEnvio = new TimeSpan(11,0,0), Ativo = false };
        await _service.Invoking(s => s.UpdateAsync(9, dto)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAtivasAsync_ReturnsList()
    {
        _repoMock.Setup(r => r.GetAtivasAsync()).ReturnsAsync(new List<ConfiguracaoMensagem> { new() { Id = 1, Nome = "A", TextoMensagem = "Olá {Nome}", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(10,0,0), Ativo = true, DataCriacao = DateTime.UtcNow } });
        var list = await _service.GetAtivasAsync();
        list.Should().HaveCount(1);
    }
}
