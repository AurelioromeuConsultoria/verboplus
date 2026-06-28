---
name: migrate-controller
description: Migrar um controller legado para o padrão Clean Architecture do projeto (ControllerBase, ActionResult tipado, só I{X}Service, exceção→HTTP com {message}, RBAC). Use ao portar endpoints legados. NÃO use para criar feature nova do zero (use backend-feature-crud).
---

# Migrate Controller (legado → Clean Architecture)

**Agente:** backend-dotnet-engineer (com revisão de ef-migrations/segurança quando tocar dado/auth).
**Fonte:** MIGRATION_RULES.md §16.

## Objetivo
Portar um controller legado para o padrão atual sem acoplar acesso a dados nem quebrar RBAC.

## Pré-requisitos
- Ler MIGRATION_RULES.md §16, CODING_STANDARDS.md.
- Service de destino existente ou criado em paralelo (`migrate-service`).

## Entradas esperadas
Controller legado (rotas, verbos, payloads), service-alvo, recurso/ação RBAC.

## Processo
1. Herdar `ControllerBase`; anotar `[ApiController]`, rota `api/[controller]`, `[Authorize]`.
2. Tornar todos os endpoints `async`, retornando `ActionResult<T>` tipado.
3. Remover qualquer acesso a `DbContext`/repositório direto — injetar só `I{X}Service` via DI.
4. Traduzir exceções semânticas do service em status HTTP (404/400/401/403/500).
5. Erros de auth retornam corpo `{ message }`.
6. Mapear recurso/ação no `PermissionResourceMap` (RBAC).
7. Nomes em Português (domínio); se vier de legado em inglês, **alinhar política** (não inventar — sinalizar TODO).
8. Registrar no DI da API.

## Validações
- Zero acesso direto a EF/DbContext no controller.
- Corpo de erro `{ message }`; status corretos por exceção.
- Recurso/ação no `PermissionResourceMap`.

## Resultado esperado
Controller no padrão atual + entrada de RBAC + registro de DI.

## Critérios de conclusão
Build verde; endpoints respondem pelo service; RBAC mapeado.

## Quando NÃO usar
Feature nova (→ `backend-feature-crud`); migração da lógica de negócio em si (→ `migrate-service`).

## Exemplos
- "Portar o controller legado de Membros para o padrão atual."
