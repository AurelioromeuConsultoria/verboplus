using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class ConfiguracaoPortalControllerTests
{
    private readonly Mock<IConfiguracaoPortalService> _serviceMock = new();
    private readonly ConfiguracaoPortalController _controller;

    public ConfiguracaoPortalControllerTests()
    {
        _controller = new ConfiguracaoPortalController(_serviceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAsync())
            .ReturnsAsync(new ConfiguracaoPortalDto { TempoTransicaoCarrossel = 5 });

        var result = await _controller.Get();

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenTempoIsOutsideRange()
    {
        var result = await _controller.Update(new AtualizarConfiguracaoPortalDto
        {
            TempoTransicaoCarrossel = 61
        });

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>();
    }

    [Fact]
    public async Task Get_ReturnsPayloadFromService()
    {
        var dto = new ConfiguracaoPortalDto { TempoTransicaoCarrossel = 7 };
        _serviceMock.Setup(s => s.GetAsync()).ReturnsAsync(dto);

        var result = await _controller.Get();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenTempoIsValid()
    {
        var dto = new AtualizarConfiguracaoPortalDto { TempoTransicaoCarrossel = 8 };
        var updated = new ConfiguracaoPortalDto { TempoTransicaoCarrossel = 8 };
        _serviceMock.Setup(s => s.UpdateAsync(dto)).ReturnsAsync(updated);

        var result = await _controller.Update(dto);

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }
}
