# ARCHITECTURE.md

> **Referência arquitetural oficial do projeto AppIgreja / VerboPlus (Verbo+).**
>
> Este documento descreve a **arquitetura REAL e efetivamente implementada**, derivada exclusivamente do código existente, da estrutura da solução, das dependências entre projetos e dos documentos canônicos do repositório (`.claude/PROJECT_CONTEXT.md`, `.claude/CODING_STANDARDS.md`, `.claude/INTEGRATION_PATTERNS.md`, `.claude/MIGRATION_RULES.md`).
>
> Regras deste documento:
> - **Nada é inventado.** Não há propostas de melhoria nem boas práticas genéricas.
> - **Fatos verificados** têm referência de arquivo/pasta sempre que possível.
> - **Inconsistências** e **coexistência de padrões** são registradas explicitamente, identificando o predominante.
> - Onde não há evidência suficiente: `TODO: confirmar com o time`.
> - Escopo arquitetural central: **backend .NET** (`BackEnd/`). Os clientes (frontends e app mobile) são documentados como módulos consumidores da API.
> - Última análise: **2026-06-27**.

---

## Visão Arquitetural

### Estilo arquitetural predominante
- **Backend: Clean Architecture em 4 camadas** (`Domain` → `Application` → `Infrastructure` → `API`) + um **Worker** separado (`SistemaIgreja.BackgroundWorker`) e um projeto de **testes**. Verificado pelos `ProjectReference` dos `.csproj` (ver *Fluxo de Dependências*).
- **Multi-tenant tenant-per-row** em todo o backend: toda entidade de negócio implementa `ITenantEntity` e é filtrada por um *global query filter* por `TenantId`, com carimbo automático de `TenantId` no `SaveChanges`.
- **Polirrepositório de clientes consumindo uma única API**: os frontends web (admin, portal, landing), o app mobile (Flutter) e o formulário público consomem a mesma API .NET. Não há microsserviços — o backend é um **monólito modular** (modular monolith) por domínio dentro da mesma solução.

### Principais objetivos aparentes da arquitetura
- **Testabilidade e separação de responsabilidades** (domínio isolado, sem dependência de infra; ~209 arquivos de teste citados).
- **Isolamento de dados por igreja (tenant)** com baixo custo operacional (1 banco, *rede de segurança* em duas camadas contra vazamento entre tenants).
- **Operação config-driven com a mesma imagem em todos os ambientes**: comportamento (provider de banco, integrações ligadas/desligadas) escolhido por configuração/env var, não por branch de código.
- **Resiliência controlada de integrações**: integração externa nunca derruba o fluxo de negócio (kill-switch + *result objects*).

### Princípios aparentes adotados pela equipe
- **Domínio em Português, infraestrutura/técnico em Inglês** (regra forte e consistente).
- **Mapeamento manual de DTOs (sem AutoMapper)**, **`System.Text.Json` exclusivo**, **sem Polly**, **sem broker de mensageria**, **sem cache distribuído** — decisões de simplicidade observadas por ausência consistente.
- **Segredos só em variáveis de ambiente** (pós-incidente de rotação em 2026-06-12).
- **Migração incremental com preservação de dados** (sem *big bang*; strangler no módulo de Comunicação).

---

## Estrutura da Solução

A solução .NET é `BackEnd/SistemaIgreja.sln`. O diretório raiz do repositório **não é um repositório git único** — é um conjunto de subprojetos coexistindo numa pasta, cada um com seu próprio `.git/`.

### Projetos da solução .NET (`BackEnd/`)

| Projeto | Responsabilidade | Dependências permitidas (regra de camada) | Dependências observadas (`ProjectReference`) | Tecnologias |
|---|---|---|---|---|
| **SistemaIgreja.Domain** | Entidades de negócio (77) + `ITenantEntity`. Modelo puro, zero infra. | Nenhuma. | **Nenhuma** ✓ | .NET 10, C#, Data Annotations |
| **SistemaIgreja.Application** | DTOs (~278), Interfaces (`I{X}Service` **e** `I{X}Repository`, ~77), Services (~83, casos de uso), Configuration (`{X}Settings`), Security (JWT/PasswordPolicy), Utils, JsonConverters. **Clientes HTTP de integração vivem aqui.** | Domain. | **Domain** ✓ | .NET 10, `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`, `SixLabors.ImageSharp`, `Microsoft.Extensions.Http`/`Options` |
| **SistemaIgreja.Infrastructure** | `SistemaIgrejaDbContext`, Repositories (~59), Migrations (40+), `UnitOfWork`, `AuditSaveChangesInterceptor`, Services de infra (Billing, schedulers, SMTP, S3, Audit), Resources. | Domain + Application. | **Domain + Application** ✓ | EF Core 9 (Npgsql / SqlServer / Sqlite), `AWSSDK.S3`, `Microsoft.Extensions.Hosting.Abstractions` |
| **SistemaIgreja.API** | Controllers REST (~64), Middleware, Permissions (RBAC), Swagger, Health checks, `KidsPushNotificationService` (Firebase). | Application + Infrastructure. | **Application + Infrastructure** ✓ | ASP.NET Core, `JwtBearer` 8.0.17, `Swashbuckle` 6.6.2, `Sentry.AspNetCore` 6.6.0, `FirebaseAdmin` 3.2.0, `EFCore.Design` |
| **SistemaIgreja.BackgroundWorker** | Host genérico standalone com os 4 schedulers (jobs agendados fora do processo da API). Fica **fora de `src/`**. | Application + Infrastructure. | **Application + Infrastructure** ✓ | `Microsoft.Extensions.Hosting` 10.0.0, `Microsoft.Extensions.Http`, `Sentry.Extensions.Logging` 6.6.0 |
| **tests/SistemaIgreja.API.Tests** | Testes unitários/integração (espelha `Controllers/`, `Services/`). | API + Application + Domain. | **API + Application + Domain** ✓ | `net10.0`, xUnit 2.9.2, Moq 4.20.72, FluentAssertions 8.6.0, SQLite in-memory |

> **Verificação de framework:** todos os `.csproj` apontam `net10.0`, **inclusive o projeto de testes**. O `Dockerfile` do Worker comenta evitar restaurar `tests` "que está em .NET 9" — divergência registrada como pendência. `TODO: confirmar com o time`.

### Módulos do polirrepositório (fora da solução .NET)

| Módulo | Pasta | Tipo | Consome a API? |
|---|---|---|---|
| **FrontEnd** (admin "Verbo+") | `FrontEnd/` | SPA React 19 + Vite 6 (pnpm) | Sim (JWT + headers de tenant) |
| **Portal** (site público) | `Portal/` | SPA React 18 + Vite 5 | Sim (endpoints públicos do site) |
| **AppKids** (app do responsável) | `AppKids/` | Flutter (Dart 3.2+) | Sim (`/api/auth`, `/api/kids/**`) + Firebase FCM |
| **VerboPlus** (landing marketing) | `VerboPlus/` | React 19 + Vite 6 + Tailwind 3.4 | **Não** (marketing puro; CTA WhatsApp / `/signup`) |
| **CadastroMembro** (form público) | `CadastroMembro/` | HTML/CSS/JS vanilla | Sim (`POST /api/Membros/cadastro`) |
| **evolution-api** | `evolution-api/` | Apenas `.env` do serviço WhatsApp de terceiros | n/a |
| **legal** | `legal/` | Termos de Uso + Política de Privacidade (Markdown v1) | n/a |

