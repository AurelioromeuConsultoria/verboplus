# Plano de Cobertura de Testes do Backend

## Meta de parada recomendada

### Meta mínima aceitável

Ponto em que a base já está protegida o suficiente para o produto seguir crescendo com risco controlado:

- `Application + API + Domain`: `50%` a `55%`
- `API`: pelo menos `40%`
- `Infrastructure`: pelo menos `5%` a `8%`, de forma seletiva
- todos os fluxos críticos com teste de regra, autorização e regressão

### Meta boa

Ponto considerado muito saudável para o estágio atual do AppIgreja:

- `Application + API + Domain`: `60%` a `65%`
- `API`: `45%` a `55%`
- `Application`: `65%+`
- `Infrastructure`: `8%` a `12%`, priorizando query services, repositórios sensíveis e operações reais

### Meta exagerada

Ponto em que o retorno tende a cair muito:

- perseguir `80%+` no backend total
- tentar levar `Infrastructure` inteira para cobertura alta indiscriminadamente
- gastar tempo com CRUD trivial, wiring e boilerplate só para subir número

## Snapshot de referência atual

Medição consolidada após a onda mais recente de testes:

- `898` testes passando
- cobertura bruta de linhas: `10,00%`
- cobertura bruta de branches: `48,10%`
- `Infrastructure`: `4,10%` linhas
- `Application`: `61,70%` linhas
- `API`: `63,61%` linhas
- `Domain`: `95,62%` linhas

Leitura útil:

- `Application + API + Domain`: meta boa já atingida no recorte que mais importa para evolução do produto
- `API`: já está em patamar forte e não precisa mais de campanha agressiva
- `Infrastructure`: continua sendo a principal frente aberta, mas agora com retorno marginal menor

## Política oficial após a campanha de cobertura

Leitura consolidada do estado atual:

- `Domain`, `Application` e `API` já estão em patamar suficiente para sair do modo de campanha ampla
- `Infrastructure` ainda merece evolução, mas apenas de forma seletiva
- cobertura bruta geral não deve mais guiar sozinha a priorização

### O que já consideramos suficiente

- `Domain`
- `Application`
- `API`

Nessas camadas, a regra agora deve ser:

- toda funcionalidade nova sai com teste
- todo bug corrigido ganha teste de regressão
- lacunas antigas entram apenas quando estiverem em módulo prioritário ou área de risco real

### O que ainda precisa melhorar

- `Infrastructure`, com foco em:
  - services com regra operacional
  - schedulers e workers
  - query services
  - repositórios com filtros, includes, ordenação ou comportamento relevante
  - pontos que já causaram bug, suporte ou risco operacional

### O que não vale perseguir como objetivo principal

- cobertura bruta total como meta isolada
- CRUD trivial só para subir percentual
- wiring, boilerplate e partes com pouco risco real
- tentativa de equalizar `Infrastructure` inteira na marra

### Nova regra de execução

Daqui em diante, a política oficial é:

- encerrar a campanha ampla de cobertura nas camadas já maduras
- manter uma trilha seletiva de testes apenas para `Infrastructure` crítica
- exigir teste novo em toda feature nova relevante
- exigir regressão automatizada para todo bug corrigido em fluxo importante
- priorizar risco, confiança e facilidade de evolução acima do percentual bruto

### Checklist rápido de decisão

Criar teste novo quando:

- houver regra de negócio nova ou alterada
- houver mudança de autorização, escopo ou acesso
- houver transição de status ou workflow sensível
- houver integração, scheduler, worker, query service ou comportamento operacional relevante
- houver correção de bug em fluxo importante

Não criar teste novo apenas para empurrar cobertura quando:

- for CRUD trivial sem regra adicional
- for wiring ou boilerplate de baixo risco
- for detalhe técnico sem impacto real de operação

Regra curta:

- se a mudança pode quebrar confiança, operação, segurança ou suporte, ela deve sair com teste

