# Escalas de Voluntariado - Guia Funcional e Roteiro de Homologacao

## Visao geral da funcionalidade

Hoje o modulo de **Escalas de Voluntariado por evento/ocorrencia** cobre o fluxo abaixo:

- Listar ocorrencias de eventos/cultos por periodo e por evento.
- Gerar ocorrencias recorrentes manualmente (UI/API).
- Montar escala por ocorrencia.
- Criar/publicar escala e gerenciar itens (adicionar/remover voluntario, equipe, cargo, ordem).
- Bloquear conflito de alocacao da mesma pessoa no mesmo evento/ocorrencia.
- Permitir excecao manual de conflito para admin (com motivo obrigatorio).
- Sugerir voluntarios por equipe com disponibilidade e carga recente.
- Gerar ocorrencias recorrentes automaticamente via Worker (EscalaScheduler).

## O que da para fazer hoje

### Frontend

- Tela de listagem e filtros: `voluntariado/escalas`
  - Filtrar por evento e periodo.
  - Visualizar status da ocorrencia e se ja existe escala.
  - Acionar geracao manual de ocorrencias.
- Tela de edicao/montagem: `voluntariado/escalas/ocorrencia/:ocorrenciaId`
  - Criar escala para ocorrencia.
  - Adicionar voluntario com equipe/cargo/ordem.
  - Aplicar excecao manual (admin).
  - Remover itens.
  - Publicar escala.

### API (principais endpoints)

- Ocorrencias:
  - `GET /api/EventosOcorrencias/periodo`
  - `GET /api/EventosOcorrencias/evento/{eventoId}`
  - `POST /api/EventosOcorrencias/gerar-recorrencia`
- Escalas:
  - `GET /api/Escalas/ocorrencia/{eventoOcorrenciaId}`
  - `POST /api/Escalas`
  - `POST /api/Escalas/{escalaId}/itens`
  - `DELETE /api/Escalas/{escalaId}/itens/{escalaItemId}`
  - `GET /api/Escalas/{escalaId}/sugestoes?equipeId={id}`
  - `POST /api/Escalas/{escalaId}/publicar`

### Worker (automacao)

- Servico: `EscalaSchedulerService`.
- Configuracao em `SistemaIgreja.BackgroundWorker/appsettings.json`, secao `EscalaScheduler`:
  - `Enabled`
  - `BaseIntervalMinutes`
  - `JitterSecondsMax`
  - `DiasJanelaInicio`
  - `DiasJanelaFim`
- Com `Enabled = true`, o Worker:
  - busca eventos ativos e recorrentes;
  - pre-gera ocorrencias dentro da janela configurada;
  - evita duplicidade pela validacao de horario.

## Regras de negocio implementadas

- Conflito por evento inteiro:
  - a mesma pessoa nao pode ser escalada em mais de uma equipe na mesma ocorrencia.
- Excecao manual:
  - disponivel para admin;
  - exige motivo;
  - fica marcada no item da escala.
- Publicacao:
  - escala precisa existir e ter itens para ser publicada.

## Roteiro de homologacao (checklist rapido)

Tempo estimado: 10 a 15 minutos.

### 1) Preparacao de dados

- [ ] Garantir evento ativo e recorrente com recorrencia configurada.
- [ ] Garantir equipes, cargos e voluntarios cadastrados.
- [ ] Garantir ao menos 1 voluntario pertencendo a 2 equipes (para testar conflito real).
- [ ] Subir API, Frontend e (para automacao) Worker.

### 2) Homologacao da listagem e geracao manual

- [ ] Acessar `voluntariado/escalas`.
- [ ] Definir periodo com datas que incluam o culto/evento.
- [ ] Selecionar um evento recorrente.
- [ ] Clicar em **Gerar Ocorrencias**.
- [ ] Validar retorno com quantidade criada e recarregamento da lista.
- [ ] Confirmar ocorrencias exibidas com data/hora e status.

### 3) Homologacao da montagem da escala

- [ ] Abrir uma ocorrencia em **Montar Escala**.
- [ ] Adicionar voluntario na equipe A.
- [ ] Validar item criado na grade.
- [ ] Adicionar outro voluntario sem conflito.
- [ ] Validar sucesso.

### 4) Homologacao da regra de conflito

- [ ] Tentar adicionar a mesma pessoa em equipe B na mesma ocorrencia.
- [ ] Validar bloqueio com mensagem de conflito.
- [ ] Como admin, marcar **Forcar alocacao com conflito** sem motivo.
- [ ] Validar que sistema exige motivo.
- [ ] Informar motivo e salvar.
- [ ] Validar item salvo e marcado como **Excecao manual**.

### 5) Homologacao da publicacao

- [ ] Com itens na escala, clicar em **Publicar Escala**.
- [ ] Validar mensagem de sucesso.
- [ ] Validar status atualizado para **Publicada**.

### 6) Homologacao das sugestoes inteligentes

- [ ] No editor, selecionar equipe.
- [ ] Verificar voluntarios sugeridos com indicacao de disponibilidade/carga recente.
- [ ] Confirmar que voluntarios ja conflitando aparecem sinalizados/indisponiveis.

### 7) Homologacao da automacao no Worker

- [ ] Confirmar `EscalaScheduler.Enabled = true`.
- [ ] Ajustar intervalo curto para teste (ex.: `BaseIntervalMinutes = 1`).
- [ ] Subir Worker e acompanhar logs do `EscalaSchedulerService`.
- [ ] Validar logs de:
  - [ ] inicializacao do scheduler;
  - [ ] eventos processados;
  - [ ] total de ocorrencias geradas.
- [ ] Voltar na tela de escalas e confirmar novas ocorrencias no periodo da janela.

## Criterios de aceite

- [ ] Nao permite dupla alocacao da mesma pessoa no mesmo evento/ocorrencia sem excecao.
- [ ] Permite excecao apenas com permissao adequada e motivo.
- [ ] Escala publica com status consistente.
- [ ] Sugestoes refletem disponibilidade e carga recente.
- [ ] Worker gera ocorrencias automaticamente conforme configuracao.

## Observacoes

- Se o banco ja estiver atualizado, `dotnet ef database update` nao aplica nada (comportamento esperado).
- Pode aparecer warning do EF sobre default de enum em `Evento.Tipo`; nao bloqueia execucao do fluxo.
