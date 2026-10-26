# Asset- und Lizenznachweise

| Bestandteil | Herkunft | Nutzung / Lizenz |
|---|---|---|
| Sieben 3D-Tiere, Props, Wald | In diesem Repository in `WebApp/src/world.js` und bestehendem Unity-Code erzeugt | Eigenständige Geometrie, keine Asset-Store-/Filmfiguren |
| Deutsche Sprachdateien | Lokal erzeugt mit Piper, Stimme de_DE-thorsten-high | Modellkarte: MIT-Modellrepository, CC0-Sprachdatensatz Thorsten Voice; Modellhash und Dateihashes in audio/manifest.json. Build-Werkzeug Piper ist nicht in der App enthalten. Endgültige Sprecherabnahme offen. |
| Musik und Wind | Eigene Laufzeit-Synthese in audio.js | Keine Fremdaufnahmen, keine Samples aus kommerziellen Titeln |
| Schriften Nunito und Lora | Fontsource / Google Fonts, selbst gehostet | SIL Open Font License, beigefügt unter docs/licenses |
| Three.js | npm three 0.180.0 | MIT, Lizenztext beigefügt |
| Capacitor | Ionic, 8.5.3 | MIT, Lizenztext beigefügt |
| App-Icon | OpenAI Imagegen, 25.10.2026, eigene Figurenreferenz aus echtem Suri-Render | Generiertes Projekt-Asset. Keine Fremdmarken oder fremden Figuren. `WebApp/assets/app-icon-source.png`; Ableitungen für iOS/Android/Web. |
| Screenshots / Video | Automatisierte Aufnahmen aus der laufenden Three.js-App | Echte Renders; keine KI-Illustrationen und keine Unity-Aufnahmen |

Sprachquellen:
- https://huggingface.co/rhasspy/piper-voices/blob/main/de/de_DE/thorsten/high/MODEL_CARD
- https://github.com/thorstenMueller/Thorsten-Voice
- https://github.com/OHF-Voice/piper1-gpl (nur Build-Werkzeug)

Icon-Prompt (built-in Imagegen; Referenz: echter Suri-Screenshot):

> A polished square mobile app icon for SCHLAFWALD, ages 2–6: a single adorable sleeping snail, cream body, peach rose spiral shell, tucked under one soft sage green leaf in a tiny moss nest; a small warm crescent moon above, deep muted teal blue forest background. Soft 3D children’s storybook materials, tender bedtime mood, readable silhouette at 60px. Fully opaque edge-to-edge square, no text, no rounded-corner mask, no copyrighted character. An icon illustration, never an application screenshot.

Die Stimme ist synthetisch; Aufnahmen wurden weder aus kommerziellen Hörbüchern extrahiert noch von einer fremden bekannten Stimme nachgeahmt. Daten-/Modell-Lizenznachweise ersetzen keine abschließende Prüfung der Store-Pakete und sämtlicher transitiver Softwarelizenzen vor Veröffentlichung.

## Emma-Auswahl vom 26.10.2026

`WebApp/assets/voice-reference/suri-emma-take-2.mp3` und die identische private Vorschau-Kopie `WebApp/public/audio/visits/emma-reference.mp3`: vom Nutzer ausgewählte ElevenLabs-Stimme Emma, Generation Sdnyrtn7A56rGWPlL7q3. Herkunft/Hash/Prompt: selection.json neben der Referenz. Dies dokumentiert die Aufnahme und redaktionelle Auswahl, keine pauschale kommerzielle Lizenzfreigabe. Tarif-/Stimmrechte sind vor öffentlichem Vertrieb zu bestätigen. Keine Schlüssel oder temporären Download-URLs gespeichert.
