# DECISIONS.md — Histórico de Decisões Arquiteturais e Técnicas

> **Projeto:** AppIgreja / Verbo+ (VerboPlus) — Sistema de Gestão de Igrejas (ChMS) vendido como SaaS multi-tenant.
> **Última reconstrução desta análise:** 2026-06-27.

---

## Introdução

Este documento reconstrói, **a partir das evidências encontradas no código, dependências, estrutura da
solução e documentação canônica do repositório**, as decisões técnicas e arquiteturais que provavelmente
levaram o projeto ao seu estado atual. O objetivo é responder não apenas *como* o sistema funciona, mas
*por que* determinadas escolhas foram tomadas, servindo como memória de longo prazo para futuros
desenvolvedores, agentes e automações.

**Como usar este documento**
- Ele descreve decisões **já materializadas no código**, não um roadmap nem um conjunto de recomendações.
- Cada decisão traz: a decisão, as evidências, a motivação provável, os impactos positivos e os trade-offs.
- A última seção lista as decisões cuja motivação **não pôde ser confirmada** com segurança.

**Convenção de confiabilidade usada em todo o documento**
- **[FATO]** — observado diretamente no código, dependências, estrutura ou documentação canônica.
- **[INFERÊNCIA]** — provável a partir das evidências, mas não declarado explicitamente.
- **[HIPÓTESE]** — plausível, porém sem evidência suficiente; requer confirmação.
- **`TODO: confirmar motivação com o time`** — usado quando a *motivação* não pode ser determinada.

**Fontes utilizadas** (exclusivamente):
código existente; estrutura da solução; dependências observadas (`*.csproj`, `package.json`,
`pubspec.yaml`); `.claude/PROJECT_CONTEXT.md`; `.claude/CODING_STANDARDS.md`; `.claude/ARCHITECTURE.md`;
`.claude/DOMAIN_KNOWLEDGE.md`; `.claude/INTEGRATION_PATTERNS.md`; `.claude/MIGRATION_RULES.md`;
`SAAS_READINESS.md` e demais documentos de planejamento no repositório.

**Nota estrutural importante [FATO]:** o diretório raiz **não é um repositório git único**. Cada
subprojeto (`BackEnd`, `FrontEnd`, `Portal`, `AppKids`, `VerboPlus`, `CadastroMembro`, …) possui seu
próprio `.git/`. Trata-se de um conjunto de repositórios coexistindo numa mesma pasta, não de um monorepo
formal. Por isso, parte da cronologia é reconstruída a partir de *timestamps* de migrations EF Core,
scripts SQL versionados e mensagens de commit embutidas em scripts, e não de um histórico git unificado.

---

## Decisões Arquiteturais

### 1. Clean Architecture em 4 camadas + Worker separado (backend)

**Decisão.** O backend (`BackEnd/SistemaIgreja.sln`) adota Clean Architecture com quatro camadas —
`Domain → Application → Infrastructure → API` — mais um processo `BackgroundWorker` separado e um projeto
de testes.

**Evidências [FATO]**
- `ProjectReference` nos `.csproj` confirmam a direção de dependência:
  - `Domain` → nenhuma dependência interna;
  - `Application` → `Domain`;
  - `Infrastructure` → `Domain` + `Application`;
  - `API` → `Application` + `Infrastructure`;
  - `BackgroundWorker` → `Application` + `Infrastructure`;
  - `tests/SistemaIgreja.API.Tests` → `API` + `Application` + `Domain`.
- Domain é modelo puro, **sem nenhuma dependência de infraestrutura**.
- Worker fica **fora de `src/`** (`BackEnd/SistemaIgreja.BackgroundWorker/`).

**Motivação provável [INFERÊNCIA]:** testabilidade e separação de responsabilidades (declarado nos docs
canônicos como objetivo das camadas).

**Impactos positivos:** desacoplamento; domínio isolado e testável; troca de infraestrutura sem tocar regra.

**Trade-offs aparentes:** maior complexidade inicial; mais projetos a manter.

---

### 2. Monólito modular, sem microsserviços

**Decisão.** Arquitetura de **monólito modular**: os domínios funcionais (Pessoas, Kids, Financeiro,
Comunicação, Patrimônio, Billing etc.) são módulos lógicos dentro da mesma solução, **não assemblies nem
serviços separados**.

**Evidências [FATO]:** docs canônicos afirmam "Não há microsserviços"; única solução `.sln`; um único
`SistemaIgrejaDbContext` com 130+ `DbSet`; ~77 entidades de domínio.

**Motivação provável:** `TODO: confirmar motivação com o time` (consistente com simplicidade operacional de
uma equipe pequena — [INFERÊNCIA]).

**Impactos:** deploy e operação simples (uma imagem de API, uma de Worker); coesão alta.

**Trade-offs:** escalabilidade por módulo limitada; acoplamento no mesmo banco/processo.

---

### 3. Interfaces de repositório vivem em `Application`, implementações em `Infrastructure`

**Decisão.** Todas as interfaces `I{X}Repository` (e `I{X}Service`) residem em `Application/Interfaces/`;
apenas a **implementação** dos repositórios fica em `Infrastructure/Repositories/`.

**Evidências [FATO]:** vale para os ~59 repositórios; documentado em CODING_STANDARDS e ARCHITECTURE.

**Motivação provável [INFERÊNCIA]:** manter a inversão de dependência — a Application define o contrato, a
Infrastructure implementa.

**Impactos:** Application não depende de EF Core/Infrastructure; testabilidade via mocks.

**Trade-offs:** contrato e implementação ficam em projetos diferentes (navegação dividida).

---

### 4. Multi-tenancy *tenant-per-row* com rede de segurança em duas camadas

**Decisão.** Isolamento multi-tenant por linha (um único banco), com:
1. **Global query filter** aplicado por reflexão a toda entidade `ITenantEntity` no `OnModelCreating`;
2. **Carimbo automático de `TenantId`** em `SaveChanges`/`SaveChangesAsync` (entidade `Added` com
   `TenantId == 0` recebe `CurrentTenantId`).

