# Asset-Stand und verbleibende Art-Abnahme

Die ausführbare WebApp besitzt sieben echte, animierbare 3D-Figuren und Habitate. Ihre Geometrie ist eigenständig im Projekt erzeugt; eine 2D-Illustration ersetzt keine Figur. Der originale Unity-Suri-Prototyp bleibt erhalten, dort fehlen weiterhin sechs Tiermodelle.

## Vorhanden

- Fino: Fuchsohren und Schnauze, Höhle, weich geformte Decke, Schwanzpose.
- Mila: lange Ohren, Moosnest, Streichelritual und entspannte Schlafpose.
- Piko: weiche Stacheln, Blätternest, Blättersammeln und Einrollen.
- Luna: Gesichtsscheiben, Baum/Ast, Glühwürmchen und Kopf ins Gefieder.
- Nuri: Baumhöhle, Kuscheleichel, Umarmen und Schwanzdecke.
- Leni: Rehkitzohren, längere Schnauze/Hals, Rückenpunkte, gefaltete Beine und Mooskissen.
- Suri: Spiralhaus, bewegliche Augen/Fühler, Kriechwelle, gebogenes schützendes Blatt, vollständiger Rückzug.
- Eigener 3D-Wald mit räumlicher Tiefe, Nebel, Pilzen, Moos, Mond und ruhigen Lichtübergängen.
- 101 gesprochene deutsche MP3s; erzeugte Klangflächen/Wind. Sprecher-/Musik-Endabnahme offen.
- App-Icon und Plattformableitungen; echte Browser-Szenenaufnahmen und Video für das Review.

## Qualitätsgrenzen

Diese Vorabversion ist visuell tatsächlich geprüft. Sie ist noch keine bestätigte Premium-Art-/Store-Abnahme. Prozedurale Baumkronen, wiederholte Farnformen, einfache Fell-/Federoberflächen und Geräusche können weiter künstlerisch verfeinert werden. Alle Änderungen müssen Silhouetten, mobile Budgets und wiederverwendbare Animationsschnittstellen erhalten.

[ART_DIRECTION.md](ART_DIRECTION.md) hält Farben, Materialcharakter und Figurenregeln fest; [ASSET_LICENSES.md](ASSET_LICENSES.md) dokumentiert Herkunft/Lizenzen. Das App-Icon ist eine gekennzeichnete Illustration, die Review-Screenshots sind echte Renders. Gerätespezifische Store-Screenshots werden erst am geprüften Zielgerät abgenommen.

## Modell-Austausch

WebApp: `buildAnimal(id, parent)` liefert Rig, `cue(name)` und `update(dt,time)`; Story- und Interaktionssystem dürfen beim Austausch unverändert bleiben. Individuelle Teile benötigen stabile Pivots und Ruheposen. Statische Habitate werden nach Material gebündelt; bewegte Teile davon ausnehmen.

Unity: `AnimalPerformance` (`Cue`, `OnStep*`, `FinishSleep`) und `AnimalRig.CaptureRest` bleiben die Integrationsschnittstelle. Suris benannte Teile sind Body, Foot*, Neck, Head, EyeL/EyeR, FeelerL/R und Shell; genaue bestehende Builder-Hierarchie vor einem Import prüfen. Die sechs `implemented:false`-Flags erst nach tatsächlicher Unity-Implementierung und Tests setzen.
