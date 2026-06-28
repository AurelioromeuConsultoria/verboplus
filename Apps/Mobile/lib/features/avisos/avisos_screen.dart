import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../app_state.dart';
import '../../core/app_palette.dart';
import '../../core/cache_service.dart';
import '../../core/connectivity_service.dart';
import '../../core/offline_banner.dart';
import '../../core/shimmer.dart';
import '../kids/kids_repository.dart';

class AvisosScreen extends StatefulWidget {
  const AvisosScreen({super.key, this.initialAvisoId});

  /// Se definido, abre automaticamente o detalhe desse aviso após carregar.
  final String? initialAvisoId;

  @override
  State<AvisosScreen> createState() => _AvisosScreenState();
}

class _AvisosScreenState extends State<AvisosScreen> {
  List<MeuAvisoKidsDto> _avisos = [];
  bool _loading = true;
  bool _somenteNaoLidos = false;
  String _filtroTipo = 'todos';
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final cache = context.read<CacheService>();
    final connectivity = context.read<ConnectivityService>();
    final repo = context.read<KidsRepository>();

    if (_avisos.isEmpty) {
      final cached = await cache.loadAvisos();
      if (cached != null && mounted) {
        setState(() {
          _avisos = cached.map(MeuAvisoKidsDto.fromJson).toList();
          _loading = false;
        });
      }
    }

    if (!connectivity.isOnline) {
      if (mounted && _loading) setState(() { _loading = false; });
      return;
    }

    if (mounted) setState(() { _loading = _avisos.isEmpty; _error = null; });

