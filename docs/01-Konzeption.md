# Konzeption – SCHLAFWALD

## 1. Leitidee

Keine Spielmechanik, sondern eine **erzählte Einschlafgeschichte**: Ein Elternteil und ein Kind begleiten sieben Tiere in den Schlaf.
Jede Handlung des Kindes ist eine fürsorgliche Geste (zudecken, streicheln, Nest bauen, Licht dimmen, Kuscheltier zurückgeben, Dach bauen).
Kein Punktestand, keine Zeit, keine Niederlage, kein Applaus – das Ende ist **Stille**: sieben schlafende Tiere, ein Mond, „Gute Nacht“.

## 2. Dramaturgie (Kapitelstruktur)

| Kapitel | Inhalt | Kamera | Licht/Ton |
|---|---|---|---|
| 1 Der Abend kommt | Begrüßung, Sonnenuntergang, Waldeingang | langsame Fahrt vom Waldrand zum Pfad | Sonne tief, warm; Wind, leise Klangfläche |
| 2 Fino 🦊 | Decke über den Fuchs ziehen | Anflug entlang des Pfads → Höhle | Dämmerung beginnt |
| 3 Mila 🐰 | Kaninchen streicheln | | |
| 4 Piko 🦔 | drei Blätter zum Nest schieben | | |
| 5 Luna 🦉 | drei Glühwürmchen berühren | | Licht wird weicher |
| 6 Nuri 🐿️ | Eichel zurückbringen | | |
| 7 Leni 🦌 | Rücken streicheln, Mooskissen | Lichtung im Mondlicht | Mond dominiert |
| 8 Suri 🐌 | Schnecke begleiten, Blätterdach, Haus berühren | nah, Augenhöhe der Schnecke | fast Nacht |
| 9 Der ganze Wald schläft | Rückfahrt/Überflug, alle Tiere, Mond | Überblick → Tier für Tier → Mond | Musik blendet aus, Wald fast unbewegt |

Jedes Tierkapitel folgt demselben Takt (**„Umblättern“**): Kapitelwechsel mit sanfter Randabdunklung → Kamerafahrt zum Tier (mit Einführung durch den Erzähler)
→ Ritual (Aufforderung → Hinweis → Selbsthilfe) → Einschlafen (Erzählzeilen lösen Animations-Cues aus) → kurze Stille → nächste Fahrt.
Die Tiere sind dadurch keine isolierten Minispiele, sondern Stationen auf **einem** Pfad.

### Weltlayout (Meter, +Z = Pfadrichtung; Pfad: `world.path` in story_de.json)

| Tier | Schlafplatz (x, z) | Seite | Ritual |
|---|---|---|---|
| Fino | (−3.6, 12) | links | Decke ziehen (Drag) |
| Mila | (3.4, 23) | rechts | Streicheln (Stroke) |
| Piko | (−3.4, 33) | links | 3 Blätter schieben (Drag ×3) |
| Luna | (3.6, 43), Ast auf 2.3 m | rechts | 3 Glühwürmchen berühren (Touch ×3) |
| Nuri | (−3.6, 53) | links | Eichel in die Pfoten (Drag) |
| Leni | (3.6, 63) | rechts | Rücken streicheln + Kissen (Stroke, Drag) |
| Suri | (3.3, 72.9) | rechts | Pfad folgen (FollowPath) → Blätterdach (Drag) → Häuschen berühren (Touch) |

Dazwischen: Baumhäuser mit leuchtenden Fenstern (z ≈ 17, 37, 58), Tierhöhlen, bemooste Steine, Pilze, Farne, Blumen, Nebelbänke, Glühwürmchen.
Wald-Segmente à 20 m werden nach Kameraabstand aktiviert.

## 3. Visuelles Design

- **Stil:** stilisiertes, weiches Kinderbuch-3D; runde, organische Formen; große Augen; Kuschel-Silhouetten; keine Kanten, keine Stacheln (auch nicht beim Igel – später Stoffstacheln-Look).
- **Palette:** Abend: Pfirsich/Rosé/Violett; Nacht: Indigo/Blau-Grün; Moos-Grün, warme Fensterlichter. Keine Neon-/Grellfarben.
- **Licht:** ein gerichtetes Licht (Sonne → Mond) **ohne Schatten**, Trilight-Ambient, exponentieller Nebel; Rim-Licht für kuschelige Silhouetten.
- **Material (Shader `StylizedLit`):** Wrap-Diffuse, weiche Schattenfarbe, prozedurales Fell-/Material-Rauschen, Wind-Schwingen über Vertexfarben-Alpha, globaler `_SleepMotion` beruhigt alles bei Einbruch der Nacht.
- **Himmel (`SkyDome`):** Farbverlauf, weiche Sonne, Mond mit Hof, prozedurale Sterne (kein Texturspeicher).
- **Kamera:** nur langsame, gebogene Fahrten (Bézier entlang des Pfads, kritisch gedämpfte Verfolgung, 0.05 m „Atmen“); nie Schnitte, nie Wackeln.
- **Verboten:** harte Schatten, grelle Farben, Jump-Scares, hektische Kamera, plötzliche Lautstärkespitzen.

## 4. Layout & Bedienung

