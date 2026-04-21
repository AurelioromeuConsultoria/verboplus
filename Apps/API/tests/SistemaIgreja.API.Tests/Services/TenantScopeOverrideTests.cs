using FluentAssertions;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Services;

public class TenantScopeOverrideTests
{
    [Fact]
    public void SetTenant_AssignsResolvedTenant()
    {
        var sut = new TenantScopeOverride();

        sut.SetTenant(25, "campus-centro");

        sut.TenantId.Should().Be(25);
        sut.TenantSlug.Should().Be("campus-centro");
        sut.IsResolved.Should().BeTrue();
    }

    [Fact]
    public void SetTenant_WithInvalidId_ClearsTenantIdButPreservesSlug()
    {
        var sut = new TenantScopeOverride();

        sut.SetTenant(0, "slug-invalido");

        sut.TenantId.Should().BeNull();
        sut.TenantSlug.Should().Be("slug-invalido");
        sut.IsResolved.Should().BeFalse();
    }

    [Fact]
    public void Clear_RemovesTenantResolution()
    {
        var sut = new TenantScopeOverride();
        sut.SetTenant(10, "campus");

        sut.Clear();

        sut.TenantId.Should().BeNull();
        sut.TenantSlug.Should().BeNull();
        sut.IsResolved.Should().BeFalse();
    }
}
