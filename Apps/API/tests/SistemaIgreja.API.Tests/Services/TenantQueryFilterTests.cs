using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.API.Tests.Services;

public class TenantQueryFilterTests
{
    [Fact]
    public async Task TenantizedEntities_AreAutomaticallyFilteredByCurrentTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var seedContext = CreateContext(connection, new FixedTenantContext(Tenant.InitialTenantId)))
        {
            await seedContext.Database.EnsureCreatedAsync();
            seedContext.IgnoreTenantFilters = true;

            seedContext.Tenants.Add(new Tenant
            {
                Id = 2,
                Nome = "Outra Igreja",
                Slug = "outra-igreja",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });

            var pessoaTenant1 = new Pessoa
            {
                TenantId = Tenant.InitialTenantId,
                Nome = "Pessoa Tenant 1",
                Email = "tenant1@app.com",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            var pessoaTenant2 = new Pessoa
            {
                TenantId = 2,
                Nome = "Pessoa Tenant 2",
                Email = "tenant2@app.com",
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            seedContext.Pessoas.AddRange(pessoaTenant1, pessoaTenant2);
            await seedContext.SaveChangesAsync();

            seedContext.Usuarios.AddRange(
                new Usuario
                {
                    TenantId = Tenant.InitialTenantId,
                    PessoaId = pessoaTenant1.Id,
                    EmailLogin = "user1@app.com",
                    SenhaHash = "hash",
                    TipoUsuario = TipoUsuario.Admin,
                    Ativo = true
                },
                new Usuario
                {
                    TenantId = 2,
                    PessoaId = pessoaTenant2.Id,
                    EmailLogin = "user2@app.com",
                    SenhaHash = "hash",
                    TipoUsuario = TipoUsuario.Admin,
                    Ativo = true
                });

            seedContext.Visitantes.AddRange(
                new Visitante
                {
                    TenantId = Tenant.InitialTenantId,
                    PessoaId = pessoaTenant1.Id,
                    DataVisita = DateTime.UtcNow,
                    DataCadastro = DateTime.UtcNow
                },
                new Visitante
                {
                    TenantId = 2,
                    PessoaId = pessoaTenant2.Id,
                    DataVisita = DateTime.UtcNow,
                    DataCadastro = DateTime.UtcNow
                });

            await seedContext.SaveChangesAsync();
        }

        await using var tenant2Context = CreateContext(connection, new FixedTenantContext(2));

        var pessoas = await tenant2Context.Pessoas.ToListAsync();
        var usuarios = await tenant2Context.Usuarios.ToListAsync();
        var visitantes = await tenant2Context.Visitantes.ToListAsync();

        pessoas.Should().ContainSingle();
        pessoas[0].TenantId.Should().Be(2);
        usuarios.Should().ContainSingle();
        usuarios[0].TenantId.Should().Be(2);
        visitantes.Should().ContainSingle();
        visitantes[0].TenantId.Should().Be(2);
    }

    // E1 — Cobertura de isolamento para entidades sensíveis hoje não testadas:
    // dados de crianças (Kids), doações e contatos/fornecedores.
    [Fact]
    public async Task SensitiveEntities_AreIsolatedByTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var seedContext = CreateContext(connection, new FixedTenantContext(Tenant.InitialTenantId)))
        {
            await seedContext.Database.EnsureCreatedAsync();
            seedContext.IgnoreTenantFilters = true;

            seedContext.Tenants.Add(new Tenant
            {
                Id = 2,
                Nome = "Outra Igreja",
                Slug = "outra-igreja",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });

            var crianca1 = NovaPessoa(Tenant.InitialTenantId, "Crianca Tenant 1", TipoPessoa.Crianca);
            var crianca2 = NovaPessoa(2, "Crianca Tenant 2", TipoPessoa.Crianca);
            seedContext.Pessoas.AddRange(crianca1, crianca2);
            await seedContext.SaveChangesAsync();

            seedContext.CriancasDetalhes.AddRange(
                new CriancaDetalhe
                {
                    TenantId = Tenant.InitialTenantId,
                    PessoaId = crianca1.Id,
                    Alergias = "Amendoim",
                    DataCadastro = DateTime.UtcNow
                },
                new CriancaDetalhe
                {
                    TenantId = 2,
                    PessoaId = crianca2.Id,
                    Alergias = "Lactose",
                    DataCadastro = DateTime.UtcNow
                });

            seedContext.KidsCheckins.AddRange(
                new KidsCheckin { TenantId = Tenant.InitialTenantId, CriancaPessoaId = crianca1.Id, CodigoSessao = "S1" },
                new KidsCheckin { TenantId = 2, CriancaPessoaId = crianca2.Id, CodigoSessao = "S2" });

            seedContext.KidsOcorrencias.AddRange(
                new KidsOcorrencia
                {
                    TenantId = Tenant.InitialTenantId,
                    CriancaPessoaId = crianca1.Id,
                    RegistradoPorPessoaId = crianca1.Id,
                    Tipo = "Saude",
                    Titulo = "Ocorrencia T1",
                    Descricao = "Detalhe sensivel T1",
                    DataCriacao = DateTime.UtcNow
                },
                new KidsOcorrencia
                {
                    TenantId = 2,
                    CriancaPessoaId = crianca2.Id,
                    RegistradoPorPessoaId = crianca2.Id,
                    Tipo = "Saude",
                    Titulo = "Ocorrencia T2",
                    Descricao = "Detalhe sensivel T2",
                    DataCriacao = DateTime.UtcNow
                });

            seedContext.DoacoesOnline.AddRange(
                new DoacaoOnline { TenantId = Tenant.InitialTenantId, NomeDoador = "Doador T1", Valor = 10m },
                new DoacaoOnline { TenantId = 2, NomeDoador = "Doador T2", Valor = 20m });

            seedContext.Contatos.AddRange(
                new Contato { TenantId = Tenant.InitialTenantId, Nome = "Contato T1", WhatsApp = "111", Mensagem = "Ola", Membro = false },
                new Contato { TenantId = 2, Nome = "Contato T2", WhatsApp = "222", Mensagem = "Ola", Membro = false });

            seedContext.Fornecedores.AddRange(
                new Fornecedor { TenantId = Tenant.InitialTenantId, Nome = "Fornecedor T1" },
                new Fornecedor { TenantId = 2, Nome = "Fornecedor T2" });

            await seedContext.SaveChangesAsync();
        }

        await using var tenant2Context = CreateContext(connection, new FixedTenantContext(2));

        // Exatamente 1 registro por conjunto (o do tenant 1 foi filtrado) e todos pertencem ao tenant 2.
        var criancasDetalhes = await tenant2Context.CriancasDetalhes.ToListAsync();
        criancasDetalhes.Should().ContainSingle().Which.TenantId.Should().Be(2);

        var checkins = await tenant2Context.KidsCheckins.ToListAsync();
        checkins.Should().ContainSingle().Which.TenantId.Should().Be(2);

        var ocorrencias = await tenant2Context.KidsOcorrencias.ToListAsync();
        ocorrencias.Should().ContainSingle().Which.TenantId.Should().Be(2);

        var doacoes = await tenant2Context.DoacoesOnline.ToListAsync();
        doacoes.Should().ContainSingle().Which.TenantId.Should().Be(2);

        var contatos = await tenant2Context.Contatos.ToListAsync();
        contatos.Should().ContainSingle().Which.TenantId.Should().Be(2);

        var fornecedores = await tenant2Context.Fornecedores.ToListAsync();
        fornecedores.Should().ContainSingle().Which.TenantId.Should().Be(2);
    }

    // E2 — Travessia por FK não vaza entre tenants: um registro de outro tenant que
    // aponta (via FK) para uma entidade do tenant atual não pode aparecer, nem direto
    // no DbSet nem por navegação (Include).
    [Fact]
    public async Task ForeignKeyNavigation_DoesNotLeakAcrossTenants()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        int criancaId;
        await using (var seedContext = CreateContext(connection, new FixedTenantContext(Tenant.InitialTenantId)))
        {
            await seedContext.Database.EnsureCreatedAsync();
            seedContext.IgnoreTenantFilters = true;

            seedContext.Tenants.Add(new Tenant
            {
                Id = 2,
                Nome = "Outra Igreja",
                Slug = "outra-igreja",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            });

            var crianca = NovaPessoa(2, "Crianca Tenant 2", TipoPessoa.Crianca);
            seedContext.Pessoas.Add(crianca);
            await seedContext.SaveChangesAsync();
            criancaId = crianca.Id;

            // Check-in legítimo do tenant 2.
            seedContext.KidsCheckins.Add(new KidsCheckin
            {
                TenantId = 2,
                CriancaPessoaId = criancaId,
                CodigoSessao = "S-T2"
            });

            // Check-in "malicioso": pertence ao tenant 1, mas a FK aponta para a criança do tenant 2.
            seedContext.KidsCheckins.Add(new KidsCheckin
            {
                TenantId = Tenant.InitialTenantId,
                CriancaPessoaId = criancaId,
                CodigoSessao = "S-VAZAMENTO"
            });

            await seedContext.SaveChangesAsync();
        }

        await using var tenant2Context = CreateContext(connection, new FixedTenantContext(2));

        // Acesso direto ao DbSet: só o check-in do tenant 2.
        var checkins = await tenant2Context.KidsCheckins.ToListAsync();
        checkins.Should().ContainSingle();
        checkins[0].TenantId.Should().Be(2);
        checkins[0].CodigoSessao.Should().Be("S-T2");

        // Travessia por navegação: o registro do tenant 1 não aparece, mesmo apontando
        // para a criança do tenant 2.
        var criancaCarregada = await tenant2Context.Pessoas
            .Include(p => p.Checkins)
            .SingleAsync(p => p.Id == criancaId);

        criancaCarregada.Checkins.Should().ContainSingle();
        criancaCarregada.Checkins.Single().TenantId.Should().Be(2);
    }

    [Fact]
    public async Task ComunicacaoEntities_AreFilteredByTenant_AndTenantIdStampedOnInsert()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var seed = CreateContext(connection, new FixedTenantContext(Tenant.InitialTenantId)))
        {
            await seed.Database.EnsureCreatedAsync();
            seed.IgnoreTenantFilters = true;
            seed.ComunicacaoCampanhas.AddRange(
                new ComunicacaoCampanha { TenantId = Tenant.InitialTenantId, Nome = "Campanha T1", Objetivo = "x", PublicoAlvo = "membros" },
                new ComunicacaoCampanha { TenantId = 2, Nome = "Campanha T2", Objetivo = "x", PublicoAlvo = "membros" });
            seed.ComunicacaoTemplates.AddRange(
                new ComunicacaoTemplate { TenantId = Tenant.InitialTenantId, Nome = "Tpl T1", Objetivo = "x", Corpo = "oi" },
                new ComunicacaoTemplate { TenantId = 2, Nome = "Tpl T2", Objetivo = "x", Corpo = "oi" });
            await seed.SaveChangesAsync();
        }

        // Leitura como tenant 2: só enxerga os registros do próprio tenant.
        await using (var t2 = CreateContext(connection, new FixedTenantContext(2)))
        {
            (await t2.ComunicacaoCampanhas.ToListAsync()).Should().ContainSingle().Which.Nome.Should().Be("Campanha T2");
            (await t2.ComunicacaoTemplates.ToListAsync()).Should().ContainSingle().Which.Nome.Should().Be("Tpl T2");
        }

        // Insert SEM setar TenantId é carimbado com o tenant atual (rede de segurança).
        int novaId;
        await using (var t2 = CreateContext(connection, new FixedTenantContext(2)))
        {
            var nova = new ComunicacaoCampanha { Nome = "Sem tenant explícito", Objetivo = "x", PublicoAlvo = "membros" };
            t2.ComunicacaoCampanhas.Add(nova);
            await t2.SaveChangesAsync();
            novaId = nova.Id;
            nova.TenantId.Should().Be(2);
        }

        // Tenant 1 não enxerga a campanha criada pelo tenant 2.
        await using (var t1 = CreateContext(connection, new FixedTenantContext(Tenant.InitialTenantId)))
        {
            (await t1.ComunicacaoCampanhas.AnyAsync(c => c.Id == novaId)).Should().BeFalse();
        }
    }

    private static Pessoa NovaPessoa(int tenantId, string nome, TipoPessoa tipo) => new()
    {
        TenantId = tenantId,
        Nome = nome,
        TipoPessoa = tipo,
        Ativo = true,
        DataCriacao = DateTime.UtcNow
    };

    private static SistemaIgrejaDbContext CreateContext(SqliteConnection connection, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
            .UseSqlite(connection)
            .Options;

        return new SistemaIgrejaDbContext(options, tenantContext);
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = null;
        public bool IsResolved => TenantId.HasValue;
    }
}
