# Kids / AppKids - Revisao pos Sprint 8 e abertura da Sprint 9

Este documento marca a transicao entre a primeira grande fase de implementacao de Kids / AppKids e a proxima fase de refinamento.

Referencias:

- [KIDS_APPKIDS_PLANO_ESTRUTURADO.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_APPKIDS_PLANO_ESTRUTURADO.md)
- [KIDS_APPKIDS_SPRINTS.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_APPKIDS_SPRINTS.md)
- [KIDS_SPRINT8_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT8_EXECUCAO_TECNICA.md)

## 1. Resumo executivo

As 8 sprints planejadas para a primeira onda do modulo Kids foram percorridas e entregaram a espinha dorsal do produto:

- contexto do responsavel
- avisos reais
- retirada segura
- painel operacional do culto
- ocorrencias e historico operacional
- estrutura formal de sala e turma
- indicadores consolidados iniciais

Com isso, a fase original de construcao base pode ser considerada encerrada.

Ao mesmo tempo, o estado atual ainda nao representa a fase final de maturidade do modulo. A base esta pronta, mas ainda ha pontos de refinamento, endurecimento operacional e alinhamento de UX antes de um rollout mais forte.

## 2. O que esta efetivamente entregue

### Sprint 1 a 2 - fundacao e contexto do responsavel

Entregue:

- contratos `me/*` no backend
- leitura de `minhas criancas`
- leitura de `meus check-ins`
- home do AppKids orientada ao responsavel
- detalhe da crianca no app

### Sprint 3 - avisos reais

Entregue:

- feed persistente de avisos no backend
- leitura e marcacao como lido
- envio administrativo de avisos
- feed funcional no AppKids

### Sprint 4 - retirada segura

Entregue:

- token de retirada
- PIN de retirada
- validacao de retirada
- confirmacao operacional em duas etapas
- excecao auditada
- exibicao do token e PIN no AppKids

### Sprint 5 - painel operacional

Entregue:

- endpoint de painel operacional
- cards de resumo
- lista de presentes
- distribuicao por sala
- alertas criticos
- historico operacional preservado no web

### Sprint 6 - ocorrencias e historico

Entregue:

- entidade propria de ocorrencia de Kids
- criacao, atualizacao e listagem
- ocorrencias abertas para operacao
- historico por crianca na tela web
- acoes de contato e encerramento

### Sprint 7 - sala, turma e capacidade

Entregue:

- entidades formais `KidsSala` e `KidsTurma`
- endpoints administrativos de sala e turma
- migration estrutural
- tela web com cadastro basico de sala e turma
- estrutura visivel no painel

### Sprint 8 - consolidacao e indicadores

Entregue:

- endpoint de indicadores consolidados
- cards de indicadores no painel web
- consolidacao de validacao tecnica da trilha

## 3. O que ficou parcial ou ainda precisa refinamento

Estes itens nao invalidam o encerramento das 8 sprints, mas indicam claramente o backlog da proxima fase.

### 3.1 AppKids ainda nao absorveu toda a evolucao do modulo

Situacao atual:

- AppKids cobre bem `minhas criancas`, avisos e retirada segura
- AppKids ainda nao consome `salas`, `turmas`, `indicadores` ou historico de ocorrencias visiveis ao responsavel
- o app ainda carrega sinais de transicao da fase inicial do projeto

Impacto:

- a experiencia do responsavel ainda nao reflete tudo que o backend ja suporta

### 3.2 Painel web concentra muita responsabilidade em uma tela so

Situacao atual:

- [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx) ficou forte e funcional
- mas hoje ela acumula painel, historico, ocorrencias, estrutura e indicadores numa unica pagina

Impacto:

- a operacao funciona
- a manutencao e a legibilidade da tela ainda podem melhorar bastante

### 3.3 Estrutura de sala e turma ainda nao esta totalmente propagada

Situacao atual:

- backend formalizou `KidsSala` e `KidsTurma`
- `CriancaDetalhe` passou a ter `TurmaId`
- mas o cadastro administrativo de crianca ainda nao ganhou um fluxo web dedicado e refinado consumindo essa estrutura de ponta a ponta

Impacto:

- a modelagem existe
- a experiencia de cadastro e manutencao ainda nao esta plenamente coerente

### 3.4 Permissoes ainda podem ser endurecidas

Situacao atual:

- a base de escopo melhorou bastante
- mas o modulo ainda depende fortemente de autenticacao e convencao de uso

Pontos a endurecer:

- diferenciar mais claramente perfil administrativo, operador e responsavel
- revisar endpoints administrativos que hoje podem estar amplos demais

### 3.5 Observabilidade ainda esta inicial

Situacao atual:

- ha logs e auditoria em fluxos importantes
- mas ainda nao existe visao consolidada de saude operacional do modulo

Pontos pendentes:

- indicadores de falha por fluxo
- visao de erros de push
- visao de excecoes de retirada
- rastreio mais claro de alteracoes estruturais

### 3.6 Documentacao de AppKids ficou defasada

Arquivos como:

- [AppKids/README.md](/Users/aurelioromeu/repos/AppIgreja/AppKids/README.md)
- [AppKids/APP_KIDS_ESPECIFICACAO.md](/Users/aurelioromeu/repos/AppIgreja/AppKids/APP_KIDS_ESPECIFICACAO.md)

