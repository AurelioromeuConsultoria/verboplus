# Kids / AppKids

## Módulo: Conteúdo da Aula

### Objetivo

Transformar o `AppKids` em um canal de continuidade da experiência da criança além do culto, permitindo que responsáveis recebam:

- resumo do que foi ensinado
- materiais da aula
- atividades para casa
- lembretes pastorais/pedagógicos da turma

O objetivo não é virar um repositório solto de anexos, mas um módulo organizado de `conteúdo + contexto + acompanhamento`.

---

## Problema que o módulo resolve

Hoje, quando a aula termina:

- o responsável muitas vezes não sabe o que foi ensinado
- materiais acabam indo por grupos informais
- PDFs/imagens se perdem no WhatsApp
- a equipe não tem histórico organizado por turma/data

O módulo resolve isso criando uma trilha oficial, consultável e segmentada dentro do `AppKids`.

---

## Conceito principal

Cada aula pode gerar uma publicação de `Conteúdo do Dia`, vinculada a:

- data
- culto/sessão
- sala
- turma
- faixa/grupo atendido

Essa publicação pode conter:

- `tema da aula`
- `versículo`
- `resumo`
- `atividade em casa`
- `materiais anexos`
- `links complementares`
- `observação para os responsáveis`

---

## Estrutura funcional sugerida

### 1. Conteúdo do Dia

Publicação principal da aula, com foco no que a criança aprendeu.

Campos recomendados:

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
- `publicadoEm`
- `publicadoPorPessoaId`
- `status`

### 2. Materiais anexos

Arquivos ou links ligados ao conteúdo.

Tipos recomendados:

- `PDF`
- `imagem`
- `link`
- `arquivo complementar`

Campos:

- `tipo`
- `nomeExibicao`
- `urlOuPath`
- `ordem`
- `tamanho`
- `mimeType`

### 3. Segmentação

O conteúdo deve poder ser publicado para:

- `geral do Kids`
- `uma sala`
- `uma turma`
- `um conjunto de crianças`

Minha recomendação é começar por:

- geral
- sala
- turma

e deixar publicação por criança como etapa futura.

---

## Experiência no Admin

### Tela recomendada

Novo bloco dentro de `Kids`, preferencialmente como subárea:

- `Kids > Conteúdo`

### Funcionalidades principais

- criar conteúdo da aula
- anexar PDF/imagem/link
- escolher sala/turma
- publicar
- editar
- despublicar
- listar publicações recentes
- ver quantos responsáveis visualizaram

### Fluxo de publicação

1. líder/tia abre `Novo conteúdo`
2. preenche tema, versículo e resumo
3. anexa material
4. informa atividade em casa
5. escolhe destino
6. publica
7. responsáveis recebem push e veem no app

---

## Experiência no AppKids

### Nova área sugerida

- `Conteúdos`

ou, se quisermos algo ainda mais simples no início:

- card de `Conteúdo da aula` na home
- link para `Materiais`

### O que o responsável vê

#### Card principal

- tema da aula
- data
- turma/sala
- resumo curto

#### Blocos internos

- `Versículo`
- `O que aprenderam`
- `Atividade para casa`
- `Materiais`

#### Ações

- abrir PDF
- visualizar imagem
- abrir link
- marcar como lido
- baixar material quando fizer sentido

---

## Comportamentos importantes

### Feed persistente

Push não é a fonte de verdade.

O conteúdo precisa ficar salvo no app em uma área consultável com:

- hoje
- recentes
- filtro por criança

### Conteúdo associado à criança

Se o responsável tiver mais de uma criança:

- o conteúdo deve indicar claramente para qual criança/turma ele se aplica

### Histórico

Os responsáveis devem conseguir rever conteúdos anteriores.

Minha sugestão:

- últimos `30` ou `60` dias na primeira versão

---

## Regras de negócio

- somente equipe autorizada publica conteúdo
- conteúdo publicado precisa respeitar escopo de sala/turma
- conteúdo despublicado some do app, mas mantém rastreabilidade no Admin
- anexos devem ser seguros e auditáveis
- arquivos devem aceitar expiração controlada de URL se storage externo for usado
- o conteúdo precisa ser versionável o suficiente para edição sem duplicidade confusa

---

## Dados que eu exibiria no app logo no MVP

- `titulo`
- `tema`
- `versiculo`
- `resumo`
- `atividade em casa`
- `anexos`
- `data`
- `criança/turma relacionada`

---

## O que eu evitaria

- chat aberto entre pais e tias
- mural bagunçado sem segmentação
- upload sem título/tema/contexto
- depender de WhatsApp como fonte oficial
- deixar tudo como simples “anexo” sem resumo pedagógico

---

## Indicadores úteis

No Admin:

- quantos conteúdos publicados por semana
- quantos responsáveis visualizaram
- conteúdos mais acessados
- publicações por turma

No futuro:

- taxa de leitura por publicação
- taxa de abertura por turma

---

## Minha recomendação objetiva

A primeira versão deve focar em:

1. publicação de `Conteúdo do Dia`
2. anexos `PDF/imagem/link`
3. segmentação por `sala/turma`
4. visualização no `AppKids`
5. push complementar
6. histórico recente

Esse é o ponto em que o módulo já vira algo útil e claramente superior ao envio informal por WhatsApp.
