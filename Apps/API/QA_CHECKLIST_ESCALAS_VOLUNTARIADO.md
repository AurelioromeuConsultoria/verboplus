# QA Checklist - Escalas de Voluntariado

Tempo estimado: 10 a 15 minutos.

## 1) Preparacao

- [ ] API no ar.
- [ ] FrontEnd no ar.
- [ ] Worker no ar (para teste de automacao).
- [ ] Existe evento ativo e recorrente com recorrencia cadastrada.
- [ ] Existem equipes, cargos e voluntarios cadastrados.
- [ ] Existe ao menos 1 voluntario em 2 equipes (para teste de conflito).

## 2) Listagem e geracao de ocorrencias

- [ ] Acessar `voluntariado/escalas`.
- [ ] Filtrar por periodo valido.
- [ ] Selecionar evento recorrente.
- [ ] Clicar em **Gerar Ocorrencias**.
- [ ] Validar mensagem de sucesso com quantidade criada.
- [ ] Confirmar ocorrencias visiveis na lista.

## 3) Montagem de escala

- [ ] Abrir uma ocorrencia em **Montar Escala**.
- [ ] Adicionar voluntario na equipe A.
- [ ] Confirmar item criado.
- [ ] Adicionar outro voluntario sem conflito.
- [ ] Confirmar item criado.

## 4) Conflito por evento inteiro

- [ ] Tentar adicionar a mesma pessoa em equipe B na mesma ocorrencia.
- [ ] Validar bloqueio por conflito.
- [ ] Como admin, marcar excecao manual sem motivo.
- [ ] Validar obrigatoriedade do motivo.
- [ ] Informar motivo e salvar.
- [ ] Confirmar item salvo como **Excecao manual**.

## 5) Publicacao da escala

- [ ] Clicar em **Publicar Escala** com itens cadastrados.
- [ ] Validar mensagem de sucesso.
- [ ] Confirmar status **Publicada**.

## 6) Sugestoes inteligentes

- [ ] Selecionar uma equipe no editor.
- [ ] Confirmar lista de sugestoes carregada.
- [ ] Validar indicacao de disponibilidade/carga recente.
- [ ] Validar sinalizacao de voluntario com conflito.

## 7) Automacao do Worker

- [ ] Confirmar `EscalaScheduler.Enabled = true`.
- [ ] Ajustar `BaseIntervalMinutes` para valor curto (ex.: `1`) para teste.
- [ ] Validar logs do `EscalaSchedulerService` (inicio, processamento, total gerado).
- [ ] Confirmar novas ocorrencias na lista apos execucao do Worker.

## 8) Resultado final esperado

- [ ] Conflito bloqueia dupla alocacao da mesma pessoa na mesma ocorrencia.
- [ ] Excecao manual funciona apenas com motivo.
- [ ] Escala publica com status correto.
- [ ] Sugestoes inteligentes aparecem corretamente.
- [ ] Automacao gera ocorrencias recorrentes dentro da janela configurada.
