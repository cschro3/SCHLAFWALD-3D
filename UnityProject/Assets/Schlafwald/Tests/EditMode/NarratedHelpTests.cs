using NUnit.Framework;
using Schlafwald.Core;

namespace Schlafwald.Tests
{
    public class NarratedHelpTests
    {
        [Test]
        public void SpeakingAHintPausesWaitingButAssistStillBecomesReachable()
        {
            var help = new HelpEscalator(12f, 30f);
            Assert.AreEqual(HelpLevel.Hint, help.Tick(12f, false, false));
            Assert.AreEqual(HelpLevel.None, help.Tick(8f, false, true));
            Assert.AreEqual(12f, help.IdleTime);
            Assert.AreEqual(HelpLevel.Assist, help.Tick(18f, false, false));
            Assert.AreEqual(HelpLevel.None, help.Tick(100f, false, false));
        }

        [Test]
        public void GenuineProgressDuringNarrationRestartsWaiting()
        {
            var help = new HelpEscalator(12f, 30f);
            help.Tick(11f, false, false);
            help.Tick(1f, true, true);
            Assert.AreEqual(0f, help.IdleTime);
            Assert.AreEqual(HelpLevel.None, help.Tick(11f, false, false));
            Assert.AreEqual(HelpLevel.Hint, help.Tick(1f, false, false));
        }
    }
}
