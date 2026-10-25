using System.IO;
using System.Runtime.CompilerServices;
using Schlafwald.Core;

namespace Schlafwald.Tests
{
    /// <summary>Lädt die echte story_de.json – in Unity per JsonUtility, unter "dotnet test" per System.Text.Json.</summary>
    public static class TestSupport
    {
        public static string StoryPath([CallerFilePath] string here = "")
        {
            string dir = Path.GetDirectoryName(here);
            return Path.GetFullPath(Path.Combine(dir, "..", "..", "Resources", "Story", "story_de.json"));
        }

        public static StoryData LoadStory()
        {
            string json = File.ReadAllText(StoryPath());
#if UNITY_5_3_OR_NEWER
            return UnityEngine.JsonUtility.FromJson<StoryData>(json);
#else
            return System.Text.Json.JsonSerializer.Deserialize<StoryData>(json,
                new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
#endif
        }
    }
}