## Triagem atual do backlog

### Continua na fila

- `Infrastructure` crítica com lógica operacional, query relevante, scheduler, worker ou histórico de falha
- testes de integração seletivos para fluxos ponta a ponta prioritários
- regressões de bugs importantes
- novas features de módulos sensíveis

### Sai do modo campanha

- `Domain`
- `Application`
- `API`

Nessas camadas, a cobertura passa a crescer principalmente por:

- feature nova
- bug corrigido
- mudança de autorização
- mudança de workflow ou comportamento de negócio

### Pode sair da fila sem prejuízo agora

- CRUD trivial sem regra adicional
- wiring e boilerplate de baixo risco
- classes passivas cobertas apenas para empurrar percentual
- antigos itens de cobertura ampla em áreas já maduras

## Plano em sprints para chegar à meta

### Sprint 1 - fechar meta mínima da API e consolidar infraestrutura crítica

Objetivo:
- levar `API` para a faixa de `40%+`
- levar `Infrastructure` para algo entre `4%` e `5%`

Entregáveis:
- cobrir controllers ainda sem teste direto em módulos administrativos e operacionais residuais
- cobrir repositórios críticos restantes de `Infrastructure`, especialmente `EscalaRepository`, `KidsCheckinRepository`, `CriancaDetalheRepository`, `IndisponibilidadeVoluntarioRepository`
- cobrir query paths com filtro, ordenação e includes relevantes

Critério de aceite:
- todos os testes verdes
- fluxos sensíveis restantes de `API` com pelo menos cenário feliz, erro e acesso negado
- camada `API` acima de `40%`

### Sprint 2 - levar infraestrutura ao piso saudável

Objetivo:
- levar `Infrastructure` para `5%` a `6%`
- reduzir os principais vazios em repositórios com lógica e consultas reais

Entregáveis:
- cobrir repositórios restantes com comportamento relevante em `Voluntariado`, `Kids`, `Patrimônio`, `Eventos` e `Comunicação`
- cobrir services de infraestrutura/worker que ainda tenham lógica observável
- adicionar testes seletivos para interceptors, helpers e serviços operacionais ainda descobertos

Critério de aceite:
- `Infrastructure` em `5%+`
- principais consultas e persistências sensíveis com defesa automatizada
- nenhum bug recém-corrigido sem teste de regressão

### Sprint 3 - sair da meta mínima e entrar na meta boa

Objetivo:
- levar `Application + API + Domain` para `55%+`
- levar `API` para `42%` a `45%`

Entregáveis:
- ampliar cobertura de `Application` onde ainda houver serviços centrais com ramificações abertas
- completar controllers e fluxos de negócio ainda sem cobertura consistente
- revisar testes frágeis e transformar cenários repetidos em base reutilizável

Critério de aceite:
- `Application + API + Domain` acima de `55%`
- `API` acima de `42%`
- sem crescimento relevante de testes frágeis ou artificiais

### Sprint 4 - primeira faixa da meta boa

Objetivo:
- levar `Application + API + Domain` para `60%+`
- levar `API` para `45%+`
- levar `Infrastructure` para `8%+`

Entregáveis:
- testes de integração seletivos para fluxos ponta a ponta prioritários:
  - `login`
  - `escala`
  - `troca de escala`
  - `campanha/aniversário`
  - permissões administrativas
- cobertura adicional de infraestrutura apenas onde houver risco operacional ou query relevante

Critério de aceite:
- meta boa inicial atingida
- suíte ainda rápida o suficiente para uso contínuo no dia a dia
- política de cobertura deixa de ser “campanha” e vira regra de evolução

## Política após atingir a meta

Quando a meta mínima aceitável for atingida, a estratégia muda:

- toda funcionalidade nova sai com teste
- todo bug corrigido ganha teste de regressão
- lacunas antigas entram apenas quando forem de módulo prioritário ou risco real
- porcentagem deixa de ser objetivo principal; risco e confiança passam a guiar a decisão

