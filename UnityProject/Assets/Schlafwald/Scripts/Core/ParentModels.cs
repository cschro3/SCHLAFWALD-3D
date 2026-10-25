using System;
using System.Collections.Generic;

namespace Schlafwald.Core
{
    [Serializable]
    public class ParentSettings
    {
        public float masterVolume = 0.8f;
        public float narrationVolume = 1f;
        public float musicVolume = 0.6f;
        public float ambienceVolume = 0.6f;
        public bool musicEnabled = true;
        public string narratorId = "warm";
        /// <summary>Wunschdauer der Geschichte in Minuten; 0 = natürlicher Ablauf.</summary>
        public float targetMinutes = 0f;
        /// <summary>Bildschirm nach dem Ende der Geschichte nach X Minuten ausblenden; 0 = nie.</summary>
        public float dimAfterMinutes = 3f;
        public bool audioOnlyMode = false;
        public bool showSubtitles = false;
        /// <summary>Leer = alle umgesetzten Tiere.</summary>
        public List<string> enabledAnimals = new List<string>();

        public void Clamp()
        {
            masterVolume = C(masterVolume, 0f, 1f);
            narrationVolume = C(narrationVolume, 0f, 1f);
            musicVolume = C(musicVolume, 0f, 1f);
            ambienceVolume = C(ambienceVolume, 0f, 1f);
            targetMinutes = targetMinutes <= 0f ? 0f : C(targetMinutes, 5f, 60f);
            dimAfterMinutes = dimAfterMinutes <= 0f ? 0f : C(dimAfterMinutes, 1f, 30f);
            if (string.IsNullOrEmpty(narratorId)) narratorId = "warm";
            if (enabledAnimals == null) enabledAnimals = new List<string>();
        }

        public bool IsAnimalEnabled(string id) { return enabledAnimals.Count == 0 || enabledAnimals.Contains(id); }
        static float C(float v, float lo, float hi) { return v < lo ? lo : (v > hi ? hi : v); }
    }

    [Serializable]
    public class ProgressData
    {
        public List<string> sleepingAnimals = new List<string>();
        public long lastPlayedUtcTicks;
        public bool finished;
    }

    public static class ProgressRules
    {
        /// <summary>Gleiche Nacht fortsetzen, wenn zuletzt vor kurzer Zeit gespielt und noch nicht beendet.</summary>
        public static bool ShouldResume(ProgressData p, long nowUtcTicks, double maxMinutes = 90)
        {
            if (p == null || p.finished || p.sleepingAnimals.Count == 0) return false;
            double minutes = new TimeSpan(nowUtcTicks - p.lastPlayedUtcTicks).TotalMinutes;
            return minutes >= 0 && minutes <= maxMinutes;
        }
    }

    public static class ScreenDim
    {
        /// <summary>1 = volle Helligkeit. Nach dimAfterMinutes blendet der Bildschirm über fadeSeconds weich aus.</summary>
        public static float Brightness(float secondsSinceEnd, float dimAfterMinutes, float fadeSeconds = 60f)
        {
            if (dimAfterMinutes <= 0f) return 1f;
            float t = secondsSinceEnd - dimAfterMinutes * 60f;
            if (t <= 0f) return 1f;
            if (t >= fadeSeconds) return 0f;
            float k = t / fadeSeconds;
            return 1f - k * k * (3f - 2f * k);
        }
    }

    public struct MathChallenge
    {
        public string Question;
        public int[] Answers;
        public int CorrectIndex;
    }

    /// <summary>Elterngate: 3 s halten + Rechenaufgabe, die Kleinkinder (2–6 Jahre) nicht lösen können.</summary>
    public static class ParentGate
    {
        public static MathChallenge Create(Random rng)
        {
            int a = rng.Next(6, 10), b = rng.Next(6, 10);
            int correct = a * b;
            var answers = new int[3];
            int ci = rng.Next(0, 3);
            var used = new HashSet<int> { correct };
            for (int i = 0; i < 3; i++)
            {
                if (i == ci) { answers[i] = correct; continue; }
                int wrong;
                do { wrong = correct + (rng.Next(0, 2) == 0 ? -1 : 1) * rng.Next(2, 9); } while (wrong <= 0 || !used.Add(wrong));
                answers[i] = wrong;
            }
            return new MathChallenge { Question = a + " × " + b + " = ?", Answers = answers, CorrectIndex = ci };
        }

        public static bool Check(MathChallenge c, int chosenIndex) { return chosenIndex == c.CorrectIndex; }
    }

    public class HoldGate
    {
        readonly float seconds;
        float held;
        public HoldGate(float seconds) { this.seconds = seconds; }
        public float Progress { get { return held >= seconds ? 1f : held / seconds; } }
        public bool Tick(float dt, bool isHeld)
        {
            held = isHeld ? held + dt : 0f;
            return held >= seconds;
        }
    }

    public static class AspectFov
    {
        /// <summary>Hält die horizontale Bildbreite stabil: im Hochformat wird das vertikale FOV vergrößert.</summary>
        public static float VerticalFov(float verticalFovAt16x9, float aspect)
        {
            const float ref169 = 16f / 9f;
            if (aspect >= ref169) return verticalFovAt16x9;
            double h = 2.0 * Math.Atan(Math.Tan(verticalFovAt16x9 * Math.PI / 360.0) * ref169);
            double v = 2.0 * Math.Atan(Math.Tan(h / 2.0) / Math.Max(aspect, 0.3f));
            return (float)(v * 180.0 / Math.PI);
        }
    }
}
