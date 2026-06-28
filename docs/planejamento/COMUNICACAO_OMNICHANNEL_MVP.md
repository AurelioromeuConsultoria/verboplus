# Comunicacao Omnichannel - Sequencia, MVP e Modelagem

## 1. Objetivo

Este documento organiza a frente de comunicacao omnichannel do AppIgreja como produto real, conectando:

- a base que ja existe no sistema
- a sequencia recomendada de evolucao
- o recorte do MVP
- a modelagem funcional e tecnica inicial
- as telas principais do modulo

O objetivo nao e criar mais um bloco isolado de "disparo de mensagens". A proposta e estruturar um modulo de comunicacao que sirva a igreja inteira, com segmentacao, campanhas, automacoes, rastreabilidade e reaproveitamento dos canais ja existentes.

## 2. O que ja existe hoje

O projeto ja possui partes importantes da fundacao:

- configuracoes de mensagens automaticas para visitantes
- mensagens agendadas com fila, status e processamento em background
- envio via Evolution API para WhatsApp
- campanha de aniversario com historico de tentativas e resultado
- notificacoes internas para usuarios administrativos
- notificacoes e push no modulo Kids / AppKids

Isso significa que a frente de comunicacao omnichannel nao deve nascer do zero. Ela deve consolidar e generalizar capacidades que hoje estao espalhadas por contextos diferentes.

## 3. Leitura de produto

Hoje a comunicacao esta fragmentada em quatro formatos:

- comunicacao com visitante por WhatsApp
- comunicacao administrativa interna via notificacoes
- comunicacao transacional do Kids
- campanhas especificas como aniversario

O modulo de comunicacao deve unificar essas frentes sob uma mesma linguagem de produto:

- audiencia
- canal
- template
- campanha
- automacao
- entrega
- resultado
- preferencia e consentimento

## 4. Sequencia recomendada de Comunicacao Omnichannel

A ordem abaixo prioriza valor real, baixo retrabalho e reaproveitamento da base atual.

### Fase 1 - Fundacao unica de comunicacao

Objetivo:
- parar de tratar cada fluxo como uma implementacao isolada

Entregas:
- consolidar linguagem oficial do modulo
- padronizar conceito de canal: WhatsApp, email, push, notificacao interna
- padronizar status de entrega
- criar trilha de auditoria minima por envio e campanha
- definir relacao do modulo com Pessoa, Visitante, Usuario e Kids

Resultado esperado:
- mesma estrutura conceitual para campanhas, automacoes e avisos

### Fase 2 - MVP operacional

Objetivo:
- colocar no ar o primeiro modulo utilizavel por operacao

Entregas:
- campanhas simples
- segmentacao basica
- templates
- fila de envios
- historico de entregas
- dashboard operacional minimo

Resultado esperado:
- equipe consegue criar, disparar, acompanhar e auditar comunicacoes sem depender de fluxos tecnicos dispersos

### Fase 3 - Automacoes por gatilho

Objetivo:
- transformar comunicacao de disparo manual em motor recorrente de relacionamento

Entregas:
- automacao por evento de negocio
- onboarding de visitante
- aniversario
- lembretes de escala
- avisos do Kids
- reengajamento simples

Resultado esperado:
- regras reutilizaveis por gatilho, sem duplicar logica em cada modulo

### Fase 4 - Preferencias, consentimento e maturidade analitica

Objetivo:
- escalar comunicacao com governanca

Entregas:
- preferencias por canal
- opt-in e opt-out
- bloqueios por canal
- metricas por campanha, canal e audiencia
- comparativos de desempenho

Resultado esperado:
- operacao madura, segura e escalavel

## 5. Escopo do MVP de comunicacao

O MVP precisa ser pequeno o suficiente para entrar rapido, mas forte o suficiente para nao virar uma tela de disparo improvisada.

### O que entra no MVP

- modulo central de campanhas
- templates reutilizaveis
- segmentacao basica por publico
- disparo manual com agendamento opcional
- fila de processamento
- historico por envio
- painel de status
- suporte inicial a WhatsApp
- suporte inicial a email
- suporte inicial a push para contextos em que o app ja tenha token ativo
- notificacao interna como canal complementar administrativo

### Segmentos iniciais do MVP

- visitantes
- membros
- voluntarios
- responsaveis do Kids

### Casos de uso iniciais do MVP

- mensagem para novos visitantes
- email de boas-vindas e follow-up leve
- campanha de aniversario
- lembrete operacional para voluntarios
- aviso para responsaveis do Kids
- comunicado geral segmentado por publico

### O que fica fora do MVP

- construtor avancado de jornadas visuais
- segmentacao dinamica complexa com multiplas regras compostas
- testes A/B
- automacoes multietapas sofisticadas
- inbox conversacional de atendimento
- unificacao completa com CRM pastoral
- email marketing completo com editor avancado, metricas aprofundadas e automacoes sofisticadas

## 6. Principios do modulo

