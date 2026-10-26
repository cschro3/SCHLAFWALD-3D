#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Schlafwald.EditorTools
{
    /// <summary>
    /// Einmalige Projekteinrichtung: URP-Asset (mobil, ohne Schatten), Main-Szene, Build-Einstellungen, Player-Settings
    /// (Querformat, Linear, IL2CPP/ARM64, keine Internet-Berechtigung) und Pflicht-Shader.
    /// STATUS: Dieser Code konnte ohne Unity-Editor nicht ausgeführt/kompiliert werden (siehe docs/QA.md) – bei Fehlermeldungen bitte melden.
    /// </summary>
    public static class ProjectSetup
    {
        const string SettingsDir = "Assets/Schlafwald/Settings";
        const string ScenePath = "Assets/Schlafwald/Scenes/Main.unity";

        [MenuItem("Schlafwald/1. Projekt einrichten")]
        public static void Run()
        {
            // Unity rejects additive scene creation while any loaded scene has no saved path,
            // including the clean default Untitled scene (isDirty alone is insufficient).
            if (!File.Exists(ScenePath) && !SaveUntitledScenes()) return;
            CreateUrpAsset();
            ApplyPlayerSettings();
            IncludeShaders();
            CreateMainScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SCHLAFWALD] Projekt eingerichtet. Szene: " + ScenePath);
        }

        static bool SaveUntitledScenes()
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || !string.IsNullOrEmpty(scene.path)) continue;
                if (Application.isBatchMode)
                    throw new BuildFailedException("[SCHLAFWALD] Ungespeicherte Szene vor der Einrichtung. Szenen zuerst speichern; keine Benutzerdaten werden verworfen.");
                if (EditorSceneManager.SaveScene(scene)) continue;
                Debug.LogWarning("[SCHLAFWALD] Einrichtung abgebrochen: Die unbenannte Szene wurde nicht gespeichert. Erst speichern, dann erneut einrichten.");
                return false;
            }
            return true;
        }

        static void CreateUrpAsset()
        {
            Directory.CreateDirectory(SettingsDir);
            string rdPath = SettingsDir + "/SchlafwaldRenderer.asset", rpPath = SettingsDir + "/SchlafwaldURP.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rdPath);
            if (renderer == null) { renderer = ScriptableObject.CreateInstance<UniversalRendererData>(); AssetDatabase.CreateAsset(renderer, rdPath); }
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(rpPath);
            if (asset == null) { asset = UniversalRenderPipelineAsset.Create(renderer); AssetDatabase.CreateAsset(asset, rpPath); }

            // Über SerializedObject, damit die Einstellungen auch bei kleinen URP-API-Änderungen funktionieren.
            var so = new SerializedObject(asset);
            Set(so, "m_SupportsHDR", false);
            Set(so, "m_MSAA", 4);
            Set(so, "m_MainLightShadowsSupported", false);   // Art Direction: keine harten Schatten
            Set(so, "m_AdditionalLightShadowsSupported", false);
            Set(so, "m_SupportsCameraDepthTexture", false);
            Set(so, "m_SupportsCameraOpaqueTexture", false);
            Set(so, "m_RenderScale", 1.0f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            GraphicsSettings.defaultRenderPipeline = asset;
            for (int i = 0; i < QualitySettings.names.Length; i++) { QualitySettings.SetQualityLevel(i, false); QualitySettings.renderPipeline = asset; }
            QualitySettings.SetQualityLevel(0, false);
        }

        static void Set(SerializedObject so, string name, object v)
        {
            var p = so.FindProperty(name);
            if (p == null) { Debug.LogWarning("[SCHLAFWALD] URP-Eigenschaft nicht gefunden: " + name); return; }
            if (v is bool) p.boolValue = (bool)v; else if (v is int) p.intValue = (int)v; else if (v is float) p.floatValue = (float)v;
        }

        static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Schlafwald";
            PlayerSettings.productName = "Schlafwald";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "app.schlafwald.nacht");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "app.schlafwald.nacht");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.forceInternetPermission = false;   // Datenschutz: App braucht kein Netz
            PlayerSettings.iOS.targetOSVersionString = "16.0";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, true);
        }

        static void IncludeShaders()
        {
            // Resources/Shaders ist ohnehin im Build; zusätzlich fest eintragen, damit Shader.Find nie ins Leere läuft.
            var gs = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            if (gs == null) return;
            var so = new SerializedObject(gs);
            var arr = so.FindProperty("m_AlwaysIncludedShaders");
            if (arr == null) return;
            foreach (var name in new[] { "Schlafwald/StylizedLit", "Schlafwald/SoftUnlit", "Schlafwald/SkyDome" })
            {
                var sh = Shader.Find(name); if (sh == null) continue;
                bool has = false;
                for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) has = true;
                if (has) continue;
                arr.arraySize++; arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CreateMainScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new BuildFailedException("[SCHLAFWALD] Main-Szene konnte nicht gespeichert werden; die offene Szene bleibt zur Wiederherstellung erhalten.");
                EditorSceneManager.CloseScene(scene, true);
            }
            // Setup/build must never overwrite an authored scene or discard other build scenes.
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existing = scenes.FindIndex(s => s.path == ScenePath);
            if (existing < 0) scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            else scenes[existing] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