**Evidências [FATO]**
- `ITenantEntity { int TenantId { get; set; } }` implementado por entidades de negócio.
- `CurrentTenantId` provém de `ITenantContext` — `HttpTenantContext` (API) e `TenantScopeOverride` (Worker).
- `IgnoreTenantFilters` permite *lookups* cross-tenant (ex.: billing de plataforma).
- Entidades **globais** (exceção consciente, sem `TenantId`): `Tenant`, `TenantDomain`, `Plano`,
  `EventoWebhookBilling`, `VerificacaoEmail`.
- Teste de isolamento: `TenantQueryFilterTests`.

**Motivação provável [INFERÊNCIA]:** isolar dados de cada igreja no mesmo banco, com baixo custo
operacional.

**Impactos:** simples de operar (1 banco); proteção em duas camadas reduz vazamento entre tenants.

**Trade-offs:** sem isolamento físico; risco se uma entidade nova **não** implementar `ITenantEntity`.

---

### 5. Pessoa como hub central do domínio

**Decisão.** `Pessoa` é o registro único e central de todo indivíduo (adulto ou criança). Dela derivam
`Usuario` (1:1), `Visitante` (1:N), `Voluntario` (1:N), `PessoaPerfil` (1:N), `CriancaDetalhe` (1:1),
`ResponsavelCrianca` (N:N).

**Evidências [FATO]:** DOMAIN_KNOWLEDGE ("Pessoa é o centro do domínio"); migration
`RefatoracaoPessoaCentralizada` que consolidou `Visitante`/`Voluntario`/`Usuario` em `Pessoa`;
deduplicação por Email → WhatsApp → Telefone, preenchendo apenas campos vazios (nunca sobrescreve).

**Motivação provável [INFERÊNCIA]:** eliminar duplicação de pessoas; modelo canônico único.

**Impactos:** deduplicação consistente; relacionamentos uniformes.

**Trade-offs:** complexidade de consolidação; colisão de terminologia "Perfil" (ver Trade-offs).

---

### 6. RBAC próprio (sem framework de autorização externo)

**Decisão.** Controle de acesso baseado em papéis implementado na própria aplicação:
`PerfilAcesso` + `PerfilAcessoPermissao` (recurso × ação) + `PessoaPerfil`, aplicado por
`PermissionMiddleware` (mapeia *path*→recurso e método→ação; `GET→view`, `POST/PUT/PATCH→edit`,
`DELETE→delete`).

**Evidências [FATO]:** `PermissionResourceMap`; `IsPlatformAdmin` faz *bypass*; nega com **403 sem corpo**;
pula `/api/auth`, `/api/upload`, OPTIONS e rotas não-`/api`. Migration `AdicionarPerfisAcesso` (fev/2026).

**Motivação provável:** `TODO: confirmar motivação com o time`.

**Impactos:** granularidade recurso×ação; controle total da lógica.

**Trade-offs:** manutenção do mapa de recursos; sem padrão de mercado (policies do ASP.NET não usadas).

---

### 7. Schedulers via `BackgroundService`, sem broker de mensageria

**Decisão.** Jobs assíncronos implementados exclusivamente com `BackgroundService`
(não `IHostedService` cru, não `System.Timers`, não cron externo, não Hangfire/Quartz, não fila/broker).
Quatro schedulers registrados via `AddHostedService` **no Worker**: `MessageSchedulerService`,
`BillingSchedulerService`, `EscalaScheduler`, `BirthdayCampaignScheduler`.

**Evidências [FATO]**
- Loop padrão `while(!stoppingToken...)` com `RecordSuccess/RecordFailure` e `await Task.Delay(...)`.
- **Jitter obrigatório:** `intervalo base + Random.Shared.Next(0, JitterSecondsMax+1)`.
- A "fila" é a tabela `MensagemAgendada`, processada por estado; reserva concorrente via SQL cru
  (`FOR UPDATE SKIP LOCKED` no PostgreSQL / `WITH (UPDLOCK, ROWLOCK)` no SQL Server).
- Monitoramento via `ISchedulerExecutionMonitor` (Singleton), exposto em health check.
- **Sem dead-letter queue** — falha vira estado `Erro` na própria entidade.

**Motivação provável [INFERÊNCIA]:** evitar a complexidade de um broker dedicado; jitter para
dessincronizar instâncias.

**Impactos:** simplicidade; sem infraestrutura adicional.

**Trade-offs [FATO, gap registrado]:** schedulers podem estar habilitados **na API e no Worker
simultaneamente sem lock distribuído** → risco de execução duplicada (mitigado parcialmente por
`SKIP LOCKED`).

---

### 8. Ausência de middleware global de exceção

**Decisão.** Não há middleware global de tratamento de exceções; cada *action* traduz exceção→HTTP via
`try/catch`, e o Sentry captura o que escapa.

**Evidências [FATO]:** mapeamento recorrente `ArgumentException→400`, `KeyNotFoundException→404`,
`UnauthorizedAccessException→401/403`, `InvalidOperationException→400/409`; corpo de erro **sempre
`{ message }`** (objeto anônimo) — "o frontend depende disso".

**Motivação provável [INFERÊNCIA]:** controle explícito por endpoint.

**Impactos:** previsibilidade do contrato de erro para o frontend.

**Trade-offs:** repetição de `try/catch`; risco de divergência entre actions. (Doc marca como
`TODO: confirmar` se a ausência é definitiva — ver Decisões que Precisam de Confirmação.)

---

## Decisões Tecnológicas

### Linguagem e runtime
- **[FATO] C# / .NET 10 (`net10.0`)** em todos os `.csproj` do backend, com `Nullable enable` e
  `ImplicitUsings enable`. Motivação: `TODO: confirmar motivação com o time` (runtime moderno + null-safety
  — [INFERÊNCIA]).
- **[FATO]** Frontends web em **JavaScript/JSX (React)**; mobile em **Dart/Flutter**; formulário público de
  cadastro em **HTML/CSS/JS vanilla** (`CadastroMembro/index.html`).
- **Nota de drift [FATO]:** o `.csproj` de testes declara `net10.0`, mas um comentário no Dockerfile do
  Worker menciona ".NET 9" — divergência registrada (ver Confirmação).

