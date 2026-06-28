import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;

/// Cliente HTTP para as APIs do backend VerboPlus.
/// Tokens persistidos no keychain (iOS) / keystore Android via flutter_secure_storage.
class ApiClient {
  ApiClient({required this.baseUrl});

  final String baseUrl;

  static const _tokenKey = 'auth_token';
  static const _refreshKey = 'refresh_token';

  static const _storage = FlutterSecureStorage(
    aOptions: AndroidOptions(encryptedSharedPreferences: true),
    iOptions: IOSOptions(accessibility: KeychainAccessibility.first_unlock),
  );

  Future<String?> getToken() => _storage.read(key: _tokenKey);
  Future<String?> getRefreshToken() => _storage.read(key: _refreshKey);

  Future<void> setTokens(String token, String refreshToken) async {
    await _storage.write(key: _tokenKey, value: token);
    await _storage.write(key: _refreshKey, value: refreshToken);
  }

  Future<void> clearTokens() async {
    await _storage.delete(key: _tokenKey);
    await _storage.delete(key: _refreshKey);
  }

  Future<http.Response> get(
    String path, {
    Map<String, String>? headers,
    Map<String, String>? queryParams,
  }) async {
    final token = await getToken();
    final uri = Uri.parse('$baseUrl$path').replace(queryParameters: queryParams);
    return http.get(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
        ...?headers,
      },
    );
  }

  Future<http.Response> post(
    String path, {
    Object? body,
    Map<String, String>? headers,
  }) async {
    final token = await getToken();
    final uri = Uri.parse('$baseUrl$path');
    return http.post(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
        ...?headers,
      },
      body: body != null ? jsonEncode(body) : null,
    );
  }

  Future<http.Response> put(
    String path, {
    Object? body,
    Map<String, String>? headers,
  }) async {
    final token = await getToken();
    final uri = Uri.parse('$baseUrl$path');
    return http.put(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
        ...?headers,
      },
      body: body != null ? jsonEncode(body) : null,
    );
  }

  Future<http.Response> patch(
    String path, {
    Object? body,
    Map<String, String>? headers,
  }) async {
    final token = await getToken();
    final uri = Uri.parse('$baseUrl$path');
    return http.patch(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
        ...?headers,
      },
      body: body != null ? jsonEncode(body) : null,
    );
  }

  Future<http.Response> delete(
    String path, {
    Object? body,
    Map<String, String>? headers,
  }) async {
    final token = await getToken();
    final uri = Uri.parse('$baseUrl$path');
    return http.delete(
      uri,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
        ...?headers,
      },
      body: body != null ? jsonEncode(body) : null,
    );
  }

  bool isUnauthorized(http.Response response) => response.statusCode == 401;
}
