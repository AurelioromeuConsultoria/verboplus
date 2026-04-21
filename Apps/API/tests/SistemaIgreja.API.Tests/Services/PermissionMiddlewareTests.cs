using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SistemaIgreja.API.Permissions;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class PermissionMiddlewareTests
{
    [Fact]
    public async Task Invoke_AllowsAnonymousRequestToPassThrough()
    {
        var nextCalled = false;
        var middleware = new PermissionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/usuarios";

        await middleware.Invoke(context, Mock.Of<IPermissionService>());

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_SkipsNonApiAndWhitelistedPaths()
    {
        foreach (var path in new[] { "/home", "/api/auth/login", "/api/upload/image" })
        {
            var nextCalled = false;
            var middleware = new PermissionMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            var context = BuildAuthenticatedContext("12");
            context.Request.Path = path;

            await middleware.Invoke(context, Mock.Of<IPermissionService>());

            nextCalled.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Invoke_AllowsUnknownResourceOrMethod()
    {
        var permissionService = new Mock<IPermissionService>(MockBehavior.Strict);

        var nextCalledUnknownResource = false;
        var middlewareUnknownResource = new PermissionMiddleware(_ =>
        {
            nextCalledUnknownResource = true;
            return Task.CompletedTask;
        });
        var contextUnknownResource = BuildAuthenticatedContext("12");
        contextUnknownResource.Request.Path = "/api/nao-mapeado";
        contextUnknownResource.Request.Method = "GET";

        await middlewareUnknownResource.Invoke(contextUnknownResource, permissionService.Object);
        nextCalledUnknownResource.Should().BeTrue();

        var nextCalledUnknownMethod = false;
        var middlewareUnknownMethod = new PermissionMiddleware(_ =>
        {
            nextCalledUnknownMethod = true;
            return Task.CompletedTask;
        });
        var contextUnknownMethod = BuildAuthenticatedContext("12");
        contextUnknownMethod.Request.Path = "/api/usuarios";
        contextUnknownMethod.Request.Method = "OPTIONS";

        await middlewareUnknownMethod.Invoke(contextUnknownMethod, permissionService.Object);
        nextCalledUnknownMethod.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_AllowsPlatformAdminWithoutCheckingPermission()
    {
        var permissionService = new Mock<IPermissionService>(MockBehavior.Strict);
        var nextCalled = false;
        var middleware = new PermissionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = BuildAuthenticatedContext("12", [new Claim("IsPlatformAdmin", "true")]);
        context.Request.Path = "/api/usuarios";
        context.Request.Method = "GET";

        await middleware.Invoke(context, permissionService.Object);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_ReturnsUnauthorized_WhenUserIdIsInvalid()
    {
        var middleware = new PermissionMiddleware(_ => Task.CompletedTask);
        var context = BuildAuthenticatedContext("abc");
        context.Request.Path = "/api/usuarios";
        context.Request.Method = "GET";

        await middleware.Invoke(context, Mock.Of<IPermissionService>());

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Invoke_ReturnsForbidden_WhenPermissionIsDenied()
    {
        var permissionService = new Mock<IPermissionService>();
        permissionService.Setup(s => s.HasPermissionAsync(12, "usuarios", "view"))
            .ReturnsAsync(false);

        var middleware = new PermissionMiddleware(_ => Task.CompletedTask);
        var context = BuildAuthenticatedContext("12");
        context.Request.Path = "/api/usuarios";
        context.Request.Method = "GET";

        await middleware.Invoke(context, permissionService.Object);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Invoke_CallsNext_WhenPermissionIsGranted()
    {
        var permissionService = new Mock<IPermissionService>();
        permissionService.Setup(s => s.HasPermissionAsync(12, "usuarios", "edit"))
            .ReturnsAsync(true);

        var nextCalled = false;
        var middleware = new PermissionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = BuildAuthenticatedContext("12");
        context.Request.Path = "/api/usuarios";
        context.Request.Method = "POST";

        await middleware.Invoke(context, permissionService.Object);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_DoesNotUseTenantHeaders_WhenUserIsNotPlatformAdmin()
    {
        var permissionService = new Mock<IPermissionService>();
        permissionService.Setup(s => s.HasPermissionAsync(12, "usuarios", "view"))
            .ReturnsAsync(true);

        var nextCalled = false;
        var middleware = new PermissionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = BuildAuthenticatedContext("12");
        context.Request.Path = "/api/usuarios";
        context.Request.Method = "GET";
        context.Request.Headers["X-Tenant-Id"] = "99";

        await middleware.Invoke(context, permissionService.Object);

        nextCalled.Should().BeTrue();
        permissionService.Verify(s => s.HasPermissionAsync(12, "usuarios", "view"), Times.Once);
    }

    private static DefaultHttpContext BuildAuthenticatedContext(string userId, IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (extraClaims is not null)
        {
            claims.AddRange(extraClaims);
        }

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return context;
    }
}
