# PROJECT_CONTEXT.md

> **Memória permanente e contexto compartilhado do projeto AppIgreja / VerboPlus.**
> Este documento foi gerado a partir de evidências observadas no código, estrutura de diretórios, arquivos de configuração e dependências. Ele serve como ponto único de verdade para novas conversas, agentes e engenheiros que entram no projeto.
>
> Convenções deste documento:
> - **Fatos verificados** vêm com caminho de arquivo de referência.
> - Onde uma informação não pôde ser confirmada com confiança, está marcada como `TODO: confirmar com o time`.
> - Última atualização da análise: **2026-06-27**.

---

## 1. Visão Geral

### Objetivo do sistema
Plataforma de **gestão de igrejas** (ChMS — Church Management System) sendo comercializada como **SaaS multi-tenant** sob a marca **Verbo+** (VerboPlus), focada no mercado brasileiro. Origem do produto na "Igreja Kingdom" (domínios `kingdombr.com.br` ainda em uso em produção).

### Problema que resolve
Centraliza a operação administrativa de uma igreja: cadastro de pessoas/membros/visitantes, voluntariado e escalas de ministério, eventos e inscrições, módulo infantil (Kids) com check-in seguro, financeiro (receitas/despesas/orçamento/patrimônio), comunicação omnichannel (WhatsApp), portal público e doações online — tudo isolado por igreja (tenant) e cobrado por assinatura.

### Contexto de negócio
- Produto em fase de **preparação para lançamento como SaaS** (ver [SAAS_READINESS.md](../docs/SAAS_READINESS.md)).
- Modelo de receita: **assinatura mensal da igreja** (3 planos) via gateway **Asaas**, com trial → inadimplência → suspensão e gating HTTP 402.
- Também processa **doações online** (PIX/Boleto/Cartão via Asaas) para a igreja-cliente.
- Conformidade **LGPD** já implementada (consentimento versionado, exportação, anonimização, solicitações do titular).

### Principais funcionalidades (por evidência de controllers/entidades)
- **Pessoas / Membros / Visitantes** — cadastro centralizado em `Pessoa`, perfis, aniversários.
- **Voluntariado e Escalas** — equipes, escalas por equipe, modelos de escala, indisponibilidades, solicitações de troca.
- **Eventos** — eventos, ocorrências, recorrências, inscrições.
- **Kids** — turmas, salas, check-in/checkout, pré-check-in, retirada segura por token/PIN, ocorrências, conteúdo de aula, notificações push, device tokens.
- **Financeiro** — receitas, despesas, categorias, centros de custo, contas bancárias, fornecedores, orçamento, relatórios, dashboard financeiro.
- **Patrimônio** — itens e movimentações.
- **Comunicação (omnichannel)** — templates, campanhas, segmentos, automações, entregas, preferências, mensagens agendadas, campanha de aniversário.
- **Portal público / Site** — configuração do portal, destaques, notícias, galerias de fotos, enquetes, contatos, projetos, hub de casas (células).
- **SaaS / Plataforma** — tenants, planos, assinaturas, faturas, billing, webhooks de billing, signup self-service, verificação de e-mail.
- **Segurança / LGPD** — perfis de acesso (RBAC), auditoria, consentimentos, solicitações do titular.

### Módulos existentes (subprojetos no monorepo)
| Módulo | Pasta | Tipo |
|---|---|---|
| **BackEnd** | `BackEnd/` | API .NET 10 (Clean Architecture) + BackgroundWorker |
| **FrontEnd** | `FrontEnd/` | Painel administrativo (React 19 + Vite) — a marca "Verbo+" |
| **Portal** | `Portal/` | Site público da igreja (React 18 + Vite) |
| **AppKids** | `AppKids/` | App mobile do responsável (Flutter) |
| **VerboPlus** | `VerboPlus/` | Landing page de marketing do SaaS (React 19 + Vite + Tailwind) |
| **CadastroMembro** | `CadastroMembro/` | Formulário público de cadastro de membro (HTML/CSS/JS vanilla) |
| **evolution-api** | `evolution-api/` | Config de integração WhatsApp (Evolution API, serviço de terceiros) |
| **legal** | `legal/` | Termos de Uso + Política de Privacidade (Markdown versionado) |

> **Nota estrutural:** o diretório raiz **não é um repositório git** (`Is a git repository: false`). Cada subprojeto tem seu próprio `.git/` (BackEnd, FrontEnd, Portal, AppKids, CadastroMembro). É um conjunto de repositórios coexistindo numa pasta, não um monorepo com git unificado.

---

## 2. Stack Tecnológica

### Linguagem principal
- **C# / .NET 10** (`net10.0`) — backend.
- **JavaScript/JSX (React)** — frontends web.
- **Dart / Flutter** — app mobile Kids.
- **HTML/CSS/JS vanilla** — formulário público de cadastro.

### Frameworks
- **Backend:** ASP.NET Core (Web API) + EF Core 9 + .NET Generic Host (Worker).
- **FrontEnd (admin) e VerboPlus:** React 19 + Vite 6.
- **Portal:** React 18 + Vite 5.
- **AppKids:** Flutter (Dart 3.2+).

### Bibliotecas importantes

