using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ConfiguracoesMensagensControllerTests
{
    private readonly Mock<IConfiguracaoMensagemService> _serviceMock = new();
    private readonly ConfiguracoesMensagensController _controller;

    public ConfiguracoesMensagensControllerTests()
    {
        _controller = new ConfiguracoesMensagensController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ConfiguracaoMensagemDto>());
        var result = await _controller.GetAll();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAtivas_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAtivasAsync()).ReturnsAsync(new List<ConfiguracaoMensagemDto>());
        var result = await _controller.GetAtivas();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_WhenNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((ConfiguracaoMensagemDto?)null);
        var result = await _controller.GetById(1);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var dto = new CriarConfiguracaoMensagemDto { Nome = "Cfg", TextoMensagem = "Olá {Nome}", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(10,0,0), Ativo = true };
        var created = new ConfiguracaoMensagemDto { Id = 7, Nome = dto.Nome, TextoMensagem = dto.TextoMensagem, DiasAposVisita = 1, HorarioEnvio = dto.HorarioEnvio, Ativo = true, DataCriacao = DateTime.UtcNow };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);
        var result = await _controller.Create(dto);
        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_OnArgumentException()
    {
        var dto = new AtualizarConfiguracaoMensagemDto { Nome = "X", TextoMensagem = "Y", DiasAposVisita = 2, HorarioEnvio = new TimeSpan(11,0,0), Ativo = true };
        _serviceMock.Setup(s => s.UpdateAsync(99, dto)).ThrowsAsync(new ArgumentException());
        var result = await _controller.Update(99, dto);
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _serviceMock.Setup(s => s.DeleteAsync(3)).Returns(Task.CompletedTask);
        var result = await _controller.Delete(3);
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var dto = new ConfiguracaoMensagemDto { Id = 2, Nome = "Lembrete", TextoMensagem = "Olá", DiasAposVisita = 1, HorarioEnvio = new TimeSpan(9, 0, 0) };
        _serviceMock.Setup(s => s.GetByIdAsync(2)).ReturnsAsync(dto);

        var result = await _controller.GetById(2);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.CreateAsync(It.IsAny<CriarConfiguracaoMensagemDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Create(new CriarConfiguracaoMensagemDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenServiceSucceeds()
    {
        var dto = new AtualizarConfiguracaoMensagemDto { Nome = "Atualizada", TextoMensagem = "Teste", DiasAposVisita = 2, HorarioEnvio = new TimeSpan(8, 0, 0), Ativo = true };
        var updated = new ConfiguracaoMensagemDto { Id = 9, Nome = "Atualizada", TextoMensagem = "Teste", DiasAposVisita = 2, HorarioEnvio = new TimeSpan(8, 0, 0), Ativo = true };
        _serviceMock.Setup(s => s.UpdateAsync(9, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(9, dto);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenServiceThrowsGenericException()
    {
        _serviceMock.Setup(s => s.UpdateAsync(99, It.IsAny<AtualizarConfiguracaoMensagemDto>()))
            .ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Update(99, new AtualizarConfiguracaoMensagemDto());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenServiceThrows()
    {
        _serviceMock.Setup(s => s.DeleteAsync(3)).ThrowsAsync(new InvalidOperationException("erro"));

        var result = await _controller.Delete(3);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
