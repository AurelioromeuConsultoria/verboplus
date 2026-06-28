using FluentAssertions;
using Moq;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class PermissionServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly PermissionService _service;

    public PermissionServiceTests()
    {
        _service = new PermissionService(_usuarioRepositoryMock.Object);
    }

    [Fact]
    public async Task HasPermissionAsync_ReturnsFalse_WhenUserMissingOrInactive()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Usuario?)null);
        (await _service.HasPermissionAsync(1, "USUARIOS", "view")).Should().BeFalse();

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Usuario { Ativo = false });
        (await _service.HasPermissionAsync(2, "USUARIOS", "view")).Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_ReturnsAdminFallback_WhenNoPerfilPermissions()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Usuario
        {
            Ativo = true,
            TipoUsuario = TipoUsuario.Admin,
            PerfilAcesso = null
        });

        (await _service.HasPermissionAsync(3, "USUARIOS", "edit")).Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_UsesPermissionFlags()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(new Usuario
        {
            Ativo = true,
            TipoUsuario = TipoUsuario.Portal,
            PerfilAcesso = new PerfilAcesso
            {
                Permissoes =
                [
                    new PerfilAcessoPermissao
                    {
                        Recurso = "AUDITORIA",
                        PodeVer = true,
                        PodeEditar = false,
                        PodeExcluir = true
                    }
                ]
            }
        });

        (await _service.HasPermissionAsync(4, "auditoria", "view")).Should().BeTrue();
        (await _service.HasPermissionAsync(4, "auditoria", "edit")).Should().BeFalse();
        (await _service.HasPermissionAsync(4, "auditoria", "delete")).Should().BeTrue();
        (await _service.HasPermissionAsync(4, "auditoria", "other")).Should().BeFalse();
    }
}
