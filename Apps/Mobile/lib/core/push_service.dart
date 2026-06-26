import 'dart:io';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'api_client.dart';

/// Serviço de push notifications via Firebase Cloud Messaging.
///
/// Lifecycle esperado:
///   1. [initializeFirebase] — chamado em main() antes de runApp.
///   2. [setupMessageHandlers] — chamado em main() logo após criar a instância.
///   3. [registerTokenWithBackend] — chamado após login (e em auto-login).
///   4. [unregisterToken] — chamado em logout para parar de receber push.
///
/// Se as config files do Firebase (google-services.json / GoogleService-Info.plist)
/// não estiverem presentes, o serviço opera em modo no-op sem crash.
class PushService {
  PushService(this._api);

  final ApiClient _api;
  String? _currentToken;
  void Function()? onNotificationOpened;

  static bool _firebaseReady = false;
  static bool _handlersSetUp = false;

  static Future<void> initializeFirebase() async {
    try {
      await Firebase.initializeApp();
      _firebaseReady = true;
    } catch (e) {
      // Config files ausentes — modo no-op até o projeto Firebase ser criado.
      debugPrint('PushService: Firebase init skipped — $e');
    }
  }

  Future<String?> requestPermissionAndGetToken() async {
    if (!_firebaseReady) return null;
    try {
      final messaging = FirebaseMessaging.instance;
      final settings = await messaging.requestPermission(
        alert: true,
        badge: true,
        sound: true,
      );
      if (settings.authorizationStatus == AuthorizationStatus.denied) {
        return null;
      }
      // No iOS, o token FCM exige que o APNs token esteja disponível primeiro.
      if (!kIsWeb && Platform.isIOS) {
        await messaging.getAPNSToken();
      }
      final token = await messaging.getToken();
      _currentToken = token;
      return token;
    } catch (e) {
      debugPrint('PushService: getToken failed — $e');
      return null;
    }
  }

  Future<bool> registerTokenWithBackend() async {
    final token = await requestPermissionAndGetToken();
    if (token == null) return false;
    try {
      final platform = !kIsWeb && Platform.isIOS ? 'iOS' : 'Android';
      final response = await _api.post(
        '/api/kids/me/device-token',
        body: {'token': token, 'platform': platform},
      );
      return response.statusCode == 204;
    } catch (e) {
      debugPrint('PushService: registerTokenWithBackend failed — $e');
      return false;
    }
  }

  Future<void> unregisterToken() async {
    final token = _currentToken ?? await _fetchToken();
    if (token == null) return;
    try {
      await _api.delete('/api/kids/me/device-token', body: {'token': token});
    } catch (_) {}
    _currentToken = null;
  }

  void setupMessageHandlers() {
    if (!_firebaseReady || _handlersSetUp) return;
    _handlersSetUp = true;

    // Mensagem recebida com o app em foreground — sem popup automático no FCM,
    // mas aqui poderia acionar um snackbar ou badge no ícone de avisos.
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      debugPrint('PushService: foreground — ${message.notification?.title}');
    });

    // Usuário tocou na notificação com o app em background/suspended.
    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      debugPrint('PushService: opened from bg — ${message.notification?.title}');
      onNotificationOpened?.call();
    });
  }

  /// Verifica se o app foi aberto a partir de uma notificação (estado terminated).
  static Future<bool> hadInitialNotification() async {
    if (!_firebaseReady) return false;
    try {
      final msg = await FirebaseMessaging.instance.getInitialMessage();
      return msg != null;
    } catch (_) {
      return false;
    }
  }

  String? get currentToken => _currentToken;

  Future<String?> _fetchToken() async {
    if (!_firebaseReady) return null;
    try {
      return await FirebaseMessaging.instance.getToken();
    } catch (_) {
      return null;
    }
  }
}
