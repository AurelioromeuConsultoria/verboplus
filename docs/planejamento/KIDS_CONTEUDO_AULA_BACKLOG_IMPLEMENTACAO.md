# Kids / AppKids

## Backlog de Implementação

### Frente: Conteúdo da Aula

---

## KC-01

### Objetivo

Criar a base de domínio do módulo.

### Arquivos principais

- `BackEnd/src/SistemaIgreja.Domain/Entities/*`
- `BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs`

### Entregas

- `KidsConteudoAula`
- `KidsConteudoAulaAnexo`
- índices e relacionamentos

### Critério de aceite

- entidades compilam
- mapping no EF consistente

---

## KC-02

### Objetivo

Gerar persistência e migration.

### Arquivos principais

- `BackEnd/src/SistemaIgreja.Infrastructure/Repositories/*`
- `BackEnd/src/SistemaIgreja.Infrastructure/Migrations/*`

### Entregas

- repositórios
- migration inicial

### Critério de aceite

- banco atualiza sem erro

---

## KC-03

### Objetivo

Criar DTOs e contratos administrativos.

### Arquivos principais

- `BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs`

### Entregas

- requests e responses do módulo

### Critério de aceite

- contratos cobrem criação, edição, listagem e detalhe

---

## KC-04

### Objetivo

Criar service administrativo de conteúdo da aula.

### Arquivos principais

- `BackEnd/src/SistemaIgreja.Application/Services/*`

### Entregas

- criar
- editar
- publicar
- arquivar
- listar

### Critério de aceite

- regras de sala/turma aplicadas
- publicação respeita permissões

---

## KC-05

### Objetivo

Criar endpoints administrativos.

### Arquivos principais

- `BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs`

### Entregas

- endpoints CRUD/publicação

### Critério de aceite

- respostas consistentes
- erros de permissão corretos

---

## KC-06

### Objetivo

Criar leitura `me/*` para o `AppKids`.

### Arquivos principais

- `BackEnd/src/SistemaIgreja.Application/Services/*`
- `BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs`

### Entregas

- `GET me/conteudos-aula`
- `GET me/conteudos-aula/{id}`
- `POST me/conteudos-aula/{id}/visualizado`

### Critério de aceite

- responsável só vê o que é do seu escopo

---

## KC-07

### Objetivo

Subir a tela administrativa de conteúdo.

### Arquivos principais

- `FrontEnd/src/api/kids.js`
- `FrontEnd/src/pages/Kids/*`

### Entregas

- listagem
- formulário
- ações de publicar/arquivar

### Critério de aceite

- operação administrativa básica usável

---

## KC-08

### Objetivo

Subir feed de conteúdos no `AppKids`.

### Arquivos principais

- `AppKids/lib/features/*`

### Entregas

- card na home
- listagem
- detalhe

### Critério de aceite

- responsável encontra conteúdo recente facilmente

---

## KC-09

### Objetivo

Renderizar anexos e links.

### Arquivos principais

- `AppKids/lib/features/*`

### Entregas

- abrir PDF
- abrir imagem
- abrir link

### Critério de aceite

- todos os tipos de anexo suportados no MVP funcionam

---

## KC-10

### Objetivo

Integrar push contextual.

### Arquivos principais

- backend de comunicação
- `AppKids`

### Entregas

- push ao publicar
- deep link para conteúdo

### Critério de aceite

- conteúdo continua existindo no feed mesmo sem push

---

## KC-11

### Objetivo

Adicionar rastreio de visualização.

### Arquivos principais

- backend
- app

### Entregas

- marcação de visualizado
- dado disponível para futura métrica

### Critério de aceite

- abertura do conteúdo registra consumo

---

## KC-12

### Objetivo

Cobrir a frente com testes e checklist.

### Arquivos principais

- `BackEnd/tests/*`
- documento de regressão

### Entregas

- testes da service
- checklist mínimo

### Critério de aceite

- build e testes da frente verdes
