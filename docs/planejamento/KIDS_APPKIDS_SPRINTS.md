# Kids / AppKids - Plano por sprints

Este documento transforma a direcao de produto do modulo Kids em uma sequencia de sprints executaveis.

Ele segue as diretrizes de engenharia e qualidade ja formalizadas no projeto:

- [PLANEJAMENTO_PRODUTO_E_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/PLANEJAMENTO_PRODUTO_E_QUALIDADE.md)
- [CHECKLIST_ENTREGA_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/CHECKLIST_ENTREGA_QUALIDADE.md)
- [QUALIDADE_SPRINT1_PAPEIS_E_PERMISSOES.md](/Users/aurelioromeu/repos/AppIgreja/QUALIDADE_SPRINT1_PAPEIS_E_PERMISSOES.md)
- [QUALIDADE_SPRINT1_FLUXOS_E_RISCOS.md](/Users/aurelioromeu/repos/AppIgreja/QUALIDADE_SPRINT1_FLUXOS_E_RISCOS.md)
- [KIDS_APPKIDS_PLANO_ESTRUTURADO.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_APPKIDS_PLANO_ESTRUTURADO.md)

## Premissas de execucao

Cada sprint de Kids deve respeitar estas regras:

- backend impondo permissao e escopo, nunca o frontend sozinho
- regra de negocio concentrada em servicos e contratos claros
- toda entrega sensivel com log minimo e, quando aplicavel, auditoria
- toda tela nova com estado de loading, erro e vazio
- toda sprint com pelo menos uma contrapartida explicita de qualidade
- toda feature sensivel com roteiro de regressao do caminho feliz e de bloqueio

## Estrutura recomendada

Sugestao pratica:

- sprints de 1 a 2 semanas
- capacidade reservada para qualidade em toda sprint
- evitar abrir duas frentes grandes de Kids ao mesmo tempo
- concluir a base de permissao e contrato antes de escalar o AppKids

## Sprint 1 - Fundacao funcional e de seguranca

### Objetivo

Fechar a base de modelagem, escopo e contratos do modulo para que as proximas entregas nao nascam com retrabalho estrutural.

### Resultado de negocio

O modulo passa a ter definicao clara de quem pode ver, quem pode operar e em qual contexto.

### Escopo principal

- formalizar papeis do modulo:
  - admin
  - lider kids
  - operador kids
  - responsavel
- revisar os endpoints atuais de Kids por papel e contexto
- definir os endpoints administrativos versus endpoints do responsavel
- definir o contrato oficial do fluxo de check-out futuro
- definir o contexto operacional minimo:
  - culto
  - sessao
  - sala
  - turma
- mapear informacoes criticas obrigatorias nas telas operacionais

### Backlog sugerido

- criar matriz de endpoints de Kids com papel, acao e escopo exigido
- definir quais rotas passam a ser `me/*` para o responsavel
- revisar `GET /api/kids/criancas` para nao continuar como lista irrestrita em cenarios de responsavel
- revisar historico de check-ins para nao expor mais do que o necessario
- desenhar contrato novo da retirada segura
- documentar estados de excecao operacional permitidos

### Entregaveis

- matriz funcional de acesso do modulo
- proposta de contrato dos endpoints novos e ajustados
- mapa de entidades e relacionamentos faltantes
- criterios de auditoria do modulo

### Contrapartidas de qualidade

- checklist de risco do modulo revisado
- checklist de regressao inicial de Kids
- definicao de logs minimos para:
  - check-in
  - check-out
  - notificacoes
  - falhas de autorizacao

### Criterios de aceite

- esta claro quem pode operar cada fluxo
- esta claro quem nao deve acessar cada fluxo
- esta claro quais contratos atuais precisam mudar antes de expandir o modulo
- a equipe consegue implementar a Sprint 2 sem ambiguidade estrutural

## Sprint 2 - Contexto do responsavel e base do AppKids

### Objetivo

Transformar o AppKids em app real do responsavel, e nao apenas extensao do fluxo administrativo.

### Resultado de negocio

O responsavel passa a enxergar somente seu proprio contexto no app.

### Escopo principal

- criar endpoint de minhas criancas
- criar endpoint de meus check-ins ativos
- criar endpoint de detalhe resumido da minha crianca
- adaptar autenticacao e sessao do AppKids para a experiencia do responsavel
- definir home do AppKids centrada em "minhas criancas"

### Backlog sugerido

