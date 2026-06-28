---
name: create-integration
description: Adicionar um cliente de integração HTTP typed com terceiro (config tipada, System.Text.Json, result object, kill-switch, retry/timeout, DI API+Worker, health check). Use ao integrar um serviço externo novo. NÃO use para webhook de entrada (create-webhook-receiver), job em lote (create-multitenant-scheduler) ou cliente legado (replace-legacy-http-client).
---

# Create Integration (cliente HTTP typed)

**Agente:** integracoes-jobs-engineer.
**Fonte:** INTEGRATION_PATTERNS.md §15, §4.

## Objetivo
Integrar um terceiro de forma config-driven e tolerante a falha, sem derrubar o fluxo de negócio.

## Pré-requisitos
- Ler INTEGRATION_PATTERNS.md, CODING_STANDARDS.md §4.
- API do terceiro documentada (endpoints, auth, formato).

## Entradas esperadas
Nome do serviço, base URL, tipo de credencial (global env var vs por tenant cifrada), se é crítico, se é pagamento (sem retry), se é canal de comunicação.

## Processo
1. **Config tipada** `Application/Configuration/{X}Settings.cs` com `const SectionName`, defaults, secrets `string.Empty`.
2. **Seção em `appsettings.json`** com secrets vazios; documentar env vars `{Section}__{Key}`.
3. **Interface + DTOs**: `I{X}Service`/`I{X}Client`; request (objeto anônimo) e resposta (`class` + `[JsonPropertyName]`); **result object** `{X}Result { Success, ErrorMessage, StatusCode? }`.
4. **Cliente HTTP** recebendo `HttpClient` + `IOptions<{X}Settings>` + `ILogger<T>`. Variante **A** (construtor, credencial global), **B** (`HttpRequestMessage` por chamada), **C** (reconfigura por chamada, credencial por tenant). `System.Text.Json` (`PropertyNameCaseInsensitive` se a API for instável). Validar argumentos cedo.
5. **Auth**: API Key/token em header da config; por tenant → cifrar com `IDataProtector` e mascarar. Nunca hardcode.
6. **Erros**: result object (`Success=false`) — não lançar; `try/catch (Exception)` em volta do HTTP; logar com `{StatusCode}`/`{RequestUri}`/IDs, sem PII, truncando corpo grande.
7. **Retry/timeout**: só se justificar (loop manual + backoff só 5xx/429; timeout explícito). **Pagamentos não retentam.** Sem Polly.
8. **Kill-switch**: no-op quando credencial vazia/`Enabled=false`.
9. **DI**: `AddHttpClient<I,Impl>()` + `Configure<{X}Settings>()` na API; replicar no Worker se scheduler usar.
10. **Health check** (se crítica): `{X}ConfigurationHealthCheck : IHealthCheck` + `AddCheck<...>`.
11. **Canal de comunicação** (se aplicável): implementar `IComunicacaoCanalProvider` (`ValidarConfiguracaoAsync` + `EnviarAsync`).
12. **Persistência**: correlacionar com colunas `Gateway*`/`External*`; upsert manual; sem bulk/procedure.
13. **Testes**: mockar `HttpClient`; cobrir feliz, não-2xx, exceção, retry.
14. **Documentar** bloqueio de produção em SAAS_READINESS.md se exigir conta/credencial.

## Validações
- Kill-switch comprovado (credencial vazia = no-op, não exceção).
- `System.Text.Json` (nunca Newtonsoft/Polly/SDK gerado); secrets fora do git.
- Nada lançado para o chamador; DI sincronizado API/Worker; sem PII.

## Resultado esperado
`{X}Settings` + seção vazia + cliente typed + result objects + kill-switch + DI + health check + testes; env vars documentadas.

## Critérios de conclusão
Build/testes verdes; com credencial vazia o app sobe e a feature fica off; checklist §15 coberto.

## Quando NÃO usar
CRUD sem terceiro (→ `backend-feature-crud`); webhook (→ `create-webhook-receiver`); job em lote (→ `create-multitenant-scheduler`); cliente legado (→ `replace-legacy-http-client`); provisionar credencial em prod (→ `provision-integration-secret`).

## Exemplos
- "Integrar um gateway de SMS via HTTP typed com kill-switch."
- "Cliente de PIX por tenant com credencial cifrada (variante C)."
