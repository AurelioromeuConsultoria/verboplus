using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class PessoasAniversariosCampanhaControllerTests
{
    private readonly Mock<ICampanhaAniversarioService> _serviceMock = new();
    private readonly PessoasAniversariosCampanhaController _controller;

    public PessoasAniversariosCampanhaControllerTests()
    {
        _controller = new PessoasAniversariosCampanhaController(_serviceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        var filtros = new CampanhaAniversarioHistoricoFiltroDto { Busca = "mar", Limit = 10 };
        _serviceMock.Setup(x => x.GetAsync(filtros)).ReturnsAsync(new CampanhaAniversarioConfiguracaoDto
        {
            Id = 1,
            Ativo = true,
            MensagemTemplate = "Teste"
        });

        var result = await _controller.Get(filtros);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var dto = new AtualizarCampanhaAniversarioDto { Ativo = true, MensagemTemplate = "Parabéns", HorarioEnvio = "09:00" };
        _serviceMock.Setup(x => x.UpdateAsync(dto)).ReturnsAsync(new CampanhaAniversarioConfiguracaoDto
        {
            Id = 1,
            Ativo = true,
            MensagemTemplate = "Parabéns"
        });

        var result = await _controller.Update(dto);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EnviarTeste_ReturnsOk_WhenSuccessful()
    {
        var dto = new EnviarTesteCampanhaAniversarioDto { Nome = "Marco", WhatsApp = "5511999999999" };
        _serviceMock.Setup(x => x.EnviarTesteAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new CampanhaAniversarioEnvioTesteResultadoDto
        {
            Sucesso = true,
            Mensagem = "ok"
        });

        var result = await _controller.EnviarTeste(dto, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EnviarTeste_ReturnsBadRequest_WhenServiceFails()
    {
        var dto = new EnviarTesteCampanhaAniversarioDto { Nome = "Marco", WhatsApp = "5511999999999" };
        _serviceMock.Setup(x => x.EnviarTesteAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new CampanhaAniversarioEnvioTesteResultadoDto
        {
            Sucesso = false,
            Mensagem = "falha"
        });

        var result = await _controller.EnviarTeste(dto, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Reenviar_ReturnsOk_WhenSuccessful()
    {
        _serviceMock.Setup(x => x.ReenviarAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new CampanhaAniversarioReenvioResultadoDto
        {
            Sucesso = true,
            EnvioId = 10,
            Mensagem = "ok"
        });

        var result = await _controller.Reenviar(10, CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Reenviar_ReturnsBadRequest_WhenServiceFails()
    {
        _serviceMock.Setup(x => x.ReenviarAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(new CampanhaAniversarioReenvioResultadoDto
        {
            Sucesso = false,
            EnvioId = 10,
            Mensagem = "falha"
        });

        var result = await _controller.Reenviar(10, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
