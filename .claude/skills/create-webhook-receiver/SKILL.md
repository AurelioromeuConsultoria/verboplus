---
name: create-webhook-receiver
description: Criar um receptor de webhook de gateway externo com controller [AllowAnonymous] de rota absoluta, validação por token (Ordinal), idempotência (tabela de eventos ou estado da entidade) e isenção nos middlewares de gating/permissão. Use ao receber eventos de terceiros.
---

# Create Webhook Receiver (idempotente)

**Agente:** integracoes-jobs-engineer.
**Fonte:** INTEGRATION_PATTERNS.md §7, §15.

## Objetivo
Receber eventos externos com validação por token e idempotência, sem quebrar gating/RBAC nem reprocessar eventos.

## Pré-requisitos
- Ler INTEGRATION_PATTERNS.md §7.
- Cliente da integração existente (`create-integration`).
- Definida a estratégia de idempotência (tabela de eventos vs estado da entidade).

## Entradas esperadas
Rota do webhook, nome do header de token, chave de idempotência (ex.: `paymentId`+`evento`), entidade-alvo.

## Processo
1. **Controller `[AllowAnonymous]`** com **rota absoluta** (`[HttpPost("/api/webhooks/...")]`, sem `[Route]` de classe); `[FromBody] JsonElement` (não DTO tipado).
2. **Isenção de middleware**: garantir prefixo `/api/webhooks` isento em `SubscriptionGatingMiddleware` e `PermissionMiddleware`.
3. **Validação por token**: comparar header com a config usando `StringComparison.Ordinal`; inválido → `Unauthorized()`. (Atual: token ausente na config = aceita.)
4. **Leitura defensiva**: helpers que checam `ValueKind` antes de extrair (`TryGetProperty` + `JsonValueKind`).
5. **Idempotência**: (a) tabela de eventos (`EventoWebhookBilling` por `(paymentId, evento)`) — já existe → `Ok()` silencioso; ou (b) estado da entidade — buscar por `External*Id`; se no estado final, ignora.
6. **Resposta**: `Ok()` quando processado (inclusive ao ignorar por idempotência ou não achar a entidade); `Unauthorized()` só em token inválido.
7. **Separação**: manter webhooks distintos em rotas separadas.
8. **HMAC** é gap conhecido — só implementar se solicitado, alinhando com o time.

## Validações
- Reenvio do mesmo evento não duplica efeito (testado).
- Token inválido → 401; prefixo isento nos dois middlewares.
- Sem PII no log do payload (truncar corpo).

## Resultado esperado
Controller de webhook + token + idempotência + isenções de middleware + testes (feliz, token inválido, reprocessamento).

## Critérios de conclusão
Teste de reprocessamento prova no-op; 401 em token inválido; isento de gating/RBAC; build/testes verdes.

## Quando NÃO usar
Cliente HTTP de saída (→ `create-integration`); mudar comportamento de token/HMAC sem alinhar (→ `review-tenant-isolation`/segurança); processamento em lote (→ `create-multitenant-scheduler`).

## Exemplos
- "Receber confirmação de pagamento do Asaas e confirmar a doação (idempotente por externalPaymentId)."