- criar `GET /api/kids/me/criancas`
- criar `GET /api/kids/me/checkins`
- criar `GET /api/kids/me/criancas/{id}`
- revisar `Auth/me` e o contexto necessario para o app tomar decisoes de perfil
- ajustar o AppKids para trocar de uma lista administrativa ampla para uma visao pessoal do responsavel

### Entregaveis

- backend com endpoints do responsavel
- AppKids exibindo "minhas criancas"
- status atual de check-in visivel no app

### Contrapartidas de qualidade

- testes de permissao para garantir que responsavel nao acessa criancas de terceiros
- testes de caminho de sucesso e bloqueio dos endpoints `me`
- estados de loading, erro e vazio no AppKids

### Criterios de aceite

- um responsavel autenticado ve apenas suas criancas
- o app nao depende de listas administrativas abertas
- o modulo passa a ter separacao real entre experiencia interna e experiencia do responsavel

## Sprint 3 - Avisos reais

### Objetivo

Entregar a primeira frente de valor forte para os pais e consolidar o AppKids como canal principal de comunicacao.

### Resultado de negocio

Pais passam a receber comunicacao confiavel, persistente e contextualizada no AppKids.

### Escopo principal

- feed de avisos no app
- avisos gerais
- avisos por culto
- avisos por sala ou turma
- avisos por crianca
- avisos individuais por responsavel
- marcacao de lido
- push apontando para o feed

### Backlog sugerido

- criar endpoint para listar avisos do responsavel
- criar endpoint administrativo para emitir aviso geral
- criar endpoint administrativo para aviso segmentado
- ligar a tela de avisos do AppKids ao backend
- diferenciar push de aviso e push de evento operacional

### Entregaveis

- central de avisos funcional no AppKids
- criacao de avisos pelo administrativo
- push integrado ao fluxo de comunicacao

### Contrapartidas de qualidade

- log de criacao de aviso
- log de falha de envio
- criterio de auditoria para avisos administrativos
- roteiro de regressao para aviso geral e segmentado

### Criterios de aceite

- o responsavel recebe e consulta avisos no app
- o aviso continua visivel no feed mesmo depois do push
- ha segmentacao minima por contexto

## Sprint 4 - Retirada segura

### Objetivo

Fechar o principal risco de seguranca do modulo.

### Resultado de negocio

A retirada passa a ser um fluxo confiavel, auditavel e menos vulneravel a improvisos.

### Escopo principal

- redesenhar contrato de checkout
- token de retirada por sessao
- QR dinamico ou PIN temporario
- lista de autorizados a retirar
- validacao do operador
- fluxo de excecao operacional
- trilha completa de retirada

### Backlog sugerido

- criar endpoint para resolver retirada por token valido
- reduzir dependencia de QR contendo dados improvisados
- permitir estrategia QR ou PIN temporario
- destacar no operador quem esta autorizado
- registrar quem validou e por qual metodo
- tratar retirada fora da regra com excecao auditada

### Entregaveis

- novo contrato de retirada
- fluxo operacional de retirada validado
- suporte no AppKids para apresentar o token ou PIN

### Contrapartidas de qualidade

- testes de autorizacao de retirada
- testes de token expirado ou invalido
- auditoria da retirada e da excecao
- logs minimos do fluxo de validacao

### Criterios de aceite

- nao ha necessidade de workaround fragil para o QR de checkout
- o operador consegue validar quem pode retirar
- a trilha de auditoria mostra quem retirou, quando e por qual metodo

## Sprint 5 - Painel operacional do culto

### Objetivo

Dar visibilidade operacional em tempo real para lideres e equipe.

### Resultado de negocio

A operacao do culto deixa de depender de consulta dispersa e memoria manual.

### Escopo principal

- painel ao vivo por culto ou sessao
- presentes agora
- ainda nao retirados
- distribuicao por sala ou turma
- alertas criticos em destaque
- lotacao e capacidade
- acoes operacionais rapidas

### Backlog sugerido

- evoluir a pagina web de historico para visao operacional
- filtrar por sessao, sala e turma
- destacar alergias, restricoes e observacoes criticas
- mostrar ocorrencias operacionais abertas
- permitir acao rapida para detalhes, contato e retirada

### Entregaveis

- painel operacional do culto no frontend web
- visao consolidada por sessao
- indicadores principais para lideranca

### Contrapartidas de qualidade

- tratamento de loading, erro e vazio
- logs de consultas operacionais sensiveis
- checklist de regressao do painel