---

## Camadas Arquiteturais

Camadas **realmente encontradas** no backend (do núcleo para a borda):

1. **Domain** — entidades de negócio (Português) + interface `ITenantEntity`. **Sem classe base**; entidades implementam interface, não herdam. PK sempre `int Id` (sem `Guid`). Validação por Data Annotations. **Não há value objects formais** (status são enums). Sem dependências internas.
2. **Application** — casos de uso e regras de negócio (`{X}Service : I{X}Service`), contratos (interfaces de serviço **e de repositório**), DTOs (`class`, nunca `record`), configuração tipada (`{X}Settings`), segurança (JWT/`PasswordPolicy`), utilitários. **Os clientes HTTP de integração (Evolution, Asaas) moram aqui**, não em Infrastructure.
3. **Infrastructure** — persistência (EF Core, `DbContext`, Repositories, Migrations, `UnitOfWork`, interceptor de auditoria) e integrações que tocam SDK pesado ou o banco (S3, SMTP, Billing, schedulers).
4. **API (Presentation)** — superfície HTTP (controllers), autenticação/autorização, middlewares (gating de assinatura, permissões RBAC), Swagger, health checks. Integração exclusiva da API: push Firebase (`KidsPushNotificationService`).
5. **Worker (Background)** — host genérico standalone com os schedulers; mesma `Application`/`Infrastructure`, processo separado.

> **Detalhe estrutural importante:** as **interfaces de repositório `I{X}Repository` vivem em `Application/Interfaces/`** (junto com as de serviço); só a **implementação** fica em `Infrastructure/Repositories/`. Vale para todos os ~59 repositórios. Isso mantém a inversão de dependência: Application define o contrato, Infrastructure implementa.

---

## Fluxo de Dependências

Fluxo de dependências entre assemblies (verificado nos `.csproj`):

```
Domain   (sem dependências internas)
   ▲
   │
Application  ──► Domain
   ▲   ▲
   │   │
Infrastructure ──► Domain + Application
   ▲                      ▲
   │                      │
  API ──► Application + Infrastructure
   ▲
   │
BackgroundWorker ──► Application + Infrastructure
tests ──► API + Application + Domain
```

Fluxo arquitetural **predominante** de uma operação de negócio:

```
Controller (API)
  → I{X}Service (Application)        // regra de negócio + mapeamento manual DTO↔entidade
      → I{X}Repository (Application/Interfaces) ─► {X}Repository (Infrastructure)
          → SistemaIgrejaDbContext (EF Core)
              → Banco de Dados (PostgreSQL)
```

Observações:
- **Inversão de dependência respeitada:** o service depende da **interface** do repositório (em Application); a implementação concreta (Infrastructure) é injetada via DI.
- **`IUnitOfWork`** entra **apenas em operações multi-passo/transacionais**; CRUD simples persiste direto via repositório (`SaveChanges` no próprio repo).
- **Controllers não acessam `DbContext` nem EF Core** — só `I{X}Service` (ocasionalmente `I{X}Repository` para resolver IDs, ex. `IUsuarioRepository`).

---

## Fluxo de Requisições

Caminho típico de uma requisição HTTP autenticada (ordem exata do pipeline em `Program.cs`):

```
Requisição HTTP
  → UseSentry
  → CORS custom → UseRouting → UseCors
  → Swagger (apenas Development)
  → UseStaticFiles            // uploads/wwwroot em disco
  → UseRateLimiter            // políticas "signup" (5/min/IP) e "login" (10/min/IP)
  → UseAuthentication         // JWT Bearer HS256, ClockSkew=0, exp 1h
  → UseAuthorization
  → SubscriptionGatingMiddleware   // 402 se assinatura suspensa (isenta /api/auth, /api/upload, /api/webhooks, /api/billing)
  → PermissionMiddleware           // RBAC: path→recurso, método→ação; IsPlatformAdmin faz bypass; 403 sem corpo
  → MapHealthChecks (/health)
  → MapControllers
      → {Entidade}Controller (async ActionResult<T>)
          → try/catch traduzindo exceção semântica → status HTTP, corpo { message }
          → I{X}Service → I{X}Repository → DbContext → Banco
      → resposta (200/201/204 | 400/401/403/404/409 | 402 | 500)
```

Notas observadas:
- **Não há middleware global de exceção.** A tradução exceção→HTTP é feita **em cada action** com `try/catch`. Sentry captura o que escapa. `TODO: confirmar com o time` se a ausência é definitiva.
- **Corpo de erro sempre `{ message }`** (objeto anônimo), nunca string crua — o frontend depende disso.
- **Mapeamento de exceções:** `ArgumentException`→400, `KeyNotFoundException`→404, `UnauthorizedAccessException`→401/403, `InvalidOperationException`→400/409.

---

## Fluxo de Integrações Externas

### Inventário (verificado)

| Integração | Tipo | Transporte | Cliente | Onde roda |
|---|---|---|---|---|
| **Evolution API** (WhatsApp) | REST de terceiro | `HttpClient` typed | `EvolutionApiService` (Application) | API + Worker |
| **Asaas — Billing da plataforma** | REST (assinaturas) | `HttpClient` typed | `AsaasBillingClient` (Application) | API (+ ciclo no Worker) |
| **Asaas — Doações (PIX por tenant)** | REST (cobrança avulsa) | `HttpClient` typed | `AsaasPaymentService` (Application) | API |
| **SMTP / E-mail** | SMTP | `System.Net.Mail.SmtpClient` | `SmtpEmailService` (Infrastructure) | API + Worker |
| **Firebase Cloud Messaging** | Push | SDK `FirebaseAdmin` | `KidsPushNotificationService` (API) | **API apenas** |
| **AWS S3** (storage opcional) | Object storage | SDK `AWSSDK.S3` | `S3FileStorageService` (Infrastructure) | API (Singleton) |
| **Sentry** | Observabilidade | SDK `Sentry.*` | `Program.cs` | API + Worker |

**Não existem:** SOAP, gRPC, ETL com ferramenta dedicada, SDK REST gerado, broker de mensageria, OAuth1/OAuth2, mTLS, paginação por cursor de provedor externo.