    try {
      final avisos = await repo.getMeusAvisos(
        naoLidos: _somenteNaoLidos,
        tipo: _filtroTipo == 'todos' ? null : _filtroTipo,
      );
      if (!mounted) return;
      if (!_somenteNaoLidos && _filtroTipo == 'todos') {
        await cache.saveAvisos(avisos.map((a) => a.toJson()).toList());
      }
      setState(() { _avisos = avisos; _loading = false; });
      if (!mounted) return;
      // Atualiza o badge do sino na home
      final naoLidos = avisos.where((a) => !a.foiLido).length;
      context.read<AppState>().setNaoLidosCount(naoLidos);
      // Deep link: abre aviso específico se veio de push notification
      final targetId = widget.initialAvisoId;
      if (targetId != null) {
        final target = avisos.where((a) => a.id.toString() == targetId).firstOrNull;
        if (target != null && mounted) _abrirDetalhe(target);
      }
    } catch (e) {
      if (!mounted) return;
      setState(() {
        if (_avisos.isEmpty) _error = e is KidsApiException ? e.message : e.toString();
        _loading = false;
      });
    }
  }

  Future<void> _abrirDetalhe(MeuAvisoKidsDto aviso) async {
    await showModalBottomSheet<void>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => _AvisoDetalheSheet(aviso: aviso),
    );
    // Marca como lido após fechar o sheet
    if (mounted) _marcarComoLido(aviso);
  }

  Future<void> _marcarComoLido(MeuAvisoKidsDto aviso) async {
    if (aviso.foiLido) return;
    try {
      final atualizado = await context.read<KidsRepository>().marcarAvisoComoLido(aviso.id);
      if (!mounted) return;
      setState(() {
        _avisos = _avisos.map((a) => a.id == atualizado.id ? atualizado : a).toList();
      });
      if (mounted) {
        final naoLidos = _avisos.where((a) => !a.foiLido).length;
        context.read<AppState>().setNaoLidosCount(naoLidos);
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e is KidsApiException ? e.message : 'Erro ao marcar como lido.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final naoLidos = _avisos.where((a) => !a.foiLido).length;

    return Scaffold(
      backgroundColor: AppPalette.bg,
      appBar: AppBar(
        backgroundColor: AppPalette.card,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_rounded),
          onPressed: () {
            if (context.canPop()) {
              context.pop();
            } else {
              context.go('/');
            }
          },
        ),
        title: Row(
          children: [
            const Text('Avisos'),
            if (naoLidos > 0) ...[
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                decoration: BoxDecoration(
                  color: AppPalette.primary,
                  borderRadius: BorderRadius.circular(999),
                ),
                child: Text(
                  '$naoLidos',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 12,
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ),
            ],
          ],
        ),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(1),
          child: Container(height: 1, color: AppPalette.border),
        ),
      ),
      body: OfflineBanner(
        child: RefreshIndicator(
          onRefresh: _load,
          child: Column(
            children: [
              _buildFilterRow(),
              Expanded(child: _buildContent()),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildFilterRow() {
    final tipos = <String, String>{
      'todos': 'Todos',
      'AVISO_GERAL': 'Gerais',
      'AVISO_CRIANCA': 'Da criança',
      'CHECKIN': 'Check-in',
      'CHECKOUT': 'Check-out',
      'ALERTA': 'Alertas',
    };

    return Container(
      color: AppPalette.card,
      padding: const EdgeInsets.fromLTRB(16, 10, 16, 12),
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        child: Row(
          children: [
            _FilterChip(
              label: _somenteNaoLidos ? 'Não lidos' : 'Todos os status',
              selected: _somenteNaoLidos,
              onTap: () {
                setState(() => _somenteNaoLidos = !_somenteNaoLidos);
                _load();
              },
            ),
            const SizedBox(width: 6),
            Container(width: 1, height: 20, color: AppPalette.border),
            const SizedBox(width: 6),
            ...tipos.entries.map((e) => Padding(
              padding: const EdgeInsets.only(right: 6),
              child: _FilterChip(
                label: e.value,
                selected: _filtroTipo == e.key,
                onTap: () {
                  setState(() => _filtroTipo = e.key);
                  _load();
                },
              ),
            )),
          ],
        ),
      ),
    );
  }

  Widget _buildContent() {
    if (_loading) {
      return ListView.separated(
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 32),
        itemCount: 4,
        separatorBuilder: (_, __) => const SizedBox(height: 8),
        itemBuilder: (_, __) => const _SkeletonAvisoCard(),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.cloud_off_rounded, size: 48, color: AppPalette.lightInk),
              const SizedBox(height: 16),
              Text(
                _error!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: AppPalette.midInk),
              ),
              const SizedBox(height: 16),
              FilledButton(onPressed: _load, child: const Text('Tentar novamente')),
            ],
          ),
        ),
      );
    }

    if (_avisos.isEmpty) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('🔔', style: TextStyle(fontSize: 52)),
            const SizedBox(height: 16),
            const Text(
              'Nenhum aviso encontrado',
              style: TextStyle(fontSize: 17, fontWeight: FontWeight.w800, color: AppPalette.ink),
            ),
            const SizedBox(height: 8),
            const Text(
              'Quando a equipe publicar algo,\naparecerá aqui.',
              textAlign: TextAlign.center,
              style: TextStyle(color: AppPalette.midInk, height: 1.5),
            ),
          ],
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 32),
      itemCount: _avisos.length,
      separatorBuilder: (_, __) => const SizedBox(height: 8),
      itemBuilder: (_, i) => _AvisoCard(
        aviso: _avisos[i],
        onTap: () => _abrirDetalhe(_avisos[i]),
      ),
    );
  }
}

// ─── Aviso card ─────────────────────────────────────────────────────────────

class _AvisoCard extends StatelessWidget {
  const _AvisoCard({required this.aviso, required this.onTap});
  final MeuAvisoKidsDto aviso;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final style = _AvisoStyle.from(aviso.tipo);
    final timeStr = _timeLabel(aviso.dataCriacao);
    final isNew = !aviso.foiLido;

