using System;
using System.Collections.Generic;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>
    /// Baut die echten 3D-Prototyp-Modelle prozedural (Rotationskörper, Röhren, Blätter) mit Vertexfarben.
    /// Vertex-Alpha = Wind-Gewicht (siehe StylizedLit.shader). Diese Meshes sind die ersetzbaren Platzhalter (docs/ASSETS_ZU_ERSETZEN.md).
    /// </summary>
    public class MeshBuilder
    {
        readonly List<Vector3> verts = new List<Vector3>();
        readonly List<Color32> cols = new List<Color32>();
        readonly List<int> tris = new List<int>();

        public static Color32 C(float r, float g, float b, float sway = 0f)
        {
            return new Color32((byte)(Mathf.Clamp01(r) * 255), (byte)(Mathf.Clamp01(g) * 255), (byte)(Mathf.Clamp01(b) * 255), (byte)(Mathf.Clamp01(sway) * 255));
        }

        public static Color32 C(Color c, float sway = 0f) { return C(c.r, c.g, c.b, sway); }

        // ---------- Profile ----------
        public static Vector2[] SphereProfile(int rings = 10)
        {
            var p = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float a = Mathf.PI * i / rings;
                p[i] = new Vector2(Mathf.Sin(a), -Mathf.Cos(a));
            }
            return p;
        }

        /// <summary>Radius/Höhe-Paare von unten nach oben.</summary>
        public static Vector2[] Profile(params float[] rh)
        {
            var p = new Vector2[rh.Length / 2];
            for (int i = 0; i < p.Length; i++) p[i] = new Vector2(rh[i * 2], rh[i * 2 + 1]);
            return p;
        }

        // ---------- Primitive ----------
        public void AddLathe(Vector2[] profile, int segments, Matrix4x4 m, Color32 colorStart, Color32 colorEnd, Func<Vector3, Vector3> deform = null)
        {
            int rings = profile.Length;
            int baseIndex = verts.Count;
            for (int r = 0; r < rings; r++)
            {
                Color32 col = Color32.Lerp(colorStart, colorEnd, rings > 1 ? r / (float)(rings - 1) : 0f);
                for (int s = 0; s < segments; s++)
                {
                    float a = s / (float)segments * Mathf.PI * 2f;
                    var p = new Vector3(Mathf.Cos(a) * profile[r].x, profile[r].y, Mathf.Sin(a) * profile[r].x);
                    if (deform != null) p = deform(p);
                    verts.Add(m.MultiplyPoint3x4(p));
                    cols.Add(col);
                }
            }
            for (int r = 0; r < rings - 1; r++)
                for (int s = 0; s < segments; s++)
                {
                    int s1 = (s + 1) % segments;
                    int a = baseIndex + r * segments + s, b = baseIndex + r * segments + s1;
                    int c = baseIndex + (r + 1) * segments + s, d = baseIndex + (r + 1) * segments + s1;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
        }

        public void AddEllipsoid(Vector3 center, Vector3 radii, Color32 col, int segments = 14, int rings = 10, Quaternion? rot = null, Func<Vector3, Vector3> deform = null)
        {
            AddEllipsoid(center, radii, col, col, segments, rings, rot, deform);
        }

        public void AddEllipsoid(Vector3 center, Vector3 radii, Color32 bottom, Color32 top, int segments = 14, int rings = 10, Quaternion? rot = null, Func<Vector3, Vector3> deform = null)
        {
            var m = Matrix4x4.TRS(center, rot ?? Quaternion.identity, radii);
            AddLathe(SphereProfile(rings), segments, m, bottom, top, deform);
        }

        /// <summary>Verjüngter Stiel/Zylinder entlang Y mit weicher Rundung an beiden Enden.</summary>
        public void AddStem(Vector3 basePos, Quaternion rot, float height, float r0, float r1, Color32 c0, Color32 c1, int segments = 8, bool roundTop = true)
        {
            var prof = new List<Vector2> { new Vector2(0.001f, 0f), new Vector2(r0, 0.0f), new Vector2(r0 * 0.98f, height * 0.05f) };
            const int steps = 5;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                prof.Add(new Vector2(Mathf.Lerp(r0, r1, t), height * Mathf.Lerp(0.05f, 1f, t)));
            }
            if (roundTop) prof.Add(new Vector2(0.001f, height + r1 * 0.5f));
            AddLathe(prof.ToArray(), segments, Matrix4x4.TRS(basePos, rot, Vector3.one), c0, c1);
        }

        /// <summary>Röhre entlang einer Mittellinie (Schneckenhaus, Wurzeln, Äste), Radius je Stützpunkt.</summary>
        public void AddTube(Vector3[] centers, float[] radii, int segments, Matrix4x4 m, Color32 c0, Color32 c1)
        {
            int n = centers.Length, baseIndex = verts.Count;
            Vector3 prevT = (centers[1] - centers[0]).normalized;
            Vector3 normal = Vector3.Cross(prevT, Mathf.Abs(prevT.y) < 0.9f ? Vector3.up : Vector3.right).normalized;
            for (int i = 0; i < n; i++)
            {
                Vector3 t = i < n - 1 ? (centers[i + 1] - centers[i]).normalized : (centers[i] - centers[i - 1]).normalized;
                // Parallel-Transport: Normale mit der Tangente mitdrehen
                Vector3 axis = Vector3.Cross(prevT, t);
                if (axis.sqrMagnitude > 1e-8f) normal = Quaternion.AngleAxis(Vector3.Angle(prevT, t), axis.normalized) * normal;
                normal = (normal - Vector3.Dot(normal, t) * t).normalized;
                Vector3 binormal = Vector3.Cross(t, normal);
                prevT = t;
                Color32 col = Color32.Lerp(c0, c1, i / (float)(n - 1));
                for (int s = 0; s < segments; s++)
                {
                    float a = s / (float)segments * Mathf.PI * 2f;
                    Vector3 p = centers[i] + (normal * Mathf.Cos(a) + binormal * Mathf.Sin(a)) * radii[i];
                    verts.Add(m.MultiplyPoint3x4(p));
                    cols.Add(col);
                }
            }
            for (int i = 0; i < n - 1; i++)
                for (int s = 0; s < segments; s++)
                {
                    int s1 = (s + 1) % segments;
                    int a = baseIndex + i * segments + s, b = baseIndex + i * segments + s1;
                    int c = baseIndex + (i + 1) * segments + s, d = baseIndex + (i + 1) * segments + s1;
                    // Windung relativ zur Parallel-Transport-Orientierung (binormal = t × normal) ist rechtshändig → (a,b,c)
                    tris.Add(a); tris.Add(b); tris.Add(c);
                    tris.Add(b); tris.Add(d); tris.Add(c);
                }
        }

        /// <summary>Gebogenes Blatt (zweiseitig zu rendern). Länge entlang +Z, Breite entlang X, Wölbung nach oben (+Y).</summary>
        public void AddLeaf(Matrix4x4 m, float length, float width, Color32 edge, Color32 rib, float cup = 0.25f, float droop = 0.15f, int nx = 6, int ny = 10)
        {
            int baseIndex = verts.Count;
            for (int j = 0; j <= ny; j++)
            {
                float t = j / (float)ny;
                float w = width * 0.5f * Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.85f)), 0.75f);
                for (int i = 0; i <= nx; i++)
                {
                    float u = i / (float)nx * 2f - 1f;
                    float x = u * w;
                    float y = cup * width * u * u * 0.5f * Mathf.Sin(Mathf.PI * t) - droop * length * t * t;
                    verts.Add(m.MultiplyPoint3x4(new Vector3(x, y, t * length)));
                    Color32 lc = Color32.Lerp(rib, edge, Mathf.Abs(u));
                    lc.a = (byte)(lc.a * t); // Wind-Gewicht wächst zur Blattspitze
                    cols.Add(lc);
                }
            }
            for (int j = 0; j < ny; j++)
                for (int i = 0; i < nx; i++)
                {
                    int a = baseIndex + j * (nx + 1) + i, b = a + 1, c = a + (nx + 1), d = c + 1;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
        }

        /// <summary>Weiche, wellige Stoffbahn (Decke) in der XZ-Ebene.</summary>
        public void AddCloth(Matrix4x4 m, float sizeX, float sizeZ, Color32 body, Color32 border, int n = 10)
        {
            int baseIndex = verts.Count;
            for (int j = 0; j <= n; j++)
                for (int i = 0; i <= n; i++)
                {
                    float u = i / (float)n * 2f - 1f, v = j / (float)n * 2f - 1f;
                    float y = 0.03f * Mathf.Sin(u * 3.1f + v * 2.2f) + 0.02f * Mathf.Cos(v * 4f) - 0.04f * (u * u + v * v);
                    verts.Add(m.MultiplyPoint3x4(new Vector3(u * sizeX * 0.5f, y, v * sizeZ * 0.5f)));
                    float edge = Mathf.Max(Mathf.Abs(u), Mathf.Abs(v));
                    cols.Add(Color32.Lerp(body, border, Mathf.SmoothStep(0.75f, 1f, edge)));
                }
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    int a = baseIndex + j * (n + 1) + i, b = a + 1, c = a + (n + 1), d = c + 1;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
        }

        public void AddQuad(Matrix4x4 m, float w, float h, Color32 col)
        {
            int b = verts.Count;
            verts.Add(m.MultiplyPoint3x4(new Vector3(-w / 2, -h / 2, 0))); verts.Add(m.MultiplyPoint3x4(new Vector3(-w / 2, h / 2, 0)));
            verts.Add(m.MultiplyPoint3x4(new Vector3(w / 2, h / 2, 0))); verts.Add(m.MultiplyPoint3x4(new Vector3(w / 2, -h / 2, 0)));
            for (int i = 0; i < 4; i++) cols.Add(col);
            tris.Add(b); tris.Add(b + 1); tris.Add(b + 2); tris.Add(b); tris.Add(b + 2); tris.Add(b + 3);
        }

        /// <summary>Terrain-/Fächer-Gitter aus Höhenfunktion.</summary>
        public void AddGrid(float x0, float z0, float x1, float z1, int nx, int nz, Func<float, float, float> height, Func<float, float, float, Color32> color)
        {
            int baseIndex = verts.Count;
            for (int j = 0; j <= nz; j++)
                for (int i = 0; i <= nx; i++)
                {
                    float x = Mathf.Lerp(x0, x1, i / (float)nx), z = Mathf.Lerp(z0, z1, j / (float)nz);
                    float y = height(x, z);
                    verts.Add(new Vector3(x, y, z));
                    cols.Add(color(x, y, z));
                }
            for (int j = 0; j < nz; j++)
                for (int i = 0; i < nx; i++)
                {
                    int a = baseIndex + j * (nx + 1) + i, b = a + 1, c = a + (nx + 1), d = c + 1;
                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
        }

        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
            if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetColors(cols);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public int VertexCount { get { return verts.Count; } }
    }
}
