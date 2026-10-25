using System;
using System.Collections;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Verbindet Konfiguration (AnimalConfig), 3D-Modell (AnimalRig) und Animation (AnimalPerformance).
    /// Der SleepForestController liest <see cref="Sleepiness"/>; die Story löst Cues aus.
    /// </summary>
    public class AnimalController : MonoBehaviour
    {
        public AnimalConfig Config { get; private set; }
        public AnimalRig Rig { get; private set; }
        public AnimalPerformance Performance { get; private set; }
        public bool IsAsleep { get { return Performance != null && Performance.IsAsleep; } }
        public float Sleepiness { get { return Performance != null ? Performance.Sleepiness : 0f; } }
        public string Id { get { return Config.id; } }
        public Vector3 SleepSpotPosition { get; private set; }
        public event Action<AnimalController> FellAsleep;

        /// <summary>Erzeugt das Tier, falls ein 3D-Charakter dafür umgesetzt ist (AnimalConfig.implemented).</summary>
        public static AnimalController Create(AnimalConfig config, Transform parent, AppServices services)
        {
            if (!config.implemented) return null;
            AnimalRig rig; AnimalPerformance perf;
            var go = new GameObject("Animal_" + config.id);
            go.transform.SetParent(parent, false);
            switch (config.species)
            {
                case "snail":
                    rig = SnailBuilder.Build(go.transform);
                    // das Rig-Wurzelobjekt wird zum Tier selbst: Teile unter "go" einhängen
                    rig.transform.SetParent(go.transform, false);
                    perf = go.AddComponent<SnailPerformance>();
                    break;
                default:
                    UnityEngine.Object.Destroy(go);
                    Debug.LogWarning("Keine 3D-Umsetzung für Spezies '" + config.species + "' (Phase 3).");
                    return null;
            }
            var ctl = go.AddComponent<AnimalController>();
            ctl.Config = config; ctl.Rig = rig; ctl.Performance = perf;
            var s = config.sleepSpot.position;
            ctl.SleepSpotPosition = new Vector3(s.x, ForestTerrain.Height(s.x, s.z), s.z);
            return ctl;
        }

        /// <summary>Platziert das Tier am Startpunkt seines ersten Schritts (Suri startet auf dem ersten Stein).</summary>
        public void PlaceAtStart(AppServices services)
        {
            Vector3 p = SleepSpotPosition;
            if (Config.steps.Count > 0 && Config.steps[0].type == "FollowPath" && Config.steps[0].points.Length > 0)
            {
                var q = Config.steps[0].points[0];
                p = new Vector3(q.x, ForestTerrain.Height(q.x, q.z), q.z);
            }
            transform.position = p;
            // Blick Richtung zweitem Pfadpunkt
            if (Config.steps.Count > 0 && Config.steps[0].points != null && Config.steps[0].points.Length > 1)
            {
                var a = Config.steps[0].points[0]; var b = Config.steps[0].points[1];
                transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(b.x - a.x, b.z - a.z) * Mathf.Rad2Deg, 0f);
            }
            Performance.Init(Rig, Config, services);
        }

        public void PlaceAsleep(AppServices services)
        {
            transform.position = SleepSpotPosition;
            Performance.Init(Rig, Config, services);
            Performance.SnapToAsleep();
            transform.position = SleepSpotPosition;
        }

        public void Cue(string cue) { if (!string.IsNullOrEmpty(cue) && Performance != null) Performance.Cue(cue); }

        /// <summary>Wartet, bis das Tier nach der Erzählung wirklich ruhig schläft.</summary>
        public IEnumerator CompleteSleep()
        {
            yield return Performance.FinishSleep();
            if (FellAsleep != null) FellAsleep(this);
        }
    }
}
