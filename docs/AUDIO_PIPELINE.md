# Audio-Pipeline

## Erzähleraufnahmen

1. Sprecher-Skript erzeugen: `python3 tools/export_recording_script.py` → `docs/recording/sprecher_skript.md` und `.csv` (101 Zeilen, mit Ton, Pause und Bedingungen).
2. Tempo ca. **105–110 Wörter/Minute**, warme Stimme, „…“ = bewusste Pause (0.9 s), `soft`/`whisper` leiser und zärtlicher; bei `whisper` nicht flüstern bis unverständlich.
3. Dateiname = Zeilen-ID (z. B. `suri_o2.ogg`), Mono, 44.1/48 kHz, normalisiert auf ca. −18 LUFS, **keine Lautstärkespitzen**, 150 ms Fade-in/-out.
4. Ablage: `UnityProject/Assets/Schlafwald/Resources/Audio/Narration/<stimme>/<ID>.ogg` (`warm` = Hauptstimme, `soft` = zweite Stimme).
   Import: *Load Type: Compressed In Memory* (kurze Zeilen) bzw. *Streaming* (> 10 s), *Vorbis* Qualität 0.6.
5. Fertig – kein Code nötig. Im Elternbereich steht „Sprachaufnahmen vorhanden: X / 101“.

Die Dauer einer Zeile ist die Länge der Datei; `pauseAfter` aus den Story-Daten kommt danach. Animations-Cues (`cue`, `cueDelay`) sind relativ zum Zeilenstart –
bei neuen Aufnahmen `cueDelay` ggf. nachziehen (z. B. Suris letztes „Gute Nacht“ → `shellWobble` nach 1.2 s).

## Ambiente, Musik, Effekte

| Schlüssel (Resources-Pfad) | Wirkung |
|---|---|
| `Audio/Ambience/wind`, `Audio/Ambience/crickets` | Loop, ersetzt Wind/Grillen |
| `Audio/Music/pad_a`, `Audio/Music/pad_b` | Loops (16 s im Platzhalter), werden langsam gegeneinander geblendet |
| `Audio/Music/note_196` … `note_523` | Einzelnoten der Melodie (G3, A3, C4, D4, E4, G4, A4, C5) |
| `Audio/Sfx/<key>` | `squeak`, `yawn`, `snore`, `tap`, `rustle`, `chirp`, `twinkle` (+ neue Schlüssel aus der Story) |

Lautstärken: Master × Kategorie (Erzähler / Musik / Waldgeräusche getrennt regelbar). Die Atmosphäre (`SleepAtmosphere`) senkt Aktivität und Musik mit jedem schlafenden Tier; nach der Geschichte blendet die Musik über 45 s aus.

## Entwicklungs-Fallback (ausdrücklich gekennzeichnet)

`FallbackVoice` erzeugt wortloses Murmeln im Sprechrhythmus. **Das ist keine Sprachsynthese und kein Ersatz für die finale Stimme.**
Solange es läuft: Untertitel sind sichtbar (Eltern können vorlesen), im Editor/Debug-Build zeigt ein gelbes Banner „ENTWICKLUNGS-FALLBACK“, die Konsole warnt einmalig.
**Release-Gate:** Ein Release-Build ohne vollständige Aufnahmen darf nicht veröffentlicht werden (Elternbereich zeigt die Abdeckung).
