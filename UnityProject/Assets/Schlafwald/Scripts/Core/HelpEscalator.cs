namespace Schlafwald.Core
{
    public enum HelpLevel { None, Hint, Assist }

    /// <summary>
    /// Stufenweise Hilfe ohne Niederlage: erst freundliche Hinweise des Erzählers, nach einer
    /// angemessenen Pause beendet das Tier sein Ritual mit sanfter Unterstützung selbst.
    /// </summary>
    public class HelpEscalator
    {
        readonly float helpAfter;
        readonly float assistAfter;
        float idle;
        float nextHint;

        public bool AssistTriggered { get; private set; }
        public float IdleTime { get { return idle; } }
        public int HintsGiven { get; private set; }

        /// <param name="timeScale">1 normal; &gt;1 verkürzt die Wartezeiten (Audiomodus ohne Bildschirm).</param>
        public HelpEscalator(float helpAfter, float assistAfter, float timeScale = 1f)
        {
            if (timeScale <= 0f) timeScale = 1f;
            this.helpAfter = helpAfter / timeScale;
            this.assistAfter = assistAfter / timeScale;
            nextHint = this.helpAfter;
        }

        public void NotifyProgress()
        {
            idle = 0f;
            nextHint = helpAfter;
        }

        public HelpLevel Tick(float dt, bool progressed)
        {
            if (AssistTriggered) return HelpLevel.None;
            if (progressed) { NotifyProgress(); return HelpLevel.None; }
            idle += dt;
            if (idle >= assistAfter) { AssistTriggered = true; return HelpLevel.Assist; }
            if (idle >= nextHint && nextHint < assistAfter)
            {
                nextHint += helpAfter;
                HintsGiven++;
                return HelpLevel.Hint;
            }
            return HelpLevel.None;
        }

        /// <summary>Speaking suspends waiting without erasing time accumulated before the hint.</summary>
        public HelpLevel Tick(float dt, bool progressed, bool isNarrating)
        {
            if (progressed) { NotifyProgress(); return HelpLevel.None; }
            return isNarrating ? HelpLevel.None : Tick(dt, false);
        }
    }
}
