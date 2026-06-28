# Planejamento de Produto e Qualidade Contínua

## Resumo do que discutimos

### Prioridades de produto

#### Prioridade alta
- Voluntariado
- Kids / AppKids
- Área do membro
- Jornada 360 da Pessoa
- Comunicação omnichannel
- Qualidade contínua da plataforma

#### No radar para depois
- CRM pastoral / follow-up
- Financeiro e patrimônio avançados

### Ordem macro sugerida
1. Voluntariado
2. Kids / AppKids
3. Área do membro
4. Jornada 360 da Pessoa
5. Comunicação omnichannel
6. Evolução técnica contínua em paralelo

### Diretrizes de produto já decididas
- O líder de equipe pode e deve montar escala tanto manualmente quanto automaticamente.
- O fluxo principal de confirmação e recusa deve ser feito pelo próprio voluntário.
- O líder e o admin devem ter autonomia para agir como exceção operacional.
- O cargo do voluntário pertence ao vínculo entre pessoa, equipe e cargo; a escala não deve pedir escolha manual de cargo.
- O campo de ordem na escala não faz sentido como conceito de produto e não deve pesar na experiência.
- O modelo de escala não deve servir apenas ao preenchimento automático; ele também deve orientar a montagem manual, indicando cobertura necessária e faltas.
- A tela de montagem de escala precisava sair do formato de formulário técnico e migrar para um fluxo mais operacional, intuitivo, elegante e moderno.

## Frentes de produto já priorizadas

### 1. Voluntariado
Foco principal:
- montagem manual e automática por líder
- publicação da escala
- confirmação e recusa
- troca e substituição
- painel de cobertura e risco
- histórico operacional
- lembretes e notificações

### 2. Kids / AppKids
Prioridade forte logo após Voluntariado.

Foco sugerido:
- avisos reais para pais
- retirada segura
- painel operacional do culto
- ocorrências e histórico
- evolução do app mobile como canal principal da jornada dos responsáveis

### 3. Área do membro
Próxima frente grande após Voluntariado e Kids.

Foco sugerido:
- atualizar cadastro
- ver escalas e participações
- acompanhar eventos
- receber avisos
- acessar funcionalidades de autosserviço

### 4. Jornada 360 da Pessoa
Já priorizada para próxima fase de evolução.

Objetivo:
- visão unificada da pessoa
- vínculos com eventos, voluntariado, kids, comunicação e histórico relacional

### 5. Comunicação omnichannel
Também priorizada.

Foco sugerido:
- templates
- segmentação
- campanhas por canal
- automações por gatilho
- métricas
- preferências e consentimentos

## Qualidade Contínua como prioridade alta

Essa frente não é um bloco isolado para "um dia". Ela deve caminhar junto das entregas de produto, principalmente Voluntariado e Kids. A lógica é simples: se o sistema cresce sem base operacional e técnica, o custo de manutenção, suporte e retrabalho sobe muito.

As frentes principais são:
- testes
- observabilidade
- auditoria
- permissões e segurança
- saúde operacional
- higiene de frontend e portal

## Manifesto de Qualidade Contínua

### O que esta frente significa

Qualidade contínua no AppIgreja não é uma camada opcional nem uma esteira paralela. Ela faz parte da entrega. Sempre que um fluxo importante entra no sistema, ele precisa entrar com o mínimo necessário de proteção técnica e operacional para continuar evoluindo sem virar fonte de retrabalho.

### Princípios práticos

- toda funcionalidade relevante deve nascer com um pacote mínimo de qualidade
- risco operacional vale mais do que volume de entrega
- fluxo crítico sem rastreabilidade não está realmente pronto
- permissão sem escopo de negócio não é segurança suficiente
- tela pronta sem estado de erro, vazio e carregamento ainda está incompleta
- integração sem visibilidade de falha transfere o custo para suporte e operação
- o objetivo não é perseguir perfeição abstrata; é reduzir risco real e aumentar confiança para evoluir

## Boas práticas de desenvolvimento

Estas práticas devem valer para evolução nova, refatoração e estabilização do AppIgreja. A intenção não é burocratizar entrega, mas evitar que o projeto cresça com acoplamento excessivo, duplicação e regra espalhada.

### Princípios de engenharia que devem guiar novas funcionalidades

- aplicar SOLID de forma pragmática, principalmente responsabilidade única, baixo acoplamento e dependência de abstrações
- aplicar Clean Code com foco em clareza, intenção do código, nomes bons, funções curtas e redução de ruído
- aplicar separação de responsabilidades entre domínio, aplicação, infraestrutura e interface
- favorecer coesão alta e dependências explícitas
- evitar duplicação relevante seguindo DRY, sem cair em abstração prematura
- preferir KISS em vez de desenhar soluções mais complexas do que o problema exige
- organizar evolução por comportamento de negócio e não por improviso estrutural
- proteger regras centrais do sistema atrás de serviços, políticas e contratos bem definidos
- manter código orientado a mudança segura: fácil de testar, auditar, observar e evoluir

### Regras objetivas para backend