### Componentes de uma integração (na ordem em que aparecem no código)
1. **Config tipada** `Application/Configuration/{X}Settings.cs` (`const SectionName`, defaults, secrets `string.Empty`).
2. **Seção em `appsettings.json`** com a mesma `SectionName` e **secrets vazios** (valor real vem de env var no Coolify).
3. **Interface** `I{X}Service`/`I{X}Client` (no mesmo arquivo da impl. — clientes Asaas — ou em `Application/Interfaces/` — Evolution).
4. **DTOs** request (objeto anônimo) + response (`class` + `[JsonPropertyName]`).
5. **Result objects** `{X}Result { Success, ErrorMessage }` — **integração não lança para o chamador** (exceto SMTP).
6. **Cliente HTTP** recebendo `HttpClient` + `IOptions<{X}Settings>` + `ILogger<T>`.
7. **Registro DI** `AddHttpClient<I,Impl>()` + `Configure<{X}Settings>(...)`.
8. **Health check** *(crítica)* `{X}ConfigurationHealthCheck : IHealthCheck` (só valida presença de config).
9. **Provider de canal** *(comunicação)* `IComunicacaoCanalProvider`.
10. **Scheduler** *(lote)* `BackgroundService` no Worker.
11. **Controller de webhook** *(entrada)* `[AllowAnonymous]` com rota absoluta.

### Fluxos
- **Saída síncrona (ex. doação PIX):** Service de negócio → resolve config/secret (cifrado por tenant via `IDataProtector`) → cliente typed → POST → desserializa → result object → persiste em entidade (`DoacaoOnline`) → logs.
- **Saída em lote (WhatsApp agendado):** Scheduler (`BackgroundService`) → loop por tenant ativo (scope DI + `TenantScopeOverride.SetTenant`) → reserva lote (`FOR UPDATE SKIP LOCKED`) → cliente typed com retry+backoff → marca item `Enviada`/`Erro` → monitor de execução → delay base + jitter.
- **Entrada (webhook):** provedor → `POST /api/webhooks/billing/asaas` (`[AllowAnonymous]`, `[FromBody] JsonElement`) → valida **token** (`StringComparison.Ordinal`) → **idempotência** (`(paymentId, evento)` em `EventoWebhookBilling`) → atualiza `Assinatura`/`Fatura` → `Ok()` | `Unauthorized()`.

Padrões transversais: **kill-switch** (no-op com credencial vazia), **retry manual com backoff só na Evolution** (5xx/429; Asaas não retenta), **correlação por colunas `Gateway*`/`External*`**, **sem dead-letter queue** (estado de erro fica na própria entidade). Detalhes completos em `.claude/INTEGRATION_PATTERNS.md`.

---

## Organização dos Projetos

- **Separação lógica por camada** dentro da solução .NET (Clean Architecture); **separação por domínio** dentro de cada camada (subpastas em `DTOs/`, `Services/`, `Repositories/`, `Controllers/`).
- **Limites entre módulos do polirrepositório:** cada cliente (admin/portal/landing/app/form) tem repositório, build e pipeline próprios; comunicam-se com o backend **apenas via API REST** (não há código compartilhado entre frontends e backend além do contrato HTTP).
- **Comunicação entre componentes do backend:** in-process via DI (interfaces). **Não há comunicação inter-processo** entre API e Worker — ambos compartilham o **mesmo banco** e o mesmo código (`Application`/`Infrastructure`), mas rodam isolados.
- **Domínios funcionais** (Pessoas, Voluntariado/Escalas, Eventos, Kids, Financeiro, Patrimônio, Comunicação, Portal, SaaS/Billing, Segurança/LGPD) são **módulos lógicos dentro do mesmo monólito**, não assemblies separados.

---

## Estratégia de Injeção de Dependência

- **Container:** o `Microsoft.Extensions.DependencyInjection` nativo do .NET (sem Autofac/outros).
- **Padrão de registro:** **inline em cada `Program.cs`**, agrupado por categoria (Repositories → Services → `Configure<T>` → HttpClients → Storage). **Não há módulo de DI compartilhado** entre API (~152 serviços) e Worker (~40) — duplicação consciente.
- **Ciclos de vida observados:**
  - `AddScoped` — repositórios e services (padrão).
  - `AddSingleton` — componentes de processo (`IFileStorageService`/storage, `ISchedulerExecutionMonitor`).
  - `AddHttpClient<I, Impl>` — clientes HTTP typed.
  - `Configure<T>` — options (`{X}Settings`).
  - `AddHostedService` — schedulers (no Worker).
- **Convenções:**
  - **Múltiplas implementações da mesma interface** resolvidas por `IEnumerable<T>` — `IComunicacaoCanalProvider` (canais de comunicação). O **canal Push só é registrado na API** (depende de Firebase).
  - **Seleção de implementação por config:** `Storage:Provider` decide `S3FileStorageService` vs `LocalFileStorageService`; `Database:Provider` decide o provider EF.
  - **`ValidateOnBuild`** no Worker — *drift* de DI quebra no startup (mitigação contra o registro duplicado divergir).
  - **Guard de startup:** recusa subir com `Jwt:Key` vazia ou placeholder.
  - **Construtor dual** em repositórios/services com tenant (um sem `ITenantContext` delegando a `new DefaultTenantContext()`) — para compatibilidade/testes.

---

## Estratégia de Persistência

- **ORM:** **EF Core 9, Code First**. Provider escolhido em runtime por `Database:Provider`: **PostgreSQL** (Npgsql, produção, com `EnableRetryOnFailure`), **SQL Server** (alternativo, mantido por coexistência), **SQLite** (testes).
- **Repositories:** padrão `{X}Repository : I{X}Repository` (interface em Application). **Não há `Repository<T>` genérico** — cada repositório é autocontido, recebe `SistemaIgrejaDbContext` (direto, nunca abstraído) + `ITenantContext`.
- **Unit of Work:** `IUnitOfWork` é **facilitador de transação**, não agregador de repositórios (`BeginTransactionAsync`, `CommitTransactionAsync`, `ExecuteInTransactionAsync`, etc.), com `CreateExecutionStrategy()` para resiliência.
- **Procedures:** **nenhuma** (zero `CREATE PROCEDURE`/`EXEC`).
- **Queries customizadas / SQL bruto:** quase tudo é LINQ. **Único `FromSqlRaw`** é a reserva concorrente de mensagens, **ramificada por provider**: `FOR UPDATE SKIP LOCKED` (PostgreSQL) / `WITH (UPDLOCK, ROWLOCK)` (SQL Server) em `MensagemAgendadaRepository`. SQL cru é sempre parametrizado (`{0}`), nunca concatenado.
- **Bulk operations:** **nenhuma** dedicada (sem `EFCore.BulkExtensions`); inserts via `Add`/`AddRange` + `SaveChangesAsync`. `TODO: confirmar com o time`.
- **Paginação:** server-side, entrada `[FromQuery] {X}PagedQueryDto`, default **20**, teto **200**, retorno em **tupla `(Items, Total)`** do repositório, convertida em `PagedResultDto<T>` no service. Ordenação dinâmica por `switch` case-insensitive.
- **Multi-tenancy (rede de segurança em duas camadas):**
  1. **Global query filter** aplicado por reflexão a toda `ITenantEntity` no `OnModelCreating` (~linha 2246 do `SistemaIgrejaDbContext`).
  2. **Carimbo de `TenantId` no `SaveChanges`/`SaveChangesAsync`**: `ITenantEntity` `Added` com `TenantId == 0` recebe `CurrentTenantId`. Vale para API **e** Worker.
  - `CurrentTenantId` vem de `ITenantContext` (`HttpTenantContext` na API; `TenantScopeOverride` no Worker). `IgnoreTenantFilters` permite lookups cross-tenant pontuais (ex. billing de plataforma).
