# MIGRATION_RULES.md

> **Regras reais de migração do projeto AppIgreja / VerboPlus.**
> Documento de referência oficial para migrar sistemas legados, componentes antigos e integrações existentes para a arquitetura atual.
>
> **Princípio deste documento:** só registra **comportamentos observados em migrações reais já existentes no código**. Nada aqui é teoria ou estratégia proposta. Onde não há evidência suficiente, está marcado `TODO: confirmar com o time`.
>
> Convenções:
> - **Fatos verificados** vêm com caminho de arquivo de referência.
> - Documentos canônicos relacionados: [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md), [CODING_STANDARDS.md](CODING_STANDARDS.md), [INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md).
> - Última atualização da análise: **2026-06-27**.

---

## 0. O que foi efetivamente encontrado (resumo de evidências)

| Tipo de migração | Status no projeto | Evidência principal |
|---|---|---|
| **Provider de banco: SQL Server → PostgreSQL** | **Ocorreu** (cód. multi-provider mantido) | commit "feat: Migração para PostgreSQL"; migrations `InitialCreatePostgreSQL` / `Baseline_Postgres`; `Program.cs` (seleção dinâmica) |
| **Consolidação de entidades (DataSet-like → Entity central)** | **Ocorreu** | `20251212034213_RefatoracaoPessoaCentralizada.cs` (Visitantes/Voluntarios/Usuarios → `Pessoa` + `PessoaPerfil`) |
| **Tenantização de entidades existentes (backfill)** | **Ocorreu, recorrente** | `20260618213103_AdicionarTenantIdComunicacaoNotificacoes.cs` |
| **Strangler de módulo (Comunicação)** | **Em andamento** (documentado e parcialmente codado) | `COMUNICACAO_SPRINT1_MAPA_LEGADO.md` |
| **Migração de versão do .NET** | **Não observável no repo** | Todos os `.csproj` já em `net10.0` — sem framework legado coexistindo |
| **DataSets / DataTables / ADO.NET legado** | **Ausente** (nada a migrar) | Zero `DataSet`/`DataTable`/`SqlDataAdapter` em `BackEnd/` |
| **Stored procedures → ORM** | **Sem procedures** | Zero `CREATE PROCEDURE`/`EXEC`; acesso 100% EF Core LINQ |
| **Crystal Reports / relatórios legados** | **Ausente** | Nenhuma lib de relatório (iText/QuestPDF/Crystal/Dink) |
| **Biblioteca HTTP / JSON legada → moderna** | **Convenção, não migração observada** | `System.Text.Json` exclusivo; `HttpClient` typed; sem Newtonsoft/RestSharp/WebClient |

> **Importante:** o diretório raiz **não é um repositório git** (cada subprojeto tem o seu). Não há histórico git unificado para inspecionar. As evidências de migração vêm de: arquivos de migration EF Core, scripts SQL/shell versionados, mensagens de commit embutidas em scripts, e os documentos de planejamento `*_MAPA_LEGADO.md` / `MULTITENANCY_ROADMAP.md`.

---

## 1. Filosofia de Migração

A estratégia **predominante e comprovada** é **migração incremental com coexistência e preservação de dados**, executada via **migrations EF Core versionadas e idempotentes**. Não há "big bang".

Padrões concretos observados:

- **Migração por módulos / por sprint.** A tenantização e a refatoração do módulo de Comunicação foram fatiadas em PRs/sprints, com mapa de legado escrito antes de tocar no banco (`COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, linhas 97-101: "criar o dominio central de comunicacao sem acoplar ainda ao banco" → "migrar implementacoes concretas gradualmente, por sprint e por canal").
- **Strangler pattern explícito** para o módulo de Comunicação: o novo domínio central nasce **ao lado** do legado, que é mantido como "estrutura especializada no curto prazo" e absorvido por adaptadores depois (`COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, linhas 89-95: "nao criar segunda fila paralela", "reaproveitar o fluxo de `MensagemAgendada` como base de processamento", "manter ... como estruturas especializadas no curto prazo").
- **Coexistência tecnológica deliberada** no acesso a dados: após migrar para PostgreSQL, o código **não removeu** o suporte a SQL Server — mantém os dois providers selecionáveis em runtime (ver §11 e §12).
- **Idempotência** como regra de ouro das migrations de dados: backfills só tocam linhas ainda não migradas (`WHERE "TenantId" = 0`), e DDL bruto usa `IF NOT EXISTS` / `ON CONFLICT ... DO NOTHING` (`20260217122041_InitialCreatePostgreSQL.cs`, linhas 14-44; `20260618213103_AdicionarTenantIdComunicacaoNotificacoes.cs`, linha 72).

