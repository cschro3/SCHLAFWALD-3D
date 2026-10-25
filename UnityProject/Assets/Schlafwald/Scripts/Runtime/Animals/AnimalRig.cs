using System.Collections.Generic;
using UnityEngine;

namespace Schlafwald
{
    /// <summary>
    /// Benannte Körperteile eines Tiers + Ruhepose. Die prozeduralen Prototyp-Modelle registrieren ihre Teile hier;
    /// ein später importiertes Blender-Modell kann dieselben Namen tragen (siehe docs/ASSETS_ZU_ERSETZEN.md),
    /// dann steuert dieselbe Performance-Logik die finalen Meshes.
    /// </summary>
    public class AnimalRig : MonoBehaviour
    {
        public struct Pose { public Vector3 position; public Quaternion rotation; public Vector3 scale; }

        readonly Dictionary<string, Transform> parts = new Dictionary<string, Transform>();
        readonly Dictionary<string, Pose> rest = new Dictionary<string, Pose>();

        public void Register(string name, Transform t) { parts[name] = t; }
        public Transform Get(string name) { Transform t; return parts.TryGetValue(name, out t) ? t : null; }
        public bool Has(string name) { return parts.ContainsKey(name); }

        public void CaptureRest()
        {
            foreach (var kv in parts)
                rest[kv.Key] = new Pose { position = kv.Value.localPosition, rotation = kv.Value.localRotation, scale = kv.Value.localScale };
        }

        public Pose Rest(string name) { Pose p; return rest.TryGetValue(name, out p) ? p : new Pose { rotation = Quaternion.identity, scale = Vector3.one }; }
    }
}
