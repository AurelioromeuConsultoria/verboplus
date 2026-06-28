# INTEGRATION_PATTERNS.md

> **Referência oficial de padrões de integração do backend AppIgreja / VerboPlus (`BackEnd/`).**
> Documenta os padrões **realmente observados no código** das integrações existentes, para que novas integrações sigam exatamente o mesmo modelo. **Não** propõe arquiteturas teóricas nem boas práticas genéricas que não estejam em uso.
>
> Regras deste documento:
> - Cada padrão tem **referência de arquivo/linha**.
> - Onde há **mais de um padrão**, identifica-se o **predominante**, as **exceções** e o **provável motivo**.
> - Onde não há evidência suficiente: `TODO: confirmar com o time`.
> - Complementa [.claude/PROJECT_CONTEXT.md](.claude/PROJECT_CONTEXT.md) (§6 Integrações Externas) e [.claude/CODING_STANDARDS.md](.claude/CODING_STANDARDS.md) (§4 Convenções de Integrações). Este documento é o **detalhamento** dessas seções.
> - Escopo: **backend .NET** (`BackEnd/src` + `BackEnd/SistemaIgreja.BackgroundWorker`). Frontends consomem a própria API e não têm integrações de terceiros relevantes.
> - Última análise: **2026-06-27**.

---

## 1. Visão Geral

### Filosofia de integração adotada
O projeto trata integrações externas como **detalhe de infraestrutura plugável e config-driven**, sempre atrás de uma interface, sempre desligável por configuração (**kill-switch**), e sempre falhando de forma controlada (no-op ou *result object*) em vez de derrubar o fluxo de negócio. Não há framework de integração — cada cliente é **escrito à mão** com `HttpClient` typed e `System.Text.Json`.

### Estratégia predominante (por evidência)
- **HTTP REST com `HttpClient` typed via `HttpClientFactory`** é o transporte de toda integração de API externa (Evolution, Asaas). Não há SOAP, gRPC, SDK REST gerado, ETL com ferramenta dedicada nem broker de mensageria.
- **SDKs oficiais** são usados apenas onde o provedor exige: `FirebaseAdmin` (push) e `AWSSDK.S3` (storage).
- **Sincronização assíncrona** (filas internas de mensagens, ciclo de billing) roda em **`BackgroundService` com jitter**, na API e/ou no Worker — sem Polly, sem Hangfire, sem Quartz, sem cron externo.
- **Webhooks de entrada** (Asaas) são recebidos por controllers `[AllowAnonymous]`, validados por **token** e processados com **idempotência**.

### Responsabilidades das camadas (onde cada peça de integração vive)
| Camada | Papel na integração | Exemplos |
|---|---|---|
| **Application/Configuration** | Classe `{X}Settings` (Options pattern) com `SectionName` e defaults; secrets `string.Empty` | `EvolutionApiSettings`, `AsaasBillingSettings`, `EmailSettings` |
| **Application/Services** | **Cliente HTTP** (interface + impl no mesmo arquivo) e **orquestração** de integração | `EvolutionApiService`, `AsaasBillingClient`, `AsaasPaymentService` |
| **Application/Interfaces** | Interface do serviço de integração | `IEvolutionApiService`, `IBillingService` |
| **Infrastructure/Services** | Integrações que tocam SDK/infra (SMTP, S3) e **schedulers** que disparam integrações; orquestração que toca o `DbContext` | `SmtpEmailService`, `S3FileStorageService`, `BillingService`, `MessageSchedulerService` |
| **API/Services** | Integração **exclusiva da API** (depende de processo HTTP) | `KidsPushNotificationService` (Firebase) |
| **API/Controllers** | **Recepção de webhooks** | `WebhooksBillingController`, `DoacoesController` (`/api/webhooks/asaas`) |

> **Detalhe que vale memorizar:** o **cliente HTTP da integração mora em `Application/Services`** (junto da lógica), não em `Infrastructure`. A `Infrastructure` recebe integrações que dependem de SDK pesado (S3) ou que tocam o banco (billing/schedulers). Ver [EvolutionApiService.cs](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs) (Application) vs. [S3FileStorageService.cs](BackEnd/src/SistemaIgreja.Infrastructure/Services/S3FileStorageService.cs) (Infrastructure).

### Inventário de integrações existentes
| Integração | Tipo | Transporte | Cliente | Onde roda |
|---|---|---|---|---|
| **Evolution API** (WhatsApp) | API REST de terceiro | `HttpClient` typed | `EvolutionApiService` | API + Worker |
| **Asaas — Billing da plataforma** | API REST (recorrência/assinaturas) | `HttpClient` typed | `AsaasBillingClient` | API (+ ciclo no Worker) |
| **Asaas — Doações (PIX por tenant)** | API REST (cobrança avulsa) | `HttpClient` typed | `AsaasPaymentService` | API |
| **SMTP / E-mail** | Protocolo SMTP | `System.Net.Mail.SmtpClient` | `SmtpEmailService` | API + Worker |
| **Firebase Cloud Messaging** | Push | SDK `FirebaseAdmin` | `KidsPushNotificationService` | **API apenas** |
| **AWS S3** (compatível) | Object storage | SDK `AWSSDK.S3` | `S3FileStorageService` | API (Singleton, opcional) |
| **Sentry** | Observabilidade | SDK `Sentry.*` | config no `Program.cs` | API + Worker |

Não foram encontrados: **SOAP, ETL, importação/exportação em lote de arquivos, OAuth1/OAuth2, certificados mTLS, paginação por cursor/token de provedores externos**. Onde o documento pede esses itens, está marcado como ausente.

---

## 2. Estrutura Padrão de uma Nova Integração

Componentes **realmente criados** ao adicionar uma integração HTTP (na ordem em que aparecem no código). Itens marcados *(opcional)* só existem em parte das integrações.

