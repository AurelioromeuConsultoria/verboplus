# App Kids – Especificação funcional atual

## 1. Direção atual do app

O AppKids deixou de ser apenas um app operacional de check-in/check-out e passou a ser, prioritariamente, o app do **responsável**.

Hoje a direção principal do produto é:

- login
- minhas crianças
- detalhe da criança
- avisos reais
- retirada segura
- push

## 2. O que já existe hoje

### Backend consumido pelo app

- **Auth:** `POST /api/auth/login`, `GET /api/auth/me`
- **Contexto do responsável:**
  - `GET /api/kids/me/criancas`
  - `GET /api/kids/me/criancas/{id}`
  - `GET /api/kids/me/checkins`
- **Avisos:**
  - `GET /api/kids/me/avisos`
  - `PATCH /api/kids/me/avisos/{id}/lido`
- **Retirada segura:**
  - `POST /api/kids/retirada/validar`
  - `POST /api/kids/retirada/confirmar`
  - `POST /api/kids/retirada/excecao`
- **Push:**
  - `POST /api/kids/me/device-token`

### App Flutter

- login com token persistido
- home de responsável com `minhas crianças`
- detalhe da criança com status atual
- exibição de token, PIN e expiração de retirada quando houver check-in ativo
- feed real de avisos
- marcação de aviso como lido
- integração de push

## 3. Estado real dos fluxos

### Fluxo principal

- responsável entra no app
- vê suas crianças
- abre o detalhe da criança
- acompanha status, avisos e retirada segura

## 4. O que ainda falta ou está parcial

### App do responsável

- histórico mais rico visível ao responsável, quando fizer sentido
- exposição controlada de ocorrências visíveis ao responsável
- refinamento visual da home e do detalhe da criança

### Estrutura

- consumo futuro de `salas` e `turmas` no app, se isso trouxer valor ao responsável

## 5. Recomendações da próxima fase

### Fase 2 do AppKids

- reforçar o app como canal do responsável
- expor apenas informações realmente úteis e seguras
- manter a retirada segura como ponto central

### Expansões naturais

- histórico resumido da criança
- ocorrências visíveis ao responsável
- timeline de avisos por criança
- status mais claro na home

## 6. Resumo final

O estado correto do AppKids hoje é:

- **não** é mais um app com avisos placeholder
- **não** depende mais de fluxo operacional legado interno
- **já é** um app funcional para responsável com avisos reais e retirada segura

O próximo passo do app não é “começar a funcionar”, e sim:

- refinar
- aprofundar a experiência do responsável com segurança
