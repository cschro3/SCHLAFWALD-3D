using System;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>Elterneinstellungen, lokal in PlayerPrefs (keine Konten, keine Übertragung).</summary>
    public class ParentSettingsManager : MonoBehaviour
    {
        const string Key = "sw_parent_v1";
        public ParentSettings Current { get; private set; }
        public event Action Changed;

        public void Load()
        {
            Current = new ParentSettings();
            try
            {
                if (PlayerPrefs.HasKey(Key)) JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key), Current);
            }
            catch (Exception e) { Debug.LogWarning("Einstellungen unlesbar, Standard wird genutzt: " + e.Message); Current = new ParentSettings(); }
            Current.Clamp();
        }

        public void Save()
        {
            Current.Clamp();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
            if (Changed != null) Changed();
        }
    }

    /// <summary>Speichert nur lokal, welche Tiere in dieser Nacht schon schlafen (kein Tracking, keine Personendaten).</summary>
    public class SaveProgressManager : MonoBehaviour
    {
        const string Key = "sw_progress_v1";
        public ProgressData Data { get; private set; }

        public void Load()
        {
            Data = new ProgressData();
            try { if (PlayerPrefs.HasKey(Key)) JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(Key), Data); }
            catch (Exception) { Data = new ProgressData(); }
        }

        public bool ShouldResume() { return ProgressRules.ShouldResume(Data, DateTime.UtcNow.Ticks); }

        public void MarkSleeping(string id)
        {
            if (!Data.sleepingAnimals.Contains(id)) Data.sleepingAnimals.Add(id);
            Data.finished = false; Save();
        }

        public void MarkFinished() { Data.finished = true; Save(); }
        public void Reset() { Data = new ProgressData(); Save(); }

        public void Save()
        {
            Data.lastPlayedUtcTicks = DateTime.UtcNow.Ticks;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }
    }

    public static class StoryRepository
    {
        public static StoryData Load(string language = "de")
        {
            var ta = Resources.Load<TextAsset>("Story/story_" + language);
            if (ta == null) throw new InvalidOperationException("Story-Datei Resources/Story/story_" + language + ".json fehlt.");
            var story = JsonUtility.FromJson<StoryData>(ta.text);
            var errors = StoryValidator.Validate(story);
            if (errors.Count > 0) Debug.LogError("Story-Daten fehlerhaft:\n" + string.Join("\n", errors));
            return story;
        }
    }
}
