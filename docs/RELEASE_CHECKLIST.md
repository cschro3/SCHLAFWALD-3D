# Release-Vorbereitung – Stand 25.10.2026

Dies ist eine prüfbare Vorabversion, kein freigegebenes Store-Release. Eine öffentliche Veröffentlichung oder ein Store-Upload ist nicht erfolgt.

## Vorbereitete Pakete

- Three.js-App mit gebündelten, lokalen Sprachdateien; Produktionsbuild ohne QA-Schnittstelle.
- Capacitor Android 8.5.3: `app.schlafwald.nacht`, Minimum Android 10 (API 29), compile/target API 36, Querformat, Backup deaktiviert und keine INTERNET-Berechtigung im App-Manifest.
- Capacitor iOS 8.5.3: Minimum iOS 16, Querformat auf iPhone und iPad, keine Verschlüsselungsexport-Ausnahme erforderlich, PrivacyInfo.xcprivacy als Ressource, AppIcon.
- Android-Debug-APK ist ein Testpaket. Release-Signierung wird erst mit dem eigenen Keystore eingerichtet. Kein Schlüssel oder Kennwort wird eingecheckt.
- Xcode-Projekt und Swift-Package-Abhängigkeiten werden generiert. Der macOS-CI-Job hat einen unsignierten Geräte-Build mit Xcode 26.6 erfolgreich ausgeführt (keine Installation und kein Gerätestart).

## Verbindliche Abnahme vor Veröffentlichung

1. Physische Geräte: iPhone SE 2020/A13 mit iOS 16+, ein aktuelles iPhone, iPad 4:3; Samsung Galaxy A52/6 GB mit Android 10+ und aktuellem System-WebView. Die alten OS-Versionen sind Mindestsupport, keine SDK-Vorgabe für den Build.
2. Mindestens stabile 30 FPS im Normalbetrieb; P95 der Framezeit ≤ 40 ms, keine anhaltenden Phasen > 50 ms; Draw Calls ≤ 150, sichtbare Dreiecke ≤ 250.000, App-RAM-Ziel ≤ 250 MB. Akku/10 Minuten und Wärme real messen. SwiftShader ist funktionale Renderprüfung, kein Geräte-Performancebeweis.
3. Alle zehn Rituale per Touch, Pause, App-Wechsel, Tonunterbrechung, Fortsetzen, Flugmodus, Safe Areas, VoiceOver/TalkBack, Eltern-Gate mit 2–6-Jährigen und Eltern prüfen.
4. Deutsche Stimme vollständig anhören und natürliches Tempo/Betonungen freigeben. Alle 101 Dateien sind synthetisch erzeugt; Lautheitsprüfung ersetzt keine Sprecherabnahme. Cue-Synchronität mit dem tatsächlichen Erzähler am Gerät prüfen.
5. Hintergrund-Audio bei gesperrtem Bildschirm ist **noch nicht verlässlich freigegeben**. iOS hat die audio-Capability vorbereitet; die kontinuierliche native Wiedergabe/AudioSession und Android MediaSession/Foreground-Service müssen vor einer entsprechenden Store-Aussage implementiert und am Gerät abgenommen werden. Browser können JavaScript und Medien suspendieren.
6. Finale Art-Abnahme und reale Store-Screenshots auf den Zielgeräten. Die gelieferten Browser-Szenen sind echte Renders, aber keine heimlich als Geräteaufnahmen ausgegebenen Screenshots.
7. Apple Developer Program, App-Store-Connect-App und Google-Play-Console-Konto durch Eigentümer. Keine kostenpflichtige Anmeldung wurde vorgenommen. Gewünschte Bundle-ID und rechtlicher Herausgeber müssen verbindlich bestätigt werden.
8. Apple: eigene Team-ID, Zertifikat und Provisioning Profile in Xcode/secure CI einrichten; TestFlight-Upload erst nach Freigabe. Google: eigener Release-Keystore bzw. Play App Signing, AAB signieren und internen Testkanal einrichten. Geheimnisse ausschließlich in Keychain/Actions-Secrets.
9. Herausgeber/Kontakt und öffentlich erreichbare Datenschutzerklärung ergänzen, Kids-/Families-Fragen und Altersrating in den Stores wahrheitsgemäß ausfüllen. Keine Daten-erhoben-Angabe ohne Prüfung des fertigen nativen Pakets und eines Netzwerkmitschnitts bestätigen.
10. Finalen Pull Request prüfen und erst nach gesonderter Freigabe mergen/veröffentlichen.

## Aktuelle Store-Vorgaben – geprüfte Primärquellen

- Google Play verlangt ab 31.08.2026 für neue Apps/Updates target API 36; hier konfiguriert. https://support.google.com/googleplay/android-developer/answer/11926878
- Apple verlangt seit April 2026 den aktuellen Xcode-26-/iOS-26-SDK-Stand für Uploads; iOS-16-Mindestsupport ist davon getrennt. https://developer.apple.com/news/upcoming-requirements/
- Kids Category: externe Links/Käufe hinter Elterngate, keine Drittanbieterwerbung/Analytics. https://developer.apple.com/app-store/review/guidelines/#kids-category
- Datenschutzmanifest und UserDefaults-Grund CA92.1: https://developer.apple.com/documentation/technotes/tn3183-adding-required-reason-api-entries-to-your-privacy-manifest

Diese Quellen und die tatsächlichen Pakete bei einem späteren Upload erneut prüfen. Die Checkliste ist keine erteilte Store-Zulassung.
