# DOMAIN_KNOWLEDGE.md

> **Referência oficial do domínio de negócio do projeto AppIgreja / VerboPlus (Verbo+).**
>
> Este documento descreve **o negócio que o sistema representa** — não a arquitetura nem a implementação técnica (essas estão em [ARCHITECTURE.md](ARCHITECTURE.md), [.claude/PROJECT_CONTEXT.md](PROJECT_CONTEXT.md), [.claude/CODING_STANDARDS.md](CODING_STANDARDS.md), [.claude/INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md) e [.claude/MIGRATION_RULES.md](MIGRATION_RULES.md)).
>
> Regras deste documento:
> - **Nada é inventado.** Todo conceito vem do código, das entidades, dos serviços, das integrações e da documentação existentes.
> - **Conceitos com mais de um significado** estão documentados com suas diferenças.
> - Onde não há evidência suficiente: `TODO: confirmar com o time`.
> - Evidências citam a entidade/serviço de origem (`Domain/Entities/*.cs`, `Application/Services/*.cs`, `Infrastructure/Services/*.cs`).
> - Domínio em **Português** (convenção forte do projeto). Termos técnicos em Inglês quando o código usa Inglês.
> - Última análise: **2026-06-27**.

---

## Visão Geral do Negócio

### Problema que o sistema resolve
Plataforma de **gestão de igrejas** (ChMS — *Church Management System*) comercializada como **SaaS multi-tenant** sob a marca **Verbo+ (VerboPlus)**, focada no mercado brasileiro. Centraliza a operação administrativa de uma igreja em um único sistema, isolando os dados de cada igreja (tenant) e cobrando por assinatura mensal/anual.

A igreja de origem do produto é a **"Igreja Kingdom"** (domínios `kingdombr.com.br` ainda em uso em produção). O tenant inicial (`Tenant.InitialTenantId = 1`) é tratado como **tenant raiz imutável** (`IsRootTenant`), que não pode ser deletado nem desativado (`Tenant.cs`, `TenantManagementService`).

### Quem são os usuários
- **Administradores da igreja** — operam o painel administrativo (Verbo+) para gerir pessoas, voluntários, escalas, eventos, financeiro, patrimônio, comunicação, portal e Kids.
- **Líderes de equipe/ministério** — gerenciam escalas, conteúdos e avisos do seu escopo.
- **Operadores do ministério infantil (Kids)** — realizam check-in/checkout de crianças.
- **Responsáveis (pais/cuidadores)** — usam o **AppKids** (Flutter) para pré-check-in, acompanhar a criança e receber notificações.
- **Membros e visitantes** — recebem comunicação, se inscrevem em eventos e fazem doações pelo Portal público.
- **Visitantes do site público (Portal)** — consomem notícias, destaques, galerias, enquetes, contatos e doações.
- **Administrador da plataforma (`IsPlatformAdmin`)** — superusuário da VerboPlus que opera acima dos tenants (cria/gerencia igrejas, bypassa permissões e gating de assinatura).

### Áreas de negócio atendidas (módulos)
| Área | O que cobre |
|---|---|
| **Pessoas / Membros / Visitantes** | Cadastro centralizado de indivíduos; classificação por tipo e perfil; histórico de visitas. |
| **Voluntariado e Escalas** | Equipes, cargos, escalas por evento, modelos de escala, indisponibilidades, trocas. |
| **Eventos** | Eventos/cultos/reuniões, recorrências, ocorrências, inscrições públicas com formulário dinâmico. |
| **Kids (ministério infantil)** | Salas, turmas, pré-check-in, check-in, retirada segura por token/PIN, ocorrências, conteúdo de aula, avisos e push. |
| **Financeiro** | Receitas, despesas, categorias, centros de custo, contas, fornecedores, orçamento anual, doações online. |
| **Patrimônio** | Bens, categorias, movimentações (transferência, manutenção, empréstimo, baixa). |
| **Comunicação omnichannel** | Templates, campanhas, segmentos, automações, entregas multi-canal, preferências/opt-out. |
| **Portal público / Site** | Configuração do site, destaques, notícias, galerias, enquetes, contatos, projetos, células (HubCasa). |
| **SaaS / Plataforma / Billing** | Tenants, planos, assinaturas, faturas, trial/inadimplência/suspensão, signup self-service, verificação de e-mail. |
| **Segurança / LGPD** | RBAC, auditoria, consentimento versionado, solicitações de titular de dados. |

### Processos automatizados (por evidência de schedulers e serviços)
- **Ciclo de assinatura**: trial → inadimplência → suspensão (`BillingCycleService`, `BillingSchedulerService`).
- **Geração de ocorrências de eventos recorrentes** (a partir de `EventoRecorrencia`).
- **Envio agendado de mensagens** e **automações de comunicação** (novo visitante, aniversário, lembrete operacional) — `MessageSchedulerService`, `BirthdayCampaignSchedulerService`, `EscalaScheduler`.
- **Notificações push automáticas** no Kids (check-in, checkout, alerta de exceção).
- **Confirmação automática de doação** via webhook Asaas, com criação automática de receita financeira.

---

## Glossário do Domínio

> Apenas conceitos **existentes no projeto**. Cada termo aponta a evidência de origem.

