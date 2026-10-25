namespace Schlafwald.Core
{
    /// <summary>Dauerschätzung für den Entwicklungs-Fallback (ohne Sprachaufnahmen) und die Pacing-Planung.</summary>
    public static class NarrationTiming
    {
        public const float WordsPerSecond = 1.8f; // bewusst langsames Erzähltempo (~108 Wörter/min)

        public static int WordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            int n = 0; bool inWord = false;
            foreach (char c in text)
            {
                bool w = char.IsLetterOrDigit(c) || c == '\'' || c == '-';
                if (w && !inWord) n++;
                inWord = w;
            }
            return n;
        }

        public static float EstimateSeconds(string text, float speed = 1f)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0f;
            if (speed <= 0.1f) speed = 1f;
            float t = WordCount(text) / (WordsPerSecond * speed);
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '…') t += 0.9f;
                else if (c == '.' && i + 2 < text.Length && text[i + 1] == '.' && text[i + 2] == '.') { t += 0.9f; i += 2; }
                else if (c == ',') t += 0.2f;
                else if (c == '.' || c == '!' || c == '?') t += 0.45f;
                else if (c == '\n') t += 0.3f;
            }
            return t < 0.6f ? 0.6f : t;
        }
    }
}