---

## 2. Objetivos das Migrações (recorrentes e observados)

- **Portabilidade / redução de lock-in de banco.** O objetivo declarado da maior migração foi trocar o provider (SQL Server → PostgreSQL) mantendo ambos suportados (`commit_migration_postgresql.sh`, linhas 39-46).
- **Multi-tenancy / cloud/SaaS readiness.** Sucessivas migrations adicionam `TenantId` a entidades que nasceram single-tenant, para isolar igrejas no mesmo banco (`MULTITENANCY_ROADMAP.md`; `20260618213103_AdicionarTenantIdComunicacaoNotificacoes.cs`).
- **Normalização / consolidação de modelo.** Eliminar duplicação de dados de pessoa (Visitante/Voluntário/Usuário tinham `Nome`/`Email` próprios) consolidando numa entidade central `Pessoa` (`20251212034213_RefatoracaoPessoaCentralizada.cs`).
- **Manutenção facilitada.** Nomes de migration em português descritivo deixam a intenção explícita (`RefatoracaoPessoaCentralizada`, `FormalizarSalasTurmasKids`, `EvoluirKidsNotificacoesAvisosReais`).

`TODO: confirmar com o time` — performance não aparece explicitamente como motivador de nenhuma migração observada (o uso de `FOR UPDATE SKIP LOCKED` é de concorrência, não de migração de performance).

---

## 3. Regras Gerais (observadas em migrações reais)

1. **Preservar os dados existentes.** Toda migração estrutural inclui a etapa de mover/backfill dos dados, nunca só DDL. Ex.: `RefatoracaoPessoaCentralizada` faz `INSERT ... SELECT` de Visitantes/Voluntarios/Usuarios para `Pessoas` antes de dropar colunas (`20251212034213_...cs`, linhas 78-146).
2. **Backfill é idempotente.** Sempre condicionado ao estado não-migrado (`WHERE "TenantId" = 0`), permitindo reexecução segura (`20260618213103_...cs`, linhas 73-97).
3. **Adicionar coluna nullable/com default → popular → tornar obrigatória.** Padrão de três passos para não quebrar dados existentes: coluna `PessoaId` entra `nullable: true`, é preenchida via SQL, depois vira `nullable: false, defaultValue: 0` (`20251212034213_...cs`, linhas 59-75 → 78-146 → 148-177). Para `TenantId`, entra direto com `defaultValue: 0` e é corrigida pelo backfill (`20260618213103_...cs`, linhas 13-18).
4. **Migrations reversíveis sempre que possível.** Ambas as migrations estruturais analisadas implementam `Down()` recriando colunas/índices removidos (`20251212034213_...cs`, linhas 281-393; `20260618213103_...cs`, linhas 101-134).
5. **Evitar duplicatas ao consolidar.** Os `INSERT ... SELECT` usam `WHERE NOT EXISTS (... p.Email = v.Email ...)` para deduplicar por e-mail ao fundir entidades (`20251212034213_...cs`, linhas 101-104, 126-129).
6. **Marcar o histórico de migration ao trocar de baseline.** Ao criar a baseline PostgreSQL, as migrations antigas (SQL Server) são inseridas em `__EFMigrationsHistory` com `ON CONFLICT DO NOTHING` para o EF não tentar reaplicá-las (`20260217122041_InitialCreatePostgreSQL.cs`, linhas 13-27).
7. **Preservar comportamento de tipos legados via shim.** Ao migrar para Npgsql, ativa-se `Npgsql.EnableLegacyTimestampBehavior` para manter a semântica `timestamp sem timezone` herdada do SQL Server (`Program.cs` da API e do Worker; `commit_migration_postgresql.sh`, linha 43).
8. **Não remover a tecnologia antiga imediatamente.** SQL Server continua suportado após a migração para PostgreSQL; o legado de Comunicação é mantido em paralelo até a convergência por adaptador.