| Termo | Definição | Finalidade | Relacionamentos | Evidência |
|---|---|---|---|---|
| **Tenant** | A igreja cliente da plataforma; raiz do isolamento multi-tenant. | Isolar dados de cada igreja no mesmo banco. | Possui assinatura, faturas, pessoas, usuários, domínios. | `Tenant.cs`, `TenantManagementService` |
| **Verbo+ / VerboPlus** | Marca comercial do produto SaaS. | Identidade do produto. | É a **Operadora** LGPD; a igreja é a **Controladora**. | `PROJECT_CONTEXT.md` |
| **Plataforma** | O SaaS em si, acima dos tenants. | Operação global (admin de plataforma). | `IsPlatformAdmin` no JWT. | `Usuario.cs`, `PermissionMiddleware` |
| **Pessoa** | Registro central e unificado de qualquer indivíduo (adulto ou criança). | Hub de dados de contato e classificação. | Origina Visitante, Voluntário, Usuário, Criança, Responsável. | `Pessoa.cs`, `PessoaService` |
| **Visitante** | Registro de uma visita de uma pessoa à igreja. | Histórico de presenças/visitas. | N visitas por Pessoa; dispara automação de comunicação. | `Visitante.cs`, `VisitanteService` |
| **Membro** | Pessoa com perfil de negócio `Membro` ativo. | Vínculo formal com a igreja. | Modelado como `PerfilPessoa.Membro`. | `PessoaPerfil.cs` |
| **Perfil de Pessoa** | Papel temporal de uma Pessoa (Visitante, Membro, Voluntário, Líder, Kids, Admin). | Rastrear quando alguém assumiu/deixou um papel. | 1 Pessoa : N perfis ao longo do tempo. | `PessoaPerfil.cs`, `PerfilPessoa` (enum) |
| **Usuário** | Credencial de acesso ao sistema (login + senha). | Autenticação e autorização. | 1:1 com Pessoa; 0..1 com Perfil de Acesso. | `Usuario.cs`, `UsuarioService` |
| **Perfil de Acesso** | Conjunto de permissões RBAC (recurso × ação). | Controlar o que cada usuário pode fazer. | 1 perfil : N permissões; N usuários. | `PerfilAcesso.cs` |
| **Equipe** | Time de voluntários (louvor, som, recepção…). | Agrupar voluntários por ministério. | Tem Área, Líder, voluntários, escalas. | `Equipe.cs` |
| **Cargo** | Função dentro de uma equipe (ex.: Guitarrista). | Categorizar voluntários e vagas de escala. | N voluntários; usado em itens de modelo/escala. | `Cargo.cs` |
| **Voluntário** | Vínculo de uma Pessoa a uma Equipe em um Cargo. | Definir quem serve, onde e em que função. | N indisponibilidades; aparece em escalas. | `Voluntario.cs` |
| **Escala** | Plano de voluntários de uma equipe para uma ocorrência de evento. | Organizar quem serve em cada culto/evento. | N itens (EscalaItem); pertence a Equipe + Ocorrência. | `Escala.cs` |
| **Modelo de Escala** | Template que define quantas pessoas (por cargo) uma equipe precisa. | Gerar escalas automaticamente. | N itens de modelo; por equipe e/ou evento. | `EscalaModelo.cs` |
| **Indisponibilidade** | Data em que um voluntário não está disponível. | Evitar escalá-lo; justificar recusa. | N por voluntário. | `IndisponibilidadeVoluntario.cs` |
| **Solicitação de Troca de Escala** | Pedido de um voluntário para sair/trocar uma escala. | Permitir substituição com aprovação do líder. | 1 por item de escala (pendente). | `SolicitacaoTrocaEscala.cs` |
| **Evento** | Acontecimento da igreja (evento, culto, reunião, outro). | Organizar a agenda e aceitar inscrições. | N ocorrências, N recorrências, N inscrições. | `Evento.cs` |
| **Ocorrência de Evento** | Instância específica de um evento (data/hora). | Vincular escalas e check-ins a uma data. | Pertence a Evento; tem N escalas. | `EventoOcorrencia.cs` |
| **Recorrência de Evento** | Regra de repetição (semanal/quinzenal/mensal). | Gerar ocorrências automaticamente. | Gera N ocorrências. | `EventoRecorrencia.cs` |
| **Inscrição** | Participação de uma pessoa em um evento, via formulário. | Captar e gerir participantes. | N por evento. | `InscricaoEvento.cs` |
| **Sala (Kids)** | Sala física/lógica do ministério infantil (nível superior). | Agrupar crianças por faixa. | Contém N turmas. | `KidsSala.cs` |
| **Turma (Kids)** | Subdivisão de uma sala. | Agrupar crianças pedagogicamente. | Pertence a uma sala. | `KidsTurma.cs` |
| **Criança (CriancaDetalhe)** | Extensão de Pessoa com dados do Kids (alergias, restrições, sala/turma). | Cuidado seguro da criança. | 1:1 com Pessoa; N responsáveis. | `CriancaDetalhe.cs` |
| **Responsável** | Adulto vinculado a uma criança, com permissão de retirada. | Autorizar quem pode buscar a criança. | N:N entre Pessoas (criança × responsável). | `ResponsavelCrianca.cs` |
| **Check-in / Check-out (Kids)** | Registro de entrada e saída da criança no ministério. | Controlar presença e retirada segura. | N por criança; gera notificações. | `KidsCheckin.cs` |
| **Pré-check-in** | Autorização prévia feita pelo responsável (app), confirmada pelo operador. | Check-in seguro em 2 passos. | 0..1 vínculo com check-in. | `KidsPreCheckin.cs` |
| **Retirada Segura** | Checkout validado por QR token ou PIN. | Garantir que só responsável autorizado retire. | Parte do KidsCheckin. | `KidsRetiradaService` |
| **Ocorrência (Kids)** | Incidente registrado durante a sessão (queda, febre, comportamento). | Rastrear e comunicar incidentes. | Pertence a criança/check-in. | `KidsOcorrencia.cs` |
| **Conteúdo de Aula (Kids)** | Material pedagógico publicado aos responsáveis. | Compartilhar conteúdo da aula. | N anexos (PDF/Imagem/Link). | `KidsConteudoAula.cs` |
| **Receita** | Entrada financeira (dízimo, oferta, doação, aluguel…). | Controle de entradas. | Categoria, conta, centro de custo, projeto. | `Receita.cs` |
| **Despesa** | Saída/obrigação financeira com vencimento. | Controle de saídas e fluxo de caixa. | Fornecedor, categoria, conta, centro de custo. | `Despesa.cs` |
| **Centro de Custo** | Agrupamento de receitas/despesas por departamento/ministério. | Alocar custos por área. | N receitas/despesas/patrimônios. | `CentroCusto.cs` |
| **Conta Bancária** | Conta onde receitas entram e despesas saem. | Controle de saldo. | N receitas/despesas. | `ContaBancaria.cs` |
| **Fornecedor** | Cadastro de quem presta serviço/vende à igreja. | Vincular a despesas/patrimônio. | N despesas, N itens. | `Fornecedor.cs` |
| **Orçamento por Categoria** | Valor previsto por categoria/ano (receita ou despesa). | Comparar planejado × realizado. | Categoria de receita/despesa. | `OrcamentoCategoria.cs` |
| **Projeto** | Iniciativa da igreja com orçamento e prazo. | Agrupar transações financeiras por iniciativa. | N receitas/despesas. | `Projeto.cs` |
| **Doação Online** | Doação recebida via Portal (PIX/Cartão/Boleto). | Captação online com confirmação automática. | Vinculada a Finalidade e a uma Receita gerada. | `DoacaoOnline.cs` |
| **Finalidade de Doação** | Campanha/propósito de doação exibida no Portal. | Direcionar doações a um fim. | N doações; liga a categoria/conta/centro/projeto. | `FinalidadeDoacao.cs` |
| **Patrimônio (Bem)** | Bem móvel da igreja (equipamento, móvel, instrumento…). | Inventário e controle de bens. | Categoria, responsável, N movimentações. | `PatrimonioItem.cs` |
| **Movimentação de Patrimônio** | Histórico de transferência/manutenção/empréstimo/baixa de um bem. | Auditoria do ciclo de vida do bem. | N por bem; altera status do bem. | `PatrimonioMovimentacao.cs` |
| **Campanha (Comunicação)** | Envio coordenado de mensagens a um público por um ou mais canais. | Comunicar em massa ou por automação. | N canais, N entregas. | `Comunicacao.cs`, `ComunicacaoCampanhaService` |
| **Entrega (Comunicação)** | Estado de uma mensagem individual (pessoa × canal). | Rastrear envio/falha/bloqueio. | N por campanha. | `ComunicacaoEntregaService` |
| **Template (Comunicação)** | Modelo reutilizável de mensagem com variáveis. | Padronizar conteúdo. | Usado por canais de campanha. | `ComunicacaoTemplateService` |
| **Segmento** | Público reutilizável resolvido por regra (visitantes, membros…). | Definir audiência. | Usado por campanhas. | `ComunicacaoSegmentoService` |
| **Preferência (Comunicação)** | Consentimento/bloqueio de um indivíduo por canal (opt-in/opt-out). | Respeitar privacidade/consentimento. | Por Pessoa × Canal. | `ComunicacaoPreferenciaService` |
| **Mensagem Agendada** | Item enfileirado (tabela) a ser enviado pelo scheduler. | "Fila" de envio sem broker. | Processada por estado. | `MensagemAgendada.cs` |
| **Plano** | Catálogo global de assinatura (preço, limites). | Definir o que a igreja contrata. | N assinaturas. | `Plano.cs` |
| **Assinatura** | Contrato de uma igreja com um plano (trial/ativa/…). | Receita recorrente do SaaS. | Pertence a Tenant + Plano; gera faturas. | `Assinatura.cs` |
| **Fatura** | Cobrança individual de uma assinatura. | Histórico de pagamentos. | N por assinatura. | `Fatura.cs` |
| **Consentimento (LGPD)** | Aceite versionado de documento (Política/Termos/Parental). | Conformidade LGPD. | Por Pessoa (e quem concedeu). | `ConsentimentoRegistro.cs` |
| **Solicitação de Titular (LGPD)** | Requisição de direito do titular (acesso, exportação, eliminação…). | Atender LGPD Art. 18/19 com prazo legal. | Por Tenant/Pessoa. | `SolicitacaoTitular.cs` |
| **Log de Auditoria** | Trilha de ações no sistema. | Compliance e rastreabilidade. | Por Tenant. | `AuditLog.cs` |
| **Célula / HubCasa** | Pequeno grupo descentralizado da igreja. | Organizar células com anfitrião/líder/timóteo. | Anfitrião, Líder, Timóteo (usuários). | `HubCasa.cs` |

---

## Entidades Principais

> Propósito, atributos de negócio, ciclo de vida/estados e relacionamentos. Atributos puramente técnicos (Id, TenantId, timestamps) são omitidos salvo quando têm significado de negócio.

### Pessoa
- **Propósito:** registro central e unificado de qualquer indivíduo (adultos e crianças). É o **hub** do domínio.
- **Atributos de negócio:** `Nome`, `Email`, `Telefone`, `WhatsApp`, `FotoUrl`, `DataNascimento`, `TipoPessoa` (Adulto/Criança), `Ativo`.
- **Ciclo de vida:** criada com `Ativo = true`; pode ser ativada/desativada; `TipoPessoa` pode ser resolvido automaticamente na edição própria.
- **Estados:** `TipoPessoa` = `Adulto(1)` / `Crianca(2)`; `Ativo` = true/false.
- **Relacionamentos:** 1:0..1 Usuário; 1:N Visitante; 1:N Voluntário; 1:N PessoaPerfil; origem de Criança/Responsável.
- **Evidência:** `Pessoa.cs`, `PessoaService`, `TipoPessoa.cs`, `PerfilPessoa.cs`.

