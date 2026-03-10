import 'package:flutter/foundation.dart';
import 'core/auth_repository.dart';

/// Estado global da aplicação (usuário logado).
class AppState extends ChangeNotifier {
  AppState({Usuario? initialUser}) : _user = initialUser;

  Usuario? _user;
  Usuario? get user => _user;

  void setUser(Usuario? user) {
    _user = user;
    notifyListeners();
  }
}
