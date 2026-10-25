#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Schlafwald.EditorTools
{
    /// <summary>
    /// Batch-Builds für CI/Release:
    ///   Unity -batchmode -quit -projectPath UnityProject -executeMethod Schlafwald.EditorTools.BuildScript.BuildAndroid
    /// Umgebungsvariablen: SCHLAFWALD_BUILD_AAB=1 für App Bundle (Play Store), sonst APK.
    /// STATUS: ohne Unity-Editor nicht ausgeführt (siehe docs/QA.md).
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("Schlafwald/2. Build Android")] public static void BuildAndroid() { Build(BuildTarget.Android, BuildTargetGroup.Android); }
        [MenuItem("Schlafwald/3. Build iOS (Xcode-Projekt)")] public static void BuildIOS() { Build(BuildTarget.iOS, BuildTargetGroup.iOS); }
        [MenuItem("Schlafwald/4. Build WebGL-Vorschau")] public static void BuildWebGL() { Build(BuildTarget.WebGL, BuildTargetGroup.WebGL); }

        static void Build(BuildTarget target, BuildTargetGroup group)
        {
            ProjectSetup.Run();
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            string path;
            if (target == BuildTarget.Android)
            {
                bool aab = Environment.GetEnvironmentVariable("SCHLAFWALD_BUILD_AAB") == "1";
                EditorUserBuildSettings.buildAppBundle = aab;
                path = "Builds/Android/schlafwald" + (aab ? ".aab" : ".apk");
            }
            else if (target == BuildTarget.WebGL) path = "Builds/WebGL";
            else path = "Builds/iOS";

            // A preview may use explicit development fallbacks; a release must never silently
            // ship missing characters or recordings.
            bool release = Environment.GetEnvironmentVariable("SCHLAFWALD_RELEASE") == "1";
            if (release)
            {
                var story = StoryRepository.Load("de");
                int have, total;
                NarrationManager.Coverage(story, "warm", out have, out total);
                if (story.animals.Exists(a => !a.implemented) || have != total)
                    throw new InvalidOperationException("Release gesperrt: sieben Unity-Tiere und vollständige Sprachaufnahmen erforderlich.");
            }

            var opts = new BuildPlayerOptions { scenes = new[] { "Assets/Schlafwald/Scenes/Main.unity" }, locationPathName = path, target = target, options = release ? BuildOptions.None : BuildOptions.Development };
            var report = BuildPipeline.BuildPlayer(opts);
            Debug.Log("[SCHLAFWALD] Build " + target + ": " + report.summary.result + ", " + report.summary.totalSize / (1024 * 1024) + " MB");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
#endif