**Backend (.NET 10):**
- EF Core 9.0.x — providers: `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.2 (padrão), `Microsoft.EntityFrameworkCore.SqlServer` 9.0.6, `Microsoft.EntityFrameworkCore.Sqlite` 9.0.6 (testes).
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.17 + `System.IdentityModel.Tokens.Jwt` 8.3.1 (autenticação).
- `BCrypt.Net-Next` 4.0.3 (hash de senha).
- `Swashbuckle.AspNetCore` 6.6.2 + `Microsoft.AspNetCore.OpenApi` (Swagger).
- `Sentry.AspNetCore` 6.6.0 (API) / `Sentry.Extensions.Logging` 6.6.0 (Worker).
- `FirebaseAdmin` 3.2.0 (push notifications Kids).
- `AWSSDK.S3` 3.7.x (storage opcional).
- `SixLabors.ImageSharp` 3.1.12 (processamento de imagens).
- Testes: `xunit` 2.9.2, `Moq` 4.20.72, `FluentAssertions` 8.6.0, `coverlet.collector` 6.0.2.

**FrontEnd (admin):**
- React 19.1, Vite 6, **pnpm** 10.4.1.
- UI: **shadcn/ui** (estilo "new-york") sobre **Radix UI**, **Tailwind CSS 4.1**, `lucide-react`, `sonner` (toasts), `framer-motion`, `recharts` (gráficos), `cmdk`.
- Formulários: `react-hook-form` 7.56 + `@hookform/resolvers` + **Zod** 3.24.
- HTTP: `axios` 1.10 (com interceptors JWT/tenant).
- Roteamento: `react-router-dom` 7.6 (rotas lazy).
- i18n: `i18next` + `react-i18next` (pt-BR, en-US, es-ES).
- Observabilidade: `@sentry/react` 10.58.
- Testes: **Vitest 4** + React Testing Library 16 + jsdom.

**Portal:** React 18, Vite 5, `axios` 1.6, `react-router-dom` 6, `react-helmet-async` (SEO), `swiper` (carrossel), Vitest.

**VerboPlus (landing):** React 19, Vite 6, Tailwind 3.4, `framer-motion`, `lucide-react`.

**AppKids (Flutter):** `http`, `go_router`, `provider` (state), `flutter_secure_storage`, `firebase_core` + `firebase_messaging` (FCM), `qr_flutter`, `shared_preferences`, `connectivity_plus`, `intl`, `url_launcher`.

### Banco de dados
- **PostgreSQL** em produção (`Database:Provider = "PostgreSQL"`, via Npgsql). SQL Server suportado como alternativa; **SQLite** usado em testes.
- Acesso via **EF Core** (Code First + Migrations). Ver §8.

### Mecanismos de cache
- `TODO: confirmar com o time` — não foi encontrado Redis/MemoryCache distribuído no backend. Frontends usam `localStorage` para token/sessão. AppKids usa `shared_preferences` (cache offline).

### Mensageria / Jobs
- Sem broker de mensageria (não há RabbitMQ/Kafka/SQS observados). Jobs assíncronos via `BackgroundService` (schedulers) na API e no Worker (ver §6).

### Cloud providers
- **Coolify** (PaaS auto-hospedado, Docker) — hospeda **API + Worker** em produção. Evidência: memórias de deploy + `Dockerfile` em API e Worker.
- **Azure Static Web Apps** — hospeda os frontends (FrontEnd admin e Portal). Evidência: `azure-pipelines.yml` em ambos.
- **AWS S3** — suportado para storage de uploads (opcional; default é disco local).
- Host de produção do Postgres: `77.37.43.5:5433` (exposto; hardening pendente).

### Ferramentas de observabilidade
- **Sentry** (API, Worker e frontends) — config-driven, **desligado quando DSN vazio**, `SendDefaultPii=false` (LGPD).
- **Health checks** ASP.NET Core (DB + configs de Evolution API, Email, Push, schedulers).

### Ferramentas de autenticação
- **JWT Bearer** (chave simétrica HS256, `ClockSkew=0`, expiração 1h).
- **BCrypt** para hash de senha.
- **RBAC** próprio via `PerfilAcesso` + `PermissionMiddleware` (ver §10).

---

## 3. Arquitetura

### Estilo arquitetural
- **Backend: Clean Architecture** em 4 camadas + um Worker separado.
- **Frontends: SPA** modular por domínio/feature.
- **Multi-tenant** (tenant-per-row com global query filter) em todo o backend.

### Separação por camadas (backend) e responsabilidades
```
SistemaIgreja.Domain          → Entidades, interface ITenantEntity. Sem dependências internas.
        ↑
SistemaIgreja.Application      → DTOs, Interfaces, Services (casos de uso), Configuration (Options),
                                 Security (JWT/PasswordPolicy), Utils, JsonConverters.
                                 Depende de: Domain.
        ↑
SistemaIgreja.Infrastructure   → DbContext, Repositories, Migrations, Services de infra
                                 (Asaas, Evolution, Email/SMTP, S3, Audit, Schedulers).
                                 Depende de: Domain + Application.
        ↑
SistemaIgreja.API              → Controllers, Middleware, Permissions, Swagger, Health checks.
                                 Depende de: Application + Infrastructure.

SistemaIgreja.BackgroundWorker → Host genérico standalone com schedulers.
                                 Depende de: Application + Infrastructure.
