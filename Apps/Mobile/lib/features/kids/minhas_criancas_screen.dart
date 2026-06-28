import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../app_state.dart';
import '../../core/app_palette.dart';
import '../../core/cache_service.dart';
import '../../core/connectivity_service.dart';
import '../../core/offline_banner.dart';
import '../../core/push_service.dart';
import '../../core/shimmer.dart';
import 'kids_repository.dart';

class MinhasCriancasScreen extends StatefulWidget {
  const MinhasCriancasScreen({super.key});

  @override
  State<MinhasCriancasScreen> createState() => _MinhasCriancasScreenState();
}

class _MinhasCriancasScreenState extends State<MinhasCriancasScreen> {
  List<MinhaCriancaResumoDto> _criancas = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      if (!mounted) return;
      final push = context.read<PushService>();
      final appState = context.read<AppState>();

      push.onNotificationOpened = (String? avisoId) {
        if (!mounted) return;
        final uri = avisoId != null ? '/avisos?avisoId=$avisoId' : '/avisos';
        GoRouter.of(context).push(uri);
      };

      push.onForegroundMessage = (String? avisoId) {
        if (!mounted) return;
        appState.incrementNaoLidos();
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: const Text('Você tem um novo aviso'),
            action: SnackBarAction(
              label: 'Ver',
              textColor: Colors.white,
              onPressed: () {
                if (!mounted) return;
                final uri = avisoId != null ? '/avisos?avisoId=$avisoId' : '/avisos';
                GoRouter.of(context).push(uri);
              },
            ),
          ),
        );
      };

      push.setupMessageHandlers();

      final avisoIdInicial = await PushService.getInitialAvisoId();
      if (mounted && avisoIdInicial != null) {
        GoRouter.of(context).push('/avisos?avisoId=$avisoIdInicial');
      } else if (await PushService.hadInitialNotification() && mounted) {
        GoRouter.of(context).push('/avisos');
      }
    });
  }

  Future<void> _load() async {
    final cache = context.read<CacheService>();
    final connectivity = context.read<ConnectivityService>();
    final repo = context.read<KidsRepository>();

    if (_criancas.isEmpty) {
      final cached = await cache.loadCriancas();
      if (cached != null && mounted) {
        setState(() {
          _criancas = cached.map(MinhaCriancaResumoDto.fromJson).toList();
          _loading = false;
        });
      }
    }

    if (!connectivity.isOnline) {
      if (mounted && _loading) setState(() { _loading = false; });
      return;
    }

    if (mounted) setState(() { _loading = _criancas.isEmpty; _error = null; });

    try {
      final criancas = await repo.getMinhasCriancas();
      if (!mounted) return;
      await cache.saveCriancas(criancas.map((c) => c.toJson()).toList());
      setState(() { _criancas = criancas; _loading = false; });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        if (_criancas.isEmpty) _error = e is KidsApiException ? e.message : e.toString();
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final appState = context.watch<AppState>();
    final user = appState.user;
    final naoLidos = appState.naoLidosCount;
    final primeiroNome = user?.nome.trim().split(' ').first ?? '';
    final checkedIn = _criancas.where((c) => c.estaCheckedIn).length;

    return Scaffold(
      backgroundColor: AppPalette.bg,
      body: SafeArea(
        child: OfflineBanner(
          child: RefreshIndicator(
            onRefresh: _load,
            child: ListView(
              padding: const EdgeInsets.fromLTRB(16, 20, 16, 32),
              children: [
                // ── Header ──────────────────────────────────────────
                Row(
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text.rich(
                            TextSpan(
                              style: const TextStyle(
                                fontSize: 26,
                                fontWeight: FontWeight.w900,
                                color: AppPalette.ink,
                                letterSpacing: -0.5,
                              ),
                              children: [
                                const TextSpan(text: 'Olá, '),
                                if (primeiroNome.isNotEmpty)
                                  TextSpan(
                                    text: primeiroNome,
                                    style: const TextStyle(color: AppPalette.primary),
                                  )
                                else
                                  const TextSpan(text: 'família'),
                                const TextSpan(text: '! 👋'),
                              ],
                            ),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            DateFormat("EEEE, d 'de' MMMM", 'pt_BR').format(DateTime.now()),
                            style: const TextStyle(
                              fontSize: 13,
                              color: AppPalette.midInk,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Stack(
                      clipBehavior: Clip.none,
                      children: [
                        _HeaderIconBtn(
                          icon: Icons.notifications_outlined,
                          onTap: () => context.push('/avisos'),
                          color: AppPalette.primary,
                          bg: AppPalette.primarySoft,
                        ),
                        if (naoLidos > 0)
                          Positioned(
                            top: -4,
                            right: -4,
                            child: Container(
                              padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 2),
                              decoration: BoxDecoration(
                                color: AppPalette.danger,
                                borderRadius: BorderRadius.circular(999),
                                border: Border.all(color: Colors.white, width: 1.5),
                              ),
                              child: Text(
                                naoLidos > 99 ? '99+' : '$naoLidos',
                                style: const TextStyle(
                                  fontSize: 9,
                                  fontWeight: FontWeight.w800,
                                  color: Colors.white,
                                ),
                              ),
                            ),
                          ),
                      ],
                    ),
                    const SizedBox(width: 8),
                    _HeaderIconBtn(
                      icon: Icons.person_outline_rounded,
                      onTap: () => context.push('/configuracao'),
                      color: AppPalette.midInk,
                      bg: AppPalette.divider,
                    ),
                  ],
                ),

                const SizedBox(height: 20),

                // ── Status pills ─────────────────────────────────────
                if (!_loading && _criancas.isNotEmpty) ...[
                  Wrap(
                    spacing: 8,
                    children: [
                      _Pill(
                        label: '${_criancas.length} ${_criancas.length == 1 ? 'filho' : 'filhos'}',
                        icon: Icons.child_care_rounded,
                        color: AppPalette.primary,
                        bg: AppPalette.primarySoft,
                      ),
                      if (checkedIn > 0)
                        _Pill(
                          label: '$checkedIn em check-in',
                          icon: Icons.check_circle_rounded,
                          color: AppPalette.success,
                          bg: AppPalette.successBg,
                        )
                      else
                        _Pill(
                          label: 'Aguardando chegada',
                          icon: Icons.schedule_rounded,
                          color: AppPalette.midInk,
                          bg: AppPalette.divider,
                        ),
                    ],
                  ),
                  const SizedBox(height: 24),
                ],

                // ── Content ─────────────────────────────────────────
                if (_loading)
                  const _SkeletonList()
                else if (_error != null)
                  _ErrorCard(message: _error!, onRetry: _load)
                else if (_criancas.isEmpty)
                  const _EmptyCard()
                else ...[
                  const Text(
                    'SEUS FILHOS',
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w800,
                      color: AppPalette.lightInk,
                      letterSpacing: 1.2,
                    ),
                  ),
                  const SizedBox(height: 10),
                  ..._criancas.map(
                    (c) => Padding(
                      padding: const EdgeInsets.only(bottom: 10),
                      child: _KidCard(crianca: c),
                    ),
                  ),
                  const SizedBox(height: 8),
                  OutlinedButton.icon(
                    onPressed: () => context.push('/avisos'),
                    icon: const Icon(Icons.campaign_outlined, size: 18),
                    label: const Text('Ver avisos e comunicados'),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}

// ─── Child card ────────────────────────────────────────────────────────────────

class _KidCard extends StatelessWidget {
  const _KidCard({required this.crianca});
  final MinhaCriancaResumoDto crianca;

  @override
  Widget build(BuildContext context) {
    final color = AppPalette.kidColor(crianca.pessoaId);
    final meta = _meta();

    return Material(
      color: AppPalette.card,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        onTap: () => context.push('/criancas/${crianca.pessoaId}'),
        borderRadius: BorderRadius.circular(16),
        child: Container(
          decoration: BoxDecoration(
            border: Border(
              left: BorderSide(color: color, width: 5),
              top: const BorderSide(color: AppPalette.border),
              right: const BorderSide(color: AppPalette.border),
              bottom: const BorderSide(color: AppPalette.border),
            ),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
          child: Row(
            children: [
              _ChildAvatar(
                nome: crianca.nome,
                fotoUrl: crianca.fotoUrl,
                color: color,
                size: 50,
                fontSize: 20,
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      crianca.nome,
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w800,
                        color: AppPalette.ink,
                      ),
                    ),
                    if (meta.isNotEmpty) ...[
                      const SizedBox(height: 2),
                      Text(
                        meta,
                        style: const TextStyle(fontSize: 12, color: AppPalette.midInk),
                      ),
                    ],
                    const SizedBox(height: 7),
                    _StatusDot(checkedIn: crianca.estaCheckedIn),
                  ],
                ),
              ),
              if (crianca.temAlertaCritico) ...[
                Container(
                  width: 28,
                  height: 28,
                  decoration: const BoxDecoration(
                    color: AppPalette.warningBg,
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.warning_rounded, size: 16, color: AppPalette.warning),
                ),
                const SizedBox(width: 8),
              ],
              const Icon(Icons.chevron_right_rounded, color: AppPalette.lightInk, size: 20),
            ],
          ),
        ),
      ),
    );
  }

  String _meta() {
    final parts = <String>[];
    if (crianca.dataNascimento != null) parts.add(_idadeFormatada(crianca.dataNascimento!));
    if (crianca.salaId?.isNotEmpty == true) parts.add(crianca.salaId!);
    return parts.join(' · ');
  }

  String _idadeFormatada(DateTime nascimento) {
    final today = DateTime.now();
    var years = today.year - nascimento.year;
    var months = today.month - nascimento.month;
    if (today.day < nascimento.day) months--;
    if (months < 0) { years--; months += 12; }
    if (years > 0) return years == 1 ? '1 ano' : '$years anos';
    if (months <= 0) return 'Menos de 1 mês';
    return months == 1 ? '1 mês' : '$months meses';
  }
}

// ─── Shared small widgets ───────────────────────────────────────────────────────

class _ChildAvatar extends StatelessWidget {
  const _ChildAvatar({
    required this.nome,
    required this.color,
    required this.size,
    required this.fontSize,
    this.fotoUrl,
  });

  final String nome;
  final String? fotoUrl;
  final Color color;
  final double size;
  final double fontSize;

  @override
  Widget build(BuildContext context) {
    final initial = nome.isNotEmpty ? nome[0].toUpperCase() : '?';
    if (fotoUrl != null && fotoUrl!.isNotEmpty) {
      return ClipOval(
        child: Image.network(
          fotoUrl!,
          width: size,
          height: size,
          fit: BoxFit.cover,
          errorBuilder: (_, __, ___) => _initial(initial),
        ),
      );
    }
    return _initial(initial);
  }

  Widget _initial(String initial) => Container(
        width: size,
        height: size,
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.14),
          shape: BoxShape.circle,
        ),
        child: Center(
          child: Text(
            initial,
            style: TextStyle(
              fontSize: fontSize,
              fontWeight: FontWeight.w900,
              color: color,
            ),
          ),
        ),
      );
}

class _StatusDot extends StatelessWidget {
  const _StatusDot({required this.checkedIn});
  final bool checkedIn;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 7,
          height: 7,
          decoration: BoxDecoration(
            color: checkedIn ? AppPalette.success : AppPalette.lightInk,
            shape: BoxShape.circle,
          ),
        ),
        const SizedBox(width: 6),
        Text(
          checkedIn ? 'Em check-in' : 'Aguardando',
          style: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight.w700,
            color: checkedIn ? AppPalette.success : AppPalette.midInk,
          ),
        ),
      ],
    );
  }
}

