import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../app_state.dart';
import '../../core/app_palette.dart';
import '../../core/offline_banner.dart';
import '../../core/shimmer.dart';
import 'kids_repository.dart';

class MinhaCriancaDetalheScreen extends StatefulWidget {
  const MinhaCriancaDetalheScreen({super.key, required this.criancaPessoaId});
  final int criancaPessoaId;

  @override
  State<MinhaCriancaDetalheScreen> createState() => _MinhaCriancaDetalheScreenState();
}

class _MinhaCriancaDetalheScreenState extends State<MinhaCriancaDetalheScreen> {
  MinhaCriancaDetalheDto? _detalhe;
  KidsPreCheckinDto? _preCheckinAtivo;
  List<MeuConteudoAulaDto> _conteudosRecentes = const [];
  bool _loading = true;
  bool _creatingPreCheckin = false;
  bool _cancellingPreCheckin = false;
  bool _confirmingRetirada = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final repo = context.read<KidsRepository>();
      final detalhe = await repo.getMinhaCriancaById(widget.criancaPessoaId);
      final preCheckins = await repo.getMeusPreCheckins(somenteAtivos: true);
      final conteudos = await repo.getMeuConteudoPorCrianca(widget.criancaPessoaId, limit: 5);
      if (!mounted) return;
      if (detalhe == null) {
        setState(() { _error = 'Criança não encontrada.'; _loading = false; });
        return;
      }
      setState(() {
        _detalhe = detalhe;
        _preCheckinAtivo = _findPreCheckinAtivo(preCheckins);
        _conteudosRecentes = conteudos;
        _loading = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e is KidsApiException ? e.message : e.toString();
        _loading = false;
      });
    }
  }

  Future<void> _criarPreCheckin() async {
    setState(() { _creatingPreCheckin = true; });
    try {
      final criado = await context.read<KidsRepository>().criarMeuPreCheckin(
        criancaPessoaId: widget.criancaPessoaId,
      );
      if (!mounted) return;
      HapticFeedback.mediumImpact();
      setState(() { _preCheckinAtivo = criado; });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Pré-check-in gerado com sucesso.')),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e is KidsApiException ? e.message : e.toString())),
      );
    } finally {
      if (mounted) setState(() { _creatingPreCheckin = false; });
    }
  }

  Future<void> _cancelarPreCheckin() async {
    final ativo = _preCheckinAtivo;
    if (ativo == null) return;
    setState(() { _cancellingPreCheckin = true; });
    try {
      await context.read<KidsRepository>().cancelarMeuPreCheckin(
        ativo.id,
        motivo: 'Cancelado pelo responsável no AppKids.',
      );
      if (!mounted) return;
      HapticFeedback.lightImpact();
      setState(() { _preCheckinAtivo = null; });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Pré-check-in cancelado.')),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e is KidsApiException ? e.message : e.toString())),
      );
    } finally {
      if (mounted) setState(() { _cancellingPreCheckin = false; });
    }
  }

  Future<void> _confirmarRetirada(MeuCheckinResumoDto checkin) async {
    final user = context.read<AppState>().user;
    if (user == null) return;

    final confirmed = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => _RetiradaConfirmacaoSheet(
        nomeCrianca: _detalhe!.nome,
        pin: checkin.pinRetirada,
        token: checkin.tokenRetirada,
      ),
    );

    if (confirmed != true || !mounted) return;
    setState(() { _confirmingRetirada = true; });

    try {
      final pin = checkin.pinRetirada;
      final token = checkin.tokenRetirada;
      final metodo = (pin != null && pin.isNotEmpty) ? 'PIN' : 'Token';
      await context.read<KidsRepository>().confirmarRetirada(
        checkinId: checkin.id,
        responsavelPessoaId: user.pessoaId,
        metodo: metodo,
        pin: metodo == 'PIN' ? pin : null,
        token: metodo == 'Token' ? token : null,
      );
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Retirada confirmada com sucesso!')),
      );
      _load();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e is KidsApiException ? e.message : e.toString())),
      );
    } finally {
      if (mounted) setState(() { _confirmingRetirada = false; });
    }
  }

  KidsPreCheckinDto? _findPreCheckinAtivo(List<KidsPreCheckinDto> items) {
    for (final item in items) {
      if (item.criancaPessoaId == widget.criancaPessoaId && item.isAtivo) return item;
    }
    return null;
  }

  bool _hasImportantInfo(MinhaCriancaDetalheDto d) =>
      (d.alergias?.isNotEmpty ?? false) ||
      (d.restricoesAlimentares?.isNotEmpty ?? false) ||
      (d.observacoesVisiveisAoResponsavel?.isNotEmpty ?? false);

  // ─── Build ─────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    if (_loading && _detalhe == null) {
      return const Scaffold(
        backgroundColor: AppPalette.bg,
        body: _SkeletonDetalhe(),
      );
    }

    if (_error != null && _detalhe == null) {
      return Scaffold(
        backgroundColor: AppPalette.bg,
        appBar: AppBar(backgroundColor: AppPalette.bg),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(32),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.error_outline, size: 48, color: AppPalette.lightInk),
                const SizedBox(height: 16),
                Text(_error!, textAlign: TextAlign.center,
                    style: const TextStyle(color: AppPalette.midInk)),
                const SizedBox(height: 16),
                FilledButton(onPressed: _load, child: const Text('Tentar novamente')),
              ],
            ),
          ),
        ),
      );
    }

    final detalhe = _detalhe!;
    final kidColor = AppPalette.kidColor(detalhe.pessoaId);

    return Scaffold(
      backgroundColor: AppPalette.bg,
      appBar: AppBar(
        backgroundColor: AppPalette.bg,
        leading: IconButton(
          icon: Icon(Icons.arrow_back_rounded, color: kidColor),
          onPressed: () => Navigator.of(context).pop(),
        ),
        titleSpacing: 0,
        title: Text(
          detalhe.nome,
          style: const TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.w800,
            color: AppPalette.ink,
            letterSpacing: -0.2,
          ),
        ),
      ),
      body: OfflineBanner(
        child: RefreshIndicator(
          onRefresh: _load,
          child: ListView(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 32),
            children: [
              // ── Perfil compacto ──────────────────────────────────
              _ChildProfile(
                detalhe: detalhe,
                preCheckinAtivo: _preCheckinAtivo,
                color: kidColor,
              ),
              const SizedBox(height: 12),
              // ── Retirada (em check-in) ───────────────────────────
              if (detalhe.estaCheckedIn && detalhe.checkinAtual != null) ...[
                _RetiradaCard(
                  checkin: detalhe.checkinAtual!,
                  confirming: _confirmingRetirada,
                  onConfirmar: () => _confirmarRetirada(detalhe.checkinAtual!),
                ),
                const SizedBox(height: 12),
              ],
              // ── Pré-check-in (aguardando) ────────────────────────
              if (!detalhe.estaCheckedIn) ...[
                _PreCheckinCard(
                  preCheckinAtivo: _preCheckinAtivo,
                  creating: _creatingPreCheckin,
                  cancelling: _cancellingPreCheckin,
                  onCreate: _criarPreCheckin,
                  onCancel: _cancelarPreCheckin,
                ),
                const SizedBox(height: 12),
              ],
              // ── Informações importantes ──────────────────────────
              if (_hasImportantInfo(detalhe)) ...[
                _ImportantInfoCard(detalhe: detalhe),
                const SizedBox(height: 12),
              ],
              // ── Histórico ────────────────────────────────────────
              _HistoryCard(historico: detalhe.historicoRecente),
              const SizedBox(height: 12),
              // ── Conteúdo de aula ─────────────────────────────────
              _ConteudoCard(
                conteudos: _conteudosRecentes,
                resolveUrl: context.read<KidsRepository>().resolveAbsoluteUrl,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Perfil compacto ─────────────────────────────────────────────────────────

class _ChildProfile extends StatelessWidget {
  const _ChildProfile({
    required this.detalhe,
    required this.preCheckinAtivo,
    required this.color,
  });

  final MinhaCriancaDetalheDto detalhe;
  final KidsPreCheckinDto? preCheckinAtivo;
  final Color color;

  @override
  Widget build(BuildContext context) {
    final initial = detalhe.nome.isNotEmpty ? detalhe.nome[0].toUpperCase() : '?';
    final age = detalhe.dataNascimento != null ? _ageText(detalhe.dataNascimento!) : null;
    final meta = [
      if (age != null) age,
      if (detalhe.salaId?.isNotEmpty == true) detalhe.salaId!,
    ].join(' · ');

    final statusLabel = detalhe.estaCheckedIn
        ? 'Em check-in'
        : preCheckinAtivo != null
            ? 'Pré-check-in ativo'
            : 'Aguardando';
    final statusColor = detalhe.estaCheckedIn
        ? AppPalette.success
        : preCheckinAtivo != null
            ? AppPalette.primary
            : AppPalette.midInk;

    return Material(
      color: AppPalette.card,
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
            // Avatar
            _ProfileAvatar(
              fotoUrl: detalhe.fotoUrl,
              initial: initial,
              color: color,
            ),
            const SizedBox(width: 14),
            // Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (meta.isNotEmpty) ...[
                    Text(
                      meta,
                      style: const TextStyle(fontSize: 12, color: AppPalette.midInk),
                    ),
                    const SizedBox(height: 5),
                  ],
                  Row(
                    children: [
                      Container(
                        width: 7,
                        height: 7,
                        decoration: BoxDecoration(color: statusColor, shape: BoxShape.circle),
                      ),
                      const SizedBox(width: 6),
                      Text(
                        statusLabel,
                        style: TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.w700,
                          color: statusColor,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  static String _ageText(DateTime nascimento) {
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

class _ProfileAvatar extends StatelessWidget {
  const _ProfileAvatar({required this.initial, required this.color, this.fotoUrl});
  final String initial;
  final Color color;
  final String? fotoUrl;

  @override
  Widget build(BuildContext context) {
    if (fotoUrl != null && fotoUrl!.isNotEmpty) {
      return ClipOval(
        child: Image.network(
          fotoUrl!,
          width: 52,
          height: 52,
          fit: BoxFit.cover,
          errorBuilder: (_, __, ___) => _circle(),
        ),
      );
    }
    return _circle();
  }

  Widget _circle() => Container(
        width: 52,
        height: 52,
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.13),
          shape: BoxShape.circle,
          border: Border.all(color: color.withValues(alpha: 0.25), width: 2),
        ),
        child: Center(
          child: Text(
            initial,
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w900,
              color: color,
            ),
          ),
        ),
      );
}

// ─── Section label ───────────────────────────────────────────────────────────

class _SectionLabel extends StatelessWidget {
  const _SectionLabel(this.text);
  final String text;

  @override
  Widget build(BuildContext context) {
    return Text(
      text.toUpperCase(),
      style: const TextStyle(
        fontSize: 11,
        fontWeight: FontWeight.w800,
        color: AppPalette.lightInk,
        letterSpacing: 1.0,
      ),
    );
  }
}

// ─── Credential tile ─────────────────────────────────────────────────────────

class _CredTile extends StatelessWidget {
  const _CredTile({required this.label, required this.value});
  final String label;
  final String? value;

  @override
  Widget build(BuildContext context) {
    final display = value?.isNotEmpty == true ? value! : '—';
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppPalette.divider,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: const TextStyle(
                    fontSize: 11,
                    fontWeight: FontWeight.w700,
                    color: AppPalette.lightInk,
                    letterSpacing: 0.5,
                  ),
                ),
                const SizedBox(height: 4),
                SelectableText(
                  display,
                  style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w900,
                    color: AppPalette.ink,
                    letterSpacing: 1,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            onPressed: value == null || value!.isEmpty
                ? null
                : () async {
                    await Clipboard.setData(ClipboardData(text: value!));
                    if (!context.mounted) return;
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text('$label copiado.')),
                    );
                  },
            icon: const Icon(Icons.copy_rounded, size: 18),
            color: AppPalette.midInk,
          ),
        ],
      ),
    );
  }
}

// ─── Info banner ─────────────────────────────────────────────────────────────

class _InfoBox extends StatelessWidget {
  const _InfoBox({required this.icon, required this.title, required this.body, required this.color});
  final IconData icon;
  final String title;
  final String body;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withValues(alpha: 0.18)),
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: color, size: 18),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: TextStyle(fontSize: 12, fontWeight: FontWeight.w800, color: color)),
                const SizedBox(height: 3),
                Text(body, style: const TextStyle(fontSize: 13, color: AppPalette.ink, height: 1.4)),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Card wrapper ────────────────────────────────────────────────────────────

