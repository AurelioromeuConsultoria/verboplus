using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class DadosPessoaisServiceTests
{
    [Fact]
    public async Task ExportarAsync_ReuneDadosDoTitular()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        int pessoaId;
        await using (var seed = CreateContext(connection))
        {
            await seed.Database.EnsureCreatedAsync();

            var pessoa = new Pessoa { TenantId = Tenant.InitialTenantId, Nome = "Ana", Email = "ana@x.com", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow };
            seed.Pessoas.Add(pessoa);
            await seed.SaveChangesAsync();
            pessoaId = pessoa.Id;

            seed.PessoasPerfis.Add(new PessoaPerfil { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, Perfil = PerfilPessoa.Kids, DataInicio = DateTime.UtcNow });
            seed.CriancasDetalhes.Add(new CriancaDetalhe { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, Alergias = "Amendoim", DataCadastro = DateTime.UtcNow });
            seed.DoacoesOnline.Add(new DoacaoOnline { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, NomeDoador = "Ana", Valor = 50m });
            seed.Set<ConsentimentoRegistro>().Add(new ConsentimentoRegistro { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, Tipo = TipoConsentimento.ConsentimentoParental, VersaoDocumento = "v1", AceitoEm = DateTime.UtcNow, Origem = "kids_cadastro" });
            await seed.SaveChangesAsync();
        }

        await using var context = CreateContext(connection);
        var service = new DadosPessoaisService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var export = await service.ExportarAsync(pessoaId);

        export.Should().NotBeNull();
        export!.Pessoa.Nome.Should().Be("Ana");
        export.Perfis.Should().ContainSingle();
        export.DetalheCrianca.Should().NotBeNull();
        export.DetalheCrianca!.Alergias.Should().Be("Amendoim");
        export.Doacoes.Should().ContainSingle().Which.Valor.Should().Be(50m);
        export.Consentimentos.Should().ContainSingle().Which.VersaoDocumento.Should().Be("v1");
    }

    [Fact]
    public async Task ExportarAsync_RetornaNull_QuandoPessoaNaoExiste()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var service = new DadosPessoaisService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var export = await service.ExportarAsync(999999);

        export.Should().BeNull();
    }

    [Fact]
    public async Task AnonimizarAsync_AnonimizaIdentificaveis_PreservaAgregados_RevogaConsentimento_RegistraAuditoria()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        int pessoaId;
        await using (var seed = CreateContext(connection))
        {
            await seed.Database.EnsureCreatedAsync();

            var pessoa = new Pessoa { TenantId = Tenant.InitialTenantId, Nome = "Ana", Email = "ana@x.com", Telefone = "111", WhatsApp = "222", DataNascimento = new DateTime(2015, 1, 1), TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow };
            seed.Pessoas.Add(pessoa);
            await seed.SaveChangesAsync();
            pessoaId = pessoa.Id;

            seed.CriancasDetalhes.Add(new CriancaDetalhe { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, Alergias = "Amendoim", Observacoes = "nota sensível", DataCadastro = DateTime.UtcNow });
            seed.DoacoesOnline.Add(new DoacaoOnline { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, NomeDoador = "Ana", Email = "ana@x.com", Documento = "12345678900", Valor = 50m });
            seed.Set<ConsentimentoRegistro>().Add(new ConsentimentoRegistro { TenantId = Tenant.InitialTenantId, PessoaId = pessoaId, Tipo = TipoConsentimento.ConsentimentoParental, VersaoDocumento = "v1", AceitoEm = DateTime.UtcNow, Origem = "kids_cadastro" });
            await seed.SaveChangesAsync();
        }

        await using (var context = CreateContext(connection))
        {
            var service = new DadosPessoaisService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

            var result = await service.AnonimizarAsync(pessoaId);

            result.Should().NotBeNull();
            result!.NomeAnonimizado.Should().Be($"Titular removido #{pessoaId}");
            result.RegistrosAfetados.Should().BeGreaterThan(1);
        }

        await using var verify = CreateContext(connection);

        var pessoaAnon = await verify.Pessoas.FirstAsync(p => p.Id == pessoaId);
        pessoaAnon.Nome.Should().Be($"Titular removido #{pessoaId}");
        pessoaAnon.Email.Should().BeNull();
        pessoaAnon.Telefone.Should().BeNull();
        pessoaAnon.WhatsApp.Should().BeNull();
        pessoaAnon.DataNascimento.Should().BeNull();
        pessoaAnon.Ativo.Should().BeFalse();

        var detalhe = await verify.CriancasDetalhes.FirstAsync(c => c.PessoaId == pessoaId);
        detalhe.Alergias.Should().BeNull();
        detalhe.Observacoes.Should().BeNull();

        var doacao = await verify.DoacoesOnline.FirstAsync(d => d.PessoaId == pessoaId);
        doacao.NomeDoador.Should().Be($"Titular removido #{pessoaId}");
        doacao.Email.Should().BeNull();
        doacao.Documento.Should().BeNull();
        doacao.Valor.Should().Be(50m); // agregado financeiro preservado

        var consentimento = await verify.Set<ConsentimentoRegistro>().FirstAsync(c => c.PessoaId == pessoaId);
        consentimento.RevogadoEm.Should().NotBeNull();

        var audit = await verify.Set<AuditLog>().FirstOrDefaultAsync(a => a.Action == "Anonimizacao" && a.EntityId == pessoaId.ToString());
        audit.Should().NotBeNull();
        audit!.UserName.Should().Be("Admin Teste");
        audit.IpAddress.Should().Be("203.0.113.1");
    }

    [Fact]
    public async Task AnonimizarAsync_RetornaNull_QuandoPessoaNaoExiste()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var service = new DadosPessoaisService(context, new FixedTenantContext(Tenant.InitialTenantId), new StubCurrentUser());

        var result = await service.AnonimizarAsync(999999);

        result.Should().BeNull();
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

    private sealed class StubCurrentUser : ICurrentUserContext
    {
        public int? UserId => 7;
        public int? TenantId => Tenant.InitialTenantId;
        public string? TenantSlug => null;
        public string? UserName => "Admin Teste";
        public string? UserEmail => "admin@teste.com";
        public string? IpAddress => "203.0.113.1";
    }
}