- regra de negócio deve viver em serviço de aplicação/domínio, não espalhada em controller
- controller deve orquestrar entrada, autorização, resposta e delegação, sem concentrar regra operacional
- repositório deve cuidar de persistência; não deve acumular decisões de negócio
- integrações externas devem ficar atrás de interfaces claras e reutilizáveis
- validações de autorização e escopo devem ser explícitas e próximas da regra sensível
- efeitos colaterais relevantes devem ser observáveis por log e, quando necessário, por auditoria
- novos fluxos devem nascer com contratos e DTOs coerentes, sem vazar detalhe interno desnecessário
- preferir serviços pequenos e compostos em vez de classes gigantes com múltiplos motivos para mudar

### Regras objetivas para frontend e portal

- separar composição visual, consumo de API e regra operacional sempre que a tela começar a crescer demais
- evitar componentes monolíticos quando já houver sinais de difícil leitura ou manutenção
- centralizar padrões de feedback, erro, loading e estados vazios
- evitar duplicar transformação de dados e regras de status em várias telas
- nomes, labels e ações devem refletir linguagem de negócio, não termos técnicos internos
- toda tela nova deve ser legível para quem mantém e previsível para quem usa

### Padrões de mercado que devemos adotar como referência

- arquitetura em camadas com fronteiras claras
- contratos bem definidos entre módulos
- observabilidade desde o desenho do fluxo
- segurança por padrão e escopo por contexto
- testes cobrindo regra crítica e regressão real
- design orientado a tarefa nas interfaces operacionais
- documentação curta e útil para homologação e operação

### Regra permanente para novas funcionalidades

Toda nova funcionalidade do AppIgreja deve aplicar, de forma proporcional ao tamanho da entrega:

- princípios SOLID
- práticas de Clean Code
- separação clara de responsabilidades
- padrão de observabilidade mínimo
- revisão de permissão e segurança
- tratamento consistente de estados e erros

Se a entrega for pequena, aplicar o conjunto mínimo de forma leve. Se a entrega for sensível ou estrutural, aplicar isso como requisito explícito de aceite.

### Política permanente de testes após a campanha inicial de cobertura

A campanha ampla de cobertura do backend cumpriu o papel principal nas camadas mais importantes para evolução segura do produto.

Leitura oficial do momento:

- `Domain`, `Application` e `API` já estão em patamar suficiente para sair do modo de campanha agressiva
- `Infrastructure` continua como frente aberta, mas deve ser tratada com seleção cirúrgica
- o percentual bruto geral não deve mais ser usado sozinho como meta de produto

Daqui em diante, a política oficial é:

- toda feature nova relevante sai com teste automatizado
- todo bug corrigido em fluxo importante ganha teste de regressão
- lacunas antigas entram no backlog apenas quando forem de módulo prioritário ou risco real
- `Infrastructure` deve crescer em cobertura só onde houver lógica operacional, query relevante, risco de suporte ou histórico de falha
- não vale perseguir cobertura alta por cobertura, nem gastar energia com CRUD trivial, wiring ou boilerplate de baixo risco

### Checklist operacional para decidir teste novo

Abrir teste novo quando pelo menos um destes pontos for verdadeiro:

- a entrega cria ou altera regra de negócio
- a entrega muda autorização, escopo ou permissão
- a entrega altera transição de status, workflow ou comportamento operacional
- a entrega toca integração, scheduler, worker, query relevante ou infraestrutura com risco real
- a entrega corrige bug em fluxo importante
- a entrega mexe em área crítica de produto, como `Voluntariado`, `Financeiro`, `Eventos`, `Auth`, `Comunicação`, `Kids` ou `Auditoria`

Não abrir teste novo só para aumentar percentual quando o caso for principalmente:

- CRUD trivial sem regra adicional
- wiring, boilerplate ou código passivo de baixo risco
- detalhe visual sem lógica relevante
- refatoração neutra já protegida por testes existentes

Em caso de dúvida, usar esta regra curta:

- se a mudança pode quebrar operação, suporte, segurança ou confiança do produto, ela deve sair com teste

### Regra de decisão

Se uma entrega nova aumenta exposição de operação, suporte, segurança ou manutenção, ela deve sair acompanhada por contrapartidas explícitas em pelo menos parte destas frentes:

- testes
- observabilidade
- auditoria
- permissões e segurança
- saúde operacional
- higiene de frontend e portal

## Definition of Done de Qualidade

Este checklist deve acompanhar features novas e mudanças relevantes, sobretudo em Voluntariado, Kids e Area do Membro.

### Checklist minimo por entrega

- [ ] Regra principal de negócio coberta por teste automatizado ou checklist de regressão documentado.
- [ ] Fluxos de sucesso, erro e bloqueio revisados.
- [ ] Logs suficientes para entender falhas sem depender do relato do usuário.
- [ ] Ações sensíveis auditadas quando houver efeito administrativo, operacional ou financeiro.
- [ ] Permissões e escopo de acesso revisados no backend e validados na UX.
- [ ] Estados de loading, erro e vazio tratados no frontend.
- [ ] Mensagens de erro e feedback ao usuário estão claras e não técnicas.
- [ ] Endpoint, job ou tela crítica tem diagnóstico operacional minimamente viável.
- [ ] Não foram deixados resíduos visíveis de depuração, labels internas ou inconsistências de UX.