## Snapshot atual

Medição realizada em `2026-04-03` com:

```bash
dotnet test BackEnd/tests/SistemaIgreja.API.Tests/SistemaIgreja.API.Tests.csproj --collect:"XPlat Code Coverage"
```

Resultado atual:

- `127` testes passando
- cobertura bruta de linhas: `2,95%`
- cobertura bruta de branches: `14,58%`

Leitura mais útil do número:

- sem `Migrations`: `16,27%`
- apenas `Application + API + Domain`: `22,13%`
- apenas `Services + Controllers`: `20,18%`

Cobertura por pacote:

- `SistemaIgreja.Infrastructure`: `0%`
- `SistemaIgreja.Application`: `25,96%`
- `SistemaIgreja.Domain`: `39,44%`
- `SistemaIgreja.API`: `9,86%`

## Leitura honesta

A cobertura ainda está baixa para o tamanho do sistema.

O projeto já saiu do zero em áreas críticas, principalmente:

- `Voluntariado`
- `Visitantes`
- `Mensagens agendadas`
- `Operação`
- `Auditoria`
- `Usuários`, `Perfis`, `Pessoas`, `Equipes`, `Voluntários`
- serviços e fluxos de `Kids` que já tinham sido cobertos antes

Mesmo assim, o panorama geral ainda é de baixa proteção porque:

- há muitos serviços sem arquivo de teste próprio
- há muitos controllers sem teste direto
- `Infrastructure` praticamente não tem cobertura
- várias superfícies administrativas e financeiras ainda não têm defesa automatizada

## Mapa atual de cobertura por tipo

### Services com arquivo de teste próprio

- `ConfiguracaoMensagemService`
- `EquipeCargoVoluntarioServices`
- `EscalaService`
- `KidsEstruturaService`
- `KidsIndicadoresService`
- `KidsNotificacaoService`
- `KidsOcorrenciaService`
- `KidsPainelService`
- `KidsRetiradaService`
- `KidsService`
- `MembroCadastroService`
- `MensagemAgendadaService`
- `SolicitacaoTrocaEscalaService`
- `VisitanteService`

### Services sem arquivo de teste próprio

Total atual: `42 de 55`

- `AuditLogService`
- `AuthService`
- `CadastroMembroNotificationService`
- `CampanhaAniversarioService`
- `CargoService`
- `CategoriaDespesaService`
- `CategoriaMidiaService`
- `CategoriaNoticiaService`
- `CategoriaPatrimonioService`
- `CategoriaReceitaService`
- `CentroCustoService`
- `ConfiguracaoPortalService`
- `ContaBancariaService`
- `ContatoService`
- `DashboardFinanceiroService`
- `DashboardService`
- `DespesaService`
- `DestaqueSiteService`
- `EnqueteService`
- `EquipeService`
- `EscalaModeloService`
- `EventoOcorrenciaService`
- `EventoRecorrenciaService`
- `EventoService`
- `EvolutionApiService`
- `FornecedorService`
- `GaleriaFotoService`
- `HubCasaService`
- `NoticiaService`
- `NotificacaoUsuarioService`
- `PatrimonioItemService`
- `PatrimonioMovimentacaoService`
- `PerfilAcessoService`
- `PermissionService`
- `PessoaPerfilService`
- `PessoaService`
- `ProjetoService`
- `ReceitaService`
- `RelatorioFinanceiroService`
- `SchedulerExecutionMonitor`
- `UsuarioService`
- `VoluntarioService`

### Controllers com arquivo de teste próprio

- `AuditLogsController`
- `CargosController`
- `ConfiguracoesMensagensController`
- `EquipesController`
- `MensagensAgendadasController`
- `OperacaoController`
- `PerfisAcessoController`
- `PessoasController`
- `PessoasPerfisController`
- `UsuariosController`
- `VisitantesController`
- `VoluntariosController`

