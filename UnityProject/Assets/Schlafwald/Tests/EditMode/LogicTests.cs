using System;
using System.Collections.Generic;
using NUnit.Framework;
using Schlafwald.Core;

namespace Schlafwald.Tests
{
    public class HelpEscalatorTests
    {
        [Test]
        public void NothingHappensWhileChildMakesProgress()
        {
            var h = new HelpEscalator(10, 30);
            for (int i = 0; i < 1000; i++) Assert.AreEqual(HelpLevel.None, h.Tick(0.1f, true));
            Assert.IsFalse(h.AssistTriggered);
        }

        [Test]
        public void HintsThenAssistExactlyOnce()
        {
            var h = new HelpEscalator(10, 30);
            int hints = 0, assists = 0;
            for (int i = 0; i < 700; i++)
            {
                var l = h.Tick(0.1f, false);
                if (l == HelpLevel.Hint) hints++;
                if (l == HelpLevel.Assist) assists++;
            }
            Assert.AreEqual(2, hints, "Hinweise bei 10 s und 20 s");
            Assert.AreEqual(1, assists);
        }

        [Test]
        public void ProgressResetsTheHintTimer()
        {
            var h = new HelpEscalator(10, 30);
            for (int i = 0; i < 90; i++) h.Tick(0.1f, false);
            h.Tick(0.1f, true);
            for (int i = 0; i < 90; i++) Assert.AreEqual(HelpLevel.None, h.Tick(0.1f, false));
        }

        [Test]
        public void AudioOnlyTimeScaleShortensWaits()
        {
            var h = new HelpEscalator(10, 30, 5f);
            HelpLevel last = HelpLevel.None; float t = 0;
            while (last != HelpLevel.Assist && t < 100) { last = h.Tick(0.1f, false); t += 0.1f; }
            Assert.Less(t, 7f);
        }
    }

    public class AtmosphereTests
    {
        [Test]
        public void ForestGetsMonotonicallyCalmerAndDarker()
        {
            var prev = SleepAtmosphere.Evaluate(0f);
            for (int i = 1; i <= 100; i++)
            {
                var v = SleepAtmosphere.Evaluate(i / 100f);
                Assert.LessOrEqual(v.LightMultiplier, prev.LightMultiplier + 1e-5f);
                Assert.LessOrEqual(v.Motion, prev.Motion + 1e-5f);
                Assert.LessOrEqual(v.Particles, prev.Particles + 1e-5f);
                Assert.LessOrEqual(v.AmbienceActivity, prev.AmbienceActivity + 1e-5f);
                Assert.GreaterOrEqual(v.TimeOfDay, prev.TimeOfDay - 1e-5f);
                prev = v;
            }
            var end = SleepAtmosphere.Evaluate(1f);
            Assert.Greater(end.LightMultiplier, 0.2f, "Nacht, aber nicht stockdunkel");
            Assert.AreEqual(1f, end.TimeOfDay, 1e-4f);
        }

        [Test]
        public void CalmFromSleepinessIsAverageOverAllAnimals()
        {
            Assert.AreEqual(1f / 7f, SleepAtmosphere.CalmFromSleepiness(new List<float> { 1f }, 7), 1e-5f);
            Assert.AreEqual(1f, SleepAtmosphere.CalmFromSleepiness(new List<float> { 1, 1, 1 }, 3), 1e-5f);
            Assert.AreEqual(0f, SleepAtmosphere.CalmFromSleepiness(new List<float>(), 0));
        }

        [Test]
        public void SmoothingHasNoOvershootAndIsFramerateIndependent()
        {
            float a = 0, b = 0;
            for (int i = 0; i < 60; i++) a = SleepAtmosphere.Smooth(a, 1f, 1f / 60f, 0.5f);
            for (int i = 0; i < 30; i++) b = SleepAtmosphere.Smooth(b, 1f, 1f / 30f, 0.5f);
            Assert.AreEqual(a, b, 1e-4f);
            Assert.LessOrEqual(a, 1f);
        }
    }

    public class NarrationTimingTests
    {
        [Test] public void CountsWords() { Assert.AreEqual(5, NarrationTiming.WordCount("Gute Nacht, kleine Suri. Schlaf!")); }

        [Test]
        public void EllipsisAndSlowSpeedLengthenSpeech()
        {
            float plain = NarrationTiming.EstimateSeconds("Ganz langsam");
            float pause = NarrationTiming.EstimateSeconds("Ganz langsam …");
            float slow = NarrationTiming.EstimateSeconds("Ganz langsam", 0.5f);
            Assert.Greater(pause, plain + 0.8f);
            Assert.Greater(slow, plain * 1.8f);
            Assert.GreaterOrEqual(NarrationTiming.EstimateSeconds("Ja"), 0.6f);
        }
    }

    public class PacingTests
    {
        [Test]
        public void PauseScaleStretchesForLongTargetAndIsClamped()
        {
            var s = TestSupport.LoadStory();
            var all = new List<string>(); foreach (var a in s.animals) all.Add(a.id);
            var est = PacingPlanner.EstimateStory(s, all);
            Assert.Greater(est.Total, 300f, "Gesamtgeschichte sollte mehrere Minuten dauern");
            Assert.AreEqual(1f, PacingPlanner.PauseScale(est, 0f));
            Assert.AreEqual(PacingPlanner.MaxScale, PacingPlanner.PauseScale(est, 600f));
            Assert.AreEqual(PacingPlanner.MinScale, PacingPlanner.PauseScale(est, 1f));
            float mid = PacingPlanner.PauseScale(est, est.Total / 60f * 1.2f);
            Assert.Greater(mid, 1f);
        }

