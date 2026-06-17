using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class SignupServiceTests
{
    [Fact]
    public async Task SignupAsync_ProvisionaTrialPendente_ComConsentimentoEVerificacao()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var signup = BuildSignup(context);

        var result = await signup.SignupAsync(NovoDto("nova@igreja.com"));

        result.Status.Should().Be("pendente_verificacao");
        result.Slug.Should().NotBeNullOrEmpty();
        result.LinkConfirmacao.Should().NotBeNull(); // e-mail desabilitado nos testes

        await using var verify = CreateContext(connection);
        verify.IgnoreTenantFilters = true;

        var tenant = await verify.Tenants.FirstAsync(t => t.Slug == result.Slug);
        tenant.Ativo.Should().BeFalse(); // pendente até confirmar

        var usuario = await verify.Usuarios.FirstAsync(u => u.TenantId == tenant.Id);
        usuario.Ativo.Should().BeFalse();
        usuario.EmailLogin.Should().Be("nova@igreja.com");

        (await verify.VerificacoesEmail.CountAsync(v => v.TenantId == tenant.Id)).Should().Be(1);
        (await verify.ConsentimentosRegistros.CountAsync(c => c.TenantId == tenant.Id)).Should().Be(2);
        var assinatura = await verify.Assinaturas.FirstAsync(a => a.TenantId == tenant.Id);
        assinatura.Status.Should().Be(StatusAssinatura.Trial);
    }

    [Fact]
    public async Task ConfirmarAsync_AtivaTenantEUsuario()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var signup = BuildSignup(context);

        await signup.SignupAsync(NovoDto("confirma@igreja.com"));

        string token;
        int tenantId;
        await using (var lookup = CreateContext(connection))
        {
            lookup.IgnoreTenantFilters = true;
            var v = await lookup.VerificacoesEmail.FirstAsync();
            token = v.Token;
            tenantId = v.TenantId;
        }

        var resultado = await signup.ConfirmarAsync(token);

        resultado.Confirmado.Should().BeTrue();

        await using var verify = CreateContext(connection);
        verify.IgnoreTenantFilters = true;
        (await verify.Tenants.FirstAsync(t => t.Id == tenantId)).Ativo.Should().BeTrue();
        (await verify.Usuarios.FirstAsync(u => u.TenantId == tenantId)).Ativo.Should().BeTrue();
        (await verify.VerificacoesEmail.FirstAsync(v => v.Token == token)).ConfirmadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task SignupAsync_EmailDuplicado_LancaConflito()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var signup = BuildSignup(context);

        await signup.SignupAsync(NovoDto("dup@igreja.com"));

        await signup.Invoking(s => s.SignupAsync(NovoDto("dup@igreja.com")))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ConfirmarAsync_TokenInvalido_NaoConfirma()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        var signup = BuildSignup(context);

        var resultado = await signup.ConfirmarAsync("token-que-nao-existe");

        resultado.Confirmado.Should().BeFalse();
    }

    private static SignupDto NovoDto(string email) => new()
    {
        NomeIgreja = "Igreja Nova",
        AdminNome = "Admin Teste",
        Email = email,
        Senha = "Senha12345",
        AceiteTermosVersao = "v1"
    };

    private static SignupService BuildSignup(SistemaIgrejaDbContext context)
    {
        var tenantContext = new FixedTenantContext(Tenant.InitialTenantId);
        var unitOfWork = new UnitOfWork(context);
        var tenantManagement = new TenantManagementService(context, unitOfWork, new Mock<ILogger<TenantManagementService>>().Object, new Mock<IAuditLogService>().Object);

        var asaas = new Mock<IAsaasBillingClient>();
        asaas.SetupGet(a => a.Configurado).Returns(false);
        var billing = new BillingService(
            context, asaas.Object,
            Options.Create(new BillingSettings { TrialDias = 14, CarenciaDias = 7 }),
            Options.Create(new AsaasBillingSettings()),
            tenantContext,
            new Mock<IEmailService>().Object,
            new Mock<ILogger<BillingService>>().Object);

        return new SignupService(
            context,
            tenantManagement,
            billing,
            new ConsentimentoRegistroRepository(context),
            new UsuarioRepository(context),
            new Mock<IEmailService>().Object,
            Options.Create(new PublicAppUrlSettings { ApiBaseUrl = "https://api.teste" }),
            Options.Create(new EmailSettings { Enabled = false }),
            new Mock<ILogger<SignupService>>().Object);
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
        public string? TenantSlug { get; } = null;
        public bool IsResolved => TenantId.HasValue;
    }
}
