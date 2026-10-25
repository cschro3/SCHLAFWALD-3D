using UnityEngine;

namespace Schlafwald
{
    /// <summary>Gemütliche Schlafplätze der Tiere. Phase 2: Suristeine mit Blätterdach-Stützen; weitere Habitate folgen mit Phase 3.</summary>
    public static class HabitatBuilder
    {
        public struct SuriLayout
        {
            public Vector3 stoneA, stoneB;
            public Vector3 stoneARadii, stoneBRadii;
        }

        public static SuriLayout BuildSuri(ForestBuilder forest, Vector3 sleepSpot, Vector3 pathStart)
        {
            var layout = new SuriLayout
            {
                stoneA = new Vector3(pathStart.x, 0f, pathStart.z),
                stoneARadii = new Vector3(0.5f, 0.32f, 0.45f),
                stoneB = new Vector3(sleepSpot.x, 0f, sleepSpot.z),
                stoneBRadii = new Vector3(0.8f, 0.38f, 0.7f),
            };
            var parent = new GameObject("Habitat_Suri").transform;
            parent.SetParent(forest.Root, false);

            // Höhenfeld: Schnecke und Blätterdach orientieren sich an den Steinen
            ForestTerrain.AddBump(layout.stoneA, layout.stoneARadii.x, layout.stoneARadii.z, layout.stoneARadii.y);
            ForestTerrain.AddBump(layout.stoneB, layout.stoneBRadii.x, layout.stoneBRadii.z, layout.stoneBRadii.y);

            MakeStone(forest, layout.stoneA, layout.stoneARadii, 1, parent);
            MakeStone(forest, layout.stoneB, layout.stoneBRadii, 2, parent);

            // Vier Zweige tragen später das große Blatt (Blätterdach)
            var mb = new MeshBuilder();
            Vector3[] posts = { new Vector3(-0.55f, 0, -0.42f), new Vector3(0.55f, 0, -0.42f), new Vector3(-0.55f, 0, 0.42f), new Vector3(0.55f, 0, 0.42f) };
            foreach (var p in posts)
            {
                float y = ForestTerrain.Height(layout.stoneB.x + p.x, layout.stoneB.z + p.z);
                mb.AddStem(new Vector3(p.x, y, p.z), Quaternion.Euler(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)), 0.90f - y, 0.032f, 0.022f,
                    MeshBuilder.C(0.40f, 0.30f, 0.26f), MeshBuilder.C(0.52f, 0.40f, 0.32f), 6);
            }
            var go = forest.Place(mb.ToMesh("SW_SuriPosts"), forest.WoodMaterial, layout.stoneB, Quaternion.identity, Vector3.one, "RoofPosts", parent);
            go.transform.position = new Vector3(layout.stoneB.x, 0, layout.stoneB.z);

            // Dekoration rund um den Schlafplatz
            forest.PlaceFern(Ground(sleepSpot, -0.6f, 0.9f), 1.2f);
            forest.PlaceFern(Ground(sleepSpot, 1.1f, 0.8f), 1.0f);
            forest.PlaceMushrooms(Ground(sleepSpot, -0.15f, 1.0f), 1.3f, false);
            forest.PlaceMushrooms(Ground(sleepSpot, 1.15f, -0.85f), 1.0f, true);
            forest.PlaceFlowers(Ground(sleepSpot, -1.5f, 0.0f), 1.4f);
            forest.PlaceFlowers(Ground(sleepSpot, 1.7f, 0.5f), 1.2f);
            forest.PlaceGrass(Ground(sleepSpot, -1.3f, -0.5f), 1.4f);
            forest.PlaceGrass(Ground(sleepSpot, 0.2f, -1.2f), 1.2f);
            return layout;
        }

        static Vector3 Ground(Vector3 spot, float dx, float dz)
        {
            float x = spot.x + dx, z = spot.z + dz;
            return new Vector3(x, ForestTerrain.Ground(x, z), z);
        }

        static void MakeStone(ForestBuilder forest, Vector3 center, Vector3 radii, int seed, Transform parent)
        {
            var mb = new MeshBuilder();
            mb.AddEllipsoid(Vector3.zero, radii, MeshBuilder.C(0.50f, 0.50f, 0.58f), MeshBuilder.C(0.38f, 0.60f, 0.34f), 20, 14, null,
                p => p * (1f + 0.07f * (Mathf.PerlinNoise(p.x * 2.5f + seed * 7f, p.z * 2.5f + p.y * 1.7f) - 0.5f)));
            for (int i = 0; i < 7; i++)
            {
                float a = i * 0.9f + seed, d = 0.15f + 0.1f * ((i * 37) % 5);
                float sx = Mathf.Cos(a) * radii.x * d, sz = Mathf.Sin(a) * radii.z * d;
                float q2 = (sx / radii.x) * (sx / radii.x) + (sz / radii.z) * (sz / radii.z);
                mb.AddEllipsoid(new Vector3(sx, radii.y * Mathf.Sqrt(Mathf.Max(0f, 1f - q2)) - 0.02f, sz), new Vector3(0.14f, 0.055f, 0.14f),
                    MeshBuilder.C(0.40f, 0.64f, 0.34f), MeshBuilder.C(0.56f, 0.80f, 0.42f), 8, 6);
            }
            forest.Place(mb.ToMesh("SW_Stone" + seed), forest.RockMaterial, center, Quaternion.identity, Vector3.one, "MossyStone" + seed, parent);
        }
    }
}