- **Auditoria:** `AuditSaveChangesInterceptor` grava `AuditLog` (`EntityName`, `EntityId`, `Action`, usuário/IP, `ChangesJson`), com proteção contra recursão.
- **Migrations:** 40+ em `Infrastructure/Migrations/`, nomenclatura `{timestamp}_{NomeEmPortuguês}`; `Database:RunMigrations=true` aplica no startup. Backfills idempotentes (`WHERE "TenantId" = 0`), DDL bruto com `IF NOT EXISTS`/`ON CONFLICT DO NOTHING`, `Down()` reversível. Detalhes em `.claude/MIGRATION_RULES.md`.
- **Seed:** `HasData` aparece em migrations antigas (financeiro); recentes são schema-only. `TODO: confirmar com o time` onde vive o seed de planos/permissões.

---

## Estratégia de Configuração

- **`appsettings.json`** com uma **seção por área**: `ConnectionStrings`, `Database`, `Jwt`, `LoginLockout`, `Sentry`, `EvolutionApi`, `Email`, `Billing` (+`Asaas`), `Storage` (+`S3`), `Firebase`, `MessageScheduler`, `BirthdayCampaignScheduler`, `PublicAppUrl`, `Uploads`.
- **Secrets sempre vazios no arquivo** (`"ApiKey": ""`, `"Password": ""`, `"Dsn": ""`); defaults não-secretos podem vir preenchidos (`BaseUrl`, `Port`, `InstanceName`).
- **Secrets reais vêm de variáveis de ambiente no Coolify**, com override pela convenção `__`: `Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Billing__Asaas__ApiKey`, `Sentry__Dsn`, `Firebase__CredentialsJson`, `Email__Password`.
- **Secrets por tenant** (doações) ficam **cifrados no banco** via `IDataProtector` (não em env var), com últimos dígitos mascarados.
- **Classes de configuração:** `Application/Configuration/{X}Settings.cs` com `public const string SectionName` (pode ser aninhado, ex. `"Billing:Asaas"`), defaults sensatos, secrets `= string.Empty`. Seleção sandbox/produção dentro da própria config (`IsProduction => BaseUrl`).
- **Configuração de serviços:** `builder.Services.Configure<{X}Settings>(builder.Configuration.GetSection(...))` em cada `Program.cs`.
- **Seleção de comportamento por configuração, não por branch:** provider de banco, storage, e kill-switch de integrações — a mesma imagem roda em todos os ambientes.
- **`appsettings.json` é excluído de commits** durante migrações (`git reset HEAD -- ...appsettings.json` no `commit_migration_postgresql.sh`).

---

## Estratégia de Background Processing

- **Mecanismo único:** `BackgroundService` (não `IHostedService` cru, não `System.Timers`, **não cron externo, não fila/broker**).
- **Schedulers (4)** registrados como `AddHostedService` **no Worker** (`BackgroundWorker/Program.cs`): incluem `MessageSchedulerService`, `BillingSchedulerService`, `EscalaScheduler`, `BirthdayCampaignScheduler`.
- **Loop padrão:** `while (!stoppingToken.IsCancellationRequested) { try { trabalho; RecordSuccess } catch { LogError; RecordFailure } await Task.Delay(delayComJitter, stoppingToken); }`.
- **Jitter obrigatório:** `intervalo base + Random.Shared.Next(0, JitterSecondsMax+1)` para dessincronizar instâncias.
- **Multi-tenant dentro do job:** itera tenants ativos; **por tenant** cria scope DI e seta `TenantScopeOverride.SetTenant(id, slug)` antes de resolver services scoped.
- **Monitoramento:** `ISchedulerExecutionMonitor` (Singleton) com `RecordSuccess/RecordFailure`, exposto em health check.
- **Habilitação por config:** schedulers respeitam flag `Enabled`.
- **Filas:** **não há broker.** A "fila" é a tabela `MensagemAgendada` processada por estado, com reserva concorrente via `FOR UPDATE SKIP LOCKED`. **Sem dead-letter queue** — falha vira estado `Erro` na entidade.
- **Resiliência por item:** `try/catch` por item; um item com erro não derruba o lote.
- **`Inconsistência/gap conhecido`:** `Scheduler:Enabled` pode estar ligado **na API e no Worker** simultaneamente, **sem lock distribuído** — risco de execução duplicada (mitigado parcialmente pelo `SKIP LOCKED` da fila de mensagens). `TODO: confirmar com o time` (decisão: rodar schedulers só no Worker?).

---

## Estratégia de Integração

- **REST:** todo consumo de API externa é **`HttpClient` typed via `HttpClientFactory`** (`AddHttpClient<I, Impl>()`), escrito à mão. Três variantes de construção de cliente observadas:
  - **A — config no construtor** (credencial global): `EvolutionApiService`.
  - **B — request montado por chamada** (`HttpRequestMessage`): `AsaasBillingClient`.
  - **C — reconfiguração por chamada** (credencial **por tenant**): `AsaasPaymentService` (key descriptografada da `GivingProviderConfig`).
- **SOAP / gRPC:** **ausentes.**
- **SDKs:** só onde o provedor exige — `FirebaseAdmin` (push) e `AWSSDK.S3` (storage).
- **ETLs / sincronizações em lote / full sync:** **ausentes.** A sincronização é por **estado da entidade** (processa o que está `Pendente`/`pronto`), com **polling pontual** de status (ex. `TryRefreshAsaasStatusAsync` para doações). Não há checkpoint/cursor durável por integração.
- **Serialização:** `System.Text.Json` exclusivo (`[JsonPropertyName]`; `PropertyNameCaseInsensitive` para APIs instáveis como Evolution v1/v2). **Sem Newtonsoft.**
- **Webhooks:** controller `[AllowAnonymous]` + rota absoluta + `[FromBody] JsonElement`; validação **só por token** (sem HMAC — gap conhecido); **idempotência obrigatória** (tabela de eventos ou estado da entidade).
- **Comunicação omnichannel:** adaptação via `IComunicacaoCanalProvider` (múltiplas implementações resolvidas por `IEnumerable<>`).

---

## Estratégia de Observabilidade