### Usuario
- **Propósito:** credencial de acesso. Cada Pessoa tem no máximo um Usuário.
- **Atributos de negócio:** `EmailLogin` (pode diferir de `Pessoa.Email`), `TipoUsuario`, `IsPlatformAdmin`, `Ativo`, `PerfilAcessoId`, `TentativasLoginFalhas`, `BloqueadoAte`, `UltimoAcesso`.
- **Ciclo de vida:** criado ativo (exceto no signup, onde nasce inativo até verificação de e-mail); pode ser bloqueado temporariamente (`BloqueadoAte`) ou desativado (`Ativo=false`).
- **Estados:** `TipoUsuario` = `Admin(1)` / `Portal(2)` / `Ambos(3)`; `Ativo` true/false; bloqueio temporário via `BloqueadoAte`.
- **Relacionamentos:** 1:1 Pessoa; 0..1 PerfilAcesso; 0..1 Equipe (como líder); N notificações.
- **Evidência:** `Usuario.cs`, `UsuarioService`, `TipoUsuario.cs`, `AuthService`.

### Escala / EscalaItem
- **Propósito:** organizar quem serve em cada ocorrência de evento, por equipe.
- **Estados da Escala:** `Rascunho(1)` → `Publicada(2)` → `Fechada(3)`. Publicar dispara convites; fechar ocorre após o evento.
- **Estados do EscalaItem:** `Pendente(1)`, `Confirmado(2)`, `Recusado(3)`, `Substituido(4)`, `Serviu(5)`, `Faltou(6)`.
- **Atributos de negócio do item:** `PessoaId` (âncora histórica — preservada mesmo se o voluntário for desvinculado), `VoluntarioId` (pode virar null), `CargoId`, `Ordem`, `ConflitoAprovado` + `MotivoExcecao` + `AprovadoPorUsuarioId`, datas de convite/confirmação/recusa, marcos de lembrete (`DataLembrete7DiasEnviado`, `DataLembrete24HorasEnviado`), `ObservacaoOperacional`.
- **Relacionamentos:** Escala pertence a Equipe + EventoOcorrencia; tem N itens; item pode ter 1 Solicitação de Troca.
- **Evidência:** `Escala.cs`, `EscalaItem.cs`, `EscalaService`.

### Evento / EventoOcorrencia / EventoRecorrencia / InscricaoEvento
- **Evento — estados:** `Tipo` = `Evento(1)`/`Culto(2)`/`Reuniao(3)`/`Outro(4)`; `Ativo`; `AceitaInscricoes` (default false); `EhRecorrente`. `ConfiguracaoFormularioInscricao` é JSON com campos dinâmicos.
- **EventoOcorrencia — estados:** `Confirmado(1)`, `Cancelado(2)`, `Realizado(3)`. Validação de unicidade de horário.
- **EventoRecorrencia:** `Periodicidade` = `Semanal(1)`/`Quinzenal(2)`/`Mensal(3)`, com `DiaSemana`, `HoraInicio/Fim`, vigência (`DataInicioVigencia`/`DataFimVigencia`), `Ativo`.
- **InscricaoEvento — estados:** `Pendente(1)`, `Confirmada(2)`, `Cancelada(3)`, `Presente(4)`. Campos fixos Nome/WhatsApp (obrigatórios) + Email/Observações; demais campos em JSON `DadosInscricao`; `QuantidadeAcompanhantes`.
- **Evidência:** `Evento.cs`, `EventoOcorrencia.cs`, `EventoRecorrencia.cs`, `InscricaoEvento.cs`, `StatusInscricao.cs`.

### KidsCheckin (e o fluxo de Retirada Segura)
- **Propósito:** controlar presença da criança e garantir retirada apenas por responsável autorizado.
- **Estados:** `Status` = `"CheckedIn"` → `"CheckedOut"` (unidirecional). `Metodo` de check-in = `"ADMIN"`/`"QR"`/`"PIN"`/`"PRECHECKIN"`. `RetiradaMetodo` = `"QR"`/`"PIN"`/`"EXCECAO"`.
- **Atributos de negócio:** `CodigoSessao`, `TokenRetirada` (QR, hex 40 bytes), `PinRetirada` (6 dígitos), `TokenRetiradaExpiraEm` (check-in + 8h), `RetiradaEmModoExcecao` + `RetiradaMotivoExcecao` + `RetiradaPessoaNome`/`RetiradaPessoaDocumento`.
- **Regras-chave:** criança deve estar ativa e do tipo Criança; não pode haver check-in já aberto; **token QR expira em 8h, PIN não expira** enquanto `CheckedIn`; só responsável `Ativo` e `PodeRetirar=true` pode retirar; modo exceção exige nome de quem retirou e dispara **alerta** aos responsáveis.
- **Evidência:** `KidsCheckin.cs`, `KidsService`, `KidsRetiradaService`.

### KidsPreCheckin
- **Estados:** `"Pending"` → `"Confirmed"` | `"Expired"` | `"Cancelled"`. `Confirmed` é terminal (não pode ser cancelado).
- **Atributos:** `QrToken` (80 hex), `CodigoCurto` (8 chars de alfabeto sem ambiguidade `ABCDEFGHJKLMNPQRSTUVWXYZ23456789`), `ExpiraEm` (~10 minutos), `ObservacoesResponsavel`.
- **Regras:** responsável deve ser ativo e vinculado; criança cadastrada no Kids e sem check-in aberto; pré-check-in ativo para a mesma criança+evento é **idempotente** (retorna o existente); só **Operador** confirma.
- **Evidência:** `KidsPreCheckin.cs`, `KidsPreCheckinService`.

### Assinatura (SaaS)
- **Propósito:** contrato da igreja com um plano; núcleo da receita recorrente.
- **Estados:** `Trial(1)` → `Ativa(2)` / `Inadimplente(3)` → `Suspensa(4)`; `Cancelada(5)`.
- **Atributos de negócio:** `PlanoId`, `Ciclo` (Mensal/Anual), `Valor` (cópia do preço no momento da contratação), `MetodoPagamento` (Pix/Boleto/Cartao), `TrialFim`, `TrialAvisoEnviadoEm`, `VigenciaInicio`, `ProximaCobranca`, `InadimplenteDesde`, `SuspensaEm`, `CanceladaEm`, `GatewayCustomerId`/`GatewaySubscriptionId` (Asaas).
- **Evidência:** `Assinatura.cs`, `BillingService`, `BillingCycleService`.

### Fatura (SaaS)
- **Estados:** `Pendente(1)`, `Paga(2)`, `Vencida(3)`, `Falhou(4)`, `Cancelada(5)`.
- **Atributos:** `Valor`, `Vencimento`, `PagaEm`, `GatewayPaymentId`, `LinkPagamento`, `PixCopiaECola`.
- **Evidência:** `Fatura.cs`, `BillingService`.

### DoacaoOnline
- **Estados:** `Pendente(1)`, `AguardandoPagamento(2)`, `Confirmada(3)`, `Expirada(4)`, `Cancelada(5)`, `Falhou(6)`, `Estornada(7)`. `MetodoPagamento` = `Pix(1)`/`CartaoCredito(2)`/`Boleto(3)`.
- **Atributos:** `NomeDoador`, `Documento` (obrigatório p/ Pix), `Anonima`, `Valor`, `ReciboToken` (acesso ao recibo), `PixCopiaECola`, `PixQrCodeUrl`, `DataVencimento`, `DataConfirmacao`, `ReceitaId` (receita gerada na confirmação).
- **Evidência:** `DoacaoOnline.cs`, `DoacoesService`, `AsaasPaymentService`.

