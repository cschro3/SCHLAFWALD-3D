#!/usr/bin/env python3
"""Exportiert alle Erzählerzeilen als Sprecher-Skript (Markdown + CSV) für die Studio-Aufnahme.

Dateiname der Aufnahme = Zeilen-ID (bzw. audioKey) → UnityProject/Assets/Schlafwald/Resources/Audio/Narration/<Stimme>/<ID>.wav|ogg
Nutzung: python3 tools/export_recording_script.py [ausgabeordner]
"""
import csv, json, os, sys

root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
story = json.load(open(os.path.join(root, "UnityProject/Assets/Schlafwald/Resources/Story/story_de.json"), encoding="utf-8"))
out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(root, "docs", "recording")
os.makedirs(out, exist_ok=True)

TONE = {"warm": "warm, freundlich, langsam", "soft": "leiser, sehr ruhig, fast flüsternd-weich", "whisper": "geflüstert, zärtlich, langsam ausklingend"}
rows = []
for ch in story["chapters"]:
    for l in ch["lines"]:
        rows.append((ch["id"] + " " + ch["title"], l))
    if ch["kind"] == "animal":
        a = next(x for x in story["animals"] if x["id"] == ch["animalId"])
        for l in a["lines"]:
            rows.append((ch["id"] + " " + a["displayName"], l))

with open(os.path.join(out, "sprecher_skript.csv"), "w", newline="", encoding="utf-8") as f:
    w = csv.writer(f, delimiter=";")
    w.writerow(["datei", "kapitel", "phase", "ton", "pause_danach_s", "bedingung", "text"])
    for chap, l in rows:
        w.writerow([(l.get("audioKey") or l["id"]), chap, l.get("phase", ""), l.get("tone", "warm"), l.get("pauseAfter", 1.2), l.get("when") or l.get("animalId") or "", l["text"]])

with open(os.path.join(out, "sprecher_skript.md"), "w", encoding="utf-8") as f:
    f.write("# Sprecher-Skript SCHLAFWALD\n\nTempo: ca. 105–110 Wörter/Minute, bewusste Pausen bei „…“. Eine Datei pro Zeile, Dateiname = ID.\n\n")
    cur = None
    for chap, l in rows:
        if chap != cur:
            f.write("\n## " + chap + "\n\n"); cur = chap
        f.write("- **%s** _(%s, danach %.1f s Pause)_  \n  %s\n" % (l.get("audioKey") or l["id"], TONE.get(l.get("tone", "warm")), l.get("pauseAfter", 1.2), l["text"]))
print("%d Zeilen exportiert nach %s" % (len(rows), out))
