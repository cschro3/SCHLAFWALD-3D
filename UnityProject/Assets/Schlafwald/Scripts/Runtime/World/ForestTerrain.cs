using System.Collections.Generic;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>Höhenfeld der Welt: weiche Hügel abseits des Pfads, ebene Lichtungen an den Schlafplätzen, plus Steine als Höcker.</summary>
    public static class ForestTerrain
    {
        public static CurvePath Path;
        static readonly List<Vector3> clearings = new List<Vector3>();
        struct Bump { public Vector3 c; public float rx, rz, h; }
        static readonly List<Bump> bumps = new List<Bump>();

        public static void Reset(CurvePath path)
        {
            Path = path; clearings.Clear(); bumps.Clear();
        }

        public static void AddClearing(Vector3 center) { clearings.Add(center); }

        /// <summary>Ellipsoid-Kappe (Stein). Oberfläche y = h·sqrt(1 − q²).</summary>
        public static void AddBump(Vector3 center, float radiusX, float radiusZ, float height)
        {
            bumps.Add(new Bump { c = center, rx = radiusX, rz = radiusZ, h = height });
        }

        static float Fbm(float x, float z)
        {
            float a = 0.5f, f = 1f, sum = 0f;
            for (int i = 0; i < 3; i++) { sum += a * Mathf.PerlinNoise(x * f + 31.7f, z * f + 11.3f); a *= 0.5f; f *= 2f; }
            return sum / 0.875f;
        }

        public static float Ground(float x, float z)
        {
            float d = Path != null ? Path.DistanceXZ(x, z) : 10f;
            float open = Mathf.SmoothStep(0f, 1f, (d - 3.5f) / 12f);
            float clear = 0f;
            foreach (var c in clearings)
            {
                float dc = Mathf.Sqrt((x - c.x) * (x - c.x) + (z - c.z) * (z - c.z));
                clear = Mathf.Max(clear, Mathf.SmoothStep(1f, 0f, (dc - 3.5f) / 4.5f));
            }
            float hills = (Fbm(x * 0.045f, z * 0.045f) - 0.42f) * 9f;
            float micro = (Mathf.PerlinNoise(x * 0.35f, z * 0.35f) - 0.5f) * 0.22f * Mathf.SmoothStep(1.5f, 5f, d);
            float h = (hills * open + micro) * (1f - clear);
            h -= 0.05f * (1f - Mathf.SmoothStep(0f, 1.4f, d));
            return h;
        }

        public static float Height(float x, float z)
        {
            float g = Ground(x, z);
            foreach (var b in bumps)
            {
                float dx = (x - b.c.x) / b.rx, dz = (z - b.c.z) / b.rz;
                float q2 = dx * dx + dz * dz;
                if (q2 < 1f) g = Mathf.Max(g, b.c.y + b.h * Mathf.Sqrt(1f - q2));
            }
            return g;
        }
    }
}
