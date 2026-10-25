using UnityEngine;

namespace Schlafwald
{
    /// <summary>Baut die greifbaren 3D-Objekte der Rituale (Blatt, Decke, Eichel, Mooskissen, Glühwürmchen, Leitlicht).</summary>
    public class PropFactory
    {
        readonly Material leafMat, softMat, glowSolid;
        readonly Material haloMat;

        public PropFactory()
        {
            leafMat = MaterialLibrary.Lit(Color.white, rim: 0.4f, fur: 0.1f, sway: 0.03f, doubleSided: true);
            softMat = MaterialLibrary.Lit(Color.white, rim: 0.35f, fur: 0.35f, doubleSided: true);
            glowSolid = MaterialLibrary.Lit(new Color(1f, 0.95f, 0.7f), rim: 0f, emission: new Color(1f, 0.9f, 0.5f));
            haloMat = MaterialLibrary.Glow(ProceduralTextures.SoftDot(), Color.white, 1.0f, true);
        }

        public GameObject Create(string prop, Vector3 position)
        {
            var go = new GameObject("Prop_" + prop);
            go.transform.position = position;
            var mb = new MeshBuilder();
            Material mat = leafMat;
            switch (prop)
            {
                case "bigLeaf":
                    mb.AddLeaf(Matrix4x4.TRS(new Vector3(0, 0, -0.75f), Quaternion.identity, Vector3.one), 1.5f, 1.15f,
                        MeshBuilder.C(0.34f, 0.62f, 0.36f), MeshBuilder.C(0.52f, 0.76f, 0.42f), -0.20f, 0.10f, 8, 12);
                    break;
                case "leaf":
                    mb.AddLeaf(Matrix4x4.TRS(new Vector3(0, 0, -0.2f), Quaternion.identity, Vector3.one), 0.4f, 0.26f,
                        MeshBuilder.C(0.46f, 0.70f, 0.34f), MeshBuilder.C(0.60f, 0.80f, 0.40f), 0.2f, 0.15f, 5, 8);
                    break;
                case "blanket":
                    mb.AddCloth(Matrix4x4.identity, 0.9f, 0.7f, MeshBuilder.C(0.96f, 0.64f, 0.56f), MeshBuilder.C(0.99f, 0.84f, 0.62f));
                    mat = softMat; break;
                case "acorn":
                    mb.AddEllipsoid(new Vector3(0, 0.07f, 0), new Vector3(0.065f, 0.085f, 0.065f), MeshBuilder.C(0.72f, 0.52f, 0.30f), MeshBuilder.C(0.86f, 0.66f, 0.38f), 12, 9);
                    mb.AddEllipsoid(new Vector3(0, 0.125f, 0), new Vector3(0.072f, 0.046f, 0.072f), MeshBuilder.C(0.46f, 0.32f, 0.22f), MeshBuilder.C(0.56f, 0.40f, 0.28f), 12, 8);
                    mb.AddStem(new Vector3(0, 0.16f, 0), Quaternion.identity, 0.03f, 0.008f, 0.006f, MeshBuilder.C(0.4f, 0.3f, 0.2f), MeshBuilder.C(0.4f, 0.3f, 0.2f), 5, true);
                    mat = softMat; break;
                case "mossPillow":
                    mb.AddEllipsoid(new Vector3(0, 0.07f, 0), new Vector3(0.30f, 0.10f, 0.22f), MeshBuilder.C(0.34f, 0.54f, 0.30f), MeshBuilder.C(0.54f, 0.76f, 0.42f), 14, 9);
                    mat = softMat; break;
                case "firefly":
                    mb.AddEllipsoid(Vector3.zero, new Vector3(0.035f, 0.045f, 0.035f), MeshBuilder.C(1f, 0.95f, 0.6f), MeshBuilder.C(1f, 1f, 0.8f), 10, 8);
                    mat = glowSolid; break;
                case "guideOrb":
                    mb.AddEllipsoid(Vector3.zero, new Vector3(0.045f, 0.045f, 0.045f), MeshBuilder.C(1f, 0.95f, 0.6f), MeshBuilder.C(1f, 1f, 0.85f), 10, 8);
                    mat = glowSolid; break;
                default:
                    mb.AddEllipsoid(Vector3.zero, new Vector3(0.05f, 0.05f, 0.05f), MeshBuilder.C(1, 1, 1), 8, 6);
                    mat = softMat; break;
            }
            go.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("SW_Prop_" + prop);
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            return go;
        }

        /// <summary>Weiches, pulsierendes Leuchten – zeigt Kleinkindern ohne Text, wohin sie fassen sollen.</summary>
        public SoftPulse AddHalo(Transform parent, Vector3 localPos, float size, float alpha = 0.45f, float emphasis = 0.3f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = "Halo";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.AddComponent<Billboard>();
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = haloMat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            var pulse = go.AddComponent<SoftPulse>();
            pulse.haloRenderer = r; pulse.baseScale = size; pulse.baseAlpha = alpha; pulse.emphasis = emphasis;
            return pulse;
        }

        public GameObject Sparkle(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = "TouchSparkle";
            go.transform.position = pos; go.transform.localScale = Vector3.one * 0.16f;
            go.AddComponent<Billboard>();
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = haloMat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            var f = go.AddComponent<FadeAndDie>(); f.duration = 0.9f; f.startAlpha = 0.5f; f.tint = new Color(1f, 0.95f, 0.7f);
            return go;
        }
    }
}
