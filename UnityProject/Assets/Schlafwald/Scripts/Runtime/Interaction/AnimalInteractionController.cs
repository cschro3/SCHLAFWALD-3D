using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Führt die Interaktionsschritte eines Tiers aus und kümmert sich um die Kindersicherheit des Ablaufs:
    /// freundliche Hinweise des Erzählers, nach angemessener Pause sanfte Selbsthilfe, nie Punkte, Zeitlimits oder Niederlagen.
    /// </summary>
    public class AnimalInteractionController : MonoBehaviour
    {
        AppServices s;
        Transform propsRoot;
        float lastSparkle;
        public InteractionStep Current { get; private set; }
        public int CurrentStepIndex { get; private set; } = -1;
        public int HintsGiven { get; private set; }
        public int AssistsGiven { get; private set; }

        public IEnumerable<Vector3> CurrentTargets { get { return Current != null ? Current.ActiveTargets() : new Vector3[0]; } }

        public void Init(AppServices services)
        {
            s = services;
            propsRoot = new GameObject("RitualProps").transform;
            propsRoot.SetParent(transform, false);
        }

        InteractionStep Create(string type)
        {
            switch (type)
            {
                case "FollowPath": return new FollowPathStep();
                case "DragToTarget": return new DragToTargetStep();
                case "Touch": return new TouchStep();
                case "Stroke": return new StrokeStep();
                default: throw new System.ArgumentException("Unbekannter Schritt-Typ " + type);
            }
        }

        public IEnumerator RunStep(AnimalController animal, int index)
        {
            var cfg = animal.Config.steps[index];
            var step = Create(cfg.type);
            Current = step; CurrentStepIndex = index;
            var ctx = new StepContext { S = s, Cam = s.Camera, Animal = animal, Index = index, Config = cfg, Root = propsRoot };
            step.Begin(ctx);
            animal.Performance.OnStepBegin(index, cfg);

            bool audioOnly = s.Settings.Current.audioOnlyMode;
            var esc = new HelpEscalator(cfg.helpAfter, cfg.assistAfter, audioOnly ? 6f : 1f);

            // Aufforderung durch den Erzähler (parallel – das Kind darf sofort loslegen)
            StartCoroutine(s.Narration.SayLines(animal.Config.GetStepLines("step", index)));

            while (!step.Completed)
            {
                if (PauseController.IsPaused) { yield return null; continue; }
                float dt = Time.deltaTime;
                step.Tick(dt);
                bool progressed = step.ConsumeProgress();
                SparkleOnTouch(animal);
                // Solange der Erzähler spricht, läuft keine Wartezeit – wir drängen nie.
                var level = esc.Tick(dt, progressed || s.Narration.IsBusy);
                if (level == HelpLevel.Hint)
                {
                    HintsGiven++;
                    StartCoroutine(HintRoutine(step, animal.Config.GetStepLines("hint", index)));
                }
                else if (level == HelpLevel.Assist)
                {
                    AssistsGiven++;
                    step.SetEmphasis(0.8f);
                    StartCoroutine(s.Narration.SayLines(animal.Config.GetStepLines("assist", index)));
                    step.BeginAssist();
                }
                yield return null;
            }

            yield return step.Finish();
            step.Cleanup();
            Current = null;
            animal.Performance.OnStepComplete(index, cfg);
            var success = animal.Config.GetLines("success" + index);
            if (success.Count > 0) yield return s.Narration.SayLines(success);
        }

        IEnumerator HintRoutine(InteractionStep step, List<NarrationLine> lines)
        {
            step.SetEmphasis(1.2f);
            yield return s.Narration.SayLines(lines);
            step.SetEmphasis(0.3f);
        }

        void SparkleOnTouch(AnimalController animal)
        {
            if (s.Settings.Current.audioOnlyMode || !PointerInput.Down) return;
            if (!PointerInput.Pressed && Time.time - lastSparkle < 0.18f) return;
            lastSparkle = Time.time;
            Ray ray = s.Camera.ScreenPointToRay(PointerInput.Position);
            float dist = Vector3.Distance(s.Camera.transform.position, animal.Performance.FocusPoint);
            s.Props.Sparkle(ray.GetPoint(dist));
        }

        /// <summary>Fortsetzen: die Hinterlassenschaften abgeschlossener Schritte (z. B. Blätterdach) wieder hinstellen.</summary>
        public void SpawnCompleted(AnimalController animal)
        {
            foreach (var st in animal.Config.steps)
                if (st.type == "DragToTarget") DragToTargetStep.SpawnCompleted(s, st, propsRoot);
        }
    }
}