tests/SistemaIgreja.API.Tests  → Testes (referencia API + Application + Domain).
```

Responsabilidades:
- **Domain:** modelo de negócio puro (77 entidades), zero infra.
- **Application:** lógica de aplicação/regras, contratos (interfaces `I{X}Service`), DTOs e configuração tipada.
- **Infrastructure:** acesso a dados (EF Core, repositories) e integrações externas.
- **API:** superfície HTTP, autenticação/autorização, middleware, documentação.
- **Worker:** execução de jobs agendados fora do processo da API.

### Organização dos projetos
- `BackEnd/src/` — 4 projetos da Clean Architecture.
- `BackEnd/SistemaIgreja.BackgroundWorker/` — worker (fora de `src/`, referencia projetos de `src/`).
- `BackEnd/tests/` — projeto de testes.

### Padrões utilizados
- **Repository + Unit of Work** (`IUnitOfWork`, `I{X}Repository`).
- **Service layer** (interface-driven, injeção via DI).
- **Options pattern** (`IOptions<T>` para configs: `EvolutionApiSettings`, `BillingSettings`, `EmailSettings`, schedulers, etc.).
- **DTO + mapeamento manual** (sem AutoMapper — mappers privados `MapToDto`).
- **Global Query Filter** por tenant + carimbo automático de `TenantId` no `SaveChanges`.
- **Middleware** para permissões (RBAC) e gating de assinatura.
- **Background services** com jitter.

### Dependências entre módulos (cross-projeto)
Todos os clientes consomem a **mesma API .NET** (`https://api.kingdombr.com.br/api` em produção):
- FrontEnd (admin) → API (JWT + headers de tenant).
- Portal → API (endpoints públicos do site).
- AppKids → API (`/api/auth`, `/api/kids/**`) + Firebase FCM.
- CadastroMembro → API (`POST /api/Membros/cadastro`).
- VerboPlus (landing) → **não consome API** (marketing puro; CTA para WhatsApp e `/signup`).

---

## 4. Estrutura de Diretórios

### Raiz do repositório
- `BackEnd/` — backend .NET (ver abaixo).
- `FrontEnd/` — painel administrativo React (Verbo+).
- `Portal/` — site público da igreja.
- `AppKids/` — app Flutter do responsável.
- `VerboPlus/` — landing page de marketing.
- `CadastroMembro/` — formulário público (HTML único).
- `evolution-api/` — apenas `.env` de config do serviço WhatsApp de terceiros.
- `legal/` — `TERMOS_DE_USO.md`, `POLITICA_DE_PRIVACIDADE.md`, `README.md`.
- `SAAS_READINESS.md` — checklist vivo de prontidão para venda (fonte de verdade dos bloqueadores).
- **Dezenas de `*.md` de planejamento** na raiz (prefixos `KIDS_*`, `COMUNICACAO_*`, `ADMIN_REDESIGN_*`, `QUALIDADE_*`, `PLANEJAMENTO_*`, `MULTITENANCY_ROADMAP.md`, `PATRIMONIO_*`) — histórico de sprints, backlogs e execução técnica. Úteis como contexto de produto; **não são fonte de verdade do estado atual do código**.
- `01-..04-*.html` na raiz — templates de e-mail (verificação, trial, suspensão, pagamento pendente).

### `BackEnd/src/SistemaIgreja.API/`
- `Controllers/` — 64 controllers REST (`{Entidade}Controller`).
- `Middleware/` — ex. `SubscriptionGatingMiddleware`.
- `Permissions/` — `PermissionMiddleware`, `PermissionResourceMap`.
- `Services/` — health checks (`DatabaseHealthCheck`, `ConfigurationHealthChecks`).
- `Swagger/` — filtros (ex. `FileUploadOperationFilter`).
- `wwwroot/`, `uploads/` — assets/uploads em disco local.
- `Dockerfile`, `appsettings.json`, `Program.cs`.

### `BackEnd/src/SistemaIgreja.Application/`
- `DTOs/` (~278 DTOs em ~66 subpastas), `Interfaces/` (~77), `Services/` (~83), `Configuration/`, `Security/`, `Utils/`, `JsonConverters/`.

### `BackEnd/src/SistemaIgreja.Infrastructure/`
- `Data/` — `SistemaIgrejaDbContext`, Unit of Work, `AuditSaveChangesInterceptor`.
- `Repositories/` (~59), `Migrations/` (40+), `Services/` (Billing, schedulers, Email, S3, Audit), `Resources/`.

### `BackEnd/src/SistemaIgreja.Domain/Entities/`
- 77 classes de entidade + `ITenantEntity.cs`.

### `BackEnd/SistemaIgreja.BackgroundWorker/`
- `Program.cs`, `WorkerCurrentUserContext.cs`, `Dockerfile`.

### `FrontEnd/src/`
- `api/` — módulos HTTP por domínio (~26, inclui `financeiro/`).
- `pages/` — ~38 features (~119 `.jsx`).
- `components/` — reutilizáveis; `components/ui/` (~56 componentes shadcn); `components/Layout/` (Sidebar/Header).
- `context/` — `AuthContext` (auth multi-tenant), `ThemeContext` (verbo/light/dark).
- `hooks/` (~12), `lib/` (`apiClient.js`, `env.js`, `sentry.js`, `formatters.js`, `passwordPolicy.js`), `locales/` (pt-BR/en-US/es-ES), `i18n/`, `test/` (setup Vitest).

### `Portal/src/`
- `components/`, `pages/`, `services/` (`api.config.js`), `contexts/`, `hooks/`, `utils/`.

### `AppKids/lib/`
- `core/` (ApiClient, auth, push), `features/` (`auth/`, `kids/`, `avisos/`, `settings/`), `app_router.dart`, `app_state.dart`, `main.dart`.

---

## 5. Convenções de Código

### Idioma
- **Domínio em Português** (entidades, propriedades, DTOs, namespaces, comentários, mensagens de erro). Ex.: `Pessoa`, `Voluntario`, `Escala`, `Nome`, `DataNascimento`.

