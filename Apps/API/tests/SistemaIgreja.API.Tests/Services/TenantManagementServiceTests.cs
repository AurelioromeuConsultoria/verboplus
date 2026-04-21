using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Auditoria;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class TenantManagementServiceTests
{
    [Fact]
    public async Task ProvisionAsync_CreatesTenantAdminProfilePersonAndUser()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var service = new TenantManagementService(
            context,
            new UnitOfWork(context),
            NullLogger<TenantManagementService>.Instance,
            new FakeAuditLogService());

        var result = await service.ProvisionAsync(new ProvisionTenantDto
        {
            Nome = "Igreja Teste",
            Slug = "igreja-teste",
            DominioPrimario = "igreja-teste.exemplo.com",
            AdminNome = "Admin Teste",
            AdminEmail = "admin@igreja-teste.com",
            AdminEmailLogin = "admin@igreja-teste.com",
            AdminSenha = "123456"
        });

        result.Tenant.Id.Should().BeGreaterThan(Tenant.InitialTenantId);
        result.Tenant.Slug.Should().Be("igreja-teste");
        result.Tenant.DominioPrimario.Should().Be("igreja-teste.exemplo.com");

        context.IgnoreTenantFilters = true;

        var tenant = await context.Tenants.Include(t => t.Domains).SingleAsync(t => t.Id == result.Tenant.Id);
        tenant.Nome.Should().Be("Igreja Teste");
        tenant.Domains.Should().ContainSingle(d => d.Domain == "igreja-teste.exemplo.com");

        var perfil = await context.PerfisAcesso
            .Include(p => p.Permissoes)
            .SingleAsync(p => p.Id == result.PerfilAcessoId);
        perfil.TenantId.Should().Be(result.Tenant.Id);
        perfil.Permissoes.Should().NotBeEmpty();
        perfil.Permissoes.Should().OnlyContain(p => p.TenantId == result.Tenant.Id);

        var pessoa = await context.Pessoas.SingleAsync(p => p.Id == result.PessoaId);
        pessoa.TenantId.Should().Be(result.Tenant.Id);
        pessoa.Email.Should().Be("admin@igreja-teste.com");

        var usuario = await context.Usuarios.SingleAsync(u => u.Id == result.UsuarioId);
        usuario.TenantId.Should().Be(result.Tenant.Id);
        usuario.PerfilAcessoId.Should().Be(result.PerfilAcessoId);
        usuario.TipoUsuario.Should().Be(TipoUsuario.Admin);

        context.IgnoreTenantFilters = false;
    }

    [Fact]
    public async Task UpdateStatusAsync_DoesNotAllowDisablingInitialTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var service = new TenantManagementService(
            context,
            new UnitOfWork(context),
            NullLogger<TenantManagementService>.Instance,
            new FakeAuditLogService());

        var action = async () => await service.UpdateStatusAsync(
            Tenant.InitialTenantId,
            new AtualizarTenantStatusDto { Ativo = false });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Mang Guarulhos*");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTenantNameSlugDomainAndLogo()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        context.IgnoreTenantFilters = true;

        var tenant = new Tenant
        {
            Id = 2,
            Nome = "Igreja Antiga",
            Slug = "igreja-antiga",
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);
        context.TenantDomains.Add(new TenantDomain
        {
            TenantId = 2,
            Domain = "antiga.exemplo.com",
            IsPrimary = true,
            Ativo = true
        });
        await context.SaveChangesAsync();
        context.IgnoreTenantFilters = false;

        var service = new TenantManagementService(
            context,
            new UnitOfWork(context),
            NullLogger<TenantManagementService>.Instance,
            new FakeAuditLogService());

        var result = await service.UpdateAsync(2, new AtualizarTenantDto
        {
            Nome = "Igreja Nova",
            Slug = "igreja-nova",
            DominioPrimario = "nova.exemplo.com",
            LogoUrl = "/uploads/images/logo-nova.png"
        });

        result.Nome.Should().Be("Igreja Nova");
        result.Slug.Should().Be("igreja-nova");
        result.DominioPrimario.Should().Be("nova.exemplo.com");
        result.LogoUrl.Should().Be("/uploads/images/logo-nova.png");
    }

    [Fact]
    public async Task DeleteAsync_RemovesPristineProvisionedTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var service = new TenantManagementService(
            context,
            new UnitOfWork(context),
            NullLogger<TenantManagementService>.Instance,
            new FakeAuditLogService());

        var provisioned = await service.ProvisionAsync(new ProvisionTenantDto
        {
            Nome = "Igreja Excluir",
            Slug = "igreja-excluir",
            AdminNome = "Admin Excluir",
            AdminEmail = "admin@excluir.com",
            AdminEmailLogin = "admin@excluir.com",
            AdminSenha = "123456"
        });

        await service.DeleteAsync(provisioned.Tenant.Id);

        context.IgnoreTenantFilters = true;
        (await context.Tenants.AnyAsync(t => t.Id == provisioned.Tenant.Id)).Should().BeFalse();
        (await context.Usuarios.AnyAsync(u => u.Id == provisioned.UsuarioId)).Should().BeFalse();
        (await context.Pessoas.AnyAsync(p => p.Id == provisioned.PessoaId)).Should().BeFalse();
        (await context.PerfisAcesso.AnyAsync(p => p.Id == provisioned.PerfilAcessoId)).Should().BeFalse();
        context.IgnoreTenantFilters = false;
    }

    private static SistemaIgrejaDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        return new SistemaIgrejaDbContext(options, new FixedTenantContext(Tenant.InitialTenantId));
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = tenantId == Tenant.InitialTenantId ? Tenant.InitialTenantSlug : null;
        public bool IsResolved => TenantId.HasValue;
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public Task<PagedResultDto<AuditLogDto>> GetPagedAsync(AuditLogPagedQueryDto query)
            => throw new NotSupportedException();

        public Task<AuditLogMetricsDto> GetMetricsAsync(AuditLogPagedQueryDto query)
            => throw new NotSupportedException();

        public Task RecordAsync(string entityName, string entityId, string action, object? changes = null)
            => Task.CompletedTask;
    }
}
