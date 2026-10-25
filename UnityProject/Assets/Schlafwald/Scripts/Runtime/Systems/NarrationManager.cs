using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Erzähler: spielt vertonte Dateien aus Resources/Audio/Narration/{narratorId}/{audioKey}.
    /// Fehlt eine Aufnahme, läuft der EINDEUTIG GEKENNZEICHNETE Entwicklungs-Fallback (<see cref="FallbackVoice"/>:
    /// wortloses sanftes Murmeln im Sprechrhythmus + Untertitel für Vorlesen). Cues synchronisieren Animationen mit der Zeile.
    /// </summary>
    public class NarrationManager : MonoBehaviour
    {
        AppServices s;
        AudioSource voice;
        int active;
        public bool SilentMode;                       // Tests: keine Audioausgabe, Dauer nur geschätzt
        public float PauseScale = 1f;                 // aus PacingPlanner (Wunschdauer der Eltern)
        public bool IsBusy { get { return active > 0; } }
        public string Subtitle { get; private set; }
        /// <summary>True, solange die aktuelle Zeile über den Fallback statt über eine Aufnahme läuft.</summary>
        public bool UsingFallback { get; private set; }
        public int LinesSpoken { get; private set; }
        public event Action<NarrationLine> LineStarted;
        public event Action<NarrationLine> LineFinished;
        public event Action<string> CueRequested;

        public void Init(AppServices services)
        {
            s = services;
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false; voice.spatialBlend = 0f;
        }

        string NarratorFor(NarrationLine l) { return string.IsNullOrEmpty(l.narrator) ? s.Settings.Current.narratorId : l.narrator; }

        public static string ResourcePath(string narrator, NarrationLine l) { return "Audio/Narration/" + narrator + "/" + l.AudioKeyOrId; }

        /// <summary>Wie viele Zeilen haben eine echte Aufnahme? Für Elternbereich/Log.</summary>
        public static void Coverage(StoryData story, string narrator, out int have, out int total)
        {
            int h = 0, t = 0;
            Action<NarrationLine> count = l => { t++; if (Resources.Load<AudioClip>(ResourcePath(narrator, l)) != null) h++; };
            foreach (var c in story.chapters) foreach (var l in c.lines) count(l);
            foreach (var an in story.animals) foreach (var l in an.lines) count(l);
            have = h; total = t;
        }

        public IEnumerator SayLines(IEnumerable<NarrationLine> lines)
        {
            active++;
            try
            {
                foreach (var l in lines) yield return Say(l);
            }
            finally { active--; }
        }

        public IEnumerator Say(NarrationLine line)
        {
            active++;
            if (LineStarted != null) LineStarted(line);
            if (!string.IsNullOrEmpty(line.cue)) StartCoroutine(CueAfter(line.cue, line.cueDelay));

            string narrator = NarratorFor(line);
            AudioClip clip = SilentMode ? null : Resources.Load<AudioClip>(ResourcePath(narrator, line));
            float duration;
            bool fallback = false;
            if (clip != null)
            {
                duration = clip.length;
                voice.clip = clip; voice.volume = Mathf.Clamp01(s.Settings.Current.narrationVolume * s.Settings.Current.masterVolume);
                voice.Play();
                Subtitle = s.Settings.Current.showSubtitles ? line.text : null;
            }
            else
            {
                fallback = true;
                duration = NarrationTiming.EstimateSeconds(line.text);
                if (!SilentMode)
                {
                    var fb = FallbackVoice.Create(line.text, line.tone, duration);
                    duration = fb.length;
                    voice.clip = fb;
                    voice.volume = Mathf.Clamp01(0.55f * s.Settings.Current.narrationVolume * s.Settings.Current.masterVolume);
                    voice.Play();
                    if (!warned) { warned = true; Debug.LogWarning("[SCHLAFWALD] Keine Sprachaufnahmen gefunden – ENTWICKLUNGS-FALLBACK aktiv (Murmeln + Untertitel). Siehe docs/AUDIO_PIPELINE.md."); }
                }
                Subtitle = line.text;
            }
            UsingFallback = fallback;

            float t = 0f;
            while (t < duration) { t += Time.deltaTime; yield return null; }

            Subtitle = null; UsingFallback = false; LinesSpoken++;
            if (LineFinished != null) LineFinished(line);
            float pause = line.pauseAfter * PauseScale;
            if (pause > 0f) yield return new WaitForSeconds(pause);
            active--;
        }
        static bool warned;

        IEnumerator CueAfter(string cue, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (CueRequested != null) CueRequested(cue);
        }

        public void StopVoice() { if (voice != null) voice.Stop(); }
    }

    /// <summary>
    /// ENTWICKLUNGS-FALLBACK für fehlende Sprachaufnahmen: wortlose Silben im Sprechrhythmus (sanfte Stimme, fallende Satzmelodie).
    /// Keine Sprachsynthese, KEIN Ersatz für die finale Sprecher-Aufnahme. Parents lesen mit den Untertiteln vor.
    /// </summary>
    public static class FallbackVoice
    {
        const int Rate = 16000;

        public static AudioClip Create(string text, string tone, float targetSeconds)
        {
            var words = text.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int n = Mathf.Max(Rate / 2, (int)(targetSeconds * Rate));
            var d = new float[n];
            int totalLetters = 0; foreach (var w in words) totalLetters += Mathf.Max(2, w.Length);
            float speech = Mathf.Max(0.3f, targetSeconds - PausePart(text));
            float cursor = 0.02f;
            float baseHz = tone == "whisper" ? 0f : (tone == "soft" ? 150f : 165f);
            var rnd = new System.Random(text.GetHashCode());
            for (int wi = 0; wi < words.Length; wi++)
            {
                string w = words[wi];
                float len = speech * Mathf.Max(2, w.Length) / totalLetters;
                float contour = 1f - 0.18f * (wi / (float)Mathf.Max(1, words.Length - 1));
                int syl = Mathf.Max(1, Mathf.RoundToInt(w.Length / 3f));
                for (int sy = 0; sy < syl; sy++)
                {
                    float sLen = len / syl * 0.85f, start = cursor + sy * len / syl;
                    float f = baseHz * contour * (1f + 0.10f * ((float)rnd.NextDouble() - 0.4f));
                    int i0 = (int)(start * Rate), i1 = Mathf.Min(n, (int)((start + sLen) * Rate));
                    float lp = 0f;
                    for (int i = i0; i < i1; i++)
                    {
                        float t = (i - i0) / (float)Rate, k = (i - i0) / (float)Mathf.Max(1, i1 - i0);
                        float env = Mathf.Sin(k * Mathf.PI); env *= env;
                        float v;
                        if (baseHz <= 0f) { lp += (((float)rnd.NextDouble() * 2f - 1f) - lp) * 0.25f; v = lp * 0.6f; }
                        else
                        {
                            v = 0f;
                            for (int h = 1; h <= 5; h++) v += Mathf.Sin(t * f * h * Mathf.PI * 2f + h) / (h * 1.4f);
                            v *= 0.55f;
                        }
                        d[i] += v * env * 0.45f;
                    }
                }
                cursor += len;
                char last = w[w.Length - 1];
                if (last == '…' || w.EndsWith("...")) cursor += 0.9f; else if (last == ',') cursor += 0.2f; else if (last == '.' || last == '!' || last == '?') cursor += 0.45f;
            }
            var clip = AudioClip.Create("SW_FallbackVoice", n, 1, Rate, false);
            clip.SetData(d, 0);
            return clip;
        }

        static float PausePart(string text)
        {
            float t = 0f;
            foreach (char c in text) { if (c == '…') t += 0.9f; else if (c == ',') t += 0.2f; else if (c == '.' || c == '!' || c == '?') t += 0.45f; }
            return Mathf.Min(t, 6f);
        }
    }
}
