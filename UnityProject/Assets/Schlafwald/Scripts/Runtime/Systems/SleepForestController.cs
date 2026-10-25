using System;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    public enum StoryPhase { Booting, Intro, Animal, Finale, Ended }

    /// <summary>
    /// Globale Wahrheit des Waldes: Schlafstatus aller Tiere, Storyphase, Erzählfortschritt – und daraus die weich geglättete
    /// Atmosphäre (Licht, Bewegung, Partikel, Geräusche, Musik), die alle Teilsysteme jedes Frame erhalten.
    /// </summary>
    public class SleepForestController : MonoBehaviour
    {
        AppServices s;
        readonly List<AnimalController> animals = new List<AnimalController>();
        int total;
        float calmSmoothed;
        public StoryPhase Phase { get; private set; }
        public string CurrentChapterId { get; private set; }
        public AtmosphereValues Current { get; private set; }
        public int AnimalsAsleep { get { int n = 0; foreach (var a in animals) if (a.IsAsleep) n++; return n; } }
        public int TotalAnimals { get { return total; } }
        public float TargetCalm { get; private set; }
        public event Action<StoryPhase> PhaseChanged;
        public event Action<AnimalController> AnimalAsleep;

        public void Init(AppServices services, int participatingAnimals)
        {
            s = services; total = Mathf.Max(1, participatingAnimals);
            Current = SleepAtmosphere.Evaluate(0f);
        }

        public void Register(AnimalController a)
        {
            animals.Add(a);
            a.FellAsleep += c => { if (AnimalAsleep != null) AnimalAsleep(c); };
        }

        public void SetPhase(StoryPhase p, string chapterId = "")
        {
            Phase = p; CurrentChapterId = chapterId;
            if (PhaseChanged != null) PhaseChanged(p);
        }

        /// <summary>Tiere, die in dieser Nacht nicht mitspielen, zählen als Fortschritt der Atmosphäre nicht mit (total = Teilnehmer).</summary>
        void Update()
        {
            var sleepiness = new List<float>(animals.Count);
            foreach (var a in animals) sleepiness.Add(a.Sleepiness);
            TargetCalm = SleepAtmosphere.CalmFromSleepiness(sleepiness, total);
            if (Phase == StoryPhase.Ended) TargetCalm = 1f;
            // sehr weiche Glättung: Veränderungen dauern Sekunden, nie abrupt
            calmSmoothed = SleepAtmosphere.Smooth(calmSmoothed, TargetCalm, Time.deltaTime, 2.5f);
            Current = SleepAtmosphere.Evaluate(calmSmoothed);

            if (s.Lighting != null) s.Lighting.Apply(Current);
            if (s.Audio != null) s.Audio.Apply(Current);
            if (s.Particles != null) s.Particles.Apply(Current);
            Shader.SetGlobalFloat("_SleepMotion", Current.Motion);
        }
    }
}
