using FluentAssertions;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class DefaultTenantContextTests
{
    [Fact]
    public void DefaultTenantContext_AlwaysResolvesInitialTenant()
    {
        var sut = new DefaultTenantContext();

        sut.TenantId.Should().Be(Tenant.InitialTenantId);
        sut.TenantSlug.Should().Be(Tenant.InitialTenantSlug);
        sut.IsResolved.Should().BeTrue();
    }
}