### Checklist reforcado para fluxos criticos

Aplicar este complemento quando a funcionalidade mexer com escala, confirmacao, check-in/check-out, avisos, permissoes, jobs, usuarios ou dados sensiveis.

- [ ] Existe trilha clara de quem executou a ação e quando.
- [ ] Existe validação de escopo por pessoa, equipe ou contexto de negócio.
- [ ] Existe cobertura de cenários de exceção e negação de acesso.
- [ ] Existe forma de identificar rapidamente falha em integração, job ou automação.
- [ ] Existe roteiro curto de homologação para o fluxo.

### Como aplicar na prática

- mudança pequena: usar o checklist mínimo
- fluxo novo ou sensível: usar checklist mínimo + checklist reforçado
- refatoração de estabilização: priorizar o item de qualidade que mais reduz risco imediato

## Backlog inicial priorizado de Qualidade Contínua

Documento operacional complementar desta trilha:

- [PLANO_COBERTURA_TESTES_BACKEND.md](/Users/aurelioromeu/repos/AppIgreja/PLANO_COBERTURA_TESTES_BACKEND.md)

Este backlog serve para iniciar a frente agora, sem esperar uma fase separada.

### Triagem atual do backlog de testes

Com a campanha recente de cobertura já consolidada, o backlog de testes passa a ser lido assim:

#### Continua prioritário

- testes novos para features novas e bugs corrigidos
- `Infrastructure` crítica com lógica operacional real
- testes de integração seletivos para fluxos ponta a ponta mais sensíveis
- módulos que ainda estiverem mudando com risco de regressão forte

#### Muda de natureza

- `API`, `Application` e `Domain` deixam de pedir campanha ampla
- nessas camadas, novas lacunas entram apenas por risco real, histórico de falha ou mudança relevante de comportamento
- expansão de cobertura passa a ser guiada por evolução do produto, não por meta de percentual

#### Pode sair da fila por enquanto

- CRUD trivial sem regra adicional
- wiring, boilerplate e código passivo de baixo risco
- testes criados apenas para empurrar cobertura
- backlog antigo de cobertura ampla em áreas já estabilizadas e bem protegidas

### Triagem atual do backlog de qualidade contínua

Com o avanço já feito em testes, auditoria, permissões, operação e higiene de frontend, o backlog transversal de qualidade passa a ser lido assim:

#### Continua prioritário

- qualidade que acompanha feature nova ou módulo ainda em mudança forte
- `Infrastructure` crítica, integrações, schedulers, workers e pontos com histórico de falha
- testes de integração seletivos para fluxos ponta a ponta prioritários
- permissões contextuais em módulos ainda evoluindo
- observabilidade e auditoria em fluxos novos ou sensíveis
- higiene de frontend em telas novas, áreas críticas e pontos com manutenção cara

#### Entra em modo de manutenção saudável

- `API`, `Application` e `Domain` no backend
- trilha de auditoria já implantada nas superfícies administrativas centrais
- health, operação e logs mínimos já existentes
- padronização base de estados, feedback visual e permissões no frontend administrativo

Nessas frentes, a regra deixa de ser campanha ampla e passa a ser:

- evoluir junto com feature nova
- reforçar quando houver bug, suporte recorrente ou risco real
- revisar quando o módulo voltar a mudar de forma importante

#### Pode sair da fila por enquanto

- limpeza estrutural sem risco real imediato
- novas ondas amplas de cobertura apenas para subir percentual
- refino visual ou técnico em telas já estáveis sem dor operacional concreta
- endurecimento adicional onde já existe proteção suficiente e sem mudança relevante no módulo

### Faixa 1 - imediata

Itens para começar junto das próximas entregas, porque reduzem risco real do sistema atual.

Status atual:

- `concluído ou bem coberto`: base crítica de testes de Voluntariado, logs de fluxos críticos, auditoria administrativa central, revisão forte de permissões, health/operacao e primeira grande onda de higiene de frontend
- `ativo`: checklist de regressão manual onde ainda fizer sentido e ajustes pontuais de permissão/observabilidade em módulos que continuarem mudando
- `pausado`: base equivalente de `Kids`, aguardando estabilização maior da outra frente paralela

#### Testes

- Criar cobertura automatizada para regras críticas de `EscalaService`.
- Criar cobertura para `SolicitacaoTrocaEscalaService`.
- Documentar checklist curto de regressão manual para fluxos de Voluntariado já em produção interna.
- Preparar base equivalente para os primeiros fluxos de Kids antes de ampliar funcionalidades.

#### Observabilidade

- Padronizar logs dos fluxos críticos de Voluntariado com contexto de usuário, equipe, ocorrência e escala.
- Padronizar logs dos jobs e serviços em background mais sensíveis.
- Identificar integrações que ainda falham de forma silenciosa.
- Criar convenção de logs de erro, warning e evento de negócio.

#### Auditoria

- Garantir auditoria para publicar escala, confirmar/recusar por liderança, aprovar troca e aplicar exceção manual.
- Revisar se alterações relevantes de permissões e perfis já deixam trilha suficiente.

