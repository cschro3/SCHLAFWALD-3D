using UnityEngine;

namespace Schlafwald
{
    /// <summary>Ein weich animierter Wert (Smoothstep zwischen Start und Ziel). Grundlage aller Tier-Animationen ohne Animator-Controller.</summary>
    public class Channel
    {
        float start, target, duration, time;
        public float Value { get; private set; }

        public Channel(float initial = 0f) { start = target = Value = initial; time = duration = 0f; }

        public void Go(float to, float seconds)
        {
            start = Value; target = to; duration = Mathf.Max(0.0001f, seconds); time = 0f;
        }

        public void Set(float v) { start = target = Value = v; time = duration; }
        public bool Done { get { return time >= duration; } }
        public float Target { get { return target; } }

        public void Tick(float dt)
        {
            if (time >= duration) { Value = target; return; }
            time += dt;
            float k = Mathf.Clamp01(time / duration);
            Value = Mathf.Lerp(start, target, k * k * (3f - 2f * k));
        }
    }
}
