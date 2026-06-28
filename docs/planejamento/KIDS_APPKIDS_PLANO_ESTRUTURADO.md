# Kids / AppKids - Plano estruturado de produto e execucao

Este documento reorganiza a frente de Kids com base em tres fontes:

- o que ja foi alinhado como prioridade de negocio
- o que ja existe de implementacao no backend, frontend web e AppKids
- os riscos operacionais e de seguranca que precisam ser tratados antes de escalar o modulo

O objetivo aqui nao e apenas listar ideias. A proposta e estruturar o modulo como produto operacional real, com ordem de execucao, dependencias e definicao clara do que deve nascer primeiro.

## 1. Diagnostico atual

Hoje a base de Kids ja e relevante e nao deve ser tratada como um modulo embrionario sem fundacao.

Ja existe:

- autenticacao e contexto de usuario
- cadastro de criancas
- vinculo de responsaveis
- check-in e check-out
- codigo de sessao no check-in
- push para check-in e check-out
- AppKids iniciado em Flutter
- tela administrativa web de historico operacional de check-ins

Arquivos de referencia:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)
- [KidsService.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Services/KidsService.cs)
- [KidsDto.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs)
- [AppKids/README.md](/Users/aurelioromeu/repos/AppIgreja/AppKids/README.md)
- [APP_KIDS_ESPECIFICACAO.md](/Users/aurelioromeu/repos/AppIgreja/AppKids/APP_KIDS_ESPECIFICACAO.md)
- [kids_repository.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/features/kids/kids_repository.dart)
- [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx)

Ao mesmo tempo, ainda existem lacunas importantes:

- ausencia de escopo maduro por papel e por contexto
- visao do responsavel ainda incompleta no backend e no AppKids
- avisos reais ainda nao implementados
- retirada segura ainda dependente de contrato fragil no checkout por QR
- painel operacional ainda mais proximo de historico do que de comando ao vivo
- ocorrencias e historico da experiencia da crianca ainda nao modelados
- sala, turma e capacidade ainda pouco estruturados

Conclusao:

Kids nao precisa ser "iniciado do zero". Ele precisa ser reorganizado para virar um modulo confiavel, seguro e operacionalmente forte.

## 2. Principios do modulo

As proximas entregas devem obedecer a estes principios:

- seguranca de retirada e prioridade maxima
- AppKids deve ser o canal principal do responsavel
- operacao do culto precisa funcionar sem improviso
- informacoes criticas devem aparecer com destaque
- permissao e escopo precisam ser validados no backend
- notificacao sem historico e sem trilha nao basta
- tudo que for sensivel precisa deixar auditoria

## 3. Papeis do modulo

### 3.1 Admin

Pode:

- configurar estruturas, fluxos e politicas
- operar excecoes
- ver indicadores, auditoria e historico amplo

### 3.2 Lider Kids

Pode:

- acompanhar o painel operacional do culto
- supervisionar salas, turmas, capacidade e ocorrencias
- intervir em fluxos operacionais permitidos

Nao deve:

- ter acesso irrestrito a todo o administrativo do sistema

### 3.3 Operador Kids

Pode:

- realizar check-in e check-out
- consultar dados operacionais da crianca
- registrar ocorrencias
- validar retirada

Nao deve:

- ver dados alem do necessario para a operacao

### 3.4 Responsavel

Pode:

- ver apenas as proprias criancas
- receber avisos relacionados
- acompanhar check-in e check-out
- participar do fluxo de retirada segura quando autorizado
- consultar historico permitido

Nao deve:

- ver criancas de terceiros
- acessar cadastros administrativos
- consultar historico operacional irrestrito

## 4. Visao funcional consolidada

O modulo Kids deve ser entendido como a combinacao de quatro experiencias principais.

### 4.1 Experiencia do responsavel

Canal principal: AppKids

O responsavel deve conseguir:

- ver minhas criancas
- ver status atual da crianca no culto
- receber avisos gerais e direcionados
- acompanhar check-in e retirada
- apresentar QR, PIN ou token de retirada
- consultar historico resumido quando fizer sentido
- visualizar alertas relevantes sobre a experiencia da crianca