    return Material(
      color: isNew ? style.tintBg : AppPalette.card,
      borderRadius: BorderRadius.circular(14),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(14),
        child: Container(
          decoration: BoxDecoration(
            border: Border(
              left: BorderSide(color: style.color, width: 4),
              top: BorderSide(color: isNew ? style.color.withValues(alpha: 0.18) : AppPalette.border),
              right: BorderSide(color: isNew ? style.color.withValues(alpha: 0.18) : AppPalette.border),
              bottom: BorderSide(color: isNew ? style.color.withValues(alpha: 0.18) : AppPalette.border),
            ),
          ),
          padding: const EdgeInsets.fromLTRB(14, 12, 14, 12),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(
                  color: style.color.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Icon(style.icon, size: 18, color: style.color),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            aviso.titulo,
                            style: TextStyle(
                              fontSize: 14,
                              fontWeight: isNew ? FontWeight.w800 : FontWeight.w600,
                              color: AppPalette.ink,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        const SizedBox(width: 8),
                        Text(
                          timeStr,
                          style: const TextStyle(fontSize: 11, color: AppPalette.lightInk),
                        ),
                      ],
                    ),
                    const SizedBox(height: 3),
                    Text(
                      aviso.mensagem,
                      style: const TextStyle(fontSize: 13, color: AppPalette.midInk, height: 1.4),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 6),
                    Wrap(
                      spacing: 6,
                      children: [
                        _MiniTag(label: _tipoLabel(aviso.tipo), color: style.color),
                        if (aviso.criancaNome?.isNotEmpty == true)
                          _MiniTag(label: aviso.criancaNome!, color: AppPalette.primary),
                        if (isNew)
                          const _MiniTag(label: '● Novo', color: AppPalette.success),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  String _timeLabel(DateTime dt) {
    final now = DateTime.now();
    final local = dt.toLocal();
    final diff = now.difference(local);
    if (diff.inMinutes < 60) return '${diff.inMinutes}min';
    if (diff.inHours < 24) return '${diff.inHours}h';
    if (diff.inDays == 1) return 'Ontem';
    return DateFormat('dd/MM').format(local);
  }

  String _tipoLabel(String tipo) {
    switch (tipo) {
      case 'CHECKIN': return 'Check-in';
      case 'CHECKOUT': return 'Check-out';
      case 'ALERTA': return 'Alerta';
      case 'AVISO_CRIANCA': return 'Da criança';
      case 'AVISO_RESPONSAVEL': return 'Ao responsável';
      case 'AVISO_GERAL': return 'Geral';
      default: return tipo;
    }
  }
}

class _MiniTag extends StatelessWidget {
  const _MiniTag({required this.label, required this.color});
  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.10),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(
        label,
        style: TextStyle(fontSize: 11, fontWeight: FontWeight.w700, color: color),
      ),
    );
  }
}

class _FilterChip extends StatelessWidget {
  const _FilterChip({required this.label, required this.selected, required this.onTap});
  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(999),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
        decoration: BoxDecoration(
          color: selected ? AppPalette.primary : AppPalette.divider,
          borderRadius: BorderRadius.circular(999),
        ),
        child: Text(
          label,
          style: TextStyle(
            fontSize: 13,
            fontWeight: FontWeight.w700,
            color: selected ? Colors.white : AppPalette.midInk,
          ),
        ),
      ),
    );
  }
}

// ─── Aviso detalhe sheet ─────────────────────────────────────────────────────

class _AvisoDetalheSheet extends StatelessWidget {
  const _AvisoDetalheSheet({required this.aviso});
  final MeuAvisoKidsDto aviso;