- **Logs:** `ILogger<T>` (Microsoft.Extensions.Logging), **logging estruturado com placeholders `{Nome}`** (nunca interpolação de string). Níveis: `LogInformation` (evento de negócio concluído), `LogWarning` (falha recuperável/validação/rejeição), `LogError(ex, ...)` (exceção, sempre com a exceção como 1º argumento). Contexto sempre inclui IDs de recurso (`{PessoaId}`, `{TenantId}`, `{StatusCode}`, `{RequestUri}`). Corpo de resposta externa é **truncado** antes de logar.
- **Erros → Sentry** (`Sentry.AspNetCore` na API, `Sentry.Extensions.Logging` no Worker, `@sentry/react` no FrontEnd admin): `MinimumEventLevel=Error`, **`SendDefaultPii=false`** (LGPD), `TracesSampleRate=0`. **Kill-switch:** DSN vazio = desligado.
- **Métricas:** **não há** stack de métricas dedicada (sem Prometheus/OpenTelemetry observado). `TODO: confirmar com o time`.
- **Tracing distribuído:** **ausente** (`TracesSampleRate=0`; sem `CorrelationId`/trace id propagado). A correlação é por IDs de domínio + `MessageId` do provedor. `TODO: confirmar com o time`.
- **Monitoramento:** **health checks** ASP.NET Core em `/health` (DB + presença de config de Evolution/Email/Push/schedulers) e `SchedulerExecutionMonitor` para execução dos jobs.
- **PII:** **proibida** em logs/Sentry.

---

## Estratégia de Segurança

- **Autenticação:** **JWT Bearer HS256** (chave simétrica), `ValidateIssuer/Audience/Lifetime/IssuerSigningKey`, `ClockSkew=0`, expiração 1h. Senhas com **BCrypt** (`BCrypt.Net-Next`).
- **Login lockout:** `LoginLockout` (5 tentativas / 15 min), campos `Usuario.TentativasLoginFalhas` / `BloqueadoAte`.
- **Rate limiting:** `AddRateLimiter` com políticas `signup` (5/min/IP) e `login` (10/min/IP). Sem proteção distribuída por IP (gap conhecido).
- **Política de senha:** centralizada em `Application.Security.PasswordPolicy` (8+ chars, maiúscula+minúscula+número); backend é a fonte, front espelha em `passwordPolicy.js`.
- **Autorização:** **RBAC próprio** — `PerfilAcesso` + `PerfilAcessoPermissao` (recurso × ação) + `PessoaPerfil` (vínculo com vigência). `PermissionMiddleware` mapeia path→recurso (prefix match em `PermissionResourceMap`) e método→ação (`GET→view`, `POST/PUT/PATCH→edit`, `DELETE→delete`); **`IsPlatformAdmin` faz bypass**; nega com **403 sem corpo**. Pula `/api/auth`, `/api/upload`, OPTIONS, não-`/api`.
- **Gating de assinatura:** `SubscriptionGatingMiddleware` bloqueia tenant suspenso com **402** (corpo `{ error, message }`); isenta `/api/auth`, `/api/upload`, `/api/webhooks`, `/api/billing`; platform admin sempre passa; *fail-open* se não há tenant.
- **Gestão de segredos:** env vars no Coolify; `appsettings` sanitizado; secrets por tenant cifrados com `IDataProtector`. Guard de startup recusa `Jwt:Key` vazia/placeholder.
- **Certificados / identidades gerenciadas / mTLS / OAuth:** **ausentes.** Integrações usam API Key em header ou Service Account de SDK.
- **MFA:** **não implementado** (adiado para pós-lançamento).
- **LGPD:** consentimento **versionado** (`ConsentimentoRegistro.VersaoDocumento`), exportação, **anonimização (não exclusão física)**, `SolicitacaoTitular`. Papéis: **Igreja = Controladora**, **VerboPlus = Operadora**.
- **Gaps de segurança conhecidos (de `SAAS_READINESS.md`):** webhook Asaas sem HMAC; uploads em disco servidos sem auth (URL previsível, possível path traversal em galerias); Swagger UI público em produção; limites de plano (`MaxUsuarios`/`MaxMembros`) existem mas **não bloqueiam**; porta 5433 do Postgres exposta; schedulers duplicados sem lock.

---

## Estratégia de Deploy

- **Backend (API + Worker):** containers **Docker** no **Coolify** (PaaS auto-hospedado). Imagens base `mcr.microsoft.com/dotnet/sdk:10.0` (build) → `aspnet`/`runtime:10.0` (runtime). O `Dockerfile` do Worker copia o repo e publica só o projeto Worker (evita restaurar `tests`).
- **FrontEnd admin:** `azure-pipelines.yml` — Node 20, **pnpm**, `pnpm run test` (**bloqueia deploy se falhar**) → `pnpm run build` → **Azure Static Web App**.
- **Portal:** Azure DevOps → `npm run build` → `staticwebapp.config.json` → `AzureStaticWebApp@0` (Node 18, triggers `main`/`master`).
- **Backend CI (`BackEnd/azure-pipelines.yml`):** trigger `main`, .NET SDK 10.x, `restore`/`build`/`publish` → artefato `app.zip`. `TODO: confirmar com o time` — relação entre esse artefato e o deploy efetivo (que parece ser via Coolify/Docker).
- **VerboPlus / CadastroMembro:** build estático (Vite / HTML), hosting estático.
- **Ambientes:** Development (API local `localhost:7000`/`127.0.0.1:5013`, admin `localhost:5174`, portal `5173`; em dev o admin aponta uploads para produção) e Production (API `https://api.kingdombr.com.br`; domínio-alvo `verboplus.com.br` / `app.verboplus.com.br`).
- **Containers no repo:** `src/SistemaIgreja.API/Dockerfile`, `SistemaIgreja.BackgroundWorker/Dockerfile`, `docker-compose.evolution.yml`.
- **Cloud providers:** Coolify (API+Worker), Azure Static Web Apps (frontends), AWS S3 (storage opcional), Postgres em host próprio (`77.37.43.5:5433`).

---

## Módulos Funcionais

Domínios lógicos (módulos dentro do monólito), por evidência de controllers/entidades:

