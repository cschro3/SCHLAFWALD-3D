# Verifizierte Tierbesuche mit Emma

40 Node-Tests erfolgreich. Zwei gezielte Browsertests erfolgreich, einschließlich echter Offline-MP3-Wiedergabe bis zum Ende. Sieben echte Renderaufnahmen erstellt; keine JS-Fehler. 82/82 Audiohashes und Texthashes geprüft, alle Dateien dekodiert. Full-CI-Folgeprüfung noch gesondert bestätigen.

Erster CI-Lauf https://github.com/cschro3/SCHLAFWALD-3D/actions/runs/40296556054: Android, unsigniertes iOS und C# erfolgreich; Browser 16/17. Fehlgeschlagene Beschriftungserwartung korrigiert und als einer der zwei gezielten Fälle erneut erfolgreich ausgeführt. Lokaler Software-Renderer-Komplettlauf zuvor nach Timeout abgebrochen; kein lokaler Gesamt-Pass behauptet.

Erfasste Waldlichtung: 165–171 Draw Calls, bis 291.360 Dreiecke. Keine Aussage über 30 FPS auf realen Geräten. Unity-Laufzeittests weiterhin nicht ausgeführt. Finale Hörabnahme aller Emma-Dateien offen.

Neue Produktions-Vorschau mit allen82Emma-Clips: Site-Quellstand e37bd1f5c2335079aa7dd8a5abbb0c233cecbdbf, private Bereitstellung appgdep_6adf20f7315481918c4d2c16496d953a erfolgreich. Folge-CI40301277531 auf bf6f1007a4defbea5360d3cefa63e00beffeb978: C#, Android und unsigniertes Xcode bereits erfolgreich, Browser zum Zeitpunkt dieser Notiz noch laufend. Alle82Audios gemessen: -25,41bis-18,06LUFS, höchster TruePeak-1,62dBTP; der leiseste Clip ist Suris kurzer Gute-Nacht-Satz. Messung ersetzt keine Hörabnahme.

Abschließender Zwischenstand: Im Folge-CI40301277531 ist der vollständige Playwright-Befehl mit allen18Browserfällen erfolgreich abgeschlossen. Android, unsigniertes Xcode und C# sind ebenfalls erfolgreich. Die nachfolgenden automatisierten Aufnahme-/Webbuild-Schritte laufen zum Beobachtungszeitpunkt noch; kein vorgetäuschter Gesamt-CI-Abschluss. Job-/Schrittstatus in ci-status.json.

Android-APK aus Artefakt12640314322 zusätzlich entpackt und geprüft: alle82Emma-Dateihashes stimmen, keine QA-API, minSdk29/targetSdk36, keine INTERNET-Berechtigung, APK-v2-Signaturblock vorhanden. Kryptographische Signatur und Installation auf realem Gerät hier nicht separat geprüft; Details android-apk.json.
