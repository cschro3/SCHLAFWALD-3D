using System.Collections;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    public struct Shot
    {
        public Vector3 position, lookAt; public float fov, duration;

        public static Shot From(ShotConfig c)
        {
            return new Shot { position = new Vector3(c.position.x, c.position.y, c.position.z), lookAt = new Vector3(c.lookAt.x, c.lookAt.y, c.lookAt.z), fov = c.fov, duration = c.duration };
        }
    }

    /// <summary>
    /// Ruhige Kamera: ein "Rig" gleitet entlang gebogener Wege, die echte Kamera folgt ihm kritisch gedämpft (SmoothDamp) –
    /// dadurch bleibt jede Bewegung weich, auch wenn Flüge unterbrochen werden. Keine hektischen Bewegungen, nur mikroskopisches Atmen.
    /// </summary>
    public class CameraDirector : MonoBehaviour
    {
        public Camera Cam { get; private set; }
        Vector3 rigPos, rigLook, camVel, lookVel; float rigFov, fovVel;
        Vector3 curLook;
        Coroutine flight;
        public bool IsFlying { get; private set; }

        public void Init(Camera cam, Shot start)
        {
            Cam = cam;
            rigPos = start.position; rigLook = curLook = start.lookAt; rigFov = start.fov;
            cam.transform.position = rigPos;
            cam.transform.rotation = Quaternion.LookRotation(rigLook - rigPos);
            cam.fieldOfView = AspectFov.VerticalFov(rigFov, cam.aspect);
            cam.nearClipPlane = 0.05f;
        }

        public Shot Current { get { return new Shot { position = rigPos, lookAt = rigLook, fov = rigFov }; } }

        /// <summary>Startet einen Flug im Hintergrund (ein laufender Flug wird weich abgelöst).</summary>
        public void StartFlight(Shot target, float duration)
        {
            if (flight != null) StopCoroutine(flight);
            flight = StartCoroutine(Fly(target, Mathf.Max(0.5f, duration)));
        }

        public IEnumerator FlyTo(Shot target, float duration)
        {
            StartFlight(target, duration);
            while (IsFlying) yield return null;
        }

        IEnumerator Fly(Shot target, float duration)
        {
            IsFlying = true;
            Vector3 p0 = rigPos, l0 = rigLook; float f0 = rigFov;
            Vector3 p1 = target.position;
            Vector3 mid = (p0 + p1) * 0.5f;
            Vector3 ctrl = mid;
            if (ForestTerrain.Path != null && Vector3.Distance(p0, p1) > 4f)
            {
                Vector3 onPath = ForestTerrain.Path.PointAtZ(mid.z) + Vector3.up * 1.9f;
                ctrl = Vector3.Lerp(mid, onPath, 0.8f);
            }
            Vector3 fwd = Vector3.forward;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration); float e = k * k * (3f - 2f * k);
                Vector3 a = Vector3.Lerp(p0, ctrl, e), b = Vector3.Lerp(ctrl, p1, e);
                rigPos = Vector3.Lerp(a, b, e);
                float lead = Mathf.Sin(Mathf.PI * e) * Mathf.Min(5f, Vector3.Distance(p0, p1) * 0.18f);
                rigLook = Vector3.Lerp(l0, target.lookAt, e) + fwd * lead;
                rigFov = Mathf.Lerp(f0, target.fov, e);
                yield return null;
            }
            rigPos = p1; rigLook = target.lookAt; rigFov = target.fov;
            IsFlying = false; flight = null;
        }

        void LateUpdate()
        {
            if (Cam == null) return;
            float dt = Time.deltaTime; if (dt <= 0f) return;
            Vector3 breathe = new Vector3(Mathf.PerlinNoise(Time.time * 0.11f, 1f) - 0.5f, Mathf.PerlinNoise(2f, Time.time * 0.09f) - 0.5f, 0f) * 0.05f;
            Vector3 pos = Vector3.SmoothDamp(Cam.transform.position, rigPos + breathe, ref camVel, 0.55f, 12f, dt);
            curLook = Vector3.SmoothDamp(curLook, rigLook, ref lookVel, 0.45f, 30f, dt);
            Cam.transform.position = pos;
            Vector3 dir = curLook - pos;
            if (dir.sqrMagnitude > 1e-4f) Cam.transform.rotation = Quaternion.LookRotation(dir);
            float fov = Mathf.SmoothDamp(Cam.fieldOfView, AspectFov.VerticalFov(rigFov, Cam.aspect), ref fovVel, 0.5f, 60f, dt);
            Cam.fieldOfView = fov;
        }

        /// <summary>Standard-Kameras relativ zum Schlafplatz, falls story_de.json keine eigenen Shots definiert.</summary>
        public static Shot DefaultShot(AnimalConfig a, string kind)
        {
            Vector3 spot = new Vector3(a.sleepSpot.position.x, a.sleepSpot.position.y, a.sleepSpot.position.z);
            float toPath = -Mathf.Sign(spot.x == 0f ? 1f : spot.x);
            switch (kind)
            {
                case "approach": return new Shot { position = spot + new Vector3(toPath * 4.6f, 2.0f, -4.2f), lookAt = spot + Vector3.up * 0.4f, fov = 50f, duration = 9f };
                case "sleep": return new Shot { position = spot + new Vector3(toPath * 1.5f, 0.8f, -1.5f), lookAt = spot + Vector3.up * 0.35f, fov = 42f, duration = 7f };
                default: return new Shot { position = spot + new Vector3(toPath * 2.3f, 1.1f, -2.4f), lookAt = spot + Vector3.up * 0.3f, fov = 50f, duration = 8f };
            }
        }

        public static Shot ShotFor(AnimalConfig a, string kind)
        {
            var c = a.FindShot(kind);
            return c != null ? Shot.From(c) : DefaultShot(a, kind);
        }
    }
}
