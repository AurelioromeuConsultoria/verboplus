---
name: provision-integration-secret
description: Ativar uma integração em produção provisionando o segredo no Coolify (env var por convenção __), mantendo appsettings vazio, replicando na API+Worker e validando que o kill-switch saiu de no-op via /health. Use ao habilitar integração em produção. NÃO use para escrever código de integração (use create-integration).
---

# Provision Integration Secret (Coolify)

**Agente:** devops-infra-engineer.
**Fonte:** PROJECT_CONTEXT §9, INTEGRATION_PATTERNS §13, ARCHITECTURE (Deploy/Config).

## Objetivo
Ativar uma integração em produção com segredo seguro e validar que ficou realmente ativa — sem divergência entre ambientes.

## Pré-requisitos
- Cliente da integração implementado (`create-integration`) com env vars documentadas.
- Acesso ao Coolify.

## Entradas esperadas
Integração e suas env vars (`{Section}__{Key}`), valores reais (fora do git), ambiente (sandbox/produção).

## Processo
1. **Confirmar convenção** `__` (ex.: `Billing__Asaas__ApiKey`, `Email__Password`, `EvolutionApi__ApiKey`, `Sentry__Dsn`, `Firebase__CredentialsJson`) — conferir a tabela de §13.
2. **Garantir** `appsettings.json` com o secret **vazio** (nunca commitar segredo — pós-incidente 2026-06-12).
3. **Setar** os valores SOMENTE como env vars no Coolify, na **API e no Worker** (se o scheduler usa a integração). Secrets por tenant (doações) NÃO vão em env var — ficam cifrados no banco (`IDataProtector`).
4. **Habilitar** a flag (`Enabled`/credencial presente) — esquecer deixa a feature silenciosamente off (kill-switch).
5. **Deploy** (mesma imagem em todos os ambientes; comportamento por configuração, não por branch).
6. **Validar via `/health`**: presença de config de Evolution/Email/Push e schedulers; conferir `SchedulerExecutionMonitor`. Sentry liga só com DSN não-vazio.
7. **Hardening relacionado** (com plataforma-seguranca-lgpd quando for código): porta 5433 exposta, Swagger público em prod, schedulers duplicados sem lock.

## Validações
- Nenhum segredo no git/appsettings.
- Env vars presentes na API e no Worker; `/health` reporta a integração configurada.
- Guard de startup aceita `Jwt:Key`; kill-switch saiu de no-op.

## Resultado esperado
Env vars provisionadas no Coolify; integração ativa e saudável em `/health`; bloqueador atualizado em SAAS_READINESS.md.

## Critérios de conclusão
Health check verde para a integração; app sobe sem secret no repositório; comportamento esperado em produção.

## Quando NÃO usar
Escrever código de integração/feature/migration ou lógica de segurança da app (→ engenheiros); hardening que exige código (HMAC, auth de upload — só a parte de infra/rede).

## Exemplos
- "Configurar Asaas e SMTP em produção (bloqueadores de lançamento)."
- "Ativar Firebase push setando Firebase__CredentialsJson no Coolify."