### Backend — nomenclatura e organização
- **Controllers:** `{Entidade}Controller`, `[ApiController]`, rota `api/[controller]`, `[Authorize]` por padrão, herdam `ControllerBase`. Retornam `ActionResult<T>` tipado. **100% async**.
- **Services (Application):** `{X}Service : I{X}Service`, injetam `IUnitOfWork`, repositórios e `ILogger<T>`. Mapeamento manual DTO↔entidade.
- **Repositories (Infrastructure):** `{X}Repository : I{X}Repository`, recebem `SistemaIgrejaDbContext` + contexto de tenant. Paginação retorna tupla `(Items, Total)`; ordenação/filtro dinâmicos.
- **DTOs:** `{X}Dto` (read), `Create{X}Dto`, `Update{X}Dto`, `{X}PagedQueryDto`/`...RequestDto`. Genérico `PagedResultDto<T>`. Validação só via DataAnnotations (sem regra de negócio no DTO).
- **Interfaces:** `I{X}Service` (aplicação), `I{X}Repository` (dados), `I{X}` (serviços genéricos). Registradas no DI em `Program.cs` (≈152 serviços na API; ≈40 no Worker — **não há módulo de DI compartilhado**, registro é inline em cada `Program.cs`).

### Tratamento de erros (backend)
- Services lançam exceções semânticas (`KeyNotFoundException`, `ArgumentException`). Controllers traduzem para status HTTP (401/403/404/400/500).
- Mensagens de erro de auth retornam corpo `{ message }` (não string crua) — o frontend depende disso.

### Logging / Validações / Async
- Logging via `ILogger<T>`; erros enviados ao **Sentry** (`MinimumEventLevel=Error`).
- Validação de senha centralizada em `SistemaIgreja.Application.Security.PasswordPolicy` (8+ chars, maiúscula+minúscula+número), aplicada em `SignupService`, `UsuarioService.CreateAsync`, `AuthService.AlterarSenhaAsync`. Espelhada no front em `src/lib/passwordPolicy.js`.
- **async/await em toda a stack de dados** (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).

### Frontend — convenções
- Componentes em **PascalCase**; sufixos `List` / `Form` / `Details` / `Dialog`. Componentes shadcn em `components/ui/` com nomes kebab (`button.jsx`).
- API centralizada em `lib/apiClient.js` (Axios) + módulos por domínio em `api/*.js` (`pessoasApi.getAll()` etc.); DTOs normalizados (aceita PascalCase e camelCase).
- Estado global via **Context API** (Auth, Theme); persistência em `localStorage` (`token`, `refreshToken`, `usuario`, `selectedTenantId`). Sem Redux/Zustand.
- Rotas **lazy-loaded** com `React.lazy`; proteção via `<ProtectedRoute>` e `<RequirePermission>`.
- Alias `@` → `src` (vite.config.js).

### Async/await
- Backend: padrão obrigatório em dados e integrações externas.
- Frontend: chamadas Axios `async`; interceptors fazem refresh de token e redirect 402→`/billing`.

---

## 6. Integrações Externas

> **Referência canônica de padrões de integração:** [.claude/INTEGRATION_PATTERNS.md](.claude/INTEGRATION_PATTERNS.md) — detalha clients HTTP, autenticação, retries, webhooks, schedulers, kill-switch e checklists. Consultar antes de criar/alterar qualquer integração. Esta seção é apenas o resumo por provedor.

### Evolution API (WhatsApp)
- **Finalidade:** envio de mensagens WhatsApp (campanhas, lembretes, aniversários).
- **Protocolo:** REST/HTTP (`HttpClientFactory`, `EvolutionApiService`).
- **Autenticação:** API Key (header).
- **Sincronização:** envio com retry (`MaxRetries`, `RetryDelaySeconds`) e delay configurável; números formatados com código de país (`CodigoPaisPadrao=55`).
- **Config:** seção `EvolutionApi` (`BaseUrl`, `ApiKey`, `InstanceName`, `TimeoutSeconds`, `MaxRetries`). Serviço de terceiros auto-hospedado (`evolution.kingdombr.com.br`); pasta `evolution-api/` guarda só o `.env`.

### Asaas (pagamentos + billing de assinatura)
- **Finalidade:** (a) **doações online** (PIX/Boleto/Cartão) da igreja-cliente; (b) **billing da assinatura** do SaaS (cobrança da igreja).
- **Protocolo:** REST/HTTP (`AsaasPaymentService`, `AsaasBillingClient` via `HttpClientFactory`).
- **Autenticação:** API Key. Webhook validado **só por token** (`WebhookToken`) — **sem HMAC** (hardening pendente).
- **Sincronização:** webhook `POST /api/webhooks/billing/asaas` + scheduler de transições (trial→inadimplente→suspensa).
- **Config:** seção `Billing` (`TrialDias`, `CarenciaDias`, `Asaas.ApiKey/WebhookToken/Environment`).

### SMTP / E-mail
- **Finalidade:** verificação de e-mail (signup), avisos de trial/suspensão/pagamento.
- **Protocolo:** SMTP (`SmtpEmailService`).
- **Config:** seção `Email` (`Enabled` — **default `false`**, `Host`, `Port`, `Username`, `Password`, `UseSsl`, `FromAddress`). Provedor pretendido: **Resend** (chave `re_...`). Sem SMTP configurado em produção, e-mails são no-op.

### Firebase Cloud Messaging (push Kids)
- **Finalidade:** notificações push no AppKids (status de check-in, avisos).
- **Protocolo:** FirebaseAdmin SDK (server) + `firebase_messaging` (client Flutter).
- **Autenticação:** Service Account (`Firebase:CredentialsPath`).
- **Observação:** canal Push (`ComunicacaoPushCanalProvider` + `IKidsPushNotificationService`) é **exclusivo da API** (não registrado no Worker, depende de Firebase).

