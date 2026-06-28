# Patrimonio - Plano estrategico e roteiro tecnico

## Decisao de posicionamento no Admin

A recomendacao para o contexto atual do AppIgreja e criar a secao **Patrimonio** dentro do menu **Financeiro**.

Motivos:

- O Admin ja concentra em `Financeiro` os cadastros de suporte operacional, como fornecedores, centros de custo, projetos, despesas e receitas.
- Patrimonio tem relacao direta com aquisicao, valor do bem, fornecedor, projeto e centro de custo.
- Isso reduz dispersao no menu e acelera a entrega do MVP.

Estrutura sugerida no menu:

- Financeiro
- Patrimonio
- Cadastro de bens
- Categorias de patrimonio
- Relatorios de patrimonio

Observacao:

- Se o modulo crescer para inventario fisico, manutencao preventiva, transferencias entre unidades e depreciacao, ele pode virar um grupo proprio no futuro.

## Objetivo do modulo

Permitir que a igreja cadastre, acompanhe e consulte todos os bens sob sua responsabilidade, sabendo:

- o que e o bem;
- onde ele esta;
- quem responde por ele;
- quanto custou;
- em que estado se encontra;
- se esta em uso, emprestado, em manutencao ou baixado.

## Escopo recomendado do MVP

O MVP deve resolver o problema principal de controle patrimonial sem adicionar complexidade contabil cedo demais.

Entregas do MVP:

1. Cadastro de bens patrimoniais.
2. Listagem com busca e filtros.
3. Historico basico de movimentacoes.
4. Upload de foto e comprovante.
5. Vinculos opcionais com fornecedor, centro de custo, projeto e despesa.
6. Relatorio simples por categoria, local e status.

Fica fora do MVP:

- depreciacao contabil automatica;
- inventario por leitor de codigo de barras/QR;
- manutencao preventiva com agenda automatica;
- workflow complexo de aprovacao;
- multiplas unidades com transferencia em lote.

## Estrutura funcional do modulo

### 1. Cadastro de bens

Cada bem deve poder ser cadastrado individualmente ou em grupo, dependendo do nivel de controle desejado.

Regra pratica:

- Itens de maior valor ou rastreabilidade: cadastro unitario com codigo unico.
- Itens de consumo controlado ou grande volume: cadastro com quantidade.

Exemplos:

- Cadeira plastica: pode ser um item com quantidade.
- Notebook, projetor, mesa de som, camera, instrumento musical, veiculo: idealmente um item por unidade.

### 2. Consulta operacional

A listagem principal deve responder rapidamente:

- quantos bens existem;
- onde cada bem esta;
- quais estao em manutencao;
- quais foram emprestados;
- quais estao sem responsavel definido;
- qual o valor total dos bens.

### 3. Movimentacoes

Mesmo no MVP, vale registrar movimentacoes manuais para manter rastreabilidade.

Tipos iniciais:

- cadastro inicial;
- transferencia de local;
- troca de responsavel;
- envio para manutencao;
- retorno de manutencao;
- emprestimo;
- devolucao;
- baixa.

### 4. Relatorios

Relatorios iniciais:

- bens por categoria;
- bens por local;
- bens por status;
- bens por ministerio;
- valor total por categoria;
- bens em manutencao;
- bens baixados.

## Campos mais uteis para o cadastro de patrimonio

### Identificacao do bem

- Codigo patrimonial ou tombo
- Nome do bem
- Categoria
- Subcategoria opcional
- Descricao detalhada
- Marca
- Modelo
- Numero de serie
- Quantidade
- Unidade de controle opcional

### Localizacao e responsabilidade

- Campus/unidade
- Ministerio ou area responsavel
- Sala/localizacao
- Responsavel pelo bem
- Observacao de localizacao

### Dados de aquisicao

- Tipo de aquisicao: comprado, doado, fabricado, recebido em cessao
- Data de aquisicao
- Valor de aquisicao
- Fornecedor
- Numero da nota fiscal
- Despesa vinculada
- Centro de custo vinculado
- Projeto vinculado

### Estado do bem

- Status: em uso, em manutencao, emprestado, ocioso, baixado
- Estado de conservacao: novo, bom, regular, ruim, inutilizavel
- Data da ultima avaliacao
- Observacoes gerais

### Manutencao e garantia

- Possui garantia
- Garantia ate
- Data da ultima manutencao
- Proxima manutencao prevista
- Custo acumulado de manutencao opcional

### Midia e anexos

- Foto principal
- Fotos adicionais opcionais
- Documento fiscal
- Comprovante
- Manual ou anexo tecnico opcional

## Categorias iniciais sugeridas

Para acelerar a adocao, vale iniciar com categorias pre-cadastradas:

- Moveis
- Cadeiras e mesas
- Instrumentos musicais
- Equipamentos de audio
- Equipamentos de video
- Iluminacao
- Informatica
- Eletrodomesticos
- Veiculos
- Material infantil
- Equipamentos de limpeza
- Utensilios gerais
- Patrimonio administrativo

## Estrutura de telas no FrontEnd

### 1. Tela de listagem

Rota sugerida:

