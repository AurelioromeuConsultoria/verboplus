# Kids / AppKids - Backlog de implementação do check-in no app

## CHECKIN-01

Criar a entidade `KidsPreCheckin` e sua configuração EF.

Arquivos principais:

- `BackEnd/src/SistemaIgreja.Domain/Entities`
- `BackEnd/src/SistemaIgreja.Infrastructure/Data`

Critério de aceite:

- tabela persistida com status, token, expiração e tenant

## CHECKIN-02

Criar repository/interface de `KidsPreCheckin`.

Arquivos principais:

- `BackEnd/src/SistemaIgreja.Application/Interfaces`
- `BackEnd/src/SistemaIgreja.Infrastructure/Repositories`

Critério de aceite:

- suporte a busca por `id`, `qrToken`, `codigoCurto`, `pendentes`, `ativos por criança/sessão`

## CHECKIN-03

Criar DTOs de pré-check-in.

Arquivos principais:

- `BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs`

Critério de aceite:

- contratos consistentes para app e Admin

## CHECKIN-04

Implementar `KidsPreCheckinService`.

Arquivos principais:

- `BackEnd/src/SistemaIgreja.Application/Services`

Critério de aceite:

- criação, listagem, validação, confirmação e cancelamento funcionando

## CHECKIN-05

Adicionar endpoints `me/*` de pré-check-in.

Arquivos principais:

- `BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs`

Critério de aceite:

- responsável autenticado consegue criar, listar e cancelar seus pré-check-ins

## CHECKIN-06

Adicionar endpoints administrativos de pré-check-in.

Arquivos principais:

- `BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs`

Critério de aceite:

- operação consegue listar, validar, confirmar e cancelar

## CHECKIN-07

Garantir regras de expiração e duplicidade.

Critério de aceite:

- não existe mais de um pré-check-in ativo por criança/sessão
- pendentes expirados não podem ser confirmados

## CHECKIN-08

Criar testes de backend da frente.

Arquivos principais:

- `BackEnd/tests/SistemaIgreja.API.Tests/Services`

Critério de aceite:

- suíte cobrindo cenários centrais

## CHECKIN-09

Adicionar API client do AppKids.

Arquivos principais:

- `AppKids/lib/features/kids/kids_repository.dart`

Critério de aceite:

- app consegue criar, listar e cancelar pré-check-in

## CHECKIN-10

Adicionar UX de check-in no detalhe da criança.

Arquivos principais:

- `AppKids/lib/features/kids/minha_crianca_detalhe_screen.dart`

Critério de aceite:

- botão `Iniciar check-in`
- visualização de QR/token temporário
- cancelamento

## CHECKIN-11

Adicionar resumo de estado na lista `Minhas crianças`.

Arquivos principais:

- `AppKids/lib/features/kids/minhas_criancas_screen.dart`

Critério de aceite:

- lista mostra estado de disponibilidade/pré-check-in/check-in

## CHECKIN-12

Adicionar lista de pré-check-ins pendentes no Admin.

Arquivos principais:

- `FrontEnd/src/pages/Kids/KidsCheckinsList.jsx`
- `FrontEnd/src/api/kids.js`

Critério de aceite:

- equipe enxerga pendentes

## CHECKIN-13

Adicionar confirmação operacional no Admin.

Critério de aceite:

- equipe confirma um pré-check-in e ele vira `KidsCheckin`

## CHECKIN-14

Adicionar cancelamento operacional no Admin.

Critério de aceite:

- equipe cancela pendente com registro de motivo

## CHECKIN-15

Revisar textos/UX da frente no app e no Admin.

Critério de aceite:

- estados humanizados
- sem textos técnicos expostos ao usuário