- **Querformat fest** (Landscape Left/Right): weite Waldbilder, stabiles Halten auf dem Schoß/Ständer, Kamerafotos der Rituale in 16:9 bis 4:3 geprüft. `AspectFov` hält die Bildbreite stabil, falls ein Gerät doch schmaler ist.
- **Keine Lesefähigkeit nötig:** Erzähler statt Text; pulsierende sanfte Lichter markieren, wo man hinfassen soll.
- **Großzügige Touch-Zonen:** mindestens 11 % der Bildschirmhöhe oder der Weltradius (≥ 0.6 m); Magnet beim Ablegen; Loslassen „daneben“ führt Objekte sanft zurück – nie ein Fehlerzustand.
- **Nur ein Finger** wird ausgewertet; Mehrfachberührungen stören nichts.
- **Keine Menüs im Kinderbereich.** Einziges UI-Element: ein sehr schwacher Elternbereich-Hotspot oben rechts (3 s halten + Rechenaufgabe 6–9 × 6–9).

## 5. Interaktionsmodell (wiederverwendbar)

| Typ | Verhalten | Genutzt von |
|---|---|---|
| `FollowPath` | Leitlicht wartet 0.45 m voraus; solange der Finger es begleitet, folgt das Tier (Tempo 0.12 m/s) | Suri |
| `DragToTarget` | Objekte (1…n) per Drag zum Ziel; Magnet; Rückkehr bei „daneben“; Ablegen mit weicher Kurve | Fino, Piko, Nuri, Leni, Suri |
| `Touch` | Ziele (1…n) für `holdSeconds` berühren; Licht wird danach sanfter | Luna, Suri |
| `Stroke` | Fingerweg über dem Tier summiert „Streichelstrecke“ | Mila, Leni |

**Hilfe ohne Niederlage (`HelpEscalator`):** Der Erzähler spricht die Aufforderung (Zähler läuft nicht, solange er spricht) →
nach `helpAfter` Sekunden Hinweis (Halo pulsiert stärker, Erzähler wiederholt freundlich, alle `helpAfter`) →
nach `assistAfter` Sekunden führt das Tier sein Ritual **selbst** zu Ende (`BeginAssist`) – mit eigener, liebevoller Erzählzeile. Genau einmal; nie Abbruch.
Im Audiomodus laufen die Wartezeiten 6× schneller.

## 6. Der Wald verändert sich (`SleepForestController`)

`Calm` = Mittelwert der Müdigkeit (`Sleepiness` 0…1) aller **teilnehmenden** Tiere, über 2.5 s Halbwertszeit geglättet.

| Calm | 0 (Abend) | 1 (alle schlafen) |
|---|---|---|
| Licht (Tageszeit) | Sonnenuntergang | Mondnacht (nie stockdunkel: Mondlicht-Intensität 0.62 + Ambient) |
| Bewegung (Gras/Laub, Partikelgeschwindigkeit) | 100 % | 18 % |
| Partikel (Glühwürmchen) | 100 % | 12 % |
| Waldgeräusche (Dichte) | 100 % | 5 % |
| Musikintensität | 0.65 | 0.22, danach 45 s Ausblenden |

Alle Kurven sind in `Core/SleepAtmosphere` getestet (monoton, kein Überschwingen, framerate-unabhängig).

## 7. Der Erzähler

- Texte = Daten (`story_de.json`): `id`, `text`, `phase`, `pauseAfter`, `tone` (warm/soft/whisper), optional `narrator` (zweite Stimme), `cue`, `cueDelay`, `animalId`/`when` (Bedingungen), `shotId` (Kamera).
- **Synchronisierung:** `cue` + `cueDelay` lösen Animationen exakt zum gesprochenen Satz aus (Suri: „Fühler einziehen“ → `retractFeelers`, „Ganz langsam …“ → `enterShell`, …).
- **Audio:** `Resources/Audio/Narration/<Stimme>/<ID>` ersetzt jede Zeile ohne Codeänderung. Fehlende Aufnahme → **ausdrücklich gekennzeichneter** Entwicklungs-Fallback (wortloses Murmeln, Untertitel, gelber Hinweis im Editor/Debug-Build, Log-Warnung, Zähler im Elternbereich).
- **Wunschdauer:** `PacingPlanner` streckt/staucht Pausen (Faktor 0.5–3) auf die gewünschte Gesamtdauer.

## 8. Technische Leitplanken (mobil)

- URP, MSAA 4×, keine Schatten, kein HDR, keine Kameratexturen; 30 fps Ziel (ruhige Szenen schonen Akku); 10 fps nach dem Ausblenden.
- Prozedural gebaute Meshes mit Vertexfarben, SRP-Batcher-fähige Shader (ein Material pro Gruppe), Segment-Culling.
- Budget (Ziel, **noch ungemessen**): ≤ 150 Draw-Calls, ≤ 400k Vertices sichtbar, ≤ 250 MB RAM, ≤ 5 % Akku / 10 min Geschichte auf aktuellen Geräten.
- Vollständig offline; **keine** Netzwerk-API im Code; Android ohne `INTERNET`-Berechtigung (siehe `ProjectSetup`).

## 9. Architektur

`AppComposer` baut zur Laufzeit alles auf und reicht Abhängigkeiten explizit über `AppServices` weiter (keine Singletons, testbar):

`StoryDirector` (Dramaturgie) → `CameraDirector` · `NarrationManager` · `AnimalInteractionController` → `AnimalController` → `AnimalPerformance` ·
`SleepForestController` → `LightingController` · `AudioEnvironmentManager` · `AtmosphereParticles` · `SceneTransitionManager` · `SaveProgressManager` · `ParentSettingsManager`.

Pause (`PauseController`) hält die Zeit (`Time.timeScale`) und das Audio an – Gründe: Eltern-Pause, Elternbereich offen, App im Hintergrund.