class _SectionCard extends StatelessWidget {
  const _SectionCard({required this.label, required this.child, this.accent});
  final String label;
  final Widget child;
  final Color? accent;

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: AppPalette.card,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppPalette.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (accent != null)
            Container(
              height: 4,
              decoration: BoxDecoration(
                color: accent,
                borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
              ),
            ),
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _SectionLabel(label),
                const SizedBox(height: 10),
                child,
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Retirada card ───────────────────────────────────────────────────────────

class _RetiradaCard extends StatelessWidget {
  const _RetiradaCard({
    required this.checkin,
    required this.confirming,
    required this.onConfirmar,
  });

  final MeuCheckinResumoDto checkin;
  final bool confirming;
  final VoidCallback onConfirmar;

  @override
  Widget build(BuildContext context) {
    return _SectionCard(
      label: 'Retirada segura',
      accent: AppPalette.success,
      child: Column(
        children: [
          Row(
            children: [
              Expanded(child: _CredTile(label: 'PIN', value: checkin.pinRetirada)),
              const SizedBox(width: 10),
              Expanded(child: _CredTile(label: 'TOKEN', value: checkin.tokenRetirada)),
            ],
          ),
          if (checkin.tokenRetiradaExpiraEm != null) ...[
            const SizedBox(height: 10),
            _InfoBox(
              icon: Icons.schedule_rounded,
              title: 'Válido até',
              body: DateFormat('dd/MM/yyyy HH:mm').format(checkin.tokenRetiradaExpiraEm!.toLocal()),
              color: AppPalette.info,
            ),
          ],
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            child: FilledButton.icon(
              onPressed: confirming ? null : onConfirmar,
              icon: confirming
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                  : const Icon(Icons.exit_to_app_rounded),
              label: Text(confirming ? 'Confirmando...' : 'Confirmar minha retirada'),
              style: FilledButton.styleFrom(backgroundColor: AppPalette.success),
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Pre-check-in card ───────────────────────────────────────────────────────

class _PreCheckinCard extends StatelessWidget {
  const _PreCheckinCard({
    required this.preCheckinAtivo,
    required this.creating,
    required this.cancelling,
    required this.onCreate,
    required this.onCancel,
  });

  final KidsPreCheckinDto? preCheckinAtivo;
  final bool creating;
  final bool cancelling;
  final Future<void> Function() onCreate;
  final Future<void> Function() onCancel;

  @override
  Widget build(BuildContext context) {
    return preCheckinAtivo == null ? _buildEmpty() : _buildTicket(context);
  }

  // ── Estado vazio ──────────────────────────────────────────────────────────

  Widget _buildEmpty() {
    return Container(
      decoration: BoxDecoration(
        border: Border.all(color: AppPalette.border),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header sutil
          Container(
            padding: const EdgeInsets.fromLTRB(18, 14, 18, 14),
            decoration: const BoxDecoration(
              color: AppPalette.primarySoft,
              borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
            ),
            child: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: AppPalette.primary.withValues(alpha: 0.14),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(Icons.qr_code_2_rounded, color: AppPalette.primary, size: 20),
                ),
                const SizedBox(width: 12),
                const Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Pré-check-in',
                        style: TextStyle(fontSize: 15, fontWeight: FontWeight.w800, color: AppPalette.primary),
                      ),
                      Text(
                        'Gere antes de sair de casa',
                        style: TextStyle(fontSize: 12, color: AppPalette.midInk),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          // Botão
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 14, 16, 16),
            child: SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                onPressed: creating ? null : onCreate,
                icon: creating
                    ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                    : const Icon(Icons.qr_code_scanner_rounded),
                label: Text(creating ? 'Gerando...' : 'Gerar pré-check-in'),
              ),
            ),
          ),
        ],
      ),
    );
  }

  // ── Ticket ativo ──────────────────────────────────────────────────────────

  Widget _buildTicket(BuildContext context) {
    final codigo = preCheckinAtivo!.codigoCurto;
    final expira = DateFormat("dd/MM 'às' HH:mm").format(preCheckinAtivo!.expiraEm.toLocal());

    return Column(
      children: [
        // Ticket card
        Container(
          decoration: BoxDecoration(
            color: AppPalette.card,
            borderRadius: BorderRadius.circular(20),
            border: Border.all(color: AppPalette.border),
            boxShadow: const [
              BoxShadow(color: Color(0x0A000000), blurRadius: 16, offset: Offset(0, 4)),
            ],
          ),
          clipBehavior: Clip.hardEdge,
          child: Column(
            children: [
              // ── Header colorido ──────────────────────────
              Container(
                padding: const EdgeInsets.fromLTRB(18, 14, 18, 14),
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [AppPalette.primary, Color.lerp(AppPalette.primary, const Color(0xFF3A1A8A), 0.4)!],
                  ),
                ),
                child: Row(
                  children: [
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            'PRÉ-CHECK-IN ATIVO',
                            style: TextStyle(
                              fontSize: 10,
                              fontWeight: FontWeight.w800,
                              color: Colors.white60,
                              letterSpacing: 1.1,
                            ),
                          ),
                          const SizedBox(height: 3),
                          Text(
                            'Válido até $expira',
                            style: const TextStyle(
                              fontSize: 14,
                              fontWeight: FontWeight.w700,
                              color: Colors.white,
                            ),
                          ),
                        ],
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.2),
                        borderRadius: BorderRadius.circular(999),
                      ),
                      child: const Row(
                        children: [
                          Icon(Icons.check_rounded, color: Colors.white, size: 13),
                          SizedBox(width: 4),
                          Text('Ativo', style: TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.w700)),
                        ],
                      ),
                    ),
                  ],
                ),
              ),