### Frameworks
- **[FATO] Backend:** ASP.NET Core (Web API, SDK `Microsoft.NET.Sdk.Web`) + .NET Generic Host
  (`Microsoft.Extensions.Hosting` 10.0.0) no Worker.
- **[FATO] FrontEnd (admin) e VerboPlus (landing):** React 19 + Vite 6.
- **[FATO] Portal:** React 18 + Vite 5.
- **[FATO] AppKids:** Flutter (Dart `>=3.2.0 <4.0.0`), navegação `go_router` 13, estado `provider` 6.

### ORM e banco de dados
- **[FATO] EF Core 9 (Code First + Migrations)**, multi-provider:
  - `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.2 — **padrão/produção**;
  - `Microsoft.EntityFrameworkCore.SqlServer` 9.0.6 — alternativo;
  - `Microsoft.EntityFrameworkCore.Sqlite` 9.0.6 — **apenas em testes**.
- **[FATO]** Provider escolhido em runtime por `Database:Provider`; migrations aplicadas no startup quando
  `Database:RunMigrations = true`.
- **[FATO] Padrão Repository + Unit of Work**, mas **sem `Repository<T>` genérico** (cada repositório é
  autocontido) e com `IUnitOfWork` atuando **como facilitador de transação, não como agregador de
  repositórios** (`BeginTransaction`, `Commit`, `ExecuteInTransactionAsync`, `CreateExecutionStrategy()`).
  CRUD simples persiste direto via repositório.
- **[FATO]** Sem stored procedures (zero `CREATE PROCEDURE`/`EXEC`); SQL cru só na reserva concorrente de
  mensagens, sempre parametrizado e ramificado por provider.
- **Motivação provável:** PostgreSQL em produção (fim de lock-in), SQLite acelera testes — [INFERÊNCIA]
  (a migração para Postgres é declarada; ver Decisões de Migração).

### Bibliotecas HTTP
- **[FATO] Backend:** `HttpClient` *typed* via `HttpClientFactory` (`AddHttpClient<I, Impl>()`), escrito à
  mão; clientes `EvolutionApiService`, `AsaasBillingClient`, `AsaasPaymentService`.
- **[FATO] FrontEnd admin:** `axios` 1.10 com interceptors JWT/tenant (centralizado em `lib/apiClient.js`).
- **[FATO] Portal:** `axios` 1.6. **AppKids:** Dart `http` 1.2.

### Serialização e mapeamento
- **[FATO] `System.Text.Json` exclusivamente, nunca Newtonsoft** — `[JsonPropertyName]` em DTOs de
  resposta; `PropertyNameCaseInsensitive = true` para APIs instáveis (Evolution v1/v2).
- **[FATO] Mapeamento DTO↔entidade manual (`private static MapToDto`), sem AutoMapper** — declarado como
  proibido por convenção. Motivação declarada: controle explícito, evitar "mágica" de reflection.
- **[FATO]** DTOs sempre `class`, **nunca `record`**; validação só por DataAnnotations (sem regra de negócio
  no DTO). No frontend, validação via `react-hook-form` + Zod 3.24.

### Autenticação
- **[FATO] JWT Bearer, chave simétrica HS256**, `ClockSkew = 0`, expiração de 1h
  (`Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.17 + `System.IdentityModel.Tokens.Jwt` 8.3.1).
- **[FATO]** Hash de senha com `BCrypt.Net-Next` 4.0.3.
- **[FATO]** *Guard* de startup recusa subir com `Jwt:Key` vazia ou placeholder.
- **[FATO]** MFA **não implementado** (adiado para pós-lançamento).
- **Nota [FATO]:** as libs de auth são 8.x enquanto o target é .NET 10.

### Mensageria
- **[FATO] Sem broker** (não há RabbitMQ/Kafka/SQS). A "fila" é a tabela `MensagemAgendada` (ver decisão
  arquitetural 7).

### Observabilidade
- **[FATO] Sentry** em API (`Sentry.AspNetCore` 6.6.0), Worker (`Sentry.Extensions.Logging` 6.6.0) e
  frontend admin (`@sentry/react` 10.58): `MinimumEventLevel = Error`, **`SendDefaultPii = false` (LGPD)**,
  `TracesSampleRate = 0`; **desligado quando o DSN está vazio** (kill-switch).
- **[FATO] Logging estruturado** com `ILogger<T>` e placeholders `{Nome}` (nunca interpolação de string);
  PII proibida em logs.
- **[FATO] Health checks** ASP.NET em `/health` (DB + presença de config de Evolution/Email/Push/schedulers)
  + `SchedulerExecutionMonitor`.
- **[FATO] Sem métricas (Prometheus/OpenTelemetry) e sem tracing distribuído** (`TracesSampleRate = 0`).

### Outras bibliotecas relevantes
- **[FATO]** Processamento de imagem: `SixLabors.ImageSharp` 3.1.12.
- **[FATO]** Push (Kids): `FirebaseAdmin` 3.2.0 (servidor) + `firebase_messaging`/`firebase_core` (Flutter).
- **[FATO]** Storage opcional: `AWSSDK.S3` 3.7.x.
- **[FATO] FrontEnd admin:** shadcn/ui (estilo "new-york") sobre Radix UI, Tailwind CSS 4.1, lucide-react,
  sonner, framer-motion, recharts, cmdk, react-router-dom 7.6 (lazy routes), i18next (pt-BR/en-US/es-ES),
  gerenciado por **pnpm 10.4.1**.
- **[FATO] Estado global no frontend via Context API — sem Redux/Zustand.**
- **[FATO] Testes backend:** xUnit 2.9.2, Moq 4.20.72, FluentAssertions 8.6.0, coverlet 6.0.2; frontend
  Vitest 4 + Testing Library + jsdom.

---

## Decisões de Infraestrutura

### Cloud provider e deploy do backend
- **[FATO]** API e Worker rodam como **containers Docker no Coolify** (PaaS auto-hospedado).
- **[FATO]** Imagens base `mcr.microsoft.com/dotnet/sdk:10.0` → `aspnet`/`runtime:10.0`.
- **[FATO]** O Dockerfile do Worker copia o repositório mas **publica apenas o projeto do Worker, para
  evitar restaurar `tests`**.
