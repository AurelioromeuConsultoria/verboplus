import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';

/// Cache local simples em shared_preferences.
/// Guarda JSON + timestamp para cada chave.
class CacheService {
  static const _criancasKey = 'cache_v1_criancas';
  static const _avisosKey = 'cache_v1_avisos';

  Future<SharedPreferences> get _prefs => SharedPreferences.getInstance();

  Future<void> saveCriancas(List<Map<String, dynamic>> data) =>
      _save(_criancasKey, data);

  Future<List<Map<String, dynamic>>?> loadCriancas() =>
      _load(_criancasKey);

  Future<void> saveAvisos(List<Map<String, dynamic>> data) =>
      _save(_avisosKey, data);

  Future<List<Map<String, dynamic>>?> loadAvisos() =>
      _load(_avisosKey);

  Future<void> _save(String key, List<Map<String, dynamic>> data) async {
    final prefs = await _prefs;
    await prefs.setString(key, jsonEncode(data));
  }

  Future<List<Map<String, dynamic>>?> _load(String key) async {
    final prefs = await _prefs;
    final raw = prefs.getString(key);
    if (raw == null) return null;
    try {
      final list = jsonDecode(raw) as List<dynamic>;
      return list.cast<Map<String, dynamic>>();
    } catch (_) {
      return null;
    }
  }

  Future<void> clear() async {
    final prefs = await _prefs;
    await prefs.remove(_criancasKey);
    await prefs.remove(_avisosKey);
  }
}