### AWS S3 (storage de uploads)
- **Finalidade:** armazenamento de arquivos/imagens (opcional).
- **Autenticação:** AccessKey/SecretKey, URLs assinadas (`SignedUrlExpiryMinutes`).
- **Config:** seção `Storage` (`Provider` = `local` por default ou `s3`). **Hoje produção usa disco local** (não escala multi-instância — migração pendente).

### Sentry (observabilidade)
- **Finalidade:** error tracking (API, Worker, FrontEnd admin).
- **Config:** seção `Sentry` (`Dsn` vazio = desligado, `SendDefaultPii=false`, `TracesSampleRate=0`). Projeto frontend separado (`appigreja-frontend`).

---

## 7. Conceitos de Domínio

### Entidades / agregados principais
- **Tenant** — a igreja (raiz do isolamento multi-tenant). `Tenant.InitialTenantId = 1` (slug `kingdom`). `TenantDomain` mapeia domínios para tenants. **Globais (sem TenantId):** `Tenant`, `TenantDomain`, `Plano`, `EventoWebhookBilling`, `VerificacaoEmail`.
- **Pessoa** — entidade central (membros, visitantes, voluntários, crianças, responsáveis derivam/relacionam-se a ela). Possui `FotoUrl`. Tipos via `TipoPessoa`/`PerfilPessoa`.
- **Usuario** — credencial de acesso ao sistema (login, senha BCrypt, lockout). Relaciona-se a `PerfilAcesso` via `PessoaPerfil`/`PessoaPerfil`.
- **Voluntario / Equipe / Escala** — `Escala`, `EscalaItem`, `EscalaModelo`, `IndisponibilidadeVoluntario`, `SolicitacaoTrocaEscala`.
- **Evento** — `Evento`, `EventoOcorrencia`, `EventoRecorrencia`, `InscricaoEvento`, `StatusInscricao`.
- **Kids** — `KidsTurma`, `KidsSala`, `KidsCheckin`, `KidsPreCheckin`, `KidsOcorrencia`, `KidsConteudoAula(+Anexo)`, `KidsNotificacao`, `KidsDeviceToken`, `CriancaDetalhe`, `ResponsavelCrianca`.
- **Financeiro** — `Receita`, `Despesa`, categorias (`CategoriaReceita`/`CategoriaDespesa`), `CentroCusto`, `ContaBancaria`, `Fornecedor`, `OrcamentoCategoria`, `DoacaoOnline`, `FinalidadeDoacao`, `GivingProviderConfig`.
- **Patrimônio** — `PatrimonioItem`, `PatrimonioMovimentacao`, `CategoriaPatrimonio`.
- **Comunicação** — `Comunicacao`, `ComunicacaoTemplate/Campanha/CampanhaCanal/Segmento/Automacao/Entrega/Preferencia`, `MensagemAgendada`, `ConfiguracaoMensagem`, `ConfiguracaoCampanhaAniversario`, `EnvioCampanhaAniversario`, `NotificacaoUsuario`.
- **Portal/Site** — `ConfiguracaoPortal`, `DestaqueSite`, `Noticia`(+`CategoriaNoticia`), `GaleriaFoto`(+`Item`), `Enquete`, `Contato`, `Projeto`, `HubCasa`.
- **SaaS/Billing** — `Plano`, `Assinatura`, `Fatura`, `EventoWebhookBilling`, `VerificacaoEmail`.
- **Segurança/LGPD** — `PerfilAcesso`, `PessoaPerfil`, `AuditLog`, `ConsentimentoRegistro`, `SolicitacaoTitular`.

### Relacionamentos-chave
- **Toda entidade de negócio implementa `ITenantEntity`** (`int TenantId`) e é filtrada por tenant. Índices únicos compostos por `(TenantId, ...)`.
- `Pessoa` é o hub: relaciona com voluntariado, kids, escalas, comunicação, eventos.

### Terminologia de negócio
- **Tenant = igreja**; **Verbo+ = a marca do produto**; **plataforma = o SaaS** (admin de plataforma = `IsPlatformAdmin`, claim no JWT, bypassa o `PermissionMiddleware`).
- **Escala** = agenda de voluntários por ministério/equipe.
- **Retirada segura** (Kids) = checkout validado por token/PIN.

### Fluxos importantes
- **Onboarding self-service:** `POST /api/signup` (rate-limited) → verificação de e-mail → criação de tenant + usuário admin → trial.
- **Billing:** trial → (Asaas) inadimplência → suspensão; `SubscriptionGatingMiddleware` retorna **HTTP 402** quando suspensa; front redireciona para `/billing`.
- **Check-in Kids:** pré-check-in → check-in → notificação push ao responsável → retirada segura (token/PIN).
- **Comunicação:** campanha/segmento → entregas via canal (WhatsApp) → scheduler processa filas.

---

## 8. Banco de Dados

### Tecnologias
- **EF Core 9** (Code First). Provider de produção **PostgreSQL** (Npgsql); SQL Server alternativo; **SQLite** em testes.
- Seleção de provider em runtime por `Database:Provider`.

### Estratégia de acesso
- **Repository pattern** + `IUnitOfWork`. `SistemaIgrejaDbContext` com 130+ `DbSet`.
- **Multi-tenancy:** global query filter por `TenantId` (loop genérico no `OnModelCreating`, ~linha 2248) + **carimbo automático** em `SaveChanges`/`SaveChangesAsync` (`StampTenantId()` — qualquer `ITenantEntity` no estado `Added` com `TenantId == 0` recebe `CurrentTenantId`). Vale para API **e** Worker (mesmo DbContext). `CurrentTenantId` vem de `ITenantContext` (`HttpTenantContext` na API; `TenantScopeOverride` no Worker).
- **Auditoria:** `AuditSaveChangesInterceptor` grava `AuditLog`.

