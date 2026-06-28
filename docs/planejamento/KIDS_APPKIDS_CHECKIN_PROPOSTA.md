# Kids / AppKids - Proposta de fluxo de check-in no app

## Objetivo

Definir o fluxo recomendado de check-in no `AppKids` com base em:

- o estado atual do módulo `Kids`
- o papel do `AppKids` como app do responsável
- o padrão observado em concorrentes como `Planning Center`, `Church Center`, `KidCheck` e `Breeze`

O objetivo não é apenas "colocar um botão de check-in", mas estruturar um fluxo:

- simples para os responsáveis
- seguro para a operação
- compatível com recepção, sala, turma e retirada segura

---

## Conclusão recomendada

O melhor modelo para o `AppKids` é:

`pré-check-in no app` + `confirmação presencial pela equipe`

Em outras palavras:

1. o responsável inicia o check-in no app
2. o sistema gera um `pré-check-in` temporário
3. a recepção ou equipe do `Kids` valida esse pré-check-in no local
4. só depois disso o `check-in` real é concluído
5. ao concluir, o sistema ativa o contexto operacional da criança e a retirada segura

Esse modelo entrega:

- conveniência para a família
- menos fila
- segurança operacional
- coerência com o restante do módulo

---

## O que não recomendamos

### 1. Check-in totalmente autônomo no app

Fluxo:

- responsável toca em `check-in`
- sistema já marca a criança como presente

Problemas:

- não garante chegada física real
- reduz capacidade de triagem da equipe
- enfraquece o controle de sala, lotação e recepção
- pode gerar check-in "fantasma"

### 2. App só como consulta e sem qualquer ação de entrada

Fluxo:

- responsável só acompanha
- todo check-in acontece fora do app

Problemas:

- o app perde valor operacional importante
- a experiência fica abaixo do mercado
- não aproveita o canal direto com os responsáveis

### 3. QR estático por criança

Problemas:

- reuso indevido
- baixa segurança
- pouca rastreabilidade

---

## Fluxo funcional ideal

### Etapa 1. Seleção do culto/sessão

No `AppKids`, o responsável acessa `Minhas crianças` e escolhe a criança.

Se houver uma sessão de `Kids` elegível, o app mostra:

- culto/data/horário
- sala/turma sugerida, quando aplicável
- status da janela de check-in

Exemplos de estado:

- `Check-in disponível`
- `Check-in ainda não liberado`
- `Janela encerrada`

### Etapa 2. Pré-check-in no app

O responsável toca em `Iniciar check-in`.

O app envia uma solicitação ao backend e recebe:

- `preCheckinId`
- `qrToken`
- `expiraEm`
- contexto da sessão

Esse estado ainda não representa presença concluída.

É um `pré-check-in pendente de validação`.

### Etapa 3. Validação na recepção

Na entrada do `Kids`, a equipe:

- escaneia o `QR` do app
- ou busca pelo `preCheckinId`
- ou localiza a criança/responsável na lista de pré-check-ins pendentes

Antes da confirmação final, a equipe ainda pode:

- revisar dados críticos
- conferir alergias/restrições
- ajustar sala/turma
- validar capacidade da sala
- registrar alguma observação de entrada

### Etapa 4. Confirmação do check-in

Depois da conferência, a equipe conclui o check-in.

Nesse momento o sistema:

- cria o `KidsCheckin` oficial
- marca a criança como presente
- define sessão/sala/turma
- registra operador e horário
- ativa a retirada segura
- libera token/PIN de retirada no `AppKids`

### Etapa 5. Acompanhamento no AppKids

Após a confirmação:

- a criança aparece como `em check-in`
- o responsável visualiza o estado atualizado
- o app mostra o contexto de retirada segura
- o responsável pode receber avisos da sala/turma

---

## Regras de negócio recomendadas

### Regra 1. O pré-check-in expira

O QR/token do pré-check-in deve ter validade curta.

Sugestão:

- entre `5` e `15` minutos

Se expirar:

- o responsável precisa gerar novo pré-check-in

### Regra 2. Só pode existir um pré-check-in ativo por criança por sessão

Evita duplicidade e comportamento confuso.

### Regra 3. O pré-check-in não substitui presença

Enquanto não for confirmado:

- não entra na contagem operacional de presentes
- não ativa retirada segura
- não deve ser tratado como check-in real

### Regra 4. A equipe pode converter pré-check-in em check-in com ajustes

No momento da validação, a equipe pode:

- alterar sala
- alterar turma
- cancelar o pré-check-in
- concluir com observação

### Regra 5. Deve existir fallback sem QR

