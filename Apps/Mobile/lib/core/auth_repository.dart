import 'dart:convert';
import 'package:http/http.dart' as http;
import 'api_client.dart';

class AuthRepository {
  AuthRepository(this._api);

  final ApiClient _api;

  /// POST /api/auth/login
  /// Body: { "email": "...", "senha": "..." }
  Future<LoginResult> login(String email, String senha) async {
    final response = await _api.post(
      '/api/auth/login',
      body: {'email': email, 'senha': senha},
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      final token = data['token'] as String?;
      final refreshToken = data['refreshToken'] as String?;
      final usuario = data['usuario'] as Map<String, dynamic>?;
      if (token == null || refreshToken == null || usuario == null) {
        return LoginResult.failure('Resposta inválida do servidor');
      }
      await _api.setTokens(token, refreshToken);
      return LoginResult.success(Usuario.fromJson(usuario));
    }

    if (response.statusCode == 401) {
      final msg = _errorMessage(response);
      return LoginResult.failure(msg);
    }
    return LoginResult.failure(_errorMessage(response));
  }

  /// GET /api/auth/me (com token)
  Future<Usuario?> me() async {
    final response = await _api.get('/api/auth/me');
    if (response.statusCode != 200) return null;
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    return Usuario.fromJson(data);
  }

  Future<void> logout() async {
    await _api.clearTokens();
  }

  /// PUT /api/auth/alterar-senha
  Future<void> alterarSenha(String senhaAtual, String novaSenha) async {
    final response = await _api.put(
      '/api/auth/alterar-senha',
      body: {'senhaAtual': senhaAtual, 'novaSenha': novaSenha},
    );
    if (response.statusCode == 204) return;
    throw Exception(_errorMessage(response));
  }

  /// POST /api/auth/registrar-responsavel
  Future<LoginResult> registrarResponsavel({
    required String tenantSlug,
    required String nome,
    required String email,
    required String senha,
    String? telefone,
    String? whatsApp,
  }) async {
    final response = await _api.post(
      '/api/auth/registrar-responsavel',
      body: {
        'tenantSlug': tenantSlug,
        'nome': nome,
        'email': email,
        'senha': senha,
        if (telefone != null) 'telefone': telefone,
        if (whatsApp != null) 'whatsApp': whatsApp,
      },
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      final token = data['token'] as String?;
      final refreshToken = data['refreshToken'] as String?;
      final usuario = data['usuario'] as Map<String, dynamic>?;
      if (token == null || refreshToken == null || usuario == null) {
        return LoginResult.failure('Resposta inválida do servidor');
      }
      await _api.setTokens(token, refreshToken);
      return LoginResult.success(Usuario.fromJson(usuario));
    }
    return LoginResult.failure(_errorMessage(response));
  }

  String _errorMessage(http.Response response) {
    try {
      final m = jsonDecode(response.body);
      if (m is Map && m['message'] != null) return m['message'] as String;
    } catch (_) {}
    return 'Erro ao fazer login. Tente novamente.';
  }
}

class Usuario {
  Usuario({
    required this.id,
    required this.pessoaId,
    required this.nome,
    required this.email,
  });

  final int id;
  final int pessoaId;
  final String nome;
  final String email;

  factory Usuario.fromJson(Map<String, dynamic> json) {
    return Usuario(
      id: (json['id'] as num).toInt(),
      pessoaId: (json['pessoaId'] as num).toInt(),
      nome: json['nome'] as String? ?? '',
      email: json['email'] as String? ?? json['emailLogin'] as String? ?? '',
    );
  }
}

sealed class LoginResult {
  const LoginResult();
  factory LoginResult.success(Usuario user) = LoginSuccess;
  factory LoginResult.failure(String message) = LoginFailure;
}

class LoginSuccess extends LoginResult {
  LoginSuccess(this.user);
  final Usuario user;
}

class LoginFailure extends LoginResult {
  LoginFailure(this.message);
  final String message;
}