---

## 4. Convenções para Migração de Controllers

> **Observação honesta:** não há evidência de controllers **legados sendo reescritos** (ex.: de `ApiController` clássico do .NET Framework para `ControllerBase` do ASP.NET Core). Todos os controllers já estão no padrão moderno. As convenções abaixo são o **alvo** para o qual qualquer controller migrado deve convergir, extraídas do padrão real uniforme do projeto ([CODING_STANDARDS.md](CODING_STANDARDS.md); [PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) §5).

- **Assinatura:** `{Entidade}Controller : ControllerBase`, anotado `[ApiController]`, rota `api/[controller]`, `[Authorize]` por padrão.
- **Retorno HTTP:** `ActionResult<T>` tipado; **100% async** (`async Task<ActionResult<T>>`).
- **Tratamento de erros:** o controller **traduz exceções semânticas** lançadas pelo service em status HTTP (`KeyNotFoundException` → 404, `ArgumentException` → 400, auth → 401/403). Erros de auth retornam corpo `{ message }` (não string crua) — o frontend depende disso.
- **Dependências / injeção:** injeta `I{X}Service` via construtor; **sem acesso direto a DbContext/repositório no controller**.
- **RBAC:** ao migrar/adicionar um controller, mapear recurso/ação no `PermissionResourceMap` (o `PermissionMiddleware` cobre `/api/*`).

`TODO: confirmar com o time` — se algum controller futuro vier de código legado em inglês, decidir a política de renomeação (o domínio é em português; ver §10 de PROJECT_CONTEXT sobre divergência de nomes do Portal).

---

## 5. Convenções para Migração de Services

- **Regra de negócio sai da entidade/queue legada e vai para o `Service`.** O mapa de legado de Comunicação aponta que `ConfiguracaoMensagem` "mistura configuracao de template com regra de automacao por dias apos visita" e que essa regra temporal **deve migrar** para o domínio/serviço novo (`COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, linhas 16-23). Ou seja: ao migrar, **separar responsabilidades** que estavam acopladas.
- **Service é interface-driven:** `{X}Service : I{X}Service`, injeta `IUnitOfWork`, repositórios e `ILogger<T>` ([CODING_STANDARDS.md]; PROJECT_CONTEXT §5).
- **Mapeamento DTO↔entidade é manual** (mappers privados `MapToDto`) — **não há AutoMapper**. Migrações devem manter esse padrão (PROJECT_CONTEXT §11/§12).
- **Reaproveitar pipeline existente em vez de duplicar.** Diretriz explícita: "nao criar segunda fila paralela para entregas; reaproveitar o fluxo de `MensagemAgendada` como base de processamento" (`COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, linhas 91-92).
- **Integração do legado por adaptador.** `NotificacaoUsuario` e `KidsNotificacao` devem ser "integradas depois ao painel central **por adaptador**", mantendo a implementação concreta antiga (linhas 70-71, 86-87).

---

## 6. Convenções para Migração de Repositories

- **Acesso a dados é EF Core, não ADO.NET.** Não há `DataSet`/`DataTable`/`SqlDataAdapter` para migrar — o backend já nasceu/foi migrado para EF Core 9 (Code First). Qualquer migração de acesso a dados deve resultar em repositório EF Core.
- **Padrão de repositório:** `{X}Repository : I{X}Repository`, recebe `SistemaIgrejaDbContext` + contexto de tenant; paginação retorna tupla `(Items, Total)`; ordenação/filtro dinâmicos (PROJECT_CONTEXT §5).
- **Queries vão para LINQ;** SQL bruto só quando o ORM não expressa o recurso (ex.: locking de fila). Nesse caso usa-se `FromSqlRaw`/`FromSqlInterpolated`.
- **SQL bruto deve ser provider-aware.** Onde há SQL cru, o repositório **detecta o provider** e emite a sintaxe correta para cada banco — evidência real em `MensagemAgendadaRepository.cs` (~linhas 174-197): PostgreSQL usa `FOR UPDATE SKIP LOCKED`, SQL Server usa `WITH (UPDLOCK, ROWLOCK)`, selecionado por `_context.Database.ProviderName?.Contains("Npgsql")`. **Esta é a regra ao migrar qualquer query manual: ramificar por provider, não assumir um só banco.**

