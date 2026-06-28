using FluentAssertions;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetEstatisticas_ReturnsOk()
    {
        var serviceMock = new Mock<IDashboardService>();
        serviceMock.Setup(s => s.GetEstatisticasAsync()).ReturnsAsync(new DashboardDto());
        var controller = new DashboardController(serviceMock.Object);

        var result = await controller.GetEstatisticas();

        result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>();
    }

    [Fact]
    public async Task GetEstatisticas_ReturnsDashboardPayload_FromService()
    {
        var dto = new DashboardDto
        {
            TotalPessoas = 15,
            TotalEventos = 4
        };
        var serviceMock = new Mock<IDashboardService>();
        serviceMock.Setup(s => s.GetEstatisticasAsync()).ReturnsAsync(dto);
        var controller = new DashboardController(serviceMock.Object);

        var result = await controller.GetEstatisticas();

        var ok = result.Result.Should().BeOfType<Microsoft.AspNetCore.Mvc.OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(dto);
        serviceMock.Verify(s => s.GetEstatisticasAsync(), Times.Once);
    }
}