        [Test]
        public void SelectingFewerAnimalsShortensTheStory()
        {
            var s = TestSupport.LoadStory();
            var all = new List<string>(); foreach (var a in s.animals) all.Add(a.id);
            Assert.Less(PacingPlanner.EstimateStory(s, new List<string> { "suri" }).Total, PacingPlanner.EstimateStory(s, all).Total);
        }
    }

    public class ParentTests
    {
        [Test]
        public void SettingsAreClampedToSafeRanges()
        {
            var p = new ParentSettings { masterVolume = 5f, targetMinutes = 1f, dimAfterMinutes = 99f, narratorId = "" };
            p.Clamp();
            Assert.AreEqual(1f, p.masterVolume);
            Assert.AreEqual(5f, p.targetMinutes);
            Assert.AreEqual(30f, p.dimAfterMinutes);
            Assert.AreEqual("warm", p.narratorId);
            Assert.IsTrue(p.IsAnimalEnabled("suri"));
            p.enabledAnimals.Add("fino");
            Assert.IsFalse(p.IsAnimalEnabled("suri"));
        }

        [Test]
        public void ResumeOnlyForUnfinishedRecentNight()
        {
            long now = DateTime.UtcNow.Ticks;
            var p = new ProgressData { lastPlayedUtcTicks = now - TimeSpan.FromMinutes(30).Ticks };
            Assert.IsFalse(ProgressRules.ShouldResume(p, now), "noch kein Tier schläft");
            p.sleepingAnimals.Add("fino");
            Assert.IsTrue(ProgressRules.ShouldResume(p, now));
            p.lastPlayedUtcTicks = now - TimeSpan.FromHours(5).Ticks;
            Assert.IsFalse(ProgressRules.ShouldResume(p, now));
            p.lastPlayedUtcTicks = now; p.finished = true;
            Assert.IsFalse(ProgressRules.ShouldResume(p, now));
        }

        [Test]
        public void ScreenDimsSmoothlyOnlyAfterDelay()
        {
            Assert.AreEqual(1f, ScreenDim.Brightness(10000f, 0f));
            Assert.AreEqual(1f, ScreenDim.Brightness(179f, 3f));
            float mid = ScreenDim.Brightness(180f + 30f, 3f, 60f);
            Assert.That(mid, Is.InRange(0.4f, 0.6f));
            Assert.AreEqual(0f, ScreenDim.Brightness(180f + 61f, 3f, 60f));
        }

        [Test]
        public void MathChallengeHasExactlyOneCorrectAnswer()
        {
            var rng = new Random(42);
            for (int i = 0; i < 200; i++)
            {
                var c = ParentGate.Create(rng);
                int right = 0;
                for (int k = 0; k < 3; k++) if (ParentGate.Check(c, k)) right++;
                Assert.AreEqual(1, right);
                var set = new HashSet<int>(c.Answers);
                Assert.AreEqual(3, set.Count, "Antworten müssen verschieden sein: " + c.Question);
            }
        }

        [Test]
        public void HoldGateNeedsContinuousHold()
        {
            var g = new HoldGate(3f);
            Assert.IsFalse(g.Tick(2f, true));
            Assert.IsFalse(g.Tick(0.5f, false));
            Assert.IsFalse(g.Tick(2.9f, true));
            Assert.IsTrue(g.Tick(0.2f, true));
        }

        [Test]
        public void PortraitWidensVerticalFovLandscapeKeepsIt()
        {
            Assert.AreEqual(50f, AspectFov.VerticalFov(50f, 16f / 9f), 1e-4f);
            Assert.AreEqual(50f, AspectFov.VerticalFov(50f, 2.1f), 1e-4f);
            Assert.Greater(AspectFov.VerticalFov(50f, 9f / 16f), 100f);
            Assert.Greater(AspectFov.VerticalFov(50f, 4f / 3f), 50f);
        }
    }

    public class LineFilterTests
    {
        [Test]
        public void IntroSpeaksSingularWhenOnlyOneAnimalParticipates()
        {
            var s = TestSupport.LoadStory();
            var intro = s.FindChapter("intro");
            var one = LineFilter.Filter(intro.lines, new List<string> { "suri" }, 7);
            var all = new List<string>(); foreach (var a in s.animals) all.Add(a.id);
            var seven = LineFilter.Filter(intro.lines, all, 7);
            Assert.That(one.Exists(l => l.id == "ch1_05b"));
            Assert.That(!one.Exists(l => l.id == "ch1_05"));
            Assert.That(seven.Exists(l => l.id == "ch1_05"));
            Assert.That(!seven.Exists(l => l.id == "ch1_05b"));
        }

        [Test]
        public void FinaleOnlyMentionsParticipatingAnimals()
        {
            var s = TestSupport.LoadStory();
            var fin = LineFilter.Filter(s.FindChapter("finale").lines, new List<string> { "suri", "fino" }, 7);
            Assert.That(fin.Exists(l => l.animalId == "fino")); Assert.That(fin.Exists(l => l.animalId == "suri"));
            Assert.That(!fin.Exists(l => l.animalId == "mila"));
            Assert.AreEqual("Schlaf schön.", fin[fin.Count - 1].text);
        }
    }
}
