using System.Collections.Generic;

namespace Schlafwald.Core
{
    /// <summary>Prüft die Story-Daten auf Vollständigkeit, damit Texteditoren nichts kaputt machen können.</summary>
    public static class StoryValidator
    {
        public static readonly string[] StepTypes = { "FollowPath", "DragToTarget", "Touch", "Stroke" };
        public static readonly string[] Props = { "", "leaf", "bigLeaf", "blanket", "acorn", "mossPillow", "firefly", "guideOrb" };
        public static readonly string[] Tones = { "warm", "soft", "whisper" };
        public static readonly string[] ChapterKinds = { "intro", "animal", "finale" };

        public static List<string> Validate(StoryData s)
        {
            var e = new List<string>();
            if (s == null) { e.Add("Story ist null."); return e; }
            if (s.version < 1) e.Add("version fehlt.");
            if (s.narrators.Count == 0) e.Add("Mindestens ein Erzähler nötig.");
            if (s.world == null || s.world.path == null || s.world.path.Length < 2) e.Add("world.path braucht mindestens 2 Punkte.");

            var lineIds = new HashSet<string>();
            var animalIds = new HashSet<string>();
            foreach (var a in s.animals)
            {
                if (string.IsNullOrEmpty(a.id)) { e.Add("Tier ohne id."); continue; }
                if (!animalIds.Add(a.id)) e.Add("Doppelte Tier-id: " + a.id);
            }
            if (s.animals.Count != 7) e.Add("Erwartet 7 Tiere, gefunden " + s.animals.Count + ".");

            foreach (var a in s.animals) ValidateAnimal(a, lineIds, e);

            int intro = 0, finale = 0;
            var chapterAnimals = new HashSet<string>();
            foreach (var c in s.chapters)
            {
                if (System.Array.IndexOf(ChapterKinds, c.kind) < 0) e.Add("Kapitel " + c.id + ": unbekannte kind '" + c.kind + "'.");
                if (c.kind == "intro") intro++;
                if (c.kind == "finale") finale++;
                if (c.kind == "animal")
                {
                    if (!animalIds.Contains(c.animalId)) e.Add("Kapitel " + c.id + ": unbekanntes Tier '" + c.animalId + "'.");
                    else if (!chapterAnimals.Add(c.animalId)) e.Add("Tier " + c.animalId + " hat mehrere Kapitel.");
                }
                var shotIds = new HashSet<string>();
                foreach (var sh in c.shots) shotIds.Add(sh.id);
                foreach (var l in c.lines)
                {
                    CheckLine(l, "Kapitel " + c.id, lineIds, e);
                    if (!string.IsNullOrEmpty(l.animalId) && !animalIds.Contains(l.animalId))
                        e.Add("Zeile " + l.id + ": unbekanntes animalId '" + l.animalId + "'.");
                    if (!string.IsNullOrEmpty(l.shotId) && !l.shotId.StartsWith("@") && !shotIds.Contains(l.shotId))
                        e.Add("Zeile " + l.id + ": unbekannter shotId '" + l.shotId + "'.");
                    if (l.shotId != null && l.shotId.StartsWith("@"))
                    {
                        var parts = l.shotId.Substring(1).Split('.');
                        if (parts.Length != 2 || !animalIds.Contains(parts[0])) e.Add("Zeile " + l.id + ": ungültiger Tier-Shot '" + l.shotId + "'.");
                    }
                }
            }
            if (intro != 1) e.Add("Genau ein intro-Kapitel erwartet.");
            if (finale != 1) e.Add("Genau ein finale-Kapitel erwartet.");
            foreach (var id in animalIds) if (!chapterAnimals.Contains(id)) e.Add("Tier " + id + " hat kein Kapitel.");
            return e;
        }

        static void ValidateAnimal(AnimalConfig a, HashSet<string> lineIds, List<string> e)
        {
            string who = "Tier " + a.id;
            if (string.IsNullOrEmpty(a.displayName)) e.Add(who + ": displayName fehlt.");
            if (string.IsNullOrEmpty(a.modelId)) e.Add(who + ": modelId fehlt.");
            if (a.sleepSpot == null || a.sleepSpot.position == null) e.Add(who + ": sleepSpot fehlt.");
            if (string.IsNullOrEmpty(a.successTrigger)) e.Add(who + ": successTrigger fehlt.");
            if (a.animationStates == null || a.animationStates.Length == 0) e.Add(who + ": animationStates fehlen.");
            if (a.steps.Count == 0) e.Add(who + ": mindestens ein Interaktionsschritt nötig.");

            for (int i = 0; i < a.steps.Count; i++)
            {
                var st = a.steps[i];
                string sw = who + " Schritt " + i;
                if (System.Array.IndexOf(StepTypes, st.type) < 0) e.Add(sw + ": unbekannter Typ '" + st.type + "'.");
                if (System.Array.IndexOf(Props, st.prop ?? "") < 0) e.Add(sw + ": unbekannte prop '" + st.prop + "'.");
                if (st.points == null || st.points.Length == 0) e.Add(sw + ": points fehlen.");
                if (st.type == "DragToTarget" && st.target == null) e.Add(sw + ": target fehlt.");
                if (st.type == "FollowPath" && (st.points == null || st.points.Length < 2)) e.Add(sw + ": FollowPath braucht >= 2 Punkte.");
                if (st.radius <= 0.2f) e.Add(sw + ": radius zu klein für Kleinkinder (>= 0.2 m).");
                if (st.assistAfter <= st.helpAfter) e.Add(sw + ": assistAfter muss größer als helpAfter sein.");
                if (a.GetStepLines("step", i).Count == 0) e.Add(sw + ": Erzähler-Aufforderung (phase step" + i + ") fehlt.");
                if (a.GetStepLines("hint", i).Count == 0) e.Add(sw + ": Hinweis (hint" + i + " oder hint) fehlt.");
                if (a.GetStepLines("assist", i).Count == 0) e.Add(sw + ": Hilfe (assist" + i + " oder assist) fehlt.");
            }
            if (a.GetLines("intro").Count == 0) e.Add(who + ": intro-Text fehlt.");
            if (a.GetLines("outro").Count == 0) e.Add(who + ": outro-Text fehlt.");
            foreach (var l in a.lines) CheckLine(l, who, lineIds, e);
            var shotIds = new HashSet<string>();
            foreach (var sh in a.shots) if (!shotIds.Add(sh.id)) e.Add(who + ": doppelter shot " + sh.id);
        }

        static void CheckLine(NarrationLine l, string where, HashSet<string> ids, List<string> e)
        {
            if (string.IsNullOrEmpty(l.id)) { e.Add(where + ": Zeile ohne id."); return; }
            if (!ids.Add(l.id)) e.Add("Doppelte Zeilen-id: " + l.id);
            if (string.IsNullOrWhiteSpace(l.text)) e.Add("Zeile " + l.id + ": Text leer.");
            if (System.Array.IndexOf(Tones, l.tone) < 0) e.Add("Zeile " + l.id + ": unbekannter tone '" + l.tone + "'.");
            if (l.when != "" && l.when != "all" && l.when != "partial") e.Add("Zeile " + l.id + ": unbekanntes when '" + l.when + "'.");
            if (l.pauseAfter < 0f || l.pauseAfter > 10f) e.Add("Zeile " + l.id + ": pauseAfter außerhalb 0..10.");
        }
    }
}
