using System.Collections.Generic;

namespace Schlafwald.Core
{
    /// <summary>Berechnet, wie stark Pausen gestreckt/gestaucht werden, damit die Geschichte etwa die Wunschdauer der Eltern hat.</summary>
    public static class PacingPlanner
    {
        public const float TypicalSecondsPerStep = 25f;
        public const float DefaultShotSeconds = 8f;
        public const float MinScale = 0.5f;
        public const float MaxScale = 3.0f;

        public struct Estimate { public float FixedSeconds; public float FlexibleSeconds; public float Total { get { return FixedSeconds + FlexibleSeconds; } } }

        public static Estimate EstimateStory(StoryData story, ICollection<string> enabledAnimalIds, float narratorSpeed = 1f)
        {
            var est = new Estimate();
            foreach (var c in story.chapters)
            {
                if (c.kind == "animal" && !enabledAnimalIds.Contains(c.animalId)) continue;
                AddLines(c.lines, enabledAnimalIds, narratorSpeed, ref est);
                if (c.kind == "animal")
                {
                    var a = story.FindAnimal(c.animalId);
                    est.FixedSeconds += DefaultShotSeconds;
                    foreach (var phase in new[] { "intro", "outro" }) AddLines(a.GetLines(phase), enabledAnimalIds, narratorSpeed, ref est);
                    for (int i = 0; i < a.steps.Count; i++)
                    {
                        AddLines(a.GetStepLines("step", i), enabledAnimalIds, narratorSpeed, ref est);
                        AddLines(a.GetStepLines("success", i), enabledAnimalIds, narratorSpeed, ref est);
                        est.FlexibleSeconds += TypicalSecondsPerStep;
                    }
                }
            }
            return est;
        }

        static void AddLines(List<NarrationLine> lines, ICollection<string> enabled, float speed, ref Estimate est)
        {
            foreach (var l in lines)
            {
                if (!string.IsNullOrEmpty(l.animalId) && !enabled.Contains(l.animalId)) continue;
                est.FixedSeconds += NarrationTiming.EstimateSeconds(l.text, speed);
                est.FlexibleSeconds += l.pauseAfter;
            }
        }

        /// <summary>Faktor für pauseAfter und Ruhezeiten. targetMinutes &lt;= 0 → unverändert.</summary>
        public static float PauseScale(Estimate est, float targetMinutes)
        {
            if (targetMinutes <= 0f || est.FlexibleSeconds <= 0f) return 1f;
            float s = (targetMinutes * 60f - est.FixedSeconds) / est.FlexibleSeconds;
            return s < MinScale ? MinScale : (s > MaxScale ? MaxScale : s);
        }
    }
}
