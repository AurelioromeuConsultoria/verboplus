# Kids / AppKids

## Backlog por Fases

### Frente: Conteúdo da Aula

---

## Fase 1

### Objetivo

Colocar no ar um MVP útil para publicação e leitura de conteúdo da aula.

### Escopo

- entidade de `ConteudoAulaKids`
- entidade de `ConteudoAulaKidsAnexo`
- publicação por `sala/turma`
- campos:
  - `titulo`
  - `tema`
  - `versiculo`
  - `resumo`
  - `atividadeEmCasa`
  - `observacaoResponsavel`
- anexos:
  - PDF
  - imagem
  - link
- listagem no Admin
- publicação no `AppKids`
- push opcional complementar

### Entregas

- `Admin`: criar, editar, publicar, listar
- `AppKids`: ver conteúdo do dia e recentes
- histórico inicial

---

## Fase 2

### Objetivo

Melhorar experiência, segmentação e consumo.

### Escopo

- filtro por criança no app
- histórico mais rico
- destaque de conteúdo mais recente
- confirmação de leitura
- painel de visualizações no Admin

### Entregas

- `AppKids`: feed mais organizado
- `Admin`: visão básica de engajamento

---

## Fase 3

### Objetivo

Expandir o conteúdo da aula para discipulado familiar contínuo.

### Escopo

- desafio da semana
- memória do dia
- calendário infantil
- checklist do responsável

---

## Backlog técnico sugerido

### Backend

1. criar `ConteudoAulaKids`
2. criar `ConteudoAulaKidsAnexo`
3. criar repositórios
4. criar service de publicação
5. criar endpoints administrativos
6. criar endpoints `me/*` para o app
7. integrar push contextual
8. criar testes

### Frontend Admin

1. nova área `Kids > Conteúdo`
2. formulário de publicação
3. upload/anexo de link
4. listagem de conteúdos
5. edição/despublicação

### AppKids

1. card de conteúdo em destaque
2. listagem de conteúdos recentes
3. tela de detalhe do conteúdo
4. visualização de anexos
5. filtro por criança

---

## PRs sugeridos

### PR 1

Base de domínio e banco

- entidades
- mapping
- migration
- DTOs iniciais

### PR 2

Backend administrativo

- service
- CRUD de publicação
- testes

### PR 3

Admin web

- tela de criação
- tela de listagem
- ações básicas

### PR 4

Backend do `AppKids`

- endpoints de leitura
- feed persistente

### PR 5

`AppKids`

- home com conteúdo
- detalhe
- anexos

### PR 6

Push + histórico + refinamentos

---

## Critério de sucesso do MVP

O MVP pode ser considerado bem-sucedido quando:

- a equipe publica um conteúdo do dia sem depender de WhatsApp
- o responsável encontra o conteúdo no app com facilidade
- PDF/imagem/link funcionam bem
- o conteúdo fica disponível depois do culto
- a experiência é simples o suficiente para o time realmente usar

---

## Minha recomendação final

Se formos abrir essa frente agora, eu começaria por uma sprint curta focada em:

1. `Conteúdo da Aula` no Admin
2. `visualização no AppKids`
3. `PDF/imagem/link`

Sem tentar colocar engajamento avançado logo de início.

Isso já entrega algo muito forte e usável.
