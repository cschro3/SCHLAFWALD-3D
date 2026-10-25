# Qualitätssicherung

Status-Legende: ✅ ausgeführt und bestanden · 🟡 geschrieben/kompiliert, aber **nicht ausgeführt** · ⬜ noch nicht erstellt · ❌ fehlgeschlagen

## Automatisiert

| Prüfung | Wie | Status |
|---|---|---|
| Story-Daten vollständig & konsistent (7 Tiere, Phasen, Hinweise, Hilfen, Kapitel, Shots) | `dotnet test tools/CoreTests` (`StoryTests`) | ✅ |
| Suri-Wortlaut entspricht der Mission | `StoryTests.SuriTextMatchesMissionWording` | ✅ |
| Finale nennt alle Tiere in Reihenfolge, endet mit „Schlaf schön.“ | `StoryTests` | ✅ |
| Kleinkind-Touch-Zonen ≥ 0.6 m, Hilfe ≤ 40 s | `StoryTests.EveryStepHasChildFriendlyTouchZone…` | ✅ |
| Hilfe-Eskalation: nie Abbruch, Hinweise dann genau eine Selbsthilfe | `HelpEscalatorTests` | ✅ |
| Wald wird monoton ruhiger/dunkler, ohne Überschwingen | `AtmosphereTests` | ✅ |
| Pacing (Wunschdauer), Tierauswahl verkürzt Geschichte | `PacingTests` | ✅ |
| Fortsetzen, Bildschirm-Ausblenden, Elterngate (genau eine richtige Antwort, 3 s halten), Einstellungs-Grenzen | `ParentTests` | ✅ |
| Zeilenfilter (Singular/Plural-Intro, Finale nur mit teilnehmenden Tieren) | `LineFilterTests` | ✅ |
| Core + Runtime kompilieren gegen Unity-2021.3-API | `dotnet build tools/RuntimeCompileCheck` | ✅ (Compile-Check, **kein** Funktionstest) |
| PlayMode-Tests kompilieren | `dotnet build tools/PlayModeCompileCheck` | ✅ |
| Kameraeinstellungen zeigen alle Ritual-Objekte (16:9, 19.5:9, 4:3, 3:2) | `python3 tools/check_camera_framing.py …` | ✅ (rechnerisch) |
| Komplettdurchlauf ohne jede Eingabe (Selbsthilfe) | PlayMode `StoryCompletesWithoutAnyInput_ChildNeverFails` | 🟡 |
| Berührungen nur außerhalb der Ziele | PlayMode `StoryCompletesWhenChildOnlyTouchesOutsideTargets` | 🟡 |
| Bot löst alle Rituale ohne Hilfe | PlayMode `StoryCompletesWithBotSolvingEveryRitual_WithoutHelp` | 🟡 |
| Pause/Fortsetzen | PlayMode `PauseFreezesTheStoryAndResumeContinues` | 🟡 |
| Audiomodus ohne Bildschirm | PlayMode `AudioOnlyModeFinishesAndLeavesNoCameraRendering` | 🟡 |
| EditMode-Tests im Unity Test Runner (dieselben Dateien wie oben) | Unity → Test Runner | 🟡 |

## Manuell (Checkliste für das erste Unity-/Geräte-Review)

| # | Prüfung | Soll | Status |
|---|---|---|---|
| 1 | Projekt öffnet ohne Compilerfehler; Shader kompilieren (kein Pink) | | 🟡 |
| 2 | Menü *Schlafwald → Projekt einrichten* läuft fehlerfrei durch | | 🟡 |
| 3 | Suri-Ablauf spielen: Pfad folgen → Blätterdach → Häuschen → Einschlafen | Fühler zuerst, dann ins Haus, Haus wackelt, Ruhe | 🟡 |
| 4 | Erzählzeilen ↔ Animationen synchron (Cues) | ±0.3 s | 🟡 |
| 5 | Finale: Überflug, Mond, Musik blendet aus, Bild bleibt ruhig | | 🟡 |
| 6 | Elternbereich: Gate, alle Regler, Pause, „Neue Nacht“ | | 🟡 |
| 7 | App in Hintergrund/Vordergrund, Anruf/Unterbrechung, Bildschirm sperren | Pause + sauberes Fortsetzen | 🟡 |
| 8 | Flugmodus (offline) | identisches Verhalten | 🟡 |
| 9 | Displays: iPhone SE/15 Pro Max, iPad, Android 16:9 bis 21:9, Tablets | Ritual-Objekte sichtbar, Touch-Zonen bequem | 🟡 |
| 10 | Kleinkind-Test (2–3 Jahre, mit Elternteil): wird die Aufforderung ohne Text verstanden? | Halo/Erzähler genügt | ⬜ |
| 11 | Lautheit: keine Spitzen, Erzähler klar über Musik | | ⬜ |

## Performance-Messung (offen)

Ziele: 30 fps stabil (Frametime-Spitzen < 50 ms), ≤ 150 Draw-Calls, ≤ 250 MB RAM, geringe Erwärmung, ≤ 5 % Akku/10 min.
Vorgehen: Development Build → Unity Profiler (Frame Debugger, Memory Profiler) am Gerät, danach Xcode Instruments (Energy) bzw. Android Studio Energy-Profiler/`dumpsys batterystats`.
Bekannte Risiken, die zuerst zu prüfen sind: ~300 Gras-Büschel + ~200 Bäume als einzelne Objekte (SRP-Batcher), Partikel-Overdraw (Additiv), IMGUI-Allokationen (`OnGUI`), prozedurale Audio-Generierung beim Start (< 1 s erwartet),
`MaterialPropertyBlock` an Halos/Nebel (bricht SRP-Batching für diese wenigen Objekte).

## Bekannte Einschränkungen

- Nur Suri hat eine 3D-Figur; die anderen sechs sind als Daten vorhanden (`implemented: false`) und im Elternbereich als „folgt in Phase 3“ markiert.
- Keine Sprecherstimme/Musik – nur gekennzeichneter Fallback und generierte Platzhalterklänge.
- Der Elternbereich nutzt IMGUI (Prototyp); vor Release durch UI Toolkit/uGUI ersetzen (Barrierefreiheit, Dynamic Type, saubere Skalierung).
- Barrierefreiheit (VoiceOver/TalkBack) für den Elternbereich fehlt noch.
- Nur Deutsch; die Struktur (`story_<sprache>.json`) ist für weitere Sprachen vorbereitet.
- Unity-`.meta`-Dateien fehlen noch (werden beim ersten Öffnen erzeugt und sollten eingecheckt werden).
