---
name: review-tenant-isolation
description: Revisão adversarial de isolamento de tenant e PII de uma feature antes do merge (ITenantEntity, global query filter + carimbo, IgnoreTenantFilters em try/finally, índice único por tenant, RBAC/gating, uploads, teste de isolamento). Use como gate de segurança de qualquer feature nova.
---

# Review Tenant Isolation

**Agente:** plataforma-seguranca-lgpd.
**Fonte:** ARCHITECTURE (Segurança), PROJECT_CONTEXT §10/§12.

## Objetivo
Garantir que uma feature não vaza dados entre tenants nem PII — a invariante máxima do SaaS.

## Pré-requisitos
- Feature implementada; acesso ao diff.
- Ler ARCHITECTURE (Segurança), PROJECT_CONTEXT §10/§12.

## Entradas esperadas
Entidades/endpoints/queries alterados; pontos com SQL cru, `IgnoreTenantFilters`, lookups cross-tenant, uploads, logs novos.

## Processo
1. **Entidades**: toda entidade de negócio nova implementa `ITenantEntity`? Alguma nova global sem `TenantId` (só as 5 conhecidas)? Justificada?
2. **Query filter + carimbo**: a entidade é coberta pelo global query filter e pelo carimbo no `SaveChanges`?
3. **Cross-tenant**: todo `IgnoreTenantFilters` é deliberado (ex.: billing) e está em `try/finally`?
4. **Índices**: índices únicos compostos por `(TenantId, ...)`?
5. **RBAC**: recurso/ação no `PermissionResourceMap`; método→ação correto; paths sensíveis não isentos indevidamente.
6. **Gating**: rotas respeitam `SubscriptionGatingMiddleware` (ou isentas corretas: `/api/auth`, `/api/upload`, `/api/webhooks`, `/api/billing`).
7. **PII**: nenhum dado pessoal em logs/Sentry; erros não vazam PII no corpo.
8. **Uploads/links**: arquivos novos não servidos sem auth com URL previsível (gap conhecido).
9. **Teste de isolamento**: existe teste provando que tenant A não lê dado de tenant B.

## Validações
Itens 1, 2, 4 e 9 são **bloqueantes**. Fail-open deliberado (gating sem tenant) NÃO deve ser "consertado" sem alinhamento — apenas registrado.

## Resultado esperado
Relatório de achados (bloqueante/atenção/ok) por item; teste de isolamento adicionado/confirmado; nota em SAAS_READINESS.md ao fechar gap de hardening.

## Critérios de conclusão
Nenhum achado bloqueante em aberto; teste de isolamento verde; não-bloqueantes registrados com responsável.

## Quando NÃO usar
Como substituto da implementação (é revisão); mudar fail-open/HMAC sem alinhar; hardening de rede/infra (→ devops).

## Exemplos
- "Revisar isolamento da nova feature de Patrimônio."
- "Auditar se o relatório consolidado vaza dados de outra igreja."
