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
}
