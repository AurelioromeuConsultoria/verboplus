using FluentAssertions;
using SistemaIgreja.API.Permissions;

namespace SistemaIgreja.API.Tests.Services;

public class PermissionResourceMapTests
{
    [Theory]
    [InlineData("/api/usuarios/5", "usuarios")]
    [InlineData("/api/comunicacaocampanhas/10/entregas", "comunicacao")]
    [InlineData("/api/kids/checkins", "kids")]
    [InlineData("/api/auditlogs/metrics", "usuarios")]
    public void GetResourceFromPath_ReturnsMappedResource(string path, string expected)
    {
        var result = PermissionResourceMap.GetResourceFromPath(path);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetResourceFromPath_ReturnsNull_ForUnknownPath()
    {
        PermissionResourceMap.GetResourceFromPath("/api/desconhecido").Should().BeNull();
    }

    [Theory]
    [InlineData("GET", "view")]
    [InlineData("POST", "edit")]
    [InlineData("PUT", "edit")]
    [InlineData("PATCH", "edit")]
    [InlineData("DELETE", "delete")]
    [InlineData("OPTIONS", null)]
    public void GetActionFromMethod_ReturnsExpectedAction(string method, string? expected)
    {
        var result = PermissionResourceMap.GetActionFromMethod(method);

        result.Should().Be(expected);
    }
}
