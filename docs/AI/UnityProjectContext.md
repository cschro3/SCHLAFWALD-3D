# Unity-Projektkontext – 25.10.2026

Baseline: `0bed788ff3e962ac7bc748e3671426b97e0ed90e`; Ausgangsbranch `claude/phase-1-2-suri-prototype`.

Bestätigt: UnityProject ist ein Unity-Projekt, Editor 2022.3.50f1, URP 14.0.11, Unity Test Framework 1.1.33, Legacy Input (`Input.GetTouch` / Maus). Keine Netzwerkdienste, kein MCP-Paket. Keine Packages-Lockdatei, keine serialisierte Main-Szene oder BuildSettings in der Baseline. ProjectSetup erzeugt diese erst im Editor.

Core ist Unity-frei. Runtime referenziert Core, Editor referenziert Runtime + URP. Test-Assemblies: EditMode (Core), PlayMode (Runtime). Komposition: AppComposer → AppServices mit expliziten Referenzen; StoryDirector spielt JSON-Kapitel und Animation-Cues. Vier wiederverwendbare Rituale; AnimalController erzeugt ausschließlich die Schnecke. Die übrigen sechs Tiere sind in den gemeinsamen Story-Daten vollständig beschrieben, `implemented: false` gilt weiterhin für Unity.

Laufzeit: prozedurale Meshes/Materialien, drei eigene Shader, Kamera- und Animationskanäle, lokaler PlayerPrefs-Fortschritt mit 90-Minuten-Fenster, Pausengründe, IMGUI-Elternprototyp. Stil: warmer Kinderbuchwald, runde Formen, langsame Kamerafahrten, keine harten Schatten. C#-Namensräume Schlafwald / Schlafwald.Core, PascalCase-API, explizite Init-Methoden.

Werkzeuge hier: kein Unity-Editor/Hub, keine Unity-Lizenz, kein Docker, keine physische GPU, kein verbundenes Mobilgerät und kein Unity-MCP. Editorarchiv erreichbar (4.125 GB), Aktivierung fehlt. Kein Editor-, Shader- oder Gerätepass wird aus .NET-Checks abgeleitet. Baseline-GitHub-CI lief erfolgreich am Ausgangscommit, Unity-Job war nicht aktiviert.

Zusätzliche Laufzeit in `WebApp/` nutzt dieselben Story-Daten; sie ersetzt oder löscht Unity nicht. Eine Unity-6.3-Migration wird bis zu einem aktivierten Unity-Baseline-Lauf zurückgestellt.

Quellen: README, sämtliche angeforderten docs, ci.yml, story_de.json, Packages/manifest.json, ProjectVersion, asmdefs, AppComposer, StoryDirector, alle Core-Klassen, AnimalController/Rig/Performance/SnailBuilder/SnailPerformance, InteractionSteps, CameraDirector, Lighting/World/Habitat, Audio und Tests.
