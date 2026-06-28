# Kids / AppKids - Checklist oficial de regressao

Use este checklist antes de fechar mudancas relevantes no modulo Kids.

Objetivo:

- validar o caminho feliz
- validar o caminho de bloqueio
- reduzir regressao invisivel em fluxos sensiveis

## 1. Login e sessao

- [ ] login com usuario valido funciona
- [ ] sessao autenticada permite acessar o fluxo esperado
- [ ] logout continua funcionando

## 2. Operacao administrativa atual

- [ ] listagem de criancas carrega
- [ ] detalhe de crianca carrega
- [ ] vinculo de responsavel continua funcionando
- [ ] alteracao de permissao de retirada continua funcionando

## 3. Check-in

- [ ] check-in de crianca valida funciona
- [ ] check-in duplicado e bloqueado
- [ ] codigo de sessao e gerado
- [ ] notificacao de check-in e criada

## 4. Check-out atual

- [ ] check-out de responsavel autorizado funciona
- [ ] codigo de sessao invalido e bloqueado
- [ ] check-out sem autorizacao e bloqueado
- [ ] notificacao de check-out e criada

## 5. Push basico

- [ ] registro de device token continua funcionando
- [ ] push de check-in nao quebra o fluxo se o envio falhar
- [ ] push de check-out nao quebra o fluxo se o envio falhar

## 6. Escopo e seguranca

- [ ] endpoint amplo de Kids continua protegido por autenticacao
- [ ] nao ha exposicao indevida do contexto do responsavel
- [ ] falha de autorizacao retorna mensagem coerente

## 7. Logs e observabilidade

- [ ] check-in gera log minimo util
- [ ] check-out gera log minimo util
- [ ] falha de autorizacao gera log de warning

## 8. Auditoria

- [ ] alteracoes sensiveis do modulo continuam deixando trilha minima

## 9. Observacoes desta fase

- o fluxo atual de checkout por QR ainda e temporario
- a retirada segura baseada em token sera a evolucao oficial da Sprint 4
