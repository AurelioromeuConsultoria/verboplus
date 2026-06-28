# Kids / AppKids - Ordem tecnica real de implementacao

Este documento traduz o plano por sprints em uma ordem pratica de implementacao no codigo.

Objetivo:

- dizer por onde comecar de verdade
- indicar a sequencia de arquivos e camadas a alterar
- reduzir retrabalho entre backend, frontend web e AppKids

Referencias:

- [KIDS_APPKIDS_SPRINTS.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_APPKIDS_SPRINTS.md)
- [KIDS_SPRINT1_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT1_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT2_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT2_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT3_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT3_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT4_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT4_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT5_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT5_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT6_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT6_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT7_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT7_EXECUCAO_TECNICA.md)
- [KIDS_SPRINT8_EXECUCAO_TECNICA.md](/Users/aurelioromeu/repos/AppIgreja/KIDS_SPRINT8_EXECUCAO_TECNICA.md)

## Principio geral de execucao

Para cada sprint de Kids, seguir sempre esta ordem:

1. fechar contrato e regra no backend
2. criar ou ajustar repositorios e services
3. expor controller
4. cobrir testes de regra e bloqueio
5. integrar frontend web ou AppKids
6. revisar logs, auditoria e regressao

Nao inverter essa ordem quando houver impacto de escopo ou seguranca.

## Ordem macro entre sprints

Executar nesta ordem:

1. Sprint 1 - Fundacao funcional e de seguranca
2. Sprint 2 - Contexto do responsavel e base do AppKids
3. Sprint 3 - Avisos reais
4. Sprint 4 - Retirada segura
5. Sprint 5 - Painel operacional do culto
6. Sprint 6 - Ocorrencias e historico
7. Sprint 7 - Sala, turma e capacidade
8. Sprint 8 - Consolidacao e indicadores

Motivo:

- cada sprint prepara contrato e contexto para a seguinte
- avisos dependem do contexto do responsavel
- retirada segura depende do AppKids e do escopo correto
- painel melhora muito depois que retirada e alertas estiverem definidos
- ocorrencias ficam mais uteis depois do painel
- sala e capacidade fecham a estrutura operacional

## Sprint 1 - Ordem tecnica real

### Objetivo tecnico

Fechar base de escopo e contratos sem ainda abrir nova superficie grande de produto.

### Sequencia sugerida

1. Revisar e documentar escopo de endpoints atuais de Kids

Arquivos centrais:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)
- [KidsService.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Services/KidsService.cs)

2. Preparar consultas de vinculo por responsavel

Arquivos centrais:

- [IResponsavelCriancaRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Interfaces/IResponsavelCriancaRepository.cs)
- [ResponsavelCriancaRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Repositories/ResponsavelCriancaRepository.cs)

3. Preparar leitura de usuario autenticado para resolver `PessoaId`

Arquivos centrais:

- [IUsuarioRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Interfaces/IUsuarioRepository.cs)
- [UsuarioRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Repositories/UsuarioRepository.cs)

4. Definir DTOs e contratos-alvo dos endpoints `me/*`

Arquivo central:

- [KidsDto.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs)

5. Registrar checklist de logs, auditoria e regressao

Arquivos de referencia:

- [CHECKLIST_ENTREGA_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/CHECKLIST_ENTREGA_QUALIDADE.md)
- [PLANEJAMENTO_PRODUTO_E_QUALIDADE.md](/Users/aurelioromeu/repos/AppIgreja/PLANEJAMENTO_PRODUTO_E_QUALIDADE.md)

### Implementacao recomendada

- nesta sprint, priorizar documentacao tecnica e pequenos ajustes de service
- ainda nao mexer pesado em AppKids

## Sprint 2 - Ordem tecnica real

### Objetivo tecnico

Entregar os endpoints `me/*` e migrar a home do AppKids para a experiencia do responsavel.

### Sequencia sugerida

1. Adicionar metodos de service do contexto do responsavel

Arquivo central:

- [KidsService.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Services/KidsService.cs)

Metodos sugeridos:

- `GetMinhasCriancasAsync`
- `GetMinhaCriancaByIdAsync`
- `GetMeusCheckinsAsync`

2. Adicionar ou ajustar consultas de repositrio

Arquivos centrais:

- [ResponsavelCriancaRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Repositories/ResponsavelCriancaRepository.cs)
- [KidsCheckinRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Repositories/KidsCheckinRepository.cs)

3. Criar DTOs especificos do responsavel

Arquivo central:

- [KidsDto.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs)

4. Expor endpoints no controller

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

5. Testar sucesso e bloqueio por vinculo

Local sugerido:

