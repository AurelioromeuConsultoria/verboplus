import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:go_router/go_router.dart';
import '../../app_state.dart';
import '../../core/auth_repository.dart';
import '../../core/push_service.dart';
import 'kids_repository.dart';
import 'qr_scanner_screen.dart';

class CheckinCheckoutScreen extends StatefulWidget {
  const CheckinCheckoutScreen({super.key});

  @override
  State<CheckinCheckoutScreen> createState() => _CheckinCheckoutScreenState();
}

class _CheckinCheckoutScreenState extends State<CheckinCheckoutScreen> {
  List<CriancaDto> _criancas = [];
  bool _loading = true;
  String? _error;
  String? _successMessage;

  @override
  void initState() {
    super.initState();
    _load();
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      if (!mounted) return;
      final push = context.read<PushService>();
      push.onNotificationOpened = () => GoRouter.of(context).go('/avisos');
      push.setupMessageHandlers();
      if (await PushService.hadInitialNotification() && mounted) {
        GoRouter.of(context).go('/avisos');
      }
    });
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final repo = context.read<KidsRepository>();
      final list = await repo.getCriancas();
      if (mounted) setState(() {
        _criancas = list;
        _loading = false;
      });
    } catch (e) {
      if (mounted) setState(() {
        _error = e is KidsApiException ? e.message : e.toString();
        _loading = false;
      });
    }
  }

  Future<void> _doCheckinByQr() async {
    final repo = context.read<KidsRepository>();
    final user = context.read<AppState>().user;
    if (user == null) return;

    final ok = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (ctx) => QrScannerScreen(
          mode: QrMode.checkin,
          onCheckinScanned: (_) {},
          onCheckoutScanned: (_) {},
        ),
      ),
    );
    if (!mounted || ok != true) return;

    // After scan, we need the criancaPessoaId from the scanner - the scanner
    // pops with true and we don't get the value here. So we need to pass
    // a callback that does the checkin and then pops. Refactor: get value via
    // Navigator.pop(context, criancaPessoaId) and then do checkin here.
    // For now: open scanner that returns the id.
    _load();
  }

  void _openCheckinQrAndRun(int? criancaPessoaId) async {
    if (criancaPessoaId == null) return;
    final repo = context.read<KidsRepository>();
    final user = context.read<AppState>().user;
    if (user == null) return;
    setState(() => _error = null);
    try {
      final res = await repo.checkin(
        criancaPessoaId: criancaPessoaId,
        metodo: 'QR',
        checkinByPessoaId: user.pessoaId,
      );
      if (mounted) {
        setState(() => _successMessage = 'Check-in: ${res.codigoSessao}');
        _load();
      }
    } catch (e) {
      if (mounted) setState(() {
        _error = e is KidsApiException ? e.message : e.toString();
      });
    }
  }

  void _openCheckoutQrAndRun(String? codigoSessao) async {
    if (codigoSessao == null || codigoSessao.isEmpty) return;
    final repo = context.read<KidsRepository>();
    final user = context.read<AppState>().user;
    if (user == null) return;
    setState(() => _error = null);
    try {
      // Backend exige criancaPessoaId no checkout. Se o QR só tiver CodigoSessao,
      // precisamos de um endpoint GET checkin por codigoSessao para obter criancaPessoaId.
      // Por ora: assumir que o QR de checkout pode conter "codigoSessao,criancaPessoaId"
      // ou que temos endpoint getCheckinByCodigo. Documentar isso.
      final parts = codigoSessao.split(',');
      final codigo = parts.first.trim();
      int? criancaId;
      if (parts.length >= 2) criancaId = int.tryParse(parts[1].trim());
      if (criancaId == null) {
        setState(() => _error = 'Use o QR gerado no check-in (código + ID criança).');
        return;
      }
      await repo.checkout(
        criancaPessoaId: criancaId,
        codigoSessao: codigo,
        checkoutByPessoaId: user.pessoaId,
        metodo: 'QR',
      );
      if (mounted) {
        setState(() => _successMessage = 'Check-out realizado.');
        _load();
      }
    } catch (e) {
      if (mounted) setState(() {
        _error = e is KidsApiException ? e.message : e.toString();
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final user = context.watch<AppState>().user;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Kids - Check-in / Check-out'),
        actions: [
          IconButton(
            icon: const Icon(Icons.notifications_outlined),
            onPressed: () => context.push('/avisos'),
            tooltip: 'Avisos',
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await context.read<AuthRepository>().logout();
              context.read<AppState>().setUser(null);
              if (context.mounted) context.go('/login');
            },
            tooltip: 'Sair',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : _body(user),
      ),
      floatingActionButton: Column(
        mainAxisAlignment: MainAxisAlignment.end,
        children: [
          FloatingActionButton.extended(
            onPressed: () async {
              final id = await Navigator.of(context).push<int>(
                MaterialPageRoute(
                  builder: (ctx) => QrScannerScreen(
                    mode: QrMode.checkin,
                    onCheckinScanned: (id) => Navigator.of(ctx).pop(id),
                    onCheckoutScanned: (_) {},
                  ),
                ),
              );
              _openCheckinQrAndRun(id);
            },
            icon: const Icon(Icons.qr_code_scanner),
            label: const Text('Check-in (QR)'),
          ),
          const SizedBox(height: 12),
          FloatingActionButton.extended(
            onPressed: () async {
              final codigo = await Navigator.of(context).push<String>(
                MaterialPageRoute(
                  builder: (ctx) => QrScannerScreen(
                    mode: QrMode.checkout,
                    onCheckinScanned: (_) {},
                    onCheckoutScanned: (s) => Navigator.of(ctx).pop(s),
                  ),
                ),
              );
              if (codigo != null) _openCheckoutQrAndRun(codigo);
            },
            icon: const Icon(Icons.logout),
            label: const Text('Check-out (QR)'),
          ),
        ],
      ),
    );
  }

  Widget _body(Usuario? user) {
    if (_error != null) {
      return ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            color: Theme.of(context).colorScheme.errorContainer,
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Text(_error!),
            ),
          ),
        ],
      );
    }
    if (_successMessage != null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(_successMessage!), backgroundColor: Colors.green),
        );
        setState(() => _successMessage = null);
      });
    }

    final ativos = _criancas.where((c) => c.estaCheckedIn).toList();
    final inativos = _criancas.where((c) => !c.estaCheckedIn).toList();

    return ListView(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 100),
      children: [
        if (ativos.isNotEmpty) ...[
          Text('Com check-in (${ativos.length})', style: Theme.of(context).textTheme.titleSmall),
          const SizedBox(height: 8),
          ...ativos.map((c) => _CriancaTile(
                crianca: c,
                user: user,
                onCheckin: null,
                onCheckout: () => _doCheckout(c),
                kidsRepo: context.read<KidsRepository>(),
                onStateChanged: () => _load(),
              )),
          const SizedBox(height: 24),
        ],
        Text('Crianças (${_criancas.length})', style: Theme.of(context).textTheme.titleSmall),
        const SizedBox(height: 8),
        ..._criancas.map((c) => _CriancaTile(
              crianca: c,
              user: user,
              onCheckin: () => _doCheckin(c),
              onCheckout: c.estaCheckedIn ? () => _doCheckout(c) : null,
              kidsRepo: context.read<KidsRepository>(),
              onStateChanged: () => _load(),
            )),
      ],
    );
  }

  Future<void> _doCheckin(CriancaDto c) async {
    final repo = context.read<KidsRepository>();
    final user = context.read<AppState>().user;
    if (user == null) return;
    setState(() => _error = null);
    try {
      final res = await repo.checkin(
        criancaPessoaId: c.pessoaId,
        metodo: 'ADMIN',
        checkinByPessoaId: user.pessoaId,
      );
      if (mounted) {
        setState(() => _successMessage = 'Check-in: ${c.nome} — Código: ${res.codigoSessao}');
        _load();
      }
    } catch (e) {
      if (mounted) setState(() {
        _error = e is KidsApiException ? e.message : e.toString();
      });
    }
  }

  Future<void> _doCheckout(CriancaDto c) async {
    final codigo = c.checkinAtual?.codigoSessao;
    if (codigo == null) return;
    final repo = context.read<KidsRepository>();
    final user = context.read<AppState>().user;
    if (user == null) return;
    setState(() => _error = null);
    try {
      await repo.checkout(
        criancaPessoaId: c.pessoaId,
        codigoSessao: codigo,
        checkoutByPessoaId: user.pessoaId,
        metodo: 'ADMIN',
      );
      if (mounted) {
        setState(() => _successMessage = 'Check-out: ${c.nome}');
        _load();
      }
    } catch (e) {
      if (mounted) setState(() {
        _error = e is KidsApiException ? e.message : e.toString();
      });
    }
  }
}

class _CriancaTile extends StatelessWidget {
  const _CriancaTile({
    required this.crianca,
    required this.user,
    required this.onCheckin,
    required this.onCheckout,
    required this.kidsRepo,
    required this.onStateChanged,
  });

  final CriancaDto crianca;
  final Usuario? user;
  final VoidCallback? onCheckin;
  final VoidCallback? onCheckout;
  final KidsRepository kidsRepo;
  final VoidCallback onStateChanged;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: CircleAvatar(
          child: Text(crianca.nome.isNotEmpty ? crianca.nome[0].toUpperCase() : '?'),
        ),
        title: Text(crianca.nome),
        subtitle: crianca.estaCheckedIn && crianca.checkinAtual != null
            ? Text('Código: ${crianca.checkinAtual!.codigoSessao}')
            : null,
        trailing: crianca.estaCheckedIn
            ? (onCheckout != null
                ? TextButton(onPressed: onCheckout, child: const Text('Check-out'))
                : null)
            : (onCheckin != null
                ? FilledButton(onPressed: onCheckin, child: const Text('Check-in'))
                : null),
      ),
    );
  }
}
