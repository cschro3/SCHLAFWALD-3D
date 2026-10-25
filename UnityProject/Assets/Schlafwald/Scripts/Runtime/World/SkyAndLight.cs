using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>Weiche Himmelskuppel (Verlauf, Sonne, Mond, Sterne) – folgt der Kamera.</summary>
    public class SkyDome : MonoBehaviour
    {
        Material mat;
        public Material Mat { get { return mat; } }

        public static SkyDome Create(Transform parent)
        {
            var go = new GameObject("SkyDome");
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * 400f;
            var mb = new MeshBuilder();
            mb.AddEllipsoid(Vector3.zero, Vector3.one, MeshBuilder.C(1, 1, 1), 32, 20);
            go.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("SW_SkyDome");
            var mr = go.AddComponent<MeshRenderer>();
            var sky = go.AddComponent<SkyDome>();
            sky.mat = MaterialLibrary.Sky();
            mr.sharedMaterial = sky.mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
            go.AddComponent<FollowCamera>();
            return sky;
        }
    }

    /// <summary>
    /// Steuert Tageszeit und Beleuchtung: Sonnenuntergang → Dämmerung → Mondnacht, vollständig aus
    /// <see cref="AtmosphereValues.TimeOfDay"/>. Keine harten Schatten (Licht ohne Schatten), nur weiche Verläufe.
    /// </summary>
    public class LightingController : MonoBehaviour
    {
        struct Key
        {
            public Color horizon, zenith, fog, ambSky, ambEq, ambGround, light;
            public float intensity, fogDensity, sun, moon, stars;
        }

        static readonly Key Sunset = new Key
        {
            horizon = new Color(0.98f, 0.66f, 0.52f), zenith = new Color(0.30f, 0.34f, 0.62f), fog = new Color(0.62f, 0.47f, 0.50f),
            ambSky = new Color(0.50f, 0.46f, 0.66f), ambEq = new Color(0.52f, 0.40f, 0.44f), ambGround = new Color(0.24f, 0.20f, 0.20f),
            light = new Color(1.0f, 0.74f, 0.52f), intensity = 1.15f, fogDensity = 0.016f, sun = 1f, moon = 0f, stars = 0f
        };
        static readonly Key Twilight = new Key
        {
            horizon = new Color(0.58f, 0.45f, 0.66f), zenith = new Color(0.13f, 0.16f, 0.38f), fog = new Color(0.28f, 0.26f, 0.44f),
            ambSky = new Color(0.34f, 0.36f, 0.58f), ambEq = new Color(0.30f, 0.28f, 0.46f), ambGround = new Color(0.12f, 0.12f, 0.20f),
            light = new Color(0.75f, 0.70f, 0.95f), intensity = 0.80f, fogDensity = 0.020f, sun = 0.25f, moon = 0.5f, stars = 0.4f
        };
        static readonly Key Night = new Key
        {
            horizon = new Color(0.15f, 0.22f, 0.40f), zenith = new Color(0.035f, 0.055f, 0.17f), fog = new Color(0.10f, 0.14f, 0.28f),
            ambSky = new Color(0.22f, 0.28f, 0.50f), ambEq = new Color(0.17f, 0.21f, 0.38f), ambGround = new Color(0.08f, 0.09f, 0.16f),
            light = new Color(0.60f, 0.70f, 1.0f), intensity = 0.62f, fogDensity = 0.024f, sun = 0f, moon = 1f, stars = 1f
        };

        static Key Mix(Key a, Key b, float t)
        {
            return new Key
            {
                horizon = Color.Lerp(a.horizon, b.horizon, t), zenith = Color.Lerp(a.zenith, b.zenith, t), fog = Color.Lerp(a.fog, b.fog, t),
                ambSky = Color.Lerp(a.ambSky, b.ambSky, t), ambEq = Color.Lerp(a.ambEq, b.ambEq, t), ambGround = Color.Lerp(a.ambGround, b.ambGround, t),
                light = Color.Lerp(a.light, b.light, t), intensity = Mathf.Lerp(a.intensity, b.intensity, t),
                fogDensity = Mathf.Lerp(a.fogDensity, b.fogDensity, t), sun = Mathf.Lerp(a.sun, b.sun, t),
                moon = Mathf.Lerp(a.moon, b.moon, t), stars = Mathf.Lerp(a.stars, b.stars, t)
            };
        }

        public static readonly Vector3 MoonDirection = new Vector3(0.30f, 0.50f, 0.81f).normalized;

        Light sunMoonLight;
        SkyDome sky;
        Camera cam;
        List<Renderer> fogBanks = new List<Renderer>();
        MaterialPropertyBlock block;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        public void Init(Camera camera, Transform parent, List<Renderer> banks)
        {
            cam = camera;
            fogBanks = banks ?? fogBanks;
            block = new MaterialPropertyBlock();
            var lg = new GameObject("SunMoonLight");
            lg.transform.SetParent(parent, false);
            sunMoonLight = lg.AddComponent<Light>();
            sunMoonLight.type = LightType.Directional;
            sunMoonLight.shadows = LightShadows.None; // Art Direction: keine harten Schatten
            sky = SkyDome.Create(parent);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.farClipPlane = 600f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.skybox = null;
            Apply(SleepAtmosphere.Evaluate(0f));
        }

        public void Apply(AtmosphereValues v)
        {
            if (sunMoonLight == null) return;
            float t = Mathf.Clamp01(v.TimeOfDay);
            Key k = t < 0.35f ? Mix(Sunset, Twilight, Smooth(t / 0.35f)) : Mix(Twilight, Night, Smooth((t - 0.35f) / 0.65f));

            RenderSettings.ambientSkyColor = k.ambSky; RenderSettings.ambientEquatorColor = k.ambEq; RenderSettings.ambientGroundColor = k.ambGround;
            RenderSettings.fogColor = k.fog; RenderSettings.fogDensity = k.fogDensity;
            if (cam != null) cam.backgroundColor = k.fog;

            // Sonnenrichtung: sinkt hinter den Horizont; Licht wandert weich zum Mond
            Vector3 sunDir = new Vector3(-0.35f, Mathf.Lerp(0.07f, -0.35f, Smooth(Mathf.Clamp01(t / 0.30f))), 0.93f).normalized;
            Vector3 lightFrom = Vector3.Slerp(new Vector3(sunDir.x, Mathf.Max(sunDir.y, 0.10f) + 0.12f, sunDir.z).normalized, MoonDirection, Smooth(Mathf.Clamp01((t - 0.15f) / 0.35f)));
            sunMoonLight.transform.rotation = Quaternion.LookRotation(-lightFrom);
            sunMoonLight.color = k.light;
            sunMoonLight.intensity = k.intensity;

            var m = sky.Mat;
            m.SetColor("_Horizon", k.horizon); m.SetColor("_Zenith", k.zenith);
            m.SetColor("_SunColor", Color.Lerp(new Color(1f, 0.72f, 0.48f), new Color(1f, 0.55f, 0.45f), t));
            m.SetVector("_SunDir", sunDir); m.SetFloat("_SunAmount", k.sun);
            m.SetVector("_MoonDir", MoonDirection); m.SetFloat("_MoonAmount", k.moon);
            m.SetFloat("_Stars", k.stars);

            Color bank = new Color(k.fog.r * 1.35f + 0.04f, k.fog.g * 1.35f + 0.04f, k.fog.b * 1.35f + 0.05f, 0.11f);
            foreach (var r in fogBanks)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block); block.SetColor(ColorId, bank); r.SetPropertyBlock(block);
            }
        }

        static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }
    }

    /// <summary>Glühwürmchen und Staubflocken im Mondlicht; werden mit jedem schlafenden Tier weniger und langsamer.</summary>
    public class AtmosphereParticles : MonoBehaviour
    {
        ParticleSystem fireflies, motes;
        const float FireflyRate = 7f, MoteRate = 5f;
        Camera cam;

        public void Init(Camera camera, Transform parent)
        {
            cam = camera;
            transform.SetParent(parent, false);
            fireflies = Make("Fireflies", new Color(1f, 0.93f, 0.50f, 1f), 0.10f, 0.20f, 6f, 10f, 0.55f, 0.9f, 2.0f, 28f, 6f);
            motes = Make("Motes", new Color(0.85f, 0.9f, 1f, 0.6f), 0.03f, 0.06f, 8f, 14f, 0.2f, 0.4f, 1.8f, 26f, 8f);
        }

        ParticleSystem Make(string name, Color color, float sizeMin, float sizeMax, float lifeMin, float lifeMax, float noiseStrength, float noiseFreq, float centerY, float width, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true; main.playOnAwake = true; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startSpeed = 0.05f; main.startColor = color; main.maxParticles = 120;
            var em = ps.emission; em.rateOverTime = name == "Fireflies" ? FireflyRate : MoteRate;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(width, height, width);
            sh.position = new Vector3(0, centerY - 1f, 0);
            var noise = ps.noise; noise.enabled = true; noise.strength = noiseStrength; noise.frequency = noiseFreq; noise.quality = ParticleSystemNoiseQuality.Low;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sharedMaterial = MaterialLibrary.Glow(ProceduralTextures.SoftDot(), Color.white, name == "Fireflies" ? 1.4f : 0.8f, true);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            return ps;
        }

        void LateUpdate()
        {
            if (cam == null) return;
            Vector3 f = cam.transform.forward; f.y = 0f; f = f.sqrMagnitude > 0.01f ? f.normalized : Vector3.forward;
            transform.position = cam.transform.position + f * 9f;
        }

        public void Apply(AtmosphereValues v)
        {
            if (fireflies == null) return;
            var e1 = fireflies.emission; e1.rateOverTime = FireflyRate * v.Particles;
            var e2 = motes.emission; e2.rateOverTime = MoteRate * (0.4f + 0.6f * v.Particles);
            var m1 = fireflies.main; m1.simulationSpeed = Mathf.Lerp(0.35f, 1f, v.Motion);
            var m2 = motes.main; m2.simulationSpeed = Mathf.Lerp(0.35f, 1f, v.Motion);
        }
    }
}
