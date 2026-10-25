using UnityEngine;

namespace Schlafwald
{
    /// <summary>Richtet ein Objekt zur Kamera aus (Halos, Nebelbänke).</summary>
    public class Billboard : MonoBehaviour
    {
        public bool yAxisOnly;
        Transform cam;
        void LateUpdate()
        {
            if (cam == null) { var c = Camera.main; if (c == null) return; cam = c.transform; }
            Vector3 d = transform.position - cam.position;
            if (yAxisOnly) d.y = 0f;
            if (d.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(d);
        }
    }

    /// <summary>Sanftes Pulsieren eines Halos – führt Kinder ohne Text zum nächsten Ziel.</summary>
    public class SoftPulse : MonoBehaviour
    {
        public Renderer haloRenderer;
        public float baseScale = 0.5f;
        public float baseAlpha = 0.45f;
        public float speed = 1.6f;
        [Range(0f, 2f)] public float emphasis = 0.4f;   // 0 = dezent, 1 = Hinweis
        public Color tint = new Color(1f, 0.93f, 0.6f);
        MaterialPropertyBlock block;
        float phase;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Awake() { block = new MaterialPropertyBlock(); phase = Random.value * 6.28f; }

        void Update()
        {
            if (haloRenderer == null) return;
            float p = 0.5f + 0.5f * Mathf.Sin(Time.time * speed + phase);
            float s = baseScale * (1f + 0.12f * p + 0.25f * emphasis * p);
            haloRenderer.transform.localScale = new Vector3(s, s, s);
            float a = baseAlpha * (0.55f + 0.45f * p) * (0.6f + 0.8f * emphasis);
            haloRenderer.GetPropertyBlock(block);
            block.SetColor(ColorId, new Color(tint.r, tint.g, tint.b, Mathf.Clamp01(a)));
            haloRenderer.SetPropertyBlock(block);
        }
    }

    /// <summary>Lässt ein Objekt nach einer Zeit weich verblassen und zerstört es (Schleimspur, Funken).</summary>
    public class FadeAndDie : MonoBehaviour
    {
        public float delay = 0f, duration = 10f, startAlpha = 0.2f;
        public Color tint = Color.white;
        Renderer r; MaterialPropertyBlock block; float t;
        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Awake() { r = GetComponent<Renderer>(); block = new MaterialPropertyBlock(); }

        void Update()
        {
            t += Time.deltaTime;
            if (t < delay) return;
            float k = (t - delay) / Mathf.Max(0.01f, duration);
            if (k >= 1f) { Destroy(gameObject); return; }
            if (r == null) return;
            r.GetPropertyBlock(block);
            block.SetColor(ColorId, new Color(tint.r, tint.g, tint.b, startAlpha * (1f - k)));
            r.SetPropertyBlock(block);
        }
    }

    /// <summary>Folgt der Kamera in X/Z (Himmelskuppel, Partikel-Volumen).</summary>
    public class FollowCamera : MonoBehaviour
    {
        public bool followY = true;
        public Vector3 offset;
        void LateUpdate()
        {
            var c = Camera.main; if (c == null) return;
            Vector3 p = c.transform.position + offset;
            if (!followY) p.y = offset.y;
            transform.position = p;
        }
    }

    /// <summary>Schaltet Wald-Segmente nach Kameraabstand (Draw-Calls & Akku).</summary>
    public class SegmentCuller : MonoBehaviour
    {
        public Transform[] segments;
        public Vector3[] centers;
        public float radius = 55f;
        float next;

        void Update()
        {
            if (Time.unscaledTime < next) return;
            next = Time.unscaledTime + 0.4f;
            var c = Camera.main; if (c == null) return;
            Vector3 p = c.transform.position;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null) continue;
                bool on = Mathf.Abs(centers[i].z - p.z) < radius;
                if (segments[i].gameObject.activeSelf != on) segments[i].gameObject.SetActive(on);
            }
        }
    }
}
