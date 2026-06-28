# Comunicacao Omnichannel - Plano por sprints

Este documento transforma a direcao de produto da frente de comunicacao omnichannel em uma sequencia de sprints executaveis.

Ele segue as diretrizes de engenharia e qualidade ja formalizadas no projeto:

- [PLANEJAMENTO_PRODUTO_E_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/PLANEJAMENTO_PRODUTO_E_QUALIDADE.md)
- [CHECKLIST_ENTREGA_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/CHECKLIST_ENTREGA_QUALIDADE.md)
- [COMUNICACAO_OMNICHANNEL_MVP.md](/Users/aurelioromeu/repos/AppIgreja/COMUNICACAO_OMNICHANNEL_MVP.md)

## Premissas de execucao

Cada sprint de comunicacao deve respeitar estas regras:

- backend impondo permissao, escopo e politicas de canal
- campanhas, automacoes e envios usando uma linguagem de dominio unica
- cada envio com historico consultavel por destinatario e canal
- falha de integracao externa nunca silenciosa
- toda tela nova com estado de loading, erro e vazio
- toda sprint com contrapartida explicita de qualidade
- toda integracao de canal com diagnostico operacional minimo

## Estrutura recomendada

Sugestao pratica:

- sprints de 1 a 2 semanas
- evitar abrir muitos canais novos ao mesmo tempo sem consolidar o dominio central
- entregar primeiro a base comum antes de multiplicar automacoes
- tratar WhatsApp e email como canais externos principais do MVP
- usar push e notificacao interna como canais complementares e contextuais

## Sprint 1 - Fundacao do modulo

### Objetivo

Fechar a base conceitual, de modelagem e de escopo para que o modulo nao nasca como somatorio de fluxos isolados.

### Resultado de negocio

A equipe passa a ter definicao clara do que e template, campanha, automacao, canal, entrega, preferencia e segmento.

### Escopo principal

- formalizar o dominio oficial do modulo
- mapear as estruturas atuais que serao reaproveitadas
- definir estados oficiais de campanha e entrega
- definir relacao entre Pessoa, Visitante, Usuario e destinatario de comunicacao
- definir permissoes operacionais do modulo
- fechar o primeiro mapa de canais: WhatsApp, email, push e notificacao interna

### Backlog sugerido

- mapear `ConfiguracaoMensagem`, `MensagemAgendada`, `EnvioCampanhaAniversario`, `NotificacaoUsuario` e `KidsNotificacao`
- definir estrategia de convivio entre legado e novo modelo
- criar matriz de permissoes do modulo
- definir contrato inicial de campanha manual
- definir contrato inicial de entrega por canal
- definir politica minima de preferencia e consentimento por canal

### Entregaveis

- mapa funcional do modulo
- proposta de entidades centrais
- matriz inicial de permissao e escopo
- glossario oficial do modulo

### Contrapartidas de qualidade

- checklist de risco da frente de comunicacao
- convencao minima de logs para disparo, falha e reprocessamento
- criterio de auditoria para campanhas e automacoes

### Criterios de aceite

- a equipe sabe o que sera reaproveitado e o que sera evoluido
- os estados de campanha e entrega estao fechados
- a Sprint 2 pode comecar sem ambiguidade estrutural

## Sprint 2 - Templates, campanhas e fila unificada

### Objetivo

Colocar no ar o nucleo operacional do modulo.

### Resultado de negocio

A equipe consegue criar e disparar campanhas simples em uma estrutura unica, com historico por envio.

### Escopo principal

- cadastro de templates
- criacao de campanha manual
- selecao de audiencia basica
- geracao de entregas por destinatario
- fila unificada de processamento
- dashboard e lista de campanhas

### Backlog sugerido

- criar entidade ou contrato de template
- criar entidade ou contrato de campanha
- criar entidade ou contrato de entrega
- criar pagina de listagem de campanhas
- criar wizard inicial de nova campanha
- adaptar o processador atual para fila generica

### Entregaveis

- biblioteca de templates
- criacao de campanha manual
- painel de campanhas
- historico de entregas por campanha

### Contrapartidas de qualidade

- logs de criacao, cancelamento e processamento de campanha
- tratamento de duplicidade de envio
- estados de erro e vazio nas telas novas
- roteiro de regressao da campanha manual

### Criterios de aceite

- uma campanha pode ser criada, agendada e processada
- existe visibilidade de totais por status
- falhas ficam registradas e consultaveis

## Sprint 3 - Canais externos do MVP: WhatsApp e email

### Objetivo

Entregar os dois principais canais externos do MVP em cima do nucleo central.

### Resultado de negocio

A equipe consegue acionar publico externo por WhatsApp e email sem sair de um mesmo modulo.

### Escopo principal

- conectar WhatsApp ao novo fluxo de entrega
- conectar email ao novo fluxo de entrega
- suportar template por canal
- suportar assunto e corpo HTML ou texto no email
- suportar politica minima de remetente
- permitir acompanhamento por canal

### Backlog sugerido

- adaptar integracao atual com Evolution API ao modelo central
- criar interface de provedor de email
- definir configuracao de remetente do email
- criar preview por canal no wizard
- registrar motivo de falha por canal
- registrar destino resolvido por envio

### Entregaveis

- campanhas por WhatsApp
- campanhas por email
- templates por canal
- detalhe de campanha com leitura por canal

### Contrapartidas de qualidade

- log estruturado de falha de integracao externa
- validacao minima de template de email
- diagnostico operacional de configuracao ausente de remetente ou provedor
- cobertura de cenarios de falha de envio

