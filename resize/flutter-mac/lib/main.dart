import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter Resize Demo',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepPurple),
        useMaterial3: true,
      ),
      home: const MyHomePage(),
    );
  }
}

class MyHomePage extends StatelessWidget {
  const MyHomePage({super.key});

  static const platform = MethodChannel('nativno/resize');

  Future<void> _setSize(double width, double height) async {
    try {
      await platform.invokeMethod('setSize', {'width': width, 'height': height});
    } on PlatformException catch (e) {
      debugPrint("Failed to set size: '${e.message}'.");
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        title: const Text('Native window resize playground'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Wrap(
              spacing: 10,
              children: [
                ElevatedButton(
                  onPressed: () => _setSize(600, 450),
                  child: const Text('Small'),
                ),
                ElevatedButton(
                  onPressed: () => _setSize(1200, 800),
                  child: const Text('Middle'),
                ),
                ElevatedButton(
                  onPressed: () => _setSize(1800, 1200),
                  child: const Text('Large'),
                ),
              ],
            ),
            const SizedBox(height: 20),
            const Text('Try resizing the app window with the mouse after pressing a button.'),
          ],
        ),
      ),
    );
  }
}
