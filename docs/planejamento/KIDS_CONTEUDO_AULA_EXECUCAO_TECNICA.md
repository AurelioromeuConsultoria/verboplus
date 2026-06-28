# Kids / AppKids

## Execução Técnica

### Frente: Conteúdo da Aula

---

## Objetivo da frente

Criar um módulo de `Conteúdo da Aula` que permita à equipe do Kids publicar materiais e resumos da aula do dia para responsáveis dentro do `AppKids`, com histórico e segmentação por sala/turma.

---

## Objetivo da Fase 1

Entregar um MVP funcional que permita:

- publicar conteúdo da aula no Admin
- anexar `PDF`, `imagem` e `link`
- segmentar por `sala` e `turma`
- exibir o conteúdo no `AppKids`
- manter histórico recente

---

## O que entra

- modelagem de conteúdo da aula
- modelagem de anexos
- CRUD administrativo inicial
- endpoints de leitura para responsáveis
- listagem no `AppKids`
- detalhe da publicação
- push complementar opcional

---

## O que não entra nesta fase

- comentários/respostas dos pais
- chat
- métricas avançadas de engajamento
- workflow editorial complexo
- publicação por criança individual
- upload com edição de imagem

---

## Modelagem sugerida

### Entidade principal

`KidsConteudoAula`

Campos sugeridos:

- `Id`
- `TenantId`
- `Titulo`
- `Tema`
- `Versiculo`
- `Resumo`
- `AtividadeEmCasa`
- `ObservacaoResponsavel`
- `Status`
  - `Draft`
  - `Published`
  - `Archived`
- `DataReferencia`
- `EventoOcorrenciaId`
- `SalaId`
- `TurmaId`
- `PublicadoEm`
- `PublicadoPorPessoaId`
- `CriadoEm`
- `AtualizadoEm`

### Entidade de anexo

`KidsConteudoAulaAnexo`

Campos sugeridos:

- `Id`
- `TenantId`
- `ConteudoAulaId`
- `Tipo`
  - `Pdf`
  - `Imagem`
  - `Link`
- `NomeExibicao`
- `Url`
- `StoragePath`
- `MimeType`
- `TamanhoBytes`
- `Ordem`
- `CriadoEm`

---

## Regras de negócio

### Publicação

- somente equipe autorizada publica conteúdo
- publicação pode ser:
  - geral
  - por sala
  - por turma
- turma não pode ser usada fora da sala correspondente
- conteúdo publicado precisa ter pelo menos:
  - `titulo`
  - `resumo`
  - `dataReferencia`

### Anexos

- anexos aceitos no MVP:
  - PDF
  - imagem
  - link
- anexos devem ser ordenáveis
- anexos precisam respeitar validação de extensão e mime type

### Exposição no AppKids

- responsável só vê conteúdos aplicáveis às crianças vinculadas
- se houver mais de uma criança, o app deve indicar claramente a associação
- feed é persistente e consultável

---

## Contratos sugeridos

### Admin

`POST /api/kids/conteudos-aula`

Cria publicação.

`PUT /api/kids/conteudos-aula/{id}`

Atualiza publicação.

`GET /api/kids/conteudos-aula`

Lista publicações.

`GET /api/kids/conteudos-aula/{id}`

Detalhe da publicação.

`POST /api/kids/conteudos-aula/{id}/publicar`

Publica conteúdo.

`POST /api/kids/conteudos-aula/{id}/arquivar`

Arquiva conteúdo.

### AppKids

`GET /api/kids/me/conteudos-aula`

Lista conteúdos aplicáveis ao responsável.

Filtros sugeridos:

- `criancaPessoaId`
- `limit`
- `somenteRecentes`

`GET /api/kids/me/conteudos-aula/{id}`

Detalhe do conteúdo.

`POST /api/kids/me/conteudos-aula/{id}/visualizado`

Marca como visualizado.

---

## DTOs sugeridos

### Admin

`CreateKidsConteudoAulaRequest`

- `titulo`
- `tema`
- `versiculo`
- `resumo`
- `atividadeEmCasa`
- `observacaoResponsavel`
- `dataReferencia`
- `eventoOcorrenciaId`
- `salaId`
- `turmaId`
- `anexos`

`UpdateKidsConteudoAulaRequest`

Mesmo shape, com atualização.

`KidsConteudoAulaAdminDto`

- dados principais
- anexos
- status
- publicado por
- publicado em

### App

`MeuConteudoAulaKidsResumoDto`

- `id`
- `titulo`
- `tema`
- `resumoCurto`
- `dataReferencia`
- `criancaPessoaIds`
- `criancaNomes`
- `salaId`
- `turmaId`
- `foiVisualizado`
- `temAnexos`

`MeuConteudoAulaKidsDetalheDto`

- `titulo`
- `tema`
- `versiculo`
- `resumo`
- `atividadeEmCasa`
- `observacaoResponsavel`
- `dataReferencia`
- `criancasRelacionadas`
- `anexos`

---

## Mudanças técnicas no backend

### Domínio

- criar `KidsConteudoAula`
- criar `KidsConteudoAulaAnexo`
- definir enums/constantes de status e tipo de anexo

### Infraestrutura

- mapping no `DbContext`
- índices por:
  - `TenantId`
  - `DataReferencia`
  - `Status`
  - `SalaId`
  - `TurmaId`
- repositórios
- migration

### Application

- service administrativo
- service de leitura do responsável
- integração opcional com push
- validação de escopo por sala/turma

### API

- controller admin
- endpoints `me/*`

---

## Mudanças técnicas no Admin

### Nova subárea

- `Kids > Conteúdo`

### Telas

- listagem
- criação/edição
- publicação
- arquivamento

### UI mínima do formulário

- título
- tema
- versículo
- resumo
- atividade para casa
- observação aos pais
- sala
- turma
- anexos

---

## Mudanças técnicas no AppKids

### Home

Adicionar card de conteúdo mais recente quando houver.

### Área dedicada

Nova tela:

- `Conteúdos`

### Telas

- feed de conteúdos
- detalhe do conteúdo
- visualização de anexos

---

## Push recomendado

Push deve ser complementar, com payload simples:

- `type: kids_content`
- `contentId`
- `titulo`
- `criancaPessoaId` opcional

Mensagem sugerida:

- “O conteúdo da aula de hoje já está disponível.”

---

## Auditoria e rastreabilidade

Registrar:

- quem criou
- quem publicou
- quem editou
- quando foi arquivado
- quando foi visualizado pelo responsável

---

## Testes recomendados

### Backend

- publicar conteúdo por sala
- publicar conteúdo por turma
- impedir turma fora da sala
- listar conteúdo correto para responsável
- ocultar conteúdo fora do escopo
- marcar conteúdo como visualizado

### Frontend / App

- listagem com conteúdo
- estado vazio
- abertura do detalhe
- renderização de anexo PDF/imagem/link

---

## Definition of Done

Considerar a fase concluída quando:

- o Admin publica conteúdo do dia
- o conteúdo aparece no `AppKids`
- anexos funcionam
- o responsável vê a publicação correta para suas crianças
- o histórico recente está acessível
- permissões e escopo estão consistentes
- testes mínimos da frente estão verdes
