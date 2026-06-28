---
name: create-multitenant-scheduler
description: Criar um BackgroundService que processa trabalho por tenant com loop tolerante a falha, jitter, ISchedulerExecutionMonitor, scope DI + TenantScopeOverride por tenant e reserva concorrente (SKIP LOCKED/UPDLOCK). Use para jobs em lote/fila-por-estado no Worker.
---

# Create Multitenant Scheduler

**Agente:** integracoes-jobs-engineer.
**Fonte:** INTEGRATION_PATTERNS.md §14.

## Objetivo
Job em background resiliente por tenant: falha de um item não derruba o lote; isolamento de tenant por scope.

## Pré-requisitos
- Ler INTEGRATION_PATTERNS.md §14.
- "Fila" como tabela processada por estado já definida; services/repos a consumir registrados.

## Entradas esperadas
Nome do scheduler, intervalo base + jitter máx, tamanho do lote, entidade/estado da fila, se é multi-tenant.

## Processo
1. Classe herdando `BackgroundService` em `Infrastructure/Services/`; injetar `IOptions<{X}SchedulerSettings>`, `ILogger<T>`, `IServiceProvider`, `ISchedulerExecutionMonitor` e (se multi-tenant) `ITenantService`.
2. **Loop** `while(!stoppingToken.IsCancellationRequested)`; medir `startedAtUtc`; `try` → trabalho + `RecordSuccess(SchedulerName, segundos)`; `catch (Exception)` → log + `RecordFailure(SchedulerName, ex.Message)`.
3. **Jitter sempre**: `await Task.Delay((intervaloBase + Random.Shared.Next(0, jitterMax+1)) * 1000, stoppingToken)`.
4. **Multi-tenant**: `GetActiveTenantsAsync()`; por tenant `using var scope = _serviceProvider.CreateScope()` e `scope.ServiceProvider.GetService<TenantScopeOverride>()?.SetTenant(id, slug)` ANTES de resolver services scoped.
5. **Try/catch por item**: falha marca `Status = Erro` na entidade e segue o lote (sem dead-letter queue).
6. **Reserva concorrente**: `FOR UPDATE SKIP LOCKED` (PG) / `WITH (UPDLOCK, ROWLOCK)` (SQL Server) — ramificado por provider.
7. **Flag `Enabled`**: respeitar; no-op quando desligado.
8. **Registro**: `AddHostedService<{X}Scheduler>()` no `Program.cs` do **Worker** (não duplicar dispatch API+Worker sem alinhar — gap de lock conhecido).

## Validações
- Jitter presente; `RecordSuccess`/`RecordFailure` em todas as saídas.
- Scope + `SetTenant` por tenant antes de resolver scoped.
- Falha de item não derruba o lote; flag `Enabled` respeitada; reserva ramificada por provider.

## Resultado esperado
`{X}SchedulerService : BackgroundService` + settings + registro no Worker.

## Critérios de conclusão
Worker sobe (`ValidateOnBuild`); respeita `Enabled`; processa por tenant sem vazamento; falha isolada por item; build/testes verdes.

## Quando NÃO usar
CRUD (→ `backend-feature-crud`); cliente HTTP isolado (→ `create-integration`); webhook (→ `create-webhook-receiver`); decidir Worker vs API (decisão pendente — alinhar com devops).

## Exemplos
- "Job que envia mensagens agendadas pendentes por tenant a cada X segundos."
- "Scheduler que verifica cobranças vencidas e suspende inadimplentes."