### PatrimonioItem
- **Estados (`Status` string):** `"EmUso"` (default), `"EmManutencao"`, `"Emprestado"`, `"Baixado"`. **`EstadoConservacao`:** `"Bom"`/`"Regular"`/`"Ruim"`/`"Irrecuperavel"`.
- **Atributos de negócio:** `Codigo` (único), `Marca`/`Modelo`/`NumeroSerie`, `Quantidade`, `Campus`/`Localizacao`/`MinisterioArea`, `ResponsavelPessoaId`, `TipoAquisicao` ("Comprado"/"Doado"/"Herdado"), `ValorAquisicao`, garantia (`PossuiGarantia`/`GarantiaAte`), manutenção (`DataUltimaManutencao`/`DataProximaManutencao`).
- **Evidência:** `PatrimonioItem.cs`, `PatrimonioItemService`, `PatrimonioMovimentacaoService`.

---

## Relacionamentos de Negócio

### Pessoa como hub
`Pessoa` é o centro do domínio. A partir dela derivam/vinculam-se:
- **Usuario** (1:1) — credencial de acesso.
- **Visitante** (1:N) — cada visita.
- **Voluntario** (1:N) — vínculo a Equipe + Cargo.
- **PessoaPerfil** (1:N) — papéis temporais.
- **CriancaDetalhe** (1:1, quando criança) e **ResponsavelCrianca** (N:N entre pessoas).
- Receitas (como contribuinte), patrimônio (como responsável), consentimentos e solicitações de titular.

### Cardinalidades observadas (principais)
- **Tenant 1 : N** Pessoa, Usuario, Assinatura, Fatura, AuditLog, ConsentimentoRegistro, SolicitacaoTitular, TenantDomain.
- **Plano 1 : N** Assinatura (Plano é **global**, não isolado por tenant).
- **Assinatura 1 : N** Fatura.
- **Equipe 1 : N** Voluntario; **Equipe 1 : N** Escala; **Equipe 0..1** Usuario (líder).
- **Voluntario 1 : N** IndisponibilidadeVoluntario.
- **Evento 1 : N** EventoOcorrencia, EventoRecorrencia, InscricaoEvento, EscalaModelo.
- **EventoOcorrencia 1 : N** Escala (uma por equipe).
- **Escala 1 : N** EscalaItem; **EscalaItem 1 : 0..1** SolicitacaoTrocaEscala.
- **KidsSala 1 : N** KidsTurma; **CriancaDetalhe N : N** ResponsavelCrianca (via vínculo); **Criança 1 : N** KidsCheckin.
- **Campanha 1 : N** Canal e Entrega; **Pessoa × Canal 1 : 1** Preferência.
- **FinalidadeDoacao 1 : N** DoacaoOnline; **DoacaoOnline 1 : 0..1** Receita (gerada na confirmação).
- **PatrimonioItem 1 : N** PatrimonioMovimentacao.

### Regras de relacionamento observadas
- **Vínculo único de voluntário:** a combinação `(Pessoa, Equipe, Cargo)` deve ser única (`VoluntarioService`).
- **Perfil único ativo:** não pode haver dois registros do mesmo `PerfilPessoa` com `DataFim = null` para a mesma Pessoa (`PessoaPerfilService`).
- **Âncora histórica em escala:** `EscalaItem.PessoaId` é sempre preenchido para preservar o histórico mesmo se o voluntário for desvinculado.
- **Substituto válido:** ao aprovar troca de escala, o substituto deve ser da **mesma equipe**, diferente do solicitante e sem conflito no mesmo evento.

---

## Fluxos de Negócio

### 1. Cadastro de pessoa / visitante
1. Visitante chega → busca-se Pessoa existente por **Email → WhatsApp → Telefone** (deduplicação).
2. Se não existir, cria-se Pessoa; reutilizando, apenas **completa campos vazios** (não sobrescreve).
3. Garante perfil `PerfilPessoa.Visitante` ativo e registra `Visitante` (data da visita).
4. **Dispara automação de comunicação** de novo visitante.
- **Evidência:** `VisitanteService`, `PessoaPerfilService`, `ComunicacaoAutomacaoService`.

### 2. Voluntariado e escala
1. Cadastra-se Equipe (com Área e opcionalmente Líder) e Cargos.
2. Vincula-se Pessoa como Voluntário (Equipe + Cargo), com `MaxEscalasPorMes` opcional.
3. Define-se `EscalaModelo` (vagas por cargo) — específico do evento ou padrão da equipe.
4. Cria-se Escala (`Rascunho`), manual ou por geração automática a partir do modelo (respeitando indisponibilidades e `DiasFolgaAposEscala`).
5. **Publicar** → itens viram `Pendente` com convite; voluntários **Confirmam** ou **Recusam**.
6. Lembretes automáticos em **7 dias** e **24 horas** antes.
7. Após o evento, registra-se **presença** (`Serviu`/`Faltou`).
- **Troca:** voluntário abre `SolicitacaoTrocaEscala` (sugere substituto) → líder **Aprova** (item original vira `Substituido`, novo item `Pendente`) ou **Rejeita**.
- **Evidência:** `EscalaService`, `EscalaModeloService`, `SolicitacaoTrocaEscalaService`.

### 3. Eventos e inscrições
1. Cria-se Evento (define `AceitaInscricoes` e formulário dinâmico).
2. Recorrências geram Ocorrências automaticamente; ocorrências também podem ser manuais.
3. Inscrição pública via formulário → `Pendente` (deduplicação por WhatsApp no mesmo evento).
4. Admin **Confirma**, **Cancela** ou marca **Presente**.
- **Evidência:** `EventoService`, `InscricaoEventoService`.

### 4. Kids — pré-check-in → check-in → retirada segura
1. **Pré-check-in (app do responsável):** gera `QrToken` + `CodigoCurto`, expira em ~10 min, status `Pending`.
2. **Confirmação (operador no local):** valida QR/código → cria `KidsCheckin` (`CheckedIn`, método `PRECHECKIN`); pré-check-in vira `Confirmed`. Pode também haver check-in direto por `ADMIN`/`QR`/`PIN`.
3. Check-in gera `TokenRetirada` (QR, expira em 8h) e `PinRetirada` (6 dígitos, sem expiração) e **notifica todos os responsáveis ativos** (push).
4. Durante a sessão, podem ser registradas **Ocorrências** (queda, febre…).
5. **Retirada segura:** responsável `Ativo` + `PodeRetirar=true` valida por QR ou PIN → `CheckedOut`; notifica responsáveis.
6. **Modo exceção:** retirada por pessoa não autorizada → exige nome/documento, marca `RetiradaEmModoExcecao`, dispara **ALERTA**.
- **Evidência:** `KidsPreCheckinService`, `KidsService`, `KidsRetiradaService`, `KidsOcorrenciaService`.

### 5. Comunicação (campanha e automações)
1. Define-se Segmento (público) e Template; cria-se Campanha (manual ou automática) com 1+ canais e prioridade (fallback).
2. Resolve-se a audiência → gera **Entregas** (`Pendente`); preferências de **opt-out** geram `IgnoradoPorPreferencia`.
3. Scheduler **reserva** lote (`Reservado`), envia e marca `Enviado`/`Entregue` ou `Falhou`.
4. **Automações:** novo visitante (D+N após visita), aniversário (diário), lembrete operacional (sob demanda, idempotente por `ChaveEvento`).
- **Evidência:** `ComunicacaoCampanhaService`, `ComunicacaoEntregaService`, `ComunicacaoAutomacaoService`, `MessageSchedulerService`, `BirthdayCampaignSchedulerService`.

### 6. Doação online (PIX) → receita
1. Doador escolhe Finalidade e preenche dados → cria `DoacaoOnline` (`Pendente`).
2. Para Pix: cria cobrança no Asaas → `AguardandoPagamento` (QR + copia-e-cola + vencimento).
3. Webhook Asaas `PAYMENT_RECEIVED`/`PAYMENT_CONFIRMED` → `Confirmada` e **cria automaticamente uma Receita** (categoria/conta/centro/projeto herdados da Finalidade).
4. Recibo disponível por `ReciboToken` apenas se `Confirmada`.
- **Evidência:** `DoacoesService`, `AsaasPaymentService`.