### Migrations
- 40+ migrations em `Infrastructure/Migrations/`. `Database:RunMigrations = true` aplica no startup.
- **Nomenclatura:** `{timestamp}_{NomeEmPortugues}.cs` (ex.: `20251211042104_InitialCreate`, `20260216205904_AdicionarPerfisAcesso`, `20260618213103_AdicionarTenantIdComunicacaoNotificacoes`).
- **Nota dev:** o guard de `Jwt:Key` faz `dotnet ef` exigir env vars locais → rodar migrations com `Jwt__Key='...' ConnectionStrings__DefaultConnection='Host=localhost;...' dotnet ef ...`. Há `BackEnd/commit_migration_postgresql.sh`.

### Padrões SQL / convenções de nomenclatura
- Paginação server-side (`page`/`pageSize` com teto, ex. 200); ordenação dinâmica.
- Índices únicos compostos por tenant: `(TenantId, Email)`, `(TenantId, Nome)`.
- Nomes de tabela/coluna seguem as entidades (Português, plural para tabelas via convenção EF).

---

## 9. Infraestrutura

### Ambientes
- **Development** — API local (`http://localhost:7000` / `127.0.0.1:5013`), front em `localhost:5174` (admin) / `5173`. Em dev o admin aponta uploads para produção (`VITE_UPLOADS_BASE_URL=https://api.kingdombr.com.br`) para compartilhar storage/DB.
- **Production** — API em `https://api.kingdombr.com.br`; admin/portal em Azure SWA; domínio-alvo do produto: `verboplus.com.br` / `app.verboplus.com.br`.

### Estratégia de deploy
- **Backend (API + Worker):** containers Docker no **Coolify**. Imagens base `mcr.microsoft.com/dotnet/sdk:10.0` (build) → `aspnet`/`runtime:10.0` (runtime). Worker copia o repo inteiro e publica só o projeto Worker (evita restaurar `tests`).
- **FrontEnd admin:** `azure-pipelines.yml` — Node 20, pnpm, `pnpm run test` (bloqueia deploy se falhar) → `pnpm run build` → deploy Azure Static Web App (grupo de vars `SWA_AppIgreja_Admin_Variables`).
- **Portal:** Azure DevOps pipeline → `npm run build` → `staticwebapp.config.json` → `AzureStaticWebApp@0`. Node 18, triggers `main`/`master`.
- **Backend CI (`BackEnd/azure-pipelines.yml`):** trigger `main`, .NET SDK 10.x, `restore`/`build`/`publish` → artefato `app.zip`. (Deploy efetivo dos containers é via Coolify.)
- **VerboPlus / CadastroMembro:** build estático (Vite / HTML), hosting estático.

### Containers
- `BackEnd/src/SistemaIgreja.API/Dockerfile`, `BackEnd/SistemaIgreja.BackgroundWorker/Dockerfile`, `BackEnd/docker-compose.evolution.yml` (Evolution API local).

### Secrets management
- **Variáveis de ambiente no Coolify** (API + Worker). `appsettings.json` mantém segredos **vazios** (lidos de env vars). Override por `__` (ex.: `Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Billing__Asaas__ApiKey`, `Sentry__Dsn`).
- Guard de startup recusa subir com `Jwt:Key` vazia ou placeholder.
- **Histórico:** segredos reais já estiveram versionados no git e foram **rotacionados** (Postgres, Evolution, JWT) em 2026-06-12 — valores antigos do histórico estão inúteis.

### Monitoramento
- **Sentry** (erros) + **health checks** (`/health` — DB e configs). `SchedulerExecutionMonitor` monitora execução dos jobs.

---

## 10. Segurança

### Autenticação
- **JWT Bearer** HS256, `ValidateIssuer/Audience/Lifetime/IssuerSigningKey`, `ClockSkew=0`, expiração 1h. Senhas com **BCrypt**.
- **Login lockout:** `LoginLockout` (5 tentativas / 15 min), campos `Usuario.TentativasLoginFalhas` / `BloqueadoAte`.
- **Rate limiting:** política `signup` (5/min por IP) e `login` (10/min por IP) via `AddRateLimiter`. (Login ainda sem proteção distribuída por IP — ver gaps.)
- **Política de senha:** 8+ chars com maiúscula+minúscula+número (`PasswordPolicy`).
- **MFA:** **não implementado** (adiado para pós-lançamento).

### Autorização
- **RBAC** próprio: `PerfilAcesso` + `PerfilAcessoPermissao` (recurso × ação) + `PessoaPerfil` (vínculo com vigência).
- **`PermissionMiddleware`** em `/api/*` mapeia path→recurso e método→ação (`PermissionResourceMap`); `IsPlatformAdmin` faz bypass; nega com **403**.
- **`SubscriptionGatingMiddleware`** bloqueia tenants com assinatura suspensa (**402**).
- Pipeline: `UseAuthentication` → `UseAuthorization` → `PermissionMiddleware` → `SubscriptionGatingMiddleware`.

### Gestão de segredos
- Env vars no Coolify; `appsettings` sanitizado; ver §9.

### Políticas observadas (LGPD)
- Consentimento **versionado** (`ConsentimentoRegistro.VersaoDocumento`, constante `TERMOS_VERSAO`/`v1` no front e CadastroMembro).
- Exportação, **anonimização (não exclusão física)**, solicitações do titular (`SolicitacaoTitular` / `SolicitacoesTitularController`).
- Sentry com `SendDefaultPii=false`. Termos/Política em `legal/` (modelos — **pendente revisão jurídica e preenchimento de razão social/CNPJ/DPO**).
- Papéis: **Igreja = Controladora**, **VerboPlus = Operadora**.

