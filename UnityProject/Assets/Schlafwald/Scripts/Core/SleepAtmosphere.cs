using System;
using System.Collections.Generic;

namespace Schlafwald.Core
{
    public struct AtmosphereValues
    {
        public float Calm;              // 0 = Abend, 1 = alle schlafen
        public float LightMultiplier;   // Gesamthelligkeit
        public float Motion;            // Blätter, Gräser, Partikel-Geschwindigkeit
        public float Particles;         // Glühwürmchen & Co.
        public float AmbienceActivity;  // Dichte einzelner Waldgeräusche
        public float MusicIntensity;    // Musiklautstärke-Faktor
        public float TimeOfDay;         // 0 = Sonnenuntergang, 1 = tiefe Nacht
    }

    /// <summary>Alle Kurven, mit denen der Wald mit jedem schlafenden Tier ruhiger und dunkler wird.</summary>
    public static class SleepAtmosphere
    {
        public static float CalmFromSleepiness(IEnumerable<float> sleepiness, int totalAnimals)
        {
            if (totalAnimals <= 0) return 0f;
            float sum = 0f;
            foreach (var s in sleepiness) sum += Clamp01(s);
            return Clamp01(sum / totalAnimals);
        }

        public static AtmosphereValues Evaluate(float calm)
        {
            calm = Clamp01(calm);
            float e = calm * calm * (3f - 2f * calm);
            return new AtmosphereValues
            {
                Calm = calm,
                LightMultiplier = Lerp(1f, 0.30f, e),
                Motion = Lerp(1f, 0.18f, e),
                Particles = Lerp(1f, 0.12f, e),
                AmbienceActivity = Lerp(1f, 0.05f, e),
                MusicIntensity = Lerp(0.65f, 0.22f, e),
                TimeOfDay = 0.05f + 0.95f * e,
            };
        }

        /// <summary>Framerate-unabhängige Glättung, damit nichts abrupt springt.</summary>
        public static float Smooth(float current, float target, float dt, float halfLife)
        {
            if (halfLife <= 0f) return target;
            float k = 1f - (float)Math.Pow(0.5, dt / halfLife);
            return current + (target - current) * k;
        }

        static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
    }
}
