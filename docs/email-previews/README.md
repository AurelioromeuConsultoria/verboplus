# Previews de e-mail (mockups de design)

Estes arquivos são **mockups estáticos** dos e-mails transacionais do Verbo+, feitos para
visualizar o design no navegador (`open 01-verificacao-email.html`). Os valores são fixos
(nome "Marco", token `demo`, etc.) — não há dados dinâmicos.

> ⚠️ **Não são os templates usados em produção.** O HTML que o Worker realmente envia é
> montado em código em
> [`BackEnd/src/SistemaIgreja.Infrastructure/Services/EmailTemplates.cs`](../../BackEnd/src/SistemaIgreja.Infrastructure/Services/EmailTemplates.cs).
> Editar um arquivo aqui **não** altera o e-mail enviado, e vice-versa. Ao mudar o design,
> atualize os dois lados.

## Arquivos

| Arquivo | E-mail |
|---|---|
| `01-verificacao-email.html` | Verificação de e-mail (boas-vindas / confirmar conta) |
| `02-trial-acabando.html`    | Aviso de trial acabando |
| `03-assinatura-suspensa.html` | Assinatura suspensa |
| `04-pagamento-pendente.html`  | Pagamento pendente |
