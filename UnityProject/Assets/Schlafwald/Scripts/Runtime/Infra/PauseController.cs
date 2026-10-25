using System.Collections.Generic;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>
    /// Zentrale Pause: Eltern-Pause, Elternbereich geöffnet, App im Hintergrund. Solange irgendein Grund aktiv ist,
    /// steht die Zeit (und mit ihr Animationen, Kamera, Erzähler-Timer) und alles Audio.
    /// </summary>
    public static class PauseController
    {
        static readonly HashSet<string> reasons = new HashSet<string>();
        public static float BaseTimeScale = 1f;
        public static bool IsPaused { get { return reasons.Count > 0; } }
        public static bool Has(string reason) { return reasons.Contains(reason); }

        public static void Request(string reason) { reasons.Add(reason); Apply(); }
        public static void Release(string reason) { reasons.Remove(reason); Apply(); }
        public static void Clear() { reasons.Clear(); Apply(); }

        static void Apply()
        {
            Time.timeScale = reasons.Count > 0 ? 0f : BaseTimeScale;
            AudioListener.pause = reasons.Count > 0;
        }
    }
}