| Módulo | Responsabilidade | Entidades-chave |
|---|---|---|
| **Pessoas / Membros / Visitantes** | Cadastro centralizado de pessoas; hub do domínio. | `Pessoa`, `TipoPessoa`, `PerfilPessoa`, `Usuario` |
| **Voluntariado e Escalas** | Equipes, escalas por ministério, modelos, indisponibilidades, trocas. | `Escala`, `EscalaItem`, `EscalaModelo`, `IndisponibilidadeVoluntario`, `SolicitacaoTrocaEscala` |
| **Eventos** | Eventos, ocorrências, recorrências, inscrições. | `Evento`, `EventoOcorrencia`, `EventoRecorrencia`, `InscricaoEvento` |
| **Kids** | Turmas, salas, check-in/checkout, pré-check-in, retirada segura (token/PIN), ocorrências, conteúdo de aula, push, device tokens. | `KidsTurma`, `KidsSala`, `KidsCheckin`, `KidsPreCheckin`, `KidsOcorrencia`, `KidsConteudoAula`, `KidsNotificacao`, `KidsDeviceToken`, `CriancaDetalhe`, `ResponsavelCrianca` |
| **Financeiro** | Receitas, despesas, categorias, centros de custo, contas, fornecedores, orçamento, doações online, dashboard/relatórios. | `Receita`, `Despesa`, `CategoriaReceita/Despesa`, `CentroCusto`, `ContaBancaria`, `Fornecedor`, `OrcamentoCategoria`, `DoacaoOnline`, `GivingProviderConfig` |
| **Patrimônio** | Itens e movimentações. | `PatrimonioItem`, `PatrimonioMovimentacao`, `CategoriaPatrimonio` |
| **Comunicação (omnichannel)** | Templates, campanhas, segmentos, automações, entregas, preferências, mensagens agendadas, campanha de aniversário. *(módulo em strangler/migração)* | `Comunicacao`, `ComunicacaoTemplate/Campanha/Segmento/Automacao/Entrega/Preferencia`, `MensagemAgendada`, `ConfiguracaoCampanhaAniversario`, `NotificacaoUsuario` |
| **Portal público / Site** | Configuração do portal, destaques, notícias, galerias, enquetes, contatos, projetos, hub de casas. | `ConfiguracaoPortal`, `DestaqueSite`, `Noticia`, `GaleriaFoto`, `Enquete`, `Contato`, `Projeto`, `HubCasa` |
| **SaaS / Plataforma / Billing** | Tenants, planos, assinaturas, faturas, webhooks de billing, signup self-service, verificação de e-mail. | `Tenant`, `TenantDomain`, `Plano`, `Assinatura`, `Fatura`, `EventoWebhookBilling`, `VerificacaoEmail` |
| **Segurança / LGPD** | RBAC, auditoria, consentimentos, solicitações do titular. | `PerfilAcesso`, `PessoaPerfil`, `AuditLog`, `ConsentimentoRegistro`, `SolicitacaoTitular` |

---

## Decisões Arquiteturais Detectadas

| Decisão | Evidência encontrada | Motivação provável | Impacto arquitetural |
|---|---|---|---|
| **Clean Architecture (4 camadas)** | `ProjectReference` dos `.csproj`; pastas `Domain/Application/Infrastructure/API` | Testabilidade e separação de responsabilidades | Domínio isolado e testável; mais boilerplate; mapeamento manual de DTOs |
| **Interfaces de repositório em Application** | `Application/Interfaces/I{X}Repository.cs` + impl. em `Infrastructure/Repositories/` | Inversão de dependência | Application define contrato; Infrastructure só implementa |
| **Multi-tenancy tenant-per-row + global filter + carimbo no `SaveChanges`** | `SistemaIgrejaDbContext` (~linha 2246; `StampTenantId`) | Isolar igrejas no mesmo banco, baixo custo operacional | Rede de segurança contra vazamento; risco se entidade nova não implementar `ITenantEntity`; sem isolamento físico |
| **DTO + mapeamento manual (sem AutoMapper)** | mappers `private static MapToDto` nos services | Controle explícito, evitar mágica de reflection | Verboso, um mapper por entidade |
| **Clientes HTTP em `Application/Services`** | `EvolutionApiService`/`AsaasBillingClient` em Application; só S3/SMTP em Infrastructure | Manter cliente junto da lógica de orquestração | Infrastructure reservada a SDK pesado / acesso a banco |
| **EF Core multi-provider (PG/SqlServer/SQLite)** | `Infrastructure.csproj` com 3 providers; `Database:Provider` em `Program.cs` | PG em prod (fim de lock-in SQL Server); SQLite acelera testes | Diferenças de SQL entre providers a vigiar; SQL cru ramificado por provider |
| **Worker separado da API** | `SistemaIgreja.BackgroundWorker` (`AddHostedService`) | Escalar jobs independentemente | **Registro de DI duplicado** (API ~152 vs Worker ~40); mitigado por `ValidateOnBuild` |
| **Schedulers via `BackgroundService` com jitter** | 4 schedulers em `Infrastructure/Services/` | Evitar broker dedicado, simplicidade | Rodam na API **e** no Worker sem lock → risco de envio duplicado |
| **Integração config-driven com kill-switch** | `Configurado`/`Enabled` em Asaas/Email; DSN do Sentry | Mesma imagem em todos os ambientes | Esquecer de setar credencial = feature silenciosamente off |
| **Result objects nas integrações (não exceção)** | `AsaasBillingResult`, `EvolutionApiResponse` | Falha controlada, não derrubar fluxo | Chamador deve checar `Success` explicitamente |
| **Webhook validado só por token, com idempotência** | `BillingService.ProcessarWebhookAsync`; `EventoWebhookBilling` | Simplicidade; idempotência protege reprocessamento | **Sem HMAC** (gap de hardening) |
| **Segredos só em env vars (Coolify)** | `appsettings` com secrets vazios; `commit_migration_postgresql.sh` exclui o arquivo | Pós-incidente de segredos no git (rotação 2026-06-12) | Depende de gestão correta no Coolify |
| **Migração incremental (strangler) + preservação de dados** | `RefatoracaoPessoaCentralizada`, `AdicionarTenantId...`, `COMUNICACAO_SPRINT1_MAPA_LEGADO.md` | Evitar *big bang*; manter dados | Coexistência temporária de legado; migrations idempotentes/reversíveis |
| **Frontends separados consumindo 1 API** | Pastas `FrontEnd/Portal/VerboPlus/CadastroMembro/AppKids` | Públicos e ciclos de deploy distintos | Múltiplos repos/pipelines a manter |

---

## Restrições Arquiteturais

Restrições **encontradas no código/convenções** (não propostas):

- **Toda nova entidade de negócio DEVE implementar `ITenantEntity`** (`int TenantId`), sob pena de vazar dados entre igrejas. Globais são exceção consciente e curta: `Tenant`, `TenantDomain`, `Plano`, `EventoWebhookBilling`, `VerificacaoEmail`.
- **Controllers não podem acessar `DbContext`/EF Core/clientes HTTP diretamente** — só `I{X}Service` (e ocasionalmente `I{X}Repository`). Acesso a dados sempre via repository.
- **Services não acessam `DbContext` direto** (vai pelo repository) nem `HttpContext` (vem via `ICurrentUserContext`). *Exceção:* services de orquestração em **Infrastructure** (ex. `BillingService`) usam `DbContext` diretamente.
- **Domain não depende de nada interno** (sem referência a Application/Infrastructure).
- **Segredos nunca em `appsettings`/git** — somente env vars; secrets por tenant cifrados.
- **Domínio sempre em Português**; técnico/infra em Inglês.
- **Mapeamento de DTO é manual** — **AutoMapper proibido por convenção**.
- **Serialização de integração é `System.Text.Json`** — Newtonsoft evitado.
- **Sem `record` para DTOs/entidades** — sempre `class`. **Sem classe base de entidade / `Repository<T>` genérico.**
- **PII proibida em logs/Sentry** (`SendDefaultPii=false`).
- **SQL cru** só quando o ORM não expressa, **parametrizado** e **ramificado por provider**.
- **FrontEnd admin usa pnpm**; CI bloqueia deploy se testes falharem.
- **Tecnologias aparentemente fora de escopo** (por ausência consistente): Redis/cache distribuído, broker de mensageria, AutoMapper, Polly, MFA, geração de PDF server-side. `TODO: confirmar com o time` se são proibidas ou apenas não adotadas.

