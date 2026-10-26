# Freie Tierbesuche und Emma

Stand: 26. Oktober 2026. Das Kind wählt eines von sieben 3D-Tieren in der Waldlichtung. Jedes erzählt eine eigene kleine Geschichte in der Ich-Form. Die zehn vorhandenen Fürsorgerituale bleiben erhalten. Nach dem Einschlafen bleibt der Besuch bei diesem Tier; erst eine bewusste Auswahl startet den nächsten. Schlafende Freunde lassen sich leise besuchen oder ausdrücklich erneut erzählen. Abbrechen weckt kein anderes Tier und speichert keinen unvollständigen Besuch als fertig. Der Abend darf ohne sieben abgeschlossene Besuche enden.

Neue Fortschritte liegen separat in `sw_visits_v1`; alte Geschichten und Speicherstände bleiben erhalten. Im Elternbereich ist eine zusammenhängende Erzählrunde weiterhin wählbar.

## Stimme

Vom Nutzer ausdrücklich gewählt: Emma – Warm, Clear, Expressive, ElevenLabs-ID RqAMJpZ689MpxoPI49H6. Freigegeben wurde Suri-Hörprobe **Version 2**, Generation Sdnyrtn7A56rGWPlL7q3. Die unveränderte MP3 und ihre Herkunft stehen unter WebApp/assets/voice-reference/. Sie ist im Elternbereich als Emma-Hörprobe offline abspielbar. Diese 20,428 Sekunden entsprechen nicht den einzelnen neuen Ritualtexten und werden deshalb nicht fälschlich als komplette Vertonung eingesetzt.

Die 82 neuen Textabschnitte sind inzwischen vollständig mit Emma als **Vorschaufassung** aufgenommen und integriert. Es wurden 164 Varianten mit insgesamt 13.376 vorhandenen Credits erzeugt; für die Vorschau ist pro Abschnitt die zweite Variante übernommen. Kein Abonnement und kein Credit-Zukauf. Nicht alle generierten Clips wurden einzeln angehört: Das ist keine finale redaktionelle Hörabnahme. Ein kostenloser Schätzaufruf für die 6.710 Zeichen der Tiertexte meldete 13.420 Credits / 2,684 USD für zwei komplette Varianten. Das ist keine verbindliche Kalkulation für 82 Einzelclips und enthält den Schlussabschnitt nicht. Es wurde nichts generiert oder berechnet. Keine neue Zahlung und kein Abonnement ausgelöst.

Standard sind die lokal mitgelieferten Emma-Clips; optional bietet die App ausdrücklich gekennzeichnete lokale deutsche Gerätestimmen oder gemeinsames Vorlesen an; die vom Nutzer abgelehnten Piper-Aufnahmen werden für neue Besuche nicht automatisch eingesetzt. Keine Online-Sprachdienste zur Laufzeit. Gerätestimmen sind kein Nachweis finaler Sprachqualität.

`python tools/visit_recordings.py export <directory>` erzeugt das vollständige Sprecherbuch mit Phasen, Betonung und Animations-Cues. `import <directory> --reviewed` prüft alle 82 Dateien per ffprobe und SHA-256, bevor ein atomar geschriebenes Manifest die finale, ausdrücklich abgenommene Vertonung aktiviert. Individuelle Aufnahmen benötigen Hörprüfung, Aussprachekontrolle und bestätigte Nutzungsrechte. Keine Schlüssel oder signierten Download-URLs im Repository.

## Grenzen

Die Weblaufzeit ist eine Vorabversion. Unity-Lizenzierung, physische Mobilgeräte, bestätigte 30 FPS, natives Audio bei Gerätesperre, finales Sprach-/Art-Finishing und Release-Signierung bleiben offen. Frühere Android-/Xcode-CI-Nachweise belegen den früheren Stand; sie ersetzen keine neue Buildprüfung dieser Änderungen.

## Prüfung dieses Stands

38/38 Node-Logik- und Audiosteuerungstests lokal erfolgreich. Produktions-Webbuild erfolgreich; QA-Steuerung im Produktionsbundle nicht vorhanden. Erste echte Aufnahmen der freien Waldlichtung auf Tablet, Handy quer und hoch geprüft; Schildüberdeckung erkannt und zur Nachprüfung korrigiert. Der lokale vollständige Browserlauf wurde nach einem erfolgreichen Suri-Pointer-Fall und einem Timeout beim Durchlauf aller sieben Tiere unter langsamem Software-Rendering abgebrochen. Kein lokaler Komplett-Pass. Neuer CI-Lauf: https://github.com/cschro3/SCHLAFWALD-3D/actions/runs/40296556054 (Ergebnis gesondert prüfen). Cloud-Browser ohne WebGL zeigt korrekt die Grafik-Fehleransicht.

## Integration der Emma-Vorschau

82/82 MP3s wurden vollständig dekodiert, auf -20 LUFS / -2 dBTP normalisiert und mit Dateihash, Texthash, Dauer und Generation-ID dokumentiert. Gesamtaufnahmezeit 421,378 Sekunden, zuzüglich der Erzählpausen und Interaktionen. Das Manifest kennzeichnet sie ausdrücklich als `generated-preview`. Kein vorgetäuschtes `approved`. Einmalig wird nach diesem Update Emma als Standard gesetzt; eine anschließend bewusst gewählte lokale Gerätestimme bleibt wählbar. Die App wartet auf das tatsächliche Audio-Endereignis und löst wartende Abläufe beim Abbrechen korrekt auf.

Tatsächlich ausgeführt: 40/40 lokale Node-Tests; zwei neue/gezielt erneut ausgeführte Browsertests erfolgreich (Gerätestimmenauswahl mit Sprachmock sowie **echte lokale Emma-MP3 nach Offline-Neustart bis zum tatsächlichen Ende abgespielt**, kein Sprachmock im zweiten Fall). Alle sieben neuen Renderaufnahmen erfolgreich und ohne JavaScript-Fehler. Der Review-Fix für die Namensschilder ist in Tablet, Handy quer und hoch als behoben bewertet. Der erste neue CI-Lauf bestand Android, unsigniertes Xcode, C# und 16/17 Browserfälle; einzig eine veraltete erwartete Button-Beschriftung schlug fehl. Diese Erwartung ist korrigiert und gezielt erfolgreich erneut ausgeführt. Ein neuer kompletter CI-Lauf für den Stand mit Emma wird separat nachgewiesen.