---

## 7. Convenções para Migração de Integrações

> Referência canônica completa: [INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md). Resumo das regras com reflexo em migração:

- **Cliente HTTP escrito à mão sobre `HttpClient` typed** (`AddHttpClient<IServico, ServicoImpl>()`), nunca framework de integração (`INTEGRATION_PATTERNS.md`, linha 19).
- **Serialização exclusivamente `System.Text.Json`** com `[JsonPropertyName]` — **nunca Newtonsoft** (`INTEGRATION_PATTERNS.md`, linhas 170, 448). Se um componente migrado trouxer `Newtonsoft.Json`, deve ser convertido para `System.Text.Json`.
- **Autenticação por API Key/token em header**, lida de **env var** (nunca hardcode). Webhooks validados por token (`Asaas` usa `WebhookToken`; HMAC ainda pendente).
- **Kill-switch:** integração migrada deve ficar **desligada quando a credencial estiver vazia** (padrão Sentry/Email), falhando como no-op em vez de derrubar o fluxo.
- **Retry + timeout** configuráveis (padrão Evolution/Asaas).

> **Honestidade de evidência:** não há registro de uma biblioteca HTTP/JSON antiga sendo **substituída** dentro do repo — `Newtonsoft`/`RestSharp`/`WebClient` simplesmente **não existem**. Trate as regras acima como o **alvo obrigatório** ao trazer integrações de fora.

---

## 8. Convenções para Migração de DataSets e DataTables

> **Não há DataSets/DataTables no projeto** (busca exaustiva em `BackEnd/` retornou zero ocorrências de `DataSet`, `DataTable`, `SqlDataAdapter`, `IDataReader`). Logo, **não há migração de DataSet observada**.

A regra derivada (por analogia ao que o projeto fez ao consolidar `Pessoa`) para quando isso aparecer:

- **DataSet/registro tabular legado → Entity de domínio + DTO.** O destino é uma classe de entidade em `Domain/Entities/` (implementando `ITenantEntity` se for dado de igreja) com DTOs `{X}Dto`/`Create{X}Dto`/`Update{X}Dto`.
- **Mapeamento manual** (sem AutoMapper).
- **Preservação de dados** seguindo o padrão de 3 passos (§3.3) e dedup por chave natural (§3.5), como em `RefatoracaoPessoaCentralizada`.

`TODO: confirmar com o time` — confirmar que não existe nenhum DataSet em código não versionado aqui (ex.: sistema legado externo ainda não trazido ao repo).

---

## 9. Convenções para Migração de Procedures

> **Não há stored procedures no projeto** (zero `CREATE PROCEDURE`, `sp_`, `.StoredProcedure`). O acesso é todo via EF Core LINQ.

Regras observadas que se aplicam quando lógica de procedure precisar ser trazida:

- **Lógica de procedure vira LINQ/Service.** O default é reimplementar em LINQ no repositório + regra no Service.
- **Quando precisar de SQL cru** (concorrência, locking, recursos específicos do banco), usar `FromSqlRaw`/`FromSqlInterpolated` **parametrizado** (placeholders `{0}`, `{1}`, nunca concatenação) e **ramificado por provider** — exatamente como `MensagemAgendadaRepository.cs` faz com `FOR UPDATE SKIP LOCKED` vs `WITH (UPDLOCK, ROWLOCK)` (§6).
- **Tratamento de parâmetros:** parâmetros posicionais do EF (`{0}`), valores passados como argumentos do `FromSqlRaw`, nunca interpolados na string (proteção contra injeção).

`TODO: confirmar com o time` — não há evidência de procedures de um sistema anterior. Se a igreja-origem (Kingdom) tinha procedures, confirmar onde foram reimplementadas.