- comunicacao deve nascer centrada em audiencia e objetivo, nao em canal isolado
- todo envio precisa deixar historico consultavel
- campanhas e automacoes devem reutilizar templates e politicas de canal
- regra de permissao e escopo deve ser validada no backend
- falha de integracao precisa ser visivel operacionalmente
- cada canal deve poder crescer sem quebrar o modelo central

## 7. Modelo funcional do MVP

O MVP pode ser entendido por sete blocos:

### 7.1 Audiencia

Representa quem vai receber.

Recortes iniciais:
- tipo de publico
- segmento salvo
- destinatario individual
- lista derivada de contexto operacional

Exemplos:
- visitantes dos ultimos 7 dias
- voluntarios de uma equipe
- responsaveis de uma turma do Kids
- aniversariantes do dia

### 7.2 Canal

Representa por onde a comunicacao sera entregue.

Canais do MVP:
- WhatsApp
- Email
- Push
- Notificacao interna

Campos de regra do canal:
- ativo
- prioridade
- requer opt-in
- requer identificador valido
- janela de envio
- politica de remetente, quando aplicavel

### 7.3 Template

Representa o conteudo-base reutilizavel.

Campos sugeridos:
- nome
- objetivo
- canal
- assunto ou titulo, quando aplicavel
- corpo
- corpoHtml, quando aplicavel
- variaveis permitidas
- status
- versao

Variaveis iniciais sugeridas:
- {Nome}
- {PrimeiroNome}
- {Equipe}
- {Crianca}
- {DataCulto}
- {Link}

### 7.4 Campanha

Representa uma acao planejada de comunicacao.

Campos sugeridos:
- nome
- objetivo
- publico alvo
- canais selecionados
- template por canal
- modo de envio: imediato ou agendado
- status
- criado por
- data de criacao

Status iniciais:
- rascunho
- agendada
- processando
- concluida
- concluida com falhas
- cancelada

### 7.5 Automacao

Representa uma regra que gera envios a partir de gatilho.

Gatilhos iniciais:
- visitante criado
- aniversario chegou
- escala publicada
- aviso Kids criado

Campos sugeridos:
- nome
- gatilho
- segmento alvo
- template
- canal
- delay
- ativo

### 7.6 Entrega

Representa cada tentativa de envio para cada destinatario e canal.

Campos sugeridos:
- campanhaId ou automacaoId
- destinatarioPessoaId ou visitanteId
- canal
- destino resolvido
- remetente resolvido, quando aplicavel
- conteudo final renderizado
- status
- tentativas
- processadoEm
- entregueEm
- erro

Status iniciais:
- pendente
- reservado
- enviado
- entregue
- falhou
- cancelado
- ignorado por preferencia

### 7.7 Preferencia e consentimento

Mesmo que a governanca completa fique para depois, o modelo ja deve prever:

- pessoa pode receber WhatsApp
- pessoa pode receber push
- pessoa pode receber notificacao interna
- origem do consentimento
- data da ultima alteracao

## 8. Proposta de modelagem tecnica

Hoje ja existem entidades proximas, como:

- `ConfiguracaoMensagem`
- `MensagemAgendada`
- `EnvioCampanhaAniversario`
- `NotificacaoUsuario`
- `KidsNotificacao`

A recomendacao para o MVP e evoluir para um nucleo mais generico, reaproveitando a infraestrutura ja pronta de fila e execucao.

### Entidades novas ou consolidadas

- `ComunicacaoTemplate`
- `ComunicacaoSegmento`
- `ComunicacaoCampanha`
- `ComunicacaoCampanhaCanal`
- `ComunicacaoAutomacao`
- `ComunicacaoEntrega`
- `ComunicacaoPreferencia`

### Entidades atuais que podem ser absorvidas ou migradas gradualmente

- `ConfiguracaoMensagem` pode evoluir para `ComunicacaoTemplate` em casos de automacao para visitante
- `MensagemAgendada` pode evoluir para `ComunicacaoEntrega`
- `EnvioCampanhaAniversario` pode virar uma especializacao operacional em cima de campanha e entrega
- `NotificacaoUsuario` e `KidsNotificacao` podem continuar existindo na primeira etapa, mas integradas ao mesmo conceito de campanha e entrega

## 9. Fluxos principais do MVP

### 9.1 Criar campanha manual

1. usuario escolhe objetivo
2. usuario escolhe audiencia
3. usuario escolhe canal
4. usuario escolhe template
5. sistema mostra estimativa de alcance e bloqueios
6. usuario agenda ou dispara
7. sistema gera entregas
8. fila processa por canal
9. painel mostra status e falhas

### 9.2 Executar automacao

1. evento de negocio acontece
2. automacao elegivel e identificada
3. sistema resolve audiencia
4. sistema renderiza template
5. sistema cria entregas
6. fila processa
7. resultado fica disponivel no historico

### 9.3 Consultar historico

1. operador abre campanha
2. visualiza totais por status
3. filtra falhas
4. reprocessa subconjunto quando aplicavel

## 10. Telas do modulo de comunicacao