- `/financeiro/patrimonio`

Componentes principais:

- campo de busca por nome, codigo ou numero de serie;
- filtros por categoria, local, status, ministerio, responsavel e fornecedor;
- cards resumo no topo;
- tabela com acoes de visualizar, editar e excluir.

Colunas sugeridas:

- Codigo
- Nome
- Categoria
- Local
- Responsavel
- Status
- Estado
- Valor
- Ultima atualizacao

Cards resumo:

- total de bens;
- valor total estimado;
- em manutencao;
- emprestados;
- baixados.

### 2. Tela de cadastro/edicao

Rotas sugeridas:

- `/financeiro/patrimonio/novo`
- `/financeiro/patrimonio/:id/editar`

Blocos sugeridos na UI:

- Identificacao
- Localizacao e responsabilidade
- Dados de aquisicao
- Estado e manutencao
- Anexos e observacoes

### 3. Tela de detalhe

Rota sugerida:

- `/financeiro/patrimonio/:id`

Exibir:

- dados completos do bem;
- historico de movimentacoes;
- anexos;
- vinculos financeiros;
- linha do tempo basica.

### 4. Tela de categorias

Rota sugerida:

- `/financeiro/patrimonio/categorias`

Uso:

- cadastrar e manter categorias personalizadas sem depender de codigo.

## Modelo de dados sugerido para o BackEnd

### Entidades principais

#### PatrimonioItem

Campos sugeridos:

- `Id`
- `Codigo`
- `Nome`
- `Descricao`
- `CategoriaPatrimonioId`
- `Marca`
- `Modelo`
- `NumeroSerie`
- `Quantidade`
- `Campus`
- `Localizacao`
- `MinisterioArea`
- `ResponsavelPessoaId`
- `TipoAquisicao`
- `DataAquisicao`
- `ValorAquisicao`
- `FornecedorId`
- `NumeroNotaFiscal`
- `DespesaId`
- `CentroCustoId`
- `ProjetoId`
- `Status`
- `EstadoConservacao`
- `DataUltimaAvaliacao`
- `PossuiGarantia`
- `GarantiaAte`
- `DataUltimaManutencao`
- `DataProximaManutencao`
- `FotoUrl`
- `DocumentoUrl`
- `Observacoes`
- `Ativo`
- `CreatedAt`
- `UpdatedAt`

#### PatrimonioCategoria

Campos sugeridos:

- `Id`
- `Nome`
- `Descricao`
- `Ativo`
- `CreatedAt`
- `UpdatedAt`

#### PatrimonioMovimentacao

Campos sugeridos:

- `Id`
- `PatrimonioItemId`
- `TipoMovimentacao`
- `DataMovimentacao`
- `Origem`
- `Destino`
- `ResponsavelOrigem`
- `ResponsavelDestino`
- `Observacoes`
- `UsuarioId`
- `CreatedAt`

## Enumeracoes sugeridas

### TipoAquisicao

- Comprado
- Doado
- Fabricado
- Cedido

### StatusPatrimonio

- EmUso
- EmManutencao
- Emprestado
- Ocioso
- Baixado

### EstadoConservacao

- Novo
- Bom
- Regular
- Ruim
- Inutilizavel

### TipoMovimentacaoPatrimonio

- CadastroInicial
- TransferenciaLocal
- TrocaResponsavel
- ManutencaoEnvio
- ManutencaoRetorno
- Emprestimo
- Devolucao
- Baixa

## Endpoints sugeridos

### Patrimonio

- `GET /api/patrimonio`
- `GET /api/patrimonio/{id}`
- `POST /api/patrimonio`
- `PUT /api/patrimonio/{id}`
- `DELETE /api/patrimonio/{id}`

Filtros importantes em `GET /api/patrimonio`:

- `search`
- `categoriaId`
- `status`
- `responsavelPessoaId`
- `ministerio`
- `localizacao`
- `fornecedorId`
- `somenteAtivos`

### Categorias de patrimonio

- `GET /api/patrimonio/categorias`
- `GET /api/patrimonio/categorias/{id}`
- `POST /api/patrimonio/categorias`
- `PUT /api/patrimonio/categorias/{id}`
- `DELETE /api/patrimonio/categorias/{id}`

### Movimentacoes

- `GET /api/patrimonio/{id}/movimentacoes`
- `POST /api/patrimonio/{id}/movimentacoes`

### Relatorios

- `GET /api/patrimonio/relatorios/resumo`
- `GET /api/patrimonio/relatorios/por-categoria`
- `GET /api/patrimonio/relatorios/por-local`
- `GET /api/patrimonio/relatorios/por-status`

## Permissoes sugeridas

Opcao de curto prazo:

- reutilizar `RESOURCES.FINANCEIRO` para o MVP.

Opcao recomendada para consolidacao:

- criar `RESOURCES.PATRIMONIO`.

Permissoes desejadas:

- visualizar patrimonio;
- cadastrar/editar patrimonio;
- excluir patrimonio;
- visualizar relatorios de patrimonio.

## Integracoes com o que ja existe no sistema

O modulo ganha muito valor se nascer conectado a estruturas existentes:

- **Fornecedor**: origem da compra.
- **Despesa**: lancamento financeiro associado a aquisicao.
- **Centro de Custo**: classificacao administrativa.
- **Projeto**: vinculo com campanhas ou iniciativas especificas.
- **Pessoa**: responsavel pelo bem.
- **Upload**: foto e documentos.
- **Auditoria**: rastreabilidade de alteracoes.

## Ordem recomendada de implementacao

### Fase 1 - Base do modulo

Objetivo:

- colocar o modulo no ar com cadastro funcional.

Entregas:

1. Entidades `PatrimonioItem` e `PatrimonioCategoria`.
2. Migrations e repositorios.
3. DTOs e services.
4. Controller de patrimonio e categorias.
5. Rotas no FrontEnd.
6. Menu `Financeiro > Patrimonio`.
7. Tela de listagem.
8. Tela de cadastro/edicao.
9. Upload de foto/comprovante.

Criterios de aceite:

- cadastrar bem com campos principais;
- editar e excluir;
- listar com busca e filtros principais;
- vincular fornecedor, projeto e centro de custo de forma opcional.

### Fase 2 - Rastreabilidade

Objetivo:

- garantir historico e contexto operacional.

Entregas:

1. Entidade `PatrimonioMovimentacao`.
2. Registro automatico de `CadastroInicial`.
3. Cadastro manual de movimentacoes.
4. Tela de detalhe do patrimonio.
5. Linha do tempo de movimentacoes.

Criterios de aceite:

- visualizar historico do bem;
- registrar transferencia, manutencao e baixa;
- saber quem foi o ultimo responsavel.

### Fase 3 - Relatorios e maturidade

Objetivo:

- transformar o modulo em ferramenta de gestao.

Entregas:

1. Dashboard/resumo patrimonial.
2. Relatorios por categoria, local e status.
3. Alertas de garantia e manutencao.
4. Campo de proxima manutencao com destaque visual.

Criterios de aceite:

- consultar valor total do patrimonio;
- visualizar bens em manutencao;
- identificar bens com garantia vencendo ou manutencao pendente.

## Backlog tecnico sugerido

### BackEnd

1. Criar entidades em `SistemaIgreja.Domain/Entities`.
2. Adicionar `DbSet`s no `SistemaIgrejaDbContext`.
3. Criar configuracoes EF se necessario.
4. Gerar migration.
5. Criar repositorios.
6. Criar DTOs.
7. Criar services de patrimonio e categorias.
8. Criar controllers.
9. Registrar servicos em `Program.cs`.
10. Cobrir casos basicos com testes.

### FrontEnd

1. Criar APIs em `FrontEnd/src/api` ou `FrontEnd/src/lib/api`.
2. Criar paginas:
   - `FrontEnd/src/pages/Patrimonio/PatrimoniosList.jsx`
   - `FrontEnd/src/pages/Patrimonio/PatrimonioForm.jsx`
   - `FrontEnd/src/pages/Patrimonio/PatrimonioDetails.jsx`
   - `FrontEnd/src/pages/Patrimonio/CategoriasPatrimonioList.jsx`
   - `FrontEnd/src/pages/Patrimonio/CategoriaPatrimonioForm.jsx`
3. Adicionar rotas em `FrontEnd/src/App.jsx`.
4. Adicionar item no menu em `FrontEnd/src/components/Layout/Sidebar.jsx`.
5. Adicionar textos de traducao em `FrontEnd/src/locales/pt-BR/common.json`.
6. Adicionar permissao dedicada se decidido.

## Sugestao de backlog funcional

### Sprint 1

- CRUD de categorias
- CRUD de bens
- Listagem com filtros
- Menu e permissao

### Sprint 2

- Tela de detalhe
- Historico de movimentacoes
- Upload de anexos
- Relatorio resumo

### Sprint 3

- Alertas de manutencao
- Alertas de garantia
- Baixa patrimonial com motivo
- Refinos de UX

## Decisoes de produto recomendadas

### O que simplificar agora

- Nao implementar depreciacao no MVP.
- Nao exigir campos financeiros em todos os bens.
- Nao obrigar numero de serie.
- Nao obrigar fornecedor para bens antigos ou doados.

### O que vale exigir desde o inicio

- Nome do bem
- Categoria
- Status
- Localizacao
- Responsavel ou area responsavel

### O que melhora muito a operacao

- Codigo patrimonial unico
- Foto do item
- Historico de movimentacao
- Filtros por local e status

## Criterios de sucesso do modulo

O modulo sera considerado bem sucedido quando permitir:

- descobrir rapidamente onde cada bem esta;
- saber quem responde por cada item;
- localizar itens em manutencao ou emprestados;
- consultar valor patrimonial por categoria;
- reduzir perdas, esquecimento e desorganizacao.

## Recomendacao final

O melhor caminho e iniciar com um **MVP forte e simples**, dentro de `Financeiro`, priorizando:

1. cadastro de bens;
2. listagem com filtros;
3. categorias;
4. historico basico;
5. relatorio resumo.

Essa abordagem entrega valor real sem travar o projeto em regras contabeis mais complexas.
