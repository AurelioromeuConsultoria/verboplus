import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../../core/app_palette.dart';
import 'kids_repository.dart';

class HistoricoCheckinsScreen extends StatefulWidget {
  const HistoricoCheckinsScreen({super.key});

  @override
  State<HistoricoCheckinsScreen> createState() => _HistoricoCheckinsScreenState();
}

class _HistoricoCheckinsScreenState extends State<HistoricoCheckinsScreen> {
  final _scrollCtrl = ScrollController();

  List<MeuCheckinResumoDto> _items = [];
  List<MinhaCriancaResumoDto> _criancas = [];
  int _page = 1;
  bool _hasMore = false;
  bool _loadingInitial = true;
  bool _loadingMore = false;
  String? _error;
  int? _filtroId;

  @override
  void initState() {
    super.initState();
    _scrollCtrl.addListener(_onScroll);
    _loadCriancas();
  }

  @override
  void dispose() {
    _scrollCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadCriancas() async {
    try {
      final repo = context.read<KidsRepository>();
      final criancas = await repo.getMinhasCriancas();
      if (!mounted) return;
      setState(() => _criancas = criancas);
    } catch (_) {}
    _load(reset: true);
  }

  void _onScroll() {
    if (_scrollCtrl.position.pixels >= _scrollCtrl.position.maxScrollExtent - 200 &&
        _hasMore &&
        !_loadingMore) {
      _loadMore();
    }
  }

  Future<void> _load({bool reset = false}) async {
    if (reset) {
      setState(() { _items = []; _page = 1; _hasMore = false; _error = null; _loadingInitial = true; });
    }
    try {
      final repo = context.read<KidsRepository>();
      final result = await repo.getMeuHistorico(
        page: 1,
        pageSize: 20,
        criancaPessoaId: _filtroId,
      );
      if (!mounted) return;
      setState(() {
        _items = result.items;
        _page = 1;
        _hasMore = result.hasMore;
        _loadingInitial = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() { _error = e.toString().replaceFirst('KidsApiException: ', ''); _loadingInitial = false; });
    }
  }

  Future<void> _loadMore() async {
    if (_loadingMore || !_hasMore) return;
    setState(() => _loadingMore = true);
    try {
      final repo = context.read<KidsRepository>();
      final next = _page + 1;
      final result = await repo.getMeuHistorico(
        page: next,
        pageSize: 20,
        criancaPessoaId: _filtroId,
      );
      if (!mounted) return;
      setState(() {
        _items.addAll(result.items);
        _page = next;
        _hasMore = result.hasMore;
      });
    } catch (_) {} finally {
      if (mounted) setState(() => _loadingMore = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppPalette.bg,
      appBar: AppBar(
        backgroundColor: AppPalette.bg,
        title: const Text('Histórico de presenças'),
        bottom: PreferredSize(
          preferredSize: const Size.fromHeight(1),
          child: Container(height: 1, color: AppPalette.border),
        ),
        actions: [
          if (_criancas.length > 1)
            _FiltroButton(
              criancas: _criancas,
              filtroId: _filtroId,
              onChanged: (id) {
                setState(() => _filtroId = id);
                _load(reset: true);
              },
            ),
        ],
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_loadingInitial) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_error != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline_rounded, size: 48, color: AppPalette.lightInk),
            const SizedBox(height: 12),
            Text(_error!, style: const TextStyle(color: AppPalette.midInk), textAlign: TextAlign.center),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: () => _load(reset: true), child: const Text('Tentar novamente')),
          ],
        ),
      );
    }
    if (_items.isEmpty) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.history_rounded, size: 56, color: AppPalette.lightInk),
            const SizedBox(height: 12),
            const Text('Nenhuma presença registrada ainda.', style: TextStyle(color: AppPalette.midInk)),
          ],
        ),
      );
    }

    // Agrupa por data
    final grupos = _agruparPorData(_items);

    return RefreshIndicator(
      onRefresh: () => _load(reset: true),
      child: ListView.builder(
        controller: _scrollCtrl,
        padding: const EdgeInsets.fromLTRB(16, 12, 16, 32),
        itemCount: grupos.length + (_loadingMore ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == grupos.length) {
            return const Padding(
              padding: EdgeInsets.symmetric(vertical: 16),
              child: Center(child: CircularProgressIndicator(strokeWidth: 2)),
            );
          }
          final grupo = grupos[index];
          return _GrupoData(
            label: grupo.label,
            checkins: grupo.checkins,
          );
        },
      ),
    );
  }

  List<_Grupo> _agruparPorData(List<MeuCheckinResumoDto> items) {
    final map = <String, List<MeuCheckinResumoDto>>{};
    final fmt = DateFormat('EEEE, d MMM yyyy', 'pt_BR');
    for (final c in items) {
      final key = fmt.format(c.checkinTime.toLocal());
      map.putIfAbsent(key, () => []).add(c);
    }
    return map.entries.map((e) => _Grupo(label: e.key, checkins: e.value)).toList();
  }
}