#### Permissões e segurança

- Revisar escopo por equipe no Voluntariado endpoint por endpoint.
- Revisar escopo por pessoa em `Minhas Escalas`.
- Mapear papéis reais do sistema: admin, líder, voluntário, responsável do Kids e membro.
- Levantar endpoints sensíveis com risco de acesso permissivo demais.

#### Saúde operacional

- Melhorar diagnóstico do ambiente local para evitar perda de tempo com falso problema de CORS, porta ou configuração.
- Revisar health endpoint para incluir dependências críticas.
- Levantar jobs existentes, periodicidade, falhas possíveis e visibilidade atual.

#### Higiene de frontend e portal

- Padronizar estados de loading, erro e vazio nas telas principais de Voluntariado.
- Remover resíduos visíveis de depuração e labels técnicos.
- Consolidar padrões de badges, status, toasts e mensagens de erro.

### Faixa 2 - logo depois

Itens que devem entrar assim que a base mínima acima estiver caminhando.

Status atual:

- `ativo`: testes de integração seletivos, correlação/métricas operacionais quando houver ganho claro, matriz de permissões por módulo e revisão de rotas públicas sensíveis
- `ativo sob demanda`: melhorias adicionais na leitura da auditoria e reprocessamento manual de jobs onde a operação realmente precisar
- `pausado`: suíte inicial mais ampla de `Kids` e novos testes de frontend fora das áreas que voltarem a sofrer mudança relevante

#### Testes

- Adicionar testes de frontend para fluxos operacionais centrais de Voluntariado.
- Criar suíte inicial para check-in/check-out do Kids.
- Transformar roteiros manuais mais usados em checklists oficiais por módulo.

#### Observabilidade

- Criar correlação por request para facilitar diagnóstico ponta a ponta.
- Definir métricas mínimas por módulo crítico.
- Preparar visão simples de erros e jobs processados.

#### Auditoria

- Melhorar consulta e leitura dos logs de auditoria no administrativo.
- Destacar ações excepcionais e administrativas no histórico.

#### Permissões e segurança

- Consolidar matriz de permissões de negócio por módulo.
- Revisar rotas públicas, autenticação e consistência entre backend e frontend.

#### Saúde operacional

- Criar checklist operacional de homologação e ambiente.
- Adicionar reprocessamento manual para jobs falhos onde fizer sentido.

#### Higiene de frontend e portal

- Revisar portal público para consistência de consumo de API e tratamento de falhas.
- Quebrar telas grandes demais quando a manutenção estiver cara.

### Faixa 3 - estrutural e contínua

Itens que não precisam bloquear produto, mas devem permanecer no radar ativo.

Status atual:

- `ativo`: incorporar qualidade como critério fixo de aceite, revisar permissões contextuais quando módulos evoluírem e expandir observabilidade onde houver risco real
- `manutenção saudável`: padrão visual/operacional do admin e a maior parte da cobertura já consolidada em `API`, `Application` e `Domain`
- `pausado por enquanto`: novas campanhas amplas de cobertura ou refino estrutural sem dor operacional concreta

- Expandir cobertura de testes por módulo e por regressões históricas.
- Evoluir observabilidade para dashboard por domínio de negócio.
- Fortalecer auditoria em módulos administrativos e financeiros.
- Revisar continuamente segurança de permissões contextuais.
- Criar padrão de qualidade visual e operacional para Admin, Portal e AppKids.
- Incorporar qualidade como critério fixo de aceite de roadmap futuro.

## Quadro resumido atual

### Ativo agora

- `Infrastructure` crítica, integrações, schedulers, workers e query paths com risco real
- testes de integração seletivos para fluxos ponta a ponta prioritários
- permissões contextuais em módulos que continuarem evoluindo
- observabilidade e auditoria em fluxos novos ou sensíveis
- higiene de frontend nas telas novas, críticas ou com manutenção cara
- checklist de regressão manual e operacional onde ainda agregar valor

### Pausado

- expansão mais ampla de `Kids`, enquanto a outra frente ainda estabiliza escopo
- campanhas amplas de cobertura só para elevar percentual
- novas ondas grandes de refino estrutural sem dor operacional concreta
- testes de frontend fora das áreas que realmente voltarem a mudar

### Já incorporado como regra

- `API`, `Application` e `Domain` deixam o modo campanha e passam a crescer por feature nova, bug corrigido ou risco real
- feature nova relevante deve sair com teste
- bug corrigido em fluxo importante deve ganhar regressão
- health, operação, auditoria administrativa central e base de permissões já fazem parte do padrão mínimo
- qualidade contínua passa a ser critério fixo de aceite, não frente paralela eventual

## Próximos 5 itens ativos

1. testes de integração seletivos para `login`, permissões administrativas, `escala` e `troca de escala`
2. cobertura adicional de `Infrastructure` apenas nas classes com maior risco operacional real
3. consolidar matriz de permissões por módulo e revisar rotas públicas sensíveis que ainda mudarem
4. evoluir observabilidade e auditoria somente nos fluxos novos ou que continuarem gerando risco operacional
5. aplicar higiene de frontend apenas nas telas que ainda sofrerem mudança relevante ou tenham custo alto de manutenção

