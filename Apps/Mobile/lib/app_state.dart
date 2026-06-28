import 'package:flutter/foundation.dart';
import 'core/auth_repository.dart';

class AppState extends ChangeNotifier {
  AppState({Usuario? initialUser}) : _user = initialUser;

  Usuario? _user;
  Usuario? get user => _user;

  int _naoLidosCount = 0;
  int get naoLidosCount => _naoLidosCount;

  void setUser(Usuario? user) {
    _user = user;
    notifyListeners();
  }

  void setNaoLidosCount(int count) {
    if (_naoLidosCount == count) return;
    _naoLidosCount = count;
    notifyListeners();
  }

  void incrementNaoLidos() {
    _naoLidosCount++;
    notifyListeners();
  }
}