O modulo pode nascer com seis telas principais.

### 10.1 Dashboard de comunicacao

Objetivo:
- dar leitura operacional rapida

Blocos:
- campanhas enviadas hoje
- entregas pendentes
- falhas recentes
- canais ativos
- automacoes executadas
- atalhos para nova campanha, templates e segmentos

### 10.2 Lista de campanhas

Objetivo:
- gerir tudo que foi planejado ou disparado

Colunas sugeridas:
- nome
- objetivo
- audiencia
- canais
- agendamento
- status
- volume previsto
- volume enviado
- criador

Acoes:
- criar
- duplicar
- cancelar
- abrir detalhes

### 10.3 Wizard de nova campanha

Passos:
- passo 1: objetivo e nome
- passo 2: audiencia
- passo 3: canal
- passo 4: template e conteudo
- passo 5: revisao e disparo

Esse wizard e a principal tela do MVP. Ele deve ser orientado a tarefa, nao a formulario tecnico.

### 10.4 Biblioteca de templates

Objetivo:
- evitar mensagem improvisada e regra espalhada

Recursos:
- listar templates por canal
- criar e editar
- preview com variaveis
- ativar e arquivar

### 10.5 Segmentos

Objetivo:
- salvar audiencias recorrentes

Segmentos iniciais sugeridos:
- visitantes recentes
- aniversariantes
- voluntarios por equipe
- responsaveis do Kids por sala ou turma
- membros com app ativo

### 10.6 Detalhe da campanha

Objetivo:
- diagnostico operacional

Blocos:
- resumo da campanha
- timeline de processamento
- totais por canal
- lista de entregas
- falhas com motivo
- opcao de reprocessar falhas elegiveis

## 11. Backoffice x experiencia final

O modulo de comunicacao deve diferenciar bem:

- experiencia do operador: criar, agendar, monitorar e corrigir
- experiencia do destinatario: receber no canal certo, com mensagem adequada e historico coerente

No MVP, o foco principal e o backoffice. A experiencia final ja e parcialmente atendida pelos canais existentes.

## 12. Reaproveitamento tecnico recomendado

Para acelerar entrega, vale reaproveitar:

- servico de processamento em background de mensagens
- integracao com Evolution API
- estrutura de notificacoes internas
- push do AppKids
- modelos atuais de campanhas de aniversario

Para suportar email no MVP, o modulo tambem deve prever desde o inicio:

- provedor de envio desacoplado por interface
- template com assunto e corpo HTML ou texto
- identidade de remetente configuravel
- historico de falha por destinatario
- status de entrega compativel com email

O ganho esperado e reduzir retrabalho de infraestrutura e concentrar esforco em modelagem de produto, UX operacional e consolidacao de dominio.

## 13. Riscos que devem ser tratados desde o MVP

- disparo sem permissao ou escopo correto
- audiencia montada com criterio errado
- duplicidade de envio
- falha silenciosa em integracao externa
- template sem validacao minima
- ausencia de historico confiavel por entrega
- falta de governanca de preferencia por canal
- email sem identidade de envio, configuracao tecnica ou tratamento de bounce minimo

## 14. Sequencia de implementacao sugerida

### Etapa 1

- documentar dominio oficial do modulo
- criar mapa de reaproveitamento das estruturas atuais
- fechar nomenclatura de entidades e estados

### Etapa 2

- implementar templates e campanhas
- generalizar fila de entregas
- expor dashboard e lista de campanhas

### Etapa 3

- conectar WhatsApp como canal principal do MVP
- conectar email como canal externo nativo do MVP
- conectar push em fluxos elegiveis
- integrar notificacao interna no mesmo painel

### Etapa 4

- introduzir automacoes iniciais
- visitante novo
- aniversario
- lembretes operacionais
- avisos Kids

### Etapa 5

- introduzir preferencias e consentimento
- amadurecer metricas e reprocessamento

## 15. Recomendacao pratica para o AppIgreja

Se a intencao for entrar por valor real com baixo risco, o melhor recorte e:

- WhatsApp e email como canais externos principais
- push como canal complementar para Kids e app
- notificacao interna como canal administrativo
- campanhas manuais + automacoes simples
- foco em visitantes, voluntarios e responsaveis do Kids

Esse recorte aproveita o que o sistema ja tem, gera uso transversal entre modulos e prepara o terreno para a Jornada 360 da Pessoa.

## 16. Resumo executivo

O modulo de comunicacao nao deve nascer como uma tela de envio em massa. Ele deve nascer como um nucleo compartilhado de relacionamento e operacao.

Para o MVP, a melhor estrategia e:

- consolidar o que ja existe
- unificar o conceito de campanha, template, canal e entrega
- entrar primeiro com WhatsApp, email, push e notificacao interna
- servir publicos que ja existem no sistema
- deixar historico, status e falha visiveis desde o inicio

Assim, a comunicacao omnichannel deixa de ser uma colecao de fluxos soltos e passa a ser uma capacidade estrutural do produto.
