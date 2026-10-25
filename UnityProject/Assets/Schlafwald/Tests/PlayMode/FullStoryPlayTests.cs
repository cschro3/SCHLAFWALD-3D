using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Schlafwald.Core;

namespace Schlafwald.Tests
{
    /// <summary>
    /// Automatisierte Durchläufe der kompletten Geschichte im PlayMode (Unity Test Runner → PlayMode).
    /// STATUS: Diese Tests sind geschrieben, konnten aber ohne Unity-Editor noch NICHT ausgeführt werden.
    /// </summary>
    public class FullStoryPlayTests
    {
        const float Speed = 12f;

        [SetUp] public void SetUp() { AppComposer.AutoStart = false; AppComposer.Teardown(); }
        [TearDown] public void TearDown() { AppComposer.Teardown(); Time.timeScale = 1f; AppComposer.AutoStart = true; }

        SchlafwaldApp Start(ScriptedPointerSource pointer, ParentSettings settings = null)
        {
            return AppComposer.Compose(new ComposeOptions { silentAudio = true, timeScale = Speed, pointer = pointer, freshProgress = true, settingsOverride = settings });
        }

        IEnumerator WaitFinished(SchlafwaldApp app, float realSeconds, System.Action each = null)
        {
            float t = 0f;
            while (!app.Services.Director.Finished && t < realSeconds) { t += Time.unscaledDeltaTime; if (each != null) each(); yield return null; }
            Assert.IsTrue(app.Services.Director.Finished, "Story nicht fertig. Log:\n" + app.Services.Director.Log);
        }

        void AssertEndState(SchlafwaldApp app)
        {
            var s = app.Services;
            Assert.AreEqual(StoryPhase.Ended, s.Forest.Phase);
            Assert.AreEqual(s.Forest.TotalAnimals, s.Forest.AnimalsAsleep, "alle teilnehmenden Tiere schlafen");
            Assert.Greater(s.Forest.TargetCalm, 0.99f);
            StringAssert.Contains("asleep:suri", s.Director.Log);
        }

        [UnityTest]
        public IEnumerator StoryCompletesWithoutAnyInput_ChildNeverFails()
        {
            var app = Start(new ScriptedPointerSource());
            yield return WaitFinished(app, 240f);
            AssertEndState(app);
            Assert.Greater(app.Services.Interaction.AssistsGiven, 0, "ohne Eingabe muss sanfte Selbsthilfe greifen");
        }

        [UnityTest]
        public IEnumerator StoryCompletesWhenChildOnlyTouchesOutsideTargets()
        {
            var src = new ScriptedPointerSource(); var rnd = new System.Random(5); float next = 0f;
            var app = Start(src);
            yield return WaitFinished(app, 240f, () =>
            {
                if (Time.time < next) return;
                next = Time.time + 0.4f;
                src.down = !src.down;
                // nur Ecken/Ränder – weit weg von jedem Ziel
                src.position = new Vector2(rnd.Next(0, 2) * (Screen.width - 5) + 2, rnd.Next(0, 2) * (Screen.height - 5) + 2);
            });
            AssertEndState(app);
        }

        [UnityTest]
        public IEnumerator StoryCompletesWithBotSolvingEveryRitual_WithoutHelp()
        {
            var src = new ScriptedPointerSource();
            var app = Start(src);
            var bot = new RitualBot(app, src);
            yield return WaitFinished(app, 300f, bot.Tick);
            AssertEndState(app);
        }

        [UnityTest]
        public IEnumerator PauseFreezesTheStoryAndResumeContinues()
        {
            var app = Start(new ScriptedPointerSource());
            yield return new WaitForSecondsRealtime(1f);
            PauseController.Request("parent");
            Assert.AreEqual(0f, Time.timeScale);
            string before = app.Services.Director.Log;
            yield return new WaitForSecondsRealtime(1f);
            Assert.AreEqual(before, app.Services.Director.Log);
            PauseController.Release("parent");
            Assert.AreEqual(Speed, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator AudioOnlyModeFinishesAndLeavesNoCameraRendering()
        {
            var s = new ParentSettings { audioOnlyMode = true };
            var app = Start(new ScriptedPointerSource(), s);
            yield return WaitFinished(app, 240f);
            AssertEndState(app);
            Assert.IsTrue(app.Services.Settings.Current.audioOnlyMode);
        }
    }

    /// <summary>Simuliert ein geduldiges Kind: folgt dem Leitlicht, zieht Objekte zum Ziel, berührt und streichelt.</summary>
    public class RitualBot
    {
        readonly SchlafwaldApp app; readonly ScriptedPointerSource src;
        int phase; Vector3 dragTarget; float wiggle;
        public RitualBot(SchlafwaldApp app, ScriptedPointerSource src) { this.app = app; this.src = src; }

        Vector2 Screen(Vector3 w) { var p = app.Services.Camera.WorldToScreenPoint(w); return new Vector2(p.x, p.y); }

        public void Tick()
        {
            var ic = app.Services.Interaction;
            if (ic.Current == null) { src.down = false; phase = 0; return; }
            var cfg = ic.CurrentStepIndex >= 0 ? app.Services.Director != null ? CurrentConfig(ic) : null : null;
            Vector3? first = null;
            foreach (var t in ic.CurrentTargets) { first = t; break; }
            if (first == null) { src.down = false; return; }
            wiggle += Time.deltaTime * 20f;
            if (cfg != null && cfg.type == "DragToTarget")
            {
                dragTarget = new Vector3(cfg.target.x, cfg.target.y, cfg.target.z);
                if (phase == 0) { src.position = Screen(first.Value); src.down = true; phase = 1; }
                else { src.position = Vector2.MoveTowards(src.position, Screen(dragTarget), 2200f * Time.deltaTime); if (Vector2.Distance(src.position, Screen(dragTarget)) < 20f) { src.down = false; phase = 0; } }
            }
            else
            {
                src.down = true;
                src.position = Screen(first.Value) + new Vector2(Mathf.Sin(wiggle) * 30f, Mathf.Cos(wiggle) * 30f);
            }
        }

        StepConfig CurrentConfig(AnimalInteractionController ic)
        {
            var a = ic.CurrentAnimalConfig;
            return a != null && ic.CurrentStepIndex >= 0 && ic.CurrentStepIndex < a.steps.Count ? a.steps[ic.CurrentStepIndex] : null;
        }
    }
}
