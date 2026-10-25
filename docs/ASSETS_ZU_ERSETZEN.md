# Platzhalter-Assets, die ersetzt werden müssen

Alle 3D-Modelle, Klänge und Texturen dieses Prototyps sind **eigene, zur Laufzeit/Editor erzeugte Platzhalter** (keine fremden Ressourcen, keine Lizenzfragen).
Sie sind funktionierende 3D-Modelle mit echter Animation – aber **keine finale Kunst**.

## 3D

| Platzhalter | Erzeugt in | Ersatz (Ziel) | Hinweise |
|---|---|---|---|
| Suri (Schnecke) | `Animals/SnailBuilder.cs` | Blender-Modell, Prefab mit Teilen gleichen Namens | s. u. „Rig-Konvention“; ~6–10k Dreiecke; 1 Material + 1 Textur 1024² |
| Weitere 6 Tiere | – (Phase 3) | je ein Modell mit gleicher Konvention | Fell als Shader-Effekt (`_FurAmount`) oder Shell-Fur, nicht als Haarkarten |
| Bäume (6 Varianten), Steine, Pilze, Blumen, Farne, Gras | `World/ForestBuilder.cs` | Modelle mit weicher, runder Silhouette | Vertexfarben-Alpha = Wind-Gewicht beibehalten |
| Baumhäuser, Höhlen | `ForestBuilder.BuildHouses` | modellierte Häuser (Fensterlicht bleibt emissiv) | |
| Habitat Suri (Steine, Zweige) | `World/HabitatBuilder.cs` | modellierte Steine mit Moos | Höhenfeld in `ForestTerrain` an die Modelle anpassen |
| Ritual-Props (Blatt, Decke, Eichel, Kissen, Glühwürmchen) | `Interaction/PropFactory.cs` | modellierte Props | Pivot = Mittelpunkt |
| Himmel/Mond/Sterne | `SkyDome.shader` | beibehalten oder durch gemalte Skybox ersetzen | |

### Rig-Konvention (Suri, gilt sinngemäß für alle Tiere)

Wird ein importiertes Modell verwendet, müssen die Teile **exakt so heißen** (siehe `SnailBuilder`), damit `SnailPerformance` unverändert funktioniert:

`Body`, `Foot0…Foot5` (vorn→hinten), `Mantle`, `Neck`, `Head`, `EyeL/EyeR` (+ Kinder `SparkL/R`), `CheekL/R`, `FeelerL/FeelerL2`, `FeelerR/FeelerR2`, `Shell`, `ShellOpening`.

Die Animation arbeitet relativ zur **Ruhepose** (`AnimalRig.CaptureRest`), skaliert/rotiert diese Transforms prozedural (Atmen, Blinzeln, Kriechwelle, Fühler einziehen, ins Haus gleiten).
Wer lieber mit Animator/Clips arbeitet: `AnimalPerformance` ist die einzige Schnittstelle zur Story (`Cue(string)`, `OnStep*`, `FinishSleep`) – eine Animator-basierte Subklasse genügt.

## Audio

| Platzhalter | Quelle | Ersatz |
|---|---|---|
| Erzählerstimme | `FallbackVoice` (Murmeln) | Studioaufnahmen, siehe [AUDIO_PIPELINE.md](AUDIO_PIPELINE.md) |
| Wind, Grillen | `ProceduralAudio.Wind/Crickets` | Loops (nahtlos, ≥ 20 s) |
| Klangflächen, Klavier-Plinks | `ProceduralAudio.Pad/Note` | komponierte Musik (Klavier + warme Pads), Loops/Stems |
| Tiergeräusche (squeak, yawn, snore, tap, rustle, chirp, twinkle) | `ProceduralAudio.Sfx` | Aufnahmen/Foley, **niedlich, leise, ohne Spitzen** |

## 2D / Store

App-Icon, Screenshots, Feature-Grafik, Launch-Screen: **nicht vorhanden** (Phase 6).
