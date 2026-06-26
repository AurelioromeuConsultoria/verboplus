import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'core/api_client.dart';
import 'core/app_palette.dart';
import 'core/auth_repository.dart';
import 'core/push_service.dart';
import 'app_state.dart';
import 'app_router.dart';
import 'features/auth/login_screen.dart';
import 'features/kids/minha_crianca_detalhe_screen.dart';
import 'features/kids/minhas_criancas_screen.dart';
import 'features/kids/kids_repository.dart';
import 'features/avisos/avisos_screen.dart';
import 'features/settings/settings_screen.dart';

/// URL base da API. Sobrescrito em produção via --dart-define=API_BASE_URL=...
const kBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: 'http://localhost:7000',
);

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await PushService.initializeFirebase();

  final api = ApiClient(baseUrl: kBaseUrl);
  final authRepo = AuthRepository(api);
  final pushService = PushService(api);
  pushService.setupMessageHandlers();

  Usuario? initialUser;
  final token = await api.getToken();
  if (token != null) {
    initialUser = await authRepo.me();
    if (initialUser == null) {
      await api.clearTokens();
    } else {
      // Sessão restaurada — re-registra token de push em background.
      pushService.registerTokenWithBackend();
    }
  }

  runApp(AppKidsApp(
    api: api,
    authRepo: authRepo,
    initialUser: initialUser,
    pushService: pushService,
  ));
}

class AppKidsApp extends StatelessWidget {
  const AppKidsApp({
    super.key,
    required this.api,
    required this.authRepo,
    required this.pushService,
    this.initialUser,
  });

  final ApiClient api;
  final AuthRepository authRepo;
  final PushService pushService;
  final Usuario? initialUser;

  @override
  Widget build(BuildContext context) {
    final kidsRepo = KidsRepository(api);

    return MultiProvider(
      providers: [
        Provider<ApiClient>.value(value: api),
        Provider<AuthRepository>.value(value: authRepo),
        Provider<KidsRepository>.value(value: kidsRepo),
        Provider<PushService>.value(value: pushService),
        ChangeNotifierProvider<AppState>(
          create: (_) => AppState(initialUser: initialUser),
        ),
      ],
      child: MaterialApp.router(
        title: 'Verbo+ Kids',
        theme: _buildTheme(),
        routerConfig: AppRouter.router(
          loginScreen: const LoginScreen(),
          homeScreen: const MinhasCriancasScreen(),
          avisosScreen: const AvisosScreen(),
          settingsScreen: const SettingsScreen(),
          minhaCriancaDetalheBuilder: (criancaPessoaId) =>
              MinhaCriancaDetalheScreen(criancaPessoaId: criancaPessoaId),
        ),
      ),
    );
  }

  ThemeData _buildTheme() {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: AppPalette.deepSea,
      brightness: Brightness.light,
    ).copyWith(
      primary: AppPalette.deepSea,
      secondary: AppPalette.aqua,
      tertiary: AppPalette.apricot,
      surface: AppPalette.shell,
      onSurface: AppPalette.ink,
      onSurfaceVariant: AppPalette.mutedInk,
      surfaceContainerHighest: AppPalette.fog,
      outlineVariant: AppPalette.line,
    );

    return ThemeData(
      colorScheme: colorScheme,
      useMaterial3: true,
      scaffoldBackgroundColor: AppPalette.cream,
      appBarTheme: const AppBarTheme(
        centerTitle: false,
        elevation: 0,
        surfaceTintColor: Colors.transparent,
        backgroundColor: AppPalette.cream,
        foregroundColor: AppPalette.ink,
        titleTextStyle: TextStyle(
          color: AppPalette.ink,
          fontSize: 22,
          fontWeight: FontWeight.w800,
        ),
        iconTheme: IconThemeData(color: AppPalette.ink),
      ),
      cardTheme: CardThemeData(
        elevation: 0,
        color: AppPalette.shell,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(32),
          side: BorderSide(color: colorScheme.outlineVariant),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: AppPalette.lilac,
          foregroundColor: Colors.white,
          minimumSize: const Size.fromHeight(56),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(26),
          ),
          textStyle: const TextStyle(fontSize: 17, fontWeight: FontWeight.w700),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(54),
          foregroundColor: AppPalette.deepSea,
          side: const BorderSide(color: AppPalette.line),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(26),
          ),
          textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
        ),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: AppPalette.shell,
        side: BorderSide(color: colorScheme.outlineVariant),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: AppPalette.lilac,
        contentTextStyle: const TextStyle(
          color: Colors.white,
          fontWeight: FontWeight.w600,
        ),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(18)),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: Colors.white.withValues(alpha: 0.88),
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 18, vertical: 18),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(24),
          borderSide: BorderSide(color: colorScheme.outlineVariant),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(24),
          borderSide: BorderSide(color: colorScheme.outlineVariant),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(24),
          borderSide: BorderSide(color: colorScheme.primary, width: 1.5),
        ),
      ),
      textTheme: const TextTheme(
        headlineMedium: TextStyle(
          fontSize: 34,
          fontWeight: FontWeight.w800,
          letterSpacing: -0.8,
          color: AppPalette.ink,
        ),
        headlineSmall: TextStyle(
          fontSize: 28,
          fontWeight: FontWeight.w800,
          letterSpacing: -0.6,
          color: AppPalette.ink,
        ),
        titleLarge: TextStyle(
          fontSize: 24,
          fontWeight: FontWeight.w800,
          letterSpacing: -0.3,
          color: AppPalette.ink,
        ),
        titleMedium: TextStyle(
          fontSize: 18,
          fontWeight: FontWeight.w700,
          color: AppPalette.ink,
        ),
        bodyLarge: TextStyle(fontSize: 16, height: 1.4, color: AppPalette.ink),
        bodyMedium: TextStyle(
          fontSize: 14,
          height: 1.4,
          color: AppPalette.mutedInk,
        ),
        labelLarge: TextStyle(
          fontSize: 14,
          fontWeight: FontWeight.w700,
          color: AppPalette.ink,
        ),
      ),
    );
  }
}
