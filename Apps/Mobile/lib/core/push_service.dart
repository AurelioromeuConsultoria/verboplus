import 'dart:io';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'api_client.dart';

/// Serviço de notificações push (FCM). Inicializa Firebase, obtém token e registra no backend.
/// Trata mensagens em primeiro plano e ao abrir pelo toque na notificação.
class PushService {
  PushService(this._api);

  final ApiClient _api;
  String? _fcmToken;
  void Function()? onNotificationOpened;

  /// Inicializa Firebase. Não lança se não estiver configurado (app segue sem push).
  static Future<void> initializeFirebase() async {
    try {
      await Firebase.initializeApp();
    } catch (_) {
      // Firebase não configurado (ex.: falta google-services.json / GoogleService-Info.plist)
    }
  }

  /// Solicita permissão e obtém o token FCM. Retorna null se não houver Firebase ou permissão negada.
  Future<String?> requestPermissionAndGetToken() async {
    try {
      final messaging = FirebaseMessaging.instance;
      final settings = await messaging.requestPermission(
        alert: true,
        badge: true,
        sound: true,
      );
      if (settings.authorizationStatus == AuthorizationStatus.denied) return null;
      final token = await messaging.getToken();
      _fcmToken = token;
      return token;
    } catch (_) {
      return null;
    }
  }

  /// Envia o token FCM para o backend (POST /api/kids/me/device-token). Só faz sentido após login.
  Future<bool> registerTokenWithBackend() async {
    final token = _fcmToken ?? await requestPermissionAndGetToken();
    if (token == null) return false;
    final platform = Platform.isIOS ? 'iOS' : 'Android';
    final response = await _api.post(
      '/api/kids/me/device-token',
      body: {'token': token, 'platform': platform},
    );
    return response.statusCode == 204;
  }

  static bool _handlersSetup = false;

  /// Configura handlers para mensagens em primeiro plano e ao abrir notificação. Só executa uma vez.
  void setupMessageHandlers() {
    if (_handlersSetup) return;
    _handlersSetup = true;
    try {
      FirebaseMessaging.onMessage.listen((RemoteMessage message) {
        final title = message.notification?.title ?? message.data['title'] ?? 'App Kids';
        final body = message.notification?.body ?? message.data['body'] ?? '';
        _onForegroundMessage(title, body, message.data);
      });
      FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
        onNotificationOpened?.call();
      });
    } catch (_) {
      _handlersSetup = false;
    }
  }

  /// Verifica se o app foi aberto por uma notificação (app em background/terminado).
  static Future<bool> hadInitialNotification() async {
    try {
      final msg = await FirebaseMessaging.instance.getInitialMessage();
      return msg != null;
    } catch (_) {
      return false;
    }
  }

  void _onForegroundMessage(String title, String body, Map<String, dynamic> data) {
    // Pode injetar um callback para mostrar SnackBar/Overlay se necessário
  }

  /// Retorna o token atual (pode ser null).
  String? get currentToken => _fcmToken;
}