### Controllers sem arquivo de teste próprio

Total atual: `38 de 50`

- `AuthController`
- `CategoriasDespesasController`
- `CategoriasMidiasController`
- `CategoriasNoticiasController`
- `CategoriasPatrimonioController`
- `CategoriasReceitasController`
- `CentrosCustosController`
- `ConfiguracaoPortalController`
- `ContasBancariasController`
- `ContatosController`
- `DashboardController`
- `DashboardFinanceiroController`
- `DespesasController`
- `DestaquesSiteController`
- `EnquetesController`
- `EscalasController`
- `EscalasModelosController`
- `EventosController`
- `EventosOcorrenciasController`
- `EventosRecorrenciasController`
- `FornecedoresController`
- `GaleriasFotosController`
- `HubCasasController`
- `IndisponibilidadesVoluntariosController`
- `InscricoesEventosController`
- `KidsController`
- `MembrosController`
- `NoticiasController`
- `NotificacoesController`
- `PatrimonioController`
- `PatrimonioMovimentacoesController`
- `PessoasAniversariosCampanhaController`
- `ProjetosController`
- `ReceitasController`
- `RelatoriosFinanceirosController`
- `SearchController`
- `SolicitacoesTrocasEscalasController`
- `UploadController`

## Meta prática

### Curto prazo

- levar `Application + API + Domain` para pelo menos `35%`
- levar `Services + Controllers` para pelo menos `30%`

### Médio prazo

- levar `Application + API + Domain` para `50%+`
- ter cobertura de regressão para todos os fluxos críticos administrativos e operacionais

### Regra de qualidade

Não perseguir percentual cego.

O critério principal deve ser:

- regra crítica coberta
- autorização sensível coberta
- transição de estado coberta
- bug corrigido com teste de regressão
- fluxo operacional importante com cenário feliz e cenário de falha

## Ordem de execução recomendada

### Onda 1 - maior retorno imediato

Objetivo: proteger autorização, autenticação e fluxos mais quentes de negócio.

#### Services

- `AuthService`
- `UsuarioService`
- `PessoaService`
- `PessoaPerfilService`
- `CampanhaAniversarioService`
- `EscalaModeloService`

#### Controllers

- `AuthController`
- `EscalasController`
- `SolicitacoesTrocasEscalasController`
- `InscricoesEventosController`

#### Cenários obrigatórios

- acesso negado
- sucesso básico
- falha de validação
- transição de status
- retorno consistente em cenários de not found

### Onda 2 - administrativo e cadastros centrais

Objetivo: fechar as superfícies administrativas já endurecidas no backend.

#### Services

- `CargoService`
- `EquipeService`
- `VoluntarioService`
- `PerfilAcessoService`
- `CategoriaNoticiaService`
- `CategoriaMidiaService`
- `ContatoService`
- `FornecedorService`
- `NoticiaService`
- `EnqueteService`

#### Controllers

- `CategoriasNoticiasController`
- `CategoriasMidiasController`
- `NoticiasController`
- `EnquetesController`
- `ContatosController`
- `FornecedoresController`

### Onda 3 - financeiro

Objetivo: proteger regras de cadastro e movimentação financeira antes de crescer mais o módulo.

#### Services

- `ReceitaService`
- `DespesaService`
- `ProjetoService`
- `CentroCustoService`
- `ContaBancariaService`
- `CategoriaReceitaService`
- `CategoriaDespesaService`
- `CategoriaPatrimonioService`
- `PatrimonioItemService`
- `PatrimonioMovimentacaoService`
- `RelatorioFinanceiroService`
- `DashboardFinanceiroService`

#### Controllers

