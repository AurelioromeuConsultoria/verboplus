---
name: validate-coding-standards
description: Validar um diff de backend contra as regras duras de CODING_STANDARDS (sem AutoMapper, sem record em DTO/entidade, sem Newtonsoft/Polly, corpo de erro {message}, DTOs class + DataAnnotations PT, idioma domínio PT/técnico EN, async total, sem PII em logs). Use como gate de padrões antes do merge.
---

# Validate Coding Standards

**Agente:** backend-dotnet-engineer (auto-revisão) ou revisor.
**Fonte:** CODING_STANDARDS.md.

## Objetivo
Garantir aderência às convenções inquebráveis do projeto antes do merge.

## Pré-requisitos
- Diff disponível; CODING_STANDARDS.md lido.

## Entradas esperadas
Arquivos backend alterados (entidades, DTOs, services, controllers, logs).

## Processo (checklist)
1. **Proibições**: nenhum AutoMapper (mapper manual `MapToDto`); nenhum `record` em DTO/entidade (`class`); nenhum Newtonsoft.Json (só System.Text.Json); nenhum Polly; nenhum Repository<T> genérico; nenhuma classe base de entidade; nenhum middleware global de exceção.
2. **DTOs**: `{X}Dto`/`Criar{X}Dto`/`Atualizar{X}Dto`/`{X}PagedQueryDto`; `class` + DataAnnotations em Português; sem regra de negócio.
3. **Erros**: services lançam exceções semânticas; controllers traduzem em try/catch com corpo **sempre** `{ message }`.
4. **Idioma**: domínio em Português (entidades, propriedades, mensagens); técnico em Inglês (sufixos Repository/Service/Dto, verbos CRUD, nomes de teste `MethodName_Scenario_Expected`).
5. **Async**: 100% async/await na stack de dados; `DateTime.UtcNow` em código novo.
6. **Logging**: `ILogger<T>` com placeholders estruturados (nunca interpolação); inclui IDs; **sem PII**; sem secrets.
7. **Paginação**: entrada `[FromQuery] {X}PagedQueryDto`; default 20/teto 200; `AsNoTracking` em leitura paginada.
8. **DI**: registro inline em cada `Program.cs`; sincronizado API/Worker quando há job dependente.

## Validações
Itens 1, 3 e 6 são **bloqueantes**. Onde o doc marca "TODO: confirmar com o time", seguir o padrão predominante e sinalizar — nunca inventar convenção nova.

## Resultado esperado
Lista de violações (bloqueante/atenção) com arquivo:linha e a regra correspondente.

## Critérios de conclusão
Nenhuma violação bloqueante; itens de atenção registrados.

## Quando NÃO usar
Para avaliar correção funcional/arquitetura de camadas (→ `validate-architecture-compliance`); para isolamento de tenant (→ `review-tenant-isolation`).

## Exemplos
- "Validar o diff da feature de Patrimônio contra os padrões de código."
