using UnityEngine;

namespace Schlafwald
{
    /// <summary>
    /// Erzeugt Materialien mit den eigenen URP-Shadern (Resources/Shaders). Fehlt ein Shader (z. B. Build-Stripping),
    /// fällt es sichtbar auf URP/Lit bzw. Standard zurück, statt pink zu rendern.
    /// </summary>
    public static class MaterialLibrary
    {
        static Shader Find(string name, params string[] fallbacks)
        {
            var s = Shader.Find(name);
            if (s != null && s.isSupported) return s;
            foreach (var f in fallbacks) { s = Shader.Find(f); if (s != null && s.isSupported) return s; }
            return Shader.Find("Sprites/Default");
        }

        public static Material Lit(Color baseColor, float rim = 0.3f, float fur = 0f, float sway = 0f, bool doubleSided = false, Color? emission = null, Color? rimColor = null, Color? shade = null)
        {
            var sh = Find("Schlafwald/StylizedLit", "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Lit", "Standard");
            var m = new Material(sh) { name = "SW_Lit" };
            SetColor(m, "_BaseColor", baseColor); SetColor(m, "_Color", baseColor);
            SetColor(m, "_ShadeColor", shade ?? new Color(0.46f, 0.40f, 0.58f));
            SetColor(m, "_RimColor", rimColor ?? new Color(1f, 0.92f, 0.78f));
            SetColor(m, "_Emission", emission ?? Color.black);
            SetFloat(m, "_RimStrength", rim); SetFloat(m, "_FurAmount", fur); SetFloat(m, "_SwayAmount", sway);
            SetFloat(m, "_Cull", doubleSided ? 0f : 2f);
            return m;
        }

        /// <summary>Weiche Leucht-/Nebelfläche. additive = Leuchten (Glühwürmchen), sonst Alpha (Nebel, Schleim).</summary>
        public static Material Glow(Texture tex, Color tint, float intensity = 1f, bool additive = true)
        {
            var sh = Find("Schlafwald/SoftUnlit", "Universal Render Pipeline/Particles/Unlit", "Sprites/Default");
            var m = new Material(sh) { name = "SW_Glow" };
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", tex);
            SetColor(m, "_Color", tint);
            SetFloat(m, "_Intensity", intensity);
            SetFloat(m, "_SrcBlend", additive ? (float)UnityEngine.Rendering.BlendMode.SrcAlpha : (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            SetFloat(m, "_DstBlend", additive ? (float)UnityEngine.Rendering.BlendMode.One : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.renderQueue = 3000;
            return m;
        }

        public static Material Sky()
        {
            var sh = Find("Schlafwald/SkyDome", "Sprites/Default");
            return new Material(sh) { name = "SW_Sky", renderQueue = 1000 };
        }

        static void SetColor(Material m, string n, Color c) { if (m.HasProperty(n)) m.SetColor(n, c); }
        static void SetFloat(Material m, string n, float v) { if (m.HasProperty(n)) m.SetFloat(n, v); }
    }

    public static class ProceduralTextures
    {
        static Texture2D softDot, fogBlob;

        /// <summary>Weicher runder Lichtpunkt (Alpha, RGB weiß).</summary>
        public static Texture2D SoftDot()
        {
            if (softDot != null) return softDot;
            const int n = 64;
            softDot = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "SW_SoftDot" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float a = Mathf.Clamp01(1f - d); a = a * a * (3f - 2f * a); a *= a;
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            softDot.SetPixels32(px); softDot.Apply(false, true);
            return softDot;
        }

        /// <summary>Weiche, leicht wolkige Nebelfläche mit Rand-Ausblendung.</summary>
        public static Texture2D FogBlob()
        {
            if (fogBlob != null) return fogBlob;
            const int n = 64;
            fogBlob = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "SW_FogBlob" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float edge = Mathf.Clamp01(1f - d); edge = edge * edge * (3f - 2f * edge);
                    float noise = 0.55f + 0.45f * Mathf.PerlinNoise(x * 0.09f + 3.1f, y * 0.09f + 7.7f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(edge * noise) * 255));
                }
            fogBlob.SetPixels32(px); fogBlob.Apply(false, true);
            return fogBlob;
        }
    }
}
