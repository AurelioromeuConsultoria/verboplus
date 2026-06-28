---
name: tenantize-legacy-entity
description: Adicionar TenantId a uma entidade legada single-tenant (defaultValue:0 → backfill por tenant idempotente → índice único composto (TenantId,...) → confirmar global query filter + carimbo). Use ao multi-tenantizar dado legado. NÃO use para schema sem dimensão de tenant (use ef-schema-migration).
---

# Tenantize Legacy Entity

**Agente:** ef-migrations-engineer + revisão de plataforma-seguranca-lgpd.
**Fonte:** MIGRATION_RULES.md §3, §18, §20.

## Objetivo
Multi-tenantizar uma entidade legada sem perder dados e garantindo isolamento de tenant.

## Pré-requisitos
- Ler MIGRATION_RULES.md, ARCHITECTURE (Segurança).
- Confirmar que o dado legado é single-tenant (tenant 0).

## Entradas esperadas
Entidade legada, mapeamento de qual tenant recebe os dados, índices únicos existentes.

## Processo
1. `TenantId` entra direto com `defaultValue: 0, nullable: false` (todo dado legado = tenant 0).
2. **Backfill por tenant** condicionado a `WHERE "TenantId" = 0` (idempotente, reexecutável).
3. Criar **índice único composto** `(TenantId, ...)` (nunca unicidade global de dado de tenant).
4. **Confirmar** cobertura do global query filter (reflexão no `OnModelCreating`) + carimbo automático no `SaveChanges` para a entidade.
5. `Down()` reversível.
6. **Atualizar/incluir** `TenantQueryFilterTests` (prova de isolamento).
7. Revisar o SQL gerado para PostgreSQL.

## Validações
- Backfill idempotente (`WHERE "TenantId" = 0`); reexecução no-op.
- Índice único composto por tenant.
- Entidade coberta por query filter + carimbo; teste de isolamento verde.

## Resultado esperado
Migration de tenantização + índice por tenant + teste de isolamento atualizado.

## Critérios de conclusão
Aplica/reverte em PG; tenant A não lê dado de tenant B (teste prova); reexecução no-op.

## Quando NÃO usar
Schema sem dimensão de tenant (→ `ef-schema-migration`); entidade nova já tenantizada (→ `backend-feature-crud`).

## Exemplos
- "Adicionar TenantId às tabelas de Comunicação (legado single-tenant)."