---

## Inconsistências Arquiteturais Detectadas

Registradas **sem propor correção**:

1. **Nomenclatura de DTOs coexistente:** `Criar{X}Dto`/`Atualizar{X}Dto` (legado, **predominante**) vs. `Create{X}Request`/`{X}Response` (recente). Há comentário `// DTOs legados mantidos para compatibilidade`.
2. **Local da interface do cliente de integração:** `IEvolutionApiService` em `Application/Interfaces/` vs. `IAsaasBillingClient` no mesmo arquivo da implementação.
3. **DTOs de integração:** em arquivo `*Dto.cs` (Evolution) vs. classes inline no cliente (Asaas).
4. **Ordem de parâmetros do construtor de repositório:** alguns `DbContext` primeiro, outros `ITenantContext` primeiro (predominante: `DbContext` primeiro).
5. **Atribuição de tenant na criação:** `await ResolveTenantIdAsync()` (só `PessoaRepository`) vs. `_tenantContext.TenantId ?? Tenant.InitialTenantId` (predominante, ~57/59).
6. **`DateTime.Now` vs `DateTime.UtcNow`:** entidades/services antigos usam `Now`; recentes (`AuthService`, `KidsRetiradaService`, `Tenant`, `Plano`) usam `UtcNow`. Em transição; recomendado `UtcNow` para código novo.
7. **`CancellationToken`:** usado só em integrações HTTP; ausente no resto da stack.
8. **Corpo de `NotFound`:** `NotFound()` vazio (predominante) vs. `NotFound(new { message })` (Kids).
9. **`Forbid()`/`Unauthorized()` vs `StatusCode(4xx, new { message })`** (predominante: o segundo).
10. **Respostas com objeto anônimo** (`new { url, ... }`, `new { processadas }`) em vez de DTO em algumas actions.
11. **Tratamento de erro de integração:** *result object* (predominante) vs. exceção propagada (`SmtpEmailService`) vs. *swallow + log* (`KidsPushNotificationService`).
12. **Schedulers podem rodar na API e no Worker** (`Scheduler:Enabled` em ambos) **sem lock distribuído** — coexistência de dispatch.
13. **Webhook sem token configurado = aceito** (validação só roda se há token).
14. **Registro de DI duplicado** entre API e Worker (sem módulo compartilhado) — *drift* já causou bug, mitigado por `ValidateOnBuild`.
15. **Divergência de nomes de rota Portal × API:** a doc do Portal cita endpoints em inglês (`/api/events`, `/api/church/info`) enquanto o backend usa controllers em português. `TODO: confirmar com o time` o mapeamento real.
16. **Framework do projeto de testes:** `.csproj` em `net10.0`, mas comentário no `Dockerfile` do Worker menciona `.NET 9`.

---

## Checklist para Novos Módulos

Baseado nos padrões encontrados:

- [ ] O módulo é um **domínio lógico dentro do monólito** (subpastas em cada camada), **não** um novo assembly/microsserviço.
- [ ] Entidades em `Domain/Entities/` implementando `ITenantEntity` (salvo globais justificados).
- [ ] Interfaces (`I{X}Service`, `I{X}Repository`) em `Application/Interfaces/`; implementações nas camadas corretas.
- [ ] DTOs por domínio em `Application/DTOs/{Dominio}/` quando houver paginação/complexidade.
- [ ] Controllers `{Entidade}Controller` em `API/Controllers/`, com recurso mapeado no `PermissionResourceMap` (RBAC).
- [ ] Registro de DI **na API e no Worker** (se houver job dependente), respeitando lifetimes (`AddScoped` para service/repo).
- [ ] Migrations idempotentes/reversíveis; índices únicos por `(TenantId, ...)`.
- [ ] Cobertura por testes (xUnit + Moq + FluentAssertions), incluindo isolamento de tenant.
- [ ] Frontend (se aplicável) em módulo `api/` + `pages/` no FrontEnd admin, com i18n (pt-BR/en-US/es-ES).
- [ ] Logs/erros via `ILogger`/Sentry, sem PII.

---

## Checklist para Novas Funcionalidades

1. **Entidade** (`Domain/Entities/`): `public int Id`, `: ITenantEntity` (`[Required] int TenantId` + `virtual Tenant`), Data Annotations. Sem classe base.
2. **DbContext**: `DbSet<>` + config no `OnModelCreating` (índice único `(TenantId, ...)`, FKs, `MaxLength`). Confirmar cobertura do global filter + carimbo de tenant.
3. **Migration**: `dotnet ef migrations add {NomeEmPortuguês}` (com env vars de `Jwt`/`Connection`). Revisar SQL gerado (PG). Backfill idempotente + `Down()` reversível.
4. **DTOs** (`Application/DTOs/{Dominio}/`): `{X}Dto`, `Criar{X}Dto`, `Atualizar{X}Dto`, `{X}PagedQueryDto`. `class` + DataAnnotations em Português. Sem regra de negócio.
5. **Repositório**: interface em `Application/Interfaces/I{X}Repository.cs` + impl. em `Infrastructure/Repositories/{X}Repository.cs` (construtor `DbContext` + `ITenantContext`, paginação tupla `(Items, Total)` teto 200, ordenação dinâmica, `AsNoTracking` em leitura paginada).
6. **Service** `{X}Service : I{X}Service`: injeta repos + `ILogger<T>` (+ `ITenantContext`/`IUnitOfWork` se preciso). Mapper `private static MapToDto`. Lança exceções semânticas. Log estruturado com placeholders, sem PII.
7. **DI**: registrar repo e service **na API e no Worker** se houver job dependente (atenção ao fechamento transitivo / `ValidateOnBuild`).
8. **Controller** `{X}Controller`: `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`, `ControllerBase`, `async ActionResult<T>`, `try/catch` traduzindo exceção→HTTP com corpo `{ message }`. `CreatedAtAction` no POST, `NoContent` no DELETE.
9. **Permissões**: mapear recurso/ação no `PermissionResourceMap`; semente em `PerfilAcesso` se necessário.
10. **Testes** (`tests/`): xUnit + Moq + FluentAssertions, `MethodName_Scenario_Expected`; teste de isolamento de tenant quando aplicável.
11. **Frontend**: módulo em `api/`, página(s) em `pages/`, shadcn/ui + `react-hook-form` + Zod, rota protegida (`ProtectedRoute`/`RequirePermission`), i18n; teste Vitest + RTL.
12. **Observabilidade**: logs/erros via `ILogger`/Sentry; sem PII.