### Criterios de aceite

- operador consegue escolher WhatsApp ou email na campanha
- o sistema cria e processa entregas corretamente por canal
- falhas de integracao sao visiveis sem depender de suporte manual

## Sprint 4 - Push, notificacao interna e contexto dos modulos

### Objetivo

Conectar canais contextuais do ecossistema AppIgreja ao mesmo modulo.

### Resultado de negocio

Comunicacoes administrativas e avisos em app passam a compartilhar a mesma trilha operacional.

### Escopo principal

- conectar push ao modelo central
- conectar notificacao interna ao modelo central
- integrar avisos do Kids ao conceito de campanha ou automacao
- integrar comunicacao administrativa com o novo painel

### Backlog sugerido

- mapear o que segue especifico do Kids e o que entra no modulo central
- criar adaptador para push
- criar adaptador para notificacao interna
- permitir campanha multicanal quando fizer sentido
- consolidar historico por destinatario

### Entregaveis

- push integrado ao painel de comunicacao
- notificacao interna integrada ao painel de comunicacao
- leitura centralizada de envios contextuais

### Contrapartidas de qualidade

- logs de criacao e falha de push
- criterio de auditoria para avisos administrativos
- regressao do fluxo de avisos Kids impactados

### Criterios de aceite

- o painel central enxerga campanhas e entregas desses canais
- o modulo nao quebra os fluxos que ja funcionam no Kids ou no admin

## Sprint 5 - Segmentacao e casos de uso prioritarios

### Objetivo

Entrar em operacao real com publicos e segmentacoes que ja geram valor imediato.

### Resultado de negocio

O modulo passa a ser usado para relacionamento e operacao em publicos centrais do produto.

### Escopo principal

- segmentos salvos
- filtros basicos por publico
- campanhas para visitantes
- campanhas para voluntarios
- campanhas para responsaveis do Kids
- campanhas para membros com contato valido

### Backlog sugerido

- criar segmentos salvos basicos
- criar estimativa de audiencia antes do disparo
- destacar bloqueios por falta de canal ou preferencia
- conectar segmentos aos modulos existentes

### Entregaveis

- tela de segmentos
- audiencia estimada antes do envio
- campanhas operacionais para publicos prioritarios

### Contrapartidas de qualidade

- validacao de audiencia antes da criacao das entregas
- logs de exclusao por bloqueio de canal ou preferencia
- checklist de regressao por publico prioritario

### Criterios de aceite

- o operador sabe quem sera impactado antes do disparo
- o sistema evita criar entrega invalida sem diagnostico

## Sprint 6 - Automacoes iniciais

### Objetivo

Transformar o modulo em motor recorrente de comunicacao.

### Resultado de negocio

Casos de uso previsiveis deixam de depender de disparo manual.

### Escopo principal

- automacao para novo visitante
- automacao para aniversario
- automacao para lembrete operacional
- automacao para aviso contextual do Kids

### Backlog sugerido

- definir gatilhos oficiais da primeira onda
- criar tabela ou contrato de automacao
- permitir delay e canal por automacao
- reaproveitar templates existentes
- registrar execucao e falha de automacao

### Entregaveis

- automacoes basicas ativas
- historico de execucao
- painel simples de status das automacoes

### Contrapartidas de qualidade

- logs de disparo por gatilho
- prevenicao de duplicidade por evento
- observabilidade minima do job ou worker de automacoes

### Criterios de aceite

- os principais gatilhos operam sem duplicidade indevida
- o time consegue diagnosticar falha de automacao rapidamente

## Sprint 7 - Preferencias, consentimento e reprocessamento

### Objetivo

Dar governanca minima para escalar o modulo com seguranca.

### Resultado de negocio

Comunicacao passa a respeitar preferencia por canal e ganhar capacidade de correcao operacional.

### Escopo principal

- preferencias por canal
- bloqueio por opt-out
- historico minimo de consentimento
- reprocessamento de falhas elegiveis
- filtros de campanha por sucesso e falha

### Backlog sugerido

- criar modelo de preferencia por canal
- refletir bloqueios no momento da geracao da entrega
- criar acao de reprocessar falhas
- destacar erros recorrentes por canal

### Entregaveis

- governanca minima de preferencia
- reprocessamento controlado
- painel com leitura melhor de falhas

### Contrapartidas de qualidade

- trilha de auditoria de alteracao de preferencia
- cobertura de cenarios de opt-out e bloqueio
- criterio de seguranca para reprocessamento

### Criterios de aceite

- o sistema respeita preferencia por canal
- falhas elegiveis podem ser reprocessadas com rastreabilidade

## Ordem macro recomendada

1. Sprint 1: fundacao do modulo
2. Sprint 2: templates, campanhas e fila unificada
3. Sprint 3: WhatsApp e email
4. Sprint 4: push e notificacao interna
5. Sprint 5: segmentacao e publicos prioritarios
6. Sprint 6: automacoes iniciais
7. Sprint 7: preferencias, consentimento e reprocessamento

## Recomendacao pratica

Se a execucao precisar ser ainda mais enxuta, o melhor recorte para uma primeira onda e:

- Sprint 1 completa
- Sprint 2 completa
- Sprint 3 com WhatsApp e email
- um recorte da Sprint 5 para visitantes e responsaveis do Kids

Esse conjunto ja cria um MVP de comunicacao real, com base estrutural melhor do que os fluxos isolados atuais e sem esperar a maturidade completa do modulo.
