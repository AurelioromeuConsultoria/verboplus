import 'dart:convert';
import 'package:http/http.dart' as http;
import '../../core/api_client.dart';

/// Repositório para APIs de Kids orientadas ao responsável.
class KidsRepository {
  KidsRepository(this._api);

  final ApiClient _api;

  String resolveAbsoluteUrl(String value) {
    final trimmed = value.trim();
    if (trimmed.isEmpty) return trimmed;
    final uri = Uri.tryParse(trimmed);
    if (uri != null && uri.hasScheme) {
      return trimmed;
    }

    final baseUri = Uri.parse(_api.baseUrl);
    if (trimmed.startsWith('/')) {
      return baseUri.resolve(trimmed).toString();
    }

    return baseUri.resolve('/$trimmed').toString();
  }

  /// GET /api/kids/me/criancas
  Future<List<MinhaCriancaResumoDto>> getMinhasCriancas() async {
    final response = await _api.get('/api/kids/me/criancas');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((e) => MinhaCriancaResumoDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// GET /api/kids/me/criancas/{id}
  Future<MinhaCriancaDetalheDto?> getMinhaCriancaById(int criancaPessoaId) async {
    final response = await _api.get('/api/kids/me/criancas/$criancaPessoaId');
    if (response.statusCode == 404) return null;
    if (response.statusCode == 403) throw KidsApiException(_msg(response));
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    return MinhaCriancaDetalheDto.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }

  /// GET /api/kids/me/checkins
  Future<List<MeuCheckinResumoDto>> getMeusCheckins() async {
    final response = await _api.get('/api/kids/me/checkins');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((e) => MeuCheckinResumoDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// GET /api/kids/me/historico?page=1&pageSize=20&criancaPessoaId=X
  Future<HistoricoPagedDto> getMeuHistorico({
    int page = 1,
    int pageSize = 20,
    int? criancaPessoaId,
  }) async {
    var uri = '/api/kids/me/historico?page=$page&pageSize=$pageSize';
    if (criancaPessoaId != null) uri += '&criancaPessoaId=$criancaPessoaId';
    final response = await _api.get(uri);
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    return HistoricoPagedDto.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }

  /// GET /api/kids/me/precheckins
  Future<List<KidsPreCheckinDto>> getMeusPreCheckins({
    String? status,
    bool somenteAtivos = false,
  }) async {
    final query = <String>[];
    if (status != null && status.isNotEmpty) {
      query.add('status=${Uri.encodeQueryComponent(status)}');
    }
    if (somenteAtivos) {
      query.add('somenteAtivos=true');
    }

    final suffix = query.isEmpty ? '' : '?${query.join('&')}';
    final response = await _api.get('/api/kids/me/precheckins$suffix');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((e) => KidsPreCheckinDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// GET /api/kids/me/criancas/{id}/conteudos-aula
  Future<List<MeuConteudoAulaDto>> getMeuConteudoPorCrianca(
    int criancaPessoaId, {
    int? limit,
  }) async {
    final query = <String>[];
    if (limit != null) {
      query.add('limit=$limit');
    }

    final suffix = query.isEmpty ? '' : '?${query.join('&')}';
    final response = await _api.get('/api/kids/me/criancas/$criancaPessoaId/conteudos-aula$suffix');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((e) => MeuConteudoAulaDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// POST /api/kids/me/precheckins
  Future<KidsPreCheckinDto> criarMeuPreCheckin({
    required int criancaPessoaId,
    int? eventoOcorrenciaId,
    String? salaId,
    String? turmaId,
    String? observacoes,
  }) async {
    final body = <String, dynamic>{
      'criancaPessoaId': criancaPessoaId,
    };
    if (eventoOcorrenciaId != null) {
      body['eventoOcorrenciaId'] = eventoOcorrenciaId;
    }
    if (salaId != null && salaId.isNotEmpty) {
      body['salaId'] = salaId;
    }
    if (turmaId != null && turmaId.isNotEmpty) {
      body['turmaId'] = turmaId;
    }
    if (observacoes != null && observacoes.isNotEmpty) {
      body['observacoes'] = observacoes;
    }

    final response = await _api.post('/api/kids/me/precheckins', body: body);
    if (response.statusCode == 400 || response.statusCode == 403 || response.statusCode == 409) {
      throw KidsApiException(_msg(response));
    }
    if (response.statusCode != 200) {
      throw KidsApiException(_msg(response));
    }

    return KidsPreCheckinDto.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// POST /api/kids/me/precheckins/{id}/cancelar
  Future<KidsPreCheckinDto> cancelarMeuPreCheckin(
    int preCheckinId, {
    String? motivo,
  }) async {
    final body = <String, dynamic>{};
    if (motivo != null && motivo.isNotEmpty) {
      body['motivo'] = motivo;
    }

    final response = await _api.post('/api/kids/me/precheckins/$preCheckinId/cancelar', body: body);
    if (response.statusCode == 400 || response.statusCode == 403 || response.statusCode == 409) {
      throw KidsApiException(_msg(response));
    }
    if (response.statusCode != 200) {
      throw KidsApiException(_msg(response));
    }

    return KidsPreCheckinDto.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// GET /api/kids/me/avisos
  Future<List<MeuAvisoKidsDto>> getMeusAvisos({
    bool naoLidos = false,
    String? tipo,
    int? criancaPessoaId,
    int? limit,
  }) async {
    final query = <String>[];
    if (naoLidos) {
      query.add('naoLidos=true');
    }
    if (tipo != null && tipo.isNotEmpty) {
      query.add('tipo=${Uri.encodeQueryComponent(tipo)}');
    }
    if (criancaPessoaId != null) {
      query.add('criancaPessoaId=$criancaPessoaId');
    }
    if (limit != null) {
      query.add('limit=$limit');
    }

    final suffix = query.isEmpty ? '' : '?${query.join('&')}';
    final response = await _api.get('/api/kids/me/avisos$suffix');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((e) => MeuAvisoKidsDto.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// PATCH /api/kids/me/avisos/{id}/lido
  Future<MeuAvisoKidsDto> marcarAvisoComoLido(int id) async {
    final response = await _api.patch('/api/kids/me/avisos/$id/lido');
    if (response.statusCode != 200) throw KidsApiException(_msg(response));
    return MeuAvisoKidsDto.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// POST /api/kids/retirada/validar
  Future<RetiradaValidacaoDto> validarRetirada({
    String? token,
    String? pin,
  }) async {
    final body = <String, dynamic>{};
    if (token != null && token.isNotEmpty) {
      body['token'] = token;
    }
    if (pin != null && pin.isNotEmpty) {
      body['pin'] = pin;
    }

    final response = await _api.post('/api/kids/retirada/validar', body: body);
    if (response.statusCode == 400 || response.statusCode == 409) {
      throw KidsApiException(_msg(response));
    }
    if (response.statusCode != 200) {
      throw KidsApiException(_msg(response));
    }
    return RetiradaValidacaoDto.fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  /// POST /api/kids/retirada/confirmar
  Future<void> confirmarRetirada({
    required int checkinId,
    required int responsavelPessoaId,
    required String metodo,
    String? token,
    String? pin,
    String? observacoes,
  }) async {
    final body = <String, dynamic>{
      'checkinId': checkinId,
      'responsavelPessoaId': responsavelPessoaId,
      'metodo': metodo,
    };
    if (token != null && token.isNotEmpty) {
      body['token'] = token;
    }
    if (pin != null && pin.isNotEmpty) {
      body['pin'] = pin;
    }
    if (observacoes != null && observacoes.isNotEmpty) {
      body['observacoes'] = observacoes;
    }

    final response = await _api.post('/api/kids/retirada/confirmar', body: body);
    if (response.statusCode == 403) {
      throw KidsApiException('Responsável não autorizado para retirada.');
    }
    if (response.statusCode == 400 || response.statusCode == 409) {
      throw KidsApiException(_msg(response));
    }
    if (response.statusCode != 200) {
      throw KidsApiException(_msg(response));
    }
  }

  /// POST /api/kids/retirada/excecao
  Future<void> registrarRetiradaExcecao({
    required int checkinId,
    required String pessoaRetirandoNome,
    required String motivo,
    String? pessoaRetirandoDocumento,
    String? observacoes,
  }) async {
    final body = <String, dynamic>{
      'checkinId': checkinId,
      'pessoaRetirandoNome': pessoaRetirandoNome,
      'motivo': motivo,
    };
    if (pessoaRetirandoDocumento != null && pessoaRetirandoDocumento.isNotEmpty) {
      body['pessoaRetirandoDocumento'] = pessoaRetirandoDocumento;
    }
    if (observacoes != null && observacoes.isNotEmpty) {
      body['observacoes'] = observacoes;
    }

    final response = await _api.post('/api/kids/retirada/excecao', body: body);
    if (response.statusCode == 400 || response.statusCode == 409 || response.statusCode == 403) {
      throw KidsApiException(_msg(response));
    }
    if (response.statusCode != 200) {
      throw KidsApiException(_msg(response));
    }
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

class MeuConteudoAulaAnexoDto {
  MeuConteudoAulaAnexoDto({
    required this.id,
    required this.tipo,
    required this.nomeExibicao,
    this.url,
    this.storagePath,
    this.mimeType,
    this.tamanhoBytes,
    required this.ordem,
  });

  final int id;
  final String tipo;
  final String nomeExibicao;
  final String? url;
  final String? storagePath;
  final String? mimeType;
  final int? tamanhoBytes;
  final int ordem;

  factory MeuConteudoAulaAnexoDto.fromJson(Map<String, dynamic> json) =>
      MeuConteudoAulaAnexoDto(
        id: json['id'] as int,
        tipo: json['tipo'] as String? ?? '',
        nomeExibicao: json['nomeExibicao'] as String? ?? '',
        url: json['url'] as String?,
        storagePath: json['storagePath'] as String?,
        mimeType: json['mimeType'] as String?,
        tamanhoBytes: (json['tamanhoBytes'] as num?)?.toInt(),
        ordem: (json['ordem'] as num?)?.toInt() ?? 0,
      );
}

class MeuConteudoAulaDto {
  MeuConteudoAulaDto({
    required this.id,
    required this.criancaPessoaId,
    required this.criancaNome,
    required this.titulo,
    this.tema,
    this.versiculo,
    required this.resumo,
    this.atividadeEmCasa,
    this.observacaoResponsavel,
    required this.dataReferencia,
    this.salaId,
    this.turmaId,
    this.publicadoEm,
    required this.anexos,
  });

  final int id;
  final int criancaPessoaId;
  final String criancaNome;
  final String titulo;
  final String? tema;
  final String? versiculo;
  final String resumo;
  final String? atividadeEmCasa;
  final String? observacaoResponsavel;
  final DateTime dataReferencia;
  final String? salaId;
  final String? turmaId;
  final DateTime? publicadoEm;
  final List<MeuConteudoAulaAnexoDto> anexos;

  factory MeuConteudoAulaDto.fromJson(Map<String, dynamic> json) => MeuConteudoAulaDto(
        id: json['id'] as int,
        criancaPessoaId: json['criancaPessoaId'] as int,
        criancaNome: json['criancaNome'] as String? ?? '',
        titulo: json['titulo'] as String? ?? '',
        tema: json['tema'] as String?,
        versiculo: json['versiculo'] as String?,
        resumo: json['resumo'] as String? ?? '',
        atividadeEmCasa: json['atividadeEmCasa'] as String?,
        observacaoResponsavel: json['observacaoResponsavel'] as String?,
        dataReferencia: DateTime.parse(json['dataReferencia'] as String),
        salaId: json['salaId'] as String?,
        turmaId: json['turmaId'] as String?,
        publicadoEm: json['publicadoEm'] != null
            ? DateTime.tryParse(json['publicadoEm'] as String)
            : null,
        anexos: ((json['anexos'] as List<dynamic>?) ?? const [])
            .map((e) => MeuConteudoAulaAnexoDto.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class MinhaCriancaResumoDto {
  MinhaCriancaResumoDto({
    required this.pessoaId,
    required this.nome,
    this.dataNascimento,
    this.salaId,
    required this.estaCheckedIn,
    this.checkinAtual,
    required this.temAlertaCritico,
    this.fotoUrl,
  });

  final int pessoaId;
  final String nome;
  final DateTime? dataNascimento;
  final String? salaId;
  final bool estaCheckedIn;
  final MeuCheckinResumoDto? checkinAtual;
  final bool temAlertaCritico;
  final String? fotoUrl;

  factory MinhaCriancaResumoDto.fromJson(Map<String, dynamic> json) {
    return MinhaCriancaResumoDto(
      pessoaId: (json['pessoaId'] as num).toInt(),
      nome: json['nome'] as String? ?? '',
      dataNascimento: json['dataNascimento'] != null
          ? DateTime.tryParse(json['dataNascimento'] as String)
          : null,
      salaId: json['salaId'] as String?,
      estaCheckedIn: json['estaCheckedIn'] as bool? ?? false,
      checkinAtual: json['checkinAtual'] != null
          ? MeuCheckinResumoDto.fromJson(json['checkinAtual'] as Map<String, dynamic>)
          : null,
      temAlertaCritico: json['temAlertaCritico'] as bool? ?? false,
      fotoUrl: json['fotoUrl'] as String?,
    );
  }

  Map<String, dynamic> toJson() => {
        'pessoaId': pessoaId,
        'nome': nome,
        'dataNascimento': dataNascimento?.toIso8601String(),
        'salaId': salaId,
        'estaCheckedIn': estaCheckedIn,
        'checkinAtual': checkinAtual?.toJson(),
        'temAlertaCritico': temAlertaCritico,
        'fotoUrl': fotoUrl,
      };
}

class MinhaCriancaDetalheDto {
  MinhaCriancaDetalheDto({
    required this.pessoaId,
    required this.nome,
    this.dataNascimento,
    this.salaId,
    this.alergias,
    this.restricoesAlimentares,
    this.observacoesVisiveisAoResponsavel,
    required this.estaCheckedIn,
    this.checkinAtual,
    required this.historicoRecente,
    this.fotoUrl,
  });

  final int pessoaId;
  final String nome;
  final DateTime? dataNascimento;
  final String? salaId;
  final String? alergias;
  final String? restricoesAlimentares;
  final String? observacoesVisiveisAoResponsavel;
  final bool estaCheckedIn;
  final MeuCheckinResumoDto? checkinAtual;
  final List<MeuCheckinResumoDto> historicoRecente;
  final String? fotoUrl;

  factory MinhaCriancaDetalheDto.fromJson(Map<String, dynamic> json) {
    final historico = (json['historicoRecente'] as List<dynamic>? ?? [])
        .map((e) => MeuCheckinResumoDto.fromJson(e as Map<String, dynamic>))
        .toList();

    return MinhaCriancaDetalheDto(
      pessoaId: (json['pessoaId'] as num).toInt(),
      nome: json['nome'] as String? ?? '',
      dataNascimento: json['dataNascimento'] != null
          ? DateTime.tryParse(json['dataNascimento'] as String)
          : null,
      salaId: json['salaId'] as String?,
      alergias: json['alergias'] as String?,
      restricoesAlimentares: json['restricoesAlimentares'] as String?,
      observacoesVisiveisAoResponsavel: json['observacoesVisiveisAoResponsavel'] as String?,
      estaCheckedIn: json['estaCheckedIn'] as bool? ?? false,
      checkinAtual: json['checkinAtual'] != null
          ? MeuCheckinResumoDto.fromJson(json['checkinAtual'] as Map<String, dynamic>)
          : null,
      historicoRecente: historico,
      fotoUrl: json['fotoUrl'] as String?,
    );
  }
}

class MeuCheckinResumoDto {
  MeuCheckinResumoDto({
    required this.id,
    required this.criancaPessoaId,
    required this.criancaNome,
    required this.checkinTime,
    this.checkoutTime,
    required this.status,
    this.salaId,
    this.tokenRetirada,
    this.pinRetirada,
    this.tokenRetiradaExpiraEm,
  });

  final int id;
  final int criancaPessoaId;
  final String criancaNome;
  final DateTime checkinTime;
  final DateTime? checkoutTime;
  final String status;
  final String? salaId;
  final String? tokenRetirada;
  final String? pinRetirada;
  final DateTime? tokenRetiradaExpiraEm;

  factory MeuCheckinResumoDto.fromJson(Map<String, dynamic> json) {
    return MeuCheckinResumoDto(
      id: (json['id'] as num).toInt(),
      criancaPessoaId: (json['criancaPessoaId'] as num).toInt(),
      criancaNome: json['criancaNome'] as String? ?? '',
      checkinTime: DateTime.parse(json['checkinTime'] as String),
      checkoutTime: json['checkoutTime'] != null
          ? DateTime.tryParse(json['checkoutTime'] as String)
          : null,
      status: json['status'] as String? ?? '',
      salaId: json['salaId'] as String?,
      tokenRetirada: json['tokenRetirada'] as String?,
      pinRetirada: json['pinRetirada'] as String?,
      tokenRetiradaExpiraEm: json['tokenRetiradaExpiraEm'] != null
          ? DateTime.tryParse(json['tokenRetiradaExpiraEm'] as String)
          : null,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'criancaPessoaId': criancaPessoaId,
        'criancaNome': criancaNome,
        'checkinTime': checkinTime.toIso8601String(),
        'checkoutTime': checkoutTime?.toIso8601String(),
        'status': status,
        'salaId': salaId,
        'tokenRetirada': tokenRetirada,
        'pinRetirada': pinRetirada,
        'tokenRetiradaExpiraEm': tokenRetiradaExpiraEm?.toIso8601String(),
      };
}

class HistoricoPagedDto {
  HistoricoPagedDto({
    required this.items,
    required this.total,
    required this.page,
    required this.pageSize,
    required this.hasMore,
  });

  final List<MeuCheckinResumoDto> items;
  final int total;
  final int page;
  final int pageSize;
  final bool hasMore;

  factory HistoricoPagedDto.fromJson(Map<String, dynamic> json) {
    return HistoricoPagedDto(
      items: ((json['items'] as List<dynamic>?) ?? [])
          .map((e) => MeuCheckinResumoDto.fromJson(e as Map<String, dynamic>))
          .toList(),
      total: (json['total'] as num?)?.toInt() ?? 0,
      page: (json['page'] as num?)?.toInt() ?? 1,
      pageSize: (json['pageSize'] as num?)?.toInt() ?? 20,
      hasMore: json['hasMore'] as bool? ?? false,
    );
  }
}

class KidsPreCheckinDto {
  KidsPreCheckinDto({
    required this.id,
    required this.criancaPessoaId,
    required this.criancaNome,
    required this.responsavelPessoaId,
    required this.responsavelNome,
    this.eventoOcorrenciaId,
    this.checkinId,
    this.eventoDataHoraInicio,
    this.salaId,
    this.turmaId,
    required this.qrToken,
    required this.codigoCurto,
    required this.status,
    required this.expiraEm,
    this.observacoesResponsavel,
    required this.criadoEm,
    this.confirmadoEm,
    this.confirmadoPorNome,
    this.canceladoEm,
    this.canceladoPorNome,
    this.cancelamentoMotivo,
  });

  final int id;
  final int criancaPessoaId;
  final String criancaNome;
  final int responsavelPessoaId;
  final String responsavelNome;
  final int? eventoOcorrenciaId;
  final int? checkinId;
  final DateTime? eventoDataHoraInicio;
  final String? salaId;
  final String? turmaId;
  final String qrToken;
  final String codigoCurto;
  final String status;
  final DateTime expiraEm;
  final String? observacoesResponsavel;
  final DateTime criadoEm;
  final DateTime? confirmadoEm;
  final String? confirmadoPorNome;
  final DateTime? canceladoEm;
  final String? canceladoPorNome;
  final String? cancelamentoMotivo;

  bool get isAtivo => status == 'Pending' || status == 'Confirmed';

  factory KidsPreCheckinDto.fromJson(Map<String, dynamic> json) {
    return KidsPreCheckinDto(
      id: (json['id'] as num).toInt(),
      criancaPessoaId: (json['criancaPessoaId'] as num).toInt(),
      criancaNome: json['criancaNome'] as String? ?? '',
      responsavelPessoaId: (json['responsavelPessoaId'] as num).toInt(),
      responsavelNome: json['responsavelNome'] as String? ?? '',
      eventoOcorrenciaId: (json['eventoOcorrenciaId'] as num?)?.toInt(),
      checkinId: (json['checkinId'] as num?)?.toInt(),
      eventoDataHoraInicio: json['eventoDataHoraInicio'] != null
          ? DateTime.tryParse(json['eventoDataHoraInicio'] as String)
          : null,
      salaId: json['salaId'] as String?,
      turmaId: json['turmaId'] as String?,
      qrToken: json['qrToken'] as String? ?? '',
      codigoCurto: json['codigoCurto'] as String? ?? '',
      status: json['status'] as String? ?? '',
      expiraEm: DateTime.parse(json['expiraEm'] as String),
      observacoesResponsavel: json['observacoesResponsavel'] as String?,
      criadoEm: DateTime.parse(json['criadoEm'] as String),
      confirmadoEm: json['confirmadoEm'] != null
          ? DateTime.tryParse(json['confirmadoEm'] as String)
          : null,
      confirmadoPorNome: json['confirmadoPorNome'] as String?,
      canceladoEm: json['canceladoEm'] != null
          ? DateTime.tryParse(json['canceladoEm'] as String)
          : null,
      canceladoPorNome: json['canceladoPorNome'] as String?,
      cancelamentoMotivo: json['cancelamentoMotivo'] as String?,
    );
  }
}

class RetiradaValidacaoDto {
  RetiradaValidacaoDto({
    required this.checkinId,
    required this.criancaPessoaId,
    required this.criancaNome,
    this.salaId,
    required this.checkinTime,
    this.tokenRetiradaExpiraEm,
    required this.expirado,
    required this.metodoValidado,
    required this.metodosDisponiveis,
    required this.responsaveisAutorizados,
  });

  final int checkinId;
  final int criancaPessoaId;
  final String criancaNome;
  final String? salaId;
  final DateTime checkinTime;
  final DateTime? tokenRetiradaExpiraEm;
  final bool expirado;
  final String metodoValidado;
  final List<String> metodosDisponiveis;
  final List<RetiradaAutorizadoDto> responsaveisAutorizados;

  factory RetiradaValidacaoDto.fromJson(Map<String, dynamic> json) {
    return RetiradaValidacaoDto(
      checkinId: (json['checkinId'] as num).toInt(),
      criancaPessoaId: (json['criancaPessoaId'] as num).toInt(),
      criancaNome: json['criancaNome'] as String? ?? '',
      salaId: json['salaId'] as String?,
      checkinTime: DateTime.parse(json['checkinTime'] as String),
      tokenRetiradaExpiraEm: json['tokenRetiradaExpiraEm'] != null
          ? DateTime.tryParse(json['tokenRetiradaExpiraEm'] as String)
          : null,
      expirado: json['expirado'] as bool? ?? false,
      metodoValidado: json['metodoValidado'] as String? ?? '',
      metodosDisponiveis: (json['metodosDisponiveis'] as List<dynamic>? ?? [])
          .map((e) => e.toString())
          .toList(),
      responsaveisAutorizados: (json['responsaveisAutorizados'] as List<dynamic>? ?? [])
          .map((e) => RetiradaAutorizadoDto.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }
}

class RetiradaAutorizadoDto {
  RetiradaAutorizadoDto({
    required this.responsavelPessoaId,
    required this.responsavelNome,
    this.parentesco,
    required this.podeRetirar,
  });

  final int responsavelPessoaId;
  final String responsavelNome;
  final String? parentesco;
  final bool podeRetirar;

  factory RetiradaAutorizadoDto.fromJson(Map<String, dynamic> json) {
    return RetiradaAutorizadoDto(
      responsavelPessoaId: (json['responsavelPessoaId'] as num).toInt(),
      responsavelNome: json['responsavelNome'] as String? ?? '',
      parentesco: json['parentesco'] as String?,
      podeRetirar: json['podeRetirar'] as bool? ?? false,
    );
  }
}

class MeuAvisoKidsDto {
  MeuAvisoKidsDto({
    required this.id,
    required this.titulo,
    required this.mensagem,
    required this.tipo,
    required this.origem,
    this.criancaPessoaId,
    this.criancaNome,
    required this.dataCriacao,
    this.enviadoEm,
    this.lidoEm,
    required this.foiLido,
  });

  final int id;
  final String titulo;
  final String mensagem;
  final String tipo;
  final String origem;
  final int? criancaPessoaId;
  final String? criancaNome;
  final DateTime dataCriacao;
  final DateTime? enviadoEm;
  final DateTime? lidoEm;
  final bool foiLido;

  factory MeuAvisoKidsDto.fromJson(Map<String, dynamic> json) {
    return MeuAvisoKidsDto(
      id: (json['id'] as num).toInt(),
      titulo: json['titulo'] as String? ?? '',
      mensagem: json['mensagem'] as String? ?? '',
      tipo: json['tipo'] as String? ?? '',
      origem: json['origem'] as String? ?? '',
      criancaPessoaId: (json['criancaPessoaId'] as num?)?.toInt(),
      criancaNome: json['criancaNome'] as String?,
      dataCriacao: DateTime.parse(json['dataCriacao'] as String),
      enviadoEm: json['enviadoEm'] != null
          ? DateTime.tryParse(json['enviadoEm'] as String)
          : null,
      lidoEm: json['lidoEm'] != null
          ? DateTime.tryParse(json['lidoEm'] as String)
          : null,
      foiLido: json['foiLido'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'titulo': titulo,
        'mensagem': mensagem,
        'tipo': tipo,
        'origem': origem,
        'criancaPessoaId': criancaPessoaId,
        'criancaNome': criancaNome,
        'dataCriacao': dataCriacao.toIso8601String(),
        'enviadoEm': enviadoEm?.toIso8601String(),
        'lidoEm': lidoEm?.toIso8601String(),
        'foiLido': foiLido,
      };
}
