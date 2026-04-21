using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Controllers;

public class TenantsControllerTests
{
    private readonly Mock<ITenantManagementService> _serviceMock = new();
    private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
    private readonly TenantsController _controller;

    public TenantsControllerTests()
    {
        _controller = new TenantsController(_serviceMock.Object, _auditLogServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsForbidden_WhenUserIsNotPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: 2, isPlatformAdmin: false);

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WhenUserIsPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: Tenant.InitialTenantId, isPlatformAdmin: true);
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<TenantDto>());

        var result = await _controller.GetAll();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Provision_ReturnsCreated_WhenUserIsPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: Tenant.InitialTenantId, isPlatformAdmin: true);
        _serviceMock.Setup(s => s.ProvisionAsync(It.IsAny<ProvisionTenantDto>()))
            .ReturnsAsync(new ProvisionTenantResultDto
            {
                Tenant = new TenantDto
                {
                    Id = 2,
                    Nome = "Nova Igreja",
                    Slug = "nova-igreja",
                    Ativo = true
                },
                PerfilAcessoId = 10,
                PessoaId = 20,
                UsuarioId = 30
            });

        var result = await _controller.Provision(new ProvisionTenantDto());

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Provision_ReturnsForbidden_WhenUserIsNotPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: 2, isPlatformAdmin: false);

        var result = await _controller.Provision(new ProvisionTenantDto());

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk_WhenUserIsPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: Tenant.InitialTenantId, isPlatformAdmin: true);
        _serviceMock.Setup(s => s.UpdateStatusAsync(2, It.IsAny<AtualizarTenantStatusDto>()))
            .ReturnsAsync(new TenantDto { Id = 2, Nome = "Nova Igreja", Slug = "nova-igreja", Ativo = false });

        var result = await _controller.UpdateStatus(2, new AtualizarTenantStatusDto { Ativo = false });

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenUserIsPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: Tenant.InitialTenantId, isPlatformAdmin: true);
        _serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<AtualizarTenantDto>()))
            .ReturnsAsync(new TenantDto { Id = 2, Nome = "Nova Igreja", Slug = "nova-igreja", Ativo = true, LogoUrl = "/uploads/images/logo.png" });

        var result = await _controller.Update(2, new AtualizarTenantDto());

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenUserIsPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: Tenant.InitialTenantId, isPlatformAdmin: true);

        var result = await _controller.Delete(2);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task GetAuditoriaAdministrativa_ReturnsOk_WhenUserIsPlatformAdmin()
    {
        SetUser((int)TipoUsuario.Admin, tenantId: Tenant.InitialTenantId, isPlatformAdmin: true);
        _serviceMock.Setup(s => s.GetByIdAsync(2))
            .ReturnsAsync(new TenantDto { Id = 2, Nome = "Nova Igreja", Slug = "nova-igreja", Ativo = true });
        _auditLogServiceMock.Setup(s => s.GetPagedAsync(It.IsAny<AuditLogPagedQueryDto>()))
            .ReturnsAsync(new PagedResultDto<AuditLogDto>
            {
                Items = new List<AuditLogDto>
                {
                    new() { Id = 1, EntityName = "Tenant", EntityId = "2", Action = "ProvisionarTenant" }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            });

        var result = await _controller.GetAuditoriaAdministrativa(2);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    private void SetUser(int tipoUsuarioId, int tenantId, bool isPlatformAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "10"),
            new("TipoUsuarioId", tipoUsuarioId.ToString()),
            new("TenantId", tenantId.ToString()),
            new("IsPlatformAdmin", isPlatformAdmin.ToString().ToLowerInvariant())
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
