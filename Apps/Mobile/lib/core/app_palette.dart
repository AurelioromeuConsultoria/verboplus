import 'package:flutter/material.dart';

class AppPalette {
  AppPalette._();

  // Primary brand
  static const Color primary = Color(0xFF5B5BD6);
  static const Color primarySoft = Color(0xFFEEEDFD);

  // Semantic
  static const Color success = Color(0xFF079455);
  static const Color successBg = Color(0xFFECFDF3);
  static const Color warning = Color(0xFFDC6803);
  static const Color warningBg = Color(0xFFFFFAEB);
  static const Color danger = Color(0xFFB42318);
  static const Color dangerBg = Color(0xFFFEF3F2);
  static const Color info = Color(0xFF1570EF);
  static const Color infoBg = Color(0xFFEFF8FF);

  // Neutrals
  static const Color bg = Color(0xFFF5F7FF);
  static const Color card = Color(0xFFFFFFFF);
  static const Color ink = Color(0xFF101828);
  static const Color midInk = Color(0xFF475569);
  static const Color lightInk = Color(0xFF94A3B8);
  static const Color border = Color(0xFFE4E7EC);
  static const Color divider = Color(0xFFF2F4F7);

  // Legacy aliases (for login screen + shared widgets)
  static const Color deepSea = Color(0xFF1D2939);
  static const Color mutedInk = midInk;
  static const Color line = border;
  static const Color shell = card;
  static const Color cream = bg;
  static const Color fog = divider;
  static const Color lilac = primary;
  static const Color aqua = Color(0xFF06B6D4);
  static const Color sunshine = Color(0xFFF59E0B);
  static const Color sage = Color(0xFF099250);
  static const Color apricot = Color(0xFFF97316);
  static const Color lagoon = Color(0xFF2B87A0);
  static const Color roseSand = Color(0xFFE9C3B0);
  static const Color cloud = divider;

  // Kid identity colors (cycling by pessoaId)
  static const List<Color> _kidColors = [
    Color(0xFF6941C6), // violet
    Color(0xFF1570EF), // blue
    Color(0xFF099250), // green
    Color(0xFFDC6803), // orange
    Color(0xFFC01574), // pink
    Color(0xFF0E7090), // teal
    Color(0xFF9B1DCA), // purple
    Color(0xFFD92D20), // red
  ];

  static Color kidColor(int id) => _kidColors[id.abs() % _kidColors.length];
  static Color kidColorSoft(int id) =>
      Color.alphaBlend(kidColor(id).withValues(alpha: 0.13), Colors.white);

  // Kept for offline_banner + any remaining usage
  static const LinearGradient appBackground = LinearGradient(
    begin: Alignment.topCenter,
    end: Alignment.bottomCenter,
    colors: [bg, bg],
  );

  static const LinearGradient heroGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [primary, Color(0xFF4338CA)],
  );

  static const LinearGradient warmGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFFFF0C8), Color(0xFFFFE1C7)],
  );

  static const LinearGradient mistGradient = LinearGradient(
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
    colors: [Color(0xFFFFFFFF), Color(0xFFF3F8FF)],
  );
}