### 7. Faturamento / ciclo de assinatura (SaaS)
1. **Signup self-service:** valida e-mail único global + senha; resolve plano (padrão `organizacao`); gera slug do tenant; provisiona tenant + admin **inativos**; registra consentimentos; cria assinatura em **Trial** (e cliente/assinatura no Asaas com `BillingType=UNDEFINED`); envia e-mail de verificação.
2. **Verificação de e-mail** (token válido por 48h) → ativa tenant e usuário admin.
3. **Ciclo automático:** aviso de trial (3 dias antes) → `Trial`→`Inadimplente` no vencimento → `Inadimplente`→`Suspensa` após **carência (7 dias)**.
4. **Pagamento confirmado** (webhook) → `Ativa`, define `ProximaCobranca`.
5. **Gating HTTP 402:** assinatura `Suspensa` (ou `Cancelada` sem período pago) bloqueia a API; isenta `/api/auth`, `/api/upload`, `/api/webhooks`, `/api/billing`; platform admin nunca é bloqueado; **fail-open** quando não há assinatura.
- **Evidência:** `SignupService`, `BillingService`, `BillingCycleService`, `SubscriptionGatingMiddleware`, `BillingSettings`.

### 8. LGPD — solicitação de titular
1. Titular registra solicitação → `Aberta`, com `PrazoLimite = solicitação + 15 dias`.
2. Admin **Atende** (`EmAtendimento`) → **Conclui** (`Concluida`) ou **Recusa** (`Recusada`), com observação/resultado.
- **Evidência:** `SolicitacaoTitular.cs`, `SolicitacaoTitularService`.

---

## Estados e Transições

> Valores **literais** extraídos das entidades/serviços. Enums numéricos mostram o inteiro; status em string mostram a literal.

### Pessoas / Acesso
| Entidade | Campo | Valores |
|---|---|---|
| Pessoa | `TipoPessoa` | `Adulto(1)`, `Crianca(2)` |
| PessoaPerfil | `PerfilPessoa` | `Visitante(1)`, `Membro(2)`, `Voluntario(3)`, `Lider(4)`, `Kids(5)`, `Admin(6)` — **ativo se `DataFim == null`** |
| Usuario | `TipoUsuario` | `Admin(1)`, `Portal(2)`, `Ambos(3)` |
| Equipe | `Area` (`AreaEquipe`) | `Verde(1)`, `Vermelha(2)`, `Laranja(3)` |

### Voluntariado / Escalas
| Entidade | Campo | Valores | Transições |
|---|---|---|---|
| Escala | `Status` | `Rascunho(1)`, `Publicada(2)`, `Fechada(3)` | Rascunho → Publicada (envia convites) → Fechada |
| EscalaItem | `Status` | `Pendente(1)`, `Confirmado(2)`, `Recusado(3)`, `Substituido(4)`, `Serviu(5)`, `Faltou(6)` | Pendente → Confirmado/Recusado; Confirmado → Serviu/Faltou; → Substituido (via troca) |
| SolicitacaoTrocaEscala | `Status` | `Pendente(1)`, `Aprovada(2)`, `Rejeitada(3)`, `Cancelada(4)` | Pendente → Aprovada/Rejeitada |

### Eventos
| Entidade | Campo | Valores | Transições |
|---|---|---|---|
| Evento | `Tipo` (`TipoEvento`) | `Evento(1)`, `Culto(2)`, `Reuniao(3)`, `Outro(4)` | — |
| EventoOcorrencia | `Status` | `Confirmado(1)`, `Cancelado(2)`, `Realizado(3)` | Confirmado → Cancelado/Realizado |
| EventoRecorrencia | `Periodicidade` | `Semanal(1)`, `Quinzenal(2)`, `Mensal(3)` | — |
| InscricaoEvento | `Status` (`StatusInscricao`) | `Pendente(1)`, `Confirmada(2)`, `Cancelada(3)`, `Presente(4)` | Pendente → Confirmada/Cancelada → Presente |

### Kids
| Entidade | Campo | Valores | Transições |
|---|---|---|---|
| KidsCheckin | `Status` | `"CheckedIn"`, `"CheckedOut"` | CheckedIn → CheckedOut (irreversível) |
| KidsCheckin | `Metodo` | `"ADMIN"`, `"QR"`, `"PIN"`, `"PRECHECKIN"` | — |
| KidsCheckin | `RetiradaMetodo` | `"QR"`, `"PIN"`, `"EXCECAO"` | — |
| KidsPreCheckin | `Status` | `"Pending"`, `"Confirmed"`, `"Expired"`, `"Cancelled"` | Pending → Confirmed (terminal) / Expired / Cancelled |
| KidsOcorrencia | `Status` | `"Aberta"`, `"Encerrada"` | Aberta → Encerrada |
| KidsNotificacao | `Status` | `"Enviado"`, `"Falhou"` | — |
| KidsNotificacao | `Tipo` | `"CHECKIN"`, `"CHECKOUT"`, `"ALERTA"`, `"AVISO_GERAL"` (e destino `GERAL`/`CRIANCA`/`RESPONSAVEL`) | — |
| KidsNotificacao | `Origem` | `"AUTOMATICA"`, `"MANUAL"` | — |
| KidsConteudoAula | `Status` | `"Draft"`, `"Published"`, `"Archived"` | Draft → Published; Archived → Draft ao editar |
| KidsConteudoAulaAnexo | `Tipo` | `"Pdf"`, `"Imagem"`, `"Link"` | — |
| KidsDeviceToken | `Platform` | `"Android"`, `"iOS"` | — |

### Financeiro / Patrimônio / Doações
| Entidade | Campo | Valores | Transições |
|---|---|---|---|
| Receita | `Status` (`StatusReceita`) | `Pendente(1)`, `Recebida(2)`, `Cancelada(3)` | criada Pendente; lote e doação confirmada → Recebida |
| Despesa | `Status` (`StatusDespesa`) | `Pendente(1)`, `Paga(2)`, `Cancelada(3)` | Pendente → Paga/Cancelada |
| Receita/Despesa | `TipoRecorrencia` | `Semanal(1)`, `Quinzenal(2)`, `Mensal(3)`, `Bimestral(4)`, `Trimestral(5)`, `Semestral(6)`, `Anual(7)` | — |
| OrcamentoCategoria | `Tipo` (`TipoOrcamento`) | `Receita(1)`, `Despesa(2)` | — |
| PatrimonioItem | `Status` | `"EmUso"`, `"EmManutencao"`, `"Emprestado"`, `"Baixado"` | via movimentações |
| PatrimonioItem | `EstadoConservacao` | `"Bom"`, `"Regular"`, `"Ruim"`, `"Irrecuperavel"` | — |
| PatrimonioMovimentacao | `TipoMovimentacao` | `CadastroInicial`, `TransferenciaLocal`, `ManutencaoEnvio`, `ManutencaoRetorno`, `Emprestimo`, `Devolucao`, `Baixa` | aplica mudança de status no bem |
| DoacaoOnline | `Status` | `Pendente(1)`, `AguardandoPagamento(2)`, `Confirmada(3)`, `Expirada(4)`, `Cancelada(5)`, `Falhou(6)`, `Estornada(7)` | Pendente → AguardandoPagamento → Confirmada/Expirada/Falhou; Confirmada → Estornada |
| DoacaoOnline | `MetodoPagamento` | `Pix(1)`, `CartaoCredito(2)`, `Boleto(3)` | — |
| GivingProviderConfig | `Provider` / `Environment` | `Asaas(1)` / `Sandbox(1)`, `Production(2)` | — |

### Comunicação
| Entidade | Campo | Valores | Transições |
|---|---|---|---|
| ComunicacaoTemplate | `Status` | `Rascunho(1)`, `Ativo(2)`, `Arquivado(3)` | — |
| ComunicacaoCampanha | `Status` | `Rascunho(1)`, `Agendada(2)`, `Processando(3)`, `Concluida(4)`, `ConcluidaComFalhas(5)`, `Cancelada(6)` | Rascunho/Agendada → Processando → Concluida/ConcluidaComFalhas; → Cancelada |
| ComunicacaoCampanha | `Origem` | `Manual(1)`, `Automatica(2)` | — |
| ComunicacaoEntrega | `Status` | `Pendente(1)`, `Reservado(2)`, `Enviado(3)`, `Entregue(4)`, `Falhou(5)`, `Cancelado(6)`, `IgnoradoPorPreferencia(7)` | Pendente → Reservado → Enviado/Entregue ou Falhou; pode voltar a Pendente (reprocesso) |
| ComunicacaoPreferencia | `Status` | `Permitido(1)`, `Bloqueado(2)` | — |
| CanalComunicacao | (canais) | `WhatsApp(1)`, `Email(2)`, `Push(3)`, `NotificacaoInterna(4)` | — |
| EnvioCampanhaAniversario | `Status` | `Pendente`, `EmProcessamento`, `Enviado`, `Erro` | Pendente → EmProcessamento → Enviado/Erro |

