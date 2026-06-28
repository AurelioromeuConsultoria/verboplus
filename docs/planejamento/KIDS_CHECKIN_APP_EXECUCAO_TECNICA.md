# Kids / AppKids - Execução técnica do check-in no app

## Objetivo

Detalhar tecnicamente a implementação do fluxo de:

- `pré-check-in` no `AppKids`
- `confirmação presencial` pela equipe
- `conversão` em `KidsCheckin` oficial

Este documento complementa:

- [KIDS_APPKIDS_CHECKIN_PROPOSTA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_APPKIDS_CHECKIN_PROPOSTA.md)

---

## Escopo desta frente

### Entra

- entidade `KidsPreCheckin`
- endpoints `me/*` para o responsável
- geração de QR/token temporário
- listagem de pré-check-ins pendentes no Admin
- confirmação manual de pré-check-in
- cancelamento
- expiração

### Não entra

- impressão física de etiqueta
- automação de fila
- suporte offline
- visitor onboarding completo
- auto check-in sem validação da equipe

---

## Estado atual

Hoje o módulo já possui:

- vínculo `responsável -> criança`
- `AppKids` com `minhas crianças`
- painel operacional web
- `KidsCheckin` oficial
- retirada segura
- salas/turmas

O que falta é a camada intermediária entre:

- intenção do responsável de chegar
- confirmação real da recepção

---

## Modelo de domínio

## Nova entidade

`KidsPreCheckin`

Campos sugeridos:

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
- `ObservacoesResponsavel`
- `CriadoEm`
- `ConfirmadoEm`
- `ConfirmadoPorPessoaId`
- `CanceladoEm`
- `CanceladoPorPessoaId`
- `CancelamentoMotivo`
- `TenantId`

### Status

- `Pending`
- `Confirmed`
- `Expired`
- `Cancelled`

### Observações de modelagem

- `QrToken` deve ser único
- `CodigoCurto` é fallback operacional
- `SalaId` e `TurmaId` podem ser sugestão inicial, não decisão final obrigatória
- `SessaoId` deve ser obrigatório se o produto já tiver contexto oficial de sessão/culto

---

## Regras técnicas centrais

### Regra 1

Só pode existir um `pre-checkin` ativo por:

- `crianca`
- `sessão`
- `tenant`

### Regra 2

`pre-checkin` pendente não conta como presença.

### Regra 3

Ao confirmar:

- cria `KidsCheckin`
- fecha `KidsPreCheckin` como `Confirmed`
- ativa token/PIN de retirada

### Regra 4

Ao expirar:

- status muda para `Expired`
- não pode mais ser confirmado

### Regra 5

Ao cancelar:

- precisa registrar `quem` cancelou
- e `motivo`, quando for cancelamento administrativo

---

## Endpoints sugeridos

## Responsável / AppKids

### `POST /api/kids/me/pre-checkins`

Cria um pré-check-in.

Request:

- `criancaPessoaId`
- `sessaoId`
- `salaId` opcional
- `turmaId` opcional
- `observacoes` opcional

Response:

- `id`
- `status`
- `qrToken`
- `codigoCurto`
- `expiraEm`
- `sessao`
- `crianca`

### `GET /api/kids/me/pre-checkins`

Lista pré-check-ins do responsável.

Filtros úteis:

- `status`
- `ativosOnly`

### `GET /api/kids/me/pre-checkins/{id}`

Detalhe do pré-check-in.

### `DELETE /api/kids/me/pre-checkins/{id}`

Cancela um pré-check-in ainda pendente.

---

## Operação / Admin

### `GET /api/kids/pre-checkins`

Lista pendentes para recepção/operação.

Filtros úteis:

- `status`
- `sessaoId`
- `salaId`
- `turmaId`
- `busca`

### `POST /api/kids/pre-checkins/validar`

Valida QR/token/código curto e retorna o contexto do pré-check-in.

Request:

- `qrToken` ou `codigoCurto`

### `POST /api/kids/pre-checkins/{id}/confirmar`

Confirma a entrada e cria o `KidsCheckin`.

Request:

- `salaId` opcional
- `turmaId` opcional
- `observacoesEquipe` opcional

