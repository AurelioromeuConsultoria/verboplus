---
name: migrate-repository
description: Migrar acesso a dados legado (ADO.NET/DataSet/DataReader ou SQL solto) para um repository EF Core no padrão do projeto, com LINQ, SQL cru parametrizado e ramificado por provider, filtro de tenant e paginação por tupla. Use ao portar camada de dados legada.
---

# Migrate Repository (ADO.NET/legado → EF Core)

**Agente:** backend-dotnet-engineer (SQL cru ramificado → revisar com ef-migrations-engineer).
**Fonte:** MIGRATION_RULES.md §18.

## Objetivo
Portar acesso a dados legado para `{X}Repository : I{X}Repository` em EF Core, sem ADO.NET legado e respeitando o isolamento de tenant.

## Pré-requisitos
- Ler MIGRATION_RULES.md §18, CODING_STANDARDS.md §5.
- Entidade/DTOs de destino existentes.

## Entradas esperadas
Queries legadas (SQL/DataReader), entidade-alvo, necessidade de paginação, providers (PG/SQL Server).

## Processo
1. Criar `{X}Repository : I{X}Repository`, recebendo `SistemaIgrejaDbContext` + contexto de tenant.
2. Reescrever queries em **LINQ**; SQL cru só quando o ORM não expressa.
3. SQL cru **parametrizado** (`{0}`, nunca concatenação) e **ramificado por provider** (`ProviderName.Contains("Npgsql")` → `FOR UPDATE SKIP LOCKED`; SQL Server → `WITH (UPDLOCK, ROWLOCK)`).
4. Filtrar por tenant **confiando no global query filter** (não burlar).
5. Paginação retorna `(Items, Total)`.
6. Remover ADO.NET legado (`DataSet`/`DataReader`).
7. Async em todo acesso a dados.

## Validações
- Zero `DataSet`/`DataReader`.
- SQL cru parametrizado e ramificado por provider.
- Não burla o filtro de tenant; paginação em tupla.

## Resultado esperado
Repository EF Core no padrão, sem ADO.NET, isolado por tenant.

## Critérios de conclusão
Build/testes verdes; resultados equivalentes ao legado em PG e SQL Server.

## Quando NÃO usar
Quando basta o repositório padrão de uma feature nova (→ `backend-feature-crud`); para a migration de schema em si (→ `ef-schema-migration`).

## Exemplos
- "Migrar o DAO legado de relatórios (DataReader) para um repository EF com paginação."
