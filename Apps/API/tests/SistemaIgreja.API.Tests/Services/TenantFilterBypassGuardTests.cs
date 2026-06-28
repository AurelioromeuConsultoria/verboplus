using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace SistemaIgreja.API.Tests.Services;

// E4 — Guard arquitetural (anti-regressão): habilitar IgnoreTenantFilters desliga todo o
// isolamento entre tenants. Este teste varre o código-fonte e falha se a flag for ativada
// (= true) fora de uma allowlist de classes auditadas. Assim, qualquer código novo que
// tente desligar o filtro de tenant quebra o build até ser revisado e adicionado à lista.
public class TenantFilterBypassGuardTests
{
    // Classes onde o bypass é intencional e auditado (operações cross-tenant de plataforma).
    private static readonly string[] AllowedFiles =
    [
        "DoacoesRepository.cs",
        "PerfilAcessoRepository.cs",
        "TenantManagementService.cs",
        // Billing é nível-plataforma (cross-tenant) com TenantId explícito no WHERE.
        "BillingService.cs",
        "BillingCycleService.cs",
        // Login self-service por e-mail único global precisa achar o usuário cross-tenant.
        "UsuarioRepository.cs",
        // Signup público confirma/ativa tenant sem contexto de tenant.
        "SignupService.cs"
    ];

    [Fact]
    public void IgnoreTenantFilters_IsOnlyEnabledInApprovedFiles()
    {
        var srcRoot = LocateSourceRoot();
        var assignmentRegex = new Regex(@"IgnoreTenantFilters\s*=\s*true", RegexOptions.Compiled);

        var offenders = Directory
            .EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildArtifact(path))
            .Where(path => assignmentRegex.IsMatch(File.ReadAllText(path)))
            .Select(Path.GetFileName)
            .Where(fileName => !AllowedFiles.Contains(fileName))
            .Distinct()
            .ToList();

        offenders.Should().BeEmpty(
            "habilitar IgnoreTenantFilters desliga o isolamento entre tenants; só é permitido nas " +
            "classes auditadas da allowlist. Se um novo uso for legítimo, adicione-o conscientemente a AllowedFiles.");
    }

    private static bool IsBuildArtifact(string path)
        => path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
           || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}");

    private static string LocateSourceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SistemaIgreja.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                $"Não foi possível localizar SistemaIgreja.sln a partir de '{AppContext.BaseDirectory}'.");
        }

        return Path.Combine(dir.FullName, "src");
    }
}