---

## 10. Convenções para Migração de Relatórios

> **Nenhuma tecnologia de relatório legada encontrada** — sem Crystal Reports, iTextSharp, QuestPDF, DinkToPdf, Rotativa. (`reportgenerator` que aparece é ferramenta de cobertura de testes, não geração de relatório.)

O que existe hoje:

- **Relatórios são endpoints de dados agregados** (ex.: dashboard financeiro, relatórios financeiros) servidos como JSON pela API; a renderização visual fica no FrontEnd (React + `recharts`).
- **E-mails transacionais** são templates HTML estáticos versionados na raiz (`01-verificacao-email.html` … `04-pagamento-pendente.html`) consumidos pelo `SmtpEmailService`.

Regra derivada para futuras migrações de relatórios:

- **Crystal Reports / relatório binário legado → endpoint de dados (JSON) + renderização no cliente**, salvo necessidade explícita de PDF server-side.
- `TODO: confirmar com o time` — não há padrão definido para **geração de PDF no servidor**. Se um relatório legado exigir PDF, decidir a biblioteca (nenhuma adotada hoje) antes de migrar.

---

## 11. Convenções para Migração de Configurações

- **Segredos saem de arquivos de config e vão para variáveis de ambiente.** Regra reforçada por incidente real: segredos já estiveram versionados e foram **rotacionados** (PROJECT_CONTEXT §9). Hoje `appsettings.json` mantém segredos **vazios**, lidos de env var no Coolify.
- **Override por convenção `__`** (duplo underscore): `Jwt__Key`, `ConnectionStrings__DefaultConnection`, `Billing__Asaas__ApiKey`, `Sentry__Dsn`.
- **`appsettings.json` é explicitamente excluído de commits** durante migrações: o script de commit faz `git reset HEAD -- "src/SistemaIgreja.API/appsettings.json"` e remove `*.backup`/`.DS_Store` antes de commitar (`commit_migration_postgresql.sh`, linhas 14-21).
- **Seleção de comportamento por configuração, não por branch de código:** provider de banco escolhido por `Database:Provider` em runtime; integrações ligadas/desligadas por presença de credencial (kill-switch). A mesma imagem roda em todos os ambientes.

> **Web.config:** não há `Web.config` no projeto (é ASP.NET Core, não .NET Framework). Migração de `Web.config` → `appsettings.json` + env vars seria o caminho, mas **não há evidência de que tenha ocorrido aqui**. `TODO: confirmar com o time`.

---

## 12. Tecnologias Substituídas (com evidência)

| Antiga | Nova | Motivo aparente | Evidência |
|---|---|---|---|
| **SQL Server** (provider) | **PostgreSQL** (Npgsql) | Portabilidade / custo / fim de lock-in; PG vira o provider de produção | `commit_migration_postgresql.sh`; `20260217122041_InitialCreatePostgreSQL.cs`; `Program.cs` (seleção dinâmica) |
| **Entidades dispersas** `Visitante`/`Voluntario`/`Usuario` com `Nome`/`Email` próprios | **`Pessoa` central + `PessoaPerfil`** (FK `PessoaId`) | Eliminar duplicação, modelo único de pessoa, normalização | `20251212034213_RefatoracaoPessoaCentralizada.cs` |
| **Tipo identidade `SERIAL`/default** em `PerfisAcessoPermissoes` | **`GENERATED ALWAYS AS IDENTITY`** (PG moderno) | Correção pós-migração PG do auto-incremento | `corrigir_perfis_acesso_permissoes_id.sql` |
| **Entidades single-tenant** (família `Comunicacao*`, `NotificacaoUsuario`) | **Entidades `ITenantEntity` com `TenantId` + backfill** | Isolamento multi-tenant (SaaS) | `20260618213103_AdicionarTenantIdComunicacaoNotificacoes.cs` |
| **`ConfiguracaoMensagem` / `MensagemAgendada`** (template/fila acoplados a visitante) | **Domínio central de Comunicação** (`ComunicacaoTemplate`/`Campanha`/`Entrega`/`Automacao`) | Multicanal, separar template de automação | `COMUNICACAO_SPRINT1_MAPA_LEGADO.md` (migração **em andamento**, por adaptador) |

