using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class OperacaoControllerTests
{
    [Fact]
    public void GetSchedulers_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        var monitor = new SchedulerExecutionMonitor();
        var controller = new OperacaoController(monitor);
        SetUser(controller, (int)TipoUsuario.Portal);

        var result = controller.GetSchedulers();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public void GetSchedulers_ReturnsStatuses_WhenUserIsAdmin()
    {
        var monitor = new SchedulerExecutionMonitor();
        monitor.RecordSuccess("message_scheduler", DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow, "ok");
        var controller = new OperacaoController(monitor);
        SetUser(controller, (int)TipoUsuario.Admin);

        var result = controller.GetSchedulers();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ok.Value.Should().BeAssignableTo<IEnumerable<SchedulerExecutionStatusDto>>();
        ((IEnumerable<SchedulerExecutionStatusDto>)ok.Value!).Should().ContainSingle(x => x.SchedulerName == "message_scheduler");
    }

    [Fact]
    public void GetSchedulers_ReturnsStatuses_WhenUserIsAmbos()
    {
        var monitor = new SchedulerExecutionMonitor();
        monitor.RecordFailure("escala_scheduler", DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow, "falha");
        var controller = new OperacaoController(monitor);
        SetUser(controller, (int)TipoUsuario.Ambos);

        var result = controller.GetSchedulers();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ((IEnumerable<SchedulerExecutionStatusDto>)ok.Value!).Should().ContainSingle(x => x.SchedulerName == "escala_scheduler");
    }

    [Fact]
    public void GetSchedulers_ReturnsForbidden_WhenClaimIsMissing()
    {
        var monitor = new SchedulerExecutionMonitor();
        var controller = new OperacaoController(monitor);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([], "TestAuth"))
            }
        };

        var result = controller.GetSchedulers();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    private static void SetUser(ControllerBase controller, int tipoUsuarioId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "10"),
            new("TipoUsuarioId", tipoUsuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