### 4.2 Experiencia da equipe operacional

Canal principal: painel web e fluxos operacionais de apoio

A equipe deve conseguir:

- localizar crianca rapidamente
- fazer check-in com contexto de sala ou turma
- ver alertas criticos em destaque
- validar retirada com seguranca
- registrar ocorrencias do culto
- acompanhar quem ainda nao saiu

### 4.3 Experiencia da lideranca

Canal principal: painel operacional do culto

A lideranca deve conseguir:

- acompanhar ocupacao por sala
- ver presentes, pendentes e retiradas
- acompanhar criancas com alertas criticos
- visualizar ocorrencias abertas
- agir rapidamente em gargalos de sala, lotacao e pendencia

### 4.4 Experiencia administrativa

Canal principal: frontend web administrativo

O administrativo deve conseguir:

- manter dados de criancas e responsaveis
- controlar autorizacoes de retirada
- configurar salas, turmas e capacidade
- revisar historicos e trilhas
- auditar excecoes e alteracoes sensiveis

## 5. Capacidades do modulo

### 5.1 Cadastro e vinculos

Escopo:

- cadastro da crianca
- dados sensiveis de cuidado
- vinculo com responsaveis
- controle de quem pode retirar
- vinculo com sala, turma e faixa etaria

Resultado esperado:

- base confiavel para operacao, seguranca e comunicacao

### 5.2 Check-in

Escopo:

- check-in por operador
- check-in com QR
- vinculo do check-in a uma sessao de culto
- geracao de codigo de sessao
- exibicao imediata de alertas criticos
- disparo de notificacao ao responsavel

Resultado esperado:

- entrada rapida, com seguranca e registro correto

### 5.3 Retirada segura

Escopo:

- token de retirada por sessao
- QR dinamico ou PIN temporario
- lista de autorizados
- validacao do operador
- tratamento de excecao
- trilha completa da retirada

Resultado esperado:

- eliminacao de retirada informal ou fragil

### 5.4 Avisos e comunicacao

Escopo:

- avisos gerais
- avisos por culto
- avisos por sala ou turma
- avisos por crianca
- avisos por responsavel
- feed persistente no app
- push como complemento

Resultado esperado:

- AppKids deixa de ser acessorio e vira canal real de comunicacao

### 5.5 Painel operacional do culto

Escopo:

- presentes agora
- nao retirados
- distribuicao por sala e turma
- capacidade e lotacao
- alertas operacionais
- ocorrencias abertas
- acoes rapidas operacionais

Resultado esperado:

- lideres operam o culto com clareza, sem depender de memoria ou checagem manual espalhada

### 5.6 Ocorrencias e historico

Escopo:

- febre
- queda
- choro persistente
- troca de sala
- necessidade de contato com responsavel
- medicacao ou observacao especial
- historico por crianca

Resultado esperado:

- registro confiavel do que aconteceu com a crianca durante a experiencia no Kids

### 5.7 Informacoes criticas em destaque

Escopo:

- alergias
- restricoes medicas
- observacoes de cuidado
- necessidades especiais
- alertas de recepcao ou equipe

Resultado esperado:

- informacao importante aparece antes do erro, nao depois do incidente

### 5.8 Sala, turma e capacidade

Escopo:

- estrutura de salas e turmas
- capacidade maxima
- distribuicao por culto
- lotacao em tempo real
- base para operacao por faixa etaria

Resultado esperado:

- melhor organizacao, seguranca e previsibilidade operacional

## 6. Sequencia recomendada

O modulo nao deve crescer por "feature isolada". A ordem recomendada precisa respeitar risco, valor e dependencias.

### Fase 0 - Fundacao obrigatoria

Objetivo:

- evitar que o restante do modulo seja construido sobre contratos e permissoes incompletos

Entregas:

- definicao formal dos papeis de Kids
- definicao dos escopos de acesso no backend
- revisao dos endpoints atuais de Kids
- definicao do contexto operacional de culto, sessao, sala e turma
- checklist de auditoria, log e testes minimos

