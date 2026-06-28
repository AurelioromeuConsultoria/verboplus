---
name: replace-legacy-http-client
description: Substituir um cliente HTTP legado (Newtonsoft/SDK/SOAP/ADO solto) por um HttpClient typed do projeto, com System.Text.Json, config tipada, kill-switch, retry/timeout e decisão API vs Worker. Use ao modernizar integração legada. NÃO use para criar integração nova do zero (use create-integration).
---

# Replace Legacy HTTP Client

**Agente:** integracoes-jobs-engineer.
**Fonte:** MIGRATION_RULES.md §19 (espelha INTEGRATION_PATTERNS §15).

## Objetivo
Modernizar uma integração legada para o padrão typed do projeto, sem Newtonsoft/Polly/SDK gerado.

## Pré-requisitos
- Ler MIGRATION_RULES.md §19, INTEGRATION_PATTERNS.md.
- Mapear o cliente legado (auth, serialização, endpoints).

## Entradas esperadas
Cliente legado, formato de auth, DTOs de request/resposta, se é crítico/pagamento.

## Processo
1. Cliente `HttpClient` typed via `AddHttpClient<I, Impl>()`.
2. Converter serialização para **System.Text.Json** + `[JsonPropertyName]` (eliminar Newtonsoft).
3. Config tipada em `Application/Configuration/` com `SectionName`; segredos **vazios** no `appsettings`, vindos de env var.
4. Autenticação por header (API Key/token) lida de env var; nunca hardcode.
5. **Kill-switch**: no-op quando credencial vazia.
6. Retry + timeout configuráveis (só transitórios 5xx/429; pagamento não retenta).
7. Health check se for integração crítica.
8. Decidir API vs Worker; registrar no Worker se usada por scheduler.
9. Erros como result object; falhas logadas (Sentry), sem PII.

## Validações
- Zero Newtonsoft/Polly/SDK gerado.
- Kill-switch comprovado; secrets fora do git.
- DI sincronizado API/Worker quando há scheduler.

## Resultado esperado
Cliente typed no padrão substituindo o legado, com config/kill-switch/health.

## Critérios de conclusão
Build/testes verdes; comportamento equivalente ao legado; com credencial vazia a feature fica off silenciosamente.

## Quando NÃO usar
Integração nova sem legado (→ `create-integration`); webhook de entrada (→ `create-webhook-receiver`).

## Exemplos
- "Trocar o cliente Newtonsoft do gateway X por HttpClient typed com System.Text.Json."
