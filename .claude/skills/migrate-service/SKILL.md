---
name: migrate-service
description: Migrar lógica de negócio legada para um service no padrão do projeto, extraindo regra acoplada à entidade/fila legada, com mapeamento manual e adaptador (strangler). Use ao portar regra de negócio legada. NÃO use para criar service de feature nova (use backend-feature-crud).
---

# Migrate Service (extrair regra legada)

**Agente:** backend-dotnet-engineer.
**Fonte:** MIGRATION_RULES.md §17.

## Objetivo
Extrair regra de negócio de entidade/fila legada para um `{X}Service` no padrão atual, integrando o legado por adaptador em vez de substituir de uma vez.

## Pré-requisitos
- Ler MIGRATION_RULES.md §17, CODING_STANDARDS.md.
- Mapear onde a regra vive hoje (entidade rica, fila, code-behind).

## Entradas esperadas
Lógica legada a portar; repositórios/pipeline existentes a reaproveitar; DTOs envolvidos.

## Processo
1. Criar `{X}Service : I{X}Service`; injetar `IUnitOfWork`, repositórios, `ILogger<T>`.
2. **Extrair** a regra de negócio da entidade/fila legada para o service (separar responsabilidades acopladas).
3. Mapeamento DTO↔entidade **manual** (sem AutoMapper).
4. **Reaproveitar** pipeline/fila existente em vez de criar paralelo (ex.: `MensagemAgendada`).
5. Integrar o legado por **adaptador**, não substituir de uma vez (strangler).
6. Lançar exceções semânticas; logar erros (Sentry, sem PII).
7. Registrar no DI da **API e do Worker** se um scheduler depender dele.

## Validações
- Regra fora da entidade/fila legada, dentro do service.
- Sem AutoMapper; sem fila paralela nova.
- DI sincronizado API/Worker quando há job dependente.

## Resultado esperado
Service no padrão + adaptador para o legado + registros de DI.

## Critérios de conclusão
Build/testes verdes; comportamento equivalente ao legado; estruturas legadas ainda funcionando no curto prazo.

## Quando NÃO usar
Service de feature nova (→ `backend-feature-crud`); criar fila paralela (proibido — reaproveitar legado).

## Exemplos
- "Extrair a regra de envio da fila legada de mensagens para um MensagemService."