Sem esta fase, as proximas entregas tendem a gerar retrabalho.

### Fase 1 - Contexto do responsavel e avisos reais

Objetivo:

- fazer o AppKids passar a entregar valor real para os pais

Entregas:

- endpoints de "minhas criancas"
- endpoints de "meus avisos"
- avisos gerais e direcionados
- feed de avisos no AppKids
- push integrado ao feed
- visao do estado atual da crianca para o responsavel

Motivo da prioridade:

- entrega valor rapidamente
- consolida o AppKids como canal principal
- prepara o terreno para retirada segura

### Fase 2 - Retirada segura

Objetivo:

- transformar retirada em fluxo confiavel e auditavel

Entregas:

- QR dinamico por sessao
- PIN ou OTP temporario
- lista de autorizados com destaque
- validacao operacional na saida
- excecao controlada para retirada fora da regra
- trilha completa do evento de retirada

Motivo da prioridade:

- e a frente de maior impacto em seguranca e confianca

### Fase 3 - Painel operacional do culto

Objetivo:

- dar comando em tempo real para lideres e equipe

Entregas:

- painel ao vivo por culto
- presentes, pendentes e retiradas
- lotacao por sala
- alertas criticos visiveis
- ocorrencias abertas
- acoes operacionais rapidas

Motivo da prioridade:

- melhora fortemente a operacao no momento do culto

### Fase 4 - Ocorrencias e historico

Objetivo:

- registrar e consultar o que acontece com a crianca durante a experiencia

Entregas:

- cadastro de ocorrencias
- timeline por crianca
- consulta operacional
- consulta resumida para responsavel quando aplicavel

Motivo da prioridade:

- fecha a trilha de cuidado, seguranca e memoria institucional

### Fase 5 - Refinos estruturais

Objetivo:

- elevar maturidade e escala do modulo

Entregas:

- capacidade por sala
- controles por culto e data
- melhorias de UX operacional
- indicadores e relatorios
- automacoes futuras

## 7. Backlog recomendado por epicos

### Epico 1 - Seguranca, permissao e contexto

Itens:

- formalizar politica de acesso por papel
- criar escopo por responsavel vinculado a crianca
- restringir endpoints de consulta e operacao conforme contexto
- registrar auditoria de operacoes sensiveis
- definir criterios de excecao operacional

Prioridade:

- critica

Dependencias:

- nenhuma

### Epico 2 - Contexto do responsavel

Itens:

- endpoint para minhas criancas
- endpoint para meus check-ins ativos
- endpoint para meus historicos permitidos
- ajuste do login e da restauracao de sessao para suportar a experiencia do responsavel

Prioridade:

- critica

Dependencias:

- epico 1

### Epico 3 - Avisos reais

Itens:

- listar avisos
- criar aviso geral
- criar aviso por sala ou turma
- criar aviso por crianca
- criar aviso para responsavel especifico
- marcar como lido
- integrar push

Prioridade:

- alta

Dependencias:

- epicos 1 e 2

### Epico 4 - Retirada segura

Itens:

- redesenhar contrato de checkout
- permitir retirada por token valido da sessao
- QR dinamico e temporario
- PIN ou OTP alternativo
- validacao de autorizado
- fluxo de excecao com trilha

Prioridade:

- critica

Dependencias:

- epicos 1 e 2

### Epico 5 - Painel operacional do culto

Itens:

- visao em tempo real por culto
- agrupamento por sala e turma
- destaque de alertas criticos
- indicadores de presentes, nao retirados e lotacao
- filtros e acoes operacionais

Prioridade:

- alta

Dependencias:

- epicos 1 e 4

### Epico 6 - Ocorrencias e historico

Itens:

- modelar tipos de ocorrencia
- registrar ocorrencia operacional
- timeline por crianca
- vinculacao de contato com responsavel
- destaque de historico relevante

Prioridade:

- alta

Dependencias:

- epicos 1 e 5

### Epico 7 - Estrutura de sala, turma e capacidade

