import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

/// Tela que abre a câmera para escanear QR code.
/// O QR pode conter:
/// - Check-in: número (criancaPessoaId) para fazer check-in.
/// - Check-out: código de sessão (codigoSessao) para fazer check-out.
enum QrMode { checkin, checkout }

class QrScannerScreen extends StatefulWidget {
  const QrScannerScreen({
    super.key,
    required this.mode,
    required this.onCheckinScanned,
    required this.onCheckoutScanned,
  });

  final QrMode mode;
  final void Function(int criancaPessoaId) onCheckinScanned;
  final void Function(String codigoSessao) onCheckoutScanned;

  @override
  State<QrScannerScreen> createState() => _QrScannerScreenState();
}

class _QrScannerScreenState extends State<QrScannerScreen> {
  final MobileScannerController _controller = MobileScannerController(
    detectionSpeed: DetectionSpeed.normal,
    facing: CameraFacing.back,
    torchEnabled: false,
  );
  bool _processing = false;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onDetect(BarcodeCapture capture) {
    if (_processing) return;
    final barcodes = capture.barcodes;
    if (barcodes.isEmpty) return;
    final code = barcodes.first.rawValue;
    if (code == null || code.isEmpty) return;

    setState(() => _processing = true);

    if (widget.mode == QrMode.checkin) {
      final id = int.tryParse(code.trim());
      if (id == null) {
        setState(() => _processing = false);
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('QR inválido. Esperado ID da criança.')),
        );
        return;
      }
      widget.onCheckinScanned(id);
    } else {
      widget.onCheckoutScanned(code.trim());
    }

    if (mounted) {
      setState(() => _processing = false);
      Navigator.of(context).pop(true);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          widget.mode == QrMode.checkin ? 'Escanear criança (check-in)' : 'Escanear código (check-out)',
        ),
      ),
      body: Stack(
        children: [
          MobileScanner(
            controller: _controller,
            onDetect: _onDetect,
          ),
          if (_processing)
            Container(
              color: Colors.black54,
              child: const Center(
                child: CircularProgressIndicator(color: Colors.white),
              ),
            ),
        ],
      ),
    );
  }
}
