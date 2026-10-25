# SCHLAFWALD – Wenn die Tiere schlafen

Interaktive 3D-Gute-Nacht-Geschichte für Kinder von 2 bis 6 Jahren (iOS & Android, Unity 2022.3 LTS + URP, C#).
Sieben Waldtiere, ein erzählender Abend, ein Wald, der mit jedem schlafenden Tier stiller wird – ohne Punkte, Zeitlimits, Werbung oder Datenerhebung.

> **Stand: Phase 1 + Phase 2 (vertikaler Prototyp „Suri, die Schnecke“).** Phasen 3–6 stehen aus, siehe [docs/ROADMAP.md](docs/ROADMAP.md).
> **Wichtig:** Das Unity-Projekt wurde in einer Umgebung **ohne Unity-Editor** erstellt. Was tatsächlich geprüft wurde und was nicht, steht ehrlich weiter unten.

## Was schon da ist

| Bereich | Status |
|---|---|
| Story, Kapitel, Erzählertexte aller 7 Tiere + Finale als bearbeitbare Daten | ✅ [`story_de.json`](UnityProject/Assets/Schlafwald/Resources/Story/story_de.json) (101 Zeilen, validiert) |
| Prozeduraler 3D-Märchenwald (Gelände, Bäume, Steine, Pilze, Farne, Blumen, Baumhäuser, Höhlen, Nebelbänke, Glühwürmchen) | ✅ Code geschrieben |
| Himmel (Sonnenuntergang → Mondnacht, Sterne, Mond), weiches Licht ohne harte Schatten, 3 eigene URP-Shader | ✅ Code geschrieben |
| **Suri** – echtes 3D-Modell (Spiral-Haus, segmentierter Fuß, Fühler, Augen) mit vollständiger Einschlafanimation | ✅ Code geschrieben, Geometrie per Vorschau geprüft |
| Interaktionen: Pfad folgen, Drag-and-drop, Berühren, Streicheln – mit großzügigen Touch-Zonen, Hinweisen und sanfter Selbsthilfe | ✅ Code geschrieben (4 wiederverwendbare Schrittarten) |
| Erzähler mit Cues (Sprache ↔ Animation synchron), Audio-Dateien ersetzbar, **gekennzeichneter Entwicklungs-Fallback** | ✅ |
| Adaptives Audio (Wind, Grillen, Klangflächen, leise Klaviermelodie), je ruhiger der Wald, desto weniger Aktivität | ✅ generierte Platzhalter-Klänge |
| `SleepForestController`: Schlafstatus → Licht, Bewegung, Partikel, Geräusche, Musik | ✅ |
| Elternbereich (3 s halten + Rechenaufgabe): Lautstärken, Erzähler, Pause, Wunschdauer, Bildschirm ausblenden, Audiomodus, Tierauswahl | ✅ (IMGUI-Prototyp) |
| Fortsetzen derselben Nacht, App-Wechsel/Pause, komplett offline | ✅ Code geschrieben |
| Die 6 weiteren Tiere (3D-Figuren, Habitate, Rituale) | ⏳ Phase 3 – Story/Konfiguration/Kameras liegen bereits vor, `implemented: false` |
| App-Icon, Screenshots, Store-Texte, Privacy-Manifest | ⏳ Phase 6 (Entwürfe in [docs/DATENSCHUTZ_UND_STORES.md](docs/DATENSCHUTZ_UND_STORES.md)) |

## Ehrlicher Verifikationsstatus

**Wirklich ausgeführt (reproduzierbar):**

```bash
dotnet test  tools/CoreTests                       # 25 NUnit-Tests der Logik-Schicht: bestanden
dotnet build tools/RuntimeCompileCheck             # Core + Runtime gegen die Unity-2021.3-Referenz-Assemblies: 0 Fehler
dotnet build tools/PlayModeCompileCheck            # PlayMode-Tests kompilieren: 0 Fehler
python3 tools/check_camera_framing.py UnityProject/Assets/Schlafwald/Resources/Story/story_de.json   # Suri-Ritual in 16:9, 19.5:9, 4:3, 3:2 im Bild
```

Die Logik-Tests prüfen u. a.: Story-Vollständigkeit (jedes Tier hat Einführung, Hinweis, Hilfe, Ausklang), Mission-Wortlaut für Suri, Hilfe-Eskalation
(Hinweise → genau eine Selbsthilfe, nie Abbruch), monoton ruhiger werdende Atmosphäre, Pacing für die Wunschdauer, Elterngate, Fortsetzen/Dimmen.
Dabei wurde ein echter Fehler im Eltern-Rechengate gefunden und behoben.

**NICHT geprüft – weil hier kein Unity-Editor und kein Gerät verfügbar war:**

- Das Projekt wurde **noch nie in Unity geöffnet**. Shader (HLSL), Editor-Skripte (`ProjectSetup`, `BuildScript`), Partikelsysteme, Audio-Wiedergabe und der gesamte visuelle Eindruck sind **ungetestet**.
- Es gibt **keine** Builds, **keine** Messung von Bildrate, Speicher oder Akku, **keine** Tests auf iPhone/iPad/Android.
- Die PlayMode-Tests (`Tests/PlayMode`) sind geschrieben und kompilieren, wurden aber **nicht ausgeführt**.
- Das Aussehen der Schnecke kenne ich nur aus einer Python-Vorschau der Mesh-Geometrie, nicht aus Unity.
- Es gibt **keine** gesprochene Erzählerstimme (siehe Fallback) und keine finale Musik.

Erste Schritte für Sie, um das zu schließen, stehen in [docs/QA.md](docs/QA.md).

## Schnellstart

1. Unity Hub → **Unity 2022.3.50f1 (LTS)** mit Android-/iOS-Modulen, Ordner `UnityProject/` öffnen (URP wird aus `Packages/manifest.json` geladen).
2. Menü **Schlafwald → 1. Projekt einrichten** (URP-Asset, Linear, Querformat, Main-Szene, Build-Settings).
3. `Assets/Schlafwald/Scenes/Main.unity` öffnen → **Play**. Die App baut Wald, Tiere und Systeme zur Laufzeit selbst auf (`AppComposer`), die Szene bleibt leer.
4. Beim ersten Öffnen erzeugt Unity die `.meta`-Dateien – bitte mit einchecken.
5. Ohne Sprachaufnahmen läuft der **Entwicklungs-Fallback** (Murmeln + Untertitel + gelber Hinweis im Editor). Aufnahmen: [docs/AUDIO_PIPELINE.md](docs/AUDIO_PIPELINE.md).
6. Elternbereich: oben rechts den fast unsichtbaren „•••“-Bereich 3 Sekunden halten, Rechenaufgabe lösen.

## Architektur (Mission § 9)

```
Assets/Schlafwald/Scripts
├── Core/      Unity-freie Logik, per dotnet testbar: Datenmodell, StoryValidator, HelpEscalator, SleepAtmosphere,
│              PacingPlanner, NarrationTiming, LineFilter, ParentSettings/Gate, ScreenDim, AspectFov
├── Runtime/
│   ├── Systems/      StoryDirector · SleepForestController · NarrationManager · AudioEnvironmentManager · CameraDirector
│   │                 SceneTransitionManager · SaveProgressManager · ParentSettingsManager · AppComposer
│   ├── World/        ForestBuilder · ForestTerrain · HabitatBuilder · LightingController (+SkyDome, Partikel)
│   ├── Animals/      AnimalController · AnimalRig · AnimalPerformance · SnailBuilder · SnailPerformance
│   ├── Interaction/  AnimalInteractionController · FollowPath/DragToTarget/Touch/Stroke-Steps · PropFactory
│   ├── Rendering/    MeshBuilder · MaterialLibrary
│   ├── Infra/        PointerInput · PauseController · Channel · kleine Komponenten
│   └── UI/           OverlayGui (Überblendungen, Untertitel, Elternbereich)
├── Editor/    ProjectSetup · BuildScript
Resources/Story/story_de.json     alle Texte, Kapitel, Tiere, Schritte, Kameras (bearbeitbar)
Resources/Shaders/                StylizedLit · SoftUnlit · SkyDome (URP)
Tests/EditMode · Tests/PlayMode
```

Jedes Tier ist datengetrieben (`AnimalConfig`): Name, Charaktermodell, Schlafplatz, Interaktionsschritte, Animationszustände, Erzählertexte, Audio, Schlafstatus-/Erfolgsauslöser.
Neue Tiere brauchen: JSON-Eintrag + `Builder` + `Performance`-Klasse – keine Sonderlogik in Story oder Interaktion.

Weiter: [Konzeption](docs/01-Konzeption.md) · [Assets, die ersetzt werden müssen](docs/ASSETS_ZU_ERSETZEN.md) · [Audio-Pipeline](docs/AUDIO_PIPELINE.md) · [QA](docs/QA.md) · [Datenschutz & Stores](docs/DATENSCHUTZ_UND_STORES.md) · [Roadmap](docs/ROADMAP.md)