              // ── Linha picotada ───────────────────────────
              Stack(
                alignment: Alignment.center,
                clipBehavior: Clip.none,
                children: [
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 20),
                    child: Row(
                      children: List.generate(
                        28,
                        (i) => Expanded(
                          child: Container(
                            height: 1,
                            margin: const EdgeInsets.symmetric(horizontal: 2),
                            color: i.isEven ? AppPalette.border : Colors.transparent,
                          ),
                        ),
                      ),
                    ),
                  ),
                  Positioned(
                    left: -12,
                    child: Container(
                      width: 24,
                      height: 24,
                      decoration: BoxDecoration(
                        color: AppPalette.bg,
                        shape: BoxShape.circle,
                        border: Border.all(color: AppPalette.border),
                      ),
                    ),
                  ),
                  Positioned(
                    right: -12,
                    child: Container(
                      width: 24,
                      height: 24,
                      decoration: BoxDecoration(
                        color: AppPalette.bg,
                        shape: BoxShape.circle,
                        border: Border.all(color: AppPalette.border),
                      ),
                    ),
                  ),
                ],
              ),

              // ── QR + código ──────────────────────────────
              Padding(
                padding: const EdgeInsets.fromLTRB(20, 12, 20, 16),
                child: Column(
                  children: [
                    if (codigo.isNotEmpty)
                      QrImageView(
                        data: codigo,
                        version: QrVersions.auto,
                        size: 118,
                        eyeStyle: const QrEyeStyle(eyeShape: QrEyeShape.square, color: AppPalette.ink),
                        dataModuleStyle: const QrDataModuleStyle(dataModuleShape: QrDataModuleShape.circle, color: AppPalette.ink),
                      ),
                    const SizedBox(height: 10),
                    Text(
                      codigo,
                      style: const TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.w900,
                        letterSpacing: 5,
                        color: AppPalette.ink,
                      ),
                    ),
                    const SizedBox(height: 2),
                    const Text(
                      'código de entrada',
                      style: TextStyle(fontSize: 11, color: AppPalette.lightInk, letterSpacing: 0.3),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),

        // ── Cancelar fora do ticket ───────────────────────
        const SizedBox(height: 4),
        TextButton.icon(
          onPressed: cancelling ? null : onCancel,
          icon: cancelling
              ? const SizedBox(width: 13, height: 13, child: CircularProgressIndicator(strokeWidth: 2))
              : const Icon(Icons.close_rounded, size: 15),
          label: Text(cancelling ? 'Cancelando...' : 'Cancelar pré-check-in'),
          style: TextButton.styleFrom(
            foregroundColor: AppPalette.midInk,
            textStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600),
          ),
        ),
      ],
    );
  }
}