### `POST /api/kids/pre-checkins/{id}/cancelar`

Cancela administrativamente.

Request:

- `motivo`

---

## DTOs sugeridos

### `CreateKidsPreCheckinRequest`

- `CriancaPessoaId`
- `SessaoId`
- `SalaId`
- `TurmaId`
- `Observacoes`

### `KidsPreCheckinDto`

- `Id`
- `Status`
- `CriancaPessoaId`
- `CriancaNome`
- `ResponsavelPessoaId`
- `ResponsavelNome`
- `SessaoId`
- `SalaId`
- `TurmaId`
- `QrToken`
- `CodigoCurto`
- `ExpiraEm`
- `CriadoEm`

### `ConfirmKidsPreCheckinRequest`

- `SalaId`
- `TurmaId`
- `ObservacoesEquipe`

### `CancelKidsPreCheckinRequest`

- `Motivo`

---

## Backend

## Camadas a alterar

### Domain

- nova entidade `KidsPreCheckin`

### Infrastructure

- configuração EF
- migration
- repositório

### Application

- DTOs
- interface de serviço
- serviço de pré-check-in
- validações de escopo do responsável e da operação

### API

- endpoints `me/*`
- endpoints administrativos

---

## Serviço sugerido

`KidsPreCheckinService`

Responsabilidades:

- criar pré-check-in
- validar elegibilidade
- expirar pendentes
- listar pendentes
- validar QR/token
- confirmar
- cancelar

Dependências prováveis:

- `ICriancaDetalheRepository`
- `IResponsavelCriancaRepository`
- `IKidsCheckinRepository`
- `IKidsEstruturaRepository`
- `IKidsPreCheckinRepository`
- `IKidsAuthorizationService`
- `ICurrentUserContext`
- `ILogger`
- `IUnitOfWork`

---

## AppKids

## Mudanças sugeridas

### Tela `Minhas crianças`

Mostrar estado resumido:

- `Check-in disponível`
- `Pré-check-in gerado`
- `Em check-in`

### Tela de detalhe da criança

Adicionar seção:

- `Check-in`

Estados:

- botão `Iniciar check-in`
- card com QR/token do pré-check-in
- validade
- ação `Cancelar`

### Repositório

Adicionar métodos:

- `criarPreCheckin`
- `listarPreCheckins`
- `cancelarPreCheckin`

---

## Admin / Web

## Mudanças sugeridas

### Painel Kids

Adicionar:

- card `pré-check-ins pendentes`
- lista de pendentes
- ação `Confirmar`
- ação `Cancelar`

### Fluxo operacional

Primeira versão pode ser sem scanner:

- seleção manual do pendente
- confirmação pela lista

Depois:

- leitura de QR/token

---

## UX recomendada

### App

O responsável não deve ver linguagem técnica como:

- `Pending`
- `Confirmed`

Usar:

- `Aguardando validação`
- `Apresente este QR na recepção`
- `Pré-check-in confirmado`

### Admin

A equipe deve ver:

- criança
- responsável
- sala/turma sugerida
- expiração
- alertas críticos

---

## Logs e auditoria

Registrar:

- criação de pré-check-in
- cancelamento pelo responsável
- expiração
- validação
- confirmação
- cancelamento administrativo

Com:

- `criancaPessoaId`
- `responsavelPessoaId`
- `operadorPessoaId`
- `sessaoId`
- `tenantId`

---

## Testes recomendados

### Backend

- cria pré-check-in válido
- bloqueia criança sem vínculo com responsável
- bloqueia duplicidade ativa
- bloqueia confirmação de expirado
- confirma e cria `KidsCheckin`
- cancela corretamente

### App

- exibe botão quando elegível
- exibe QR quando pendente
- exibe cancelamento

### Frontend web

- lista pendentes
- confirma pendente
- cancela pendente

---

## Definition of Done

Para considerar a frente entregue:

- responsável consegue gerar pré-check-in no app
- equipe consegue localizar e confirmar no Admin
- `KidsCheckin` real é criado corretamente
- retirada segura é ativada após confirmação
- expiração e cancelamento funcionam
- testes principais passam