- **[FATO]** Postgres de produção em host próprio `77.37.43.5:5433` (porta exposta publicamente —
  *hardening* pendente, ver Segurança).

### Deploy dos frontends
- **[FATO] FrontEnd admin:** `azure-pipelines.yml`, Node 20, pnpm, `pnpm run test` (**bloqueia deploy se os
  testes falharem**) → `pnpm run build` → **Azure Static Web Apps** (grupo de variáveis
  `SWA_AppIgreja_Admin_Variables`).
- **[FATO] Portal:** Azure DevOps → `npm run build` → `staticwebapp.config.json` → `AzureStaticWebApp@0`,
  Node 18, triggers `main`/`master`.
- **[FATO] VerboPlus / CadastroMembro:** build estático (Vite / HTML) em hosting estático.

### CI/CD do backend
- **[FATO]** `BackEnd/azure-pipelines.yml`: trigger `main`, .NET SDK 10.x, restore/build/publish → artefato
  `app.zip`.
- **[HIPÓTESE]** A relação entre esse pipeline (`app.zip`) e o deploy efetivo via Coolify/Docker não está
  clara — ver Decisões que Precisam de Confirmação.

### Secrets management
- **[FATO]** Segredos **somente em variáveis de ambiente no Coolify**; `appsettings.json` mantém os campos
  de secret **vazios**. Override via convenção `__` (ex.: `Jwt__Key`,
  `ConnectionStrings__DefaultConnection`, `Billing__Asaas__ApiKey`, `Sentry__Dsn`,
  `Firebase__CredentialsJson`, `Email__Password`).
- **[FATO]** Secrets **por tenant** (ex.: chaves de gateway de doação) são cifrados no banco via
  `IDataProtector` e armazenados mascarados (`ApiKeyUltimosDigitos`), **não** em env var.
- **[FATO]** `appsettings.json` é explicitamente excluído de commits durante migrations
  (`git reset HEAD -- ...appsettings.json` em `commit_migration_postgresql.sh`).
- **Motivação declarada [FATO]:** pós-incidente de segredos versionados no git e rotacionados em
  **2026-06-12** (ver Linha do Tempo).

### Containers e ambiente local
- **[FATO]** Dockerfiles de API e Worker; `docker-compose.evolution.yml` para a Evolution API local.
- **[FATO] Dev:** API em `localhost:7000`/`127.0.0.1:5013`, admin em `localhost:5174`, portal em `5173`.
  **Prod:** API em `https://api.kingdombr.com.br`; domínio-alvo `verboplus.com.br`/`app.verboplus.com.br`.

---

## Decisões de Integração

### Estratégia geral
- **[FATO] REST/HTTP exclusivamente** para APIs de terceiros; **não há SOAP, gRPC, ETL, SDK REST gerado nem
  broker**. Cada cliente é `HttpClient` *typed* escrito à mão.
- **[FATO] SDKs oficiais apenas onde o provedor exige:** `FirebaseAdmin` (push) e `AWSSDK.S3` (storage),
  além de `Sentry.*` (observabilidade).
- **[FATO] Kill-switch onipresente:** integração desligável por configuração; com credencial vazia ou
  `Enabled=false`, vira no-op e **não derruba o fluxo de negócio**.
- **[FATO] Result objects** `{X}Result { Success, ErrorMessage }` — a integração não lança exceção para o
  chamador (exceção: `SmtpEmailService` propaga; `KidsPushNotificationService` faz *swallow + log*).

### Serviços integrados [FATO]
| Serviço | Protocolo/SDK | Cliente | Onde roda |
|---|---|---|---|
| Evolution API (WhatsApp) | REST/HTTP (`HttpClientFactory`) | `EvolutionApiService` | API + Worker |
| Asaas — Billing da plataforma | REST/HTTP | `AsaasBillingClient` | API (ciclo no Worker) |
| Asaas — Doações PIX por tenant | REST/HTTP | `AsaasPaymentService` | API |
| SMTP / E-mail | SMTP (`System.Net.Mail.SmtpClient`) | `SmtpEmailService` | API + Worker |
| Firebase Cloud Messaging | SDK `FirebaseAdmin` | `KidsPushNotificationService` | **API apenas** |
| AWS S3 | SDK `AWSSDK.S3` | `S3FileStorageService` | API (Singleton, opcional) |
| Sentry | SDK `Sentry.*` | `Program.cs` | API + Worker |

### REST vs SOAP / SDKs adotados
- **[FATO]** REST para todos os terceiros; nenhum SOAP. SDK só onde obrigatório (Firebase, S3, Sentry).

### Autenticação às integrações
- **[FATO]** Apenas dois mecanismos: **API Key em header** (Evolution `apikey`; Asaas `access_token`) e
  **credencial de SDK** (Service Account do Firebase via `GoogleCredential.FromJson`; `BasicAWSCredentials`
  para S3). Não há OAuth1/OAuth2, JWT de terceiro, Basic Auth nem mTLS.

### Estratégia de cliente HTTP (três variantes) [FATO]
- **Variante A** — credencial global definida no construtor (`EvolutionApiService`).
- **Variante B** — request montado por chamada via `HttpRequestMessage` (`AsaasBillingClient`,
  `UserAgent "VerboPlus/1.0"`).
- **Variante C** — reconfiguração **por tenant** a cada chamada, com chave descriptografada de
  `GivingProviderConfig` (`AsaasPaymentService`).
- Regra de decisão declarada: credencial global → A/B; credencial por tenant → C.
- **[FATO]** Cliente HTTP de integração mora em **`Application/Services`** (junto da orquestração); apenas
  integrações com SDK pesado (S3) ou que tocam o banco (SMTP) ficam em `Infrastructure`.

### Webhooks
- **[FATO]** Controllers `[ApiController] [AllowAnonymous]` com rota absoluta; corpo recebido como
  `[FromBody] JsonElement` (não DTO tipado).
