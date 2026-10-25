# Roadmap

| Phase | Inhalt | Status |
|---|---|---|
| 1 Konzeption | Story, Szenenstruktur, Design, Technik | ✅ [01-Konzeption.md](01-Konzeption.md), `story_de.json` |
| 2 Spielbarer Prototyp | echter 3D-Wald, Kamera, Erzählerstruktur, vollständig animierte Suri | ✅ **Code geschrieben** – 🟡 in Unity noch nicht geöffnet/gespielt |
| 3 Alle Tiere | Fino, Mila, Piko, Luna, Nuri, Leni: Builder + Performance + Habitat je Tier | ⏳ |
| 4 Story & Atmosphäre | Feinschliff Texte/Cues, Kameras je Tier, Audio-Mix, Lichtübergänge, Musik | ⏳ (Grundsystem steht) |
| 5 Mobile Qualität | Profiling, Optimierung, Gerätetests, IMGUI → UI Toolkit | ⏳ |
| 6 Release | Icon, Screenshots, Store-Texte, Privacy-Manifest, Build-Konfiguration | ⏳ ([Entwürfe](DATENSCHUTZ_UND_STORES.md)) |

## Nächste konkrete Schritte

1. **Unity-Erstlauf (Sie):** Projekt öffnen, *Projekt einrichten*, Play – Compiler-/Shader-Fehler und visuellen Eindruck zurückmelden. Danach `.meta`-Dateien einchecken.
2. Suri visuell/zeitlich feinjustieren (Kameras, Cue-Zeiten, Farben) anhand des echten Bilds.
3. Phase 3 pro Tier: `XBuilder` (Teile nach Rig-Konvention), `XPerformance` (Cues aus `story_de.json`: z. B. Fino `yawn`/`curlUp`/`tailOverNose`/`sleep`), Habitat (Höhle, Moosnest, Igelnest, Ast, Eichhörnchen-Baum, Lichtung), danach `implemented: true`.
4. Sprecher-Aufnahmen (Skript: `tools/export_recording_script.py`) und Musik.
5. Gerätemessungen (siehe [QA.md](QA.md)), danach Optimierung.
