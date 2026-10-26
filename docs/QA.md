# Qualitätssicherung – 25.10.2026

Die ausführbare Vorabversion ist die ergänzende Three.js-/Capacitor-Laufzeit in `WebApp/`. Das ursprüngliche Unity-Projekt bleibt erhalten. **Kompilation, ausgeführte Tests, Rendering und reale Geräte sind getrennte Nachweise.**

## Tatsächlich ausgeführte Prüfungen

| Prüfung | Nachweis | Ergebnis |
|---|---|---|
| C#-Logik und Story, Hilfe, Pacing, Elternregeln | NUnit/NUnitLite; zusätzlich echter GitHub-Actions-Testlauf | 27 bestanden |
| JavaScript-Logik, sieben Tiere, zehn Rituale, Cue-Reihenfolge, Audioabdeckung | `npm test` | 26 bestanden |
| Unity-Runtime-API-Kompilation | RuntimeCompileCheck gegen Unity-2021.3-Referenzassemblies | bestanden; kein Unity-Editorlauf |
| PlayMode-Testcode kompilieren | PlayModeCompileCheck | bestanden; Tests nicht in Unity ausgeführt |
| Metadaten, Audiohashes, Storydaten | `unity_metadata.py --check`, `verify_assets.py` | bestanden |
| Unity-Kameraziele rechnerisch | `check_camera_framing.py` | 8 Prüfungen bestanden; kein Renderbeweis |
| Browser: Suris drei Rituale mit Zeigereingabe | gerendertes Chromium, Playwright | bestanden; keine Selbsthilfe |
| Browser: alle sieben, alle zehn Rituale mit Zeigereingabe | gerendertes Chromium, Playwright | bestanden; keine Selbsthilfe |
| Browser: alle sieben ohne Eingabe | gerendertes Chromium, Playwright | bestanden; zehn Selbsthilfen |
| Browser: falsche Berührungen, Pause/Fortsetzen, Elterngate, Auswahl und lokale Speicherung | Playwright | bestanden |
| Browser: Offline-Neustart und lokale Sprachdateien, beschädigter Speicher, fehlende Sprachdateien | Playwright | bestanden |
| Browser: Touch-Ereignisse und zweiter Finger | Chromium Touch-Emulation | bestanden; kein physisches Gerät |
| Android-Debug-APK | lokales Gradle/JDK-21-/API-36-Buildsystem | gebaut, Paket und 101 Audiohashes geprüft |
| iOS, unsignierter Geräte-Build | GitHub Actions, Xcode 26.6 / 17F113 | bestanden; kein signiertes IPA und kein Gerätetest |
| Tatsächliches Rendering | 15 originale Browser-Screenshots, Suri-Ablaufvideo | visuell geprüft; Software-WebGL2 |

Die abschließenden Berichtdateien und der CI-Lauf sind in [evidence/VERIFICATION.md](evidence/VERIFICATION.md) aufgeführt. Zwei zusätzliche Browserfälle für den GPU-Stopp im Audiomodus und die Wiederherstellung nach Grafik-Kontextverlust sind ebenfalls bestanden. Lokal sind elf unterschiedliche Fälle ausgeführt und nach einer korrigierten Schrittzählungs-Assertion bestanden (zehn im Gesamtlauf, Suri im gezielten Wiederholungslauf). Der vollständige GitHub-Lauf ist ebenfalls erfolgreich: **11/11 Browsertests**, anschließend echte automatisierte Aufnahmen und Produktionsbuild. C#, Android und iOS sind grün; Unity wurde ausdrücklich übersprungen.

## Ausdrücklich nicht ausgeführt

- Unity-2022.3.50f1-Baseline im Editor, Unity EditMode und Unity PlayMode: **blockiert**, kein ausführbarer aktivierter Editor und kein vollständiger sicherer Unity-Secrets-Satz. Die Lizenz-Vorprüfung ist kein Test-Pass.
- Unity-Shader-/URP-Rendering und Unity-Mobilbuilds: nicht ausgeführt. Die echte Browser-Renderprüfung belegt ausschließlich die Three.js-Laufzeit.
- Unity-6.3-LTS-Migration: nicht vorgenommen, weil keine ausgeführte Unity-Baseline vorliegt.
- Physische iOS-/Android-Geräte, Safari-/WebView-Matrix, Installations-/Touch-Tests, Energie/RAM/FPS, gesperrter Bildschirm, Tonunterbrechungen und VoiceOver/TalkBack: offen.
- Endgültige Sprecher-, Art- und Eltern-/Kind-Abnahme: offen. Das QA-Video ist beschleunigt und ohne Ton; keine Abnahme natürlicher Bewegungsdauer oder Audioqualität daraus ableiten.

## Referenzgeräte und Grenzen

iPhone SE (2020, A13), aktuelles iPhone, iPad mit 4:3; Galaxy A52/6 GB und Android 10+ mit aktuellem System-WebView. Minimum: iOS 16 und Android 10/API 29; Build-Target API 36.

Ziel: stabile 30 FPS, P95-Framezeit ≤40 ms, keine anhaltenden Phasen >50 ms; ≤150 Draw Calls, ≤250.000 sichtbare Dreiecke, RAM-Ziel ≤250 MB. Temperatur und Akkuverbrauch über mindestens zehn Minuten am Gerät messen. SwiftShader auf diesem Host ist ein funktionaler Rendernachweis, kein Performance-Pass auf einem Mobilgerät.

## Reproduzieren

```sh
npm --prefix WebApp ci
npm --prefix WebApp test
cd WebApp
node tools/sync-content.mjs
npx vite build --mode qa
node tools/offline-manifest.mjs
npx playwright install --with-deps chromium
npx playwright test
SCHLAFWALD_EVIDENCE=/tmp/schlafwald-evidence node tools/capture.mjs
```

`npm run build` baut die Produktion ohne QA-Steuerung. Die CI lädt Logikberichte, Browserberichte, echte Renderaufnahmen, Offline-Webbuild, Android-APK und Xcode-Log als Artefakte hoch. Unity-Jobs werden bei fehlender Aktivierung ausdrücklich übersprungen.


Aktualisierung 26.10.2026: Freie Tierbesuche und ausgewählte Emma-Hörprobe siehe [VISITS_AND_VOICE.md](VISITS_AND_VOICE.md). Die vollständige neue Emma-Vertonung ist noch offen.
