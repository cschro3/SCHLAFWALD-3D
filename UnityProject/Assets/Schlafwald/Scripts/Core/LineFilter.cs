using System.Collections.Generic;

namespace Schlafwald.Core
{
    /// <summary>Entscheidet, welche Erzählerzeilen bei der gewählten Tierauswahl gesprochen werden.</summary>
    public static class LineFilter
    {
        public static bool ShouldSpeak(NarrationLine line, ICollection<string> participatingAnimalIds, int totalAnimalCount)
        {
            if (!string.IsNullOrEmpty(line.animalId) && !participatingAnimalIds.Contains(line.animalId)) return false;
            bool all = participatingAnimalIds.Count >= totalAnimalCount;
            if (line.when == "all" && !all) return false;
            if (line.when == "partial" && all) return false;
            return true;
        }

        public static List<NarrationLine> Filter(IEnumerable<NarrationLine> lines, ICollection<string> participating, int total)
        {
            var r = new List<NarrationLine>();
            foreach (var l in lines) if (ShouldSpeak(l, participating, total)) r.Add(l);
            return r;
        }
    }
}
