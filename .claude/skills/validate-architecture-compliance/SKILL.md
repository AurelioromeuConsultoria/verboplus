---
name: validate-architecture-compliance
description: Validar que um diff respeita as fronteiras de camadas do projeto (controllers só I{X}Service; services sem DbContext/HttpContext direto; cliente HTTP em Application/Services; interfaces em Application/Interfaces; DI sincronizado API+Worker; sem dependência interna no Domain). Use como gate arquitetural antes do merge.
---

# Validate Architecture Compliance

**Agente:** backend-dotnet-engineer / integracoes-jobs-engineer.
**Fonte:** ARCHITECTURE.md, CODING_STANDARDS.md.

## Objetivo
Garantir que as dependências entre camadas seguem a Clean Architecture do projeto.

## Pré-requisitos
- Diff disponível; ARCHITECTURE.md lido.

## Entradas esperadas
Arquivos alterados em Domain/Application/Infrastructure/API/Worker.

## Processo (checklist)
1. **Controllers** não acessam `DbContext`/EF/`HttpClient` — só `I{X}Service` (e ocasionalmente `I{X}Repository` para resolver IDs).
2. **Services** não acessam `DbContext` direto (vai por repository) nem `HttpContext` (vem por `ICurrentUserContext`).
3. **Interfaces** `I{X}Service`/`I{X}Repository` em `Application/Interfaces/`; impl de repo em `Infrastructure/Repositories/`.
4. **Cliente HTTP** mora em `Application/Services` (orquestração); `Infrastructure/Services` só para SDK pesado (S3) ou que toca o banco (SMTP/Billing/schedulers). Firebase push é exclusivo da API.
5. **Domain** não depende de nada interno.
6. **DI**: registro inline em cada `Program.cs`; se scheduler do Worker depende do service/repo, registrado na API **e** no Worker (`ValidateOnBuild`).
7. **Sem tecnologia fora de escopo** (Redis, broker, MFA) sem alinhamento explícito.

## Validações
Itens 1, 2 e 6 são **bloqueantes** (quebram o startup do Worker ou vazam acoplamento). Inconsistências documentadas: seguir o que está mais próximo do domínio e ser consistente.

## Resultado esperado
Lista de violações de camada com arquivo:linha e a fronteira violada.

## Critérios de conclusão
Nenhuma violação bloqueante; Worker sobe (`ValidateOnBuild`); itens de atenção registrados.

## Quando NÃO usar
Para regras de estilo/proibições de lib (→ `validate-coding-standards`); para isolamento de tenant (→ `review-tenant-isolation`).

## Exemplos
- "Verificar se o novo service não acessa HttpContext e se o DI está no Worker."