- `BackEnd/tests/.../Kids...Tests.cs`

6. Evoluir o AppKids repository

Arquivo central:

- [kids_repository.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/features/kids/kids_repository.dart)

7. Mudar home e navegacao do app

Arquivos centrais:

- [main.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/main.dart)
- [app_router.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/app_router.dart)
- nova `MinhasCriancasScreen`

8. Adicionar tela de detalhe da crianca

Arquivos novos sugeridos:

- `AppKids/lib/features/kids/minhas_criancas_screen.dart`
- `AppKids/lib/features/kids/minha_crianca_detalhe_screen.dart`

### Dependencia importante

- nao comecar aviso real antes desses endpoints estarem funcionando

## Sprint 3 - Ordem tecnica real

### Objetivo tecnico

Subir o feed de avisos com segmentacao e integracao ao AppKids.

### Sequencia sugerida

1. Evoluir modelo de notificacao

Arquivos centrais:

- [KidsNotificacao.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Domain/Entities/KidsNotificacao.cs)
- [SistemaIgrejaDbContext.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs)

2. Criar migracao de banco

Local:

- `BackEnd/src/SistemaIgreja.Infrastructure/Migrations`

3. Evoluir repositrio de notificacoes

Arquivos centrais:

- [IKidsNotificacaoRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Interfaces/IKidsNotificacaoRepository.cs)
- [KidsNotificacaoRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Repositories/KidsNotificacaoRepository.cs)

4. Criar servico dedicado de notificacoes, se `KidsService` estiver muito grande

Arquivo novo sugerido:

- `BackEnd/src/SistemaIgreja.Application/Services/KidsNotificacaoService.cs`

5. Expor endpoints administrativos e `me/avisos`

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

6. Integrar push complementar

Arquivos centrais:

- [IKidsPushNotificationService.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Interfaces/IKidsPushNotificationService.cs)
- implementacao concreta existente do push

7. Atualizar AppKids para feed real

Arquivos centrais:

- [avisos_screen.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/features/avisos/avisos_screen.dart)
- [push_service.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/core/push_service.dart)

8. Adicionar marcacao de lido e estados visuais

Arquivos centrais:

- `avisos_screen.dart`
- repositorio do app

## Sprint 4 - Ordem tecnica real

### Objetivo tecnico

Migrar checkout para retirada segura baseada em token temporario.

### Sequencia sugerida

1. Evoluir modelo de check-in com campos de retirada

Arquivos centrais:

- [KidsCheckin.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Domain/Entities/KidsCheckin.cs)
- [SistemaIgrejaDbContext.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs)

2. Criar migracao

Local:

- `BackEnd/src/SistemaIgreja.Infrastructure/Migrations`

3. Evoluir repositorio de check-in para busca por token

Arquivos centrais:

- [IKidsCheckinRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/Interfaces/IKidsCheckinRepository.cs)
- [KidsCheckinRepository.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Repositories/KidsCheckinRepository.cs)

4. Criar servico dedicado de retirada

Arquivo novo sugerido:

- `BackEnd/src/SistemaIgreja.Application/Services/KidsRetiradaService.cs`

5. Expor endpoints novos de retirada

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

6. Cobrir testes de token valido, invalido, expirado e excecao

Local sugerido:

- `BackEnd/tests/.../KidsRetirada...Tests.cs`

7. Atualizar AppKids para exibir token de retirada

Arquivos centrais:

- `minha_crianca_detalhe_screen.dart`
- [kids_repository.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/features/kids/kids_repository.dart)

8. Ajustar scanner e remover dependencia de QR improvisado

Arquivos centrais:

- [qr_scanner_screen.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/features/kids/qr_scanner_screen.dart)
- [checkin_checkout_screen.dart](/Users/aurelioromeu/repos/AppIgreja/AppKids/lib/features/kids/checkin_checkout_screen.dart)

### Dependencia importante

- nao subir painel operacional final antes do fluxo de retirada estar confiavel

## Sprint 5 - Ordem tecnica real

### Objetivo tecnico

Evoluir a tela web de Kids para painel operacional do culto.

### Sequencia sugerida

1. Criar DTOs de painel operacional

Arquivo central:

- [KidsDto.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs)

2. Criar metodo de service para painel

Arquivo sugerido:

- `KidsPainelService.cs` ou extensao de `KidsService.cs`

3. Expor endpoint `/api/kids/painel-operacional`

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

4. Testar montagem do painel

5. Evoluir tela web existente

Arquivo central:

- [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx)

6. Ajustar traducoes e labels

Arquivo central:

- [common.json](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/locales/pt-BR/common.json)

