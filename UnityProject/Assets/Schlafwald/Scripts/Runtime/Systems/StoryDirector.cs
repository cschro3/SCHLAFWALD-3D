using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Dramaturgie der Nacht: Kapitel 1 (Abend), je Tier ein Kapitel (Kamerafahrt → Einführung → Ritual → Einschlafen), Kapitel 9 (der ganze Wald schläft).
    /// Alles aus Daten (story_de.json). Erzählerzeilen lösen Animations-Cues aus – Sprache und Bild bleiben synchron.
    /// </summary>
    public class StoryDirector : MonoBehaviour
    {
        AppServices s;
        readonly List<AnimalController> animals = new List<AnimalController>();
        readonly HashSet<string> participating = new HashSet<string>();
        public bool Finished { get; private set; }
        public bool Started { get; private set; }
        public string Log = "";     // Ablaufprotokoll für Tests/Diagnose

        public void Init(AppServices services, IEnumerable<AnimalController> controllers)
        {
            s = services;
            animals.AddRange(controllers);
            foreach (var a in animals) participating.Add(a.Id);
        }

        public ICollection<string> Participating { get { return participating; } }
        void Note(string m) { Log += m + "\n"; }

        List<NarrationLine> Filter(IEnumerable<NarrationLine> lines) { return LineFilter.Filter(lines, participating, s.Story.animals.Count); }

        public IEnumerator Run()
        {
            Started = true;
            var story = s.Story;
            var est = PacingPlanner.EstimateStory(story, participating);
            s.Narration.PauseScale = PacingPlanner.PauseScale(est, s.Settings.Current.targetMinutes);

            bool resume = s.Progress.ShouldResume();
            var already = new HashSet<string>();
            if (resume) foreach (var id in s.Progress.Data.sleepingAnimals) if (participating.Contains(id)) already.Add(id);
            if (already.Count == 0) { resume = false; s.Progress.Reset(); }
            foreach (var a in animals)
                if (already.Contains(a.Id)) { a.PlaceAsleep(s); s.Interaction.SpawnCompleted(a); }

            // ---- Kapitel 1 ----
            var intro = story.FindChapter("intro");
            s.Forest.SetPhase(StoryPhase.Intro, intro.id);
            Note("chapter:" + intro.id);
            StartCoroutine(s.Transitions.FadeTo(0f, 2.5f));
            if (!resume)
            {
                foreach (var l in Filter(intro.lines))
                {
                    TriggerShot(intro, l.shotId);
                    yield return s.Narration.Say(l);
                }
            }
            else
            {
                s.CameraDirector.StartFlight(Shot.From(intro.shots.Find(x => x.id == "gate")), 4f);
            }

            // ---- Kapitel 2..8 ----
            foreach (var ch in story.chapters)
            {
                if (ch.kind != "animal" || !participating.Contains(ch.animalId) || already.Contains(ch.animalId)) continue;
                yield return RunAnimalChapter(ch);
            }

            // ---- Kapitel 9 ----
            yield return RunFinale(story.FindChapter("finale"));
            Finished = true;
            Note("finished");
        }

        void TriggerShot(ChapterConfig ch, string shotId)
        {
            if (string.IsNullOrEmpty(shotId)) return;
            Shot shot;
            if (shotId.StartsWith("@"))
            {
                var parts = shotId.Substring(1).Split('.');
                var a = s.Story.FindAnimal(parts[0]);
                shot = CameraDirector.ShotFor(a, parts[1]);
            }
            else
            {
                var c = ch.shots.Find(x => x.id == shotId);
                if (c == null) return;
                shot = Shot.From(c);
            }
            float dist = Vector3.Distance(s.CameraDirector.Current.position, shot.position);
            float dur = Mathf.Min(24f, Mathf.Max(shot.duration, dist / 3.2f));
            s.CameraDirector.StartFlight(shot, dur);
        }

        IEnumerator RunAnimalChapter(ChapterConfig ch)
        {
            var animal = animals.Find(a => a.Id == ch.animalId);
            var cfg = animal.Config;
            s.Forest.SetPhase(StoryPhase.Animal, ch.id);
            Note("chapter:" + ch.id);
            StartCoroutine(s.Transitions.ChapterTurn());

            System.Action<string> cue = animal.Cue;
            s.Narration.CueRequested += cue;
            try
            {
                var approach = CameraDirector.ShotFor(cfg, "approach");
                float dist = Vector3.Distance(s.CameraDirector.Current.position, approach.position);
                s.CameraDirector.StartFlight(approach, Mathf.Min(24f, Mathf.Max(approach.duration, dist / 3.2f)));
                yield return s.Narration.SayLines(Filter(cfg.GetLines("intro")));
                while (s.CameraDirector.IsFlying) yield return null;

                var interact = CameraDirector.ShotFor(cfg, "interact");
                s.CameraDirector.StartFlight(interact, interact.duration);

                for (int i = 0; i < cfg.steps.Count; i++)
                {
                    Note("step:" + cfg.id + ":" + i);
                    yield return s.Interaction.RunStep(animal, i);
                }

                var sleepShot = CameraDirector.ShotFor(cfg, "sleep");
                s.CameraDirector.StartFlight(sleepShot, sleepShot.duration);
                Note("outro:" + cfg.id);
                yield return s.Narration.SayLines(Filter(cfg.GetLines("outro")));
                yield return animal.CompleteSleep();
                s.Progress.MarkSleeping(cfg.id);
                Note("asleep:" + cfg.id);
                yield return new WaitForSeconds(2.5f * s.Narration.PauseScale);
            }
            finally { s.Narration.CueRequested -= cue; }
        }

        IEnumerator RunFinale(ChapterConfig fin)
        {
            s.Forest.SetPhase(StoryPhase.Finale, fin.id);
            Note("chapter:" + fin.id);
            StartCoroutine(s.Transitions.ChapterTurn());
            foreach (var l in Filter(fin.lines))
            {
                TriggerShot(fin, l.shotId);
                yield return s.Narration.Say(l);
            }
            s.Forest.SetPhase(StoryPhase.Ended, fin.id);
            s.Audio.FadeOutMusic(45f);     // Musik blendet langsam aus; Waldszene bleibt ruhig sichtbar
            s.Progress.MarkFinished();
            s.Transitions.EndedAt = Time.time;
        }
    }
}
