import 'package:flutter_test/flutter_test.dart';
import 'package:app_kids/features/kids/kids_repository.dart';

void main() {
  group('MinhaCriancaResumoDto', () {
    final Map<String, dynamic> fullJson = {
      'pessoaId': 42,
      'nome': 'Maria Silva',
      'dataNascimento': '2018-03-15T00:00:00',
      'salaId': 'sala-1',
      'estaCheckedIn': true,
      'checkinAtual': {
        'id': 10,
        'criancaPessoaId': 42,
        'criancaNome': 'Maria Silva',
        'checkinTime': '2026-06-25T09:00:00',
        'checkoutTime': null,
        'status': 'CheckedIn',
        'salaId': 'sala-1',
        'tokenRetirada': 'TOKEN123',
        'pinRetirada': '4567',
        'tokenRetiradaExpiraEm': null,
      },
      'temAlertaCritico': false,
      'fotoUrl': 'https://example.com/foto.jpg',
    };

    test('fromJson mapeia todos os campos corretamente', () {
      final dto = MinhaCriancaResumoDto.fromJson(fullJson);

      expect(dto.pessoaId, 42);
      expect(dto.nome, 'Maria Silva');
      expect(dto.dataNascimento, DateTime.parse('2018-03-15T00:00:00'));
      expect(dto.salaId, 'sala-1');
      expect(dto.estaCheckedIn, isTrue);
      expect(dto.checkinAtual, isNotNull);
      expect(dto.checkinAtual!.tokenRetirada, 'TOKEN123');
      expect(dto.temAlertaCritico, isFalse);
      expect(dto.fotoUrl, 'https://example.com/foto.jpg');
    });

    test('fromJson aceita fotoUrl nulo', () {
      final json = Map<String, dynamic>.from(fullJson)..remove('fotoUrl');
      final dto = MinhaCriancaResumoDto.fromJson(json);
      expect(dto.fotoUrl, isNull);
    });

    test('fromJson aceita dataNascimento nulo', () {
      final json = Map<String, dynamic>.from(fullJson)
        ..['dataNascimento'] = null
        ..remove('checkinAtual');
      final dto = MinhaCriancaResumoDto.fromJson(json);
      expect(dto.dataNascimento, isNull);
    });

    test('toJson serializa de volta corretamente', () {
      final dto = MinhaCriancaResumoDto.fromJson(fullJson);
      final json = dto.toJson();

      expect(json['pessoaId'], 42);
      expect(json['nome'], 'Maria Silva');
      expect(json['estaCheckedIn'], isTrue);
      expect(json['fotoUrl'], 'https://example.com/foto.jpg');
      expect(json['temAlertaCritico'], isFalse);
    });

    test('toJson → fromJson round-trip preserva dados', () {
      final original = MinhaCriancaResumoDto.fromJson(fullJson);
      final json = original.toJson();
      final restored = MinhaCriancaResumoDto.fromJson(json);

      expect(restored.pessoaId, original.pessoaId);
      expect(restored.nome, original.nome);
      expect(restored.estaCheckedIn, original.estaCheckedIn);
      expect(restored.fotoUrl, original.fotoUrl);
      expect(restored.salaId, original.salaId);
    });

    test('fromJson usa fallback vazio para nome ausente', () {
      final dto = MinhaCriancaResumoDto.fromJson({'pessoaId': 1, 'estaCheckedIn': false, 'temAlertaCritico': false});
      expect(dto.nome, '');
    });
  });

  group('MinhaCriancaDetalheDto', () {
    final Map<String, dynamic> fullJson = {
      'pessoaId': 5,
      'nome': 'João Pedro',
      'dataNascimento': '2019-07-10T00:00:00',
      'salaId': 'sala-2',
      'turmaId': null,
      'alergias': 'Amendoim',
      'restricoesAlimentares': null,
      'observacoesVisiveisAoResponsavel': 'Criança muito ativa',
      'estaCheckedIn': false,
      'checkinAtual': null,
      'historicoRecente': [
        {
          'id': 7,
          'criancaPessoaId': 5,
          'criancaNome': 'João Pedro',
          'checkinTime': '2026-06-22T10:00:00',
          'checkoutTime': '2026-06-22T12:00:00',
          'status': 'CheckedOut',
          'salaId': 'sala-2',
          'tokenRetirada': null,
          'pinRetirada': null,
          'tokenRetiradaExpiraEm': null,
        },
      ],
      'fotoUrl': null,
    };

    test('fromJson mapeia todos os campos', () {
      final dto = MinhaCriancaDetalheDto.fromJson(fullJson);

      expect(dto.pessoaId, 5);
      expect(dto.nome, 'João Pedro');
      expect(dto.alergias, 'Amendoim');
      expect(dto.restricoesAlimentares, isNull);
      expect(dto.observacoesVisiveisAoResponsavel, 'Criança muito ativa');
      expect(dto.estaCheckedIn, isFalse);
      expect(dto.historicoRecente, hasLength(1));
      expect(dto.historicoRecente.first.status, 'CheckedOut');
      expect(dto.fotoUrl, isNull);
    });

    test('fromJson com fotoUrl preenchida', () {
      final json = Map<String, dynamic>.from(fullJson)
        ..['fotoUrl'] = 'https://cdn.example.com/kids/5.jpg';
      final dto = MinhaCriancaDetalheDto.fromJson(json);
      expect(dto.fotoUrl, 'https://cdn.example.com/kids/5.jpg');
    });

    test('historicoRecente vazio quando ausente no JSON', () {
      final json = Map<String, dynamic>.from(fullJson)..remove('historicoRecente');
      final dto = MinhaCriancaDetalheDto.fromJson(json);
      expect(dto.historicoRecente, isEmpty);
    });
  });

  group('MeuCheckinResumoDto', () {
    final Map<String, dynamic> json = {
      'id': 99,
      'criancaPessoaId': 5,
      'criancaNome': 'João Pedro',
      'checkinTime': '2026-06-25T08:30:00Z',
      'checkoutTime': null,
      'status': 'CheckedIn',
      'salaId': 'sala-A',
      'tokenRetirada': 'ABC123',
      'pinRetirada': '1234',
      'tokenRetiradaExpiraEm': '2026-06-25T11:00:00Z',
    };

    test('fromJson parseia datas ISO 8601', () {
      final dto = MeuCheckinResumoDto.fromJson(json);
      expect(dto.id, 99);
      expect(dto.checkinTime, DateTime.parse('2026-06-25T08:30:00Z'));
      expect(dto.checkoutTime, isNull);
      expect(dto.tokenRetirada, 'ABC123');
      expect(dto.pinRetirada, '1234');
      expect(dto.tokenRetiradaExpiraEm, isNotNull);
    });

    test('toJson → fromJson round-trip preserva dados', () {
      final original = MeuCheckinResumoDto.fromJson(json);
      final restored = MeuCheckinResumoDto.fromJson(original.toJson());
      expect(restored.id, original.id);
      expect(restored.status, original.status);
      expect(restored.tokenRetirada, original.tokenRetirada);
      expect(restored.checkinTime, original.checkinTime);
    });
  });

  group('MeuAvisoKidsDto', () {
    final Map<String, dynamic> json = {
      'id': 1,
      'titulo': 'Aviso de segurança',
      'mensagem': 'Portão fechado às 10h.',
      'tipo': 'AVISO_SEGURANCA',
      'origem': 'ADMIN',
      'criancaPessoaId': 42,
      'criancaNome': 'Maria Silva',
      'dataCriacao': '2026-06-25T07:00:00',
      'enviadoEm': '2026-06-25T07:01:00',
      'lidoEm': null,
      'foiLido': false,
    };

    test('fromJson mapeia campos corretamente', () {
      final dto = MeuAvisoKidsDto.fromJson(json);
      expect(dto.id, 1);
      expect(dto.titulo, 'Aviso de segurança');
      expect(dto.mensagem, 'Portão fechado às 10h.');
      expect(dto.tipo, 'AVISO_SEGURANCA');
      expect(dto.criancaPessoaId, 42);
      expect(dto.foiLido, isFalse);
      expect(dto.lidoEm, isNull);
    });

    test('toJson → fromJson round-trip preserva dados', () {
      final original = MeuAvisoKidsDto.fromJson(json);
      final restored = MeuAvisoKidsDto.fromJson(original.toJson());
      expect(restored.id, original.id);
      expect(restored.titulo, original.titulo);
      expect(restored.foiLido, original.foiLido);
    });

    test('fromJson aviso lido com lidoEm preenchido', () {
      final lido = Map<String, dynamic>.from(json)
        ..['lidoEm'] = '2026-06-25T08:00:00'
        ..['foiLido'] = true;
      final dto = MeuAvisoKidsDto.fromJson(lido);
      expect(dto.foiLido, isTrue);
      expect(dto.lidoEm, isNotNull);
    });
  });

  group('KidsPreCheckinDto', () {
    final Map<String, dynamic> json = {
      'id': 55,
      'criancaPessoaId': 42,
      'criancaNome': 'Maria Silva',
      'responsavelPessoaId': 1,
      'responsavelNome': 'Ana Silva',
      'eventoOcorrenciaId': null,
      'checkinId': null,
      'eventoDataHoraInicio': null,
      'salaId': 'sala-kids',
      'turmaId': null,
      'qrToken': 'QRTOKEN_ABC',
      'codigoCurto': 'AB12',
      'status': 'Pending',
      'expiraEm': '2026-06-25T14:00:00',
      'observacoesResponsavel': null,
      'criadoEm': '2026-06-25T09:00:00',
      'confirmadoEm': null,
      'confirmadoPorNome': null,
      'canceladoEm': null,
      'canceladoPorNome': null,
      'cancelamentoMotivo': null,
    };

    test('fromJson mapeia campos corretamente', () {
      final dto = KidsPreCheckinDto.fromJson(json);
      expect(dto.id, 55);
      expect(dto.qrToken, 'QRTOKEN_ABC');
      expect(dto.codigoCurto, 'AB12');
      expect(dto.status, 'Pending');
    });

    test('isAtivo retorna true quando Pending ou Confirmed', () {
      final pending = KidsPreCheckinDto.fromJson(json);
      expect(pending.isAtivo, isTrue);

      final confirmed = KidsPreCheckinDto.fromJson(
        Map<String, dynamic>.from(json)..['status'] = 'Confirmed',
      );
      expect(confirmed.isAtivo, isTrue);
    });

    test('isAtivo retorna false para status cancelado ou expirado', () {
      for (final status in ['Cancelled', 'Expired', 'CheckedIn']) {
        final dto = KidsPreCheckinDto.fromJson(
          Map<String, dynamic>.from(json)..['status'] = status,
        );
        expect(dto.isAtivo, isFalse, reason: 'status: $status deveria ser inativo');
      }
    });
  });
}
