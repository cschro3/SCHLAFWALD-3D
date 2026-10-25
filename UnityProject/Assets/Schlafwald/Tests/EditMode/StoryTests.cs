using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Schlafwald.Core;

namespace Schlafwald.Tests
{
    public class StoryTests
    {
        [Test]
        public void ShippedStoryIsValid()
        {
            var errors = StoryValidator.Validate(TestSupport.LoadStory());
            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        [Test]
        public void SevenAnimalsInMissionOrderAndSuriIsImplemented()
        {
            var s = TestSupport.LoadStory();
            CollectionAssert.AreEqual(new[] { "fino", "mila", "piko", "luna", "nuri", "leni", "suri" },
                s.animals.OrderBy(a => a.order).Select(a => a.id).ToArray());
            Assert.IsTrue(s.FindAnimal("suri").implemented);
        }

        [Test]
        public void SuriTextMatchesMissionWording()
        {
            var suri = TestSupport.LoadStory().FindAnimal("suri");
            string all = string.Join(" ", suri.lines.Select(l => l.text));
            foreach (var needle in new[] {
                "Und wer kommt denn da so langsam angeschlichen?", "Ach, das ist Suri, unsere kleine Schnecke.",
                "Sie hat sich heute ganz viel Zeit genommen, um den Wald zu entdecken.", "Jetzt ist sie aber schrecklich müde.",
                "Schau mal, ihr Schlafplatz braucht noch ein Blätterdach.", "Kannst du ihr helfen?", "Wie gemütlich!",
                "Suri zieht ihre kleinen Fühler ein.", "Ganz langsam …", "Und noch ein kleines Stückchen …", "Gute Nacht, kleine Suri." })
                StringAssert.Contains(needle, all);
        }

        [Test]
        public void FinaleKeepsAllSevenAnimalsInOrderWhenAllParticipate()
        {
            var fin = TestSupport.LoadStory().FindChapter("finale");
            var ids = fin.lines.Where(l => l.animalId != "").Select(l => l.animalId).ToArray();
            CollectionAssert.AreEqual(new[] { "fino", "mila", "piko", "luna", "nuri", "leni", "suri" }, ids);
            StringAssert.Contains("Schlaf schön.", fin.lines.Last().text);
        }

        [Test]
        public void ValidatorCatchesBrokenData()
        {
            var s = TestSupport.LoadStory();
            s.animals[0].steps[0].type = "Wischiwaschi";
            s.animals[1].lines.RemoveAll(l => l.phase == "hint");
            s.chapters[0].lines[0].tone = "schreien";
            var e = StoryValidator.Validate(s);
            Assert.That(e.Any(x => x.Contains("unbekannter Typ")));
            Assert.That(e.Any(x => x.Contains("Hinweis")));
            Assert.That(e.Any(x => x.Contains("tone")));
        }

        [Test]
        public void EveryStepHasChildFriendlyTouchZoneAndHelpOrdering()
        {
            foreach (var a in TestSupport.LoadStory().animals)
                foreach (var st in a.steps)
                {
                    Assert.GreaterOrEqual(st.radius, 0.6f, a.id + "/" + st.id + " Touch-Zone zu klein");
                    Assert.Greater(st.assistAfter, st.helpAfter);
                    Assert.LessOrEqual(st.assistAfter, 40f, "Kind soll nicht zu lange warten");
                }
        }
    }
}