---

## Checklist para Novas Integrações

1. **Config tipada** `Application/Configuration/{X}Settings.cs` com `public const string SectionName`, defaults e secrets `string.Empty`; seleção sandbox/produção via propriedade.
2. **Seção em `appsettings.json`** com a mesma `SectionName` e **secrets vazios**; documentar as env vars (`{Section}__{Key}`) para o Coolify.
3. **Interface + DTOs + result objects**: `I{X}Service`/`I{X}Client`; DTOs request (objeto anônimo) e response (`class` + `[JsonPropertyName]`); `{X}Result { Success, ErrorMessage }`.
4. **Cliente HTTP** typed: `AddHttpClient<I{X}, {X}Impl>()`, recebendo `HttpClient` + `IOptions<{X}Settings>` + `ILogger<T>`. Escolher variante (construtor / por-request / por-tenant). `System.Text.Json`. **Validar argumentos cedo.**
5. **Autenticação**: API Key/token em header a partir da config; nunca hardcode. Se por tenant, cifrar com `IDataProtector` e guardar mascarado.
6. **Erros**: *result object* (`Success=false`), não exceção; `try/catch (Exception)` em volta do HTTP; logar `LogWarning`/`LogError` com `{StatusCode}`/`{RequestUri}`/IDs, sem PII, truncando corpo grande.
7. **Retry/timeout**: só se justificar (modelo Evolution: loop manual + backoff exponencial, só 5xx/429); timeout explícito. **Pagamentos não retentam.** **Sem Polly.**
8. **Kill-switch**: no-op quando credencial vazia/`Enabled=false`.
9. **Registro DI**: na API; replicar **no Worker** se um scheduler usar (atenção a `ValidateOnBuild`).
10. **Health check** *(crítica)*: `{X}ConfigurationHealthCheck : IHealthCheck` validando presença de config; `AddCheck<...>`.
11. **Provider de canal** *(se comunicação)*: implementar `IComunicacaoCanalProvider` e registrar como mais uma implementação.
12. **Scheduler** *(se lote)*: `BackgroundService` com jitter + `ISchedulerExecutionMonitor` + scope por tenant; registrar no Worker.
13. **Webhook** *(se entrada)*: controller `[AllowAnonymous]` com rota absoluta, `[FromBody] JsonElement`, validação por token (`Ordinal`), **idempotência** (tabela de eventos ou estado da entidade), `Ok()`/`Unauthorized()`. Isentar nos middlewares (prefixo `/api/webhooks`).
14. **Persistência**: correlacionar com colunas `Gateway*`/`External*`; upsert manual por essa chave; sem bulk/procedure.
15. **Testes**: mockar o `HttpClient`/cliente; cobrir feliz, erro não-2xx, exceção e (se houver) retry.
16. **Documentar** bloqueio de produção em `SAAS_READINESS.md` se exigir conta/credencial real.

---

## Dúvidas e Pendências

- `TODO: confirmar com o time` — **Lock distribuído para schedulers:** hoje podem rodar na API **e** no Worker sem lock (só `SKIP LOCKED` da fila de mensagens cobre parcialmente). Decisão: rodar schedulers só no Worker?
- `TODO: confirmar com o time` — **Framework do projeto de testes:** `.csproj` em `net10.0`, mas `Dockerfile` do Worker menciona `.NET 9`.
- `TODO: confirmar com o time` — **Pipeline efetivo de deploy do backend:** `BackEnd/azure-pipelines.yml` gera `app.zip`, mas o deploy de produção parece ser via Coolify (Docker). Qual é o oficial?
- `TODO: confirmar com o time` — **Cache distribuído (Redis):** não há evidência no código. Existe em algum ambiente?
- `TODO: confirmar com o time` — **Métricas / tracing distribuído:** ausentes (`TracesSampleRate=0`, sem OpenTelemetry). São fora de escopo ou pendentes?
- `TODO: confirmar com o time` — **HMAC em webhooks:** validação atual é só por token; webhook sem token configurado é aceito. É intencional?
- `TODO: confirmar com o time` — **Retry/timeout dos clientes Asaas:** usam timeout default (100s) e não retentam. Manter ou padronizar com Evolution?
- `TODO: confirmar com o time` — **Divergência de nomes de rota Portal × API** (inglês na doc do Portal × português no backend). Confirmar mapeamento/aliases reais.
- `TODO: confirmar com o time` — **`DateTime.Now` vs `UtcNow`:** padronização oficial?
- `TODO: confirmar com o time` — **Middleware global de exceção:** a ausência (tradução por action) é decisão definitiva ou lacuna?
- `TODO: confirmar com o time` — **Exceptions de domínio / `CancellationToken` / `*WithoutSaveAsync` / estratégia de `Include`:** convenções não fixadas (ver `.claude/CODING_STANDARDS.md` §16).
- `TODO: confirmar com o time` — **Seed de dados** (planos, permissões): onde vive? `HasData` antigo vs. inicialização em runtime.
- `TODO: confirmar com o time` — **Bulk insert / paralelismo:** ausentes; proibidos ou apenas não adotados?
- `TODO: confirmar com o time` — **Sistema legado de origem (Kingdom):** existiu sistema anterior (procedures/DataSets/Crystal/.NET Framework) migrado para o estado atual? Nada disso está no repo.
- `TODO: confirmar com o time` — **Geração de PDF server-side:** nenhuma biblioteca adotada; relatórios são JSON renderizados no cliente.
- `TODO: confirmar com o time` — **Versionamento de API:** não há `/v1/`; compatibilidade mantida só por schema/DTO.
- `TODO: confirmar com o time` — **Estado da migração do módulo de Comunicação (strangler):** quanto do domínio central já substituiu o legado em produção?
- `TODO: confirmar com o time` — **Plataformas-alvo oficiais do AppKids** (memória indica só iOS/macOS, sem Android publicado).

---

### Fontes
Documento derivado da análise direta da solução `BackEnd/SistemaIgreja.sln` (verificação de `.csproj`, `ProjectReference`, `PackageReference` e `TargetFramework`) e da estrutura do polirrepositório, cruzada com os documentos canônicos do projeto:
- [.claude/PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) — visão de arquitetura, negócio e stack.
- [.claude/CODING_STANDARDS.md](CODING_STANDARDS.md) — padrões reais de código do backend.
- [.claude/INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md) — padrões reais de integração.
- [.claude/MIGRATION_RULES.md](MIGRATION_RULES.md) — regras reais de migração.
- [SAAS_READINESS.md](../SAAS_READINESS.md) — bloqueadores e gaps de produção.
</content>
</invoke>
