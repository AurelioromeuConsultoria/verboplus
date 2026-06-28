# CODING_STANDARDS.md

> **Padrões de código reais do backend AppIgreja / VerboPlus (`BackEnd/`).**
> Este documento descreve as convenções **observadas repetidamente no código atual**, não boas práticas teóricas. Serve como referência oficial para novas conversas, agentes e automações.
>
> Regras deste documento:
> - Cada padrão tem **referência de arquivo/linha** ou exemplo extraído do código.
> - Onde há **padrões conflitantes**, identifica-se o **predominante**, as **exceções** e o **provável motivo**.
> - Onde não há evidência suficiente para um padrão dominante: `TODO: confirmar com o time`.
> - Complementa, não substitui, o [.claude/PROJECT_CONTEXT.md](.claude/PROJECT_CONTEXT.md) (visão de arquitetura e negócio).
> - Escopo: **backend .NET** (`BackEnd/src` + `BackEnd/SistemaIgreja.BackgroundWorker` + `BackEnd/tests`). Frontends têm convenções próprias resumidas no PROJECT_CONTEXT (§5).
> - Última análise: **2026-06-27**.

---

## 1. Convenções Gerais

### Estilo de codificação predominante
- **C# / .NET 10**, nullable reference types habilitado.
- **File-scoped namespaces** em entidades e DTOs (`namespace SistemaIgreja.Domain.Entities;`).
- **Auto-properties** `{ get; set; }` em toda parte; `{ get; init; }` reservado a objetos de query/resultado (`PagedResultDto<T>`, `*PagedQuery`).
- **`record` não é usado** para DTOs nem entidades — sempre `public class`.
- **100% async/await** na stack de dados e integrações.

### Idioma (regra forte e consistente)
- **Domínio em Português**: entidades, propriedades, DTOs, mensagens de erro, logs de negócio, nomes de migration.
- **Infraestrutura/técnico em Inglês**: nomes de método de teste (`MethodName_Scenario_Expected`), termos de framework, sufixos `Repository`/`Service`/`Dto`.
- Mensagens de erro e validação **sempre em Português** (`"Email já cadastrado"`, `"Nome é obrigatório"`).

### Convenções de nomenclatura
| Tipo | Padrão | Exemplo |
|---|---|---|
| Controller | `{Entidade}Controller` | `PessoasController` |
| Service | `{X}Service : I{X}Service` | `PessoaService` |
| Repository | `{X}Repository : I{X}Repository` | `PessoaRepository` |
| DTO leitura | `{X}Dto` | `PessoaDto` |
| DTO criação | `Criar{X}Dto` (predominante) / `Create{X}Request` (recente) | `CriarPessoaDto` |
| DTO atualização | `Atualizar{X}Dto` | `AtualizarReceitaDto` |
| Query paginada | `{X}PagedQueryDto` (entrada controller) + `{X}PagedQuery` (objeto interno) | `PessoaPagedQueryDto` |
| Config | `{X}Settings` (predominante) / `{X}Options` | `EmailSettings`, `FirebaseKidsPushOptions` |
| Migration | `{timestamp}_{NomeEmPortuguês}` | `20260627001300_Pessoa_AddFotoUrl` |

### Organização de projetos (Clean Architecture — 4 camadas + Worker)
```
SistemaIgreja.Domain          → Entidades + ITenantEntity. Sem dependências internas.
SistemaIgreja.Application      → DTOs, Interfaces (I{X}Service E I{X}Repository), Services,
                                 Configuration (Settings), Security, Utils, JsonConverters.
                                 Depende de: Domain.
SistemaIgreja.Infrastructure   → DbContext, Repositories (impl.), Migrations, Services de infra
                                 (Asaas, Evolution, SMTP, S3, Audit, schedulers).
                                 Depende de: Domain + Application.
SistemaIgreja.API              → Controllers, Middleware, Permissions, Swagger, HealthChecks.
                                 Depende de: Application + Infrastructure.
SistemaIgreja.BackgroundWorker → Host genérico standalone (schedulers).
tests/SistemaIgreja.API.Tests  → Testes (API + Application + Domain).
```

> **Detalhe importante:** as **interfaces de repositório (`I{X}Repository`) vivem em `Application/Interfaces/`**, junto com as de serviço; só a **implementação** fica em `Infrastructure/Repositories/`. Isso vale para todos os ~59 repositórios.

### Organização de namespaces
- Seguem a estrutura de pastas: `SistemaIgreja.{Camada}.{Pasta}`.
- DTOs por domínio quando o domínio tem paginação/complexidade: subpastas `DTOs/Pessoas/`, `DTOs/Visitantes/`, `DTOs/MensagensAgendadas/`, `DTOs/Auditoria/`, `DTOs/Search/`. Domínios simples ficam na raiz de `DTOs/`.

---

## 2. Convenções de Classes

