using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Baut den zusammenhängenden Märchenwald prozedural aus eigenen, echten 3D-Meshes
    /// (weiche runde Bäume, Wurzeln, bemooste Steine, Pilze, Blumen, Farne, Gräser, Baumhäuser, Höhlen, Nebelbänke).
    /// Alles liegt in Segmenten, die nach Kameraabstand aktiviert werden (Akku/Draw-Calls).
    /// </summary>
    public class ForestBuilder
    {
        public Transform Root { get; private set; }
        public readonly List<Renderer> FogBanks = new List<Renderer>();
        public CurvePath Path { get; private set; }

        System.Random rnd;
        const float SegmentLength = 20f;
        const float ZMin = -34f, ZMax = 110f;
        Transform[] segments;
        Vector3[] segCenters;
        readonly List<Vector3> exclusions = new List<Vector3>();

        Material matFoliage, matGround, matRock, matGrass, matMushroomGlow, matWindow, matWood;
        Mesh[] trees, rocks, grasses, ferns, flowers, mushrooms, glowMushrooms;

        float R() { return (float)rnd.NextDouble(); }
        float R(float a, float b) { return a + (b - a) * R(); }

        public static Color32 Col(float r, float g, float b, float sway = 0f) { return MeshBuilder.C(r, g, b, sway); }

        public void Build(StoryData story, Transform parent)
        {
            rnd = new System.Random(20241);
            Root = new GameObject("Forest").transform;
            Root.SetParent(parent, false);
            Path = CurvePath.FromVec3(story.world.path, 20);
            ForestTerrain.Reset(Path);
            foreach (var a in story.animals) if (a.sleepSpot != null)
            {
                var sp = new Vector3(a.sleepSpot.position.x, 0f, a.sleepSpot.position.z);
                ForestTerrain.AddClearing(sp);
                Spots.Add(sp);
            }

            int segCount = Mathf.CeilToInt((ZMax - ZMin) / SegmentLength);
            segments = new Transform[segCount]; segCenters = new Vector3[segCount];
            for (int i = 0; i < segCount; i++)
            {
                var g = new GameObject("Segment_" + i);
                g.transform.SetParent(Root, false);
                segments[i] = g.transform;
                segCenters[i] = new Vector3(0f, 0f, ZMin + (i + 0.5f) * SegmentLength);
            }

            CreateMaterials();
            CreateMeshes();
            BuildTerrain();
            BuildHouses();
            BuildTrees();
            BuildRocks();
            BuildGroundCover();
            BuildFogBanks();

            var culler = Root.gameObject.AddComponent<SegmentCuller>();
            culler.segments = segments; culler.centers = segCenters; culler.radius = 62f;
        }

        // ---------- Materialien ----------
        void CreateMaterials()
        {
            matFoliage = MaterialLibrary.Lit(Color.white, rim: 0.38f, fur: 0.25f, sway: 0.10f);
            matGround = MaterialLibrary.Lit(Color.white, rim: 0f, fur: 0.18f);
            matRock = MaterialLibrary.Lit(Color.white, rim: 0.22f, fur: 0.30f);
            matGrass = MaterialLibrary.Lit(Color.white, rim: 0.30f, fur: 0.1f, sway: 0.28f, doubleSided: true);
            matMushroomGlow = MaterialLibrary.Lit(Color.white, rim: 0.3f, fur: 0.1f, emission: new Color(0.10f, 0.22f, 0.26f));
            matWindow = MaterialLibrary.Lit(new Color(1f, 0.82f, 0.45f), rim: 0f, emission: new Color(1.0f, 0.70f, 0.30f));
            matWood = MaterialLibrary.Lit(Color.white, rim: 0.3f, fur: 0.2f);
        }

        // ---------- Mesh-Varianten ----------
        void CreateMeshes()
        {
            trees = new Mesh[6]; for (int i = 0; i < trees.Length; i++) trees[i] = MakeTree(i);
            rocks = new Mesh[4]; for (int i = 0; i < rocks.Length; i++) rocks[i] = MakeRock(i, 1f);
            grasses = new Mesh[3]; for (int i = 0; i < grasses.Length; i++) grasses[i] = MakeGrass(i);
            ferns = new Mesh[2]; for (int i = 0; i < ferns.Length; i++) ferns[i] = MakeFern(i);
            flowers = new Mesh[3]; for (int i = 0; i < flowers.Length; i++) flowers[i] = MakeFlowers(i);
            mushrooms = new Mesh[3]; glowMushrooms = new Mesh[2];
            for (int i = 0; i < mushrooms.Length; i++) mushrooms[i] = MakeMushrooms(i, false);
            for (int i = 0; i < glowMushrooms.Length; i++) glowMushrooms[i] = MakeMushrooms(i + 10, true);
        }

        System.Func<Vector3, Vector3> Bumpy(float amp, float freq, float seed)
        {
            return p => p * (1f + amp * (Mathf.PerlinNoise(p.x * freq + seed, p.y * freq + p.z * freq * 0.7f + seed * 1.3f) - 0.5f));
        }

        Mesh MakeTree(int v)
        {
            var rr = new System.Random(900 + v * 17);
            System.Func<float> r = () => (float)rr.NextDouble();
            var mb = new MeshBuilder();
            float h = 2.6f + r() * 1.2f, rad = 0.30f + r() * 0.12f;
            float lean = (r() - 0.5f) * 0.12f;
            Color32 trunkLo = Col(0.40f, 0.30f, 0.30f), trunkHi = Col(0.52f, 0.40f, 0.36f);
            var rot = Quaternion.Euler(lean * 57f, 0, lean * 30f);
            mb.AddStem(Vector3.zero, rot, h, rad * 1.15f, rad * 0.62f, trunkLo, trunkHi, 9);
            // Wurzeln: runde, nach außen laufende Wülste
            int roots = 5;
            for (int i = 0; i < roots; i++)
            {
                float a = (i / (float)roots + r() * 0.1f) * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                mb.AddEllipsoid(dir * rad * 1.35f + Vector3.up * 0.07f, new Vector3(rad * 0.95f, rad * 0.30f, rad * 0.30f), trunkLo, trunkHi, 8, 6, Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up)));
            }
            // Krone aus weichen Blobs
            float[][] greens = {
                new[]{0.30f,0.52f,0.36f, 0.45f,0.68f,0.42f}, new[]{0.26f,0.48f,0.42f, 0.40f,0.64f,0.50f},
                new[]{0.34f,0.50f,0.30f, 0.52f,0.70f,0.38f}, new[]{0.28f,0.44f,0.40f, 0.42f,0.60f,0.48f},
                new[]{0.38f,0.54f,0.34f, 0.58f,0.72f,0.44f}, new[]{0.24f,0.46f,0.36f, 0.38f,0.62f,0.44f} };
            var g = greens[v % greens.Length];
            int blobs = 4 + (int)(r() * 3);
            for (int i = 0; i < blobs; i++)
            {
                float a = r() * Mathf.PI * 2f, d = (i == 0 ? 0f : 0.7f + r() * 0.7f);
                Vector3 c = new Vector3(Mathf.Cos(a) * d, h + 0.5f + r() * 1.0f + (i == 0 ? 0.5f : 0f), Mathf.Sin(a) * d);
                float s = 1.0f + r() * 0.7f;
                mb.AddEllipsoid(c, new Vector3(s * 1.05f, s * 0.85f, s * 1.05f),
                    Col(g[0] * 0.8f, g[1] * 0.85f, g[2] * 0.9f, 0.5f), Col(g[3], g[4], g[5], 0.8f), 14, 10, null, Bumpy(0.25f, 1.7f, v * 7f + i));
            }
            return mb.ToMesh("SW_Tree" + v);
        }

        Mesh MakeRock(int v, float moss)
        {
            var mb = new MeshBuilder();
            var rr = new System.Random(300 + v);
            float sx = 0.7f + (float)rr.NextDouble() * 0.5f, sy = 0.5f + (float)rr.NextDouble() * 0.3f, sz = 0.6f + (float)rr.NextDouble() * 0.4f;
            mb.AddEllipsoid(Vector3.zero, new Vector3(sx, sy, sz), Col(0.48f, 0.50f, 0.58f), Col(0.38f, 0.58f, 0.34f), 16, 12, null, Bumpy(0.16f, 2.4f, v * 3f));
            for (int i = 0; i < 4; i++)
            {
                float a = (float)rr.NextDouble() * 6.28f, d = (float)rr.NextDouble() * 0.5f;
                mb.AddEllipsoid(new Vector3(Mathf.Cos(a) * sx * d, sy * (0.78f - d * 0.5f), Mathf.Sin(a) * sz * d), new Vector3(0.17f, 0.07f, 0.17f), Col(0.44f, 0.66f, 0.36f), Col(0.58f, 0.78f, 0.44f), 8, 6);
            }
            return mb.ToMesh("SW_Rock" + v);
        }

        Mesh MakeGrass(int v)
        {
            var mb = new MeshBuilder(); var rr = new System.Random(500 + v);
            int blades = 9;
            for (int i = 0; i < blades; i++)
            {
                float a = (float)rr.NextDouble() * Mathf.PI * 2f, d = (float)rr.NextDouble() * 0.12f;
                var m = Matrix4x4.TRS(new Vector3(Mathf.Cos(a) * d, 0, Mathf.Sin(a) * d), Quaternion.Euler(-62f + (float)rr.NextDouble() * 20f, a * 57.3f, 0), Vector3.one);
                float len = 0.35f + (float)rr.NextDouble() * 0.3f;
                mb.AddLeaf(m, len, 0.07f, Col(0.42f, 0.66f, 0.38f, 1f), Col(0.30f, 0.52f, 0.30f, 1f), 0.1f, 0.35f, 2, 6);
            }
            return mb.ToMesh("SW_Grass" + v);
        }

        Mesh MakeFern(int v)
        {
            var mb = new MeshBuilder(); var rr = new System.Random(700 + v);
            int fronds = 8;
            for (int i = 0; i < fronds; i++)
            {
                float a = i / (float)fronds * 360f + (float)rr.NextDouble() * 20f;
                var m = Matrix4x4.TRS(new Vector3(0, 0.02f, 0), Quaternion.Euler(-35f - (float)rr.NextDouble() * 15f, a, 0), Vector3.one);
                mb.AddLeaf(m, 0.8f + (float)rr.NextDouble() * 0.3f, 0.26f, Col(0.34f, 0.58f, 0.36f, 1f), Col(0.24f, 0.46f, 0.30f, 1f), 0.15f, 0.5f, 4, 8);
            }
            return mb.ToMesh("SW_Fern" + v);
        }

        Mesh MakeFlowers(int v)
        {
            var mb = new MeshBuilder(); var rr = new System.Random(1100 + v);
            Color[] petals = { new Color(1f, 0.78f, 0.84f), new Color(1f, 0.92f, 0.62f), new Color(0.82f, 0.78f, 1f), new Color(1f, 1f, 0.95f) };
            for (int k = 0; k < 4; k++)
            {
                var pc = petals[(v + k) % petals.Length];
                Vector3 b = new Vector3((float)(rr.NextDouble() - 0.5) * 0.35f, 0, (float)(rr.NextDouble() - 0.5) * 0.35f);
                float h = 0.18f + (float)rr.NextDouble() * 0.14f;
                mb.AddStem(b, Quaternion.identity, h, 0.012f, 0.009f, Col(0.30f, 0.52f, 0.30f, 0.2f), Col(0.36f, 0.58f, 0.32f, 0.6f), 5, false);
                for (int p = 0; p < 5; p++)
                {
                    float a = p / 5f * Mathf.PI * 2f;
                    mb.AddEllipsoid(b + new Vector3(Mathf.Cos(a) * 0.035f, h + 0.01f, Mathf.Sin(a) * 0.035f), new Vector3(0.03f, 0.012f, 0.03f), Col(pc.r, pc.g, pc.b, 0.7f), 6, 4);
                }
                mb.AddEllipsoid(b + Vector3.up * (h + 0.018f), new Vector3(0.02f, 0.016f, 0.02f), Col(1f, 0.86f, 0.4f, 0.7f), 6, 4);
            }
            return mb.ToMesh("SW_Flowers" + v);
        }

        Mesh MakeMushrooms(int v, bool glow)
        {
            var mb = new MeshBuilder(); var rr = new System.Random(1300 + v);
            Color[] caps = glow
                ? new[] { new Color(0.55f, 0.85f, 0.95f), new Color(0.72f, 0.65f, 1f) }
                : new[] { new Color(0.92f, 0.52f, 0.46f), new Color(0.95f, 0.72f, 0.45f), new Color(0.80f, 0.62f, 0.86f) };
            int count = 2 + rr.Next(3);
            for (int k = 0; k < count; k++)
            {
                var cc = caps[(v + k) % caps.Length];
                float s = 0.7f + (float)rr.NextDouble() * 0.7f;
                Vector3 b = new Vector3((float)(rr.NextDouble() - 0.5) * 0.4f, 0, (float)(rr.NextDouble() - 0.5) * 0.4f);
                float h = 0.12f * s;
                mb.AddStem(b, Quaternion.identity, h, 0.032f * s, 0.026f * s, Col(0.96f, 0.92f, 0.82f), Col(0.98f, 0.95f, 0.88f), 8, false);
                mb.AddEllipsoid(b + Vector3.up * h, new Vector3(0.10f * s, 0.075f * s, 0.10f * s), Col(0.96f, 0.90f, 0.80f), Col(cc.r, cc.g, cc.b), 12, 8);
                for (int d = 0; d < 4; d++)
                {
                    float a = (float)rr.NextDouble() * Mathf.PI * 2f, e = 0.25f + (float)rr.NextDouble() * 0.45f;
                    mb.AddEllipsoid(b + new Vector3(Mathf.Cos(a) * 0.1f * s * e, h + 0.072f * s * (1f - e * e * 0.5f), Mathf.Sin(a) * 0.1f * s * e), new Vector3(0.016f * s, 0.008f * s, 0.016f * s), Col(1f, 0.97f, 0.9f), 6, 4);
                }
            }
            return mb.ToMesh("SW_Mushrooms" + v);
        }

        // ---------- Platzieren ----------
        public GameObject Place(Mesh mesh, Material mat, Vector3 pos, Quaternion rot, Vector3 scale, string name, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : SegmentFor(pos.z), false);
            go.transform.SetPositionAndRotation(pos, rot);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            return go;
        }

        Transform SegmentFor(float z)
        {
            int i = Mathf.Clamp(Mathf.FloorToInt((z - ZMin) / SegmentLength), 0, segments.Length - 1);
            return segments[i];
        }

        Vector3 Ground(float x, float z) { return new Vector3(x, ForestTerrain.Ground(x, z), z); }

        public void PlaceRock(Vector3 pos, float scale) { Place(rocks[rnd.Next(rocks.Length)], matRock, pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "Rock"); }
        public void PlaceFern(Vector3 pos, float scale) { Place(ferns[rnd.Next(ferns.Length)], matGrass, pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "Fern"); }
        public void PlaceMushrooms(Vector3 pos, float scale, bool glow)
        {
            if (glow) Place(glowMushrooms[rnd.Next(glowMushrooms.Length)], matMushroomGlow, pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "GlowMushrooms");
            else Place(mushrooms[rnd.Next(mushrooms.Length)], matFoliage, pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "Mushrooms");
        }
        public void PlaceFlowers(Vector3 pos, float scale) { Place(flowers[rnd.Next(flowers.Length)], matGrass, pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "Flowers"); }
        public void PlaceGrass(Vector3 pos, float scale) { Place(grasses[rnd.Next(grasses.Length)], matGrass, pos, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "Grass"); }
        public Material WoodMaterial { get { return matWood; } }
        public Material RockMaterial { get { return matRock; } }
        public Material FoliageMaterial { get { return matFoliage; } }
        public Material GrassMaterial { get { return matGrass; } }

        float ClearingDistance(float x, float z)
        {
            float best = float.MaxValue;
            foreach (var e in exclusions) best = Mathf.Min(best, Mathf.Sqrt((x - e.x) * (x - e.x) + (z - e.z) * (z - e.z)) - e.y);
            return best;
        }

        // ---------- Aufbau ----------
        void BuildTerrain()
        {
            var mb = new MeshBuilder();
            mb.AddGrid(-54f, ZMin, 54f, ZMax, 68, 90, ForestTerrain.Ground, (x, y, z) =>
            {
                float d = Path.DistanceXZ(x, z);
                float n = Mathf.PerlinNoise(x * 0.12f + 5f, z * 0.12f + 9f);
                Color moss = Color.Lerp(new Color(0.22f, 0.40f, 0.30f), new Color(0.38f, 0.56f, 0.32f), n);
                moss = Color.Lerp(moss, new Color(0.28f, 0.42f, 0.44f), Mathf.SmoothStep(0f, 1f, (y - 1.5f) / 4f) * 0.5f);
                Color dirt = new Color(0.62f, 0.50f, 0.40f);
                Color c = Color.Lerp(dirt, moss, Mathf.SmoothStep(0.7f, 2.0f, d));
                return MeshBuilder.C(c, 0f);
            });
            var mesh = mb.ToMesh("SW_Terrain");
            Place(mesh, matGround, Vector3.zero, Quaternion.identity, Vector3.one, "Terrain", Root);
        }

        void BuildHouses()
        {
            // Baumhäuser mit warm leuchtendem Fenster – für die gemütliche Märchenstimmung
            Vector3[] spots = { new Vector3(6.2f, 0, 17f), new Vector3(-6.6f, 0, 37.5f), new Vector3(6.6f, 0, 58f) };
            for (int i = 0; i < spots.Length; i++)
            {
                var p = Ground(spots[i].x, spots[i].z);
                exclusions.Add(new Vector3(p.x, 0, p.z) + new Vector3(0, 3f, 0));
                var mb = new MeshBuilder();
                mb.AddStem(Vector3.zero, Quaternion.identity, 3.8f, 0.62f, 0.4f, Col(0.40f, 0.30f, 0.30f), Col(0.52f, 0.40f, 0.36f), 10);
                for (int k = 0; k < 5; k++)
                {
                    float a = k / 5f * Mathf.PI * 2f; var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                    mb.AddEllipsoid(dir * 0.8f + Vector3.up * 0.08f, new Vector3(0.6f, 0.2f, 0.2f), Col(0.40f, 0.30f, 0.30f), Col(0.5f, 0.38f, 0.34f), 8, 6, Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up)));
                }
                mb.AddEllipsoid(new Vector3(0, 4.6f, 0), new Vector3(2.2f, 1.5f, 2.2f), Col(0.28f, 0.50f, 0.38f, 0.5f), Col(0.46f, 0.68f, 0.44f, 0.8f), 14, 10, null, Bumpy(0.25f, 1.5f, i * 5f));
                // Häuschen am Stamm (Richtung Pfad)
                float side = p.x > 0 ? -1f : 1f;
                var hm = Matrix4x4.TRS(new Vector3(side * 0.85f, 2.2f, 0), Quaternion.identity, Vector3.one);
                mb.AddLathe(MeshBuilder.Profile(0.001f, -0.6f, 0.62f, -0.55f, 0.72f, -0.1f, 0.68f, 0.35f, 0.5f, 0.62f, 0.001f, 0.72f), 14, hm, Col(0.86f, 0.72f, 0.56f), Col(0.94f, 0.82f, 0.64f));
                mb.AddLathe(MeshBuilder.Profile(0.001f, 0.50f, 0.82f, 0.50f, 0.78f, 0.62f, 0.55f, 0.92f, 0.25f, 1.12f, 0.001f, 1.30f), 14, hm, Col(0.72f, 0.40f, 0.40f), Col(0.84f, 0.52f, 0.46f));
                Place(mb.ToMesh("SW_TreeHouse"), matFoliage, p, Quaternion.Euler(0, i * 40f, 0), Vector3.one, "TreeHouse");
                var gm = new MeshBuilder();
                gm.AddEllipsoid(new Vector3(side * (0.85f + 0.66f), 2.28f, 0), new Vector3(0.05f, 0.2f, 0.17f), Col(1f, 0.85f, 0.5f), 10, 8);
                Place(gm.ToMesh("SW_Window"), matWindow, p, Quaternion.Euler(0, i * 40f, 0), Vector3.one, "TreeHouseWindow");
            }
            // Tierhöhlen (Erdhügel mit gemütlichem Eingang)
            Vector3[] burrows = { new Vector3(3.4f, 0, 6f), new Vector3(-3.3f, 0, 28f), new Vector3(3.3f, 0, 47f), new Vector3(-3.2f, 0, 67f) };
            foreach (var b in burrows)
            {
                var p = Ground(b.x, b.z); exclusions.Add(new Vector3(p.x, 0, p.z) + new Vector3(0, 1.3f, 0));
                var mb = new MeshBuilder();
                mb.AddEllipsoid(Vector3.zero, new Vector3(1.0f, 0.62f, 0.9f), Col(0.42f, 0.36f, 0.32f), Col(0.38f, 0.58f, 0.34f), 14, 10, null, Bumpy(0.18f, 2f, b.z));
                float yaw = b.x > 0 ? -90f : 90f;
                var q = Quaternion.Euler(0, yaw, 0);
                Vector3 front = q * Vector3.forward;
                mb.AddEllipsoid(front * 0.86f + Vector3.up * 0.2f, new Vector3(0.3f, 0.28f, 0.14f), Col(0.12f, 0.09f, 0.12f), 10, 8, q);
                Place(mb.ToMesh("SW_Burrow"), matRock, p, Quaternion.identity, Vector3.one, "Burrow");
            }
        }

        void BuildTrees()
        {
            int placed = 0, tries = 0;
            while (placed < 210 && tries < 4000)
            {
                tries++;
                float x = R(-38f, 38f), z = R(ZMin + 4f, ZMax - 4f);
                float d = Path.DistanceXZ(x, z);
                if (d < 2.8f) continue;
                if (d < 7f && R() > 0.55f) continue;
                if (ClearingDistance(x, z) < 1.5f) continue;
                bool nearStation = false;
                foreach (var c in Spots) if (Mathf.Sqrt((x - c.x) * (x - c.x) + (z - c.z) * (z - c.z)) < 3.9f) { nearStation = true; break; }
                if (nearStation) continue;
                float scale = R(0.85f, 1.35f) * (1f + Mathf.SmoothStep(8f, 26f, d) * 0.9f);
                var p = Ground(x, z);
                Place(trees[rnd.Next(trees.Length)], matFoliage, p - Vector3.up * 0.05f, Quaternion.Euler(0, R(0, 360), 0), Vector3.one * scale, "Tree");
                placed++;
            }
        }

        public readonly List<Vector3> Spots = new List<Vector3>();

        void BuildRocks()
        {
            int placed = 0, tries = 0;
            while (placed < 46 && tries < 2000)
            {
                tries++;
                float z = R(ZMin + 6f, ZMax - 6f);
                float side = R() > 0.5f ? 1f : -1f;
                float x = Path.PointAtZ(z).x + side * R(2.6f, 12f);
                if (ClearingDistance(x, z) < 1f) continue;
                bool near = false;
                foreach (var c in Spots) if (Mathf.Sqrt((x - c.x) * (x - c.x) + (z - c.z) * (z - c.z)) < 3.4f) { near = true; break; }
                if (near) continue;
                PlaceRock(Ground(x, z) - Vector3.up * 0.1f, R(0.45f, 1.5f));
                placed++;
            }
        }

        void BuildGroundCover()
        {
            for (int i = 0; i < 300; i++)
            {
                float x = R(-30f, 30f), z = R(ZMin + 4f, ZMax - 4f);
                float d = Path.DistanceXZ(x, z);
                if (d < 1.1f) continue;
                PlaceGrass(Ground(x, z), R(0.8f, 1.5f));
            }
            for (int i = 0; i < 80; i++)
            {
                float z = R(ZMin + 4f, ZMax - 4f), side = R() > 0.5f ? 1f : -1f;
                float x = Path.PointAtZ(z).x + side * R(1.6f, 14f);
                PlaceFern(Ground(x, z), R(0.8f, 1.5f));
            }
            for (int i = 0; i < 60; i++)
            {
                float z = R(ZMin + 4f, ZMax - 4f), side = R() > 0.5f ? 1f : -1f;
                float x = Path.PointAtZ(z).x + side * R(1.5f, 9f);
                PlaceFlowers(Ground(x, z), R(0.9f, 1.6f));
            }
            for (int i = 0; i < 46; i++)
            {
                float z = R(ZMin + 4f, ZMax - 4f), side = R() > 0.5f ? 1f : -1f;
                float x = Path.PointAtZ(z).x + side * R(1.8f, 10f);
                PlaceMushrooms(Ground(x, z), R(0.9f, 1.8f), i % 3 == 0);
            }
        }

        void BuildFogBanks()
        {
            var tex = ProceduralTextures.FogBlob();
            for (int i = 0; i < 18; i++)
            {
                float z = ZMin + 8f + i * ((ZMax - ZMin - 16f) / 18f) + R(-3f, 3f);
                float side = i % 2 == 0 ? 1f : -1f;
                float x = Path.PointAtZ(z).x + side * R(4f, 12f);
                var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.Destroy(go.GetComponent<Collider>());
                go.name = "FogBank";
                go.transform.SetParent(SegmentFor(z), false);
                go.transform.position = new Vector3(x, R(0.7f, 1.7f), z);
                go.transform.localScale = new Vector3(R(10f, 17f), R(2.2f, 3.6f), 1f);
                go.AddComponent<Billboard>().yAxisOnly = true;
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = MaterialLibrary.Glow(tex, new Color(0.7f, 0.65f, 0.8f, 0.10f), 1f, false);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                FogBanks.Add(mr);
            }
        }
    }
}
