using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    public class ComposeOptions
    {
        public bool silentAudio;                    // Tests: keine Tonausgabe
        public float timeScale = 1f;                // Tests: schneller Durchlauf
        public IPointerSource pointer;              // Tests: skriptbare Eingabe
        public ParentSettings settingsOverride;     // Tests: Einstellungen ohne PlayerPrefs
        public bool freshProgress;                  // Tests: Fortschritt ignorieren
        public bool autoRun = true;
    }

    /// <summary>Marker für das App-Wurzelobjekt.</summary>
    public class SchlafwaldApp : MonoBehaviour
    {
        public AppServices Services;
        public bool Running { get { return Services != null && Services.Director != null && Services.Director.Started && !Services.Director.Finished; } }
    }

    /// <summary>
    /// Setzt die App ohne vorab gebaute Szenen zusammen (Wald, Tiere, Systeme). Dadurch läuft das Projekt in jeder leeren Szene
    /// und die Szene selbst bleibt austauschbar. Siehe EditorTools/ProjectSetup für die Main-Szene.
    /// </summary>
    public static class AppComposer
    {
        public static bool AutoStart = true;
        public static SchlafwaldApp Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!AutoStart || !Application.isPlaying || Object.FindObjectOfType<SchlafwaldApp>() != null) return;
            Compose(new ComposeOptions());
        }

        public static void Teardown()
        {
            if (Instance != null) Object.Destroy(Instance.gameObject);
            Instance = null;
            PauseController.Clear();
            PointerInput.Reset();
        }

        public static SchlafwaldApp Compose(ComposeOptions o)
        {
            Teardown();
            PauseController.BaseTimeScale = o.timeScale; PauseController.Clear();
            if (o.pointer != null) PointerInput.Source = o.pointer; else PointerInput.Source = new HardwarePointerSource();

            var rootGo = new GameObject("SchlafwaldApp");
            Object.DontDestroyOnLoad(rootGo);
            var app = rootGo.AddComponent<SchlafwaldApp>();
            Instance = app;
            var s = new AppServices(); app.Services = s;
            rootGo.AddComponent<InputPump>();

            Application.targetFrameRate = 30;   // ruhige Szenen: 30 fps schonen Akku und Wärme
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // Layout fest: Querformat (siehe docs/01-Konzeption.md, Abschnitt Layout)
            Screen.autorotateToPortrait = false; Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true; Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Shader.SetGlobalFloat("_SleepMotion", 1f);

            // Einstellungen & Fortschritt (nur lokal)
            s.Settings = rootGo.AddComponent<ParentSettingsManager>();
            s.Settings.Load();
            if (o.settingsOverride != null) { s.Settings.Current.masterVolume = o.settingsOverride.masterVolume; s.Settings.Current.audioOnlyMode = o.settingsOverride.audioOnlyMode; s.Settings.Current.enabledAnimals = o.settingsOverride.enabledAnimals; s.Settings.Current.targetMinutes = o.settingsOverride.targetMinutes; s.Settings.Current.dimAfterMinutes = o.settingsOverride.dimAfterMinutes; }
            s.Progress = rootGo.AddComponent<SaveProgressManager>(); s.Progress.Load();
            if (o.freshProgress) s.Progress.Reset();
            s.Story = StoryRepository.Load("de");

            // Teilnehmende Tiere = ausgewählt UND umgesetzt (mindestens eines)
            var participating = new List<AnimalConfig>();
            foreach (var a in s.Story.animals) if (a.implemented && s.Settings.Current.IsAnimalEnabled(a.id)) participating.Add(a);
            if (participating.Count == 0) foreach (var a in s.Story.animals) if (a.implemented) participating.Add(a);

            // Kamera
            var cam = Camera.main;
            if (cam == null)
            {
                var cg = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = cg.AddComponent<Camera>();
                if (Object.FindObjectOfType<AudioListener>() == null) cg.AddComponent<AudioListener>();
            }
            s.Camera = cam;
            cam.allowMSAA = true;

            // Welt
            s.World = new ForestBuilder();
            s.World.Build(s.Story, rootGo.transform);
            s.Props = new PropFactory();

            var controllers = new List<AnimalController>();
            foreach (var a in participating)
            {
                if (a.species == "snail")
                {
                    var p0 = a.steps[0].points[0];
                    HabitatBuilder.BuildSuri(s.World, new Vector3(a.sleepSpot.position.x, 0f, a.sleepSpot.position.z), new Vector3(p0.x, 0f, p0.z));
                }
                var ctl = AnimalController.Create(a, rootGo.transform, s);
                if (ctl != null) { ctl.PlaceAtStart(s); controllers.Add(ctl); }
            }

            // Systeme
            s.Lighting = rootGo.AddComponent<LightingController>();
            s.Lighting.Init(cam, rootGo.transform, s.World.FogBanks);
            s.Particles = new GameObject("Particles").AddComponent<AtmosphereParticles>();
            s.Particles.Init(cam, rootGo.transform);

            s.Audio = rootGo.AddComponent<AudioEnvironmentManager>(); s.Audio.SilentMode = o.silentAudio; s.Audio.Init(s);
            s.Narration = rootGo.AddComponent<NarrationManager>(); s.Narration.SilentMode = o.silentAudio; s.Narration.Init(s);
            s.CameraDirector = rootGo.AddComponent<CameraDirector>();
            var intro = s.Story.FindChapter("intro");
            var camStart = Shot.From(intro.shots[0]);
            camStart.position += new Vector3(-1.5f, -0.1f, -5f);   // beginnt etwas zurück; die erste Zeile fährt sanft vor
            s.CameraDirector.Init(cam, camStart);
            s.Interaction = rootGo.AddComponent<AnimalInteractionController>(); s.Interaction.Init(s);
            s.Transitions = rootGo.AddComponent<SceneTransitionManager>(); s.Transitions.Init(s);
            s.Forest = rootGo.AddComponent<SleepForestController>(); s.Forest.Init(s, controllers.Count);
            foreach (var c in controllers) s.Forest.Register(c);
            s.Director = rootGo.AddComponent<StoryDirector>(); s.Director.Init(s, controllers);

            var overlay = rootGo.AddComponent<OverlayGui>(); overlay.Init(s);
            overlay.RestartRequested = () => { s.Progress.Reset(); Compose(o); };
            rootGo.AddComponent<AppLifecycle>().Init(s);

            if (o.autoRun) app.StartCoroutine(s.Director.Run());
            return app;
        }
    }

    /// <summary>App-Wechsel/Wiederaufnahme: beim Verlassen anhalten & speichern, beim Zurückkehren weiter.</summary>
    public class AppLifecycle : MonoBehaviour
    {
        AppServices s;
        public void Init(AppServices services) { s = services; }

        void OnApplicationPause(bool paused)
        {
            if (s == null) return;
            if (paused) { PauseController.Request("background"); s.Settings.Save(); s.Progress.Save(); }
            else PauseController.Release("background");
        }

        void OnApplicationQuit() { if (s != null) { s.Settings.Save(); s.Progress.Save(); } }
    }
}