- **[FATO] Validação por token apenas (`StringComparison.Ordinal`), sem HMAC** — *hardening* pendente.
- **[FATO]** **Dois webhooks distintos, decisão de não unificar:** billing da plataforma
  (`/api/webhooks/billing/asaas`) e doações por tenant (`/api/webhooks/asaas`). Ambos isentos no
  `SubscriptionGatingMiddleware` e `PermissionMiddleware`.
- **[FATO]** Comportamento atual: **webhook sem token configurado é aceito** (validação só roda se há token).

### Estratégias de sincronização
- **[FATO]** Webhook + scheduler (Asaas billing); *polling* de status pontual sob demanda
  (`TryRefreshAsaasStatusAsync` para doação não confirmada); incremental por **estado da entidade**
  (`Pendente`/pronto → `Enviada`/`Erro`). **Não há full sync nem espelhamento de tabelas externas.**
- **[FATO]** Idempotência obrigatória: billing por `(GatewayPaymentId, Evento)` em `EventoWebhookBilling`;
  doações por estado da entidade; comunicação por `ChaveDedupe` + estado `Reservado`.
- **[FATO]** Correlação com o externo via colunas `Gateway*`/`External*`
  (`GatewaySubscriptionId`, `Fatura.GatewayPaymentId`, `DoacaoOnline.ExternalPaymentId`).

### Paginação de provedores externos
- **[FATO] Não há consumo paginado de API externa** (sem `page/limit`/`offset`/`cursor`/`nextPageToken`
  contra Evolution/Asaas). A paginação existente é **server-side da própria API** (ver Performance).

### Persistência de dados de integração
- **[FATO]** Sem repositório dedicado à integração; o efeito é gravado nas entidades de domínio existentes
  (`Assinatura`, `Fatura`, `DoacaoOnline` + `Receita`, `MensagemAgendada`, `ComunicacaoEntrega`) via
  *upsert* manual por chave do gateway. Sem bulk insert, sem `Merge` do EF.

**Impactos das decisões de integração:** baixa dependência de bibliotecas; degradação graciosa por
kill-switch; controle total sobre serialização e retry.
**Trade-offs:** lock-in no Asaas (gateway nacional); ausência de HMAC nos webhooks; código de cliente
mantido à mão.

---

## Decisões de Migração

### 1. Migração de SQL Server → PostgreSQL (com coexistência)
- **[FATO]** A maior migração trocou o provider de SQL Server para **PostgreSQL (Npgsql)**, que se tornou o
  provider de produção. Evidências: commit `"feat: Migração para PostgreSQL"`; migrations
  `InitialCreatePostgreSQL`/`Baseline_Postgres` (`20260217122041_...`); `commit_migration_postgresql.sh`;
  seleção dinâmica no `Program.cs`.
- **[FATO]** **SQL Server não foi removido** — continua selecionável em runtime (`UseSqlServer` preservado,
  pacote ainda referenciado).
- **Motivação declarada:** portabilidade / redução de lock-in de banco.
- **Trade-offs:** diferenças de SQL entre providers a vigiar; SQL cru precisa ramificar por provider.

### 2. Estratégias de compatibilidade do PostgreSQL
- **[FATO] Shim de comportamento de timestamp:** `Npgsql.EnableLegacyTimestampBehavior = true` preserva a
  semântica "timestamp sem timezone" herdada do SQL Server, evitando reescrever todo o tratamento de datas.
- **[FATO] Marcação de baseline:** migrations antigas (SQL Server) inseridas em `__EFMigrationsHistory` com
  `ON CONFLICT DO NOTHING`, de modo que bancos novos e existentes compartilhem o mesmo conjunto de
  migrations.
- **[FATO] Correção de identidade:** `PerfisAcessoPermissoes` migrado de `SERIAL`/default para
  `GENERATED ALWAYS AS IDENTITY` (`corrigir_perfis_acesso_permissoes_id.sql`).

### 3. Convenções de migração EF Core
- **[FATO]** Migrations versionadas e **idempotentes**, sem "big bang": backfills protegidos por
  `WHERE "TenantId" = 0`; DDL cru com `IF NOT EXISTS` / `ON CONFLICT DO NOTHING`; `Down()` reversível.
- **[FATO]** Nomenclatura descritiva em português: `RefatoracaoPessoaCentralizada`,
  `FormalizarSalasTurmasKids`, `AdicionarTenantIdComunicacaoNotificacoes`, etc.
- **[FATO] Padrão de mudança não-destrutiva em três passos:** adicionar coluna nullable/com default →
  backfill → tornar NOT NULL (ex.: `PessoaId`, `TenantId`).
- **[FATO]** Aplicação automática no startup (`Database:RunMigrations`); scripts SQL de correção
  *out-of-band* em produção (`CORRIGIR_KIDSCHECKINS.sql`, `APLICAR_MIGRATION_KIDS.sql`).

### 4. Consolidação de entidades (normalização)
- **[FATO]** `Visitante`/`Voluntario`/`Usuario` (cada um com seu `Nome`/`Email`) consolidados em
  `Pessoa` + `PessoaPerfil`, com `INSERT ... SELECT` e dedup por chave natural (`WHERE NOT EXISTS ... Email`)
  antes de remover colunas. Preservação de dados como princípio (nunca DDL destrutiva sem backfill).

### 5. Tenantização incremental (prontidão para SaaS)
- **[FATO]** Migrations sucessivas adicionam `TenantId` a entidades nascidas single-tenant, convertendo-as
  em `ITenantEntity` com backfill (ex.: `AdicionarTenantIdComunicacaoNotificacoes`,
  `MULTITENANCY_ROADMAP.md`). Descrito como recorrente.