### SaaS / LGPD
| Entidade | Campo | Valores | Transições |
|---|---|---|---|
| Assinatura | `Status` (`StatusAssinatura`) | `Trial(1)`, `Ativa(2)`, `Inadimplente(3)`, `Suspensa(4)`, `Cancelada(5)` | Trial → Inadimplente → Suspensa; (qualquer) → Ativa via pagamento; → Cancelada |
| Plano/Assinatura | `Ciclo` (`CicloCobranca`) | `Mensal(1)`, `Anual(2)` | — |
| Assinatura | `MetodoPagamento` | `Pix`, `Boleto`, `Cartao` | escolhido no 1º pagamento |
| Fatura | `Status` (`StatusFatura`) | `Pendente(1)`, `Paga(2)`, `Vencida(3)`, `Falhou(4)`, `Cancelada(5)` | Pendente → Paga/Vencida |
| ConsentimentoRegistro | `Tipo` (`TipoConsentimento`) | `PoliticaPrivacidade(1)`, `TermosDeUso(2)`, `ConsentimentoParental(3)` | revogado via `RevogadoEm` (append-only) |
| SolicitacaoTitular | `Tipo` | `Acesso(1)`, `Exportacao(2)`, `Correcao(3)`, `Eliminacao(4)`, `Revogacao(5)`, `Outro(99)` | — |
| SolicitacaoTitular | `Status` | `Aberta(1)`, `EmAtendimento(2)`, `Concluida(3)`, `Recusada(4)` | Aberta → EmAtendimento → Concluida/Recusada |

---

## Regras de Negócio Detectadas

> Apenas regras **efetivamente encontradas** no código.

### Pessoas e acesso
- **E-mail de Pessoa único** (se informado) ao criar/atualizar (`PessoaService`).
- **Deduplicação de Pessoa** por Email → WhatsApp → Telefone (telefone/WhatsApp normalizados para só dígitos) (`VisitanteService`).
- **Perfil único ativo** por tipo (não duplica `Membro`, `Voluntario` etc. com `DataFim=null`).
- **EmailLogin único**; **uma Pessoa → no máximo um Usuário**; **Perfil de Acesso obrigatório** ao criar usuário (`UsuarioService`).
- **Política de senha:** 8+ caracteres com maiúscula + minúscula + número (`PasswordPolicy`, aplicada em signup, criação de usuário e troca de senha).
- **Login lockout:** após N tentativas falhas, bloqueia por tempo (campos `TentativasLoginFalhas`/`BloqueadoAte`; política 5 tentativas / 15 min).

### Voluntariado / Escalas
- **Vínculo de voluntário único** por `(Pessoa, Equipe, Cargo)`.
- **Quantidade de vaga ≥ 1** em item de modelo de escala.
- **Geração automática** respeita indisponibilidades e `DiasFolgaAposEscala`.
- **Troca de escala:** item `Substituido`/`Faltou` não gera solicitação; uma solicitação **Pendente** por item; substituto da mesma equipe, ≠ solicitante e sem conflito no evento; apenas líder/admin (ou o próprio voluntário) abre.
- **Acesso a escalas:** restrito a líder da equipe ou admin.

### Eventos / Inscrições
- **Inscrição só** se `Evento.AceitaInscricoes = true` e o evento **ainda não iniciou**.
- **Nome e WhatsApp obrigatórios**; demais campos conforme `ConfiguracaoFormularioInscricao` (JSON).
- **Deduplicação de inscrição** por WhatsApp no mesmo evento.
- **Ocorrência:** unicidade de horário; `DataHoraFim ≥ DataHoraInicio` quando informada.

### Kids
- Criança deve estar **ativa** e ser do **tipo Criança** para check-in; **não pode haver check-in aberto**.
- **Token QR de retirada expira em 8h**; **PIN não expira** enquanto `CheckedIn`.
- Retirada exige responsável `Ativo` **e** `PodeRetirar = true`.
- **Modo exceção** exige nome de quem retira e gera notificação de **ALERTA**.
- Pré-check-in: idempotente por criança+evento ativo; expira em ~10 min; só **Operador** confirma.
- Conteúdo de aula: só **Líder** cria/edita/publica; título e resumo obrigatórios antes de publicar; só `Published` aparece no app; responsável vê apenas conteúdo da sala/turma da criança (conteúdo geral é visível a todos); feed limitado aos **últimos 20**.
- Avisos manuais: só **Líder** cria; anexos só de tipo `Pdf`/`Imagem`/`Link`.
- **Alerta crítico da criança** (`TemAlertaCritico`) = possui alergias **ou** restrições alimentares.

