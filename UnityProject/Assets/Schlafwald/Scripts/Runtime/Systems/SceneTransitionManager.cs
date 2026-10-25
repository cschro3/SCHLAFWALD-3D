using System.Collections;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Weiche Übergänge wie das Umblättern eines lebendigen Märchenbuchs: sanftes Aus-/Einblenden und eine weiche Randabdunklung
    /// beim Kapitelwechsel. Zuständig auch für das späte Ausblenden des Bildschirms (Elternoption).
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        AppServices s;
        public float FadeAlpha { get; private set; } = 1f;      // 1 = schwarz (Start)
        public float Vignette { get; private set; }
        public float DimBrightness { get; private set; } = 1f;
        public float EndedAt = -1f;

        public void Init(AppServices services) { s = services; }

        public IEnumerator FadeTo(float alpha, float seconds)
        {
            float from = FadeAlpha, t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, seconds)); FadeAlpha = Mathf.Lerp(from, alpha, k * k * (3f - 2f * k));
                yield return null;
            }
            FadeAlpha = alpha;
        }

        public void SetFade(float a) { FadeAlpha = a; }

        /// <summary>Kapitelwechsel: Ränder dunkeln kurz weich ab und wieder auf.</summary>
        public IEnumerator ChapterTurn(float seconds = 2.6f)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                Vignette = Mathf.Sin(Mathf.Clamp01(t / seconds) * Mathf.PI) * 0.55f;
                yield return null;
            }
            Vignette = 0f;
        }

        void Update()
        {
            if (EndedAt >= 0f && s != null)
            {
                float since = Time.time - EndedAt;
                DimBrightness = ScreenDim.Brightness(since, s.Settings.Current.dimAfterMinutes, 60f);
                if (DimBrightness <= 0.001f && Screen.sleepTimeout != SleepTimeout.SystemSetting)
                {
                    Screen.sleepTimeout = SleepTimeout.SystemSetting; // Display darf jetzt von selbst ausgehen
                    Application.targetFrameRate = 10;
                }
            }
        }
    }
}