## Pacote minimo por frente de produto

### Voluntariado

- testes das regras de escala, troca, publicação e conflito
- auditoria das ações de liderança e exceções
- revisão de permissões por equipe e por usuário
- logs e diagnóstico dos fluxos de publicação, lembretes e automação
- refinamento contínuo da UX operacional

### Kids / AppKids

- testes dos fluxos de check-in, check-out e avisos
- revisão de escopo por responsável, criança e equipe
- observabilidade de notificações, retirada e integrações
- auditoria de ações administrativas e operacionais sensíveis
- UX confiável para estados de rede, erro e confirmação

### Area do Membro

- autenticação e escopo por pessoa como requisito de base
- auditoria de atualizações de cadastro e ações sensíveis
- tratamento consistente de estados e feedbacks
- observabilidade suficiente para autosserviço sem suporte manual excessivo

## 1. Testes

### Objetivo
Garantir confiança para evoluir módulos críticos sem medo de regressão.

### Onde concentrar primeiro
- regras de escala e cobertura do voluntariado
- confirmação, recusa, troca e substituição
- permissões por perfil e por equipe
- check-in e retirada no Kids
- campanhas automáticas e lembretes
- rotas e operações financeiras críticas quando esse módulo voltar para a fila

### Estratégia prática

#### Backend
- ampliar testes de serviço e regras de negócio
- cobrir cenários felizes, cenários de bloqueio e exceções
- testar conflitos de pessoa na mesma ocorrência
- testar indisponibilidade, carga recente e folgas
- testar quem pode confirmar, recusar, aprovar troca e publicar escala

#### Frontend
- adicionar testes para telas críticas e comportamentos de estado
- focar primeiro em componentes e fluxos com regra operacional
- validar renderização por status, cobertura, bloqueio e permissão

#### Testes de regressão guiados por fluxo
- criar checklists por módulo
- transformar os fluxos principais em casos repetíveis
- usar esses checklists antes de deploy ou mudanças maiores

### Backlog sugerido
- testes unitários para `EscalaService`
- testes para `SolicitacaoTrocaEscalaService`
- testes para endpoints de confirmar/recusar/publicar
- testes de permissão por líder de equipe
- testes do fluxo de `Minhas Escalas`
- testes para renderização de status no editor de escala

### Critério de prioridade
- primeiro o que quebra operação real
- depois o que quebra confiança do usuário
- por último o que é mais cosmético

## 2. Observabilidade

### Objetivo
Fazer o sistema contar o que está acontecendo sem depender só de olhar log bruto ou descobrir erro via usuário.

### O que precisa existir
- logs estruturados e consistentes
- correlação entre requisição, usuário e ação
- visibilidade de falhas em jobs e integrações
- métricas básicas do sistema
- trilha de erros por módulo

### O que observar primeiro

#### Jobs e serviços em background
- lembretes de escala
- campanhas de aniversário
- geração de ocorrências
- mensageria

#### Integrações externas
- Evolution API
- envio de mensagens
- push
- e-mail

#### Operações de negócio críticas
- publicação de escala
- confirmação e recusa
- aprovação de troca
- criação de ocorrência
- retirada no Kids no futuro

### Melhorias práticas
- padronizar logs com contexto: módulo, entidade, usuário, equipe, ocorrência
- registrar falha com mensagem clara e dados mínimos para diagnóstico
- diferenciar warning de erro real
- criar métricas simples:
  - quantidade de falhas por job
  - quantidade de notificações enviadas
  - quantidade de recusas e pendências
  - latência de chamadas críticas

### Backlog sugerido
- revisar serviços de background para logs padronizados
- criar identificador de correlação por request
- adicionar logs de eventos de negócio críticos
- mapear integrações que falham silenciosamente
- preparar dashboard simples de erros e jobs

## 3. Auditoria

### Objetivo
Saber quem fez o quê, quando fez e em qual contexto, principalmente em operações sensíveis.

### Onde a auditoria precisa ser mais forte
- publicação de escala
- confirmação e recusa feitas por liderança
- trocas e aprovações
- mudanças de permissões
- alterações em cadastros sensíveis
- ações financeiras e patrimoniais futuramente

### O que uma auditoria boa deve mostrar
- usuário responsável
- data e hora
- módulo
- entidade alterada
- ação executada
- antes e depois, quando fizer sentido
- motivo ou observação em casos excepcionais

### Evoluções recomendadas
- filtros por módulo e usuário
- busca por entidade
- visualização mais legível do que mudou
- destaque para ações administrativas e exceções manuais

### Backlog sugerido
- fortalecer rastreio de trocas de escala
- auditar ações de exceção de conflito
- auditar confirmações/recusas feitas por líder/admin
- melhorar consulta dos logs de auditoria no painel administrativo

## 4. Permissões e segurança

### Objetivo
Garantir que cada pessoa faça apenas o que deve fazer, com escopo correto e sem brechas.

### Frentes prioritárias

#### Escopo por equipe
- líder deve operar apenas equipes sob sua liderança
- não basta permissão de módulo; precisa permissão contextual

