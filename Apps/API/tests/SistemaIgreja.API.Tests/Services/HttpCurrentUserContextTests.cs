using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SistemaIgreja.API.Services;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class HttpCurrentUserContextTests
{
    [Fact]
    public void Properties_ReadUserClaimsAndIp()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim(ClaimTypes.NameIdentifier, "12"),
                new Claim(ClaimTypes.Name, "Marco"),
                new Claim(ClaimTypes.Email, "marco@example.com"),
                new Claim("TenantId", "5"),
                new Claim("TenantSlug", "matriz")
            ]);
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        var sut = new HttpCurrentUserContext(
            new HttpContextAccessor { HttpContext = httpContext },
            new TenantScopeOverride());

        sut.UserId.Should().Be(12);
        sut.UserName.Should().Be("Marco");
        sut.UserEmail.Should().Be("marco@example.com");
        sut.TenantId.Should().Be(5);
        sut.TenantSlug.Should().Be("matriz");
        sut.IpAddress.Should().Be("127.0.0.1");
    }

    [Fact]
    public void TenantProperties_PreferScopeOverrideOverClaims()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim("TenantId", "5"),
                new Claim("TenantSlug", "matriz")
            ]);
        var tenantOverride = new TenantScopeOverride();
        tenantOverride.SetTenant(9, "sede");

        var sut = new HttpCurrentUserContext(
            new HttpContextAccessor { HttpContext = httpContext },
            tenantOverride);

        sut.TenantId.Should().Be(9);
        sut.TenantSlug.Should().Be("sede");
    }

    [Fact]
    public void PlatformAdminHeaders_OverrideTenantClaims()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim("IsPlatformAdmin", "true"),
                new Claim("TenantId", "5"),
                new Claim("TenantSlug", "matriz")
            ]);
        httpContext.Request.Headers["X-Tenant-Id"] = "17";
        httpContext.Request.Headers["X-Tenant-Slug"] = "campus-norte";

        var sut = new HttpCurrentUserContext(
            new HttpContextAccessor { HttpContext = httpContext },
            new TenantScopeOverride());

        sut.TenantId.Should().Be(17);
        sut.TenantSlug.Should().Be("campus-norte");
    }

    [Fact]
    public void InvalidClaims_ReturnNull()
    {
        var httpContext = BuildHttpContext(
            [
                new Claim(ClaimTypes.NameIdentifier, "abc"),
                new Claim("TenantId", "-1")
            ]);

        var sut = new HttpCurrentUserContext(
            new HttpContextAccessor { HttpContext = httpContext },
            new TenantScopeOverride());

        sut.UserId.Should().BeNull();
        sut.TenantId.Should().BeNull();
    }

    private static DefaultHttpContext BuildHttpContext(IEnumerable<Claim> claims)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return httpContext;
    }
}
