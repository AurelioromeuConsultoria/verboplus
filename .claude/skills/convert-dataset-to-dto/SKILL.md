---
name: convert-dataset-to-dto
description: Converter estrutura de dados legada (DataSet/DataTable/objeto solto) em entidade + DTOs do projeto, com migration que preserva dados (3 passos), dedup por chave natural e teste de isolamento de tenant. Use ao tipar dados legados não estruturados.
---

# Convert DataSet → DTO/Entity

**Agente:** backend-dotnet-engineer + ef-migrations-engineer (migration com dado).
**Fonte:** MIGRATION_RULES.md §20.

## Objetivo
Transformar dados legados não tipados (DataSet/DataTable) em entidade + DTOs do projeto, preservando os dados existentes.

## Pré-requisitos
- Ler MIGRATION_RULES.md §20.
- Identificar a chave natural para dedup (ex.: Email).

## Entradas esperadas
Estrutura legada (colunas/tipos), se é dado de igreja (tenant), chave natural, providers-alvo.

## Processo
1. Criar entidade em `Domain/Entities/` (com `ITenantEntity`/`TenantId` se for dado de igreja).
2. Criar DTOs `{X}Dto`/`Criar{X}Dto`/`Atualizar{X}Dto`; validação por DataAnnotations.
3. Mapeamento **manual** (sem AutoMapper).
4. Migration EF Core com **preservação de dados** (3 passos: nullable → backfill → NOT NULL).
5. **Dedup por chave natural** ao consolidar (`WHERE NOT EXISTS ... Email`).
6. Backfill **idempotente** (`WHERE "TenantId" = 0` ou condição de estado não-migrado).
7. `Down()` reversível.
8. Índices únicos compostos por `(TenantId, ...)`.
9. Teste de isolamento de tenant.

## Validações
- Nenhum dado perdido; backfill reexecutável sem efeito colateral.
- Dedup por chave natural; `Down()` reverte.
- Índices por tenant; teste de isolamento verde.

## Resultado esperado
Entidade + DTOs + migration com dados preservados + teste de isolamento.

## Critérios de conclusão
Migration aplica/reverte limpa em PG; reexecução é no-op; testes verdes.

## Quando NÃO usar
Dados já tipados; quando não há legado para preservar (→ `backend-feature-crud`).

## Exemplos
- "Converter o DataTable legado de cadastros em entidade Pessoa + DTOs, deduplicando por e-mail."