- `ReceitasController`
- `DespesasController`
- `ProjetosController`
- `CentrosCustosController`
- `ContasBancariasController`
- `CategoriasReceitasController`
- `CategoriasDespesasController`
- `CategoriasPatrimonioController`
- `PatrimonioController`
- `PatrimonioMovimentacoesController`
- `RelatoriosFinanceirosController`
- `DashboardFinanceiroController`

#### Cenários obrigatórios

- filtro e paginação
- criação/edição/exclusão
- status e transições
- regras de vínculo entre categoria, centro de custo, projeto e conta
- relatórios com retorno vazio e com dados

### Onda 4 - eventos e portal

Objetivo: reduzir risco em agenda, inscrições e portal administrativo.

#### Services

- `EventoService`
- `EventoOcorrenciaService`
- `EventoRecorrenciaService`
- `GaleriaFotoService`
- `DestaqueSiteService`
- `ConfiguracaoPortalService`
- `HubCasaService`
- `NotificacaoUsuarioService`

#### Controllers

- `EventosController`
- `EventosOcorrenciasController`
- `EventosRecorrenciasController`
- `GaleriasFotosController`
- `DestaquesSiteController`
- `ConfiguracaoPortalController`
- `HubCasasController`
- `NotificacoesController`

### Onda 5 - operação, auditoria e infraestrutura de apoio

Objetivo: cobrir o que afeta monitoramento, diagnóstico e mecanismos transversais.

#### Services

- `AuditLogService`
- `PermissionService`
- `SchedulerExecutionMonitor`
- `CadastroMembroNotificationService`
- `DashboardService`
- `EvolutionApiService`

#### Controllers

- `DashboardController`
- `SearchController`
- `UploadController`
- `PessoasAniversariosCampanhaController`
- `MembrosController`

## Backlog executável por prioridade

### Prioridade P1

- criar `AuthServiceTests`
- criar `UsuarioServiceTests`
- criar `PessoaServiceTests`
- criar `PessoaPerfilServiceTests`
- criar `CampanhaAniversarioServiceTests`
- criar `AuthControllerTests`
- criar `EscalasControllerTests`
- criar `SolicitacoesTrocasEscalasControllerTests`
- criar `InscricoesEventosControllerTests`

### Prioridade P2

- criar `EquipeServiceTests`
- criar `PerfilAcessoServiceTests`
- criar `CargoServiceTests`
- criar `VoluntarioServiceTests`
- criar `ReceitaServiceTests`
- criar `DespesaServiceTests`
- criar `ProjetoServiceTests`
- criar `ContaBancariaServiceTests`

### Prioridade P3

- criar `EventoServiceTests`
- criar `EventoOcorrenciaServiceTests`
- criar `EventoRecorrenciaServiceTests`
- criar `RelatorioFinanceiroServiceTests`
- criar `PatrimonioItemServiceTests`
- criar `PatrimonioMovimentacaoServiceTests`
- criar `AuditLogServiceTests`
- criar `PermissionServiceTests`

## Critério de aceite por arquivo novo de teste

Cada novo arquivo de teste deve cobrir pelo menos:

- cenário feliz principal
- cenário de validação
- cenário de acesso negado ou regra bloqueante, quando aplicável
- cenário de exceção ou retorno vazio, quando fizer sentido
- regressão de bug conhecido, se houver histórico

## Como medir a evolução

Comando base:

```bash
dotnet test BackEnd/tests/SistemaIgreja.API.Tests/SistemaIgreja.API.Tests.csproj --collect:"XPlat Code Coverage"
```

Leitura recomendada:

- acompanhar o número bruto apenas como sinal
- acompanhar principalmente a evolução em `Application + API + Domain`
- acompanhar avanço por arquivo prioritário, não só por percentual total

## Observação importante

`Kids` não entra como foco agora por decisão de produto e execução paralela em outra frente.

Os testes já existentes de `Kids` continuam válidos, mas a expansão de cobertura desse módulo deve ficar para uma onda posterior, quando o escopo estabilizar melhor.