> Substituições **não encontradas** (não inventar): biblioteca HTTP antiga→nova, ORM antigo→novo, Newtonsoft→System.Text.Json — todas inexistentes como migração; ver §7.

---

## 13. Tecnologias Mantidas (coexistência deliberada)

- **SQL Server** — **mantido como provider alternativo** após a migração para PostgreSQL. O `switch` de provider preserva o ramo `UseSqlServer` e o pacote `Microsoft.EntityFrameworkCore.SqlServer` continua referenciado (`Infrastructure.csproj`). Motivo: flexibilidade/compatibilidade, sem custo de remover.
- **SQLite** — adotado **só em testes** (rapidez e isolamento).
- **Estruturas legadas de Comunicação** (`EnvioCampanhaAniversario`, `NotificacaoUsuario`, `KidsNotificacao`) — mantidas como "estruturas especializadas no curto prazo", convergindo gradualmente (`COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, linhas 52-55, 68-71, 84-87).
- **Comportamento de timestamp legado** — mantido via `Npgsql.EnableLegacyTimestampBehavior=true` para não reescrever toda a manipulação de datas (`Program.cs`; `commit_migration_postgresql.sh`, linha 43).

---

## 14. Estratégias de Compatibilidade (observadas)

- **Multi-provider em runtime** como camada de compatibilidade: o mesmo código atende PG e SQL Server, ramificando SQL cru por `ProviderName` (§6).
- **Shim de comportamento** (`EnableLegacyTimestampBehavior`) para preservar semântica antiga sem migrar dados/código de datas.
- **Baseline + marcação de histórico**: ao introduzir a baseline PG, as migrations antigas são marcadas como aplicadas (`__EFMigrationsHistory`, `ON CONFLICT DO NOTHING`) — permite que bancos novos e bancos já existentes convivam com o mesmo conjunto de migrations (`20260217122041_InitialCreatePostgreSQL.cs`, linhas 13-27).
- **Coexistência via adaptador** (strangler) no módulo de Comunicação: contratos centrais novos absorvem implementações concretas antigas gradualmente, sem fila paralela.
- **Colunas temporárias nullable** durante a transição estrutural (PessoaId nullable → preenchido → NOT NULL), garantindo que a aplicação não quebre no meio da migração.
- **Versionamento:** `TODO: confirmar com o time` — não há versionamento de API (ex.: `/v1/`, `/v2/`) observado; a compatibilidade é mantida no nível de schema/DTO, não de versão de rota.

---

## 15. Estratégias de Validação (como as migrações parecem ser validadas)

- **Idempotência como autovalidação:** backfills e DDL bruto são reexecutáveis sem efeito colateral (`WHERE TenantId = 0`, `IF NOT EXISTS`, `ON CONFLICT DO NOTHING`) — reduz risco de aplicação dupla.
- **Migrations reversíveis (`Down`)** permitem rollback validado.
- **Aplicação automática no startup** controlada por flag: `Database:RunMigrations = true` (PROJECT_CONTEXT §8) — valida que a migration sobe junto com a app.
- **Testes automatizados como gate.** Backend: xUnit + Moq + FluentAssertions, incluindo **teste de isolamento de tenant** (`TenantQueryFilterTests`) — crítico para validar tenantizações. Frontend admin: CI **bloqueia deploy se os testes falharem** (PROJECT_CONTEXT §9; CODING_STANDARDS).
- **Scripts SQL avulsos de correção** (`CORRIGIR_KIDSCHECKINS.sql`, `corrigir_perfis_acesso_permissoes_id.sql`) indicam **validação manual em produção** pós-migração, com fixes pontuais aplicados fora do fluxo de migration.
- **Health checks** (`/health`: DB + configs) validam que a app sobe com o banco migrado.

`TODO: confirmar com o time` — não há evidência de **comparação automatizada de resultados** (old vs new) nem de ambiente formal de homologação para migrações de banco. A validação parece ser testes + aplicação em prod + correção manual.

---

## 16. Checklist para Migração de Controller

- [ ] Herda `ControllerBase`, anotado `[ApiController]`, rota `api/[controller]`, `[Authorize]`.
- [ ] Todos os endpoints `async`, retornando `ActionResult<T>` tipado.
- [ ] Nenhum acesso a `DbContext`/repositório direto — só `I{X}Service` via DI.
- [ ] Exceções semânticas do service traduzidas em status HTTP (404/400/401/403/500).
- [ ] Erros de auth retornam corpo `{ message }`.
- [ ] Recurso/ação mapeados no `PermissionResourceMap` (RBAC).
- [ ] Nomes em português (domínio); se vier de legado em inglês, alinhar política (`TODO`).
- [ ] Registrado no DI da API.

## 17. Checklist para Migração de Service

- [ ] `{X}Service : I{X}Service`; injeta `IUnitOfWork`, repositórios, `ILogger<T>`.
- [ ] Regra de negócio **extraída** da entidade/fila legada para o service (separar responsabilidades acopladas).
- [ ] Mapeamento DTO↔entidade **manual** (sem AutoMapper).
- [ ] Reaproveita pipeline/fila existente em vez de criar paralelo.
- [ ] Legado integrado por **adaptador**, não substituído de uma vez.
- [ ] Lança exceções semânticas; loga erros (Sentry, sem PII).
- [ ] Registrado no DI da **API e do Worker** se um scheduler depender dele.

## 18. Checklist para Migração de Repository

- [ ] `{X}Repository : I{X}Repository`, recebe `SistemaIgrejaDbContext` + contexto de tenant.
- [ ] Queries em LINQ; SQL cru só quando o ORM não expressa.
- [ ] SQL cru **parametrizado** (`{0}`) e **ramificado por provider** (`ProviderName.Contains("Npgsql")`).
- [ ] Filtra por tenant (confiar no global query filter; não burlar).
- [ ] Paginação retorna `(Items, Total)`.
- [ ] Zero ADO.NET legado (`DataSet`/`DataReader`).
- [ ] Async em todo acesso a dados.

## 19. Checklist para Migração de Integração

- [ ] Cliente `HttpClient` typed via `AddHttpClient<I, Impl>()`.
- [ ] `System.Text.Json` + `[JsonPropertyName]` (converter qualquer Newtonsoft).
- [ ] Config tipada em `Application/Configuration/` com `SectionName`; segredos **vazios** no `appsettings`, vindos de env var.
- [ ] Autenticação por header (API Key/token) lida de env var; nunca hardcode.
- [ ] **Kill-switch:** no-op quando credencial vazia.
- [ ] Retry + timeout configuráveis.
- [ ] Health check se for integração crítica.
- [ ] Decidir API vs Worker; registrar no Worker se usada por scheduler.
- [ ] Falhas logadas (Sentry), sem PII.

## 20. Checklist para Migração de DataSet → DTO/Entity

- [ ] Criar entidade em `Domain/Entities/` (com `ITenantEntity`/`TenantId` se for dado de igreja).
- [ ] DTOs `{X}Dto`/`Create{X}Dto`/`Update{X}Dto`; validação por DataAnnotations.
- [ ] Mapeamento manual (sem AutoMapper).
- [ ] Migration EF Core com **preservação de dados** (3 passos: nullable → backfill → NOT NULL).
- [ ] Dedup por chave natural ao consolidar (`WHERE NOT EXISTS`).
- [ ] Backfill **idempotente**.
- [ ] `Down()` reversível.
- [ ] Índices únicos compostos por `(TenantId, ...)`.
- [ ] Teste de isolamento de tenant.

---

## 21. Anti-Patterns Detectados (aparentemente evitados)

Documentados **apenas com base no que o código mostra ter sido evitado/repreendido**:

- **Big-bang / substituição abrupta de módulo.** Explicitamente evitado: "evoluida por generalizacao gradual em vez de substituicao abrupta" (`COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, linha 39).
- **Fila/pipeline paralela duplicada.** Explicitamente evitado: "nao criar segunda fila paralela para entregas" (linha 91).
- **DDL destrutivo sem backfill.** Nunca se dropa coluna sem antes migrar os dados (`RefatoracaoPessoaCentralizada` migra antes de `DropColumn`).
- **SQL cru assumindo um único banco.** Evitado via ramificação por provider (`MensagemAgendadaRepository`).
- **SQL cru por concatenação.** Evitado — usa parâmetros `{0}` no `FromSqlRaw`.
- **Segredos em arquivo de config versionado.** Evitado ativamente: `appsettings.json` é excluído do commit e segredos foram rotacionados após incidente.
- **AutoMapper / "mágica" de mapeamento.** Evitado por decisão de projeto (mapeamento manual).
- **Newtonsoft.Json em integrações.** Evitado (`System.Text.Json` exclusivo).
- **Migration não-idempotente.** Evitado via `WHERE ... = 0` / `IF NOT EXISTS` / `ON CONFLICT DO NOTHING`.

