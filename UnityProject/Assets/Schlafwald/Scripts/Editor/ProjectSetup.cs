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
            CreateUrpAsset();
            ApplyPlayerSettings();
            IncludeShaders();
            CreateMainScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SCHLAFWALD] Projekt eingerichtet. Szene: " + ScenePath);
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
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.forceInternetPermission = false;   // Datenschutz: App braucht kein Netz
            PlayerSettings.iOS.targetOSVersionString = "13.0";
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
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Absichtlich leer: AppComposer baut Wald, Tiere und Systeme zur Laufzeit (Szene bleibt austauschbar).
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
#endif
