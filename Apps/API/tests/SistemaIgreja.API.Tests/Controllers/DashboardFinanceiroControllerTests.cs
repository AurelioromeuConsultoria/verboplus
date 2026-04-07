using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class DashboardFinanceiroControllerTests
{
    private readonly Mock<IDashboardFinanceiroService> _serviceMock = new();
    private readonly DashboardFinanceiroController _controller;

    public DashboardFinanceiroControllerTests()
    {
        _controller = new DashboardFinanceiroController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetDashboardAsync()).ReturnsAsync(new DashboardFinanceiroDto());

        var result = await _controller.GetDashboard();

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