---

## 22. Dúvidas e Pendências (`TODO: confirmar com o time`)

- `TODO: confirmar com o time` — **Migração de versão do .NET:** todos os `.csproj` já estão em `net10.0`; não há evidência no repo de uma migração a partir de .NET Framework/versão anterior. Houve essa migração antes do estado atual? (PROJECT_CONTEXT §16 também questiona se o projeto de testes ainda é .NET 9.)
- `TODO: confirmar com o time` — **Sistema legado de origem (Kingdom):** o produto nasceu na "Igreja Kingdom". Existiu um sistema anterior (com procedures, DataSets, Crystal Reports) do qual este foi migrado? Nada disso está no repo — só o resultado moderno.
- `TODO: confirmar com o time` — **Validação de migração:** há comparação automatizada old-vs-new ou ambiente de homologação dedicado para migrações de banco, ou a validação é só testes + correção manual em prod (sugerido pelos scripts `CORRIGIR_*.sql`)?
- `TODO: confirmar com o time` — **Geração de PDF/relatório server-side:** não há biblioteca adotada. Definir antes de migrar qualquer relatório legado que exija PDF.
- `TODO: confirmar com o time` — **Versionamento de API:** não há `/v1/` observado. Como a compatibilidade de contrato é mantida entre versões (só por schema/DTO?).
- `TODO: confirmar com o time` — **Procedures:** confirmar que nenhuma lógica de procedure de um sistema anterior precisa ser portada (o backend atual não tem nenhuma).
- `TODO: confirmar com o time` — **Web.config → appsettings:** não há `Web.config` no repo; confirmar se a migração ASP.NET Framework → Core ocorreu fora deste histórico.
- `TODO: confirmar com o time` — **Estado da migração de Comunicação (strangler):** o `MAPA_LEGADO` é de planejamento (Sprint 1). Quanto do domínio central já substituiu de fato as estruturas legadas em produção?