// ─── Important info card ─────────────────────────────────────────────────────

class _ImportantInfoCard extends StatelessWidget {
  const _ImportantInfoCard({required this.detalhe});
  final MinhaCriancaDetalheDto detalhe;

  @override
  Widget build(BuildContext context) {
    final items = <Widget>[];

    if (detalhe.alergias?.isNotEmpty == true) {
      items.add(_InfoBox(
        icon: Icons.health_and_safety_outlined,
        title: 'Alergias',
        body: detalhe.alergias!,
        color: AppPalette.danger,
      ));
    }
    if (detalhe.restricoesAlimentares?.isNotEmpty == true) {
      if (items.isNotEmpty) items.add(const SizedBox(height: 8));
      items.add(_InfoBox(
        icon: Icons.restaurant_menu_outlined,
        title: 'Restrições alimentares',
        body: detalhe.restricoesAlimentares!,
        color: AppPalette.warning,
      ));
    }
    if (detalhe.observacoesVisiveisAoResponsavel?.isNotEmpty == true) {
      if (items.isNotEmpty) items.add(const SizedBox(height: 8));
      items.add(_InfoBox(
        icon: Icons.info_outline_rounded,
        title: 'Observações',
        body: detalhe.observacoesVisiveisAoResponsavel!,
        color: AppPalette.info,
      ));
    }

    return _SectionCard(
      label: 'Informações importantes',
      child: Column(children: items),
    );
  }
}