### Financeiro / Doações / Patrimônio
- **Valor > 0** obrigatório em Receita, Despesa e Doação; **DataRecebimento**/**DataVencimento** obrigatórias.
- **Recorrências** calculam próximas datas: +7d (Semanal), +15d (Quinzenal), +1 mês (Mensal), +2/+3/+6 meses (Bi/Tri/Semestral), +1 ano (Anual).
- **Saldo de conta** = SaldoInicial + Σ(Receitas `Recebida`) − Σ(Despesas `Paga`).
- **Orçado × Realizado:** realizado = Σ lançamentos no ano com status ≠ Cancelada; variância e percentual calculados.
- **Doação:** `Documento` obrigatório p/ Pix; se há Finalidade, ela deve estar **ativa e visível**, `Valor ≥ ValorMinimo` e o método deve estar habilitado na finalidade; recibo só com `Confirmada`. Confirmação **gera Receita automaticamente** herdando categoria/conta/centro/projeto da Finalidade.
- **Patrimônio:** `Codigo` único e obrigatório; categoria obrigatória; `Quantidade ≥ 1`; movimentações aplicam transições de `Status` automaticamente; `CadastroInicial` registrado ao criar o bem.
- **Capacidade de sala/turma (Kids):** campo `CapacidadeMaxima` existe mas **não há evidência de bloqueio** no check-in (aparentemente informativo). `TODO: confirmar com o time`.

### Comunicação
- **Opt-out respeitado:** entrega para canal `Bloqueado` vira `IgnoradoPorPreferencia` (nunca é enviada).
- **Idempotência de entrega** por `ChaveDedupe` (`{CampanhaId}:{Canal}:{PessoaId}:{VisitanteId}`) e estado `Reservado` (evita envio duplicado entre workers).
- **Fallback de canal** por `Prioridade` em `ComunicacaoCampanhaCanal`.
- **Automação de aniversário:** uma vez por pessoa por ano (não reenvia se já `Enviado` no ano).
- **Lembrete operacional:** idempotente por `ChaveEvento`.

### SaaS / Billing / LGPD
- **E-mail único global** no signup (não por tenant).
- **Slug de tenant** único, formato `^[a-z0-9]+(?:-[a-z0-9]+)*$`, gerado por slugify com sufixo numérico em conflito.
- **Plano padrão** do signup: slug `organizacao`.
- **Preço anual padrão** = `PrecoMensal × 12` quando `PrecoAnual` é nulo.
- **Asaas `BillingType=UNDEFINED`** — pagador escolhe o método no 1º vencimento (não exige cartão no signup).
- **Limites de plano** (`MaxUsuarios`/`MaxMembros`, `null` = ilimitado) existem mas **não bloqueiam** operações (gap conhecido em `SAAS_READINESS.md`).
- **Tenant raiz** (`IsRootTenant`) imutável: não pode ser deletado/desativado.
- **Webhook de billing**: validado por token e **idempotente** por `(paymentId, evento)`.
- **Consentimento append-only**: revogação grava `RevogadoEm`, não apaga histórico.
- **Eliminação (LGPD) = anonimização**, não exclusão física (`SolicitacaoTitular.Tipo = Eliminacao`).

---

## Indicadores e Métricas

> Métricas calculadas pelos serviços (não há stack de métricas dedicada — ver ARCHITECTURE.md §Observabilidade).

### Financeiras (`ReceitaService`, `DespesaService`, `OrcamentoCategoriaService`)
- Total de receitas no período; nº de lançamentos; nº de membros que contribuíram e que **não** contribuíram; contribuições por membro e por categoria; informe anual por mês/categoria.
- Despesas segmentadas por vencimento: **vencidas**, **vencendo hoje**, **próximos 7 dias**, **próximos 30 dias** (com totais).
- **Saldo de conta** (calculado).
- **Orçado × Realizado** por categoria: variância e percentual; totais de receita/despesa orçados e realizados.

### Doações (`DoacoesService`)
- Total confirmado, por finalidade, por método; pendentes; taxa de conversão (Confirmadas/Total); valor médio; anônimas × identificadas.

### Patrimônio
- Bens por categoria, localização, responsável; bens em manutenção; com garantia ativa; vencidos em manutenção (`DataProximaManutencao < hoje`); valor total patrimonial (Σ `ValorAquisicao`).

### Auditoria (`AuditLogService`)
- Total de logs; ações críticas; ações com falha; usuários distintos; entidade/ação mais frequentes.

### LGPD (`SolicitacaoTitular`)
- **Prazo legal: 15 dias.** `PrazoVencido` e `DiasRestantes` calculados por solicitação.

### Schedulers
- `ISchedulerExecutionMonitor` registra sucesso/falha de execução dos jobs (exposto em health check).

> **SLA / tempo médio de atendimento / score:** além do prazo legal de 15 dias da LGPD e dos marcos de lembrete de escala (7 dias / 24h), **não há** indicadores de SLA ou *score* formais no código. `TODO: confirmar com o time`.

---

## Integrações e Conceitos Externos

> Detalhes técnicos em [.claude/INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md). Aqui, o que cada integração **introduz no domínio**.

| Integração | Papel no negócio | Conceitos que traz |
|---|---|---|
| **Asaas (gateway de pagamentos)** | (a) **Billing da assinatura** da igreja na plataforma; (b) **doações online** da igreja. | `customer`, `subscription`, `payment`; eventos de webhook (`PAYMENT_RECEIVED`, `PAYMENT_CONFIRMED`, `PAYMENT_OVERDUE`, `PAYMENT_REFUNDED`, `PAYMENT_CHARGEBACK_REQUESTED`); status (`RECEIVED`/`CONFIRMED`/`REFUNDED`/`CANCELLED`/`DELETED`); `BillingType=UNDEFINED`. Billing usa **uma** API key de plataforma; doações usam credencial **por tenant** (cifrada). |
| **Evolution API (WhatsApp)** | Canal de comunicação (campanhas, lembretes, aniversários). | Instância WhatsApp; envio de texto e imagem; código de país padrão `55`. |
| **Firebase Cloud Messaging (FCM)** | Push notifications no **AppKids** (check-in, checkout, alerta, avisos). | `KidsDeviceToken` (Android/iOS); canal **Push** exclusivo da API. |
| **SMTP / E-mail (Resend pretendido)** | Verificação de e-mail no signup; avisos de trial/suspensão/pagamento. | Templates `01-verificacao-email`, `02-trial-acabando`, `03-assinatura-suspensa`, `04-pagamento-pendente`. **Default desligado** (no-op sem SMTP). |
| **AWS S3 (opcional)** | Armazenamento de uploads/galerias. | URLs assinadas; default é disco local. |
| **Sentry** | Observabilidade de erros. | Sem PII (`SendDefaultPii=false`). |

**Como se relacionam com o domínio:**
- A **doação** confirmada pelo Asaas vira **Receita** no financeiro (ponte entre captação online e contabilidade da igreja).
- A **assinatura** (Asaas) controla o **acesso** ao sistema via gating HTTP 402.
- O **canal** de comunicação é uma abstração omnichannel (`IComunicacaoCanalProvider`) sobre WhatsApp/Email/Push/Notificação interna.

---

## Terminologia Utilizada pela Equipe

### Siglas e termos técnicos presentes
- **ChMS** — Church Management System (categoria do produto).
- **SaaS** — modelo de comercialização (assinatura multi-tenant).
- **Tenant** — a igreja (unidade de isolamento); **Slug** — identificador URL-friendly do tenant.
- **RBAC** — controle de acesso por papéis (`PerfilAcesso` × recurso × ação).
- **DTO** — objeto de transferência de dados (convenção de código).
- **LGPD** — Lei Geral de Proteção de Dados; papéis **Controladora** (igreja) e **Operadora** (VerboPlus).
- **PII** — dados pessoais (proibidos em logs/Sentry).
- **FCM** — Firebase Cloud Messaging (push).
- **PIX / Boleto / Cartão** — métodos de pagamento (doações e assinatura).
- **DPO** — encarregado de dados (definido como o próprio usuário na fase atual — ver memória de decisões LGPD).

### Termos de domínio (Português) com significado específico
- **Escala** — agenda de voluntários por equipe/ministério para uma ocorrência de evento.
- **Modelo de Escala** — template de vagas por cargo.
- **Retirada Segura** — checkout do Kids validado por QR token ou PIN.
- **Modo Exceção** — retirada por pessoa não autorizada (emergência), auditada e com alerta.
- **Código de Sessão** — identificador da presença da criança no dia.
- **Pré-check-in** — autorização prévia do responsável, confirmada pelo operador.
- **Alerta Crítico** — criança com alergia/restrição alimentar.
- **Finalidade (de doação)** — campanha/propósito de doação no Portal.
- **Lançamento** — registro individual de receita ou despesa; **Contribuição** — receita de membro (dízimo/oferta).
- **Bem** — item patrimonial; **Movimentação** — mudança de estado/localização do bem.
- **Carência** — período entre inadimplência e suspensão da assinatura.
- **Gating** — bloqueio de acesso (HTTP 402) por assinatura suspensa.
- **Célula / HubCasa** — pequeno grupo descentralizado, com **Anfitrião**, **Líder** e **Timóteo**.
- **Timóteo** — papel de liderança em formação numa célula (`HubCasa.TimoteoId`). `TODO: confirmar com o time` o significado pastoral exato adotado pela equipe.
- **Área (de equipe)** — classificação `Verde`/`Vermelha`/`Laranja`. `TODO: confirmar com o time` o critério de negócio por trás das cores.

### Papéis no Kids (strings de role nos serviços)
- **Operador** — confirma pré-check-in / realiza check-in.
- **Líder** — cria/edita/publica conteúdo de aula e avisos manuais.
`TODO: confirmar com o time` a relação exata entre esses papéis de Kids e o RBAC (`PerfilAcesso`)/`PerfilPessoa`.

---

## Eventos de Negócio

> Marcos de domínio observados (logs, notificações, transições, observabilidade de comunicação).

### Pessoas / Voluntariado
- Pessoa criada / atualizada; perfil de pessoa criado/atualizado/removido.
- Visitante criado → **dispara automação de comunicação**.
- Voluntário criado/atualizado/removido; indisponibilidade criada/removida.
- Escala criada / **publicada** (envia convites) / fechada.
- Convite enviado; voluntário **confirmou**/**recusou**; presença registrada (serviu/faltou); lembretes (7d / 24h).
- Solicitação de troca criada / aprovada (cria novo item, marca original `Substituido`) / rejeitada.

### Eventos
- Evento criado/atualizado; inscrições habilitadas/desabilitadas.
- Ocorrência criada/cancelada/realizada; ocorrências geradas por recorrência.
- Inscrição criada/confirmada/cancelada; presença registrada.

### Kids
- Check-in realizado → push `CHECKIN` a todos os responsáveis.
- Checkout realizado → push `CHECKOUT`.
- Retirada em exceção → push `ALERTA`.
- Ocorrência registrada/encerrada; conteúdo de aula publicado; aviso manual enviado.

### Financeiro / Doações / Patrimônio
- Receita/despesa criada; recorrência gerada; despesa paga.
- Doação criada → cobrança Pix criada → **pagamento recebido (webhook)** → doação confirmada → **receita criada automaticamente** → recibo consultado.
- Movimentação de patrimônio (cadastro inicial, transferência, manutenção envio/retorno, empréstimo, devolução, baixa).

### Comunicação (eventos nomeados em `ComunicacaoObservability`)
- `comunicacao.campanha.criada/agendada/cancelada`
- `comunicacao.entrega.reservada/enviada/falhou/reprocessada`
- `comunicacao.preferencia.atualizada`
- `comunicacao.automacao.executada/falhou`