---

### Fontes desta análise

- `BackEnd/src/SistemaIgreja.Infrastructure/Migrations/20251212034213_RefatoracaoPessoaCentralizada.cs`
- `BackEnd/src/SistemaIgreja.Infrastructure/Migrations/20260217122041_InitialCreatePostgreSQL.cs`
- `BackEnd/src/SistemaIgreja.Infrastructure/Migrations/20260618213103_AdicionarTenantIdComunicacaoNotificacoes.cs`
- `BackEnd/src/SistemaIgreja.Infrastructure/Repositories/MensagemAgendadaRepository.cs`
- `BackEnd/src/SistemaIgreja.API/Program.cs` e `BackEnd/SistemaIgreja.BackgroundWorker/Program.cs`
- `BackEnd/commit_migration_postgresql.sh`, `BackEnd/corrigir_perfis_acesso_permissoes_id.sql`, `BackEnd/APLICAR_MIGRATION_KIDS.sql`, `BackEnd/CORRIGIR_KIDSCHECKINS.sql`
- `COMUNICACAO_SPRINT1_MAPA_LEGADO.md`, `MULTITENANCY_ROADMAP.md`, `SAAS_READINESS.md`
- `.claude/PROJECT_CONTEXT.md`, `.claude/CODING_STANDARDS.md`, `.claude/INTEGRATION_PATTERNS.md`
</content>
</invoke>