ainda descrevem partes que ja foram superadas, como avisos placeholder ou ausencia de endpoints.

Impacto:

- onboarding tecnico fica menos confiavel
- risco de leitura errada do estado atual

## 4. Backlog oficial de pos-sprint

Este backlog e o que sobra da primeira fase, mas nao deve mais ser tratado como “Sprint 8 atrasada”. A partir daqui, ele passa a ser backlog da fase seguinte.

### Eixo A - endurecimento

- revisar permissoes e escopo de endpoints de Kids
- explicitar papeis operacionais do modulo
- revisar cobertura de auditoria nas mudancas estruturais
- revisar mensagens de erro e respostas padronizadas

### Eixo B - refinamento de UX

- dividir o painel web em componentes ou subfluxos mais claros
- deixar cadastro de sala e turma mais robusto
- ligar sala e turma ao cadastro administrativo da crianca
- revisar hierarquia visual do painel

### Eixo C - AppKids fase 2

- expor historico visivel ao responsavel quando apropriado
- revisar home do responsavel com status mais claro
- reduzir vestigios do fluxo antigo e telas legadas
- atualizar documentacao do app para o estado real

### Eixo D - operacao e observabilidade

- consolidar indicadores mais diretamente operacionais
- monitorar fluxos de aviso e retirada
- consolidar checklist de rollout do modulo

## 5. Proposta de Sprint 9

Nome sugerido:

- `Sprint 9 - Refinamento operacional e endurecimento do modulo`

Objetivo:

- transformar a base entregue nas 8 sprints em uma operacao mais coesa, mais segura e mais pronta para escalar

## 6. Escopo recomendado da Sprint 9

### Entra na Sprint 9

- revisao de permissoes e escopo do modulo Kids
- refinamento do painel web para reduzir concentracao de responsabilidade
- propagacao real de `sala` e `turma` no fluxo administrativo da crianca
- atualizacao da documentacao do AppKids
- limpeza de pontos legados claramente superados

### Nao entra na Sprint 9

- nova frente funcional grande para pais
- automacao externa ou BI avancado
- omnichannel
- nova app experience completa do zero

## 7. Prioridade interna da Sprint 9

### 7.1 Permissoes e endurecimento

Entrega esperada:

- matriz simples de perfis de Kids
- revisao dos endpoints administrativos
- validacoes mais explicitas no backend

Resultado:

- menos risco de acesso indevido
- menos dependencia de convencao operacional

### 7.2 Fluxo administrativo de crianca com sala e turma

Entrega esperada:

- consumo real de `GET /api/kids/salas`
- consumo real de `GET /api/kids/turmas`
- formularios administrativos usando selects em vez de texto livre
- coerencia entre cadastro da crianca e estrutura formal do modulo

Resultado:

- fim da ambiguidade estrutural
- base melhor para capacidade e distribuicao por turma

### 7.3 Refino do painel web

Entrega esperada:

- separar melhor os blocos da tela atual
- simplificar a leitura do operador
- reduzir densidade excessiva da pagina

Resultado:

- experiencia mais clara no culto
- tela mais manutencivel para o time

### 7.4 Documentacao viva

Entrega esperada:

- atualizar [AppKids/README.md](/Users/aurelioromeu/repos/AppIgreja/AppKids/README.md)
- atualizar [AppKids/APP_KIDS_ESPECIFICACAO.md](/Users/aurelioromeu/repos/AppIgreja/AppKids/APP_KIDS_ESPECIFICACAO.md)
- registrar o que ja existe e o que fica para a fase seguinte

Resultado:

- onboarding tecnico mais seguro
- menos retrabalho por informacao antiga

## 8. Backlog sugerido da Sprint 9

### S9-01

Revisar e formalizar matriz de acesso do modulo Kids.

### S9-02

Auditar endpoints administrativos de Kids e endurecer guardas de escopo.

### S9-03

Criar ou evoluir fluxo administrativo de crianca para usar `salas` e `turmas` estruturadas.

### S9-04

Refatorar [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx) em blocos/componentes menores.

### S9-05

Revisar estados de vazio, erro e confirmacao do painel web.

### S9-06

Atualizar documentacao de AppKids e alinhar o estado atual do modulo.

### S9-07

Eliminar comentarios, labels e referencias que ainda indiquem fluxos antigos superados.

## 9. Definition of Done da Sprint 9

A Sprint 9 pode ser considerada concluida quando:

- permissao e escopo do modulo estiverem mais explicitos
- fluxo administrativo de crianca estiver coerente com `sala` e `turma`
- painel web estiver mais legivel e modular
- documentacao principal do AppKids refletir o produto real
- regressao essencial de Kids continuar validada

## 10. Recomendacao final

As 8 sprints originais acabaram.

O que vem agora nao e “mais do mesmo”; e a fase de maturacao.

Por isso, a melhor leitura e:

- `fase 1 de Kids`: concluida
- `fase 2 de Kids`: comeca na Sprint 9

E a Sprint 9 deve priorizar menos funcionalidade nova e mais:

- consistencia
- governanca
- clareza operacional
- qualidade percebida