#### Escopo por pessoa
- voluntário deve ver e responder apenas as próprias escalas
- pais do Kids devem ver apenas suas crianças e avisos relacionados

#### Operações sensíveis
- aprovações
- exceções manuais
- gestão de usuários
- módulos financeiros futuros

### Revisões importantes
- endpoints expostos com permissão permissiva demais
- diferença entre `AllowAnonymous`, autenticado e autorizado
- consistência entre backend e frontend
- prevenção de escalonamento indevido por URL direta

### Melhorias recomendadas
- revisar cobertura de permissão por rota
- criar matriz de permissões por papel real de negócio
- reforçar escopo por equipe e por pessoa
- revisar operações administrativas com override
- avaliar logs de acesso negado e padrões suspeitos

### Backlog sugerido
- revisar controladores do Voluntariado
- revisar endpoints públicos e sem autenticação
- consolidar perfis como admin, líder, voluntário, responsável do kids
- mapear rotas críticas e validar uma a uma

## 5. Saúde operacional

### Objetivo
Ter previsibilidade de funcionamento do sistema em produção e em homologação.

### O que isso inclui
- health checks
- estado dos jobs
- monitoramento de fila e scheduler
- verificação de conectividade com integrações
- capacidade de reprocessar falhas
- segurança de backup e restore

### O que precisa aparecer com clareza
- API saudável ou não
- banco acessível ou não
- integrações externas respondendo ou não
- jobs executando ou falhando
- serviços críticos parados

### Cuidados especiais no projeto
- serviços em background não devem derrubar o host inteiro por falha tratável
- falhas transitórias de integração não podem se mascarar como erro de CORS ou indisponibilidade total
- problemas de porta local e ambiente precisam ser diagnosticáveis mais rápido

### Backlog sugerido
- health endpoint mais completo
- painel simples de jobs e últimos processamentos
- reprocessamento manual de jobs falhos
- política melhor para exceções em background services
- checklist de ambiente local e homologação

## 6. Higiene de frontend e portal

### Objetivo
Melhorar clareza, consistência, manutenção e resiliência da camada de interface.

### O que isso quer dizer na prática
- remover ruído e improvisos deixados no código
- reduzir lógica duplicada
- melhorar estados de loading, erro e vazio
- padronizar UX de feedback
- manter breadcrumbs, labels e ações coerentes

### Pontos que mais merecem atenção
- telas públicas do portal com logs e tratamentos inconsistentes
- componentes grandes demais e difíceis de manter
- comportamentos que parecem bug por falta de feedback visual
- labels técnicos aparecendo para usuário final
- páginas operacionais com cara de formulário técnico em vez de fluxo humano

### Evoluções recomendadas
- limpar `console.log` e resíduos de depuração
- padronizar toasts, mensagens de erro e estados vazios
- separar regras de negócio da composição visual quando a tela crescer demais
- revisar nomenclatura de botões, badges, filtros e breadcrumbs
- deixar as telas mais orientadas a tarefa e menos orientadas a estrutura interna

### Backlog sugerido
- revisão contínua das telas do Voluntariado
- revisão da experiência do portal público
- padronização de componentes de status, badges e ações
- refino dos fluxos do Kids assim que a frente começar

## Como executar essa frente sem travar produto

### Regra prática
Cada entrega relevante de produto deve carregar pelo menos um avanço de qualidade junto.

Exemplos:
- ao evoluir Voluntariado, adicionar testes das regras novas e auditoria das ações sensíveis
- ao evoluir Kids, incluir observabilidade do check-in e revisão de permissões por responsável
- ao evoluir Área do Membro, reforçar autenticação, escopo de acesso e estados de interface

### Cadência recomendada
- toda sprint ou bloco de entrega deve reservar parte da capacidade para qualidade
- não deixar esse tema para uma fase separada e abstrata
- medir progresso por redução de risco operacional, não só por número de tarefas técnicas fechadas

## Plano de execução em sprints e entregáveis

O objetivo deste plano é fazer Qualidade Contínua sair do discurso e entrar na cadência real do produto, caminhando junto das frentes prioritárias.

### Sprint 1 - Fundamentos de execução

Objetivo:
- transformar qualidade em padrão operacional da equipe

Entregáveis:
- manifesto e diretrizes permanentes de engenharia e qualidade formalizados
- checklist de `Definition of Done` adotável por feature
- backlog priorizado de qualidade contínua
- mapeamento inicial de papéis de negócio e riscos de permissão
- levantamento dos fluxos críticos atuais de Voluntariado e Kids

Critério de conclusão:
- existe referência única de como novas funcionalidades devem ser construídas e validadas

### Sprint 2 - Base crítica de Voluntariado

Objetivo:
- reduzir risco do módulo mais prioritário já em evolução

Entregáveis:
- cobertura de testes para regras críticas de escala e troca
- checklist de regressão oficial do Voluntariado
- revisão de escopo por equipe e por pessoa nos fluxos principais
- auditoria mínima para publicação, exceções manuais e ações administrativas
- padronização inicial de logs dos fluxos críticos do módulo

Critério de conclusão:
- operações centrais de Voluntariado ficam mais seguras para evoluir sem regressão invisível

