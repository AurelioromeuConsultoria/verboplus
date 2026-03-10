# App Kids – Especificação e sugestões de funcionalidades

## O que já existe

### Backend (APIs usadas pelo app)

- **Auth:** `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/refresh`, `PUT /api/auth/alterar-senha`.
- **Kids – Crianças:** `GET/POST/PUT/DELETE /api/kids/criancas`, `GET /api/kids/criancas/{id}`, responsáveis (vincular/atualizar/desvincular).
- **Kids – Check-in/Check-out:**
  - `POST /api/kids/checkin` – body: `criancaPessoaId`, `metodo` (QR/PIN/ADMIN), opcional `checkinByPessoaId`, `observacoes`. Resposta: `checkinId`, `codigoSessao`, `checkinTime`. Cria notificações automáticas (CHECKIN) para responsáveis.
  - `POST /api/kids/checkout` – body: `criancaPessoaId`, `codigoSessao`, `checkoutByPessoaId`, opcional `metodo`. Valida se quem retira é responsável com `PodeRetirar`. Cria notificações (CHECKOUT).
  - `GET /api/kids/checkins` – histórico (query: `criancaPessoaId` opcional).
- **Entidade KidsNotificacao** no domínio: `Tipo` (CHECKIN, CHECKOUT, ALERTA), `Mensagem`, `Status` (Pendente/Enviado/Falhou), `CriancaPessoaId`, `ResponsavelPessoaId`. **Não há controllers/endpoints expostos** para listar ou criar avisos manuais.

### App Flutter (estado atual)

- Login com email/senha, token em secure storage.
- Tela principal: lista de crianças, check-in/check-out por toque (método ADMIN) e por **leitura de QR**.
- QR check-in: conteúdo = **ID da criança** (`criancaPessoaId`).
- QR check-out: conteúdo = **código de sessão** (e, se necessário, `codigoSessao,criancaPessoaId` para o backend aceitar; ver “O que falta no backend”).
- Tela “Avisos” apenas placeholder.

---

## O que falta no backend (para o app ficar redondo)

1. **Check-out só com código de sessão**  
   Hoje o `CheckoutRequest` exige `CriancaPessoaId` e `CodigoSessao`. Para o pai só escanear o QR (que pode ter só o código):
   - **Opção A:** Novo endpoint, por exemplo `GET /api/kids/checkins/by-codigo?codigoSessao=XXX`, retornando o check-in ativo (com `criancaPessoaId`, nome da criança, etc.). O app chama esse GET, obtém o `criancaPessoaId` e depois chama `POST /api/kids/checkout` com esse ID e o código.
   - **Opção B:** Endpoint único de check-out por código, e.g. `POST /api/kids/checkout-by-code` com body `{ "codigoSessao": "...", "checkoutByPessoaId": 123 }`; o backend resolve a criança pelo código.

2. **Avisos para pais**
   - **Listar notificações do responsável:** e.g. `GET /api/kids/notificacoes?responsavelPessoaId=...` ou `GET /api/kids/me/notificacoes` (usando o usuário logado) – retornar `KidsNotificacaoDto` (lista).
   - **Criar aviso manual (geral ou por criança):**
     - Aviso **geral:** e.g. `POST /api/kids/avisos` com `{ "mensagem": "...", "tipo": "ALERTA" }` – backend cria uma notificação para todos os responsáveis (ou para um conjunto definido por regra).
     - Aviso **por criança:** e.g. `POST /api/kids/criancas/{id}/avisos` com `{ "mensagem": "..." }` – cria notificação para os responsáveis daquela criança.
   - **Marcar como lida/enviada:** e.g. `PATCH /api/kids/notificacoes/{id}` com `{ "status": "Enviado" }` (e eventualmente integrar com push/WhatsApp/email).

3. **QR da criança (opcional)**  
   Hoje o app assume que o QR de check-in contém o `criancaPessoaId`. Se no futuro existir um “código fixo” por criança (ex.: código de 6 dígitos), um endpoint `GET /api/kids/criancas/by-codigo?codigo=XXX` que retorne a criança ajuda a não expor o ID interno.

---

## Sugestões de funcionalidades (baseadas no que já existe e no que faz sentido)