### Controllers
- **Localização:** `BackEnd/src/SistemaIgreja.API/Controllers/` (~64 controllers).
- **Nomenclatura:** `{Entidade}Controller` (plural no recurso: `/api/pessoas`).
- **Declaração padrão:**
  ```csharp
  [ApiController]
  [Route("api/[controller]")]
  [Authorize] // ou [AllowAnonymous] para controllers públicos
  public class PessoasController : ControllerBase
  ```
  Ref.: [PessoasController.cs:12-15](BackEnd/src/SistemaIgreja.API/Controllers/PessoasController.cs#L12-L15).
- **Responsabilidades:** apenas superfície HTTP — receber requisição, chamar o service, **traduzir exceções em status HTTP**, montar resposta. **Sem regra de negócio** no controller.
- **Dependências permitidas:** `I{X}Service`, `ILogger<T>`, `ICurrentUserContext`, ocasionalmente `I{X}Repository` (ex.: `IUsuarioRepository` para resolver `PessoaId`). KidsController injeta 11 services especializados — [KidsController.cs:15-51](BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs#L15-L51).
- **Dependências proibidas:** `DbContext`, EF Core, clientes HTTP de integração diretos. O acesso a dados passa por service/repository.
- **Exceções de convenção:** `WebhooksBillingController` usa rota absoluta `[HttpPost("/api/webhooks/billing/asaas")]` sem `[Route]` de classe e `[AllowAnonymous]` — [WebhooksBillingController.cs:23](BackEnd/src/SistemaIgreja.API/Controllers/WebhooksBillingController.cs#L23). Motivo: caminho fixo de webhook externo.

### Services (Application)
- **Localização:** `BackEnd/src/SistemaIgreja.Application/Services/` (~83). **Interface e implementação no mesmo arquivo.**
- **Nomenclatura:** `{X}Service : I{X}Service`.
- **Responsabilidades:** casos de uso, regras de negócio, validação, mapeamento DTO↔entidade, orquestração de transações.
- **Dependências permitidas:** `I{X}Repository` (injetados **diretamente**, não via `IUnitOfWork`), `ILogger<T>`, `ITenantContext`, `IUnitOfWork` (quando há transação multi-passo), outros `I{X}Service`, `IConfiguration`/`IOptions<T>`.
- **Dependências proibidas:** `DbContext` direto (vai pelo repository), `HttpContext` (vem via `ICurrentUserContext`).
- **Padrão de construtor dual** (compatibilidade + testes): serviços que dependem de tenant frequentemente têm dois construtores, um sem `ITenantContext` que delega ao outro com `new DefaultTenantContext()` — [PessoaService.cs:33-60](BackEnd/src/SistemaIgreja.Application/Services/PessoaService.cs#L33-L60). Aparece em `PessoaService`, `VisitanteService`, `KidsRetiradaService`, `MembroCadastroService`.

### Repositories (Infrastructure)
- **Interface:** `Application/Interfaces/I{X}Repository.cs`. **Implementação:** `Infrastructure/Repositories/{X}Repository.cs` (~59).
- **Não existe `Repository<T>` genérico** — cada repositório é autocontido.
- **Construtor padrão dual:**
  ```csharp
  public PessoaRepository(SistemaIgrejaDbContext context)
      : this(context, new DefaultTenantContext()) { }

  public PessoaRepository(SistemaIgrejaDbContext context, ITenantContext tenantContext)
  { _context = context; _tenantContext = tenantContext; }
  ```
  Ref.: [PessoaRepository.cs:15-24](BackEnd/src/SistemaIgreja.Infrastructure/Repositories/PessoaRepository.cs#L15-L24).
- **Dependências permitidas:** `SistemaIgrejaDbContext` (direto, nunca abstraído), `ITenantContext`.
- **Responsabilidades:** queries LINQ, paginação, filtros/ordenação dinâmicos, atribuição de `TenantId` na criação. **Sem regra de negócio.**
- **Inconsistência conhecida:** a ordem dos parâmetros de construtor varia (alguns `DbContext` primeiro, outros `ITenantContext` primeiro). Predominante: **`DbContext` primeiro**.

### Entities (Domain)
- **Localização:** `BackEnd/src/SistemaIgreja.Domain/Entities/` (77 entidades + `ITenantEntity.cs`).
- **Sem classe base** — entidades implementam interface, não herdam.
- **Chave primária:** sempre `public int Id { get; set; }`. Sem `Guid`.
- **Multi-tenant:** entidade de negócio implementa `ITenantEntity`:
  ```csharp
  public interface ITenantEntity { int TenantId { get; set; } }
  ```
  Ref.: [ITenantEntity.cs](BackEnd/src/SistemaIgreja.Domain/Entities/ITenantEntity.cs). Implementação típica:
  ```csharp
  public class Pessoa : ITenantEntity
  {
      public int Id { get; set; }
      [Required] public int TenantId { get; set; }
      public virtual Tenant Tenant { get; set; } = null!;
      [Required, MaxLength(100)] public string Nome { get; set; } = string.Empty;
      public string? Email { get; set; }
      // ...
  }
  ```
- **Data Annotations nas entidades** (`[Required]`, `[MaxLength]`) — **não há configuração Fluent API** de validação no `OnModelCreating`; índices e relacionamentos sim ficam no `OnModelCreating`.
- **Navegações:** opcionais como `public virtual T? Prop`; obrigatórias como `public virtual T Prop { get; set; } = null!;`; coleções inicializadas `= new List<T>()`.
- **Globais (sem `ITenantEntity`):** `Tenant`, `TenantDomain`, `Plano`, `EventoWebhookBilling`, `VerificacaoEmail` — exceção consciente.

### DTOs
- **Localização:** `BackEnd/src/SistemaIgreja.Application/DTOs/` (~278 DTOs).
- **Sempre `class`** (nunca `record`). Validação por **DataAnnotations** (`[Required]`, `[EmailAddress]`, `[Range]`, `[MaxLength]`, `[MinLength]`) com mensagens em Português.
- **Sem regra de negócio no DTO** — só forma + validação de formato.
- **Genérico de paginação** (único lugar com `required`+`init`):
  ```csharp
  public class PagedResultDto<T>
  {
      public required IReadOnlyList<T> Items { get; init; }
      public required int Total { get; init; }
      public required int Page { get; init; }
      public required int PageSize { get; init; }
      public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
  }
  ```
  Ref.: [PagedResultDto.cs](BackEnd/src/SistemaIgreja.Application/DTOs/PagedResultDto.cs).
- **Inconsistência conhecida:** convivem nomes legados (`Criar{X}Dto`/`Atualizar{X}Dto`) e recentes (`Create{X}Request`/`{X}Response`) — ex. [VisitanteDto.cs](BackEnd/src/SistemaIgreja.Application/DTOs/Visitantes/VisitanteDto.cs) com comentário `// DTOs legados mantidos para compatibilidade`. **Predominante e recomendado para código novo: `Criar{X}Dto` / `Atualizar{X}Dto`.**

### Value Objects
- `TODO: confirmar com o time` — Não há value objects formais (sem `record struct`/tipos imutáveis de domínio). Conceitos como status são **enums**, não VOs.

### Interfaces
- `I{X}Service` e `I{X}Repository` em `Application/Interfaces/` (~77). Interfaces de integração também (`IAsaasBillingClient`, `IEvolutionApiService`).
- Registradas no DI **inline em cada `Program.cs`** (API ~152, Worker ~40). **Não há módulo de DI compartilhado** — registrar em ambos quando o Worker usar o serviço.

### Extensions / Helpers / Factories / Builders / Mappers
- **Mappers:** **manuais**, métodos `private static MapToDto(...)` dentro do próprio service. **Sem AutoMapper** (confirmado ausente em todas as camadas). Ver §6.
- **Helpers:** métodos `private static` no próprio service (ex. `IsValidEmail`, `NormalizarTelefone`, `GetStatusDescricao`). Não há projeto/pasta dedicada de helpers de domínio.
- **Factories/Builders:** `TODO: confirmar com o time` — não há factories/builders de domínio; em testes usam-se métodos auxiliares (`CriarUsuario()`), não classes factory.
- **Extensions:** uso pontual; sem convenção difundida de `static class {X}Extensions`. `TODO: confirmar com o time`.

---

## 3. Convenções de Métodos

### Async/await
- **Obrigatório** em acesso a dados e integrações. Métodos terminam em `Async` e retornam `Task`/`Task<T>`.
- EF Core sempre nas variantes async: `ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`, `AnyAsync`, `SaveChangesAsync`.

### Retorno de coleções
- Services retornam **DTOs**, nunca entidades: `Task<IEnumerable<{X}Dto>>` para listas, `Task<{X}Dto?>` para item opcional, `Task<PagedResultDto<{X}Dto>>` para paginado.
- Repositories retornam **entidades**; paginação como **tupla** `Task<(IReadOnlyList<T> Items, int Total)>`.

### Paginação (padrão consolidado)
- **Entrada:** `[FromQuery] {X}PagedQueryDto` (Page, PageSize, Sort, Direction, filtros).
- **Defaults e teto** aplicados no repositório:
  ```csharp
  var page = query.Page <= 0 ? 1 : query.Page;
  var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);
  var total = await q.CountAsync();
  var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
  return (items, total);
  ```
  Default **20**, teto **200**. Ref.: [PessoaRepository.cs:35-112](BackEnd/src/SistemaIgreja.Infrastructure/Repositories/PessoaRepository.cs#L35-L112).
- **Ordenação dinâmica:** `switch` case-insensitive sobre o nome do campo (`Sort` + `Direction`).
- **Saída:** service converte a tupla em `PagedResultDto<{X}Dto>`.

### Tratamento de null
- `??` para fallback (ex. `_tenantContext.TenantId ?? Tenant.InitialTenantId`).
- `?.` em navegações no mapeamento (`visitante.Pessoa?.Nome ?? string.Empty`).
- `?? throw new ArgumentException(...)` para “buscar ou falhar”.
- Tipos de retorno anuláveis (`Task<{X}Dto?>`) para lookups opcionais; controller traduz `null` em `NotFound()`.

### Nomenclatura de métodos
- Verbos em Inglês para CRUD técnico (`GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetPagedAsync`).
- Métodos de negócio em Português (`GerarProximaRecorrenciaAsync`, `ConfirmarAsync`, `RegistrarExcecaoAsync`, `AlterarSenhaAsync`).
- **Variantes transacionais:** `CreateWithoutSaveAsync` / `UpdateWithoutSaveAsync` (enfileiram sem `SaveChanges`) vs. `CreateAsync` / `UpdateAsync` (persistem imediatamente). Nem todo repositório implementa as `*WithoutSaveAsync` — só os usados em transações compostas.

### CancellationToken
- **Predominante: NÃO usado.** Controllers, services internos e repositórios **não** propagam `CancellationToken`.
- **Exceção:** apenas operações de integração externa (HTTP) o expõem como parâmetro opcional `CancellationToken cancellationToken = default` — ex. [AsaasPaymentService.cs:45](BackEnd/src/SistemaIgreja.Application/Services/AsaasPaymentService.cs#L45) e o loop de retry da Evolution. Poucos controllers de processamento longo o repassam (`ComunicacaoEntregasController.ProcessarPendentes`).
- `TODO: confirmar com o time` — adotar `CancellationToken` de ponta a ponta é desejado ou intencionalmente evitado?

### Padrões de exceção
- **Services lançam exceções semânticas**, controllers traduzem:
  | Exceção | Significado | HTTP no controller |
  |---|---|---|
  | `ArgumentException` | validação / não encontrado | 400 (ou 404 conforme contexto) |
  | `InvalidOperationException` | estado/regra inválida | 400 ou 409 (Conflict no signup) |
  | `UnauthorizedAccessException` | auth/permissão | 401 ou 403 |
  | `KeyNotFoundException` | recurso ausente | 404 |
- Mensagens sempre em Português. Services **não capturam** as próprias exceções de validação — propagam ao controller.
- **Exceção ao padrão:** `MembroCadastroService` retorna `CadastroMembroResultadoDto { Sucesso=false, Mensagem }` em vez de lançar — [MembroCadastroService.cs:62-80](BackEnd/src/SistemaIgreja.Application/Services/MembroCadastroService.cs#L62-L80). Padrão predominante (11/12 casos) é **lançar exceção**.

---

## 4. Convenções de Integrações

### Chamadas HTTP
- **Typed `HttpClient` via `HttpClientFactory`**: `builder.Services.AddHttpClient<IServico, ServicoImpl>()`. Ref.: [Program.cs:197-198](BackEnd/src/SistemaIgreja.API/Program.cs#L197-L198).
- Integrações ativas: `AsaasBillingClient`, `AsaasPaymentService`, `EvolutionApiService`.

### Autenticação
- **API Key em header**, valor vindo de `IOptions<{X}Settings>`:
  - Asaas: `request.Headers.Add("access_token", _settings.ApiKey)` ([AsaasBillingClient.cs:192](BackEnd/src/SistemaIgreja.Application/Services/AsaasBillingClient.cs#L192)).
  - Evolution: `_httpClient.DefaultRequestHeaders.Add("apikey", _settings.ApiKey)` ([EvolutionApiService.cs:34](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L34)).
  - Asaas Payment (PIX por tenant): chave passada por chamada a partir de `GivingProviderConfig`.
- **Nunca hardcode** — sempre config/env var.

### Serialização
- **`System.Text.Json` exclusivamente** (sem Newtonsoft).
- Mapeamento explícito com `[JsonPropertyName("...")]` nos DTOs de resposta (Asaas).
- Para APIs com formatos variáveis (Evolution v1/v2): `new JsonSerializerOptions { PropertyNameCaseInsensitive = true }` ([EvolutionApiService.cs:486](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L486)).

### Retries
- **Manual, sem Polly.** Loop `for (tentativa = 1; tentativa <= MaxRetries; tentativa++)` com **backoff exponencial** e cap, retentando só falhas transientes (5xx e 429). Ref.: [EvolutionApiService.cs:313-479](BackEnd/src/SistemaIgreja.Application/Services/EvolutionApiService.cs#L313-L479).
- Config em `EvolutionApiSettings` (`MaxRetries=3`, `RetryDelaySeconds=5`).
- **Clientes Asaas não retentam** — falham rápido retornando resultado de erro.

### Tratamento de erros
- **Result objects, não exceções**: integrações retornam DTOs com `Success` + `ErrorMessage` (`AsaasBillingResult`, `AsaasPaymentResult`). Falha vira `{ Success=false, ErrorMessage=... }` e é logada com `LogError`/`LogWarning`.
- **Exceção:** `SmtpEmailService` deixa a exceção propagar (não captura).

### Timeout
- Evolution: `_httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds)` (default 30s). Download de mídia usa timeout reduzido (`Math.Min(TimeoutSeconds, 15)`).
- Clientes Asaas usam o timeout default do `HttpClient` (100s).

### Kill-switch (padrão forte)
- Integração **desligada quando credencial vazia** → no-op, sem quebrar:
  - Asaas Billing: `Configurado => !string.IsNullOrWhiteSpace(_settings.ApiKey)` checado antes de cada chamada.
  - Email: `if (!_settings.Enabled) { log; return; }` ([SmtpEmailService.cs:31-35](BackEnd/src/SistemaIgreja.Infrastructure/Services/SmtpEmailService.cs#L31-L35)).
  - Firebase Push: `EnsureFirebaseApp()` retorna `false` sem credenciais.
  - Sentry: DSN vazio = desligado.

### Webhooks
- Validação **só por token** comparando header com `_asaasSettings.WebhookToken` (`StringComparison.Ordinal`) — [BillingService.cs:303-311](BackEnd/src/SistemaIgreja.Infrastructure/Services/BillingService.cs#L303-L311). **Sem HMAC** (hardening pendente).
- **Idempotência:** verifica `(paymentId, evento)` já processado e grava em `EventosWebhookBilling` (trilha de auditoria).
- Payload recebido como `JsonElement` (`[FromBody] JsonElement payload`).

### Configuração de integração (Options pattern)
- Classe `{X}Settings` em `Application/Configuration/` com `public const string SectionName` e defaults; secrets como `string.Empty`.
- Registro: `builder.Services.Configure<{X}Settings>(builder.Configuration.GetSection({X}Settings.SectionName))`.

### Logs
- `LogError(ex, "...")` em falha de chamada externa; `LogWarning` para falhas recuperáveis/rejeições. Sempre com contexto (`{DoacaoId}`, `{StatusCode}`, `{RequestUri}`). Sem PII.

### Sincronização
- Sem broker de mensageria. Jobs assíncronos via `BackgroundService` com jitter, na API **e** no Worker. Reserva concorrente de mensagens usa **`FOR UPDATE SKIP LOCKED`** (PostgreSQL) / `WITH (UPDLOCK, ROWLOCK)` (SQL Server) — único uso de `FromSqlRaw` ([MensagemAgendadaRepository.cs:174-197](BackEnd/src/SistemaIgreja.Infrastructure/Repositories/MensagemAgendadaRepository.cs#L174-L197)).

---

## 5. Convenções de Banco de Dados

### Tecnologia
- **EF Core 9, Code First**. Provider selecionado em runtime por `Database:Provider`: PostgreSQL (Npgsql, prod, com `EnableRetryOnFailure`), SqlServer (alt), SQLite (testes). Ref.: [Program.cs:33-61](BackEnd/src/SistemaIgreja.API/Program.cs#L33-L61).

### Nomenclatura de tabelas e colunas
- Seguem os nomes das **entidades em Português** (tabelas no plural por convenção EF: `Pessoas`, `Receitas`, `MensagensAgendadas`).
- Colunas = nomes das propriedades (Português).
- **Índices únicos compostos por tenant**: `(TenantId, Email)`, `(TenantId, Nome)`, `(TenantId, Ano, Tipo)` etc.

### Multi-tenancy (rede de segurança em duas camadas)
1. **Global query filter** aplicado por reflexão a toda `ITenantEntity` no `OnModelCreating`:
   ```csharp
   Expression<Func<TEntity, bool>> filter = e => IgnoreTenantFilters || e.TenantId == CurrentTenantId;
   modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
   ```
   Ref.: [SistemaIgrejaDbContext.cs:2246-2271](BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs#L2246-L2271).
2. **Carimbo de `TenantId` no `SaveChanges`/`SaveChangesAsync`**: qualquer `ITenantEntity` `Added` com `TenantId == 0` recebe `CurrentTenantId` — [SistemaIgrejaDbContext.cs:28-50](BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs#L28-L50). Vale para API e Worker.
- `CurrentTenantId` vem de `ITenantContext`; `IgnoreTenantFilters` permite lookups cross-tenant pontuais.
- **Inconsistência:** atribuição de tenant na criação tem dois estilos — `await ResolveTenantIdAsync()` (só `PessoaRepository`, com lookup por slug) vs. `_tenantContext.TenantId ?? Tenant.InitialTenantId` (predominante, ~57/59).

### Unit of Work
- `IUnitOfWork` é **facilitador de transação**, não agregador de repositórios. Métodos: `BeginTransactionAsync`, `CommitTransactionAsync`, `RollbackTransactionAsync`, `SaveChangesAsync`, `ExecuteInTransactionAsync(Func<Task>)` e `ExecuteInTransactionAsync<T>(Func<Task<T>>)`. Usa `CreateExecutionStrategy()` para resiliência. Ref.: [UnitOfWork.cs](BackEnd/src/SistemaIgreja.Infrastructure/Data/UnitOfWork.cs).
- Operações multi-passo: `await _unitOfWork.ExecuteInTransactionAsync(async () => { ...CreateWithoutSaveAsync...; await _unitOfWork.SaveChangesAsync(); })` — [KidsRetiradaService.cs:128-184](BackEnd/src/SistemaIgreja.Application/Services/KidsRetiradaService.cs#L128-L184). CRUD simples persiste direto via repositório, sem UoW.

### Migrations
- 40+ em `Infrastructure/Migrations/`. **Nomenclatura `{timestamp}_{NomeEmPortuguês}`** descritivo: `AdicionarKidsPreCheckins`, `AdicionarLoginLockout`, `Pessoa_AddFotoUrl`, `P1_Recorrencias_OrcamentoCategoria`.
- Estrutura EF padrão `Up()`/`Down()`; em nova tabela inclui PK, FKs e índices `(TenantId, ...)`.
- `Database:RunMigrations=true` aplica no startup.
- **Rodar local:** o guard de `Jwt:Key` exige env vars → `Jwt__Key='...' ConnectionStrings__DefaultConnection='Host=localhost;...' dotnet ef migrations add {Nome}`. Há `BackEnd/commit_migration_postgresql.sh`.

### Scripts SQL / Procedures
- **Sem procedures.** SQL bruto apenas no caso de locking concorrente acima. Tudo o mais é LINQ.

### Índices
- Definidos no `OnModelCreating` (Fluent), únicos compostos por `(TenantId, ...)` para isolar e indexar por tenant.

### Auditoria
- `AuditSaveChangesInterceptor` (EF `SaveChangesInterceptor`): coleta `Added/Modified/Deleted` (exceto o próprio `AuditLog`) e grava `AuditLog` com `EntityName`, `EntityId`, `Action` (`Create/Update/Delete`), usuário/IP (de `ICurrentUserContext`) e `ChangesJson`. Proteção contra recursão via `ConcurrentDictionary`. Ref.: [AuditSaveChangesInterceptor.cs](BackEnd/src/SistemaIgreja.Infrastructure/Data/AuditSaveChangesInterceptor.cs).

### Seed
- `TODO: confirmar com o time` — seed via `HasData` aparece em migrations antigas (financeiro); recentes são schema-only. Confirmar onde vive o seed de planos/permissões.

---

## 6. Convenções de DTOs e Mapeamentos

### Estratégia de mapeamento
- **Manual, sem AutoMapper.** Métodos `private static MapToDto(entidade)` no próprio service:
  ```csharp
  private static ReceitaDto MapToDto(Receita r) => new ReceitaDto
  {
      Id = r.Id, Descricao = r.Descricao, Valor = r.Valor,
      Status = r.Status, StatusDescricao = GetStatusDescricao(r.Status),
      CategoriaReceitaNome = r.CategoriaReceita?.Nome,
      // ...
  };
  ```
  Ref.: [ReceitaService.cs:232-263](BackEnd/src/SistemaIgreja.Application/Services/ReceitaService.cs#L232-L263).
- Quando o mapeamento precisa carregar relacionamentos, a variante é `private async Task<...> MapToResponseAsync(...)`.
- Múltiplos mappers por service quando há DTOs distintos (admin vs. público).
- Enums viram descrições via helpers `GetXDescricao(...)`; coleções mapeadas inline com `.Select(...)`.

### Localização e nomenclatura
- Ver §1 (tabela) e §2 (DTOs). Subpastas por domínio em `DTOs/` quando há paginação/complexidade.

### Padrões de conversão
- Entidade→DTO: no service (saída). DTO→Entidade: no service (`CreateAsync`/`UpdateAsync`), instanciando a entidade e atribuindo campo a campo, com `TenantId = _tenantContext.TenantId ?? Tenant.InitialTenantId`.
- Validação de formato: DataAnnotations no DTO. Validação de regra: manual no service.

---

## 7. Convenções de Logging

- **Framework:** `ILogger<T>` (Microsoft.Extensions.Logging). Erros para **Sentry** (`MinimumEventLevel=Error`, `SendDefaultPii=false`).
- **Formato: logging estruturado com placeholders** `{Nome}` (nunca interpolação de string):
  ```csharp
  _logger.LogInformation(
      "Pessoa criada. PessoaId={PessoaId} TipoPessoa={TipoPessoa} Ativo={Ativo}",
      created.Id, created.TipoPessoa, created.Ativo);
  ```
  Ref.: [PessoaService.cs:187-191](BackEnd/src/SistemaIgreja.Application/Services/PessoaService.cs#L187-L191).
- **Granularidade/níveis:**
  - `LogInformation` — eventos de negócio concluídos (login OK, criação, atualização).
  - `LogWarning` — validação falha, tentativa não autorizada, falha recuperável de integração.
  - `LogError(ex, ...)` — exceções e falhas críticas (sempre com a exceção como 1º argumento).
- **Contexto incluído:** sempre IDs de recurso e, quando relevante, `TenantId`, `UsuarioId`, `TipoUsuario`. Mensagens em Português (passado para concluído, gerúndio para em andamento).
- **Sem PII** nos logs/eventos (LGPD).

---

## 8. Convenções de Tratamento de Erros

- **Sem middleware global de exceção.** A tradução exceção→HTTP é feita **em cada action** com `try/catch` (padrão ubíquo). Sentry captura o que escapa.
- **Mapeamento padrão** (ver tabela em §3). Corpo de erro **sempre `{ message }`** (objeto anônimo), nunca string crua — o frontend depende disso:
  ```csharp
  catch (ArgumentException ex)        { return BadRequest(new { message = ex.Message }); }
  catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
  catch (Exception ex)                { return StatusCode(500, new { message = "Erro ao criar pessoa", error = ex.Message }); }
  ```
  Ref.: [PessoasController.cs:148-168](BackEnd/src/SistemaIgreja.API/Controllers/PessoasController.cs#L148-L168).
- **Exceptions customizadas:** `TODO: confirmar com o time` — não há hierarquia de exceções de domínio própria; usam-se as do BCL (`ArgumentException`, `InvalidOperationException`, `UnauthorizedAccessException`, `KeyNotFoundException`).
- **Códigos HTTP:** 200/201/204 sucesso; 400/401/403/404/409 erros de cliente; 402 gating de assinatura; 500 erro interno; 501 quando serviço opcional ausente.
- **Inconsistências observadas:**
  - `NotFound()` vazio (predominante) vs. `NotFound(new { message })` (Kids). Para código novo, preferir `{ message }`.
  - `StatusCode(403, ...)` vs. `Forbid()`/`Unauthorized()`. Predominante: `StatusCode(4xx, new { message })`.
  - Algumas actions retornam objeto anônimo (`new { url, path, ... }`, `new { processadas }`) em vez de DTO. Predominante: DTO.

---

## 9. Convenções de Testes

- **Localização:** `BackEnd/tests/SistemaIgreja.API.Tests/`, espelhando a origem (`Controllers/`, `Services/`).
- **Frameworks:** **xUnit** 2.9.2 + **Moq** 4.20.72 + **FluentAssertions** 8.6.0; SQLite in-memory para testes de repositório. Ref.: [SistemaIgreja.API.Tests.csproj](BackEnd/tests/SistemaIgreja.API.Tests/SistemaIgreja.API.Tests.csproj).
- **Nomenclatura de método:** `MethodName_Scenario_Expected` (Inglês para o resultado técnico; Português quando o comportamento é de domínio):
  - `LoginAsync_ThrowsUnauthorized_WhenUserDoesNotExist`
  - `AlterarSenhaAsync_UpdatesPassword_AndAudits`
  - `LoginAsync_BloqueiaAposMaxTentativas`
  - `GetPagedAsync_FiltersByStatusPublicoAndText`
- **AAA:** estrutura Arrange/Act/Assert **sem comentários** explícitos (layout implícito).
- **Mocking:** mocks como campos (`private readonly Mock<IAuthService> _mock = new();`), `Setup(...).ReturnsAsync(...)`/`.ThrowsAsync(...)`, `Verify(..., Times.Once)`, `It.IsAny<T>()`.
- **Asserções:** FluentAssertions — `result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(response)`, `await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("...")`.
- **Repositório/tenant:** testes com SQLite via helper `CreateContextAsync()`; incluir teste de isolamento de tenant quando aplicável.
- **Fixtures/helpers:** métodos auxiliares na própria classe de teste (`CriarUsuario()`, `SetUser(int)` para popular `ControllerContext.User`) — sem classes factory dedicadas.
- **CI:** o pipeline do FrontEnd bloqueia deploy se testes falharem (cada bug vira regressão). `TODO: confirmar com o time` — 2 testes date-dependent pré-existentes falham (ver memória do projeto).

---

## 10. Convenções de Configuração

### appsettings.json
- Seções por área: `ConnectionStrings`, `Database`, `Jwt`, `LoginLockout`, `Sentry`, `EvolutionApi`, `Email`, `Billing` (+`Asaas`), `Storage`, `Firebase`, `MessageScheduler`, `BirthdayCampaignScheduler`, `PublicAppUrl`, `Uploads`.
- **Secrets vazios** no arquivo; valores reais vêm de **env vars no Coolify**, override por `__` (`Jwt__Key`, `Billing__Asaas__ApiKey`, `Email__Password`, `Sentry__Dsn`).

### Classes de configuração
- `Application/Configuration/{X}Settings.cs`: `public const string SectionName`, propriedades com defaults sensatos, secrets `= string.Empty`. XML docs em algumas (`EvolutionApiSettings`).

### Injeção de dependência
- **Inline em `Program.cs`**, agrupada por categoria (Repositories → Services → Configure<T> → HttpClients → Storage). **Não há módulo compartilhado** entre API e Worker — duplicação consciente; drift quebra no startup do Worker via `ValidateOnBuild`.
- **Lifetimes:** `AddScoped` para repositórios/services (padrão); `AddSingleton` para componentes de processo (storage, `SchedulerExecutionMonitor`); `AddHttpClient<I,Impl>` para clientes; `Configure<T>` para options.
- **Guard de startup:** recusa subir com `Jwt:Key` vazia ou placeholder — [Program.cs:301-308](BackEnd/src/SistemaIgreja.API/Program.cs#L301-L308).

### Pipeline de middleware (ordem exata na API)
`UseSentry` → CORS custom → `UseRouting` → `UseCors` → Swagger (dev) → `UseStaticFiles` → `UseRateLimiter` → `UseAuthentication` → `UseAuthorization` → `SubscriptionGatingMiddleware` → `PermissionMiddleware` → `MapHealthChecks` → `MapControllers`. Ref.: [Program.cs](BackEnd/src/SistemaIgreja.API/Program.cs).

### Middlewares
- **Estilo convencional** (`RequestDelegate` + `InvokeAsync`/`Invoke`), **não `IMiddleware`**; dependências scoped resolvidas via parâmetros do `Invoke`.
- `PermissionMiddleware`: mapeia path→recurso (prefix match em `PermissionResourceMap`) e método→ação (`GET→view`, `POST/PUT/PATCH→edit`, `DELETE→delete`); `IsPlatformAdmin` faz bypass; nega com **403 sem corpo**. Pula `/api/auth`, `/api/upload`, OPTIONS, não-`/api`.
- `SubscriptionGatingMiddleware`: bloqueia tenant suspenso com **402** e corpo `{ error, message }`; isenta `/api/auth`, `/api/upload`, `/api/webhooks`, `/api/billing`; platform admin sempre passa; fail-open se não há tenant.

---

## 11. Convenções de Performance

- **Paginação server-side** com teto de 200 (§3) — evita listagens ilimitadas.
- **`AsNoTracking()` pragmático**: aplicado em leituras paginadas e checks de existência (`AnyAsync`); leituras de item único ficam tracked (prontas para update). Não é universal.
- **Locking concorrente** para reserva de mensagens (`FOR UPDATE SKIP LOCKED` / `WITH (UPDLOCK, ROWLOCK)`) — evita envio duplicado por múltiplos workers (§4/§5).
- **`EnableRetryOnFailure`** no Npgsql para resiliência a falhas transientes; `CreateExecutionStrategy()` no UoW.
- **Sem cache distribuído** (Redis/MemoryCache) no backend — `TODO: confirmar com o time` se é decisão ou lacuna.
- **Sem bulk insert dedicado** observado (sem `EFCore.BulkExtensions`); inserts via `AddRange`/`SaveChanges`. `TODO: confirmar com o time`.
- **Paralelismo:** não há uso difundido de `Task.WhenAll` para fan-out em services; fluxo é sequencial dentro de transações.

---

## 12. Convenções Proibidas ou Evitadas

Com base em ausência consistente no código:
- **AutoMapper** — ausente; mapeamento manual é a norma. **Não introduzir.**
- **Newtonsoft.Json** — ausente nas integrações; usa-se `System.Text.Json`.
- **Polly** — ausente; retry é loop manual.
- **`record` para DTOs/entidades** — não usado; sempre `class`.
- **Classe base de entidade / `Repository<T>` genérico** — não existem; entidades e repositórios são autocontidos.
- **`DbContext` em controller/service** — acesso a dados só via repository.
- **Secrets em `appsettings`/git** — proibido (pós-incidente de rotação 2026-06-12); só env vars.
- **Procedures / SQL bruto** — evitados; LINQ é o padrão (exceto locking concorrente).
- **PII em logs/Sentry** — proibido (`SendDefaultPii=false`).
- **Middleware global de exceção** — não adotado (tradução por action). `TODO: confirmar com o time` se é decisão definitiva.
- **Tecnologias fora de escopo (provavelmente):** Redis/cache distribuído, broker de mensageria, MFA. `TODO: confirmar com o time` se proibidas ou apenas não adotadas (ver PROJECT_CONTEXT §12).

---

## 13. Checklist para Novas Funcionalidades

1. **Entidade** (`Domain/Entities/`): `public int Id`, implementar `ITenantEntity` (`[Required] int TenantId` + `virtual Tenant`), Data Annotations (`[Required]`, `[MaxLength]`). Sem classe base.
2. **DbContext**: adicionar `DbSet<>` e config no `OnModelCreating` (índice único `(TenantId, ...)`, relacionamentos, `MaxLength`). Confirmar cobertura do global filter + carimbo de tenant.
3. **Migration**: `dotnet ef migrations add {NomeEmPortuguês}` (com env vars de Jwt/Connection). Revisar SQL gerado (PG). Backfill de `TenantId` se houver dados.
4. **DTOs** (`Application/DTOs/[Dominio]/`): `{X}Dto`, `Criar{X}Dto`, `Atualizar{X}Dto`, `{X}PagedQueryDto`. `class` + DataAnnotations em Português. Sem regra de negócio.
5. **Interface de repositório** em `Application/Interfaces/I{X}Repository.cs` + **impl.** em `Infrastructure/Repositories/{X}Repository.cs` (construtor `DbContext` + `ITenantContext`, paginação tupla `(Items, Total)` com teto 200, ordenação dinâmica, `AsNoTracking` em leitura paginada).
6. **Service** `{X}Service : I{X}Service`: injetar repos + `ILogger<T>` (+ `ITenantContext`/`IUnitOfWork` se preciso). Mapper `private static MapToDto`. Lançar exceções semânticas. Log estruturado com placeholders, sem PII.
7. **DI**: registrar repo e service **na API e no Worker** se houver job dependente.
8. **Controller** `{X}Controller`: `[ApiController]`, `[Route("api/[controller]")]`, `[Authorize]`, `ControllerBase`, `async ActionResult<T>`, `try/catch` traduzindo exceção→HTTP com corpo `{ message }`. `CreatedAtAction` no POST, `NoContent` no DELETE.
9. **Permissões**: mapear recurso no `PermissionResourceMap`; semente em `PerfilAcesso` se necessário.
10. **Testes** (`tests/`): xUnit + Moq + FluentAssertions, `MethodName_Scenario_Expected`; teste de isolamento de tenant quando aplicável.
11. **Observabilidade**: logs/erros via `ILogger`/Sentry; sem PII.

---

## 14. Checklist para Novas Integrações

1. **Config tipada** `{X}Settings` em `Application/Configuration/` com `const SectionName` e secrets `string.Empty`; seção em `appsettings.json` vazia (env var no Coolify).
2. **Cliente HTTP** typed: `AddHttpClient<I{X}, {X}Impl>()`; injetar `IOptions<{X}Settings>` + `ILogger<T>`.
3. **Auth**: API Key em header a partir da config; nunca hardcode. Webhook: validar token (idealmente também HMAC).
4. **Serialização** `System.Text.Json`; `[JsonPropertyName]` nos DTOs; `PropertyNameCaseInsensitive` se a API for inconsistente.
5. **Retry** manual com backoff exponencial só em falhas transientes (5xx/429); **timeout** explícito.
6. **Erros como result object** (`Success` + `ErrorMessage`), não exceção; logar `LogError`/`LogWarning` com contexto, sem PII.
7. **Kill-switch**: no-op quando credencial vazia/`Enabled=false`.
8. **Health check** (`AddCheck<...ConfigurationHealthCheck>`) se a integração for crítica.
9. **Worker vs API**: registrar no Worker também se usada por scheduler.
10. **Testes**: mockar o `HttpClient`/cliente; cobrir caminhos de erro/retry.
11. **Documentar** bloqueio de produção no `SAAS_READINESS.md` se exigir credencial/conta.

---

## 15. Checklist para Novas Entidades

1. `Domain/Entities/{X}.cs`: `public int Id`, `: ITenantEntity` (`[Required] int TenantId` + `virtual Tenant Tenant = null!`) salvo se global (justificar). Data Annotations.
2. Navegações: opcional `virtual T?`; obrigatória `virtual T = null!`; coleção `= new List<T>()`.
3. Enums no mesmo arquivo (ou arquivo próprio se compartilhado), armazenados como int.
4. `DbSet<>` + `OnModelCreating` (índice `(TenantId, ...)`, FKs, `MaxLength`).
5. Confirmar global query filter + carimbo de `TenantId` cobrindo a entidade.
6. Migration `Adicionar{X}` (backfill de `TenantId` se necessário).
7. DTOs + Repository + Service + Controller (ver §13). Auditoria automática via interceptor (nada extra, salvo exclusão do `AuditLog`).
8. RBAC: recurso/ação no `PermissionResourceMap`.
9. LGPD: se contém dado pessoal, considerar exportação/anonimização/consentimento.
10. Teste de isolamento de tenant.
11. **`DateTime`**: usar **`DateTime.UtcNow`** em código novo (ver pendência abaixo).

---

## 16. Dúvidas e Pendências

> `TODO: confirmar com o time` em cada item — não houve padrão único e claro no código.

- **`DateTime.Now` vs `DateTime.UtcNow`** — entidades/services antigos usam `DateTime.Now`; recentes (`AuthService`, `KidsRetiradaService`, `Tenant`, `Plano`) usam `DateTime.UtcNow`. Predominância em transição; **recomendado padronizar em `UtcNow`**. Confirmar regra oficial.
- **`CancellationToken`** — usado só em integrações HTTP, ausente no resto. Adotar de ponta a ponta ou manter como está?
- **Corpo de `NotFound`** — `NotFound()` vazio vs. `NotFound(new { message })`. Qual é o padrão oficial para o frontend?
- **Resposta de erro custom vs DTO** — algumas actions retornam objeto anônimo (`new { url, ... }`). Padronizar em DTO?
- **Exceptions de domínio** — usar só BCL ou criar hierarquia própria? Não há custom exceptions hoje.
- **Middleware global de exceção** — ausência é decisão definitiva ou lacuna?
- **Ordem de parâmetros do construtor de repositório** — `DbContext` primeiro vs `ITenantContext` primeiro variam. Fixar a ordem.
- **`ResolveTenantIdAsync` vs `_tenantContext.TenantId ?? Tenant.InitialTenantId`** — unificar atribuição de tenant na criação?
- **`*WithoutSaveAsync`** — padronizar presença em todos os repositórios ou só onde há transação?
- **Estratégia de `Include`** — inline vs. helper `WithFullIncludes`. Definir limiar/política.
- **Cache distribuído / bulk insert / paralelismo** — ausentes; são proibidos ou apenas não adotados? (ver PROJECT_CONTEXT §12).
- **Seed de dados** (planos, permissões) — onde vive? `HasData` antigo vs. inicialização em runtime.
- **Testes date-dependent falhando** — 2 testes pré-existentes falham; tratar/ignorar?
- **Projeto de testes .NET 9 vs 10** — Dockerfile do Worker menciona .NET 9, `.csproj` aponta `net10.0` (ver PROJECT_CONTEXT §16).

---

### Fontes
Documento derivado da análise direta de controllers, services, repositories, entidades, DTOs, classes de integração, middlewares, testes, `Program.cs`, `appsettings.json` e migrations em `BackEnd/`, cruzada com [.claude/PROJECT_CONTEXT.md](.claude/PROJECT_CONTEXT.md) e [SAAS_READINESS.md](SAAS_READINESS.md).
