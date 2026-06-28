# App Kids (Flutter)

App mobile do módulo Kids do Sistema Igreja, orientado ao **responsável**. Consome as APIs do backend em .NET.

## Funcionalidades atuais

- **Login** com autenticação via `POST /api/auth/login`.
- **Minhas crianças** via `GET /api/kids/me/criancas`.
- **Detalhe da criança** com status atual, dados sensíveis e histórico recente.
- **Avisos reais** via `GET /api/kids/me/avisos`, com marcação como lido.
- **Push** com registro de token FCM em `POST /api/kids/me/device-token`.
- **Retirada segura** com token/PIN, validação e confirmação.

## Como rodar

1. **Criar o projeto Flutter** (se ainda não tiver `android/` e `ios/`):
   ```bash
   cd AppKids && flutter create .
   ```
2. **Instalar dependências:**
   ```bash
   flutter pub get
   ```
3. **Configurar URL da API** (exemplo para emulador Android):
   ```bash
   flutter run --dart-define=API_BASE_URL=http://10.0.2.2:7000
   ```
   Para dispositivo físico use o IP da máquina, e.g. `http://192.168.1.10:7000`.
4. **Push (Firebase)** – Para receber notificações de avisos e mudanças de status:
   - Crie um projeto no [Firebase Console](https://console.firebase.google.com) e ative Cloud Messaging.
   - No app: baixe `google-services.json` (Android) e `GoogleService-Info.plist` (iOS) e coloque em `android/app/` e `ios/Runner/` conforme a documentação do FlutterFire.
   - No backend: em `appsettings.json` defina `Firebase:CredentialsPath` com o caminho do arquivo JSON da **conta de serviço** (Service Account) do Firebase (para o servidor enviar as mensagens).

## Estrutura

- `lib/core/` – `ApiClient`, autenticação e push.
- `lib/features/auth/` – Login.
- `lib/features/kids/` – `minhas_criancas`, detalhe da criança, retirada segura e `KidsRepository`.
- `lib/features/avisos/` – Feed real de avisos.
- `lib/app_router.dart` – Rotas do app.

## APIs usadas

| Método | Endpoint | Uso |
|--------|----------|-----|
| POST | `/api/auth/login` | Login |
| GET  | `/api/auth/me`     | (futuro: restaurar sessão) |
| GET  | `/api/kids/me/criancas` | Listar crianças do responsável |
| GET  | `/api/kids/me/criancas/{id}` | Detalhe da criança |
| GET  | `/api/kids/me/checkins` | Histórico permitido ao responsável |
| GET  | `/api/kids/me/avisos` | Feed de avisos |
| PATCH | `/api/kids/me/avisos/{id}/lido` | Marcar aviso como lido |
| POST | `/api/kids/retirada/validar` | Validar token ou PIN |
| POST | `/api/kids/retirada/confirmar` | Confirmar retirada |
| POST | `/api/kids/retirada/excecao` | Registrar exceção |
| POST | `/api/kids/me/device-token` | Registrar token FCM (body: `token`, `platform`) |

Para visão funcional mais detalhada, veja **[APP_KIDS_ESPECIFICACAO.md](APP_KIDS_ESPECIFICACAO.md)**.
