# Engine-Entscheidung – 25.10.2026

## Tatsächliche Ausgangslage

Unity 2022.3.50f1 und URP 14.0.11 sind im Quellprojekt bestätigt. Das offizielle Linux-Editorarchiv ist erreichbar (4.125.472.828 Bytes). Es existiert hier weder ein Editor noch eine Unity-Lizenz, ein Hub-Login oder ein Lizenzserver. Eine Installation allein würde deshalb keinen rechtmäßig aktivierten Erstlauf ermöglichen. Unity Personal muss über Unity Hub aktiviert werden; es werden weder Aktivierung umgangen noch Kennwörter im Repository gespeichert.

Die Baseline-CI am Commit 0bed788 besteht für .NET und die Datenwerkzeuge. Der bisherige Unity-Job war manuell und lief nicht. Die neue CI meldet fehlende Aktivierung ausdrücklich als BLOCKED und startet Unity-Tests nur mit vollständig vorhandenen Actions-Secrets. Ein grüner Lizenz-Prüfjob bedeutet keinen Unity-Testpass.

Unity 6.3 LTS ist nach Unity-Angaben bis Dezember 2027 unterstützt. Ohne aktivierbaren 2022-Baseline-Lauf ist ein Upgrade von URP 14 einschließlich Shadern und Editor-APIs nicht risikoarm verifizierbar. ProjectVersion bleibt daher unverändert. Eine spätere Migration braucht einen separaten Branch und Vorher-/Nachher-Rendervergleiche.

## Kontrollierte Ausweichlösung

`WebApp/` ist eine ergänzende Three.js-Laufzeit im selben Repository, kein zweites inhaltlich fremdes Projekt. Vite importiert direkt die vorhandene Story-Datei aus UnityProject; alle 101 Zeilen, Kapitel und die zehn Rituale der sieben Tiere bleiben die fachliche Quelle. Die existierenden Weltkoordinaten und Unity-Modelle werden nicht gelöscht oder fälschlich als migrierte Assets deklariert. Die neue Laufzeit normalisiert Ritual-Anordnungen für dieselbe mobile Lichtung; die Original-Unity-Kameras bleiben erhalten.

Die vier Ritualtypen, hilfsbereite Dramaturgie, Pausengründe, 90-Minuten-Fortsetzen, lokale Eltern-Einstellungen und Nachtatmosphäre sind in JavaScript nachvollziehbar übertragen und separat getestet. Die sechs neuen 3D-Figuren sind in der Browser-Laufzeit implementiert. `implemented: false` bleibt für deren Unity-Implementierung korrekt und wurde nicht blind auf true gesetzt.

Chromium rendert hier tatsächlich mit ANGLE/SwiftShader (WebGL 2). Laufende Szenen, Interaktionen, Bildaufnahmen und Offline-Service-Worker sind damit prüfbar. Screenshots sind echte Browser-Renders; das generierte Icon ist getrennte Illustration. Software-Rendering-FPS sind keine Mobilgeräte-FPS.

Capacitor 8.5.3 verpackt denselben Offline-Build für Android und iOS; Android minimum 29/target 36, iOS minimum 16. Eine Xcode-Projektgenerierung ist kein erfolgreicher iOS-Build und ein unsignierter Build kein installierbares Store-Release. Gesperrter Bildschirm/unterbrechungsfreies Hintergrund-Audio bleibt bis zur nativen Geräteprüfung offen: Browser können JavaScript und Audio suspendieren.

## Sichere einmalige Unity-Freischaltung

Nur falls die Unity-Linie weiter verifiziert werden soll: eine rechtmäßige, zum eigenen Unity-Konto gehörende Aktivierung bereitstellen. Für GameCI die vorgesehenen Repository-Settings → Secrets and variables → Actions verwenden: UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD. Diese Werte niemals in Chat, Dateien, Logs oder Commits schreiben. Es wurde kein kostenpflichtiger Dienst beauftragt.

Quellen (abgerufen 25.10.2026):
- https://unity.com/releases/unity-6/support
- https://docs.unity.com/en-us/engine/6000.3/manual/get-started/install-and-upgrade/licenses-and-activation/manual-license-activation/manual-activation-guide
- https://game.ci/docs/github/activation/
- https://threejs.org/docs/pages/WebGLRenderer
- https://capacitorjs.com/docs/ios