### Criterios de aceite

- a lideranca consegue identificar rapidamente presentes, pendencias e gargalos
- dados criticos aparecem com destaque
- o painel apoia decisao operacional em tempo real

## Sprint 6 - Ocorrencias e historico

### Objetivo

Registrar de forma confiavel os acontecimentos relevantes da experiencia da crianca no Kids.

### Resultado de negocio

O modulo passa a ter memoria operacional e historico consultavel.

### Escopo principal

- cadastro de ocorrencias
- classificacao por tipo
- timeline por crianca
- registro de acao tomada
- registro de contato com responsavel
- consulta administrativa e operacional

### Backlog sugerido

- modelar entidade de ocorrencia
- criar fluxo de registro rapido
- criar lista ou timeline por crianca
- destacar ocorrencias abertas no painel operacional
- definir o que pode ou nao aparecer ao responsavel

### Entregaveis

- backend de ocorrencias
- tela de registro e consulta
- historico por crianca

### Contrapartidas de qualidade

- auditoria da criacao e edicao de ocorrencia sensivel
- testes de permissao por papel
- roteiro de regressao do historico

### Criterios de aceite

- a equipe consegue registrar e recuperar ocorrencias com clareza
- a lideranca consegue consultar historico util
- nao ha exposicao indevida de dado sensivel ao responsavel

## Sprint 7 - Sala, turma e capacidade

### Objetivo

Consolidar a organizacao operacional do modulo em torno de estrutura real de atendimento.

### Resultado de negocio

Kids passa a operar melhor distribuicao, seguranca e previsibilidade por sala.

### Escopo principal

- cadastro de sala
- cadastro de turma
- capacidade maxima
- vinculo por culto ou sessao
- indicadores de ocupacao

### Backlog sugerido

- modelar sala e turma como entidades explicitas
- suportar capacidade por contexto de culto
- exibir lotacao no painel
- preparar base para check-in orientado por sala

### Entregaveis

- estrutura de sala e turma funcionando
- capacidade visivel no painel
- base pronta para refinamentos operacionais futuros

### Contrapartidas de qualidade

- testes de consistencia de capacidade
- logs de alteracao administrativa sensivel
- revisao de permissoes de configuracao

### Criterios de aceite

- salas e turmas deixam de ser apenas campo solto
- a lideranca tem visao real de capacidade e ocupacao

## Sprint 8 - Consolidacao, indicadores e refinamentos

### Objetivo

Fechar a primeira onda do modulo com estabilidade, legibilidade operacional e base de escala futura.

### Resultado de negocio

Kids se torna um modulo pronto para expandir com menor risco.

### Escopo principal

- indicadores principais do modulo
- revisao de UX do AppKids e do painel web
- ajustes de performance
- consolidacao de trilhas de auditoria
- revisao de backlog residual

### Backlog sugerido

- indicadores de check-in, retirada, capacidade e ocorrencias
- refinamento de labels e fluxos
- revisao de logs e alertas
- consolidacao dos checklists de regressao

### Entregaveis

- pacote de estabilizacao e refinamento
- criterios oficiais de aceite do modulo

### Contrapartidas de qualidade

- revisao final dos checklists de qualidade
- cobertura minima dos fluxos criticos
- mapa consolidado de risco residual

### Criterios de aceite

- o modulo esta funcionalmente forte, observavel e mais seguro para evolucoes futuras

## Ordem objetiva recomendada

Se o time quiser uma leitura curta da sequencia:

1. Sprint 1: fechar fundacao de permissao, escopo e contrato
2. Sprint 2: entregar contexto do responsavel
3. Sprint 3: colocar avisos reais no AppKids
4. Sprint 4: implementar retirada segura
5. Sprint 5: subir painel operacional do culto
6. Sprint 6: registrar ocorrencias e historico
7. Sprint 7: consolidar sala, turma e capacidade
8. Sprint 8: estabilizar, medir e refinar

## Recomendacao de governanca

Para cada sprint de Kids, a definicao de pronto deve incluir:

- objetivo de negocio claro
- validacao de permissao e escopo
- checklist de regressao curto
- log minimo dos eventos sensiveis
- trilha de auditoria quando houver efeito operacional sensivel
- estado de loading, erro e vazio nas telas novas

Sem isso, a sprint pode ate entregar funcionalidade, mas nao entrega confianca operacional suficiente para um modulo sensivel como Kids.
