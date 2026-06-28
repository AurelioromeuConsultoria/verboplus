---
name: backend-feature-crud
description: Implementar/alterar uma feature de backend .NET de ponta a ponta (entidade → DbSet → migration aditiva → DTOs → repository → service → DI API+Worker → controller → RBAC → testes). Use ao criar ou evoluir CRUD/consulta de domínio no backend. NÃO use para migration com backfill, cliente de integração, auth/LGPD ou frontend.
---

# Backend Feature CRUD (end-to-end)

**Agente:** backend-dotnet-engineer.
**Fonte:** CODING_STANDARDS.md §13/§15.

## Objetivo
Implementar uma feature de backend mantendo isolamento de tenant, convenções do projeto e cobertura de testes, sem drift entre quem implementa.

## Pré-requisitos
- Ler CODING_STANDARDS.md, ARCHITECTURE.md, PROJECT_CONTEXT.md, DOMAIN_KNOWLEDGE.md.
- Domínio definido em Português (entidade, propriedades, regras).
- Confirmado que NÃO há backfill/dado legado (senão → `ef-schema-migration`/`tenantize-legacy-entity`) e que não toca auth/RBAC/LGPD (senão → skills de qualidade/LGPD).

## Entradas esperadas
Nome da entidade (PT), atributos e tipos, relacionamentos, regras de negócio, ações expostas, perfis que acessam (RBAC).

## Processo
1. **Entidade** (`Domain/Entities/`): `public int Id`; implementa `ITenantEntity` (`[Required] int TenantId` + `virtual Tenant Tenant = null!`); DataAnnotations; sem classe base. Navegação obrigatória `= null!`, opcional `T?`, coleção `= new List<T>()`. Enums no mesmo arquivo (int).
2. **DbContext**: `DbSet<>` + `OnModelCreating` (índice único `(TenantId, ...)`, FKs, `MaxLength`).
3. **Migration aditiva**: `dotnet ef migrations add Adicionar{X}` (env vars de Jwt/Connection). Revisar SQL PostgreSQL. *Com backfill/tenantização → delegar.*
4. **DTOs** (`Application/DTOs/[Dominio]/`): `{X}Dto`, `Criar{X}Dto`, `Atualizar{X}Dto`, `{X}PagedQueryDto`. `class` (nunca `record`) + DataAnnotations PT; sem regra de negócio.
5. **Repository**: interface em `Application/Interfaces/I{X}Repository.cs`; impl em `Infrastructure/Repositories/{X}Repository.cs` (DbContext + tenant context; paginação tupla `(Items, Total)`, default 20/teto 200, ordenação dinâmica switch case-insensitive, `AsNoTracking` em leitura paginada).
6. **Service** `{X}Service : I{X}Service`: injeta repos + `ILogger<T>`; mapper manual `private static MapToDto` (sem AutoMapper); exceções semânticas; log estruturado com placeholders, sem PII.
7. **DI**: registrar repo+service em `Program.cs` da **API** e **replicar no Worker** se houver job dependente (`ValidateOnBuild`). `AddScoped`.
8. **Controller** `{X}Controller`: `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`, `ControllerBase`, `async ActionResult<T>`; try/catch traduzindo exceção→HTTP com corpo **sempre** `{ message }`; `CreatedAtAction` no POST, `NoContent` no DELETE.
9. **RBAC**: mapear recurso/ação no `PermissionResourceMap`; semear `PerfilAcesso` se necessário.
10. **Testes** (`tests/`): xUnit + Moq + FluentAssertions, AAA sem comentários, `MethodName_Scenario_Expected`; teste de isolamento de tenant quando aplicável.
11. **Observabilidade**: logs/erros via `ILogger`/Sentry, sem PII.

## Validações
- Entidade implementa `ITenantEntity` e é coberta por global query filter + carimbo no `SaveChanges` (nova entidade global exige justificativa — só as 5 existentes são globais).
- Controller não acessa DbContext/EF/HttpClient; service não acessa DbContext direto nem HttpContext.
- Corpo de erro = `{ message }`. Stack de dados 100% async.
- DI presente na API e no Worker (se aplicável); Worker sobe (`ValidateOnBuild`).
- `dotnet build` + `dotnet test` verdes.

## Resultado esperado
Entidade + DbSet/config + migration aditiva + DTOs + repo + service + DI + RBAC + testes.

## Critérios de conclusão
Build e testes passam; checklist §13 coberto; revisão de isolamento (`review-tenant-isolation`) sem achados bloqueantes.

## Quando NÃO usar
Migration com backfill/consolidação/tenantização; cliente de integração/webhook/scheduler; mudança em auth/RBAC engine/LGPD; frontend.

## Exemplos
- "Criar CRUD de Patrimônio com paginação."
- "Adicionar campo e endpoints à entidade Pessoa."
- "Listar eventos por período com filtro e paginação."