class _Grupo {
  _Grupo({required this.label, required this.checkins});
  final String label;
  final List<MeuCheckinResumoDto> checkins;
}

class _GrupoData extends StatelessWidget {
  const _GrupoData({required this.label, required this.checkins});

  final String label;
  final List<MeuCheckinResumoDto> checkins;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.only(top: 16, bottom: 6),
          child: Text(
            label,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w700,
              color: AppPalette.lightInk,
              letterSpacing: 0.4,
            ),
          ),
        ),
        Container(
          decoration: BoxDecoration(
            color: AppPalette.card,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: AppPalette.border),
          ),
          child: Column(
            children: [
              for (int i = 0; i < checkins.length; i++) ...[
                if (i > 0) const Divider(height: 1, indent: 16),
                _CheckinTile(checkin: checkins[i]),
              ],
            ],
          ),
        ),
      ],
    );
  }
}

class _CheckinTile extends StatelessWidget {
  const _CheckinTile({required this.checkin});

  final MeuCheckinResumoDto checkin;

  @override
  Widget build(BuildContext context) {
    final isCheckedOut = checkin.status == 'CheckedOut';
    final timeFmt = DateFormat('HH:mm', 'pt_BR');
    final entrou = timeFmt.format(checkin.checkinTime.toLocal());
    final saiu = checkin.checkoutTime != null
        ? timeFmt.format(checkin.checkoutTime!.toLocal())
        : null;

    final statusColor = isCheckedOut ? AppPalette.success : AppPalette.primary;
    final statusLabel = isCheckedOut ? 'Retirada realizada' : 'Presente';

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: statusColor.withValues(alpha: 0.10),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(
              isCheckedOut ? Icons.check_circle_outline_rounded : Icons.access_time_rounded,
              color: statusColor,
              size: 20,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  checkin.criancaNome,
                  style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700, color: AppPalette.ink),
                ),
                const SizedBox(height: 2),
                Text(
                  saiu != null ? 'Entrada $entrou · Saída $saiu' : 'Entrada $entrou',
                  style: const TextStyle(fontSize: 12, color: AppPalette.midInk),
                ),
              ],
            ),
          ),
          const SizedBox(width: 8),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            decoration: BoxDecoration(
              color: statusColor.withValues(alpha: 0.10),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Text(
              statusLabel,
              style: TextStyle(fontSize: 11, fontWeight: FontWeight.w700, color: statusColor),
            ),
          ),
        ],
      ),
    );
  }
}

class _FiltroButton extends StatelessWidget {
  const _FiltroButton({
    required this.criancas,
    required this.filtroId,
    required this.onChanged,
  });

  final List<MinhaCriancaResumoDto> criancas;
  final int? filtroId;
  final ValueChanged<int?> onChanged;

  @override
  Widget build(BuildContext context) {
    return PopupMenuButton<int?>(
      icon: Badge(
        isLabelVisible: filtroId != null,
        child: const Icon(Icons.filter_list_rounded),
      ),
      onSelected: onChanged,
      itemBuilder: (_) => [
        const PopupMenuItem<int?>(value: null, child: Text('Todos os filhos')),
        for (final c in criancas)
          PopupMenuItem<int>(
            value: c.pessoaId,
            child: Text(c.nome),
          ),
      ],
    );
  }
}
