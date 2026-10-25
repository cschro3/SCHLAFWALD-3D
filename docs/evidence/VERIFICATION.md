# Nachprüfbarer Lieferstand – 25.10.2026

**Prüfbare Vorabversion, kein abgeschlossenes Store-Release.** Die ausführbare Laufzeit ist Three.js/Capacitor innerhalb des bestehenden Repositories. Das originale Unity-Projekt bleibt auf 2022.3.50f1/URP 14 erhalten.

- Private Vorschau: https://schlafwald-gute-nacht-3d.speedysnow4.chatgpt.site
- Entwicklungsbranch: https://github.com/cschro3/SCHLAFWALD-3D/tree/work/schlafwald-verified-3d
- Draft-PR: https://github.com/cschro3/SCHLAFWALD-3D/pull/1

## Funktionsumfang der ausgeführten Laufzeit

| Tier | Habitat und Ritual | Verifikation |
|---|---|---|
| Fino | Höhle, Decke zurechtziehen, Einkuscheln/Schwanzpose, Atmung | echtes Rendering und vollständige Geschichte mit/ohne Zeiger |
| Mila | Moosnest, Streicheln, Ohren entspannen, Lider schließen | ebenso |
| Piko | Blätternest, Blätter sammeln, Einrollen | ebenso; feines finales Schnarch-/Foley-Design offen |
| Luna | Baum/Ast, Glühwürmchen dimmen, Kopf ins Gefieder | ebenso |
| Nuri | Baumhöhle, Kuscheleichel finden/halten, Schwanzdecke | ebenso |
| Leni | Lichtung/Moos, Kissen und Streicheln, Hinlegen | ebenso |
| Suri | begleiten, Blätterdach, Haus berühren, Fühler/Rückzug/Schlaf | drei Rituale durch echte Zeigereingabe ohne Selbsthilfe; Ablaufvideo |

Gemeinsame Story, zehn Rituale/vier Interaktionstypen, ruhiger Hilfeautomat, Finale und Abdunklung. Elternbereich mit 3-s-Halten plus Rechenaufgabe, Lautstärken, Wunschdauer, Tierauswahl, Pause/Fortsetzen/Neustart, dunklem Audiomodus und Abschalttimer. Keine Registrierung, Werbung oder Tracking. 101 lokale deutsche Piper/Thorsten-Aufnahmen; eigene synthetische Klangflächen und Wind. Keine fertige Sprecher-/Musik-Endabnahme behauptet.

## Ausgeführte Nachweise

