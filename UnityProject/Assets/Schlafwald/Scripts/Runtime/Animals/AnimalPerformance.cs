using System.Collections;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Tierindividuelle Animation (Idle, Reaktionen, Einschlafen). Die Story steuert nur über Cues
    /// ("retractFeelers", "yawn", …), die Erzählerzeilen auslösen – so bleiben Sprache und Bild synchron.
    /// </summary>
    public abstract class AnimalPerformance : MonoBehaviour
    {
        public AnimalRig Rig { get; private set; }
        public AnimalConfig Config { get; private set; }
        protected AppServices Services { get; private set; }

        /// <summary>0 = wach … 1 = tief schlafend (speist den SleepForestController).</summary>
        public float Sleepiness { get; protected set; }
        public bool IsAsleep { get; protected set; }

        public virtual void Init(AnimalRig rig, AnimalConfig config, AppServices services)
        {
            Rig = rig; Config = config; Services = services;
        }

        public virtual void OnStepBegin(int index, StepConfig step) { }
        public virtual void OnStepProgress(int index, StepConfig step, float progress, Vector3 worldPoint) { }
        public virtual void OnStepComplete(int index, StepConfig step) { }
        public virtual void Touched(Vector3 worldPoint, float strength) { }

        public abstract void Cue(string cue);
        /// <summary>Stellt sicher, dass das Tier nach der Erzählung wirklich ruhig schläft, und wartet darauf.</summary>
        public abstract IEnumerator FinishSleep();
        /// <summary>Sofort in Schlafpose (Fortsetzen einer begonnenen Nacht).</summary>
        public abstract void SnapToAsleep();
        /// <summary>Position, an der Interaktionen "am Tier" ansetzen (Touch/Stroke-Ziel).</summary>
        public virtual Vector3 FocusPoint { get { return transform.position + Vector3.up * 0.2f; } }
    }
}
