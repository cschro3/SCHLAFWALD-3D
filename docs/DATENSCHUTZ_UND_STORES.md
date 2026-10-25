# Datenschutz und Stores – Vorabversion 25.10.2026

Aktuelle Arbeitsfassungen: [PRIVACY_DE.md](PRIVACY_DE.md), [STORE_DE.md](STORE_DE.md), [RELEASE_CHECKLIST.md](RELEASE_CHECKLIST.md). Diese Dokumente ersetzen keine Freigabe des rechtlichen Herausgebers und keine Prüfung des endgültigen Store-Pakets.

## Technisch umgesetzt

Offline-Native-Pakete ohne Kinderkonto, Werbung, Tracking, Analyse- oder Social-SDKs. Eltern-Einstellungen und Fortschritt bleiben lokal. Keine Kamera-, Mikrofon- oder Standortberechtigung. Die tatsächlich gebaute Android-APK enthält **keine INTERNET-Berechtigung**; die Audio-Dateien sind im Paket enthalten. Capacitor und Three.js sind dokumentierte Softwareabhängigkeiten, keine Werbe-/Tracking-Dienste.

Die private Webvorschau wird beim Öffnen vom Hosting geladen. Dabei gelten dessen technische Zugriffs-/Protokolldaten; die Native-App-Aussage „kein Netzwerk erforderlich“ darf nicht pauschal als Aussage über die Hosting-Infrastruktur übernommen werden. Der Offline-Webcache greift nach erfolgreicher Erstinstallation.

Elternbereich: drei Sekunden halten plus Rechenaufgabe; keine Käufe und keine externen Links im Kinderbereich. Speicherung im Web lokal im Browser; iOS-Privacy-Manifest für UserDefaults ist vorbereitet und in das Xcode-Projekt eingebunden.

## Noch erforderlich

Herausgeber, Anschrift und Kontakt eintragen, öffentlich erreichbare Datenschutzerklärung bereitstellen, fertige Pakete per Netzwerkmitschnitt prüfen, Apple-App-Privacy-/Google-Data-Safety-Formulare und Kids-/Families-Angaben wahrheitsgemäß ausfüllen. Altersrating und Zielgruppenangaben werden in den Store-Portalen festgelegt, nicht durch dieses Repository erteilt.

Android: min API 29, target/compile API 36, Debug-Testpaket vorhanden; Release-Keystore/Play App Signing offen. iOS: min 16, Querformat, unsignierter Xcode-26.6-Build in Actions bestanden; Team, Zertifikat, Provisioning und TestFlight offen.

Hintergrund-Audio bei gesperrtem Bildschirm ist gewünscht und weiterhin eine Release-Aufgabe: iOS AudioSession und kontinuierliche native Wiedergabe sowie Android MediaSession/Foreground-Service implementieren und tatsächlich testen. Kein Store-Versprechen dieser Funktion vor der Abnahme.

Es gibt keinen öffentlichen Site-Deploy, Store-Upload oder Store-Release. Veröffentlichung benötigt die ausdrückliche Freigabe des Eigentümers. Entwicklerkonten und kostenpflichtige Dienste wurden nicht gekauft.
