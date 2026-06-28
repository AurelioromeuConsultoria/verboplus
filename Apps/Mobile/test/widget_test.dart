import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('MaterialApp básico renderiza sem erro', (WidgetTester tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: Text('App Kids'),
        ),
      ),
    );

    expect(find.text('App Kids'), findsOneWidget);
  });

  testWidgets('SafeArea e Scaffold renderizam filhos corretamente', (WidgetTester tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: SafeArea(
            child: Column(
              children: [
                Text('Header'),
                Text('Content'),
              ],
            ),
          ),
        ),
      ),
    );

    expect(find.text('Header'), findsOneWidget);
    expect(find.text('Content'), findsOneWidget);
  });

  testWidgets('FilledButton chama callback ao ser pressionado', (WidgetTester tester) async {
    var tapped = false;

    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: FilledButton(
            onPressed: () => tapped = true,
            child: const Text('Entrar'),
          ),
        ),
      ),
    );

    await tester.tap(find.text('Entrar'));
    expect(tapped, isTrue);
  });

  testWidgets('ListView renderiza múltiplos itens', (WidgetTester tester) async {
    await tester.pumpWidget(
      MaterialApp(
        home: Scaffold(
          body: ListView(
            children: const [
              Text('Item 1'),
              Text('Item 2'),
              Text('Item 3'),
            ],
          ),
        ),
      ),
    );

    expect(find.text('Item 1'), findsOneWidget);
    expect(find.text('Item 2'), findsOneWidget);
    expect(find.text('Item 3'), findsOneWidget);
  });

  testWidgets('CircleAvatar com inicial renderiza corretamente', (WidgetTester tester) async {
    await tester.pumpWidget(
      const MaterialApp(
        home: Scaffold(
          body: CircleAvatar(
            child: Text('M'),
          ),
        ),
      ),
    );

    expect(find.text('M'), findsOneWidget);
  });
}
