import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:provider/provider.dart';
import '../../app_state.dart';
import '../../core/app_meta.dart';
import '../../core/app_palette.dart';
import '../../core/auth_repository.dart';
import '../../core/push_service.dart';

class SettingsScreen extends StatelessWidget {
  const SettingsScreen({super.key});

  Future<void> _logout(BuildContext context) async {
    final authRepository = context.read<AuthRepository>();
    final push = context.read<PushService>();
    final appState = context.read<AppState>();
    await push.unregisterToken();
    await authRepository.logout();
    appState.setUser(null);
    if (!context.mounted) return;
    context.go('/login');
  }

  @override
  Widget build(BuildContext context) {
    final user = context.watch<AppState>().user;
    final initial = user?.nome.isNotEmpty == true ? user!.nome[0].toUpperCase() : '?';

    return Scaffold(
      backgroundColor: AppPalette.bg,
      appBar: AppBar(
        title: const Text('Configurações'),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(1),
          child: Container(height: 1, color: AppPalette.border),
        ),
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
        children: [
          // ── User avatar + info ────────────────────────────────────
          if (user != null) ...[
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: AppPalette.card,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(color: AppPalette.border),
              ),
              child: Row(
                children: [
                  Container(
                    width: 56,
                    height: 56,
                    decoration: BoxDecoration(
                      color: AppPalette.primarySoft,
                      shape: BoxShape.circle,
                    ),
                    child: Center(
                      child: Text(
                        initial,
                        style: const TextStyle(
                          fontSize: 22,
                          fontWeight: FontWeight.w900,
                          color: AppPalette.primary,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          user.nome,
                          style: const TextStyle(
                            fontSize: 17,
                            fontWeight: FontWeight.w800,
                            color: AppPalette.ink,
                          ),
                        ),
                        const SizedBox(height: 3),
                        Text(
                          user.email,
                          style: const TextStyle(fontSize: 13, color: AppPalette.midInk),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),
          ],

          // ── Conta ───────────────────────────────────────────────
          const Text(
            'CONTA',
            style: TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w800,
              color: AppPalette.lightInk,
              letterSpacing: 1.2,
            ),
          ),
          const SizedBox(height: 8),
          Container(
            decoration: BoxDecoration(
              color: AppPalette.card,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: AppPalette.border),
            ),
            child: Column(
              children: [
                _ListTileButton(
                  icon: Icons.lock_outline_rounded,
                  iconColor: AppPalette.primary,
                  title: 'Alterar senha',
                  onTap: () => context.push('/alterar-senha'),
                  isFirst: true,
                ),
                const Divider(height: 1, indent: 16),
                _ListTileButton(
                  icon: Icons.history_rounded,
                  iconColor: AppPalette.primary,
                  title: 'Histórico de presenças',
                  onTap: () => context.push('/historico'),
                  isLast: true,
                ),
              ],
            ),
          ),

          const SizedBox(height: 20),

          // ── App info ────────────────────────────────────────────
          const Text(
            'APLICATIVO',
            style: TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w800,
              color: AppPalette.lightInk,
              letterSpacing: 1.2,
            ),
          ),
          const SizedBox(height: 8),
          Container(
            decoration: BoxDecoration(
              color: AppPalette.card,
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: AppPalette.border),
            ),
            child: Column(
              children: [
                _ListTile(
                  icon: Icons.info_outline_rounded,
                  iconColor: AppPalette.primary,
                  title: 'Versão',
                  trailing: Text(
                    AppMeta.version,
                    style: const TextStyle(fontSize: 13, color: AppPalette.midInk, fontWeight: FontWeight.w600),
                  ),
                  isFirst: true,
                  isLast: true,
                ),
              ],
            ),
          ),

          const SizedBox(height: 32),

          // ── Logout ──────────────────────────────────────────────
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: () => _logout(context),
              icon: const Icon(Icons.logout_rounded),
              label: const Text('Sair da conta'),
              style: OutlinedButton.styleFrom(
                foregroundColor: AppPalette.danger,
                side: const BorderSide(color: AppPalette.danger),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ListTileButton extends StatelessWidget {
  const _ListTileButton({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.onTap,
    this.isFirst = false,
    this.isLast = false,
  });

  final IconData icon;
  final Color iconColor;
  final String title;
  final VoidCallback onTap;
  final bool isFirst;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.vertical(
        top: isFirst ? const Radius.circular(16) : Radius.zero,
        bottom: isLast ? const Radius.circular(16) : Radius.zero,
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          children: [
            Container(
              width: 36, height: 36,
              decoration: BoxDecoration(
                color: iconColor.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(icon, size: 18, color: iconColor),
            ),
            const SizedBox(width: 14),
            Expanded(
              child: Text(
                title,
                style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w600, color: AppPalette.ink),
              ),
            ),
            const Icon(Icons.chevron_right_rounded, size: 20, color: AppPalette.lightInk),
          ],
        ),
      ),
    );
  }
}

class _ListTile extends StatelessWidget {
  const _ListTile({
    required this.icon,
    required this.iconColor,
    required this.title,
    this.trailing,
    required this.isFirst,
    required this.isLast,
  });

  final IconData icon;
  final Color iconColor;
  final String title;
  final Widget? trailing;
  final bool isFirst;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      child: Row(
        children: [
          Container(
            width: 36,
            height: 36,
            decoration: BoxDecoration(
              color: iconColor.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(icon, size: 18, color: iconColor),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Text(
              title,
              style: const TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.w600,
                color: AppPalette.ink,
              ),
            ),
          ),
          if (trailing != null) trailing!,
        ],
      ),
    );
  }
}
