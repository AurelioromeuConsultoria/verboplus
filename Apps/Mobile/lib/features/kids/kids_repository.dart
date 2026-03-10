import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../core/api_client.dart';

/// Repositório para APIs de Kids (crianças, check-in, check-out).
class KidsRepository {
  KidsRepository(this._api);

  final ApiClient _api;

  /// GET /api/kids/criancas
  Future<List<CriancaDto>> getCriancas() async {
    final response = await _api.get('/api/kids/criancas');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => CriancaDto.fromJson(e as Map<String, dynamic>)).toList();
  }

  /// GET /api/kids/criancas/{id}
  Future<CriancaDto?> getCriancaById(int criancaPessoaId) async {
    final response = await _api.get('/api/kids/criancas/$criancaPessoaId');
    if (response.statusCode == 404) return null;
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    return CriancaDto.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// POST /api/kids/checkin
  /// Metodo: "QR", "PIN" ou "ADMIN"
  Future<CheckinResponse> checkin({
    required int criancaPessoaId,
    required String metodo,
    int? checkinByPessoaId,
    String? observacoes,
  }) async {
    final body = <String, dynamic>{
      'criancaPessoaId': criancaPessoaId,
      'metodo': metodo,
    };
    if (checkinByPessoaId != null) body['checkinByPessoaId'] = checkinByPessoaId;
    if (observacoes != null && observacoes.isNotEmpty) body['observacoes'] = observacoes;

    final response = await _api.post('/api/kids/checkin', body: body);
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    return CheckinResponse.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// POST /api/kids/checkout
  /// Requer CodigoSessao (retornado no check-in) e CheckoutByPessoaId (PessoaId do responsável).
  Future<void> checkout({
    required int criancaPessoaId,
    required String codigoSessao,
    required int checkoutByPessoaId,
    String? metodo,
  }) async {
    final body = <String, dynamic>{
      'criancaPessoaId': criancaPessoaId,
      'codigoSessao': codigoSessao,
      'checkoutByPessoaId': checkoutByPessoaId,
    };
    if (metodo != null) body['metodo'] = metodo;

    final response = await _api.post('/api/kids/checkout', body: body);
    if (response.statusCode == 403) throw KidsApiException('Você não tem autorização para retirar esta criança.');
    if (response.statusCode == 400 || response.statusCode == 409)
      throw KidsApiException(_msg(response));
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
  }

  /// GET /api/kids/checkins?criancaPessoaId=
  Future<List<KidsCheckinDto>> getCheckins({int? criancaPessoaId}) async {
    final path = criancaPessoaId != null
        ? '/api/kids/checkins?criancaPessoaId=$criancaPessoaId'
        : '/api/kids/checkins';
    final response = await _api.get(path);
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list.map((e) => KidsCheckinDto.fromJson(e as Map<String, dynamic>)).toList();
  }

  String _msg(http.Response r) {
    try {
      final m = jsonDecode(r.body);
      if (m is Map && m['message'] != null) return m['message'] as String;
    } catch (_) {}
    return 'Erro na comunicação com o servidor (${r.statusCode})';
  }
}

class KidsApiException implements Exception {
  KidsApiException(this.message);
  final String message;
  @override
  String toString() => message;
}

// --- DTOs (espelhando o backend) ---

class CriancaDto {
  CriancaDto({
    required this.pessoaId,
    required this.nome,
    this.dataNascimento,
    this.estaCheckedIn = false,
    this.checkinAtual,
  });

  final int pessoaId;
  final String nome;
  final DateTime? dataNascimento;
  final bool estaCheckedIn;
  final KidsCheckinDto? checkinAtual;

  factory CriancaDto.fromJson(Map<String, dynamic> json) {
    KidsCheckinDto? checkinAtual;
    if (json['checkinAtual'] != null) {
      checkinAtual = KidsCheckinDto.fromJson(json['checkinAtual'] as Map<String, dynamic>);
    }
    return CriancaDto(
      pessoaId: (json['pessoaId'] as num).toInt(),
      nome: json['nome'] as String? ?? '',
      dataNascimento: json['dataNascimento'] != null
          ? DateTime.tryParse(json['dataNascimento'] as String)
          : null,
      estaCheckedIn: json['estaCheckedIn'] as bool? ?? false,
      checkinAtual: checkinAtual,
    );
  }
}

class CheckinResponse {
  CheckinResponse({
    required this.checkinId,
    required this.codigoSessao,
    required this.checkinTime,
  });

  final int checkinId;
  final String codigoSessao;
  final DateTime checkinTime;

  factory CheckinResponse.fromJson(Map<String, dynamic> json) {
    return CheckinResponse(
      checkinId: (json['checkinId'] as num).toInt(),
      codigoSessao: json['codigoSessao'] as String? ?? '',
      checkinTime: DateTime.parse(json['checkinTime'] as String),
    );
  }
}

class KidsCheckinDto {
  KidsCheckinDto({
    required this.id,
    required this.criancaPessoaId,
    required this.criancaNome,
    required this.checkinTime,
    this.checkoutTime,
    required this.codigoSessao,
    required this.status,
  });

  final int id;
  final int criancaPessoaId;
  final String criancaNome;
  final DateTime checkinTime;
  final DateTime? checkoutTime;
  final String codigoSessao;
  final String status;

  factory KidsCheckinDto.fromJson(Map<String, dynamic> json) {
    return KidsCheckinDto(
      id: (json['id'] as num).toInt(),
      criancaPessoaId: (json['criancaPessoaId'] as num).toInt(),
      criancaNome: json['criancaNome'] as String? ?? '',
      checkinTime: DateTime.parse(json['checkinTime'] as String),
      checkoutTime: json['checkoutTime'] != null
          ? DateTime.tryParse(json['checkoutTime'] as String)
          : null,
      codigoSessao: json['codigoSessao'] as String? ?? '',
      status: json['status'] as String? ?? '',
    );
  }
}
