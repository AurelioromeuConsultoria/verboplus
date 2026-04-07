using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class AuditLogsControllerTests
{
    private readonly Mock<IAuditLogService> _serviceMock = new();
    private readonly AuditLogsController _controller;

    public AuditLogsControllerTests()
    {
        _controller = new AuditLogsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetPaged_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.GetPaged(new AuditLogPagedQueryDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetPaged_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<AuditLogPagedQueryDto>()))
            .ReturnsAsync(new PagedResultDto<AuditLogDto>
            {
                Items = new List<AuditLogDto>
                {
                    new() { Id = 1, EntityName = "Escala", EntityId = "10", Action = "Publicar" }
                },
                Total = 1,
                Page = 1,
                PageSize = 20
            });

        var result = await _controller.GetPaged(new AuditLogPagedQueryDto { Search = "Escala" });

        result.Result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.GetPagedAsync(It.Is<AuditLogPagedQueryDto>(q => q.Search == "Escala")), Times.Once);
    }

    [Fact]
    public async Task GetMetrics_ReturnsForbidden_WhenUserIsNotAdmin()
    {
        SetUser((int)TipoUsuario.Portal);

        var result = await _controller.GetMetrics(new AuditLogPagedQueryDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetMetrics_ReturnsOk_WhenUserIsAdmin()
    {
        SetUser((int)TipoUsuario.Admin);
        _serviceMock.Setup(s => s.GetMetricsAsync(It.IsAny<AuditLogPagedQueryDto>()))
            .ReturnsAsync(new AuditLogMetricsDto
            {
                TotalLogs = 10,
                CriticalActions = 4,
                FailureActions = 2,
                DistinctUsers = 3,
                TopUserLabel = "admin@igreja.com",
                TopUserCount = 5,
                TopEntityName = "Escala",
                TopEntityCount = 6,
                TopActionName = "Publicar",
                TopActionCount = 3
            });

        var result = await _controller.GetMetrics(new AuditLogPagedQueryDto { Search = "Escala" });

        result.Result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.GetMetricsAsync(It.Is<AuditLogPagedQueryDto>(q => q.Search == "Escala")), Times.Once);
    }

    private void SetUser(int tipoUsuarioId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "10"),
            new("TipoUsuarioId", tipoUsuarioId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
