# Audio-Pipeline

## Vorhandene deutsche Aufnahmen

Alle 101 Story-Zeilen liegen als echte, lokal erzeugte MP3-Erzählerdateien vor: `WebApp/public/audio/narration/<ID>.mp3` und für das bewahrte Unity-Projekt `Resources/Audio/Narration/warm/<ID>.mp3`. Es handelt sich um Piper/Thorsten-Sprachsynthese, nicht um den bisherigen Murmel-Fallback. Die endgültige Sprecherabnahme bleibt offen.

`WebApp/public/audio/manifest.json` enthält Zeilen-ID, Text-/Dateihash, Dauer und Modellnachweis. Die Gesamtdauer aller Dateien einschließlich optionaler Varianten/Hilfen beträgt rund 432 Sekunden; die tatsächliche Geschichte enthält zusätzlich Pausen und Rituale.

Reproduzierbare Erzeugung: `python tools/generate_narration.py --help`. Das Werkzeug nutzt das lizenzierte Modell `de_DE-thorsten-high`, Piper und FFmpeg ausschließlich im Buildprozess. Zieltempo ungefähr 108 Wörter/Minute, Normalisierung −20 LUFS, True-Peak-Ziel −3 dBTP, sanfte Fades. Ausgabe wird zuerst temporär geschrieben, dann atomar übernommen. Modelle, Zugangsdaten und ausführbare TTS-Engines werden nicht in die App eingebaut.

Lizenznachweise: [ASSET_LICENSES.md](ASSET_LICENSES.md). Integrität prüfen: `python tools/verify_assets.py` und `npm --prefix WebApp test`.

## Story, Pausen und Cues

`story_de.json` bleibt die Textquelle. Sprecher-Skript: `python tools/export_recording_script.py`. Im Normalbetrieb entspricht jede Zeilendauer der MP3-Dauer; danach folgt `pauseAfter`. Cues sind relativ zum Zeilenstart. Ein Schritt beendet die laufende Erzählung vor dem nächsten Prompt. Hilfe wartet während Erzählungen; eine fehlende Datei aktiviert einen deutlich gekennzeichneten Vorlesemodus und blockiert die Geschichte nicht.

Vor Freigabe alle 101 Dateien zusammen mit Cues anhören. Natürlichkeit, Betonung, Atempausen und die Suri-Rückzugsfolge sind keine Eigenschaft, die ein Hash- oder Kompilationstest bestätigen kann.

## Musik, Wald und Elternregler

`WebApp/src/audio.js` erzeugt eigene ruhige Klangflächen und Wind ohne fremde Samples. Master, Erzähler, Musik und Waldgeräusche sind getrennt regelbar. Der Wald wird mit dem Fortschritt leiser; nach dem Finale erfolgt ein sanftes Ausblenden. Finale Musik-/Foley-Abnahme steht aus.

Der Audiomodus verdunkelt den Bildschirm und begleitet Rituale per Selbsthilfe. Die verdeckte 3D-Szene wird nicht weiter gezeichnet. Der Abschalttimer stoppt die Wiedergabe, ohne Kinder unter Zeitdruck zu setzen.

## Gesperrter Bildschirm: verbindlich offen

Die Elternoption erlaubt Hintergrundwiedergabe, kann aber Browser-/Betriebssystem-Suspendierung nicht verhindern. iOS hat die Audio-Capability vorbereitet. Kontinuierliche native AudioSession/Wiedergabe und Android MediaSession/Foreground-Service fehlen noch. Durchgehendes Audio bei gesperrtem Gerät darf vor Implementierung und Gerätetest nicht als fertige Funktion beworben werden.

## Bewahrte Unity-Pipeline

Unity kann die MP3s über seine vorhandene Resources-Pipeline laden. Externe Austauschaufnahmen können wie bisher in `Audio/Narration/<stimme>/<ID>` liegen. Unitys `FallbackVoice` erzeugt nur Murmeln und bleibt ein Entwicklungs-Fallback. Der Unity-Release-Gate verlangt vollständige Aufnahmen und implementierte Tiere. Kein ausgeführter Unity-Audio-Test liegt vor.


Aktualisierung 26.10.2026: Freie Tierbesuche und ausgewählte Emma-Hörprobe siehe [VISITS_AND_VOICE.md](VISITS_AND_VOICE.md). Die vollständige neue Emma-Vertonung ist noch offen.