1. **Config tipada** — `Application/Configuration/{X}Settings.cs` com `public const string SectionName`, propriedades com default e secrets `= string.Empty`. Ex.: [EvolutionApiSettings.cs](BackEnd/src/SistemaIgreja.Application/Configuration/EvolutionApiSettings.cs), [AsaasBillingSettings.cs](BackEnd/src/SistemaIgreja.Application/Configuration/AsaasBillingSettings.cs).
2. **Seção em `appsettings.json`** — mesma `SectionName`, com **secrets vazios** (valor real vem de env var no Coolify). Ver [appsettings.json:62-89](BackEnd/src/SistemaIgreja.API/appsettings.json#L62-L89).
3. **Interface** — `I{X}Service` / `I{X}Client`. **Convive no mesmo arquivo da implementação** (ex.: `IAsaasBillingClient` em [AsaasBillingClient.cs:59](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L59)) ou em `Application/Interfaces/` (ex.: [IEvolutionApiService.cs](BackEnd/src/SistemaIgreja.Application/Interfaces/IEvolutionApiService.cs)). **Predominante para clientes novos: interface + impl no mesmo arquivo.**
4. **DTOs de request/response** — classes `public class`, com `[JsonPropertyName("...")]` nas de resposta. Em `EvolutionApiService` ficam em [EvolutionApiDto.cs](BackEnd/src/SistemaIgreja.Application/DTOs/EvolutionApiDto.cs); nos clientes Asaas ficam **inline como classes `private`/topo do arquivo** (ex.: `AsaasEntityResponse` em [AsaasBillingClient.cs:224](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L224)).
5. **Result objects** — classes `{X}Result` com `bool Success` + `string? ErrorMessage` (Asaas) ou um response próprio com `Sucesso`/`MensagemErro` (Evolution). Ver [AsaasBillingClient.cs:32-53](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L32-L53).
6. **Cliente HTTP (impl)** — classe que recebe `HttpClient` + `IOptions<{X}Settings>` + `ILogger<T>` por construtor.
7. **Registro DI** — `builder.Services.AddHttpClient<I{X}, {X}Impl>()` + `builder.Services.Configure<{X}Settings>(...)` em `Program.cs`. Ver [Program.cs:197-276](BackEnd/src/SistemaIgreja.API/Program.cs#L197-L276).
8. **Health check de configuração** *(opcional, integrações críticas)* — `{X}ConfigurationHealthCheck : IHealthCheck` em `API/Services/ConfigurationHealthChecks.cs`, registrado com `AddCheck<...>`. Ver [ConfigurationHealthChecks.cs](BackEnd/src/SistemaIgreja.API/Services/ConfigurationHealthChecks.cs).
9. **Provider de canal** *(opcional, comunicação omnichannel)* — `IComunicacaoCanalProvider` que adapta a integração ao pipeline de entregas. Ver [ComunicacaoCanalProviders.cs](BackEnd/src/SistemaIgreja.Application/Services/ComunicacaoCanalProviders.cs).
10. **Scheduler** *(opcional)* — `BackgroundService` no Worker (e/ou API) que dispara a integração em lote. Ver [MessageSchedulerService.cs](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs).
11. **Controller de webhook** *(opcional, integrações de entrada)* — controller `[AllowAnonymous]` com rota absoluta. Ver [WebhooksBillingController.cs](BackEnd/src/SistemaIgreja.API/Controllers/WebhooksBillingController.cs).

> **Não são criados:** classe `Mapper` dedicada (mapeamento é manual/inline), `SqlScript`/procedure (tudo é EF Core LINQ), camada `Repository` própria da integração (persistência usa os repositórios/`DbContext` existentes). Ver §9.

---

## 3. Fluxo Padrão de Integração

### 3.1 Saída — chamada síncrona a partir de um Service (ex.: doação PIX)
```
Service de negócio (DoacoesService)
  → resolve config + secret (GivingProviderConfig + IDataProtector.Unprotect)
  → Cliente typed (AsaasPaymentService.CreatePixPaymentAsync)
      → ConfigureHttpClient (BaseAddress + header access_token)
      → POST customers → POST payments → GET pixQrCode
      → desserializa (System.Text.Json + [JsonPropertyName])
      → retorna AsaasPaymentResult { Success, PixPayload, ... }
  → Service persiste resultado na entidade (DoacaoOnline) via repository
  → Logs (LogWarning/LogError em falha, sem PII)
```
Ref.: [AsaasPaymentService.cs:45-130](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L45-L130), [DoacoesService.cs:378-397](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L378-L397).

### 3.2 Saída — disparo assíncrono em lote (ex.: WhatsApp agendado)
```
Scheduler (BackgroundService, MessageSchedulerService)
  → loop por tenant ativo (GetActiveTenantsAsync)
      → cria scope DI + TenantScopeOverride.SetTenant(id, slug)
      → reserva lote (FOR UPDATE SKIP LOCKED) via service
      → para cada item:
          → Cliente typed (EvolutionApiService.EnviarMensagemTextoAsync)
              → formata telefone (TelefoneUtils)
              → POST message/sendText/{instance} com retry+backoff
              → retorna EvolutionApiResponse { Sucesso, MessageId, ... }
          → marca item Enviada / Erro (persistência)
  → ISchedulerExecutionMonitor.RecordSuccess/RecordFailure
  → delay base + jitter
```
Ref.: [MessageSchedulerService.cs:108-234](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L108-L234).

### 3.3 Entrada — webhook
```
Provedor externo (Asaas)
  → POST /api/webhooks/billing/asaas  (controller [AllowAnonymous])
      → lê header de token (asaas-access-token)
      → Service (BillingService.ProcessarWebhookAsync)
          → valida token (StringComparison.Ordinal) → 401 se inválido
          → idempotência: (paymentId, evento) já processado? → Ok()
          → grava EventoWebhookBilling (trilha) + atualiza Assinatura/Fatura
          → notifica por e-mail se aplicável
      → 200 OK (processado) | 401 Unauthorized (token inválido)
```
Ref.: [WebhooksBillingController.cs](BackEnd/src/SistemaIgreja.API/Controllers/WebhooksBillingController.cs), [BillingService.cs:303-402](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L303-L402).

---

## 4. Convenções de Clients

### Organização e nomenclatura
- **Localização:** `Application/Services/`. Cliente HTTP **não** fica em `Infrastructure`.
- **Nomes em uso:** `{X}Service` quando faz orquestração + HTTP (`EvolutionApiService`, `AsaasPaymentService`); `{X}Client` quando é um wrapper fino de uma API REST específica (`AsaasBillingClient`). Não há regra única — **predominante para wrapper puro de API: `{X}Client`**.
- **Interface no mesmo arquivo** da implementação (clientes Asaas) ou em `Application/Interfaces/` (Evolution). Para código novo, seguir o que estiver mais próximo do domínio.

### Construção do cliente (duas variantes observadas)
**Variante A — configuração no construtor** (cliente single-tenant, credencial global). Usada por `EvolutionApiService`: define `Timeout`, `BaseAddress` e header `apikey` **uma vez** no construtor a partir de `IOptions`.
```csharp
_httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
if (!string.IsNullOrEmpty(_settings.BaseUrl))
    _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
if (!string.IsNullOrEmpty(_settings.ApiKey))
    _httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey);
```
Ref.: [EvolutionApiService.cs:28-34](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L28-L34).

**Variante B — request montado por chamada** (`HttpRequestMessage`), quando os headers/URL são fixos mas se quer controle total. Usada por `AsaasBillingClient`:
```csharp
var request = new HttpRequestMessage(method, _settings.BaseUrl + path);
request.Headers.Add("access_token", _settings.ApiKey);
request.Headers.UserAgent.ParseAdd("VerboPlus/1.0");
request.Headers.Accept.ParseAdd("application/json");
if (body != null) request.Content = JsonContent.Create(body);
return await _httpClient.SendAsync(request, cancellationToken);
```
Ref.: [AsaasBillingClient.cs:189-201](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L189-L201).

**Variante C — reconfiguração por chamada** (credencial **por tenant**), quando a API key não é global e sim de cada igreja. `AsaasPaymentService.ConfigureHttpClient(config, apiKey)` redefine `BaseAddress`, limpa headers e adiciona `access_token` **a cada chamada**, escolhendo `BaseUrl` por ambiente:
```csharp
var baseUrl = config.Environment == GivingProviderEnvironment.Production
    ? "https://api.asaas.com/v3/" : "https://sandbox.asaas.com/api/v3/";
_httpClient.BaseAddress = new Uri(baseUrl);
_httpClient.DefaultRequestHeaders.Clear();
_httpClient.DefaultRequestHeaders.Add("access_token", apiKey);
```
Ref.: [AsaasPaymentService.cs:171-182](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L171-L182).

> **Regra de decisão:** credencial **global** → Variante A ou B (config no construtor / request por chamada). Credencial **por tenant** → Variante C (reconfigura a cada chamada com a key descriptografada da `GivingProviderConfig`).

### Construção de requests
- **POST com corpo:** `PostAsJsonAsync(endpoint, anonymousObject, ct)` (Evolution, Asaas Payment) ou `JsonContent.Create(body)` (Asaas Billing). O corpo é um **objeto anônimo** com as chaves no nome da API externa (camelCase): `new { name, email, cpfCnpj, mobilePhone }`. Ver [AsaasBillingClient.cs:94-100](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L94-L100).
- **Datas no request:** formatadas explicitamente como `string` invariante: `.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)`. Ver [AsaasBillingClient.cs:128](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L128).
- **Enums no request:** convertidos manualmente para o literal da API (`Ciclo == CicloCobranca.Anual ? "YEARLY" : "MONTHLY"`). Ver [AsaasBillingClient.cs:129](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L129).

### Serialização / desserialização
- **`System.Text.Json` exclusivamente** — nunca Newtonsoft.
- **Resposta tipada:** `ReadFromJsonAsync<T>()` com DTO marcado por `[JsonPropertyName("id")]` etc. Ver [AsaasPaymentService.cs:206-243](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L206-L243).
- **Resposta de formato instável** (Evolution v1/v2): lê como `string`, faz `JsonDocument.Parse` e navega defensivamente com `TryGetProperty`; usa `new JsonSerializerOptions { PropertyNameCaseInsensitive = true }`. Ver [EvolutionApiService.cs:344-358, 482-487](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L344-L358).
- **Datas na resposta:** parser tolerante com múltiplos formatos (`ParseAsaasDate` tenta `yyyy-MM-dd`, com hora, ISO) — [AsaasPaymentService.cs:184-204](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L184-L204).

### Responsabilidades do cliente
- Montar request, autenticar, serializar, chamar, desserializar, **traduzir falha em result object** e **logar**. **Não** persiste no banco e **não** aplica regra de negócio — isso é do service/scheduler que o chama (exceção: `EvolutionApiService` resolve mídia local/remota, o que é responsabilidade de transporte, não de negócio).

---

## 5. Convenções de Services (orquestração de integração)

- **Responsabilidades:** decidir *se* deve chamar (kill-switch / estado), resolver credencial, chamar o cliente, **interpretar o result object**, persistir o efeito (entidade), notificar (e-mail/push) e logar. Ex.: `BillingService.AssinarAsync` cria customer+subscription no Asaas e grava `Assinatura` — [BillingService.cs:60-148](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L60-L148).
- **Regras de negócio ficam no service**, não no cliente: ex. "billingType UNDEFINED para trial sem cartão" é decidido em `BillingService`, não em `AsaasBillingClient` — [BillingService.cs:102-112](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L102-L112).
- **Degradação graciosa:** quando a integração não está configurada, o service segue um caminho local. Ex.: sem Asaas, cria assinatura trial local e loga warning — [BillingService.cs:119-122](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L119-L122).
- **Dependências permitidas no service de orquestração:** o cliente de integração (`IAsaasBillingClient`), `IOptions<{X}Settings>`, `ITenantContext`, outros services (`IEmailService`), `ILogger<T>` e — quando está em `Infrastructure` — o `DbContext` diretamente (ex.: `BillingService` usa `SistemaIgrejaDbContext`).
- **Acesso cross-tenant controlado:** orquestrações de plataforma (billing) usam `_context.IgnoreTenantFilters = true` dentro de `try/finally` para enxergar todos os tenants — [BillingService.cs:63-66, 144-147](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L63-L66).

### Provider de canal (padrão de adaptação para comunicação omnichannel)
Para o módulo de Comunicação, cada integração de envio implementa `IComunicacaoCanalProvider` com:
- `Canal` (enum `CanalComunicacao`) e `Nome` (string);
- `ValidarConfiguracaoAsync()` → `ComunicacaoCanalDiagnostico { Configurado, Mensagem }` (lista campos faltantes);
- `EnviarAsync(entrega)` → `ComunicacaoCanalEnvioResultado { Sucesso, Mensagem }`.

Registrados como **múltiplas implementações da mesma interface** (`AddScoped<IComunicacaoCanalProvider, ...>` repetido), resolvidos por `IEnumerable<IComunicacaoCanalProvider>`. Ver [ComunicacaoCanalProviders.cs:21-94](BackEnd/src/SistemaIgreja.Application/Services/ComunicacaoCanalProviders.cs#L21-L94) e [Program.cs:181-184](BackEnd/src/SistemaIgreja.API/Program.cs#L181-L184). **O canal Push só é registrado na API** (depende de Firebase) — ver [BackgroundWorker/Program.cs:131-133](BackEnd/SistemaIgreja.BackgroundWorker/Program.cs#L131-L133).

---

## 6. Convenções de Autenticação

Apenas **dois mecanismos** estão em uso para integrações externas. Não há OAuth1/OAuth2, JWT de terceiro, Basic Auth nem certificados.

### API Key em header (predominante)
| Integração | Header | Origem da chave |
|---|---|---|
| Evolution API | `apikey` | `EvolutionApiSettings.ApiKey` (config/env var), setada no construtor |
| Asaas Billing | `access_token` | `AsaasBillingSettings.ApiKey` (config/env var), por request |
| Asaas Doações | `access_token` | **Por tenant**, de `GivingProviderConfig.ApiKeyProtegida`, descriptografada por chamada |

Ref.: [EvolutionApiService.cs:34](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L34), [AsaasBillingClient.cs:192](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L192), [DoacoesService.cs:388-392](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L388-L392).

### Credencial de SDK (Service Account)
- **Firebase:** `GoogleCredential.FromJson(...)` (preferido em container) ou `FromFile(path)`, de `FirebaseKidsPushOptions.CredentialsJson` / `CredentialsPath`. Inicialização **única, lazy, com lock** (`_firebaseInitialized` + `lock`). Ver [KidsPushNotificationService.cs:85-121](BackEnd/src/SistemaIgreja.API/Services/KidsPushNotificationService.cs#L85-L121).
- **AWS S3:** `BasicAWSCredentials(AccessKeyId, SecretAccessKey)` de `StorageSettings.S3`. Ver [S3FileStorageService.cs:31-39](BackEnd/src/SistemaIgreja.Infrastructure/Services/S3FileStorageService.cs#L31-L39).

### SMTP
- `NetworkCredential(Username, Password)` no `SmtpClient`, **só quando `Username` está preenchido**; `EnableSsl = UseSsl`. Ver [SmtpEmailService.cs:55-64](BackEnd/src/SistemaIgreja.Infrastructure/Services/SmtpEmailService.cs#L55-L64).

### Regras transversais de credenciais
- **Nunca hardcode.** Toda credencial vem de `IOptions<{X}Settings>` (env var no Coolify) ou — no caso por-tenant — de coluna **cifrada** no banco.
- **Cifragem de segredos por tenant:** chaves e webhook secrets de doação são guardados com `IDataProtector` (`_secretProtector.Protect/Unprotect`) e nunca em claro; armazena-se também os últimos dígitos mascarados (`ApiKeyUltimosDigitos`). Ver [DoacoesService.cs:253-256, 347](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L253-L256).

---

## 7. Convenções de Webhooks

- **Controller:** `[ApiController] [AllowAnonymous]`, **rota absoluta** no método (`[HttpPost("/api/webhooks/...")]`), sem `[Route]` de classe. Recebe o corpo como **`[FromBody] JsonElement payload`** (não DTO tipado). Ver [WebhooksBillingController.cs:12-29](BackEnd/src/SistemaIgreja.API/Controllers/WebhooksBillingController.cs#L12-L29).
- **Dois webhooks distintos** (não unificar): billing da plataforma (`/api/webhooks/billing/asaas`, processa `Assinatura`/`Fatura`) e doações por tenant (`/api/webhooks/asaas`, processa `DoacaoOnline`). Ambos isentos no `SubscriptionGatingMiddleware` e no `PermissionMiddleware` (prefixo `/api/webhooks`).
- **Validação por token (não HMAC):** compara o header de token com o segredo configurado usando `StringComparison.Ordinal`. Billing usa `AsaasBillingSettings.WebhookToken` global; doações usa `WebhookSecretProtegido` por tenant (descriptografado). Ver [BillingService.cs:306-311](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L306-L311) e [DoacoesService.cs:286-302](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L286-L302).
  - **Token ausente na config = aceita** (validação só roda se há token configurado) — comportamento atual, ver [BillingService.cs:306](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L306).
- **Idempotência (padrão obrigatório):**
  - Billing: tabela **`EventoWebhookBilling`** registra `(GatewayPaymentId, Evento, Processado, PayloadJson)`; se `(paymentId, evento)` já processado → retorna sucesso sem reprocessar — [BillingService.cs:329-348](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L329-L348).
  - Doações: idempotência por **estado da entidade** (`if (doacao.Status != Confirmada) ...`) — [DoacoesService.cs:304-315](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L304-L315).
- **Resposta:** `Ok()` quando processado (inclusive quando ignora por idempotência ou por não achar a assinatura) e `Unauthorized()` quando o token é inválido — [WebhooksBillingController.cs:27-28](BackEnd/src/SistemaIgreja.API/Controllers/WebhooksBillingController.cs#L27-L28).
- **Leitura defensiva do payload:** helpers `ReadString/ReadDecimal/ReadDate` que checam `ValueKind` antes de extrair — [BillingService.cs:537-547](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L537-L547).
- **Mapa de eventos tratados (Asaas billing):** `PAYMENT_CONFIRMED`/`PAYMENT_RECEIVED` → Ativa; `PAYMENT_OVERDUE` → Inadimplente (+e-mail); `PAYMENT_REFUNDED`/`PAYMENT_CHARGEBACK_REQUESTED` → Inadimplente. Ver [BillingService.cs:363-390](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L363-L390).

> **Gap conhecido (não inventar solução):** validação é só por token, **sem HMAC/assinatura** — hardening pendente registrado em [SAAS_READINESS.md](../docs/SAAS_READINESS.md).

---

## 8. Convenções de Tratamento de Erros

### Saída — duas estratégias, por integração
| Estratégia | Quem usa | Forma |
|---|---|---|
| **Result object** (predominante) | Evolution, Asaas Billing, Asaas Payment | retorna `{ Success/Sucesso=false, ErrorMessage/MensagemErro, StatusCode }`; **nunca lança** para o chamador |
| **Exceção propagada** | `SmtpEmailService` | não captura; deixa estourar (chamador decide) — [SmtpEmailService.cs](BackEnd/src/SistemaIgreja.Infrastructure/Services/SmtpEmailService.cs) |
| **Swallow + log** | `KidsPushNotificationService` | captura por token, loga `LogWarning` e segue (token pode estar inválido) — [KidsPushNotificationService.cs:62-82](BackEnd/src/SistemaIgreja.API/Services/KidsPushNotificationService.cs#L62-L82) |

O cliente sempre embrulha `try/catch (Exception ex)` em volta da chamada HTTP e converte em result object com a mensagem. Ver [AsaasBillingClient.cs:107-111](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L107-L111).

### Retries e backoff
- **Apenas a Evolution API tem retry.** Loop manual `for (tentativa = 1; tentativa <= MaxRetries; tentativa++)` com **backoff exponencial** (`RetryDelaySeconds * 2^(n-1)`, com cap em `min(60, RetryDelaySeconds*8)`). Ver [EvolutionApiService.cs:313-480](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L313-L480).
- **Retry só em falha transitória:** 5xx e 429. 4xx (exceto 429) falha imediatamente. Ver `IsTransientFailure` — [EvolutionApiService.cs:463-469](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L463-L469).
- **Clientes Asaas NÃO retentam** — falham rápido e retornam erro. (Decisão consistente: integração de pagamento não reenvia automaticamente.)
- **Sem Polly** — retry é loop à mão. **Não introduzir Polly** sem alinhar com o time.

### Timeout
- Evolution: explícito, `TimeSpan.FromSeconds(TimeoutSeconds)` (default 30s); download de mídia usa timeout reduzido `min(TimeoutSeconds, 15)`. Ver [EvolutionApiService.cs:28, 669](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L28-L28).
- Asaas: **timeout default do `HttpClient` (100s)** — não há override.
- Timeout (`TaskCanceledException`) na Evolution é tratado como transitório e entra no retry → retorna `StatusCode = 408` ao esgotar. Ver [EvolutionApiService.cs:405-419](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L405-L419).

### Rate limit
- Tratado **só implicitamente** pela Evolution: `429` é transitório e entra no backoff. Não há leitura de header `Retry-After` nem fila de rate limit dedicada. `TODO: confirmar com o time` se há necessidade.

### Dead letters
- **Não há dead-letter queue.** O equivalente é a **persistência do estado de falha na própria entidade**: mensagem agendada vai para `Erro` com a mensagem (`MarcarComoErroAsync`), webhook fica registrado em `EventoWebhookBilling`. Reprocessamento é manual/pelo scheduler. Ver [MessageSchedulerService.cs:148-157](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L148-L157).

### Validação de entrada antes de chamar
- Clientes validam argumentos cedo e retornam result object sem fazer HTTP: número/mensagem vazios na Evolution, `apiKey` vazia no Asaas Payment. Ver [EvolutionApiService.cs:49-67](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L49-L67), [AsaasPaymentService.cs:47-50](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L47-L50).

---

## 9. Convenções de Persistência (efeito da integração no banco)

- **Sem repositório dedicado à integração.** O efeito é gravado nas entidades de domínio existentes via `DbContext`/repository: `Assinatura`, `Fatura`, `EventoWebhookBilling` (billing); `DoacaoOnline` + `Receita` (doações); `MensagemAgendada`, `ComunicacaoEntrega` (comunicação).
- **Upsert manual por chave do gateway:** busca por `GatewayPaymentId`; se não existe, cria; senão atualiza. Não há `Upsert`/`Merge` de EF nem bulk. Ver `MarcarFaturaPagaAsync` — [BillingService.cs:440-462](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L440-L462).
- **Correlação com o externo via colunas `Gateway*`/`External*`:** `Assinatura.GatewayCustomerId`, `GatewaySubscriptionId`; `Fatura.GatewayPaymentId`; `DoacaoOnline.ExternalPaymentId`. É assim que webhook e polling reencontram a entidade local.
- **Reserva concorrente (anti-duplicação):** a fila de mensagens usa **`FOR UPDATE SKIP LOCKED`** (PostgreSQL) / `WITH (UPDLOCK, ROWLOCK)` (SQL Server) — único `FromSqlRaw` do projeto — em `MensagemAgendadaRepository.ReservarProntasParaEnvioAsync`. Ver [CODING_STANDARDS.md §4](.claude/CODING_STANDARDS.md) e `MensagemAgendadaRepository`.
- **Sem bulk insert / sem `HasData` para dados de integração.** Inserts via `Add`/`AddRange` + `SaveChangesAsync`.
- **Sincronização incremental vs. completa:**
  - **Incremental por estado:** schedulers processam só o que está "pronto"/"pendente" (`ReservarProntasParaEnvioAsync`, `ProcessarPendentesAsync`).
  - **Polling de status pontual:** doação não confirmada tem o status reconsultado no Asaas sob demanda (`TryRefreshAsaasStatusAsync`) — [DoacoesService.cs:329-376](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L329-L376).
  - **Não há** carga full/“full sync” de provedor externo nem espelhamento de tabelas externas.

---

## 10. Convenções de Paginação (de provedores externos)

- **Não existe** consumo paginado de API externa no código (nenhum `page/limit`, `offset`, `cursor`, `nextPageToken` contra Evolution/Asaas). As chamadas são pontuais (criar cobrança, consultar 1 pagamento, enviar 1 mensagem) ou recebem listas pequenas inteiras (`instance/fetchInstances`, enumerado sem paginação — [EvolutionApiService.cs:272-296](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L272-L296)).
- A paginação do projeto é **server-side da própria API** (`page`/`pageSize`, default 20, teto 200, tupla `(Items, Total)`) — documentada em [CODING_STANDARDS.md §3](.claude/CODING_STANDARDS.md), não em integração externa.
- `TODO: confirmar com o time` — se algum provedor passar a exigir consumo paginado, **não há padrão estabelecido**; definir antes de implementar.

---

## 11. Convenções de Filtros Incrementais / Checkpoints

- **Janela de tempo por configuração** (não checkpoint persistido): o `EscalaScheduler` opera numa janela `DiasJanelaInicio..DiasJanelaFim`; o `BirthdayCampaignScheduler` usa `TimeZoneId` + limites por execução. Ver [ConfigurationHealthChecks.cs:117-147](BackEnd/src/SistemaIgreja.API/Services/ConfigurationHealthChecks.cs#L117-L147) e [appsettings.json:95-101](BackEnd/src/SistemaIgreja.API/appsettings.json#L95-L101).
- **Filtro por estado da entidade** é o "incremental" real: processa-se o que está `Pendente`/`pronto para envio`, marcando `Enviada`/`Erro` — o próprio status é o checkpoint. Não há tabela de "último cursor processado" nem timestamp de última sincronização persistido por integração.
- **Limites por execução** evitam lotes gigantes: `BatchSizeReserva` (mensagens), `MaxPessoasPorExecucao` / `MaxTentativasPorPessoa` (aniversário). Ver [appsettings.json:90-101](BackEnd/src/SistemaIgreja.API/appsettings.json#L90-L101).
- `TODO: confirmar com o time` — não há checkpoint durável (ex.: "última data sincronizada com o gateway"); se necessário, definir padrão.

---

## 12. Convenções de Logging

- **Framework:** `ILogger<T>` (Microsoft.Extensions.Logging); erros sobem ao **Sentry** (`MinimumEventLevel = Error`, `SendDefaultPii = false`).
- **Logging estruturado com placeholders** `{Nome}` — nunca interpolação de string. Ex.: `LogWarning("Asaas billing respondeu {Status}: {Body}", (int)response.StatusCode, content)` — [AsaasBillingClient.cs:209](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L209).
- **Níveis em integração:**
  - `LogInformation` — chamada iniciada/concluída, `MessageId` retornado, boot do cliente. Ex.: [EvolutionApiService.cs:36-41](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L36-L41).
  - `LogWarning` — falha recuperável, resposta não-2xx, rejeição de webhook, tentativa de retry, credencial ausente. Ex.: [EvolutionApiService.cs:384-390](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L384-L390).
  - `LogError(ex, ...)` — exceção na chamada externa (exceção como **1º argumento**). Ex.: [AsaasPaymentService.cs:127](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L127).
- **Contexto incluído:** `StatusCode`, `RequestUri`, IDs de recurso (`{DoacaoId}`, `{MensagemId}`, `{TenantSlug}`, `{PaymentId}`). Corpo de resposta é **truncado** antes de logar (`Truncate(body, 600)`) para não poluir/expor demais — [EvolutionApiService.cs:521-526](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L521-L526).
- **Sem PII** em logs/Sentry (LGPD). Prévia de mensagem é truncada a ~50 chars — [EvolutionApiService.cs:323-324](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L323-L324).
- **Correlação:** não há `CorrelationId`/trace id próprio propagado entre serviços; a correlação é por IDs de domínio + `MessageId` do provedor. `TracesSampleRate=0` (sem tracing distribuído). `TODO: confirmar com o time` se correlação distribuída é desejada.

---

## 13. Convenções de Configuração

### appsettings.json
- Uma **seção por integração**, nome = `SectionName` da classe de config: `EvolutionApi`, `Email`, `Billing` (+`Asaas`), `Storage` (+`S3`), `Firebase`, `Sentry`, e seções de scheduler (`MessageScheduler`, `BirthdayCampaignScheduler`). Ver [appsettings.json](BackEnd/src/SistemaIgreja.API/appsettings.json).
- **Secrets sempre vazios no arquivo** (`"ApiKey": ""`, `"Password": ""`, `"Dsn": ""`). Defaults não-secretos podem vir preenchidos (`BaseUrl`, `InstanceName`, `Port`).

### Secrets e env vars
- Valores reais vêm de **env vars no Coolify**, com override por `__`: `Billing__Asaas__ApiKey`, `EvolutionApi__ApiKey`, `Email__Password`, `Sentry__Dsn`, `Firebase__CredentialsJson`.
- **Nunca commitar secret** (pós-incidente de rotação 2026-06-12).
- Secrets **por tenant** (doações) ficam **cifrados no banco** via `IDataProtector`, não em env var.

### Classe de configuração
```csharp
public class AsaasBillingSettings
{
    public const string SectionName = "Billing:Asaas";
    public string? ApiKey { get; set; }
    public string? WebhookToken { get; set; }
    public string Environment { get; set; } = "Sandbox";
    public bool IsProduction => string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase);
    public string BaseUrl => IsProduction ? "https://api.asaas.com/v3/" : "https://sandbox.asaas.com/api/v3/";
}
```
Ref.: [AsaasBillingSettings.cs](BackEnd/src/SistemaIgreja.Application/Configuration/AsaasBillingSettings.cs).
- `SectionName` é `const string` (pode ser aninhado com `:`, ex. `"Billing:Asaas"`).
- **Seleção de ambiente/endpoint dentro da própria config** (`IsProduction` → `BaseUrl`). Padrão sandbox/produção por flag de string, não por env name do ASP.NET.

### Registro no DI
```csharp
builder.Services.Configure<EvolutionApiSettings>(builder.Configuration.GetSection("EvolutionApi"));
builder.Services.Configure<AsaasBillingSettings>(builder.Configuration.GetSection(AsaasBillingSettings.SectionName));
builder.Services.AddHttpClient<IEvolutionApiService, EvolutionApiService>();
builder.Services.AddHttpClient<IAsaasBillingClient, AsaasBillingClient>();
```
Ref.: [Program.cs:197-276](BackEnd/src/SistemaIgreja.API/Program.cs#L197-L276).
- **DI é inline em cada `Program.cs`** — **não há módulo compartilhado** entre API e Worker. Registrar a integração **em ambos** quando um scheduler do Worker a usa (ver §15). O Worker usa `ValidateOnBuild = true` → drift quebra no startup.
- **Lifetimes:** `AddHttpClient<I,Impl>` para clientes HTTP; `AddScoped` para services de orquestração; `AddSingleton` para storage (`IFileStorageService`) e `ISchedulerExecutionMonitor`; `Configure<T>` para options.
- **Seleção de implementação por config** (storage): lê `Storage:Provider` e registra `S3FileStorageService` ou `LocalFileStorageService` — [Program.cs:289-295](BackEnd/src/SistemaIgreja.API/Program.cs#L289-L295).

### Health check de configuração
- Integração crítica ganha um `IHealthCheck` que **só valida presença de config** (não faz I/O): retorna `Healthy` / `Degraded` (config incompleta) / `Unhealthy` (valor inválido). Ver [ConfigurationHealthChecks.cs](BackEnd/src/SistemaIgreja.API/Services/ConfigurationHealthChecks.cs). Registrados via `AddCheck<...>` em [Program.cs:65-86](BackEnd/src/SistemaIgreja.API/Program.cs#L65-L86).

### Kill-switch (padrão forte e onipresente)
Integração **desligada quando credencial vazia/`Enabled=false`** → no-op, sem quebrar:
| Integração | Sinal de "ligado" |
|---|---|
| Asaas Billing | `Configurado => !string.IsNullOrWhiteSpace(_settings.ApiKey)` (checado antes de cada chamada) — [AsaasBillingClient.cs:83](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L83) |
| Email | `if (!_settings.Enabled) { log; return; }` — [SmtpEmailService.cs:31-35](BackEnd/src/SistemaIgreja.Infrastructure/Services/SmtpEmailService.cs#L31-L35) |
| Firebase | `EnsureFirebaseApp()` retorna `false` sem credencial → push não enviado — [KidsPushNotificationService.cs:52-56](BackEnd/src/SistemaIgreja.API/Services/KidsPushNotificationService.cs#L52-L56) |
| Sentry | DSN vazio = desligado |
| Asaas Doações | `config.Ativo && config.PixEnabled && ApiKeyProtegida != null` |

---

## 14. Convenções de Scheduler e Jobs

- **Mecanismo único:** `BackgroundService` (não `IHostedService` cru, não timers `System.Timers`, não cron externo, não fila/broker). Ver os 4 schedulers em `Infrastructure/Services/`.
- **Loop padrão:**
  ```csharp
  while (!stoppingToken.IsCancellationRequested)
  {
      var startedAtUtc = DateTime.UtcNow;
      try { /* trabalho */ _executionMonitor.RecordSuccess(...); }
      catch (Exception ex) { _logger.LogError(ex, "..."); _executionMonitor.RecordFailure(...); }
      await Task.Delay(ObterDelayComJitter(), stoppingToken);
  }
  ```
  Ref.: [MessageSchedulerService.cs:44-70](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L44-L70).
- **Jitter obrigatório:** `intervalo base + Random.Shared.Next(0, JitterSecondsMax+1)` segundos, para dessincronizar instâncias. Ver [MessageSchedulerService.cs:101-106](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L101-L106) e [BillingSchedulerService.cs:55-60](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingSchedulerService.cs#L55-L60).
- **Monitoramento:** `ISchedulerExecutionMonitor` (Singleton) com `RecordSuccess/RecordFailure` por execução, nomeando o scheduler (`const string SchedulerName`). Exposto em health check.
- **Habilitação por config:** schedulers respeitam `Enabled` (ex.: `BillingSchedulerService` só roda se `_settings.Enabled`).
- **Multi-tenant dentro do job:** itera tenants ativos, e **por tenant** cria um **scope DI** e seta o tenant via `TenantScopeOverride.SetTenant(id, slug)` antes de resolver services scoped:
  ```csharp
  using var scope = _serviceProvider.CreateScope();
  scope.ServiceProvider.GetService<TenantScopeOverride>()?.SetTenant(tenant.Id, tenant.Slug);
  var service = scope.ServiceProvider.GetRequiredService<I...Service>();
  ```
  Ref.: [MessageSchedulerService.cs:119-121](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L119-L121).
- **Onde rodam:** todos os 4 schedulers são registrados como `AddHostedService` **no Worker** ([BackgroundWorker/Program.cs:135-138](BackEnd/SistemaIgreja.BackgroundWorker/Program.cs#L135-L138)). Por isso o Worker re-registra integrações (Evolution, Email) e o grafo de DE delas.
  - **Gap conhecido:** `Scheduler:Enabled` também pode estar ligado na API → risco de execução duplicada **sem lock distribuído** (mitigado parcialmente pelo `SKIP LOCKED` da reserva de mensagens). Registrado como pendência em PROJECT_CONTEXT §16. **Não duplicar dispatch sem alinhar.**
- **Validação no boot:** o `MessageSchedulerService` valida a instância Evolution uma vez no início (`ValidarInstanciaAsync`) e loga o resultado — [MessageSchedulerService.cs:75-96](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L75-L96).
- **Falha de um item não derruba o lote:** `try/catch` por item; item com erro vai para estado `Erro` e o loop continua — [MessageSchedulerService.cs:127-158](BackEnd/src/SistemaIgreja.Infrastructure/Services/MessageSchedulerService.cs#L127-L158).

---

## 15. Checklist para Nova Integração

Ordem refletindo o fluxo real do projeto. (Espelha [CODING_STANDARDS.md §14](.claude/CODING_STANDARDS.md), com o detalhamento deste documento.)

1. **Config tipada** — criar `Application/Configuration/{X}Settings.cs` com `public const string SectionName`, defaults e secrets `string.Empty`. Adicionar seleção sandbox/produção via propriedade se aplicável.
2. **Seção em `appsettings.json`** — com a mesma `SectionName` e **secrets vazios**. Documentar as env vars (`{Section}__{Key}`) para o Coolify.
3. **Interface + DTOs** — `I{X}Service`/`I{X}Client`; DTOs de request (objeto anônimo na chamada) e de resposta (`class` + `[JsonPropertyName]`); **result objects** `{X}Result { Success, ErrorMessage }`.
4. **Cliente HTTP** — implementar recebendo `HttpClient` + `IOptions<{X}Settings>` + `ILogger<T>`. Escolher variante de config (construtor / por-request / por-tenant — §4). `System.Text.Json`. **Validar argumentos cedo.**
5. **Autenticação** — API Key em header a partir da config; nunca hardcode. Se for por tenant, cifrar com `IDataProtector` e guardar mascarado.
6. **Erros** — retornar result object (`Success=false`) em vez de lançar; `try/catch (Exception)` em volta do HTTP; logar `LogWarning`/`LogError` com `{StatusCode}`/`{RequestUri}`/IDs, **sem PII**, truncando corpo grande.
7. **Retry/timeout** — só se a integração justificar (modelo Evolution): loop manual + backoff exponencial, **só transitórios (5xx/429)**; timeout explícito. Pagamentos: **não** retentar.
8. **Kill-switch** — no-op quando credencial vazia/`Enabled=false` (`Configurado`, `if (!Enabled) return;`).
9. **Registro DI** — `AddHttpClient<I,Impl>()` + `Configure<{X}Settings>(...)` no `Program.cs` da **API**; replicar no **Worker** se um scheduler usar (atenção ao fechamento transitivo + `ValidateOnBuild`).
10. **Health check** *(crítica)* — `{X}ConfigurationHealthCheck : IHealthCheck` validando presença de config; registrar com `AddCheck<...>`.
11. **Provider de canal** *(se for canal de comunicação)* — implementar `IComunicacaoCanalProvider` (`ValidarConfiguracaoAsync` + `EnviarAsync`) e registrar como mais uma implementação da interface.
12. **Scheduler** *(se em lote)* — `BackgroundService` com jitter + `ISchedulerExecutionMonitor` + scope por tenant; registrar no Worker.
13. **Webhook** *(se entrada)* — controller `[AllowAnonymous]` com rota absoluta, `[FromBody] JsonElement`, validação por token (`Ordinal`), **idempotência** (tabela de eventos ou estado da entidade), `Ok()`/`Unauthorized()`. Isentar nos middlewares de gating/permissão (prefixo `/api/webhooks`).
14. **Persistência** — correlacionar com colunas `Gateway*`/`External*`; upsert manual por essa chave; sem bulk/procedure.
15. **Testes** — mockar o `HttpClient`/cliente; cobrir caminho feliz, erro não-2xx, exceção e (se houver) retry.
16. **Documentar** bloqueio de produção em [SAAS_READINESS.md](../docs/SAAS_READINESS.md) se exigir conta/credencial real.

---

## 16. Checklist para Nova Entidade Externa em Provedor Já Existente

Quando se adiciona um **novo recurso/objeto** numa integração que já existe (ex.: novo tipo de cobrança no Asaas, novo tipo de mensagem na Evolution):

1. **DTOs** — adicionar request (objeto anônimo na chamada) e response (`class` privada/topo do arquivo + `[JsonPropertyName]`) **no mesmo arquivo do cliente** (padrão Asaas) ou no `*Dto.cs` da integração (padrão Evolution).
2. **Método no cliente** — novo método `async Task<{X}Result>` reutilizando o helper de envio existente (`SendAsync`/`EnviarComRetryAsync`/`ConfigureHttpClient`); **não** duplicar a montagem de auth.
3. **Result object** — reaproveitar/estender o `{X}Result` da integração; manter `Success` + `ErrorMessage`.
4. **Mapeamento de datas/enums** — usar os helpers já existentes (`ParseAsaasDate`, conversão de enum para literal); não criar parser novo se um já cobre o formato.
5. **Evento de webhook** *(se o recurso gera webhook)* — adicionar o `case` no `switch` de eventos (ex.: `BillingService.ProcessarWebhookAsync`) e garantir a idempotência cobrindo o novo evento.
6. **Persistência** — adicionar/usar colunas `Gateway*`/`External*` na entidade correspondente para correlação.
7. **Kill-switch e config** — se o recurso exige nova flag/credencial, adicionar à `{X}Settings` (e `appsettings.json` vazio) e ao health check.
8. **Worker** — se o disparo for por scheduler, garantir que o método e suas dependências resolvem no Worker.
9. **Logs e testes** — log estruturado com o novo ID; teste cobrindo sucesso/erro do novo método.

---

## 17. Anti-Patterns Detectados (práticas evitadas, por ausência consistente)

Documentado apenas o que tem **evidência de ausência** no código. Não introduzir sem alinhar com o time.

- **AutoMapper** — ausente; mapeamento de DTO de integração é manual/inline.
- **Newtonsoft.Json** — ausente; só `System.Text.Json` nas integrações.
- **Polly / resiliência via biblioteca** — ausente; retry é loop manual (e só na Evolution).
- **Broker de mensageria (RabbitMQ/Kafka/SQS) / dead-letter queue** — ausente; jobs são `BackgroundService`, "DLQ" é estado de erro na entidade.
- **SDK REST gerado / cliente OpenAPI** — ausente; clientes são escritos à mão.
- **DTO de webhook tipado** — evitado; webhook é lido como `JsonElement` com acesso defensivo.
- **HMAC em webhook** — ausente (só token) — é **gap conhecido**, não um padrão a copiar.
- **Hardcode de credencial / secret em `appsettings`/git** — proibido (env var no Coolify; por-tenant cifrado).
- **Retry em pagamento** — evitado deliberadamente (Asaas falha rápido).
- **Cliente de integração dentro de controller/`DbContext` em cliente** — não ocorre; controller chama service, cliente não toca banco.
- **Cache distribuído (Redis) / leitura de `Retry-After` / tracing distribuído** — ausentes (`TracesSampleRate=0`). `TODO: confirmar com o time` se proibidos ou apenas não adotados.
- **Módulo de DI compartilhado entre API e Worker** — ausente por decisão; cada `Program.cs` registra inline (com risco de drift).

---

## 18. Dúvidas e Pendências

> `TODO: confirmar com o time` em cada item — não há evidência suficiente para fixar como padrão.

- **Lock distribuído para schedulers** — hoje podem rodar na API **e** no Worker sem lock (só `SKIP LOCKED` na fila de mensagens cobre parcialmente). Decisão definitiva: rodar schedulers só no Worker?
- **HMAC/assinatura de webhook** — validação atual é só por token. Quando adotar HMAC do Asaas? (hardening em SAAS_READINESS).
- **Webhook sem token configurado = aceito** — comportamento atual ([BillingService.cs:306](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L306)). É intencional para ambientes sem token, ou deveria rejeitar?
- **Retry/timeout dos clientes Asaas** — billing/payment usam timeout default (100s) e **não** retentam. Manter assim ou padronizar com a Evolution?
- **Rate limit de provedor** — só tratado como transitório (429→backoff) na Evolution. Há necessidade de respeitar `Retry-After` ou enfileirar?
- **Paginação/sincronização incremental durável** — não há cursor/checkpoint persistido por integração. Definir se/quando algum provedor exigir consumo paginado ou full sync.
- **Correlação distribuída (correlation id / tracing)** — ausente (`TracesSampleRate=0`). Adotar?
- **`DateTime.Now` vs `DateTime.UtcNow`** em código de integração — convivem (`DoacoesService` usa `DateTime.Now` em [DoacoesService.cs:309, 363](BackEnd/src/SistemaIgreja.Application/Services/DoacoesService.cs#L309-L309); billing usa `UtcNow`). CODING_STANDARDS recomenda `UtcNow` para código novo — confirmar regra oficial.
- **Local da interface do cliente** — `IEvolutionApiService` em `Application/Interfaces/` vs. `IAsaasBillingClient` no próprio arquivo. Fixar uma convenção para clientes novos.
- **DTOs de integração** — em arquivo `*Dto.cs` (Evolution) vs. classes inline no cliente (Asaas). Padronizar.

---

### Fontes
Documento derivado da análise direta dos clientes de integração (`EvolutionApiService`, `AsaasBillingClient`, `AsaasPaymentService`), serviços de orquestração (`BillingService`, `DoacoesService`), canais de comunicação (`ComunicacaoCanalProviders`), serviços de infra (`SmtpEmailService`, `S3FileStorageService`, `KidsPushNotificationService`), schedulers (`MessageSchedulerService`, `BillingSchedulerService`), controllers de webhook, classes de `Configuration`, health checks, `Program.cs` (API e Worker) e `appsettings.json` em `BackEnd/`, cruzado com [.claude/PROJECT_CONTEXT.md](.claude/PROJECT_CONTEXT.md) e [.claude/CODING_STANDARDS.md](.claude/CODING_STANDARDS.md).
</content>
</invoke>
