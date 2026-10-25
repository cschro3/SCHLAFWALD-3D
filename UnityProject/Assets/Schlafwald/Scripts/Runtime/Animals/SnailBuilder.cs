using System;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>
    /// 3D-Prototyp von Suri, der Schnecke: segmentierter Fuß (für die Kriechwelle), Hals/Kopf mit großen Augen,
    /// Fühler mit je zwei Gelenken, kleine Unterfühler, Spiral-Schneckenhaus als echtes Röhren-Mesh.
    /// ALLES hier ist ein ersetzbarer Platzhalter für ein Blender-Modell mit gleichen Teilenamen.
    /// </summary>
    public static class SnailBuilder
    {
        public const int FootSegments = 6;

        static readonly Color Skin = new Color(0.96f, 0.83f, 0.72f);
        static readonly Color SkinDark = new Color(0.90f, 0.72f, 0.62f);

        public static AnimalRig Build(Transform parent)
        {
            var root = new GameObject("Suri");
            root.transform.SetParent(parent, false);
            var rig = root.AddComponent<AnimalRig>();
            var skinMat = MaterialLibrary.Lit(Color.white, rim: 0.40f, fur: 0.12f, rimColor: new Color(1f, 0.9f, 0.85f));
            var shellMat = MaterialLibrary.Lit(Color.white, rim: 0.35f, fur: 0.16f, rimColor: new Color(1f, 0.92f, 0.85f));
            var eyeMat = MaterialLibrary.Lit(new Color(0.09f, 0.07f, 0.11f), rim: 0.6f, rimColor: new Color(0.7f, 0.75f, 1f));
            var sparkMat = MaterialLibrary.Lit(Color.white, rim: 0f, emission: new Color(0.9f, 0.9f, 0.9f));
            var cheekMat = MaterialLibrary.Lit(new Color(1f, 0.62f, 0.66f), rim: 0.1f);
            var darkMat = MaterialLibrary.Lit(new Color(0.35f, 0.18f, 0.24f), rim: 0f);

            var body = Part(rig, "Body", root.transform, Vector3.zero, Quaternion.identity);

            // Fuß: sechs weiche Segmente von vorn nach hinten
            float[] rx = { 0.075f, 0.085f, 0.085f, 0.078f, 0.065f, 0.045f };
            float[] ry = { 0.045f, 0.052f, 0.055f, 0.050f, 0.042f, 0.030f };
            for (int i = 0; i < FootSegments; i++)
            {
                var t = Part(rig, "Foot" + i, body, new Vector3(0, ry[i] * 0.92f, 0.17f - i * 0.085f), Quaternion.identity);
                Attach(t, skinMat, Blob(new Vector3(rx[i], ry[i], 0.105f), SkinDark, Skin));
            }

            // Mantel: Körperhügel unter dem Haus – füllt die Hausöffnung, solange Suri draußen ist
            var mantle = Part(rig, "Mantle", body, new Vector3(0, 0.085f, -0.045f), Quaternion.identity);
            Attach(mantle, skinMat, Blob(new Vector3(0.078f, 0.085f, 0.105f), SkinDark, Skin));

            // Hals und Kopf
            var neck = Part(rig, "Neck", body, new Vector3(0, 0.095f, 0.20f), Quaternion.Euler(18, 0, 0));
            Attach(neck, skinMat, Blob(new Vector3(0.060f, 0.095f, 0.060f), Skin, Skin));
            var head = Part(rig, "Head", body, new Vector3(0, 0.155f, 0.245f), Quaternion.identity);
            Attach(head, skinMat, Blob(new Vector3(0.088f, 0.082f, 0.088f), Skin, new Color(1f, 0.90f, 0.82f)));

            // Augen (groß, freundlich) – Schließen = vertikales Stauchen; Glanzpunkt wandert mit
            foreach (int side in new[] { -1, 1 })
            {
                string s = side < 0 ? "L" : "R";
                var eye = Part(rig, "Eye" + s, head, new Vector3(side * 0.041f, 0.020f, 0.060f), Quaternion.identity);
                Attach(eye, eyeMat, Blob(new Vector3(0.027f, 0.030f, 0.020f), new Color(1, 1, 1), new Color(1, 1, 1)));
                var spark = Part(null, "Spark" + s, eye, new Vector3(side * -0.007f, 0.012f, 0.016f), Quaternion.identity);
                Attach(spark, sparkMat, Blob(new Vector3(0.0085f, 0.0085f, 0.006f), Color.white, Color.white, 8, 6));
                var spark2 = Part(null, "SparkB" + s, eye, new Vector3(side * 0.008f, -0.011f, 0.017f), Quaternion.identity);
                Attach(spark2, sparkMat, Blob(new Vector3(0.0045f, 0.0045f, 0.0035f), Color.white, Color.white, 6, 4));
                var cheek = Part(rig, "Cheek" + s, head, new Vector3(side * 0.066f, -0.020f, 0.050f), Quaternion.identity);
                Attach(cheek, cheekMat, Blob(new Vector3(0.022f, 0.014f, 0.010f), Color.white, Color.white, 8, 6));
            }
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.010f;
                var m = Part(null, "Smile" + i, head, new Vector3(x, -0.034f - (2 - Mathf.Abs(i - 2)) * 0.0045f + 0.009f, 0.083f), Quaternion.identity);
                Attach(m, darkMat, Blob(new Vector3(0.0042f, 0.0042f, 0.0042f), Color.white, Color.white, 6, 4));
            }

            // Fühler: zwei Gelenke + Kugelspitze
            foreach (int side in new[] { -1, 1 })
            {
                string s = side < 0 ? "L" : "R";
                var f1 = Part(rig, "Feeler" + s, head, new Vector3(side * 0.040f, 0.065f, -0.010f), Quaternion.Euler(-14, 0, -side * 20));
                var mb = new MeshBuilder();
                mb.AddStem(Vector3.zero, Quaternion.identity, 0.075f, 0.013f, 0.010f, MeshBuilder.C(Skin), MeshBuilder.C(Skin), 8, false);
                Attach(f1, skinMat, mb.ToMesh("SW_Feeler1"));
                var f2 = Part(rig, "Feeler" + s + "2", f1, new Vector3(0, 0.075f, 0), Quaternion.Euler(-12, 0, -side * 10));
                var mb2 = new MeshBuilder();
                mb2.AddStem(Vector3.zero, Quaternion.identity, 0.062f, 0.010f, 0.0085f, MeshBuilder.C(Skin), MeshBuilder.C(Skin), 8, false);
                mb2.AddEllipsoid(new Vector3(0, 0.068f, 0), new Vector3(0.019f, 0.019f, 0.019f), MeshBuilder.C(0.98f, 0.72f, 0.68f), MeshBuilder.C(1f, 0.82f, 0.76f), 10, 8);
                Attach(f2, skinMat, mb2.ToMesh("SW_Feeler2"));
            }

            // Schneckenhaus
            var shellPivot = Part(rig, "Shell", body, new Vector3(0, 0.20f, -0.07f), Quaternion.Euler(-6, 0, 0));
            Attach(shellPivot, shellMat, SpiralShell());
            var plug = Part(rig, "ShellOpening", shellPivot, new Vector3(0, -0.095f, 0.012f), Quaternion.identity);
            Attach(plug, darkMat, Blob(new Vector3(0.078f, 0.050f, 0.010f), new Color(0.30f, 0.15f, 0.20f), new Color(0.38f, 0.20f, 0.26f), 12, 8));

            rig.CaptureRest();
            return rig;
        }

        /// <summary>Logarithmische Spirale (2,75 Windungen), Röhrenradius wächst mit – Windungen berühren sich.</summary>
        public static Mesh SpiralShell()
        {
            const int n = 110;
            float thetaStart = Mathf.PI, thetaEnd = 5.5f * Mathf.PI;
            float b = 0.1255f, rEnd = 0.095f;
            var centers = new Vector3[n]; var radii = new float[n];
            for (int i = 0; i < n; i++)
            {
                float th = Mathf.Lerp(thetaStart, thetaEnd, i / (float)(n - 1));
                float r = rEnd * Mathf.Exp(b * (th - thetaEnd));
                centers[i] = new Vector3(0f, r * Mathf.Sin(th), r * Mathf.Cos(th));
                radii[i] = 0.60f * r;
            }
            var mb = new MeshBuilder();
            mb.AddTube(centers, radii, 14, Matrix4x4.Scale(new Vector3(1.6f, 1f, 1f)), MeshBuilder.C(0.80f, 0.52f, 0.62f), MeshBuilder.C(0.99f, 0.78f, 0.62f));
            return mb.ToMesh("SW_SuriShell");
        }

        // ---------- Helfer ----------
        static Transform Part(AnimalRig rig, string name, Transform parent, Vector3 pos, Quaternion rot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localRotation = rot;
            if (rig != null) rig.Register(name, go.transform);
            return go.transform;
        }

        static Mesh Blob(Vector3 radii, Color bottom, Color top, int seg = 14, int rings = 10)
        {
            var mb = new MeshBuilder();
            mb.AddEllipsoid(Vector3.zero, radii, MeshBuilder.C(bottom), MeshBuilder.C(top), seg, rings);
            return mb.ToMesh("SW_Blob");
        }

        static void Attach(Transform t, Material mat, Mesh mesh)
        {
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = t.gameObject.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
        }
    }
}