7. Se necessario, renomear item de menu depois de a tela estabilizar

Arquivo central:

- [Sidebar.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/components/Layout/Sidebar.jsx)

## Sprint 6 - Ordem tecnica real

### Objetivo tecnico

Adicionar ocorrencias de Kids e timeline por crianca.

### Sequencia sugerida

1. Criar entidade de ocorrencia de Kids

Arquivos novos sugeridos:

- `BackEnd/src/SistemaIgreja.Domain/Entities/KidsOcorrencia.cs`
- mapeamento no [SistemaIgrejaDbContext.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs)

2. Criar migracao

3. Criar repositrio

Arquivos novos sugeridos:

- `IKidsOcorrenciaRepository.cs`
- `KidsOcorrenciaRepository.cs`

4. Criar service dedicado

Arquivo novo sugerido:

- `KidsOcorrenciaService.cs`

5. Expor endpoints de ocorrencia

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

6. Integrar ocorrencias abertas ao painel

Arquivos centrais:

- service do painel
- [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx)

7. Criar UI de registro e timeline

Arquivos novos sugeridos:

- `FrontEnd/src/pages/Kids/KidsOcorrencias...jsx`
- ou drawer/modal dentro da tela atual

## Sprint 7 - Ordem tecnica real

### Objetivo tecnico

Modelar sala, turma e capacidade.

### Sequencia sugerida

1. Criar entidades `KidsSala` e `KidsTurma`

Arquivos novos sugeridos:

- `BackEnd/src/SistemaIgreja.Domain/Entities/KidsSala.cs`
- `BackEnd/src/SistemaIgreja.Domain/Entities/KidsTurma.cs`

2. Atualizar `CriancaDetalhe`

Arquivo central:

- [CriancaDetalhe.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Domain/Entities/CriancaDetalhe.cs)

3. Mapear no DbContext

Arquivo central:

- [SistemaIgrejaDbContext.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Infrastructure/Data/SistemaIgrejaDbContext.cs)

4. Criar migracao

5. Criar repositrios e services

Arquivos novos sugeridos:

- `IKidsSalaRepository.cs`
- `IKidsTurmaRepository.cs`
- `KidsSalaService.cs`
- `KidsTurmaService.cs`

6. Expor endpoints administrativos de sala e turma

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

7. Atualizar DTOs de crianca e painel

Arquivo central:

- [KidsDto.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs)

8. Evoluir painel web para lotacao

Arquivo central:

- [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx)

## Sprint 8 - Ordem tecnica real

### Objetivo tecnico

Consolidar resumo do modulo, refinamentos e criterio de pronto.

### Sequencia sugerida

1. Criar DTO de dashboard do modulo

Arquivo central:

- [KidsDto.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.Application/DTOs/KidsDto.cs)

2. Criar service de dashboard de Kids

Arquivo sugerido:

- `KidsDashboardService.cs`

3. Expor endpoint `/api/kids/dashboard`

Arquivo central:

- [KidsController.cs](/Users/aurelioromeu/repos/AppIgreja/BackEnd/src/SistemaIgreja.API/Controllers/KidsController.cs)

4. Adicionar cards consolidados no painel web

Arquivo central:

- [KidsCheckinsList.jsx](/Users/aurelioromeu/repos/AppIgreja/FrontEnd/src/pages/Kids/KidsCheckinsList.jsx)

5. Refinar AppKids

Arquivos centrais:

- home de `minhas criancas`
- detalhe da crianca
- feed de avisos

6. Consolidar checklist de regressao

Arquivo sugerido:

- `KIDS_CHECKLIST_REGRESSAO.md`

## Ordem de prioridade dentro do codigo

Se a equipe quiser a versao mais curta e pratica:

### Primeiro

- backend de Sprint 1
- backend de Sprint 2
- AppKids de Sprint 2

### Depois

- backend de Sprint 3
- AppKids de Sprint 3

### Depois

- backend de Sprint 4
- AppKids de Sprint 4

### Depois

- backend de Sprint 5
- frontend web de Sprint 5

### Depois

- backend e frontend de Sprint 6

### Depois

- backend e frontend de Sprint 7

### Por ultimo

- consolidacao da Sprint 8

## Recomendacao final de trabalho

Para reduzir risco e retrabalho:

- nao tentar desenvolver varias sprints de Kids em paralelo
- concluir backend e contrato da sprint antes de abrir integracao pesada no app
- tratar testes e logs como parte da sprint, nao como fechamento opcional
- usar o AppKids apenas depois de o backend do contexto do responsavel estar estavel
- usar o painel web como evolucao progressiva da tela ja existente, nao como tela paralela nova