### Sprint 3 - Saúde operacional e observabilidade mínima

Objetivo:
- dar previsibilidade de funcionamento e facilitar diagnóstico

Entregáveis:
- health checks mais completos para dependências críticas
- padrão de logs de erro, warning e evento de negócio
- levantamento e documentação dos jobs/serviços em background
- checklist de ambiente local e homologação
- diagnóstico mais claro para falhas de configuração, porta e integração

Critério de conclusão:
- equipe consegue diferenciar mais rápido falha de código, ambiente e integração

### Sprint 4 - Higiene estrutural de frontend e portal

Objetivo:
- melhorar consistência e reduzir custo de manutenção da interface

Entregáveis:
- padronização de loading, erro e vazio nas telas críticas
- padronização de toasts, badges, labels e mensagens de erro
- limpeza de resíduos de depuração e ruído visual
- revisão das telas operacionais mais sensíveis do Voluntariado
- lista de componentes ou páginas que precisam ser quebrados/refatorados

Critério de conclusão:
- interfaces críticas ficam mais previsíveis para o usuário e mais sustentáveis para manutenção

### Sprint 5 - Base de qualidade para Kids / AppKids

Objetivo:
- iniciar a frente de Kids já com fundação mais madura do que a usada no começo do Voluntariado

Entregáveis:
- checklist de qualidade aplicado aos fluxos de check-in, check-out e avisos
- revisão de escopo por responsável, criança e operador
- testes iniciais dos fluxos críticos de Kids
- observabilidade mínima para notificações e retirada
- auditoria das ações administrativas e operacionais sensíveis

Critério de conclusão:
- Kids nasce ou evolui sem repetir os mesmos vazios operacionais e técnicos

### Sprint 6 - Consolidação transversal

Objetivo:
- deixar qualidade contínua incorporada ao modo de trabalhar

Entregáveis:
- matriz de permissões por módulo e papel
- padrão de revisão técnica para novas funcionalidades
- backlog contínuo de refatoração guiado por risco e uso real
- visão consolidada dos principais pontos de observabilidade e auditoria
- critério formal de aceite para roadmap futuro incluindo qualidade mínima

Critério de conclusão:
- novas entregas já passam a nascer com os pilares mínimos incorporados

### Cadência de entregáveis por sprint

Cada sprint deve entregar os dois tipos abaixo:

- entregável de produto ou estabilização funcional
- entregável explícito de qualidade contínua

### Regra de capacidade

Sugestão prática:

- reservar parte fixa da sprint para qualidade contínua
- atrelar pelo menos um entregável de qualidade a cada frente ativa
- priorizar primeiro o que reduz risco operacional, regressão e suporte manual

## Backlog operacional por sprint

Esta seção traduz o plano em execução prática. A ideia é sair de uma orientação conceitual para um backlog que possa ser distribuído, acompanhado e homologado.

## Artefatos criados na Sprint 1

- [QUALIDADE_SPRINT1_FLUXOS_E_RISCOS.md](/Users/aurelioromeu/repos/AppIgreja/QUALIDADE_SPRINT1_FLUXOS_E_RISCOS.md)
- [QUALIDADE_SPRINT1_PAPEIS_E_PERMISSOES.md](/Users/aurelioromeu/repos/AppIgreja/QUALIDADE_SPRINT1_PAPEIS_E_PERMISSOES.md)
- [CHECKLIST_ENTREGA_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/CHECKLIST_ENTREGA_QUALIDADE.md)

### Sprint 1 - Fundamentos de execução

#### Tarefas

- consolidar o documento de qualidade contínua como referência oficial do projeto
- alinhar o `Definition of Done` mínimo para backend, frontend e portal
- mapear fluxos críticos atuais de Voluntariado e Kids
- mapear papéis de negócio e riscos iniciais de permissão
- identificar jobs, integrações externas e pontos de operação crítica já existentes

#### Entregáveis

- guia oficial de qualidade e engenharia
- checklist de entrega por feature
- mapa inicial de riscos por módulo
- lista de fluxos críticos priorizados

#### Dependências

- entendimento mínimo das frentes já em andamento
- acesso ao código do backend, frontend, portal e AppKids

#### Critério de aceite

- a equipe consegue responder com clareza:
  - quais fluxos são críticos
  - quais papéis existem
  - quais regras mínimas toda nova entrega deve seguir

### Sprint 2 - Base crítica de Voluntariado

#### Tarefas

- criar testes para regras centrais de `EscalaService`
- criar testes para `SolicitacaoTrocaEscalaService`
- documentar checklist de regressão funcional do Voluntariado
- revisar permissões dos endpoints principais do módulo
- revisar escopo por equipe e por pessoa nas telas e APIs mais sensíveis
- garantir auditoria mínima para publicação, exceções e ações administrativas
- padronizar logs dos fluxos críticos de escala

#### Entregáveis

- suíte inicial de testes críticos do Voluntariado
- checklist oficial de regressão do módulo
- revisão documentada de permissões do módulo
- auditoria mínima funcionando nos fluxos sensíveis
- logs mais legíveis para escala, troca e publicação

#### Dependências

- estabilidade mínima dos fluxos atuais do Voluntariado
- definição de quais endpoints e telas são prioritários

