# Skills — AppIgreja / Verbo+ (índice)

Procedimentos operacionais padronizados (SOPs) executáveis, derivados de PROJECT_CONTEXT,
CODING_STANDARDS, ARCHITECTURE, DOMAIN_KNOWLEDGE, INTEGRATION_PATTERNS, MIGRATION_RULES,
DECISIONS e dos 7 agentes. Cada skill é invocável (`/nome`) e descoberta automaticamente
pelos agentes via o campo `description` do seu `SKILL.md`.

Cada skill vive em `.claude/skills/<nome>/SKILL.md`. Este arquivo é só o catálogo.

## Catálogo (18 skills)

| Categoria | Skill | Prio | Agente | Fonte |
|-----------|-------|------|--------|-------|
| Backend | [backend-feature-crud](skills/backend-feature-crud/SKILL.md) | P0 | backend-dotnet-engineer | CODING_STANDARDS §13/§15 |
| Migração legado | [migrate-controller](skills/migrate-controller/SKILL.md) | P1 | backend-dotnet-engineer | MIGRATION_RULES §16 |
| Migração legado | [migrate-service](skills/migrate-service/SKILL.md) | P1 | backend-dotnet-engineer | MIGRATION_RULES §17 |
| Migração legado | [migrate-repository](skills/migrate-repository/SKILL.md) | P1 | backend-dotnet-engineer | MIGRATION_RULES §18 |
| Migração legado | [convert-dataset-to-dto](skills/convert-dataset-to-dto/SKILL.md) | P1 | backend + ef-migrations | MIGRATION_RULES §20 |
| Migração legado | [replace-legacy-http-client](skills/replace-legacy-http-client/SKILL.md) | P1 | integracoes-jobs-engineer | MIGRATION_RULES §19 |
| Schema/Banco | [ef-schema-migration](skills/ef-schema-migration/SKILL.md) | P0 | ef-migrations-engineer | MIGRATION_RULES §3/§6/§11/§20 |
| Schema/Banco | [tenantize-legacy-entity](skills/tenantize-legacy-entity/SKILL.md) | P0 | ef-migrations + segurança | MIGRATION_RULES §3/§18/§20 |
| Integrações | [create-integration](skills/create-integration/SKILL.md) | P1 | integracoes-jobs-engineer | INTEGRATION_PATTERNS §15/§4 |
| Integrações | [create-webhook-receiver](skills/create-webhook-receiver/SKILL.md) | P1 | integracoes-jobs-engineer | INTEGRATION_PATTERNS §7 |
| Integrações | [create-multitenant-scheduler](skills/create-multitenant-scheduler/SKILL.md) | P1 | integracoes-jobs-engineer | INTEGRATION_PATTERNS §14 |
| Qualidade | [review-tenant-isolation](skills/review-tenant-isolation/SKILL.md) | P0 | plataforma-seguranca-lgpd | ARCHITECTURE (Segurança) |
| Qualidade | [validate-coding-standards](skills/validate-coding-standards/SKILL.md) | P1 | backend-dotnet-engineer | CODING_STANDARDS |
| Qualidade | [validate-architecture-compliance](skills/validate-architecture-compliance/SKILL.md) | P1 | backend / integracoes | ARCHITECTURE |
| Frontend | [frontend-admin-feature](skills/frontend-admin-feature/SKILL.md) | P1 | frontend-web-engineer | PROJECT_CONTEXT §5/§13 |
| LGPD | [handle-data-subject-request](skills/handle-data-subject-request/SKILL.md) | P2 | plataforma-seguranca-lgpd | DOMAIN_KNOWLEDGE §8 |
| Infra | [provision-integration-secret](skills/provision-integration-secret/SKILL.md) | P2 | devops-infra-engineer | PROJECT_CONTEXT §9 / INTEGRATION_PATTERNS §13 |
| Infra | [configure-static-web-app](skills/configure-static-web-app/SKILL.md) | P2 | devops-infra-engineer | PROJECT_CONTEXT §9 |

## Descartadas (anti-redundância / não-recorrência)

Itens genéricos que **contradizem a stack real** e violariam a regra "nada genérico":
- **Create SQL Table / Create Index Strategy / Optimize Query** — projeto é EF Core Code First; tabela/índice nascem em `OnModelCreating` + migration. Índice `(TenantId,...)` é regra dentro de outras skills.
- **Create Bulk Insert** — INTEGRATION_PATTERNS é explícito: "sem bulk/procedure; upsert manual".
- **Create Parser** — respostas são DTOs `class` + `[JsonPropertyName]`; já coberto em `create-integration`.
- **Create Incremental Sync** — não há broker; "sync" = fila-por-estado + `SKIP LOCKED` = `create-multitenant-scheduler`.
- **Configure Managed Identity / Private Endpoint / Azure Resource (backend)** — backend roda em **Coolify (Docker), não Azure**; segredos = env var + `IDataProtector`. Azure só para frontends (→ `configure-static-web-app`).

Passos internos que NÃO viram skill própria (já são etapas de outras): "registrar DI API+Worker", "mapear RBAC". Fluxo Kids (mobile) é uma *feature*, não um SOP repetível.
