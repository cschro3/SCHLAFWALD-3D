# Datenschutz & Store-Vorbereitung (ENTWURF – Phase 6, keine Rechtsberatung)

## Datenschutz-Konzept

- **Keine personenbezogenen Daten.** Kein Konto, keine Registrierung, kein Tracking, keine Analyse, keine Werbung, keine Social-Plugins, keine Netzwerkzugriffe, keine externen Links im Kinderbereich.
- Lokal gespeichert (PlayerPrefs, nur auf dem Gerät): Eltern-Einstellungen (Lautstärken, Stimme, Dauer, Optionen) und die Namen der Tiere, die in der aktuellen Nacht schon schlafen (+ Zeitstempel für „gleiche Nacht fortsetzen“, Ablauf nach 90 Minuten).
- Keine Berechtigungen: kein Mikrofon, keine Kamera, kein Standort, kein Speicherzugriff; Android ohne `INTERNET` (`forceInternetPermission = false` im Setup – **nach dem ersten Build im Manifest verifizieren**).
- Elternbereich geschützt (3 s halten + Rechenaufgabe); er enthält keine Links und keine Käufe.
- Keine Drittanbieter-SDKs. Unity selbst: Unity Analytics/Services nicht installiert (siehe `Packages/manifest.json`); im Build prüfen, dass keine Verbindung zu Unity-Diensten entsteht (Netzwerkmitschnitt).

## Entwurf Datenschutzhinweis (DE)

> **SCHLAFWALD erhebt keine Daten.** Die App funktioniert vollständig offline. Es gibt keine Konten, keine Werbung, keine Tracker und keine Weitergabe von Daten.
> Einstellungen des Elternbereichs und der Fortschritt der aktuellen Nacht werden nur lokal auf Ihrem Gerät gespeichert und beim Löschen der App entfernt.
> Kontakt: _<Verantwortliche/r, Anschrift, E-Mail – vor Veröffentlichung eintragen>_

## Store-Checkliste

| Thema | iOS (App Store) | Android (Google Play) | Status |
|---|---|---|---|
| Zielgruppe | Kids Category (Altersband 5 & unter) | Designed for Families, Zielgruppe 0–5/6–8 | ⬜ |
| Datenschutz-Angaben | App Privacy: „Data Not Collected“; `PrivacyInfo.xcprivacy` (keine Required-Reason-APIs außer ggf. UserDefaults/PlayerPrefs → Grund `CA92.1`) | Data-Safety-Formular: keine Daten erhoben/geteilt | ⬜ |
| Datenschutzerklärung-URL | verpflichtend (auch bei Offline-App) | verpflichtend | ⬜ |
| Elternbereich/Gate | erforderlich für Einstellungen (Parental Gate) ✅ vorhanden | | ✅ Code |
| Werbung/IAP/Links | keine | keine | ✅ |
| Altersfreigabe | 4+ | PEGI 3 / IARC | ⬜ |
| Target API / SDK | aktuelle Xcode-/Target-SDK-Anforderung beim Build prüfen | `targetSdk` gemäß aktueller Play-Vorgabe, 64-bit ✅ (ARM64) | ⬜ |
| Hintergrund-Audio (falls Audiomodus mit gesperrtem Bildschirm gewünscht) | `UIBackgroundModes: audio` ergänzen | Foreground-Service/Media-Session | ⬜ Entscheidung offen (aktuell: App pausiert im Hintergrund) |

## Store-Metadaten (Entwurf, Deutsch)

- **Name:** SCHLAFWALD – Wenn die Tiere schlafen
- **Untertitel (30 Z.):** Die Gute-Nacht-Geschichte
- **Kurzbeschreibung (80 Z.):** Sieben Waldtiere in den Schlaf begleiten – ruhig, offline, ohne Werbung.
- **Beschreibung:** Ein interaktives 3D-Märchen zum Einschlafen für Kinder von 2 bis 6 Jahren. Begleiten Sie gemeinsam Fino, Mila, Piko, Luna, Nuri, Leni und Suri in den Schlaf:
  decken Sie zu, streicheln Sie, bauen Sie Nester und Dächer. Eine warme Erzählerstimme führt durch den Abend, der Wald wird mit jedem Tier stiller – am Ende bleibt nur der Mond.
  Keine Werbung, keine Punkte, keine Zeitlimits, keine Datenerhebung. Mit Elternbereich (Lautstärke, Dauer, Audiomodus, Tierauswahl).
- **Schlüsselwörter:** Gute Nacht Geschichte, Einschlafen, Kinder, Schlaf, Märchen, Tiere, Wald, Vorlesen, Baby, Kleinkind
- **Screenshots (offen):** 1 Sonnenuntergang am Waldeingang · 2 Suri auf dem Moosstein · 3 Blätterdach legen · 4 Suri im Haus · 5 Finale mit Mond. Querformat, je Gerätegröße.
- **App-Icon (offen):** Mond über schlafendem Schneckenhaus/Waldsilhouette, weich, ohne Text.
