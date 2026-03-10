# App Kids (Flutter)

App mobile para o módulo Kids do Sistema Igreja: **login**, **check-in/check-out com leitura de QR Code** e **avisos para pais**. Consome as APIs do backend em .NET.

## Funcionalidades atuais

- **Login** – Autenticação via `POST /api/auth/login` (email/senha), token guardado em secure storage.
- **Check-in** – Lista de crianças + botão Check-in por criança; ou escanear QR com o **ID da criança** (`criancaPessoaId`) e fazer check-in com método `QR`.
- **Check-out** – Na lista, botão Check-out para quem está com check-in ativo; ou escanear QR com **código de sessão** (e, se o backend exigir, ID da criança no mesmo QR no formato `codigoSessao,criancaPessoaId`).
- **Avisos** – Tela placeholder; backend tem entidade `KidsNotificacao` mas ainda não expõe endpoints para listar/criar avisos.
- **Notificações push** – Após login, o app registra o token FCM no backend. Em cada check-in e check-out o backend envia push aos responsáveis (Firebase Cloud Messaging). Toque na notificação abre a tela de Avisos.

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
   flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5000
   ```
   Para dispositivo físico use o IP da máquina, e.g. `http://192.168.1.10:5000`.
4. **Permissões** – O app usa câmera para QR. No Android, `android/app/src/main/AndroidManifest.xml` deve ter:
   ```xml
   <uses-permission android:name="android.permission.CAMERA" />
   ```
   No iOS, em `ios/Runner/Info.plist`:
   ```xml
   <key>NSCameraUsageDescription</key>
   <string>Usar câmera para ler QR de check-in/check-out</string>
   ```
   (Se usar `flutter create .`, verifique se o `mobile_scanner` já adiciona algo; senão, inclua manualmente.)

5. **Push (Firebase)** – Para receber notificações de check-in/check-out:
   - Crie um projeto no [Firebase Console](https://console.firebase.google.com) e ative Cloud Messaging.
   - No app: baixe `google-services.json` (Android) e `GoogleService-Info.plist` (iOS) e coloque em `android/app/` e `ios/Runner/` conforme a documentação do FlutterFire.
   - No backend: em `appsettings.json` defina `Firebase:CredentialsPath` com o caminho do arquivo JSON da **conta de serviço** (Service Account) do Firebase (para o servidor enviar as mensagens).

## Estrutura

- `lib/core/` – `ApiClient` (HTTP + token), `AuthRepository`, `PushService` (FCM e registro de token).
- `lib/features/auth/` – Tela de login.
- `lib/features/kids/` – Lista de crianças, check-in/check-out, `KidsRepository`, tela de leitura de QR.
- `lib/features/avisos/` – Tela de avisos (placeholder).
- `lib/app_state.dart` – Estado global (usuário logado).
- `lib/app_router.dart` – Rotas (go_router): `/login`, `/`, `/avisos`.

## APIs usadas

| Método | Endpoint | Uso |
|--------|----------|-----|
| POST | `/api/auth/login` | Login |
| GET  | `/api/auth/me`     | (futuro: restaurar sessão) |
| GET  | `/api/kids/criancas` | Listar crianças |
| GET  | `/api/kids/criancas/{id}` | Detalhe da criança |
| POST | `/api/kids/checkin` | Check-in (body: `criancaPessoaId`, `metodo`, opcional `checkinByPessoaId`) |
| POST | `/api/kids/checkout` | Check-out (body: `criancaPessoaId`, `codigoSessao`, `checkoutByPessoaId`) |
| GET  | `/api/kids/checkins` | Histórico de check-ins |
| POST | `/api/kids/me/device-token` | Registrar token FCM (body: `token`, `platform`) |

Para mais detalhes e sugestões de funcionalidades (e o que falta no backend), veja **[APP_KIDS_ESPECIFICACAO.md](APP_KIDS_ESPECIFICACAO.md)**.