Casos reais:

- celular sem bateria
- internet ruim
- app indisponível

Fallbacks possíveis:

- busca por nome/responsável
- telefone
- código curto de pré-check-in

### Regra 6. O check-in deve respeitar janela e elegibilidade

O backend deve validar:

- sessão aberta
- criança ativa
- vínculo com responsável ativo
- sala/turma válida, quando exigida
- capacidade, quando aplicável

---

## Proposta de experiência no AppKids

### Tela da criança

Adicionar seção:

- `Check-in`

Estados:

- `Disponível para check-in`
- `Pré-check-in gerado`
- `Em check-in`
- `Check-in encerrado`

### Quando disponível

Mostrar:

- botão `Iniciar check-in`

### Após pré-check-in

Mostrar:

- QR temporário
- validade
- botão `Atualizar QR`
- mensagem: `Apresente este QR na recepção para concluir a entrada`

### Após check-in confirmado

Mostrar:

- status `Em check-in`
- sala/turma
- horário de entrada
- retirada segura

---

## Impacto no Admin / operação

### Painel Kids

Adicionar ao painel:

- contador de `pré-check-ins pendentes`
- lista de pendentes por sessão
- ação rápida `confirmar check-in`
- ação `cancelar pré-check-in`

### Recepção / estação operacional

Fluxos necessários:

- escanear QR do pré-check-in
- localizar pendente manualmente
- confirmar entrada
- ajustar sala/turma antes da confirmação

---

## Contratos técnicos sugeridos

### App / responsável

`POST /api/kids/me/pre-checkins`

Request:

- `criancaPessoaId`
- `sessaoId`

Response:

- `preCheckinId`
- `qrToken`
- `expiraEm`
- `status`

`GET /api/kids/me/pre-checkins/ativos`

Retorna os pré-check-ins ativos do responsável.

`DELETE /api/kids/me/pre-checkins/{id}`

Cancela um pré-check-in ainda não confirmado.

### Operação / Admin

`GET /api/kids/pre-checkins`

Lista pendentes por sessão/sala/status.

`POST /api/kids/pre-checkins/confirmar`

Converte o pré-check-in em `KidsCheckin`.

`POST /api/kids/pre-checkins/cancelar`

Cancela administrativamente.

---

## Ajustes de modelo sugeridos

Criar entidade própria:

- `KidsPreCheckin`

Campos recomendados:

- `Id`
- `CriancaPessoaId`
- `ResponsavelPessoaId`
- `SessaoId`
- `SalaId`
- `TurmaId`
- `QrToken`
- `CodigoCurto`
- `Status`
- `ExpiraEm`
- `ConfirmadoEm`
- `ConfirmadoPorPessoaId`
- `CanceladoEm`
- `CanceladoPorPessoaId`
- `Observacoes`
- `TenantId`

Status sugeridos:

- `Pending`
- `Confirmed`
- `Expired`
- `Cancelled`

---

## Benefícios desse modelo

### Para os responsáveis

- menos fila
- mais previsibilidade
- experiência moderna no app

### Para a operação

- controle presencial mantido
- menos digitação na recepção
- melhor organização por sala/turma

### Para segurança

- sem check-in fantasma
- rastreabilidade de quem iniciou e quem confirmou
- base melhor para retirada segura

---

## Riscos e cuidados

### 1. Não transformar pré-check-in em complexidade excessiva

Se o fluxo tiver muitos passos, ele perde o benefício.

### 2. Não esquecer os visitantes

Visitantes e pais sem app precisam continuar tendo fluxo simples no local.

### 3. Não misturar pré-check-in com retirada

São problemas diferentes:

- entrada
- saída

Devem conversar, mas não se confundir.

### 4. Não depender só de QR

Sempre manter fallback operacional.

---

## Recomendação de implementação

### Fase 1

- entidade `KidsPreCheckin`
- endpoints do responsável
- botão de iniciar check-in no `AppKids`
- geração de QR temporário
- confirmação manual no Admin

### Fase 2

- scanner de pré-check-in na operação
- lista de pendentes no painel
- cancelamento e expiração

### Fase 3

- refinamentos por sessão/sala/turma
- ajustes de UX
- métricas operacionais

---

## Recomendação final

Se o objetivo é ter um check-in competitivo, seguro e útil, o melhor caminho para o `AppKids` é:

`pré-check-in no app` + `confirmação presencial pela equipe`

Esse é o ponto de equilíbrio mais forte entre:

- experiência dos pais
- segurança da criança
- operação real do ministério
- coerência com o que o mercado faz melhor
