using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>Catmull-Rom-Kurve mit Bogenlängen-Tabelle: gleichmäßiges Tempo für Kamera, Schnecke und Pfad-Hilfen.</summary>
    public class CurvePath
    {
        readonly Vector3[] pts;
        readonly float[] cum;
        public float Length { get; private set; }

        public CurvePath(IList<Vector3> control, int samplesPerSegment = 16)
        {
            var list = new List<Vector3>();
            int n = control.Count;
            for (int i = 0; i < n - 1; i++)
            {
                Vector3 p0 = control[Mathf.Max(i - 1, 0)], p1 = control[i], p2 = control[i + 1], p3 = control[Mathf.Min(i + 2, n - 1)];
                for (int s = 0; s < samplesPerSegment; s++) list.Add(CatmullRom(p0, p1, p2, p3, s / (float)samplesPerSegment));
            }
            list.Add(control[n - 1]);
            pts = list.ToArray();
            cum = new float[pts.Length];
            for (int i = 1; i < pts.Length; i++) cum[i] = cum[i - 1] + Vector3.Distance(pts[i - 1], pts[i]);
            Length = cum[pts.Length - 1];
        }

        public static CurvePath FromVec3(Vec3[] v, int samples = 16)
        {
            var l = new List<Vector3>();
            foreach (var p in v) l.Add(new Vector3(p.x, p.y, p.z));
            return new CurvePath(l, samples);
        }

        static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        public Vector3 PointAt(float dist)
        {
            dist = Mathf.Clamp(dist, 0f, Length);
            int lo = 0, hi = pts.Length - 1;
            while (hi - lo > 1) { int mid = (lo + hi) / 2; if (cum[mid] <= dist) lo = mid; else hi = mid; }
            float seg = cum[hi] - cum[lo];
            float k = seg > 1e-6f ? (dist - cum[lo]) / seg : 0f;
            return Vector3.Lerp(pts[lo], pts[hi], k);
        }

        public Vector3 TangentAt(float dist)
        {
            float a = Mathf.Max(0f, dist - 0.05f), b = Mathf.Min(Length, dist + 0.05f);
            Vector3 d = PointAt(b) - PointAt(a);
            return d.sqrMagnitude > 1e-8f ? d.normalized : Vector3.forward;
        }

        /// <summary>Kleinster XZ-Abstand zur Kurve.</summary>
        public float DistanceXZ(float x, float z)
        {
            float best = float.MaxValue;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                float ax = pts[i].x, az = pts[i].z, bx = pts[i + 1].x, bz = pts[i + 1].z;
                float dx = bx - ax, dz = bz - az;
                float len2 = dx * dx + dz * dz;
                float t = len2 > 1e-8f ? Mathf.Clamp01(((x - ax) * dx + (z - az) * dz) / len2) : 0f;
                float px = ax + dx * t - x, pz = az + dz * t - z;
                float d2 = px * px + pz * pz;
                if (d2 < best) best = d2;
            }
            return Mathf.Sqrt(best);
        }

        /// <summary>Punkt der Kurve bei gegebenem Z (Kurve verläuft in +Z).</summary>
        public Vector3 PointAtZ(float z)
        {
            if (z <= pts[0].z) return pts[0];
            for (int i = 0; i < pts.Length - 1; i++)
                if (pts[i + 1].z >= z)
                {
                    float dz = pts[i + 1].z - pts[i].z;
                    float k = dz > 1e-6f ? (z - pts[i].z) / dz : 0f;
                    return Vector3.Lerp(pts[i], pts[i + 1], k);
                }
            return pts[pts.Length - 1];
        }
    }
}