// ─── History card ────────────────────────────────────────────────────────────

class _HistoryCard extends StatelessWidget {
  const _HistoryCard({required this.historico});
  final List<MeuCheckinResumoDto> historico;

  @override
  Widget build(BuildContext context) {
    return _SectionCard(
      label: 'Histórico recente',
      child: historico.isEmpty
          ? const Text(
              'Nenhum registro ainda.',
              style: TextStyle(color: AppPalette.midInk, fontSize: 14),
            )
          : Column(
              children: historico.asMap().entries.map((e) {
                final i = e.key;
                final item = e.value;
                final isCheckedIn = item.checkoutTime == null;
                return Padding(
                  padding: EdgeInsets.only(top: i == 0 ? 0 : 8),
                  child: Row(
                    children: [
                      Container(
                        width: 36,
                        height: 36,
                        decoration: BoxDecoration(
                          color: isCheckedIn ? AppPalette.successBg : AppPalette.divider,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Icon(
                          isCheckedIn ? Icons.login_rounded : Icons.history_toggle_off_rounded,
                          size: 18,
                          color: isCheckedIn ? AppPalette.success : AppPalette.midInk,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              isCheckedIn ? 'Check-in ativo' : 'Check-out concluído',
                              style: const TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.w700,
                                color: AppPalette.ink,
                              ),
                            ),
                            Text(
                              'Entrada: ${DateFormat('dd/MM HH:mm').format(item.checkinTime)}'
                              '${item.checkoutTime != null ? ' · Saída: ${DateFormat('HH:mm').format(item.checkoutTime!)}' : ''}',
                              style: const TextStyle(fontSize: 12, color: AppPalette.midInk),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                );
              }).toList(),
            ),
    );
  }
}

// ─── Lesson content card ─────────────────────────────────────────────────────

class _ConteudoCard extends StatelessWidget {
  const _ConteudoCard({required this.conteudos, required this.resolveUrl});
  final List<MeuConteudoAulaDto> conteudos;
  final String Function(String) resolveUrl;

  @override
  Widget build(BuildContext context) {
    return _SectionCard(
      label: 'Conteúdo da aula',
      child: conteudos.isEmpty
          ? const Text(
              'Nenhum conteúdo publicado ainda.',
              style: TextStyle(color: AppPalette.midInk, fontSize: 14),
            )
          : Column(
              children: conteudos.asMap().entries.map((e) {
                final i = e.key;
                final item = e.value;
                return Padding(
                  padding: EdgeInsets.only(top: i == 0 ? 0 : 12),
                  child: _ConteudoTile(item: item, resolveUrl: resolveUrl),
                );
              }).toList(),
            ),
    );
  }
}

class _ConteudoTile extends StatelessWidget {
  const _ConteudoTile({required this.item, required this.resolveUrl});
  final MeuConteudoAulaDto item;
  final String Function(String) resolveUrl;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppPalette.divider,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Wrap(
            spacing: 6,
            children: [
              _Tag(DateFormat('dd/MM').format(item.dataReferencia)),
              if (item.tema?.isNotEmpty == true) _Tag(item.tema!),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            item.titulo,
            style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w800, color: AppPalette.ink),
          ),
          if (item.versiculo?.isNotEmpty == true) ...[
            const SizedBox(height: 4),
            Text(
              item.versiculo!,
              style: const TextStyle(fontSize: 13, color: AppPalette.primary, fontWeight: FontWeight.w600),
            ),
          ],
          const SizedBox(height: 6),
          Text(item.resumo, style: const TextStyle(fontSize: 13, color: AppPalette.midInk, height: 1.4)),
          if (item.atividadeEmCasa?.isNotEmpty == true) ...[
            const SizedBox(height: 8),
            _InfoBox(
              icon: Icons.edit_note_rounded,
              title: 'Atividade em casa',
              body: item.atividadeEmCasa!,
              color: AppPalette.success,
            ),
          ],
          if (item.anexos.isNotEmpty) ...[
            const SizedBox(height: 10),
            ...item.anexos.map((a) => Padding(
              padding: const EdgeInsets.only(top: 6),
              child: _AnexoTile(anexo: a, resolveUrl: resolveUrl),
            )),
          ],
        ],
      ),
    );
  }
}

