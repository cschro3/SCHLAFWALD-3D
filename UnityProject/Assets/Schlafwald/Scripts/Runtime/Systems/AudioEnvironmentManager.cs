using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Adaptives Audiosystem: Wind, ferne Waldgeräusche, Klangflächen und eine leise Klaviermelodie. Je mehr Tiere schlafen,
    /// desto geringer die akustische Aktivität. Musik ist jederzeit abschaltbar; der Erzähler ist separat regelbar.
    /// Lädt echte Dateien aus Resources/Audio/..., falls vorhanden – sonst die generierten Platzhalter (ProceduralAudio).
    /// </summary>
    public class AudioEnvironmentManager : MonoBehaviour
    {
        AppServices s;
        AudioSource wind, crickets, padA, padB;
        readonly List<AudioSource> notePool = new List<AudioSource>();
        readonly List<AudioSource> sfxPool = new List<AudioSource>();
        readonly Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();
        AudioClip[] notes;
        float[] noteHz = { 196f, 220f, 261.63f, 293.66f, 329.63f, 392f, 440f, 523.25f };
        AtmosphereValues atmos = SleepAtmosphere.Evaluate(0f);
        float musicFade = 1f, musicFadeTarget = 1f, musicFadeRate;
        bool ready;
        public bool SilentMode;      // Tests: kein Audio erzeugen

        public void Init(AppServices services)
        {
            s = services;
            wind = Source("Wind", true); crickets = Source("Crickets", true); padA = Source("PadA", true); padB = Source("PadB", true);
            for (int i = 0; i < 3; i++) notePool.Add(Source("Note" + i, false));
            for (int i = 0; i < 5; i++) { var a = Source("Sfx" + i, false); a.spatialBlend = 0.6f; a.rolloffMode = AudioRolloffMode.Linear; a.minDistance = 1f; a.maxDistance = 14f; sfxPool.Add(a); }
            if (!SilentMode) StartCoroutine(Setup());
        }

        AudioSource Source(string name, bool loop)
        {
            var go = new GameObject("Audio_" + name); go.transform.SetParent(transform, false);
            var a = go.AddComponent<AudioSource>();
            a.playOnAwake = false; a.loop = loop; a.spatialBlend = 0f; a.volume = 0f;
            return a;
        }

        static AudioClip Load(string path, System.Func<AudioClip> generate)
        {
            var c = Resources.Load<AudioClip>(path);
            return c != null ? c : generate();
        }

        IEnumerator Setup()
        {
            wind.clip = Load("Audio/Ambience/wind", () => ProceduralAudio.Wind()); wind.Play(); yield return null;
            crickets.clip = Load("Audio/Ambience/crickets", () => ProceduralAudio.Crickets()); crickets.Play(); yield return null;
            padA.clip = Load("Audio/Music/pad_a", () => ProceduralAudio.Pad(new[] { 130.81f, 196f, 329.63f, 587.33f }, 16f, "SW_PadA")); padA.Play(); yield return null;
            padB.clip = Load("Audio/Music/pad_b", () => ProceduralAudio.Pad(new[] { 110f, 164.81f, 261.63f, 392f }, 16f, "SW_PadB")); padB.Play(); yield return null;
            notes = new AudioClip[noteHz.Length];
            for (int i = 0; i < notes.Length; i++)
            {
                int idx = i;
                notes[i] = Load("Audio/Music/note_" + (int)noteHz[i], () => ProceduralAudio.Note(noteHz[idx]));
                yield return null;
            }
            ready = true;
            StartCoroutine(MelodyRoutine());
            StartCoroutine(AmbienceEvents());
        }

        public void Apply(AtmosphereValues v) { atmos = v; }

        public void FadeOutMusic(float seconds)
        {
            musicFadeTarget = 0f; musicFadeRate = seconds > 0f ? 1f / seconds : 100f;
        }

        public void ResetMusicFade() { musicFadeTarget = 1f; musicFade = 1f; }

        float Master { get { return s.Settings.Current.masterVolume; } }

        void Update()
        {
            if (!ready) return;
            var p = s.Settings.Current;
            musicFade = Mathf.MoveTowards(musicFade, musicFadeTarget, Time.deltaTime * (musicFadeTarget < musicFade ? musicFadeRate : 0.2f));
            float amb = p.ambienceVolume * Master;
            float mus = p.musicEnabled ? p.musicVolume * Master * atmos.MusicIntensity * musicFade : 0f;
            float k = 1f - Mathf.Exp(-2f * Time.unscaledDeltaTime);
            wind.volume = Mathf.Lerp(wind.volume, 0.28f * amb * (0.45f + 0.55f * atmos.Motion), k);
            crickets.volume = Mathf.Lerp(crickets.volume, 0.55f * amb * atmos.AmbienceActivity * Mathf.Clamp01(atmos.TimeOfDay * 2f), k);
            float lfo = 0.5f + 0.5f * Mathf.Sin(Time.time * (Mathf.PI * 2f / 40f));
            padA.volume = Mathf.Lerp(padA.volume, 0.55f * mus * lfo, k);
            padB.volume = Mathf.Lerp(padB.volume, 0.55f * mus * (1f - lfo), k);
        }

        IEnumerator MelodyRoutine()
        {
            // Sehr ruhige Pentatonik, Schrittbewegung, lange Pausen; wird mit der Nacht seltener und leiser.
            int pos = 4;
            int[] steps = { -1, 1, 1, -2, 2, -1, 0, 1, -1 };
            int n = 0;
            while (true)
            {
                float gap = Random.Range(1.6f, 3.2f) * (1f + (1f - atmos.MusicIntensity / 0.65f) * 1.5f);
                yield return new WaitForSeconds(gap);
                var p = s.Settings.Current;
                if (!p.musicEnabled || musicFade < 0.02f) continue;
                pos = Mathf.Clamp(pos + steps[n++ % steps.Length], 0, notes.Length - 1);
                var src = notePool[n % notePool.Count];
                src.clip = notes[pos];
                src.volume = 0.42f * p.musicVolume * Master * atmos.MusicIntensity * musicFade;
                src.pitch = 1f;
                src.Play();
            }
        }

        IEnumerator AmbienceEvents()
        {
            while (true)
            {
                float act = Mathf.Max(0.04f, atmos.AmbienceActivity);
                yield return new WaitForSeconds(Random.Range(5f, 10f) / act);
                string key = Random.value < 0.6f ? "rustle" : "chirp";
                Vector3 pos = s.Camera != null ? s.Camera.transform.position + Random.onUnitSphere * 8f : Vector3.zero;
                PlaySfx(key, pos, 0.22f * atmos.AmbienceActivity, true);
            }
        }

        public void PlaySfx(string key, Vector3 pos, float volume = 0.5f) { PlaySfx(key, pos, volume, false); }

        void PlaySfx(string key, Vector3 pos, float volume, bool ambient)
        {
            if (SilentMode || !ready) return;
            AudioClip clip;
            if (!sfxCache.TryGetValue(key, out clip)) { clip = Load("Audio/Sfx/" + key, () => ProceduralAudio.Sfx(key)); sfxCache[key] = clip; }
            AudioSource src = null;
            foreach (var a in sfxPool) if (!a.isPlaying) { src = a; break; }
            if (src == null) return;
            src.transform.position = pos; src.clip = clip;
            src.volume = Mathf.Clamp01(volume) * (ambient ? s.Settings.Current.ambienceVolume : 0.8f) * Master;
            src.pitch = Random.Range(0.94f, 1.06f);
            src.Play();
        }
    }
}
