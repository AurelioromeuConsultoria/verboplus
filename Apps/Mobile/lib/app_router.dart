import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import 'app_state.dart';

class AppRouter {
  AppRouter._();

  static GoRouter router({
    required Widget loginScreen,
    required Widget homeScreen,
    required Widget avisosScreen,
  }) {
    return GoRouter(
      initialLocation: '/',
      redirect: (context, state) {
        final appState = context.read<AppState>();
        final isLoggedIn = appState.user != null;
        final isLoginRoute = state.matchedLocation == '/login';

        if (!isLoggedIn && !isLoginRoute) return '/login';
        if (isLoggedIn && isLoginRoute) return '/';
        return null;
      },
      routes: [
        GoRoute(
          path: '/login',
          builder: (_, __) => loginScreen,
        ),
        GoRoute(
          path: '/',
          builder: (_, __) => homeScreen,
        ),
        GoRoute(
          path: '/avisos',
          builder: (_, __) => avisosScreen,
        ),
      ],
    );
  }
}