### SaaS / LGPD
- Signup iniciado; e-mail verificado (ativa tenant/usuário); trial iniciado; aviso de trial enviado.
- Trial → inadimplência; inadimplência → suspensão; pagamento confirmado → reativação; cancelamento.
- Fatura paga/vencida; webhook de billing recebido (auditado em `EventoWebhookBilling`).
- Consentimento registrado/revogado; solicitação de titular aberta/atendida/concluída/recusada.
- Provisionamento de tenant (auditado como `ProvisionarTenant`).

---

## Regras Temporais

| Tema | Regra | Evidência |
|---|---|---|
| **Trial da assinatura** | Padrão **14 dias** (`TrialDias`). | `BillingSettings` |
| **Aviso de trial** | **3 dias antes** do fim (`TrialAvisoDiasAntes`), enviado uma única vez. | `BillingSettings`, `BillingCycleService` |
| **Carência de inadimplência** | **7 dias** após inadimplência antes de suspender (`CarenciaDias`). | `BillingSettings`, `BillingCycleService` |
| **Verificação de e-mail** | Token válido por **48 horas** (`VerificacaoValidaHoras`). | `SignupService` |
| **Próxima cobrança** | `now + 1 mês` (Mensal) ou `+1 ano` (Anual) após pagamento. | `BillingService` |
| **Solicitação de titular (LGPD)** | Prazo legal de resposta: **15 dias** (`PrazoLimite = SolicitadoEm + 15d`). | `SolicitacaoTitularService` |
| **Retirada Kids (token QR)** | Expira em **8 horas** após check-in. | `KidsService`/`KidsRetiradaService` |
| **Retirada Kids (PIN)** | **Não expira** enquanto `CheckedIn`. | `KidsRetiradaService` |
| **Pré-check-in Kids** | Expira em **~10 minutos**. | `KidsPreCheckinService` |
| **Lembretes de escala** | **7 dias** e **24 horas** antes do evento. | `EscalaItem` (campos de lembrete) |
| **Recorrência de eventos** | Semanal/Quinzenal/Mensal, dentro da vigência (`DataInicioVigencia`/`DataFimVigencia`). | `EventoRecorrencia` |
| **Recorrência financeira** | Semanal(+7d)…Anual(+1 ano). | `ReceitaService`/`DespesaService` |
| **Campanha de aniversário** | Diária, no `HorarioEnvio`, uma vez por pessoa/ano; fuso `America/Sao_Paulo`. | `BirthdayCampaignSchedulerService` |
| **Carrossel do Portal** | `TempoTransicaoCarrossel` (padrão **5s**). | `ConfiguracaoPortalService` |
| **Lockout de login** | 5 tentativas / janela de 15 min. | `LoginLockout` |

---

## Inconsistências de Terminologia

> Registradas **sem propor correção**.

1. **"Eliminação" (LGPD) ≠ exclusão física.** O tipo `SolicitacaoTitular.Eliminacao` significa **anonimização**, não *delete*. O nome sugere exclusão, mas o comportamento é anonimizar (preservando histórico/auditoria).
2. **"Asaas" cobre dois domínios distintos:** **billing da assinatura** (credencial de plataforma) e **doações online** (credencial por tenant). Mesmo provedor, conceitos e ciclos diferentes.
3. **"Status" como enum numérico vs. string.** Módulos antigos (financeiro, escalas, eventos, billing) usam **enums numéricos**; o módulo **Kids** usa **strings** (`"CheckedIn"`, `"Pending"`, `"Draft"`…). Mesmo conceito de "status", representações diferentes.
4. **`MetodoPagamentoDoacao` vs. `MetodoPagamentoAssinatura`:** dois enums separados de método de pagamento (Pix/Cartão/Boleto), um para doações e outro para assinatura.
5. **"Perfil" tem dois significados:** `PerfilPessoa` (papel de negócio: Visitante/Membro/Voluntário…) **e** `PerfilAcesso` (papel de permissão RBAC). Ambos chamados "perfil".
6. **"Recorrência" em contextos distintos:** `EventoRecorrencia` (regra de repetição de evento) vs. `TipoRecorrencia` financeiro (frequência de receita/despesa) — frequências não idênticas (eventos: Semanal/Quinzenal/Mensal; financeiro: até Anual).
7. **Papéis "Operador"/"Líder" no Kids** (strings em serviços) coexistem com o RBAC genérico (`PerfilAcesso`) e com `PerfilPessoa.Lider`. Não está explícito no código se são o mesmo conceito. `TODO: confirmar com o time`.
8. **Nomes de canais/destino em maiúsculas** (`"GERAL"`, `"CRIANCA"`, `"RESPONSAVEL"`, `"CHECKIN"`) no Kids vs. enum `CanalComunicacao` (PascalCase) na Comunicação — dois vocabulários de "canal/notificação".
9. **Rotas do Portal em inglês vs. controllers em português** (já registrado em ARCHITECTURE.md): a doc do Portal cita `/api/events`, `/api/church/info` enquanto o backend usa nomes em português. `TODO: confirmar com o time`.

---

## Dúvidas e Pendências

- `TODO: confirmar com o time` — **Capacidade de sala/turma no Kids:** `CapacidadeMaxima` existe mas não há evidência de bloqueio no check-in. É apenas informativo ou deveria restringir?
- `TODO: confirmar com o time` — **Faixa etária do Kids:** não há evidência de regra de idade que aloque criança em sala/turma automaticamente (a alocação é manual). Existe critério de negócio?
- `TODO: confirmar com o time` — **Papéis "Operador"/"Líder" do Kids** e sua relação com `PerfilAcesso` (RBAC) e `PerfilPessoa`.
- `TODO: confirmar com o time` — **Significado das Áreas de equipe** (`Verde`/`Vermelha`/`Laranja`): qual o critério de negócio?
- `TODO: confirmar com o time` — **"Timóteo" (HubCasa):** significado pastoral exato do papel.
- `TODO: confirmar com o time` — **Limites de plano (`MaxUsuarios`/`MaxMembros`)**: existem mas não bloqueiam. Devem passar a bloquear (decisão de produto)?
- `TODO: confirmar com o time` — **Status `Cancelada` de SolicitacaoTrocaEscala:** definido no enum, mas sem fluxo que o atribua no código analisado. Está reservado para uso futuro?
- `TODO: confirmar com o time` — **Voto único em enquete:** sem constraint no banco; a regra de "um voto por pessoa" depende do frontend. É intencional?
- `TODO: confirmar com o time` — **Validação de período de enquete:** `DataInicio`/`DataFim` parecem validadas no cliente, não no backend.
- `TODO: confirmar com o time` — **Seed de planos:** não há evidência clara de onde os 3 planos são semeados (placeholders R$49,90 / R$99,90 / R$199,90 citados em PROJECT_CONTEXT). Preços definitivos pendentes.
- `TODO: confirmar com o time` — **Indicadores de SLA / tempo médio / score:** não há métricas formais além do prazo LGPD (15d) e lembretes de escala (7d/24h). Há expectativa de SLA operacional?
- `TODO: confirmar com o time` — **Estado da migração do módulo de Comunicação (strangler):** quanto do legado já foi substituído em produção (ver `COMUNICACAO_SPRINT1_MAPA_LEGADO.md`).
- `TODO: confirmar com o time` — **Revisão jurídica dos documentos LGPD** em `legal/` (razão social, CNPJ, DPO, foro) antes de publicar.

---

### Fontes
Documento derivado da análise direta das entidades (`BackEnd/src/SistemaIgreja.Domain/Entities/`), serviços (`Application/Services/`, `Infrastructure/Services/`), middlewares e DTOs do backend .NET, cruzada com os documentos canônicos do projeto:
- [.claude/PROJECT_CONTEXT.md](PROJECT_CONTEXT.md) — visão de negócio, stack e conceitos de domínio.
- [ARCHITECTURE.md](ARCHITECTURE.md) — módulos funcionais, fluxos e integrações.
- [.claude/INTEGRATION_PATTERNS.md](INTEGRATION_PATTERNS.md) — padrões de integração externa.
- [.claude/CODING_STANDARDS.md](CODING_STANDARDS.md) — convenções (nomenclatura de domínio em Português).
- [.claude/MIGRATION_RULES.md](MIGRATION_RULES.md) — regras de migração.
- [SAAS_READINESS.md](../SAAS_READINESS.md) — gaps de produção (ex.: limites de plano não bloqueiam).
- `legal/` — Termos de Uso e Política de Privacidade (v1).
</content>
</invoke>
