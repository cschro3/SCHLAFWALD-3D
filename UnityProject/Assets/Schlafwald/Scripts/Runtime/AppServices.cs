using Schlafwald.Core;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>Explizit durchgereichte Abhängigkeiten statt globaler Singletons (leichter testbar, siehe AppComposer).</summary>
    public class AppServices
    {
        public Camera Camera;
        public StoryData Story;
        public ParentSettingsManager Settings;
        public SaveProgressManager Progress;
        public NarrationManager Narration;
        public AudioEnvironmentManager Audio;
        public CameraDirector CameraDirector;
        public LightingController Lighting;
        public AtmosphereParticles Particles;
        public SleepForestController Forest;
        public SceneTransitionManager Transitions;
        public AnimalInteractionController Interaction;
        public StoryDirector Director;
        public ForestBuilder World;
        public PropFactory Props;
    }
}