### Já coberto ou quase

- **Login** – feito.
- **Check-in/check-out com QR** – feito (com o ajuste de backend acima para check-out só com código).
- **Aviso para pais** – depende dos endpoints de notificações/avisos acima; quando existirem, o app pode: lista de avisos, aviso geral na home, aviso por criança (ao abrir detalhe da criança).

### Sugestões adicionais

1. **Mostrar código de sessão / QR para o responsável após o check-in**  
   No check-in, a API já devolve `codigoSessao`. O app pode:
   - Mostrar o código em texto grande (para o pai anotar ou tirar foto).
   - Gerar um **QR com o código** (e, se o backend continuar exigindo, no mesmo QR incluir `codigoSessao,criancaPessoaId` no formato que o backend esperar) para o pai escanear na saída. Assim o fluxo “entrada = check-in; saída = escanear o mesmo QR” fica claro.

2. **Histórico de check-ins por criança (no app)**  
   Já existe `GET /api/kids/checkins?criancaPessoaId=`. Usar para uma tela “Detalhe da criança” com lista de entradas/saídas (data/hora, quem fez).

3. **Notificações push (check-in/check-out e avisos)**  
   Integrar Firebase (ou outro) no app e no backend: ao criar `KidsNotificacao`, enviar push para o dispositivo do responsável. Melhora muito a experiência “aviso para pais”.

4. **Perfil “pai/mãe” no app**  
   Se o usuário logado for um “responsável” (tem vínculo com crianças), ao abrir o app:
   - Mostrar só “minhas crianças” e seus check-ins atuais.
   - Check-out apenas das suas crianças (já validado no backend por `PodeRetirar`).
   - Listar apenas avisos que o envolvem (por criança ou gerais).  
   Isso exige que o login esteja ligado a uma `Pessoa` e que a API “me” (ou um endpoint “minhas crianças”) devolva a lista de crianças do responsável.

5. **Sala/turma**  
   O `CriancaDto` já tem `SalaId`. No app: filtro por sala na lista de crianças; ou etiquetas por sala no check-in (ex.: “Berçário”, “2–4 anos”).

6. **Alergias e restrições visíveis no check-in**  
   `CriancaDto` tem `Alergias` e `RestricoesAlimentares`. Na lista ou no detalhe da criança, exibir um destaque (ícone + texto) para a equipe ver rápido no momento do check-in.

7. **Quem pode retirar**  
   Backend já tem `PodeRetirar` por responsável. No app (para pais): mostrar “Autorizado a retirar: sim/não” no seu vínculo; para equipe: ao fazer check-out, mostrar apenas responsáveis com `PodeRetirar` ou avisar se quem está retirando não está na lista.

8. **Relatório simples (admin)**  
   Para uso institucional (pode ser no frontend web ou num futuro app admin): “Quantas crianças hoje com check-in ativo”, “tempo médio de permanência”, “últimos check-outs”. O backend já tem os dados em `KidsCheckins`; basta agregar.

9. **PIN de retirada (alternativa ao QR)**  
   Backend já suporta `metodo: "PIN"`. O app pode: após check-in, gerar um PIN de 4–6 dígitos e mostrar ao responsável; no check-out, o responsável digita o PIN em vez de escanear QR (útil quando a câmera não for prática).

10. **Offline / fila de sincronização**  
    Em locais com rede instável: guardar check-in/check-out localmente e enviar quando houver conexão. Exige desenho de idempotência e conflitos no backend (ex.: não permitir dois check-outs para o mesmo `codigoSessao`).

---

## Resumo

- **App:** Login, check-in/check-out (lista + QR) e tela de avisos (placeholder) já estão na base. Falta conectar avisos quando a API existir e (se quiser) ajustar o fluxo de check-out por QR conforme o backend.
- **Backend:** Principalmente (1) permitir check-out por código (ou endpoint “checkin by codigo”); (2) endpoints de notificações/avisos (listar, criar geral, criar por criança); (3) opcional: código fixo por criança e push.
- **Sugestões:** Código/QR para o pai na saída, histórico por criança, push, perfil responsável, sala, alergias em destaque, quem pode retirar, PIN, relatório admin, sincronização offline.
