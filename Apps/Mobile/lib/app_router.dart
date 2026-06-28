import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'app_state.dart';
import 'features/avisos/avisos_screen.dart';
import 'features/auth/cadastro_screen.dart';
import 'features/kids/historico_checkins_screen.dart';
import 'features/settings/alterar_senha_screen.dart';

Page<T> _slide<T>(Widget child, GoRouterState state) {
  return CustomTransitionPage<T>(
    key: state.pageKey,
    child: child,
    transitionDuration: const Duration(milliseconds: 280),
    reverseTransitionDuration: const Duration(milliseconds: 220),
    transitionsBuilder: (_, animation, secondaryAnimation, child) {
      final slideIn = Tween<Offset>(
        begin: const Offset(0.22, 0),
        end: Offset.zero,
      ).animate(CurvedAnimation(parent: animation, curve: Curves.easeOutCubic));

      final fadeIn = CurvedAnimation(parent: animation, curve: Curves.easeOut);

      final slideOut = Tween<Offset>(
        begin: Offset.zero,
        end: const Offset(-0.12, 0),
      ).animate(CurvedAnimation(parent: secondaryAnimation, curve: Curves.easeInCubic));

      return SlideTransition(
        position: slideOut,
        child: FadeTransition(
          opacity: fadeIn,
          child: SlideTransition(position: slideIn, child: child),
        ),
      );
    },
  );
}

class AppRouter {
  AppRouter._();

  static GoRouter router({
    required Widget loginScreen,
    required Widget homeScreen,
    required Widget settingsScreen,
    required Widget Function(int criancaPessoaId) minhaCriancaDetalheBuilder,
    String tenantSlug = '',
  }) {
    return GoRouter(
      initialLocation: '/',
      redirect: (context, state) {
        final appState = context.read<AppState>();
        final isLoggedIn = appState.user != null;
        final loc = state.matchedLocation;
        final publicRoutes = {'/login', '/cadastro'};

        if (!isLoggedIn && !publicRoutes.contains(loc)) return '/login';
        if (isLoggedIn && publicRoutes.contains(loc)) return '/';
        return null;
      },
      routes: [
        GoRoute(
          path: '/login',
          pageBuilder: (_, state) => _slide(loginScreen, state),
        ),
        GoRoute(
          path: '/cadastro',
          pageBuilder: (_, state) => _slide(CadastroScreen(tenantSlug: tenantSlug), state),
        ),
        GoRoute(
          path: '/',
          pageBuilder: (_, state) => _slide(homeScreen, state),
        ),
        GoRoute(
          path: '/avisos',
          pageBuilder: (_, state) {
            final avisoId = state.uri.queryParameters['avisoId'];
            return _slide(AvisosScreen(initialAvisoId: avisoId), state);
          },
        ),
        GoRoute(
          path: '/configuracao',
          pageBuilder: (_, state) => _slide(settingsScreen, state),
        ),
        GoRoute(
          path: '/alterar-senha',
          pageBuilder: (_, state) => _slide(const AlterarSenhaScreen(), state),
        ),
        GoRoute(
          path: '/historico',
          pageBuilder: (_, state) => _slide(const HistoricoCheckinsScreen(), state),
        ),
        GoRoute(
          path: '/criancas/:id',
          pageBuilder: (_, state) {
            final id = int.tryParse(state.pathParameters['id'] ?? '');
            if (id == null) {
              return _slide(
                const Scaffold(body: Center(child: Text('Criança inválida.'))),
                state,
              );
            }
            return _slide(minhaCriancaDetalheBuilder(id), state);
          },
        ),
      ],
    );
  }
}