class _Pill extends StatelessWidget {
  const _Pill({
    required this.label,
    required this.icon,
    required this.color,
    required this.bg,
  });

  final String label;
  final IconData icon;
  final Color color;
  final Color bg;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
      decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(999)),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 13, color: color),
          const SizedBox(width: 5),
          Text(label, style: TextStyle(fontSize: 12, fontWeight: FontWeight.w700, color: color)),
        ],
      ),
    );
  }
}

class _HeaderIconBtn extends StatelessWidget {
  const _HeaderIconBtn({
    required this.icon,
    required this.onTap,
    required this.color,
    required this.bg,
  });

  final IconData icon;
  final VoidCallback onTap;
  final Color color;
  final Color bg;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        width: 40,
        height: 40,
        decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(12)),
        child: Icon(icon, color: color, size: 20),
      ),
    );
  }
}

class _ErrorCard extends StatelessWidget {
  const _ErrorCard({required this.message, required this.onRetry});
  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: AppPalette.dangerBg,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppPalette.danger.withValues(alpha: 0.2)),
      ),
      child: Column(
        children: [
          const Icon(Icons.cloud_off_rounded, color: AppPalette.danger, size: 36),
          const SizedBox(height: 12),
          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppPalette.danger, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 14),
          TextButton(
            onPressed: onRetry,
            child: const Text('Tentar novamente'),
          ),
        ],
      ),
    );
  }
}

