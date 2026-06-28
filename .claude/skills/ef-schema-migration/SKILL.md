---
name: ef-schema-migration
description: Criar/alterar uma migration EF Core preservando dados (3 passos nullable→backfill→NOT NULL), idempotente, reversível (Down) e com SQL cru parametrizado e ramificado por provider. Use para mudanças de schema com dado existente. NÃO use para tenantizar entidade legada (use tenantize-legacy-entity) nem para migration aditiva trivial.
---

# EF Schema Migration (não-destrutiva)

**Agente:** ef-migrations-engineer.
**Fonte:** MIGRATION_RULES.md §3, §6, §11, §20.

## Objetivo
Evoluir o schema sem perder dados, de forma idempotente, reversível e multi-provider.

## Pré-requisitos
- Ler MIGRATION_RULES.md, CODING_STANDARDS.md §5, ARCHITECTURE (Persistência).
- Env vars de Jwt/Connection disponíveis localmente.

## Entradas esperadas
Mudança pretendida (coluna nova, consolidação, destrutiva), se há dados existentes, chave natural (se consolidação), providers-alvo.

## Processo
1. Classificar: aditiva trivial (pode ficar na feature) vs com dado (escopo desta skill).
2. **3 passos não-destrutivos**: (a) coluna `nullable: true`/com default → (b) backfill `INSERT...SELECT`/`UPDATE` → (c) tornar `NOT NULL`. Nunca dropar coluna antes de migrar dados.
3. **Idempotência**: backfill condicionado ao estado não-migrado; DDL bruto com `IF NOT EXISTS`/`ON CONFLICT DO NOTHING`; consolidação deduplica por chave natural (`WHERE NOT EXISTS`).
4. **Reversibilidade**: `Down()` recriando colunas/índices removidos.
5. **SQL cru** só quando o ORM não expressa: **parametrizado** (`{0}`) e **ramificado por provider** (`ProviderName.Contains("Npgsql")`).
6. **Gerar localmente**: `Jwt__Key='...' ConnectionStrings__DefaultConnection='Host=localhost;...' dotnet ef migrations add {NomeEmPortuguês}`. Nome `{timestamp}_{NomeEmPortuguês}` descritivo.
7. **Revisar o SQL gerado para PostgreSQL** (provider de produção). Preservar shims (`Npgsql.EnableLegacyTimestampBehavior=true`).
8. **Commit seguro** via `BackEnd/commit_migration_postgresql.sh`; `appsettings.json` excluído do commit (`git reset HEAD`) — nunca commitar secret.

## Validações
- Reexecução = no-op (idempotência); `Down()` reverte; nenhum dado perdido.
- SQL cru parametrizado e ramificado por provider.
- SQL revisado para PG; nenhum secret no commit.

## Resultado esperado
Migration (`Up`/`Down`) com backfill idempotente e SQL multi-provider.

## Critérios de conclusão
Aplica/reverte limpa em PG; reexecução no-op; testes verdes; commit sem appsettings.

## Quando NÃO usar
Tenantizar entidade legada (→ `tenantize-legacy-entity`); migration aditiva trivial sem dado (→ `backend-feature-crud`); provisionar/hardenizar banco de produção (→ devops).

## Exemplos
- "Tornar PessoaId obrigatório após backfill."
- "Consolidar Pessoa duplicada por e-mail preservando vínculos."