class _Tag extends StatelessWidget {
  const _Tag(this.label);
  final String label;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: AppPalette.primarySoft,
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(
        label,
        style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w700, color: AppPalette.primary),
      ),
    );
  }
}

class _AnexoTile extends StatelessWidget {
  const _AnexoTile({required this.anexo, required this.resolveUrl});
  final MeuConteudoAulaAnexoDto anexo;
  final String Function(String) resolveUrl;

  @override
  Widget build(BuildContext context) {
    final rawValue = anexo.url ?? anexo.storagePath ?? '';
    final value = rawValue.isEmpty ? rawValue : resolveUrl(rawValue);

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: AppPalette.card,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: AppPalette.border),
      ),
      child: Row(
        children: [
          Icon(_iconForType(anexo.tipo), size: 18, color: AppPalette.primary),
          const SizedBox(width: 10),
          Expanded(
            child: Text(
              anexo.nomeExibicao,
              style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: AppPalette.ink),
            ),
          ),
          if (value.isNotEmpty)
            IconButton(
              padding: EdgeInsets.zero,
              constraints: const BoxConstraints(),
              icon: const Icon(Icons.open_in_new_rounded, size: 16),
              color: AppPalette.primary,
              onPressed: () async {
                final uri = Uri.tryParse(value);
                if (uri != null) await launchUrl(uri, mode: LaunchMode.externalApplication);
              },
            ),
        ],
      ),
    );
  }

  IconData _iconForType(String tipo) {
    switch (tipo.toLowerCase()) {
      case 'pdf': return Icons.picture_as_pdf_outlined;
      case 'imagem': return Icons.image_outlined;
      case 'link': return Icons.link_rounded;
      default: return Icons.attach_file_rounded;
    }
  }
}

