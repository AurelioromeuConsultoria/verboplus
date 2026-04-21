using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class HttpTenantContextTests
{
    [Fact]
    public void ResolvedTenant_ComesFromClaims()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim("TenantId", "8"),
                new Claim("TenantSlug", "campus-oeste")
            ]);

        var sut = new HttpTenantContext(
            new HttpContextAccessor { HttpContext = httpContext },
            new TenantScopeOverride());

        sut.TenantId.Should().Be(8);
        sut.TenantSlug.Should().Be("campus-oeste");
        sut.IsResolved.Should().BeTrue();
    }

    [Fact]
    public void PlatformAdminHeaders_OverrideClaims()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim("IsPlatformAdmin", "true"),
                new Claim("TenantId", "8"),
                new Claim("TenantSlug", "campus-oeste")
            ]);
        httpContext.Request.Headers["X-Tenant-Id"] = "21";
        httpContext.Request.Headers["X-Tenant-Slug"] = "campus-sul";

        var sut = new HttpTenantContext(
            new HttpContextAccessor { HttpContext = httpContext },
            new TenantScopeOverride());

        sut.TenantId.Should().Be(21);
        sut.TenantSlug.Should().Be("campus-sul");
    }

    [Fact]
    public void ScopeOverride_HasHighestPriority()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim("IsPlatformAdmin", "true"),
                new Claim("TenantId", "8"),
                new Claim("TenantSlug", "campus-oeste")
            ]);
        httpContext.Request.Headers["X-Tenant-Id"] = "21";
        httpContext.Request.Headers["X-Tenant-Slug"] = "campus-sul";

        var tenantOverride = new TenantScopeOverride();
        tenantOverride.SetTenant(34, "campus-leste");

        var sut = new HttpTenantContext(
            new HttpContextAccessor { HttpContext = httpContext },
            tenantOverride);

        sut.TenantId.Should().Be(34);
        sut.TenantSlug.Should().Be("campus-leste");
    }

    [Fact]
    public void InvalidTenant_DoesNotResolve()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim("TenantId", "0")
            ]);

        var sut = new HttpTenantContext(
            new HttpContextAccessor { HttpContext = httpContext },
            new TenantScopeOverride());

        sut.TenantId.Should().BeNull();
        sut.IsResolved.Should().BeFalse();
    }

    private static DefaultHttpContext BuildHttpContext(IEnumerable<Claim> claims)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return httpContext;
    }
}
