# Multitenancy Roadmap

## Objetivo

Transformar o produto atual em uma plataforma multitenant com foco inicial em:

- `BackEnd`
- `FrontEnd` Admin

Ficam fora do escopo imediato:

- `Portal`, exceto compatibilidade futura para resolver o tenant correto
- `AppKids` e futuros apps mobile, exceto compatibilidade futura para resolver o tenant correto
- produto/comercial/billing

## Premissas

- O sistema atual passa a ser o primeiro tenant oficial da plataforma.
- O nome do tenant inicial será `Mang Guarulhos`.
- A estratégia inicial será `shared app + shared database + isolamento lógico por TenantId`.
- O objetivo principal da primeira etapa é segurança e isolamento de dados entre igrejas.
- O sistema não deve ficar "meio tenantizado". O marco mínimo de segurança estrutural é o fim da Sprint 3.

## Resultado Esperado

Ao final do programa:

- múltiplas igrejas poderão operar na mesma plataforma sem vazamento de dados
- o Admin funcionará no contexto do tenant correto
- jobs e integrações rodarão por tenant
- será possível provisionar um novo tenant sem SQL manual
- `Mang Guarulhos` continuará funcionando como tenant inicial

## Estratégia Técnica

### Modelo de tenancy

- banco compartilhado
- schema compartilhado
- isolamento por `TenantId`
- resolução de tenant via autenticação no Admin
- suporte preparado para domínio/subdomínio no futuro

### Pilares obrigatórios

1. resolução correta de tenant
2. autenticação e autorização tenant-aware
3. isolamento automático nas queries
4. testes anti-vazamento

## Ordem Macro de Execução

1. fundação tenant/auth/config central
2. raiz humana e bootstrap por tenant
3. isolamento automático e núcleo admin
4. módulos operacionais
5. módulos sensíveis
6. jobs e integrações
7. backoffice e homologação

## Roadmap por Sprint

### Sprint 1

Objetivo:

- fundar o núcleo de tenant
- tornar auth, auditoria e configurações centrais tenant-aware

Entregas:

- `Tenant`
- `TenantDomain`
- `ITenantContext`
- `HttpTenantContext`
- `TenantId` em:
  - `Usuario`
  - `PerfilAcesso`
  - `PerfilAcessoPermissao`
  - `AuditLog`
  - `ConfiguracaoPortal`
  - `ConfiguracaoMensagem`
  - `ConfiguracaoCampanhaAniversario`
- JWT com `TenantId` e `TenantSlug`
- migration inicial com tenant `Mang Guarulhos`

Marco:

- o sistema já reconhece `Mang Guarulhos` como tenant

### Sprint 2

Objetivo:

- tenantizar a raiz humana do sistema
- trocar bootstrap global por bootstrap por tenant

Entregas:

- `TenantId` em:
  - `Pessoa`
  - `PessoaPerfil`
  - `Visitante`
- `Pessoa.Email` único por tenant
- repositórios e services humanos tenant-aware
- criação de admin inicial por tenant

Marco:

- pessoas e visitantes passam a pertencer explicitamente ao tenant

### Sprint 3

Objetivo:

- ativar isolamento automático
- consolidar núcleo do Admin

Entregas:

- interface base para entidades tenantizadas
- query filters globais no EF
- isolamento de:
  - usuários
  - pessoas
  - visitantes
  - auditoria
  - perfis de acesso
- `SearchService` e `DashboardService` tenantizados
- `AuthContext` e `apiClient` do Admin ajustados

Marco:

- fim da fundação estrutural segura

### Sprint 4

Objetivo:

- tenantizar estrutura ministerial base

Entregas:

- `TenantId` em:
  - `Equipe`
  - `Cargo`
  - `Voluntario`
  - `HubCasa`
- repositórios, services e controllers desses módulos
- migration com backfill para `Mang Guarulhos`

### Sprint 5

Objetivo:

- tenantizar agenda e conteúdo administrativo principal

Entregas:

- `TenantId` em:
  - `Evento`
  - `EventoRecorrencia`
  - `EventoOcorrencia`
  - `InscricaoEvento`
  - `DestaqueSite`
  - `CategoriaNoticia`
  - `Noticia`
  - `Contato`
- serviços e endpoints correspondentes
- migration com backfill para `Mang Guarulhos`

### Sprint 6

Objetivo:

- tenantizar voluntariado avançado, mídia e enquetes

Entregas:

- `TenantId` em:
  - `Escala`
  - `EscalaItem`
  - `EscalaModelo`
  - `EscalaModeloItem`
  - `IndisponibilidadeVoluntario`
  - `SolicitacaoTrocaEscala`
  - `CategoriaMidia`
  - `GaleriaFoto`
  - `GaleriaFotoItem`
  - `Enquete`
  - `EnqueteOpcao`
  - `EnqueteVoto`
- migration com backfill para `Mang Guarulhos`

### Sprint 7

Objetivo:

- tenantizar o módulo Financeiro

Entregas:

- `TenantId` em:
  - `CategoriaDespesa`
  - `CategoriaReceita`
  - `ContaBancaria`
  - `CentroCusto`
  - `Projeto`
  - `CategoriaPatrimonio`
  - `PatrimonioItem`
  - `PatrimonioMovimentacao`
  - `Despesa`
  - `Receita`
  - `Fornecedor`