### Gaps de segurança conhecidos (de SAAS_READINESS.md)
- Webhook Asaas sem HMAC; uploads em disco servidos sem auth (URL previsível, possível path traversal em galerias); Swagger UI público em produção; limites de plano (`MaxUsuarios`/`MaxMembros`) existem mas **não bloqueiam**; porta 5433 do Postgres exposta publicamente; schedulers duplicados sem lock.

---

## 11. Decisões Arquiteturais Detectadas

| Decisão | Motivação provável | Vantagens | Possíveis impactos |
|---|---|---|---|
| **Clean Architecture (4 camadas)** | Testabilidade e separação de responsabilidades | Domínio isolado, fácil de testar (209 arquivos de teste) | Mais boilerplate; mapeamento manual de DTOs |
| **Multi-tenancy tenant-per-row + global filter + carimbo no SaveChanges** | Isolar dados de igrejas no mesmo banco com baixo custo operacional | Simples de operar (1 banco), rede de segurança contra vazamento | Risco se entidade nova não implementar `ITenantEntity`; sem isolamento físico |
| **DTO + mapeamento manual (sem AutoMapper)** | Controle explícito, evitar mágica | Previsível, sem surpresas de reflection | Verboso, mapper por entidade |
| **EF Core multi-provider (PG/SQLServer/SQLite)** | PG em prod, SQLite acelera testes | Testes rápidos e isolados | Diferenças de SQL entre providers a vigiar |
| **Worker separado da API** | Escalar jobs independentemente | Isolamento de carga | **Registro de DI duplicado** (API ~152 vs Worker ~40) — drift já causou bug; mitigado com `ValidateOnBuild` |
| **Schedulers via BackgroundService com jitter** | Evitar broker dedicado, simplicidade | Sem infra extra de mensageria | Rodam na API **e** no Worker sem lock → risco de envio duplicado |
| **Frontends separados (admin/portal/landing) consumindo 1 API** | Públicos e ciclos de deploy distintos | Deploys independentes | Múltiplos repos/pipelines a manter |
| **Billing + doações via Asaas** | Gateway nacional (PIX/Boleto/Cartão) | Cobertura do mercado BR | Lock-in no Asaas |
| **Config-driven com kill-switch (DSN/Email vazio = off)** | Mesma imagem em todos os ambientes | Sem branches de config | Esquecer de setar = feature silenciosamente off |
| **Segredos só em env vars (Coolify)** | Pós-incidente de segredos no git | Imagem/genérica reaproveitável | Depende de gestão correta no Coolify |

---

## 12. Restrições do Projeto

- **Padrão obrigatório de tenant:** **toda nova entidade de negócio DEVE implementar `ITenantEntity`** (caso contrário vaza entre igrejas). Globais são exceção consciente e curta (Tenant, TenantDomain, Plano, EventoWebhookBilling, VerificacaoEmail).
- **.NET 10 / EF Core 9** no backend; **testes ficam em .NET separado** — o Dockerfile do Worker evita restaurar `tests`. `TODO: confirmar com o time` se o projeto de testes ainda é .NET 9 (comentário no Dockerfile sugere isso).
- **Domínio em Português** — manter naming consistente.
- **Sem AutoMapper** — mapeamento manual é a norma.
- **Segredos nunca em `appsettings`/git** — somente env vars.
- **Política de senha** aplicada em todos os pontos de criação/troca de senha (backend é a fonte; front espelha).
- **Sentry sem PII** (`SendDefaultPii=false`) — não logar dados pessoais.
- **FrontEnd admin usa pnpm** (lockfile pnpm); CI bloqueia deploy se testes falharem.
- **Tecnologias não observadas / aparentemente fora de escopo:** Redis/cache distribuído, broker de mensageria, AutoMapper, MFA. `TODO: confirmar com o time` se são proibidas ou apenas ainda não adotadas.

---

## 13. Checklist para Novas Funcionalidades

1. **Domínio:** criar/alterar a entidade em `Domain/Entities/`. **Implementar `ITenantEntity`** se for dado de igreja.
2. **Migration:** `dotnet ef migrations add {NomeEmPortugues}` (com env vars de Jwt/Connection — ver §8). Revisar SQL gerado (PG).
3. **Repository:** `I{X}Repository` (Application/Interfaces ou onde o padrão estiver) + `{X}Repository` (Infrastructure/Repositories), filtrando por tenant e com paginação `(Items, Total)`.
4. **DTOs:** `{X}Dto`, `Create{X}Dto`, `Update{X}Dto`, `{X}PagedQueryDto` em `Application/DTOs/`.
5. **Service:** `I{X}Service` + `{X}Service` com mapeamento manual, `ILogger<T>`, `IUnitOfWork`. Validar regras e lançar exceções semânticas.
6. **DI:** registrar repo e service **na API E no Worker** se o job depender deles (atenção ao fechamento transitivo — drift quebra no startup do Worker via `ValidateOnBuild`).
7. **Controller:** `{X}Controller` `[ApiController]`/`[Authorize]`, `ActionResult<T>`, async. Mapear recurso no `PermissionResourceMap` (RBAC).
8. **Permissões:** garantir cobertura no RBAC e, se preciso, semente de permissões em `PerfilAcesso`.
9. **Testes backend:** xUnit + Moq + FluentAssertions (padrão AAA); incluir teste de isolamento de tenant quando aplicável (`TenantQueryFilterTests`).
10. **Frontend:** módulo em `api/`, página(s) em `pages/`, usar shadcn/ui, `react-hook-form` + Zod, proteger rota com `ProtectedRoute`/`RequirePermission`; i18n (pt-BR/en-US/es-ES).
11. **Testes frontend:** Vitest + RTL (cada bug vira regressão).
12. **Observabilidade:** logs/erros via `ILogger`/Sentry; sem PII.