  @override
  Widget build(BuildContext context) {
    final style = _AvisoStyle.from(aviso.tipo);
    final bottom = MediaQuery.viewPaddingOf(context).bottom;

    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: EdgeInsets.fromLTRB(24, 16, 24, 24 + bottom),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Handle
          Center(
            child: Container(
              width: 36,
              height: 4,
              decoration: BoxDecoration(
                color: AppPalette.border,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
          const SizedBox(height: 20),

          // Tipo badge + data
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                decoration: BoxDecoration(
                  color: style.color.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(999),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(style.icon, size: 13, color: style.color),
                    const SizedBox(width: 5),
                    Text(
                      _tipoLabel(aviso.tipo),
                      style: TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                        color: style.color,
                      ),
                    ),
                  ],
                ),
              ),
              const Spacer(),
              Text(
                _dateLabel(aviso.dataCriacao),
                style: const TextStyle(fontSize: 12, color: AppPalette.lightInk),
              ),
            ],
          ),
          const SizedBox(height: 14),

          // Título
          Text(
            aviso.titulo,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w900,
              color: AppPalette.ink,
              letterSpacing: -0.3,
            ),
          ),

          if (aviso.criancaNome?.isNotEmpty == true) ...[
            const SizedBox(height: 6),
            Row(
              children: [
                const Icon(Icons.child_care_rounded, size: 14, color: AppPalette.primary),
                const SizedBox(width: 5),
                Text(
                  aviso.criancaNome!,
                  style: const TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                    color: AppPalette.primary,
                  ),
                ),
              ],
            ),
          ],

          const SizedBox(height: 16),
          const Divider(height: 1),
          const SizedBox(height: 16),

          // Mensagem completa
          ConstrainedBox(
            constraints: BoxConstraints(
              maxHeight: MediaQuery.sizeOf(context).height * 0.45,
            ),
            child: SingleChildScrollView(
              child: Text(
                aviso.mensagem,
                style: const TextStyle(
                  fontSize: 15,
                  color: AppPalette.ink,
                  height: 1.6,
                ),
              ),
            ),
          ),

          const SizedBox(height: 20),
          SizedBox(
            width: double.infinity,
            child: FilledButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Fechar'),
            ),
          ),
        ],
      ),
    );
  }

  String _dateLabel(DateTime dt) {
    final local = dt.toLocal();
    final now = DateTime.now();
    final diff = now.difference(local);
    if (diff.inMinutes < 60) return 'Há ${diff.inMinutes}min';
    if (diff.inHours < 24) return 'Há ${diff.inHours}h';
    if (diff.inDays == 1) return 'Ontem';
    return DateFormat('dd/MM/yyyy HH:mm').format(local);
  }

  String _tipoLabel(String tipo) {
    switch (tipo) {
      case 'CHECKIN': return 'Check-in';
      case 'CHECKOUT': return 'Check-out';
      case 'ALERTA': return 'Alerta';
      case 'AVISO_CRIANCA': return 'Da criança';
      case 'AVISO_RESPONSAVEL': return 'Ao responsável';
      case 'AVISO_GERAL': return 'Geral';
      default: return tipo;
    }
  }
}

// ─── Skeleton ────────────────────────────────────────────────────────────────

class _SkeletonAvisoCard extends StatelessWidget {
  const _SkeletonAvisoCard();

  @override
  Widget build(BuildContext context) {
    return Shimmer(
      child: Container(
        padding: const EdgeInsets.fromLTRB(14, 12, 14, 12),
        decoration: BoxDecoration(
          color: AppPalette.card,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppPalette.border),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SkeletonBox(width: 36, height: 36, borderRadius: 10),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: const [
                  Row(children: [
                    Expanded(child: SkeletonBox(height: 13, borderRadius: 6)),
                    SizedBox(width: 40),
                    SkeletonBox(width: 30, height: 10, borderRadius: 6),
                  ]),
                  SizedBox(height: 7),
                  SkeletonBox(height: 10, borderRadius: 6),
                  SizedBox(height: 5),
                  SkeletonBox(width: 160, height: 10, borderRadius: 6),
                  SizedBox(height: 8),
                  SkeletonBox(width: 60, height: 18, borderRadius: 6),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Aviso style ─────────────────────────────────────────────────────────────

class _AvisoStyle {
  const _AvisoStyle({required this.icon, required this.color, required this.tintBg});
  final IconData icon;
  final Color color;
  final Color tintBg;

  factory _AvisoStyle.from(String tipo) {
    switch (tipo) {
      case 'CHECKIN':
        return const _AvisoStyle(
          icon: Icons.login_rounded,
          color: AppPalette.success,
          tintBg: AppPalette.successBg,
        );
      case 'CHECKOUT':
        return const _AvisoStyle(
          icon: Icons.logout_rounded,
          color: AppPalette.info,
          tintBg: AppPalette.infoBg,
        );
      case 'ALERTA':
        return const _AvisoStyle(
          icon: Icons.warning_rounded,
          color: AppPalette.warning,
          tintBg: AppPalette.warningBg,
        );
      default:
        return const _AvisoStyle(
          icon: Icons.campaign_rounded,
          color: AppPalette.primary,
          tintBg: AppPalette.primarySoft,
        );
    }
  }
}
