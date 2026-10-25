# SCHLAFWALD – Wenn die Tiere schlafen

Interaktive, deutschsprachige 3D-Einschlafgeschichte für Kinder von 2 bis 6 Jahren. Sieben Waldtiere, zehn behutsame Rituale, ein Abend, der immer leiser wird. Offline, ohne Kinderkonto, Werbung, Tracking oder Punkte.

Private Vorschau: https://schlafwald-gute-nacht-3d.speedysnow4.chatgpt.site · [Prüfbericht](docs/evidence/VERIFICATION.md) · [Pull Request](https://github.com/cschro3/SCHLAFWALD-3D/pull/1)

## Ausprobierbare Laufzeit

`WebApp/` enthält die ergänzende, tatsächlich renderbare Three.js-App und die Capacitor-Projekte für iOS/Android. Sie verwendet **direkt dieselbe Story-Datei** wie das bestehende Unity-Projekt. Das Unity-Projekt und sein Ausgangsstand bleiben erhalten. Gründe und Grenzen: [Engine-Entscheidung](docs/ENGINE_DECISION.md).

```bash
cd WebApp
npm ci
npm run dev
# Offline-/Mobilpaket:
npm run build
npx cap sync
```

Im Querformat öffnen. Der Startknopf beginnt die vollständige Nacht. Elternbereich: oben rechts drei Sekunden halten, anschließend Rechenaufgabe lösen. Alle Lautstärken, ungefähre Wunschdauer, Tierauswahl für die nächste Nacht, Fortsetzen, Audiomodus, Abdunklung und Audiotimer sind dort erreichbar. Der Web-Audiomodus verspricht kein durchgehendes Abspielen bei gesperrtem Gerät.

## Prüfungen

```bash
dotnet test tools/CoreTests
dotnet build tools/RuntimeCompileCheck
dotnet build tools/PlayModeCompileCheck
python tools/unity_metadata.py --check
python tools/verify_assets.py
npm --prefix WebApp test
# Gerenderte End-to-End-Tests (aus WebApp):
node tools/sync-content.mjs
npx vite build --mode qa
node tools/offline-manifest.mjs
npx playwright install chromium
npx playwright test
```

QA-Builds enthalten ausschließlich für die Tests eine Steuer-Schnittstelle. Normale Produktions-Builds entfernen diese beim Kompilieren. Alle Testberichte und der ehrliche Stand stehen in [QA](docs/QA.md) und der CI. Keine Behauptung von Unity-PlayMode-, Geräte- oder Store-Erfolg aus einem Compile-Check ableiten.

## Verzeichnisstruktur

- `UnityProject/`: Originalprojekt (Unity 2022.3.50f1, URP 14); Suri sowie wiederverwendbare C#-Systeme. Fix für endlose Hinweis-Schleife, zerstörungsfreie Projekteinrichtung, Metadaten und Offline-Erzählerdateien.
- `WebApp/src/core.js`: Datenvalidierung, Hilfe, Zeit, Fortschritt und Elterngate.
- `WebApp/src/world.js`: echte 3D-Modelle, Habitate, weiche Animationen und vier Ritualtypen; statische Waldgeometrie wird gebündelt.
- `WebApp/src/main.js`: Geschichte, Touch-/Mauseingaben, zugänglicher Elternbereich, lokale Einstellungen und Lifecycle.
- `WebApp/src/audio.js`: lokale deutsche Sprachdateien, selbst erzeugte ruhige Musik und Waldklänge.
- `WebApp/android`, `WebApp/ios`: native Verpackung desselben Offline-Builds.
- `tools/generate_narration.py`: reproduzierbare lokale Piper-Vertonung aus der Original-Story. Modell/Engine werden nicht in die App eingebaut.
- `docs/`: Entscheidungen, Art Direction, Lizenznachweise, Tests und Release-Vorbereitung.

## Unity weiterhin öffnen

Unity Hub → 2022.3.50f1 → `UnityProject/`; Menü Schlafwald → Projekt einrichten. Bestehende Szenen werden dabei nicht überschrieben. Für einen vollständigen Unity-Release sind die sechs zusätzlichen Unity-Tiermodelle weiterhin nicht implementiert. Eine Migration nach Unity 6.3 wurde ohne aktivierbaren Baseline-Lauf bewusst nicht vorgenommen.

Die App ist noch nicht für die Stores freigegeben. Reale Mobilgeräte, finales Sprecher-/Art-Review, Hintergrund-Audio und Signierung sind verbindliche Release-Schritte. Es wurde kein öffentlicher Store-Upload vorgenommen.