---

## 14. Checklist para Novas Integrações

1. **Config tipada:** criar classe em `Application/Configuration/` com `SectionName`; registrar via `builder.Services.Configure<T>(...)`. Seção correspondente em `appsettings.json` com **segredos vazios** (vêm de env var no Coolify).
2. **Cliente HTTP:** registrar com `AddHttpClient<IServico, ServicoImpl>()`; implementar **retry + timeout** (padrão Evolution/Asaas).
3. **Autenticação:** API Key/token via header; nunca hardcode — env var. Para webhooks, validar token (e idealmente HMAC/assinatura).
4. **Kill-switch:** integração desligada quando credencial vazia (padrão Sentry/Email).
5. **Health check:** adicionar `AddCheck<XConfigurationHealthCheck>(...)` se a integração for crítica.
6. **Worker vs API:** decidir onde a integração roda; se usada por scheduler, registrar no Worker também.
7. **Observabilidade:** logar falhas (Sentry), sem expor PII.
8. **Documentar** no `SAAS_READINESS.md` se for bloqueador de produção (ex.: requer conta/credencial em produção).
9. **Testes:** mockar o cliente HTTP; testar caminhos de erro/retry.

---

## 15. Checklist para Novas Entidades

1. Criar classe em `Domain/Entities/` com `int TenantId` e `: ITenantEntity` (salvo se for global — justifique).
2. Adicionar `DbSet<>` no `SistemaIgrejaDbContext` e configuração em `OnModelCreating` (índices únicos por `(TenantId, ...)`, relacionamentos, `MaxLength`).
3. Confirmar que o **global query filter por tenant** cobre a entidade (loop genérico) e que o **carimbo automático** no `SaveChanges` se aplica.
4. Gerar migration (`Adicionar{Entidade}`) com backfill de `TenantId` se houver dados existentes.
5. DTOs + Repository + Service + Controller (ver §13). Auditoria via `AuditLog` se for dado sensível.
6. RBAC: mapear recurso/ação no `PermissionResourceMap`.
7. LGPD: se contém dado pessoal, considerar exportação/anonimização e consentimento.
8. Teste de isolamento de tenant (`TenantQueryFilterTests`).

---

## 16. Dúvidas e Pontos Pendentes

- `TODO: confirmar com o time` — Existe cache distribuído (Redis) em algum ambiente? Não há evidência no código.
- `TODO: confirmar com o time` — O projeto de testes (`SistemaIgreja.API.Tests`) ainda é .NET 9? O Dockerfile do Worker menciona evitar restaurar `tests` "que está em .NET 9", mas o `.csproj` analisado aponta `net10.0`.
- `TODO: confirmar com o time` — `BackEnd/azure-pipelines.yml` gera artefato `app.zip`, mas o deploy de produção da API parece ser via Coolify (Docker). Qual é o pipeline efetivo de deploy do backend?
- `TODO: confirmar com o time` — Lock distribuído para schedulers: hoje rodam na API **e** no Worker (`Scheduler:Enabled=true` em ambos) sem lock — risco de envio duplicado. Qual a decisão definitiva (rodar só no Worker?).
- `TODO: confirmar com o time` — Endpoints públicos consumidos pelo Portal (`/api/events`, `/api/ministries`, `/api/church/info`, etc.) usam nomes em inglês na doc do Portal, enquanto o backend usa controllers em português. Confirmar o mapeamento real (rotas/aliases) — pode haver divergência entre a doc do Portal e a API atual.
- `TODO: confirmar com o time` — Status de produção do **AppKids** (memória indica "só iOS/macOS", sem Android publicado). Plataformas-alvo oficiais?
- `TODO: confirmar com o time` — Preços definitivos dos 3 planos (seed usa placeholders R$49,90 / R$99,90 / R$199,90).
- `TODO: confirmar com o time` — Revisão jurídica dos documentos em `legal/` (razão social, CNPJ, DPO, foro) antes de publicar.
- `TODO: confirmar com o time` — `evolution-api/` no repo contém apenas `.env`; a instância Evolution é gerida fora deste repo (Coolify/Docker). Confirmar onde vive a infra dessa instância.

---

### Anexos úteis (fontes de verdade vivas)
- **[.claude/INTEGRATION_PATTERNS.md](.claude/INTEGRATION_PATTERNS.md)** — referência oficial de padrões de integração (clients, auth, webhooks, schedulers, kill-switch, checklists).
- **[.claude/CODING_STANDARDS.md](.claude/CODING_STANDARDS.md)** — padrões reais de código do backend.
- **[SAAS_READINESS.md](../docs/SAAS_READINESS.md)** — bloqueadores e roadmap de lançamento (manter atualizado).
- **`legal/`** — Termos de Uso e Política de Privacidade (v1).
- **`FrontEnd/CONFIGURACAO_BACKEND_IMAGENS.md`** e **`FrontEnd/ENDPOINT_LISTAGEM_FOTOS.md`** — notas de integração de imagens/galerias.
- Documentos `KIDS_*`, `COMUNICACAO_*`, `ADMIN_REDESIGN_*`, `MULTITENANCY_ROADMAP.md` na raiz — histórico de planejamento por módulo (contexto de produto, não de estado atual).