// ─── Skeleton ────────────────────────────────────────────────────────────────

class _SkeletonDetalhe extends StatelessWidget {
  const _SkeletonDetalhe();

  @override
  Widget build(BuildContext context) {
    return Shimmer(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(16, 16, 16, 32),
        child: Column(
          children: const [
            _SkeletonCardPlaceholder(height: 72),
            SizedBox(height: 12),
            _SkeletonCardPlaceholder(height: 130),
            SizedBox(height: 12),
            _SkeletonCardPlaceholder(height: 90),
            SizedBox(height: 12),
            _SkeletonCardPlaceholder(height: 160),
          ],
        ),
      ),
    );
  }
}

class _SkeletonCardPlaceholder extends StatelessWidget {
  const _SkeletonCardPlaceholder({required this.height});
  final double height;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppPalette.border),
      ),
    );
  }
}

// ─── Retirada confirmation sheet ─────────────────────────────────────────────

class _RetiradaConfirmacaoSheet extends StatelessWidget {
  const _RetiradaConfirmacaoSheet({
    required this.nomeCrianca,
    this.pin,
    this.token,
  });

  final String nomeCrianca;
  final String? pin;
  final String? token;

  @override
  Widget build(BuildContext context) {
    final credencial = (pin != null && pin!.isNotEmpty) ? pin! : (token ?? '');
    final metodo = (pin != null && pin!.isNotEmpty) ? 'PIN' : 'Token';

    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: EdgeInsets.fromLTRB(
        24, 24, 24,
        24 + MediaQuery.of(context).viewInsets.bottom,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
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
          Row(
            children: [
              Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: AppPalette.successBg,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Icon(Icons.exit_to_app_rounded, color: AppPalette.success),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Confirmar retirada',
                      style: TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: AppPalette.ink),
                    ),
                    Text(nomeCrianca, style: const TextStyle(color: AppPalette.midInk, fontSize: 13)),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          _InfoBox(
            icon: Icons.info_outline_rounded,
            title: 'Confirmação via $metodo',
            body: 'Isto registrará que você está retirando $nomeCrianca usando o $metodo "$credencial".',
            color: AppPalette.success,
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.of(context).pop(false),
                  child: const Text('Cancelar'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: FilledButton.icon(
                  onPressed: () {
                    HapticFeedback.heavyImpact();
                    Navigator.of(context).pop(true);
                  },
                  icon: const Icon(Icons.check_rounded),
                  label: const Text('Confirmar'),
                  style: FilledButton.styleFrom(backgroundColor: AppPalette.success),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
