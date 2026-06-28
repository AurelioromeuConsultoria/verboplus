import 'package:flutter/material.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:provider/provider.dart';
import 'core/api_client.dart';
import 'core/app_palette.dart';
import 'core/auth_repository.dart';
import 'core/cache_service.dart';
import 'core/connectivity_service.dart';
import 'core/push_service.dart';
import 'app_state.dart';
import 'app_router.dart';
import 'features/auth/login_screen.dart';
import 'features/kids/minha_crianca_detalhe_screen.dart';
import 'features/kids/minhas_criancas_screen.dart';
import 'features/kids/kids_repository.dart';
import 'features/settings/settings_screen.dart';

/// URL base da API. Sobrescrito em produção via --dart-define=API_BASE_URL=...
const kBaseUrl = String.fromEnvironment(
  'API_BASE_URL',
  defaultValue: 'http://10.0.2.2:7000',
);

/// Slug do tenant pré-configurado no build da organização.
/// Deixar vazio para que o campo apareça no formulário de cadastro.
const kTenantSlug = String.fromEnvironment('TENANT_SLUG', defaultValue: '');

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDateFormatting('pt_BR', null);
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
        Provider<CacheService>(create: (_) => CacheService()),
        ChangeNotifierProvider<ConnectivityService>(
          create: (_) => ConnectivityService(),
        ),
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
          settingsScreen: const SettingsScreen(),
          minhaCriancaDetalheBuilder: (criancaPessoaId) =>
              MinhaCriancaDetalheScreen(criancaPessoaId: criancaPessoaId),
          tenantSlug: kTenantSlug,
        ),
      ),
    );
  }

  ThemeData _buildTheme() {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: AppPalette.primary,
        brightness: Brightness.light,
      ).copyWith(
        primary: AppPalette.primary,
        surface: AppPalette.card,
        onSurface: AppPalette.ink,
        onSurfaceVariant: AppPalette.midInk,
        surfaceContainerHighest: AppPalette.divider,
        outlineVariant: AppPalette.border,
      ),
      scaffoldBackgroundColor: AppPalette.bg,
      appBarTheme: const AppBarTheme(
        backgroundColor: AppPalette.card,
        foregroundColor: AppPalette.ink,
        elevation: 0,
        centerTitle: false,
        surfaceTintColor: Colors.transparent,
        titleTextStyle: TextStyle(
          color: AppPalette.ink,
          fontSize: 20,
          fontWeight: FontWeight.w800,
          letterSpacing: -0.3,
        ),
        iconTheme: IconThemeData(color: AppPalette.ink),
      ),
      cardTheme: CardThemeData(
        elevation: 0,
        color: AppPalette.card,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
          side: const BorderSide(color: AppPalette.border),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: AppPalette.primary,
          foregroundColor: Colors.white,
          minimumSize: const Size.fromHeight(52),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(52),
          foregroundColor: AppPalette.primary,
          side: const BorderSide(color: AppPalette.border, width: 1.5),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          textStyle: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700),
        ),
      ),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: AppPalette.ink,
        contentTextStyle: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: AppPalette.divider,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide.none,
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: BorderSide.none,
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: AppPalette.primary, width: 2),
        ),
      ),
    );
  }
}
