# Documentos legais — VerboPlus

Documentos jurídicos da Plataforma, versionados. A **versão vigente é a `v1`**, que é exatamente a string que os formulários enviam no consentimento LGPD:

- Cadastro público de membro → `aceiteTermosVersao: "v1"` ([CadastroMembro/index.html](../CadastroMembro/index.html))
- Cadastro de criança (admin) → `consentimentoParentalVersao: "v1"` ([FrontEnd/src/pages/Kids](../FrontEnd/src/pages/Kids))

Esses valores são gravados na tabela `ConsentimentosRegistros` (campo `VersaoDocumento`), criando a trilha de consentimento. **Ao publicar uma nova versão dos documentos, incremente a versão (`v2`, ...) tanto aqui quanto nas constantes do frontend** (`TERMOS_VERSAO` / `CONSENTIMENTO_PARENTAL_VERSAO`).

## Documentos

| Arquivo | Conteúdo |
|---|---|
| [POLITICA_DE_PRIVACIDADE.md](./POLITICA_DE_PRIVACIDADE.md) | Aviso de privacidade LGPD (controlador/operador, dados, finalidades, direitos, crianças) |
| [TERMOS_DE_USO.md](./TERMOS_DE_USO.md) | Termos de uso da Plataforma (conta, planos, condutas, responsabilidades) |

## Pendências antes de publicar

1. **Revisão jurídica** — são modelos padrão; precisam de validação por advogado(a).
2. **Preencher os campos `[entre colchetes]`** — razão social, CNPJ, endereço, e-mail do Encarregado/DPO, foro, datas.
3. **Publicar e linkar** — hospedar os documentos (ex.: como rotas no Portal `/politica-de-privacidade` e `/termos-de-uso`, ou HTML estático) e substituir os links `href="#"` no formulário de cadastro de membro pelas URLs reais.
4. **DPA** — modelo de Acordo de Tratamento de Dados para os contratos com as Igrejas-clientes (ainda não criado).