- C#-Logik: **27/27** bestanden, [NUnit-XML](core-nunit.xml); zusätzlich echter Actions-Testlauf. API-Compilechecks für Runtime und PlayMode-Testcode bestanden. Diese sind keine Unity-EditMode-/PlayMode-Ausführung.
- JavaScript: **26/26** bestanden.
- Gerenderter Browser: sieben Tiere, zehn Rituale mit echten Pointer-Events ohne Assistenz sowie ohne Eingaben mit zehn Hilfen bestanden. Suri-Rückzug, Fehlberührungen, Pause, Elternschutz/Persistenz, Offline-Neustart, beschädigter Speicher, fehlende Audio-Dateien und Touch-/Zweitfingerverhalten geprüft. Schlussstatus der vollständigen CI-Suite siehe unten.
- Neu geprüft: dunkler Audiomodus beendet Suri ohne Eingaben und ohne weitere Renderframes; Grafik-Kontextverlust pausiert, Wiederherstellung setzt fort. Beide bestanden.
- Daten/Assets: **176** stabile Unity-Metadatenidentitäten, sieben Tiere/zehn Rituale/**101** Audio-Dateihashes, keine Fehler. Acht rechnerische Unity-Kameraprüfungen bestanden.
- Rendering: **15** originale Screenshots über Tiere, Elternbereich, Tablet, Telefon quer/hoch und Desktop; finales visuelles Review der sieben Korrekturen: [ship](visual-review.md). Das bedeutet Freigabe dieser begrenzten Review-Funde, keine Premium-Art-/Store-Abnahme.
- Renderlast in sieben aufgenommenen Szenen: **74–88 Calls**, **171.916–194.296 Dreiecke**; [Messwerte](render-metrics.json). Piko 183→88 Calls. Reines Software-WebGL2/SwiftShader, kein Mobilgeräte-FPS-Beweis.
- Android: frischer lokaler Gradle-Build erfolgreich, Debug-APK **14.233.488 Bytes**; SHA-256 `9a7268a0a4ec6c029ed66cbb9fbb20ae173a37c01cd7016de350cd140c52af8e`. Minimum API 29, Target 36, keine INTERNET-Berechtigung, 101 Audiohashes im APK geprüft, QA-Schnittstelle entfernt; [Paketprüfung](android-build.json). Nicht auf einem Gerät installiert.
- iOS: echter **unsignierter Xcode-26.6-Build** in GitHub Actions erfolgreich, Minimum iOS 16. Kein signiertes IPA, kein Simulator-/Gerätestart.
- Private Produktionsvorschau erfolgreich aktualisiert; Sites-Quellcommit `59d7aa3835f9586d00cc17ca7c931a467067b429`, Deployment `appgdep_6ade38e3cb648191a171e09549c2f081`, Produktionscache `schlafwald-7abaeb0cf9f2b909`.

Die Browser-Tests verwenden beschleunigte Storyzeit und stummgeschaltete Wiedergabe. Das tatsächliche Suri-Video ist eine Browseraufnahme mit **14× QA-Storyzeit, ohne Ton**. Daraus werden weder natürliche Bewegungsdauer noch Audio-/Cue-Synchronität am Gerät abgeleitet. Ein früher Test erfasste nur gepollte Schritte und übersah einen bereits durch die gehaltene Eingabe abgeschlossenen Schritt; die Prüfung wurde auf tatsächlich abgeschlossene Rituale und null Assistenz korrigiert.

## CI und ursprüngliche Blockaden

Erster Actions-Lauf: https://github.com/cschro3/SCHLAFWALD-3D/actions/runs/40213078799 – C# und iOS erfolgreich; Browser-Prüfungen wurden anschließend korrigiert, Android-Setup verwendete das entfernte SDK-Paket `tools`. Das Setup ist auf v4 und ausdrücklich `platform-tools` umgestellt. Aktueller Schlusslauf wird im PR verknüpft; ein alter roter Lauf wird nicht nachträglich als grün ausgegeben.

**Unity: blockiert.** Kein aktivierter ausführbarer Editor und keine vollständigen sicheren Aktivierungs-Secrets. EditMode, PlayMode, URP-/Shader-Rendering und Unity-Plattformbuilds nicht ausgeführt. Die License-Check-Aktion bestätigt nur diese Verfügbarkeit, keinen Unity-Test. Eine optionale spätere Unity-Baseline benötigt eine berechtigte Unity-Aktivierung über die vorgesehenen Actions-Secrets; niemals Schlüssel/Passwort ins Repository. Kein unkontrollierter LTS-Upgrade ohne Baseline.

## Verbindliche offene Release-Arbeit

1. Physische iOS-/Android-Installation, Safari/WebView, echte Touch-/Unterbrechungs-/Offline-Tests und stabile 30 FPS auf den definierten Referenzgeräten messen. RAM, Akku und Wärme sind nicht gemessen.
2. Durchgehendes natives Hintergrund-Audio bei Gerätesperre implementieren/prüfen: iOS AudioSession und Android MediaSession/Foreground-Service. Die Browseroption ist kein verlässlicher Ersatz.
3. Deutsche Stimme vollständig anhören, natürliche Cue-Zeiten prüfen, finale Musik/Foley und künstlerische Abnahme, Eltern-/Kind- und VoiceOver/TalkBack-Tests.
4. Eigene Entwicklerkonten, Herausgeber/Kontakt, Datenschutz-URL und Store-Formulare. Android-Release-Signierung und Apple-Team/Zertifikat/Provisioning über sichere Mechanismen.
5. Gerätespezifische Store-Screenshots abnehmen, Eigentümerfreigabe für öffentliche Veröffentlichung/Upload. Es ist nichts in einem Store veröffentlicht und keine öffentliche Site bereitgestellt.

Ausführliche nächste Schritte: [RELEASE_CHECKLIST.md](../RELEASE_CHECKLIST.md). Die sechs weiteren Tiere sind **in WebApp implementiert**; ihre Unity-Implementierungen bleiben korrekt als offen gekennzeichnet.
