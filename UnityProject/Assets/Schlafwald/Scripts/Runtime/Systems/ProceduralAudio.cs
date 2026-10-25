using System;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>
    /// Entwicklungs-Klangquellen, die zur Laufzeit erzeugt werden (Wind, Grillen, Klangflächen, Klavier-Glöckchen, kleine Tiergeräusche).
    /// Das sind ECHTE Audiodaten, aber PLATZHALTER: Jede Datei in Resources/Audio/{Ambience|Music|Sfx} mit gleichem Schlüssel ersetzt sie
    /// (siehe docs/AUDIO_PIPELINE.md).
    /// </summary>
    public static class ProceduralAudio
    {
        public const int HiRate = 22050, LoRate = 11025;
        static System.Random rng = new System.Random(77);

        static float F() { return (float)rng.NextDouble(); }
        static AudioClip Make(string name, float[] data, int rate)
        {
            var c = AudioClip.Create(name, data.Length, 1, rate, false);
            c.SetData(data, 0);
            return c;
        }

        static float[] Crossfaded(float[] buf, int n, int fade)
        {
            var o = new float[n];
            Array.Copy(buf, o, n);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                o[i] = buf[i] * Mathf.Sin(k * Mathf.PI * 0.5f) + buf[n + i] * Mathf.Cos(k * Mathf.PI * 0.5f);
            }
            return o;
        }

        /// <summary>Leiser Abendwind: gefiltertes Rauschen, langsame Böen (nahtlos loopbar).</summary>
        public static AudioClip Wind(float seconds = 10f)
        {
            int n = (int)(seconds * HiRate), fade = HiRate;
            var b = new float[n + fade];
            float lp1 = 0, lp2 = 0, lp3 = 0;
            for (int i = 0; i < b.Length; i++)
            {
                float w = F() * 2f - 1f;
                lp1 += (w - lp1) * 0.045f; lp2 += (lp1 - lp2) * 0.06f; lp3 += (lp2 - lp3) * 0.004f;
                float band = (lp2 - lp3) * 4.2f;
                float gust = 0.55f + 0.30f * Mathf.Sin(i / (float)HiRate * Mathf.PI * 2f * 0.17f) + 0.15f * Mathf.Sin(i / (float)HiRate * Mathf.PI * 2f * 0.43f + 1.3f);
                b[i] = band * gust;
            }
            return Make("SW_Wind", Crossfaded(b, n, fade), HiRate);
        }

        /// <summary>Ferne Waldgrillen/Nachtinsekten, sehr leise.</summary>
        public static AudioClip Crickets(float seconds = 8f)
        {
            int n = (int)(seconds * HiRate);
            var d = new float[n];
            for (int g = 0; g < 9; g++)
            {
                float start = 0.2f + F() * (seconds - 1.6f), f = 3600f + F() * 900f, len = 0.5f + F() * 0.6f, rate = 14f + F() * 8f;
                for (int i = (int)(start * HiRate); i < Math.Min(n, (int)((start + len) * HiRate)); i++)
                {
                    float t = i / (float)HiRate - start;
                    float pulse = Mathf.Max(0f, Mathf.Sin(t * rate * Mathf.PI * 2f));
                    float env = Mathf.Sin(Mathf.Clamp01(t / len) * Mathf.PI);
                    d[i] += Mathf.Sin(t * f * Mathf.PI * 2f) * pulse * env * 0.10f;
                }
            }
            return Make("SW_Crickets", d, HiRate);
        }

        /// <summary>Warme Klangfläche: Sinus-Akkord mit sanftem Schwellen. Frequenzen sind ganzzahlige Zyklen pro Loop → nahtlos.</summary>
        public static AudioClip Pad(float[] chordHz, float seconds = 16f, string name = "SW_Pad")
        {
            int n = (int)(seconds * LoRate);
            var d = new float[n];
            for (int k = 0; k < chordHz.Length; k++)
            {
                float f = Mathf.Round(chordHz[k] * seconds) / seconds;
                float swells = 1 + (k % 2), phase = F() * 6.28f;
                float detune = 1f / seconds;
                for (int i = 0; i < n; i++)
                {
                    float t = i / (float)LoRate;
                    float swell = 0.65f + 0.35f * Mathf.Sin(t / seconds * Mathf.PI * 2f * swells + phase);
                    float a = Mathf.Sin(t * f * Mathf.PI * 2f + phase) + 0.5f * Mathf.Sin(t * (f + detune) * Mathf.PI * 2f)
                              + 0.22f * Mathf.Sin(t * f * 2f * Mathf.PI * 2f) + 0.08f * Mathf.Sin(t * f * 3f * Mathf.PI * 2f);
                    d[i] += a * swell * 0.11f;
                }
            }
            return Make(name, d, LoRate);
        }

        /// <summary>Weiches Klavier-/Glockenspiel-Plink für die Einschlafmelodie.</summary>
        public static AudioClip Note(float hz, float seconds = 3.2f)
        {
            int n = (int)(seconds * LoRate);
            var d = new float[n];
            float[] amp = { 1f, 0.38f, 0.16f, 0.07f }; float[] mul = { 1f, 2f, 3f, 4.02f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)LoRate, v = 0f;
                for (int h = 0; h < 4; h++) v += amp[h] * Mathf.Sin(t * hz * mul[h] * Mathf.PI * 2f) * Mathf.Exp(-t * (1.1f + h * 1.4f));
                float attack = Mathf.Clamp01(t / 0.012f);
                d[i] = v * attack * 0.42f;
            }
            return Make("SW_Note_" + (int)hz, d, LoRate);
        }

        public static AudioClip Sfx(string key)
        {
            switch (key)
            {
                case "chirp": return Chirp();
                case "squeak": return Glide("SW_Squeak", 700f, 1150f, 0.20f, 0.30f, true);
                case "yawn": return Yawn();
                case "snore": return Snore();
                case "tap": return Tap();
                case "rustle": return Rustle();
                case "twinkle": return Twinkle();
                default: return Glide("SW_Sfx_" + key, 600f, 600f, 0.15f, 0.2f, false);
            }
        }

        static AudioClip Glide(string name, float f0, float f1, float seconds, float amp, bool vibrato)
        {
            int n = (int)(seconds * HiRate); var d = new float[n]; float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n, f = Mathf.Lerp(f0, f1, k) * (vibrato ? 1f + 0.03f * Mathf.Sin(k * 40f) : 1f);
                ph += f / HiRate * Mathf.PI * 2f;
                d[i] = Mathf.Sin(ph) * Mathf.Sin(k * Mathf.PI) * amp;
            }
            return Make(name, d, HiRate);
        }

        static AudioClip Chirp()
        {
            int n = (int)(0.5f * HiRate); var d = new float[n];
            for (int c = 0; c < 3; c++)
            {
                int s = (int)(c * 0.15f * HiRate), len = (int)(0.09f * HiRate); float ph = 0;
                for (int i = 0; i < len && s + i < n; i++)
                {
                    float k = i / (float)len; ph += Mathf.Lerp(2300f, 3100f + c * 200f, k) / HiRate * Mathf.PI * 2f;
                    d[s + i] += Mathf.Sin(ph) * Mathf.Sin(k * Mathf.PI) * 0.22f;
                }
            }
            return Make("SW_Chirp", d, HiRate);
        }

        static AudioClip Yawn()
        {
            int n = (int)(1.1f * HiRate); var d = new float[n]; float ph = 0, lp = 0;
            for (int i = 0; i < n; i++)
            {
                float k = i / (float)n; ph += Mathf.Lerp(430f, 210f, k) / HiRate * Mathf.PI * 2f;
                lp += ((F() * 2f - 1f) - lp) * 0.1f;
                d[i] = (Mathf.Sin(ph) * 0.35f + lp * 0.5f) * Mathf.Sin(Mathf.Pow(k, 0.7f) * Mathf.PI) * 0.35f;
            }
            return Make("SW_Yawn", d, HiRate);
        }

        static AudioClip Snore()
        {
            int n = (int)(3.4f * HiRate); var d = new float[n]; float lp = 0;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)HiRate; float breath = Mathf.Max(0f, Mathf.Sin(t / 3.4f * Mathf.PI * 2f * 1.5f - 0.3f));
                lp += ((F() * 2f - 1f) - lp) * 0.05f;
                float tone = Mathf.Sin(t * 92f * Mathf.PI * 2f) + 0.5f * Mathf.Sin(t * 184f * Mathf.PI * 2f);
                d[i] = (tone * 0.18f + lp * 0.35f) * breath * breath * 0.55f;
            }
            return Make("SW_Snore", d, HiRate);
        }

        static AudioClip Tap()
        {
            int n = (int)(0.25f * HiRate); var d = new float[n]; float ph = 0, lp = 0;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)HiRate; ph += Mathf.Lerp(130f, 55f, Mathf.Clamp01(t / 0.12f)) / HiRate * Mathf.PI * 2f;
                lp += ((F() * 2f - 1f) - lp) * 0.2f;
                d[i] = (Mathf.Sin(ph) * 0.5f + lp * 0.25f * Mathf.Exp(-t * 40f)) * Mathf.Exp(-t * 18f);
            }
            return Make("SW_Tap", d, HiRate);
        }

        static AudioClip Rustle()
        {
            int n = (int)(0.9f * HiRate); var d = new float[n]; float lp = 0, lp2 = 0;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)HiRate; lp += ((F() * 2f - 1f) - lp) * 0.35f; lp2 += (lp - lp2) * 0.05f;
                float env = Mathf.Sin(Mathf.Clamp01(t / 0.9f) * Mathf.PI) * (0.6f + 0.4f * Mathf.Sin(t * 37f));
                d[i] = (lp - lp2) * env * 0.35f;
            }
            return Make("SW_Rustle", d, HiRate);
        }

        static AudioClip Twinkle()
        {
            int n = (int)(1.0f * HiRate); var d = new float[n];
            float[] f = { 1568f, 2093f }; float[] st = { 0f, 0.16f };
            for (int k = 0; k < 2; k++)
                for (int i = (int)(st[k] * HiRate); i < n; i++)
                {
                    float t = i / (float)HiRate - st[k];
                    d[i] += (Mathf.Sin(t * f[k] * Mathf.PI * 2f) + 0.25f * Mathf.Sin(t * f[k] * 2f * Mathf.PI * 2f)) * Mathf.Exp(-t * 6f) * Mathf.Clamp01(t / 0.01f) * 0.16f;
                }
            return Make("SW_Twinkle", d, HiRate);
        }
    }
}