- dashboards e relatórios financeiros por tenant
- migration com backfill para `Mang Guarulhos`

### Sprint 8

Objetivo:

- tenantizar o módulo Kids

Entregas:

- `TenantId` em:
  - `CriancaDetalhe`
  - `ResponsavelCrianca`
  - `KidsCheckin`
  - `KidsNotificacao`
  - `KidsDeviceToken`
  - `KidsOcorrencia`
  - `KidsSala`
  - `KidsTurma`
- `KidsAuthorizationService` ajustado para tenant
- migration com backfill para `Mang Guarulhos`

### Sprint 9

Objetivo:

- tenantizar jobs, integrações e configurações operacionais

Entregas:

- configs por tenant para:
  - email
  - Evolution/WhatsApp
  - push
  - uploads
- `BackgroundWorker` processando tenant por tenant
- schedulers tenant-aware
- logs e health checks por tenant

### Sprint 10

Objetivo:

- criar backoffice da plataforma
- endurecer a solução
- homologar coexistência de múltiplos tenants

Entregas:

- `PlatformAdmin`
- CRUD de tenants
- provisionamento de tenant
- seed padrão tenant-aware
- suíte final anti-vazamento
- revisão final de índices/performance
- tenant de homologação além de `Mang Guarulhos`
- checklist operacional de onboarding

## Sequência de PRs por Sprint

### Sprint 1

1. núcleo de tenant
2. auth e segurança tenant-aware
3. auditoria e configurações centrais
4. migration base com `Mang Guarulhos`
5. testes

### Sprint 2

1. modelos e EF da raiz humana
2. repositórios e services humanos
3. bootstrap/admin inicial por tenant
4. migration
5. testes

### Sprint 3

1. convenção tenantizada
2. query filters globais
3. núcleo admin backend
4. frontend admin base
5. testes

### Sprint 4

1. equipe e cargo
2. voluntário e hub
3. migration
4. testes

### Sprint 5

1. eventos
2. inscrições
3. conteúdo base
4. migration
5. testes

### Sprint 6

1. escalas
2. disponibilidade e troca
3. mídia
4. enquetes
5. migration
6. testes

### Sprint 7

1. cadastros financeiros
2. transações financeiras
3. patrimônio
4. dashboards e relatórios
5. migration
6. testes

### Sprint 8

1. raiz kids
2. operação kids
3. estrutura e push
4. autorização kids
5. migration
6. testes

### Sprint 9

1. configuração por tenant
2. background worker tenant-aware
3. schedulers tenant-aware
4. observabilidade
5. testes

### Sprint 10

1. `PlatformAdmin`
2. CRUD de tenant
3. provisionamento
4. hardening
5. homologação

## Quadro de Execução

### Dependências mais críticas

- Sprint 1 antes de todo o resto
- Sprint 2 antes dos módulos que dependem de `Pessoa`
- Sprint 3 antes de considerar o sistema estruturalmente seguro
- Sprint 7 e Sprint 8 só depois da base já estabilizada
- Sprint 10 depende da maturidade de 1 a 9

### Trilhas paralelas sugeridas

#### Trilha A

- Sprints 1, 2 e 3
- fundação, auth, isolamento e núcleo admin

#### Trilha B

- Sprints 4, 5 e 6
- módulos operacionais

#### Trilha C

- Sprints 7 e 8
- módulos sensíveis

#### Trilha D

- Sprints 9 e 10
- jobs, integrações, backoffice e homologação

## Entidades Prioritárias

### Críticas na fundação

- `Usuario`
- `PerfilAcesso`
- `PerfilAcessoPermissao`
- `AuditLog`
- `ConfiguracaoPortal`
- `ConfiguracaoMensagem`
- `ConfiguracaoCampanhaAniversario`
- `Pessoa`

### Críticas nos módulos sensíveis

- `Despesa`
- `Receita`
- `PatrimonioItem`
- `KidsCheckin`
- `KidsOcorrencia`
- `ResponsavelCrianca`

## Principais Riscos

### Riscos estruturais

- login continuar operando globalmente
- seeds globais permanecerem ativos
- singleton escondido em repositórios/configurações
- queries sem isolamento automático
- jobs executarem sem contexto de tenant

### Riscos de negócio

- vazamento de dados entre igrejas
- quebra da base atual de `Mang Guarulhos`
- regressão em módulos sensíveis como Financeiro e Kids

## Critérios Gerais de Qualidade

Cada sprint deve encerrar com:

- migration validada
- testes mínimos do escopo verde
- validação de que `Mang Guarulhos` continua íntegra
- validação explícita de isolamento entre tenants quando aplicável

## Critério de Go/No-Go

O sistema só deve ser considerado apto para receber uma segunda igreja quando:

- Sprint 3 estiver consolidada para o núcleo
- módulos críticos previstos para operação inicial já estiverem tenantizados
- jobs e integrações já estiverem tenant-aware
- existir um tenant de homologação além de `Mang Guarulhos`

## Fora de Escopo Agora

- redesign do `Portal`
- white-label completo para portais de outras igrejas
- evolução do `AppKids` e futuros mobiles
- billing, pricing, planos e estratégia comercial

## Próximo Passo Recomendado

Usar este documento como guia oficial e executar o programa começando por:

1. Sprint 1
2. Sprint 2
3. Sprint 3

Sem considerar o sistema seguro para múltiplos tenants antes desse bloco estar finalizado.
