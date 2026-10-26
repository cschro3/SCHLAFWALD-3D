# Freie Tierbesuche und Emma

Stand: 26. Oktober 2026. Das Kind wählt eines von sieben 3D-Tieren in der Waldlichtung. Jedes erzählt eine eigene kleine Geschichte in der Ich-Form. Die zehn vorhandenen Fürsorgerituale bleiben erhalten. Nach dem Einschlafen bleibt der Besuch bei diesem Tier; erst eine bewusste Auswahl startet den nächsten. Schlafende Freunde lassen sich leise besuchen oder ausdrücklich erneut erzählen. Abbrechen weckt kein anderes Tier und speichert keinen unvollständigen Besuch als fertig. Der Abend darf ohne sieben abgeschlossene Besuche enden.

Neue Fortschritte liegen separat in `sw_visits_v1`; alte Geschichten und Speicherstände bleiben erhalten. Im Elternbereich ist eine zusammenhängende Erzählrunde weiterhin wählbar.

## Stimme

Vom Nutzer ausdrücklich gewählt: Emma – Warm, Clear, Expressive, ElevenLabs-ID RqAMJpZ689MpxoPI49H6. Freigegeben wurde Suri-Hörprobe **Version 2**, Generation Sdnyrtn7A56rGWPlL7q3. Die unveränderte MP3 und ihre Herkunft stehen unter WebApp/assets/voice-reference/. Sie ist im Elternbereich als Emma-Hörprobe offline abspielbar. Diese 20,428 Sekunden entsprechen nicht den einzelnen neuen Ritualtexten und werden deshalb nicht fälschlich als komplette Vertonung eingesetzt.

Die 82 neuen Textabschnitte sind noch nicht vollständig mit Emma aufgenommen. Ein kostenloser Schätzaufruf für die 6.710 Zeichen der Tiertexte meldete 13.420 Credits / 2,684 USD für zwei komplette Varianten. Das ist keine verbindliche Kalkulation für 82 Einzelclips und enthält den Schlussabschnitt nicht. Es wurde nichts generiert oder berechnet. Keine neue Zahlung und kein Abonnement ausgelöst.

Vorübergehend bietet die App ausdrücklich gekennzeichnete lokale deutsche Gerätestimmen oder gemeinsames Vorlesen an; die vom Nutzer abgelehnten Piper-Aufnahmen werden für neue Besuche nicht automatisch eingesetzt. Keine Online-Sprachdienste zur Laufzeit. Gerätestimmen sind kein Nachweis finaler Sprachqualität.

`python tools/visit_recordings.py export <directory>` erzeugt das vollständige Sprecherbuch mit Phasen, Betonung und Animations-Cues. `import <directory> --reviewed` prüft alle 82 Dateien per ffprobe und SHA-256, bevor ein atomar geschriebenes Manifest die finale Vertonung aktiviert. Individuelle Aufnahmen benötigen Hörprüfung, Aussprachekontrolle und bestätigte Nutzungsrechte. Keine Schlüssel oder signierten Download-URLs im Repository.

## Grenzen

Die Weblaufzeit ist eine Vorabversion. Unity-Lizenzierung, physische Mobilgeräte, bestätigte 30 FPS, natives Audio bei Gerätesperre, finales Sprach-/Art-Finishing und Release-Signierung bleiben offen. Frühere Android-/Xcode-CI-Nachweise belegen den früheren Stand; sie ersetzen keine neue Buildprüfung dieser Änderungen.