Itens:

- cadastro de sala
- cadastro de turma
- capacidade maxima
- vinculacao por culto e data
- indicadores de ocupacao

Prioridade:

- media para alta

Dependencias:

- epicos 1 e 5

### Epico 8 - Qualidade e confiabilidade

Itens:

- testes de check-in e checkout
- testes de permissao por responsavel
- logs de eventos criticos
- checklist de regressao do modulo
- observabilidade de notificacoes e push

Prioridade:

- critica e continua

Dependencias:

- acompanha todos os epicos

## 8. Backlog por fase de implementacao

### Sprint A - Fundacao do modulo

Meta:

- fechar regras antes de ampliar superficie

Escopo:

- matriz de acesso por papel
- revisao de endpoints existentes
- definicao de entidades faltantes
- criterios de auditoria
- definicao do fluxo oficial de retirada segura

Saida esperada:

- base funcional e tecnica validada

### Sprint B - Responsavel e avisos

Meta:

- fazer o AppKids entregar valor real

Escopo:

- minhas criancas
- meus avisos
- avisos gerais e direcionados
- feed no app
- push orientando para o feed

Saida esperada:

- responsavel passa a usar o app para acompanhar a experiencia

### Sprint C - Retirada segura

Meta:

- fechar o principal risco de seguranca do modulo

Escopo:

- token por sessao
- QR ou PIN temporario
- autorizados
- auditoria da retirada
- UX operacional de validacao

Saida esperada:

- retirada confiavel, validavel e auditavel

### Sprint D - Painel operacional

Meta:

- equipar lideres e equipe para o culto ao vivo

Escopo:

- painel em tempo real
- visao por sala
- lotacao
- pendencias
- alertas
- acoes operacionais

Saida esperada:

- lideres operam o culto com visibilidade e controle

### Sprint E - Ocorrencias e historico

Meta:

- consolidar a memoria operacional e de cuidado

Escopo:

- registro de ocorrencias
- timeline por crianca
- consultas operacionais e administrativas
- exposicao controlada ao responsavel quando fizer sentido

Saida esperada:

- historico util e confiavel da experiencia da crianca

## 9. Direcao recomendada para AppKids

O AppKids deve assumir explicitamente o papel de aplicativo do responsavel.

Isso significa:

- a home principal deve ser centrada em "minhas criancas"
- avisos devem estar na primeira camada da experiencia
- o app deve mostrar estado atual de check-in
- o fluxo de retirada deve poder ser iniciado ou suportado pelo app
- o historico visivel no app deve ser proposital e limitado

O AppKids nao deve ser pensado apenas como uma extensao do fluxo operacional interno. Ele deve ser o canal principal de confianca do responsavel.

## 10. Decisoes de arquitetura sugeridas

### Backend

Prioridades:

- separar claramente endpoints administrativos de endpoints do responsavel
- criar endpoints "me"
- revisar contrato de checkout
- modelar avisos e ocorrencias como entidades operacionais de primeira classe

### Frontend web

Prioridades:

- evoluir de historico para painel operacional
- criar telas de avisos, ocorrencias e estruturas de sala ou turma
- destacar informacoes criticas visualmente

### AppKids

Prioridades:

- sair de placeholder em avisos
- assumir visao de responsavel
- suportar retirada segura
- manter UX simples, clara e confiavel

## 11. Ordem objetiva recomendada

Se a equipe precisar de uma sequencia unica e direta, a recomendacao e:

1. fechar fundacao de permissao, escopo e contratos
2. entregar contexto do responsavel e avisos reais no AppKids
3. implementar retirada segura
4. construir painel operacional do culto
5. registrar ocorrencias e historico
6. refinar capacidade, indicadores e automacoes

## 12. Resultado esperado

Ao seguir essa ordem, o modulo Kids deixa de ser apenas um conjunto de endpoints e telas isoladas e passa a operar como:

- produto confiavel para responsaveis
- ferramenta de comando para lideres
- fluxo seguro para equipe
- modulo auditavel e escalavel para a igreja