### 6. Migração *strangler* do módulo de Comunicação (em andamento)
- **[FATO]** O módulo de Comunicação está sendo migrado por **strangler pattern**: um domínio central novo
  nasce ao lado do legado e o absorve gradualmente por adaptadores, sem criar fila paralela.
  Evidências: `COMUNICACAO_SPRINT1_MAPA_LEGADO.md` ("nao criar segunda fila paralela", "reaproveitar o fluxo
  de `MensagemAgendada`", "migrar implementacoes concretas gradualmente, por sprint e por canal").
- **[FATO]** `ConfiguracaoMensagem`/`MensagemAgendada` → domínio central
  (`ComunicacaoTemplate`/`Campanha`/`Entrega`/`Automacao`). Estruturas legadas
  (`EnvioCampanhaAniversario`, `NotificacaoUsuario`, `KidsNotificacao`) mantidas no curto prazo.
- **[HIPÓTESE]** Progresso real da substituição em produção — ver Confirmação.

### 7. Transições de convenção em curso
- **[FATO] `DateTime.Now` → `DateTime.UtcNow`:** entidades/services antigos usam `.Now`; recentes
  (`AuthService`, `KidsRetiradaService`, `Tenant`, `Plano`, billing) usam `.UtcNow` (recomendado para código
  novo).
- **[FATO] Nomenclatura de DTOs:** `Criar{X}Dto`/`Atualizar{X}Dto` (legado, predominante) →
  `Create{X}Request`/`{X}Response` (recente); comentário `// DTOs legados mantidos para compatibilidade`.

---

## Decisões de Performance

- **[FATO] Paginação server-side** como padrão: `[FromQuery] {X}PagedQueryDto`, **default 20, teto 200**,
  ordenação dinâmica por `switch` case-insensitive, retorno como tupla `(Items, Total)` no repositório →
  `PagedResultDto<T>` no service. Motivação: evitar listagens ilimitadas.
- **[FATO] `AsNoTracking()` pragmático** em leituras paginadas e checagens de existência (`AnyAsync`);
  leituras de item único permanecem rastreadas (prontas para update).
- **[FATO] Resiliência transitória:** `EnableRetryOnFailure` (Npgsql) + `CreateExecutionStrategy()` (UoW).
- **[FATO] Reserva concorrente de lote** via SQL cru `FOR UPDATE SKIP LOCKED` (PostgreSQL) /
  `WITH (UPDLOCK, ROWLOCK)` (SQL Server) — único `FromSqlRaw` do projeto — em `MensagemAgendadaRepository`.
- **[FATO] Jitter** nos schedulers para dessincronizar instâncias.
- **[FATO] Limites por execução** nos jobs (`BatchSizeReserva`, `MaxPessoasPorExecucao`,
  `MaxTentativasPorPessoa`) para evitar lotes gigantes.
- **[FATO] 100% async/await** na stack de dados e integrações.
- **[FATO] Ausências observadas:** sem bulk insert dedicado (usa `Add`/`AddRange` + `SaveChangesAsync`, sem
  `EFCore.BulkExtensions`); **sem cache distribuído** (Redis/MemoryCache não encontrados; frontends usam
  `localStorage`, AppKids usa `shared_preferences`); sem paralelismo difundido (`Task.WhenAll`);
  `CancellationToken` usado **só em integrações HTTP**. Se essas ausências são proibição ou apenas
  não-adoção: `TODO: confirmar motivação com o time`.

---

## Decisões de Segurança

### Autenticação e senha
- **[FATO]** JWT Bearer HS256, `ClockSkew=0`, expiração 1h; senhas com BCrypt.
- **[FATO]** Política de senha centralizada em `Application.Security.PasswordPolicy` (8+ chars,
  maiúscula+minúscula+número), aplicada em signup, criação de usuário e troca de senha; o frontend
  **espelha** em `src/lib/passwordPolicy.js` (backend é a fonte da verdade).
- **[FATO] Login lockout:** 5 tentativas / 15 min (`Usuario.TentativasLoginFalhas`/`BloqueadoAte`).
- **[FATO] Rate limiting** via `AddRateLimiter`: signup 5/min/IP, login 10/min/IP (sem proteção distribuída
  por IP — gap conhecido).
- **[FATO]** MFA não implementado (adiado).

### Autorização e gating
- **[FATO]** RBAC próprio via `PermissionMiddleware` (403 sem corpo; `IsPlatformAdmin` faz bypass).
- **[FATO]** `SubscriptionGatingMiddleware` retorna **HTTP 402** para tenant suspenso; isenta
  `/api/auth`, `/api/upload`, `/api/webhooks`, `/api/billing`; **fail-open quando não há tenant**.
- **[FATO]** Pipeline: `UseAuthentication → UseAuthorization → SubscriptionGatingMiddleware →
  PermissionMiddleware`.

### Gerenciamento de segredos
- **[FATO]** Secrets só em env vars (Coolify), `appsettings` sanitizado; secrets por tenant cifrados com
  `IDataProtector`; *startup guard* recusa `Jwt:Key` vazia/placeholder. Origem: incidente de 2026-06-12.

### Identidades gerenciadas e certificados
- **[FATO]** **Não há** managed identities, mTLS ou manuseio de certificados cliente/servidor além da flag
  `UseSsl` (SMTP) e URLs assinadas do S3 (`SignedUrlExpiryMinutes`). Integrações usam API Key/Service
  Account.

### Auditoria e LGPD
- **[FATO] Auditoria automática** via `AuditSaveChangesInterceptor` → tabela `AuditLog` (entidade, ação,
  usuário/IP, `ChangesJson`), com proteção contra recursão.
- **[FATO] LGPD:** consentimento **versionado** e *append-only* (`ConsentimentoRegistro.VersaoDocumento`,
  `TERMOS_VERSAO`); exportação; **"eliminação" = anonimização, não exclusão física**;
  `SolicitacaoTitular` com SLA legal de **15 dias**; Sentry `SendDefaultPii=false`; PII proibida em logs.
  Papéis: **Igreja = Controladora, VerboPlus = Operadora**; DPO = o próprio usuário (fase atual).
  Documentos legais em `legal/` (`POLITICA_DE_PRIVACIDADE.md`, `TERMOS_DE_USO.md`) — revisão jurídica
  pendente.

### Gaps de segurança conhecidos [FATO, registrados em SAAS_READINESS.md]
- Webhook Asaas sem HMAC; uploads em disco servidos sem auth (URL previsível, possível path traversal em
  galerias); Swagger UI público em produção; limites de plano (`MaxUsuarios`/`MaxMembros`) existem mas
  **não bloqueiam**; porta 5433 do Postgres exposta publicamente; schedulers duplicados sem lock distribuído.

---

## Decisões de Organização do Código

### Separação em projetos
- **[FATO]** Backend em 5 projetos (`Domain`, `Application`, `Infrastructure`, `API`, `BackgroundWorker`) +
  testes; Worker fora de `src/`.
- **[FATO]** Frontends e mobile como repositórios separados, cada um com seu `.git/`, consumindo **a mesma
  API .NET** (`https://api.kingdombr.com.br/api`). Exceção: a landing VerboPlus **não consome a API**.
- **[FATO] Sem módulo de DI compartilhado** — registro inline em cada `Program.cs` (~152 serviços na API,
  ~40 no Worker), "duplicação consciente"; `ValidateOnBuild` no Worker mitiga drift.

### Convenções de nomenclatura
- **[FATO] Domínio em Português, técnico em Inglês** (regra forte): entidades, propriedades, DTOs, mensagens
  de erro e logs em português; sufixos `Repository`/`Service`/`Dto`, verbos CRUD (`GetByIdAsync`,
  `CreateAsync`) e nomes de teste em inglês; métodos de negócio em português (`ConfirmarAsync`,
  `AlterarSenhaAsync`).
- **[FATO] Backend:** Controllers `{Entidade}Controller` (`[ApiController]`, `api/[controller]`,
  `[Authorize]` por padrão, `ControllerBase`, `ActionResult<T>`, 100% async); Services
  `{X}Service : I{X}Service` (interface e impl no mesmo arquivo); Repositories `{X}Repository`;
  DTOs `{X}Dto`/`Criar{X}Dto`/`Atualizar{X}Dto`, genérico `PagedResultDto<T>`; migrations
  `{timestamp}_{NomeEmPortugues}`.
- **[FATO] Frontend:** componentes PascalCase com sufixos `List`/`Form`/`Details`/`Dialog`; componentes
  shadcn kebab-case em `components/ui/`; alias `@` → `src`.

### Organização de diretórios e responsabilidades das camadas
- **[FATO]** Controllers = superfície HTTP (sem regra de negócio; não acessam `DbContext`/EF/HTTP clients);
  Services = regra + mapeamento manual DTO↔entidade (não acessam `DbContext` direto nem `HttpContext`);
  Repositories = consultas/paginação/filtro/carimbo de TenantId (sem regra de negócio); Entities = modelo
  puro com DataAnnotations, **sem classe base**, PK sempre `int Id` (sem `Guid`), status como **enums**
  (sem value objects formais).
- **[FATO]** DTOs subdivididos por domínio (`DTOs/Pessoas/`, `DTOs/Visitantes/`) apenas quando há
  complexidade/paginação.
- **[FATO] Exceção documentada:** services de orquestração em `Infrastructure` (ex.: `BillingService`) usam
  `DbContext` diretamente.

---

## Tecnologias Avaliadas ou Abandonadas

> Documentado apenas quando há evidência real de substituição ou de não-adoção deliberada.

### Substituídas (com evidência)
| Antiga | Atual | Possível motivo |
|---|---|---|
| SQL Server (provider primário) | PostgreSQL / Npgsql | Portabilidade / fim de lock-in (commit "Migração para PostgreSQL") |
| `Visitante`/`Voluntario`/`Usuario` dispersos | `Pessoa` central + `PessoaPerfil` | Eliminar duplicação / normalização (`RefatoracaoPessoaCentralizada`) |
| `SERIAL`/default em `PerfisAcessoPermissoes` | `GENERATED ALWAYS AS IDENTITY` | Correção de auto-incremento pós-Postgres |
| Entidades single-tenant (Comunicação/Notificações) | `ITenantEntity` com `TenantId` | Isolamento multi-tenant (SaaS) |
| `ConfiguracaoMensagem`/`MensagemAgendada` | Domínio central de Comunicação (em andamento) | Omnichannel; separar template de automação |

### Mantidas por coexistência deliberada (não abandonadas)
- **[FATO] SQL Server** — provider alternativo em runtime (pacote e branch `UseSqlServer` preservados).
- **[FATO] SQLite** — somente em testes (rapidez/isolamento).
- **[FATO] Estruturas legadas de Comunicação** — convergindo via adaptador (strangler).
- **[FATO] Comportamento legado de timestamp** — preservado via `EnableLegacyTimestampBehavior=true`.

### Evitadas/não adotadas por convenção (não são migrações)
- **[FATO]** AutoMapper, Newtonsoft.Json, Polly, `record` para DTO/entidade, `Repository<T>` genérico,
  classe base de entidade, broker de mensageria, Hangfire/Quartz/cron externo, Redux/Zustand (frontend),
  middleware global de exceção, PII em logs.

### Ausentes (nada a migrar — explicitamente verificado) [FATO]
- DataSets/DataTables/ADO.NET legado (zero `DataSet`/`SqlDataAdapter`/`IDataReader`).
- Stored procedures (zero `CREATE PROCEDURE`/`EXEC`).
- Crystal Reports / bibliotecas de relatório / geração de PDF server-side (relatórios = endpoints JSON
  agregados renderizados em React + `recharts`; e-mails = templates HTML estáticos
  `01-verificacao-email.html` … `04-pagamento-pendente.html`).
- `Web.config` (é ASP.NET Core, não .NET Framework).
- Versionamento de API (`/v1/`, `/v2/`).

> **[HIPÓTESE]** Os docs canônicos levantam — mas **não confirmam** — a possível existência de um sistema de
> origem na "Igreja Kingdom" (com procedures/DataSets/Crystal Reports/.NET Framework) do qual este teria
> sido migrado. Apenas o resultado moderno está no repositório. `TODO: confirmar com o time`.

---

## Trade-offs Arquiteturais

- **[FATO] Simplicidade operacional vs isolamento:** tenant-per-row (1 banco) é simples de operar, mas não
  oferece isolamento físico e depende de toda entidade implementar `ITenantEntity`.
- **[FATO] Simplicidade vs robustez de jobs:** "fila" em tabela + `BackgroundService` evita broker, mas
  abre risco de execução duplicada (sem lock distribuído) e não tem dead-letter queue.
- **[FATO] Explicitness vs produtividade:** mapeamento manual, sem AutoMapper/Polly/`Repository<T>`
  genérico, dá controle e previsibilidade ao custo de mais código repetitivo (boilerplate).
- **[FATO] Velocidade de entrega vs pureza arquitetural:** DI duplicado entre API e Worker (sem módulo
  compartilhado) já causou bug; mitigado por `ValidateOnBuild`, mas é dívida consciente.
- **[FATO] Mesma imagem em todos os ambientes vs risco de feature desligada:** kill-switch por configuração
  facilita deploy, porém esquecer de setar uma credencial deixa a feature silenciosamente off.
- **[FATO] Disponibilidade vs segurança no gating:** `SubscriptionGatingMiddleware` é *fail-open* quando não
  há tenant (prioriza não bloquear indevidamente).
- **[FATO] Compatibilidade vs dívida de divergência:** coexistência de `DateTime.Now`/`UtcNow`, de
  nomenclaturas de DTO legadas/novas, e de variações de estilo (constructor order, `NotFound()` vazio vs com
  corpo) — toleradas em nome da compatibilidade.
- **[FATO] Lock-in no Asaas vs cobertura nacional** (PIX/Boleto/Cartão) e no Coolify para hosting.

---

## Linha do Tempo das Decisões

> Reconstruída a partir de timestamps de migrations EF Core, scripts versionados e datas explícitas nos
> docs. Marcos sem data confiável recebem `TODO: confirmar data da decisão`.

1. **2025-12-11 — Schema inicial.** Migration `20251211042104_InitialCreate` (primeiro schema; ainda em
   contexto SQL Server). [FATO]
2. **2025-12-12 — Consolidação de Pessoa.** `20251212034213_RefatoracaoPessoaCentralizada` — unificação de
   Visitante/Voluntario/Usuario em `Pessoa` + `PessoaPerfil` com backfill. [FATO]
3. **2026-02-16 — RBAC.** `20260216205904_AdicionarPerfisAcesso` — introdução do controle de acesso por
   perfil/permissão. [FATO]
4. **2026-02-17 — Baseline PostgreSQL.** `20260217122041_InitialCreatePostgreSQL` + commit
   "Migração para PostgreSQL" — virada do provider primário para Postgres, mantendo SQL Server por
   coexistência. [FATO]
5. **2026-06-12 — Incidente e rotação de segredos.** Segredos (Postgres, Evolution, JWT) versionados no git
   foram rotacionados; origem da regra "segredos só em env vars". [FATO]
6. **2026-06-18 — Tenantização da Comunicação.** `20260618213103_AdicionarTenantIdComunicacaoNotificacoes`
   — conversão de entidades de comunicação/notificações para `ITenantEntity`. [FATO]
7. **2026-06-27 — Estado atual.** Produto em "preparação para lançamento como SaaS"; data desta análise.
   [FATO]
- **Em andamento (sem marco único de conclusão):** strangler do módulo de Comunicação; transição
  `DateTime.Now`→`UtcNow`; transição de nomenclatura de DTOs. `TODO: confirmar data da decisão` de início e
  término de cada uma.

---

## Decisões que Precisam de Confirmação

> Decisões/itens cuja motivação, estado ou definitividade **não puderam ser determinados com segurança** a
> partir das evidências.

1. **Pipeline efetivo de deploy do backend** — relação entre `BackEnd/azure-pipelines.yml` (artefato
   `app.zip`) e o deploy real via Coolify/Docker. `TODO: confirmar com o time`.
2. **Versão do .NET do projeto de testes** — `.csproj` diz `net10.0`, mas comentário no Dockerfile do
   Worker menciona ".NET 9". Drift a confirmar.
3. **Lock distribuído de schedulers** — decisão pendente de rodar schedulers apenas no Worker (hoje podem
   rodar na API e no Worker sem lock).
4. **Cache distribuído, bulk operations e paralelismo** — ausência é proibição deliberada ou apenas
   não-adoção? `TODO: confirmar motivação com o time`.
5. **HMAC em webhooks** — ausência é intencional (apenas token) ou hardening ainda não feito? E
   "webhook sem token = aceito" é intencional?
6. **Padronização de retry/timeout do Asaas** — hoje só a Evolution tem retry; Asaas "falha rápido"
   deliberadamente. Confirmar se deve permanecer assim.
7. **Métricas e tracing distribuído** — fora de escopo definitivo ou pendentes?
8. **Enforce de limites de plano** (`MaxUsuarios`/`MaxMembros`) — existem mas não bloqueiam; decisão de
   bloquear pendente.
9. **Divergência de rotas do Portal** — docs do Portal citam rotas em inglês (`/api/events`,
   `/api/church/info`) enquanto os controllers são em português; mapeamento real a confirmar.
10. **MFA** — confirmado como adiado, mas sem data/decisão formal de quando.
11. **Existência de sistema legado de origem** ("Igreja Kingdom" com procedures/DataSets/Crystal/.NET
    Framework) do qual o projeto teria sido migrado. `TODO: confirmar com o time`.
12. **Progresso real do strangler de Comunicação** em produção (o `MAPA_LEGADO` é planejamento de Sprint 1).
13. **Plataformas-alvo oficiais do AppKids** — `pubspec.yaml`/`flutter_launcher_icons` configuram Android
    (`ios: false` em ícone/splash), enquanto notas internas sugerem foco iOS/macOS. Confirmar o alvo de
    publicação.
14. **Preços definitivos dos planos** — seeds usam placeholders (R$49,90 / R$99,90 / R$199,90).
15. **Motivações de fundo** das escolhas de linguagem (.NET 10), monólito modular e RBAC próprio — não
    declaradas explicitamente. `TODO: confirmar motivação com o time`.

---

*Documento gerado por reconstrução a partir do estado atual do repositório. Não contém recomendações,
críticas nem alternativas — apenas decisões sustentadas por evidências, com lacunas explicitamente marcadas
para confirmação.*