// ─── Skeleton ───────────────────────────────────────────────────────────────────

class _SkeletonList extends StatelessWidget {
  const _SkeletonList();

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: const [
        SkeletonBox(width: 80, height: 11, borderRadius: 6),
        SizedBox(height: 10),
        _SkeletonKidCard(),
        _SkeletonKidCard(),
      ],
    );
  }
}

class _SkeletonKidCard extends StatelessWidget {
  const _SkeletonKidCard();

  @override
  Widget build(BuildContext context) {
    return Shimmer(
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
        decoration: BoxDecoration(
          color: AppPalette.card,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: AppPalette.border),
        ),
        child: Row(
          children: [
            const SkeletonBox(width: 50, height: 50, borderRadius: 25),
            const SizedBox(width: 14),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: const [
                  SkeletonBox(width: 130, height: 14, borderRadius: 6),
                  SizedBox(height: 7),
                  SkeletonBox(width: 90, height: 10, borderRadius: 6),
                  SizedBox(height: 8),
                  SkeletonBox(width: 70, height: 10, borderRadius: 6),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _EmptyCard extends StatelessWidget {
  const _EmptyCard();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 24),
      decoration: BoxDecoration(
        color: AppPalette.card,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppPalette.border),
      ),
      child: const Column(
        children: [
          Text('👨‍👩‍👧‍👦', style: TextStyle(fontSize: 52)),
          SizedBox(height: 16),
          Text(
            'Nenhum filho vinculado',
            style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800, color: AppPalette.ink),
          ),
          SizedBox(height: 8),
          Text(
            'Peça à equipe do Kids para vincular seu\ncadastro de responsável.',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 14, color: AppPalette.midInk, height: 1.5),
          ),
        ],
      ),
    );
  }
}
