import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/api_client.dart';
import 'core/auth_repository.dart';
import 'core/push_service.dart';
import 'app_state.dart';
import 'app_router.dart';
import 'features/auth/login_screen.dart';
import 'features/kids/checkin_checkout_screen.dart';
import 'features/kids/kids_repository.dart';
import 'features/avisos/avisos_screen.dart';

@pragma('vm:entry-point')
Future<void> _firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  // Handler para mensagens recebidas com o app em background/terminado.
  // O sistema exibe a notificação automaticamente quando notification está presente.
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await PushService.initializeFirebase();
  try {
    FirebaseMessaging.onBackgroundMessage(_firebaseMessagingBackgroundHandler);
  } catch (_) {}
  runApp(const AppKidsApp());
}

/// URL base da API. Em produção use variável de ambiente ou build flavor.
const kBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: 'http://localhost:5000',
);

class AppKidsApp extends StatelessWidget {
  const AppKidsApp({super.key});

  @override
  Widget build(BuildContext context) {
    final api = ApiClient(baseUrl: kBaseUrl);
    final authRepo = AuthRepository(api);
    final kidsRepo = KidsRepository(api);
    final pushService = PushService(api);

    return MultiProvider(
      providers: [
        Provider<ApiClient>.value(value: api),
        Provider<AuthRepository>.value(value: authRepo),
        Provider<KidsRepository>.value(value: kidsRepo),
        Provider<PushService>.value(value: pushService),
        ChangeNotifierProvider<AppState>(create: (_) => AppState()),
      ],
      child: MaterialApp.router(
        title: 'App Kids',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.blue),
          useMaterial3: true,
        ),
        routerConfig: AppRouter.router(
          loginScreen: const LoginScreen(),
          homeScreen: const CheckinCheckoutScreen(),
          avisosScreen: const AvisosScreen(),
        ),
      ),
    );
  }
}