#### Critério de aceite

- as operações centrais de escala, troca e publicação têm cobertura mínima de testes ou checklist oficial
- ações sensíveis deixam trilha
- escopo de acesso principal foi revisado

### Sprint 3 - Saúde operacional e observabilidade mínima

#### Tarefas

- revisar e ampliar o health endpoint
- mapear dependências críticas que devem aparecer no health check
- padronizar convenção de logs para erro, warning e evento de negócio
- revisar comportamento dos background services diante de falhas tratáveis
- documentar ambiente local e homologação com foco em diagnóstico rápido
- mapear integrações com falhas silenciosas e propor tratamento

#### Entregáveis

- health check mais útil para operação
- convenção mínima de logs adotada
- checklist de ambiente local e homologação
- inventário básico de jobs e integrações críticas

#### Dependências

- conhecimento dos serviços em background e integrações externas
- acesso aos pontos atuais de configuração

#### Critério de aceite

- a equipe consegue diagnosticar com mais rapidez se a falha está em:
  - código
  - ambiente
  - integração
  - scheduler ou job

### Sprint 4 - Higiene estrutural de frontend e portal

#### Tarefas

- padronizar estados de loading, erro e vazio nas telas críticas
- revisar toasts, mensagens de erro, badges e labels
- remover resíduos de depuração e textos técnicos visíveis ao usuário
- identificar telas grandes demais e separar responsabilidades onde já houver custo alto de manutenção
- revisar telas operacionais do Voluntariado com foco em clareza de tarefa
- revisar consumo de API e tratamento de falhas no portal

#### Entregáveis

- padrão inicial de feedback visual e estados de tela
- telas críticas com menos ruído e maior consistência
- lista priorizada de refatorações estruturais de frontend

#### Dependências

- levantamento das telas com maior dor operacional e manutenção
- alinhamento de padrão visual mínimo entre admin e portal

#### Critério de aceite

- as telas críticas têm comportamento previsível em carregamento, erro e vazio
- há menos duplicação visível de padrão de status e feedback
- o usuário final deixa de ver labels e resíduos técnicos evitáveis

### Sprint 5 - Base de qualidade para Kids / AppKids

#### Tarefas

- mapear fluxos críticos de Kids e AppKids
- revisar permissões e escopo por responsável, criança e operador
- criar testes iniciais para check-in, check-out e avisos
- garantir observabilidade mínima para notificações e retirada
- definir auditoria mínima para ações administrativas sensíveis do módulo
- formalizar checklist de regressão do Kids

#### Entregáveis

- mapa de risco e permissão do Kids
- suíte inicial de testes críticos do módulo
- checklist de regressão do Kids
- logs mínimos e trilha de auditoria nos pontos sensíveis

#### Dependências

- definição dos primeiros fluxos de Kids a estabilizar
- clareza dos endpoints e responsabilidades atuais do módulo

#### Critério de aceite

- o módulo começa a evoluir com base técnica mais segura do que no início do Voluntariado
- há clareza de quem pode ver, fazer e auditar cada ação principal

### Sprint 6 - Consolidação transversal

#### Tarefas

- consolidar matriz de permissões por módulo e papel
- consolidar padrão de revisão técnica para novas funcionalidades
- organizar backlog contínuo de refatoração guiado por risco e impacto
- definir critério fixo de aceite de qualidade para roadmap futuro
- revisar lacunas remanescentes de observabilidade, auditoria e testes nos módulos prioritários

#### Entregáveis

- matriz de permissões do sistema
- padrão operacional de revisão técnica
- backlog contínuo de refatoração priorizado
- critério formal de aceite de qualidade incorporado ao roadmap

#### Dependências

- aprendizados das sprints anteriores
- visão consolidada dos módulos críticos

#### Critério de aceite

- a equipe passa a tratar qualidade contínua como parte do fluxo normal de entrega
- novas funcionalidades já nascem com critérios mínimos claros

## Quadro resumido de execução

### Sprint 1

- foco: fundação
- saída principal: padrão oficial de qualidade e mapa de risco inicial

### Sprint 2

- foco: Voluntariado
- saída principal: testes, auditoria, permissões e logs mínimos do módulo

### Sprint 3

- foco: operação
- saída principal: health check, logs padronizados e diagnóstico de ambiente

### Sprint 4

- foco: frontend e portal
- saída principal: consistência visual e estrutural das telas críticas

### Sprint 5

- foco: Kids / AppKids
- saída principal: base de qualidade do módulo antes da expansão

### Sprint 6

- foco: consolidação
- saída principal: qualidade contínua incorporada ao processo do produto

## Próximos passos recomendados

### Curto prazo
- finalizar estabilização do Voluntariado
- resolver conflito da porta local da API
- ampliar cobertura de testes do Voluntariado
- reforçar logs e auditoria das ações críticas do módulo

### Próxima grande frente
- iniciar Kids / AppKids já com preocupação de permissões, auditoria e observabilidade desde o começo

### Em paralelo
- mapear matriz de permissões do sistema
- criar checklist de saúde operacional
- começar limpeza sistemática de higiene de frontend e portal
