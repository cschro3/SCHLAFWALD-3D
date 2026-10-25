using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    public class StepContext
    {
        public AppServices S;
        public Camera Cam;
        public AnimalController Animal;
        public int Index;
        public StepConfig Config;
        public Transform Root;
    }

    /// <summary>
    /// Ein wiederverwendbarer Interaktionsschritt. Kinder können nichts falsch machen: Fehlgriffe bewirken nichts Negatives,
    /// ausbleibende Eingaben lösen Hinweise und schließlich sanfte Selbsthilfe des Tiers aus (siehe HelpEscalator).
    /// </summary>
    public abstract class InteractionStep
    {
        protected StepContext ctx;
        protected readonly List<SoftPulse> pulses = new List<SoftPulse>();
        public bool Completed { get; protected set; }
        public float Progress { get; protected set; }
        bool progressed;

        public virtual void Begin(StepContext c) { ctx = c; }
        public abstract void Tick(float dt);
        public abstract void BeginAssist();
        public virtual IEnumerator Finish() { yield break; }
        public virtual void Cleanup() { foreach (var p in pulses) if (p != null) Object.Destroy(p.gameObject); pulses.Clear(); }
        /// <summary>Weltpositionen dessen, was gerade berührt/gegriffen/verfolgt werden soll (Hinweise, Tests).</summary>
        public abstract IEnumerable<Vector3> ActiveTargets();

        protected void MarkProgress() { progressed = true; }
        public bool ConsumeProgress() { bool p = progressed; progressed = false; return p; }
        public void SetEmphasis(float e) { foreach (var p in pulses) if (p != null) p.emphasis = e; }

        protected Vector3 TargetPoint { get { var t = ctx.Config.target; return new Vector3(t.x, t.y, t.z); } }
        protected static Vector3 V(Vec3 v) { return new Vector3(v.x, v.y, v.z); }
        protected void Sfx(string key, Vector3 pos, float vol = 0.5f) { if (ctx.S.Audio != null) ctx.S.Audio.PlaySfx(key, pos, vol); }
    }

    // ------------------------------------------------------------------------------------------------
    /// <summary>Das Tier folgt dem Leitlicht entlang eines Pfads, solange der Finger das Licht sanft begleitet (Suri).</summary>
    public class FollowPathStep : InteractionStep
    {
        CurvePath path;
        float s;                 // zurückgelegte Strecke
        GameObject orb;
        SoftPulse orbHalo;
        bool assist;
        const float Speed = 0.12f;      // m/s – Suri ist "besonders langsam"
        const float Lead = 0.45f;       // Das Licht wartet etwas voraus
        const float EndEps = 0.03f;

        public override void Begin(StepContext c)
        {
            base.Begin(c);
            var pts = new List<Vector3>();
            foreach (var p in c.Config.points) pts.Add(new Vector3(p.x, 0f, p.z));
            path = new CurvePath(pts, 12);
            orb = c.S.Props.Create("guideOrb", OrbPos());
            orb.transform.SetParent(c.Root, true);
            orbHalo = c.S.Props.AddHalo(orb.transform, Vector3.zero, 0.55f, 0.5f, 0.4f);
            pulses.Add(orbHalo);
        }

        Vector3 OnPath(float d)
        {
            Vector3 p = path.PointAt(d);
            p.y = ForestTerrain.Height(p.x, p.z);
            return p;
        }

        Vector3 OrbPos() { return OnPath(Mathf.Min(path.Length, s + Lead)) + Vector3.up * 0.16f; }

        public override IEnumerable<Vector3> ActiveTargets() { if (orb != null) yield return orb.transform.position; }

        public override void Tick(float dt)
        {
            if (Completed) return;
            bool moving = assist;
            if (!assist && PointerInput.Down && PointerInput.IsNear(ctx.Cam, orb.transform.position, ctx.Config.radius, PointerInput.Position))
            {
                moving = true; MarkProgress();
            }
            if (moving) s = Mathf.Min(path.Length, s + Speed * (assist ? 1.15f : 1f) * dt);
            Progress = path.Length > 0f ? s / path.Length : 1f;
            orb.transform.position = Vector3.Lerp(orb.transform.position, OrbPos(), 1f - Mathf.Exp(-6f * dt));
            Vector3 pos = OnPath(s);
            ctx.Animal.Performance.OnStepProgress(ctx.Index, ctx.Config, Progress, pos);
            if (s >= path.Length - EndEps) Completed = true;
        }

        public override void BeginAssist() { assist = true; }

        public override IEnumerator Finish()
        {
            // Das Licht verglimmt weich, Suri bleibt an ihrem Schlafplatz stehen
            float t = 0f; var r = orbHalo != null ? orbHalo.haloRenderer : null;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                if (orb != null) orb.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, t / 1.2f);
                yield return null;
            }
        }

        public override void Cleanup() { base.Cleanup(); if (orb != null) Object.Destroy(orb); }
    }

    // ------------------------------------------------------------------------------------------------
    /// <summary>Objekte (Decke, Blätter, Eichel, Kissen) per Drag-and-drop zum Ziel bewegen. Loslassen "daneben" führt sie sanft zurück.</summary>
    public class DragToTargetStep : InteractionStep
    {
        enum State { Idle, Dragging, Returning, Placing, Assisted }
        class Item
        {
            public GameObject go; public Vector3 start; public Quaternion startRot; public Vector3 goal; public Quaternion goalRot;
            public State state; public float t, dur; public Vector3 from; public Quaternion fromRot; public Vector3 vel; public bool placed;
            public SoftPulse halo;
        }
        readonly List<Item> items = new List<Item>();
        Item dragging; Plane dragPlane; Vector3 dragOffset;
        SoftPulse targetHalo;
        bool assisting; float assistTimer;

        public override void Begin(StepContext c)
        {
            base.Begin(c);
            int n = c.Config.points.Length;
            for (int i = 0; i < n; i++)
            {
                var p = V(c.Config.points[i]);
                var go = c.S.Props.Create(c.Config.prop, p);
                go.transform.SetParent(c.Root, true);
                go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                var it = new Item { go = go, start = p, startRot = go.transform.rotation, goal = GoalFor(i, n), state = State.Idle };
                it.goalRot = c.Config.prop == "bigLeaf" ? Quaternion.Euler(5f, 25f, -4f) : Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                it.halo = c.S.Props.AddHalo(go.transform, Vector3.up * 0.1f, Mathf.Max(0.5f, c.Config.radius * 0.9f), 0.4f, 0.3f);
                pulses.Add(it.halo);
                items.Add(it);
            }
            targetHalo = c.S.Props.AddHalo(c.Root, TargetPoint, c.Config.radius * 1.5f, 0.0f, 0.0f);
            targetHalo.baseAlpha = 0f; pulses.Add(targetHalo);
        }

        Vector3 GoalFor(int i, int n)
        {
            Vector3 t = TargetPoint;
            if (n > 1) t += new Vector3((i % 3 - 1) * 0.16f, 0.04f * (i / 3), ((i / 3) % 2 == 0 ? 1 : -1) * 0.06f);
            return t;
        }

        public override IEnumerable<Vector3> ActiveTargets()
        {
            foreach (var it in items) if (!it.placed) yield return it.go.transform.position;
        }

        public override void Tick(float dt)
        {
            if (Completed) return;
            Vector3 camPos = ctx.Cam.transform.position;

            if (!assisting && PointerInput.Pressed && dragging == null)
            {
                Item best = null; float bestD = float.MaxValue;
                foreach (var it in items)
                {
                    if (it.placed || it.state == State.Placing) continue;
                    Vector3 sp = ctx.Cam.WorldToScreenPoint(it.go.transform.position);
                    if (sp.z <= 0f) continue;
                    float d = Vector2.Distance(new Vector2(sp.x, sp.y), PointerInput.Position);
                    if (d <= PointerInput.TouchRadiusPx(ctx.Cam, it.go.transform.position, ctx.Config.radius) && d < bestD) { best = it; bestD = d; }
                }
                if (best != null)
                {
                    dragging = best; best.state = State.Dragging; best.vel = Vector3.zero;
                    dragPlane = new Plane(-ctx.Cam.transform.forward, best.go.transform.position);
                    dragOffset = Vector3.zero;
                    if (targetHalo != null) targetHalo.baseAlpha = 0.5f;
                    Sfx("rustle", best.go.transform.position, 0.3f);
                }
            }

            if (dragging != null)
            {
                if (PointerInput.Down)
                {
                    Ray ray = ctx.Cam.ScreenPointToRay(PointerInput.Position);
                    float enter;
                    if (dragPlane.Raycast(ray, out enter))
                    {
                        Vector3 want = ray.GetPoint(enter);
                        float gy = ForestTerrain.Height(want.x, want.z) + 0.08f;
                        if (want.y < gy) want.y = gy;
                        float dTarget = Vector3.Distance(want, TargetPoint);
                        if (dTarget < ctx.Config.radius * 1.2f) want = Vector3.Lerp(want, TargetPoint, 0.45f); // sanfter Magnet
                        dragging.go.transform.position = Vector3.SmoothDamp(dragging.go.transform.position, want, ref dragging.vel, 0.07f, 20f, dt);
                        dragging.go.transform.rotation = Quaternion.Slerp(dragging.go.transform.rotation, dTarget < ctx.Config.radius * 1.8f ? dragging.goalRot : dragging.startRot, 1f - Mathf.Exp(-4f * dt));
                        if (PointerInput.Delta.sqrMagnitude > 0.5f) MarkProgress();
                    }
                }
                if (PointerInput.Released || !PointerInput.Down)
                {
                    var it = dragging; dragging = null;
                    float dist = Vector3.Distance(it.go.transform.position, TargetPoint);
                    if (dist < ctx.Config.radius * 1.5f) StartPlace(it, 1.3f);
                    else StartReturn(it);
                    if (targetHalo != null) targetHalo.baseAlpha = 0f;
                }
            }

            // Assist: nächstes unplatziertes Objekt fliegt sanft zum Ziel
            if (assisting)
            {
                assistTimer -= dt;
                bool anyBusy = false;
                foreach (var it in items) if (it.state == State.Assisted || it.state == State.Placing || it.state == State.Returning) anyBusy = true;
                if (!anyBusy && assistTimer <= 0f)
                    foreach (var it in items) if (!it.placed) { StartPlace(it, 2.4f, State.Assisted); assistTimer = 0.6f; break; }
            }

            int placed = 0;
            foreach (var it in items)
            {
                AnimateItem(it, dt);
                if (it.placed) placed++;
            }
            Progress = items.Count > 0 ? placed / (float)items.Count : 1f;
            if (placed == items.Count && !Completed)
            {
                Completed = true;
                ctx.Animal.Performance.OnStepProgress(ctx.Index, ctx.Config, 1f, TargetPoint);
            }
        }

        void StartPlace(Item it, float dur, State st = State.Placing)
        {
            it.state = st; it.t = 0f; it.dur = dur; it.from = it.go.transform.position; it.fromRot = it.go.transform.rotation;
            MarkProgress();
        }

        void StartReturn(Item it)
        {
            it.state = State.Returning; it.t = 0f; it.dur = 1.4f; it.from = it.go.transform.position; it.fromRot = it.go.transform.rotation;
        }

        void AnimateItem(Item it, float dt)
        {
            if (it.state != State.Placing && it.state != State.Returning && it.state != State.Assisted) return;
            it.t += dt;
            float k = Mathf.Clamp01(it.t / it.dur); k = k * k * (3f - 2f * k);
            Vector3 to = it.state == State.Returning ? it.start : it.goal;
            Quaternion toRot = it.state == State.Returning ? it.startRot : it.goalRot;
            Vector3 p = Vector3.Lerp(it.from, to, k);
            if (it.state == State.Assisted) p += Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.35f;       // sanfter Bogen
            it.go.transform.position = p;
            it.go.transform.rotation = Quaternion.Slerp(it.fromRot, toRot, k);
            if (it.t >= it.dur)
            {
                if (it.state == State.Returning) it.state = State.Idle;
                else
                {
                    it.state = State.Idle; it.placed = true;
                    if (it.halo != null) { Object.Destroy(it.halo.gameObject); pulses.Remove(it.halo); }
                    Sfx("tap", it.go.transform.position, 0.45f);
                    ctx.Animal.Performance.OnStepProgress(ctx.Index, ctx.Config, Progress, it.go.transform.position);
                }
            }
        }

        public override void BeginAssist() { assisting = true; assistTimer = 0.3f; if (dragging != null) { var it = dragging; dragging = null; StartPlace(it, 2.0f, State.Assisted); } }

        public override IEnumerator Finish() { yield return new WaitForSeconds(0.9f); }

        /// <summary>Gelegte Objekte bleiben als Teil der Szene stehen; nur Hilfen werden entfernt.</summary>
        public override void Cleanup() { base.Cleanup(); }

        /// <summary>Beim Fortsetzen einer Nacht: Objekte direkt am Ziel erzeugen.</summary>
        public static void SpawnCompleted(AppServices s, StepConfig cfg, Transform root)
        {
            int n = cfg.points.Length;
            for (int i = 0; i < n; i++)
            {
                Vector3 t = new Vector3(cfg.target.x, cfg.target.y, cfg.target.z);
                if (n > 1) t += new Vector3((i % 3 - 1) * 0.16f, 0.04f * (i / 3), ((i / 3) % 2 == 0 ? 1 : -1) * 0.06f);
                var go = s.Props.Create(cfg.prop, t);
                go.transform.SetParent(root, true);
                go.transform.rotation = cfg.prop == "bigLeaf" ? Quaternion.Euler(5f, 25f, -4f) : Quaternion.Euler(0f, i * 70f, 0f);
            }
        }
    }

    // ------------------------------------------------------------------------------------------------
    /// <summary>Ein oder mehrere Ziele sanft berühren/antippen (Suris Häuschen, Lunas Glühwürmchen).</summary>
    public class TouchStep : InteractionStep
    {
        class Target { public Vector3 pos; public float hold; public bool done; public GameObject go; public SoftPulse halo; }
        readonly List<Target> targets = new List<Target>();
        bool assist; float assistTimer;

        public override void Begin(StepContext c)
        {
            base.Begin(c);
            foreach (var p in c.Config.points)
            {
                var t = new Target { pos = V(p) };
                if (!string.IsNullOrEmpty(c.Config.prop))
                {
                    t.go = c.S.Props.Create(c.Config.prop, t.pos);
                    t.go.transform.SetParent(c.Root, true);
                }
                t.halo = c.S.Props.AddHalo(c.Root, t.pos, Mathf.Max(0.5f, c.Config.radius * 0.8f), 0.4f, 0.3f);
                pulses.Add(t.halo);
                targets.Add(t);
            }
        }

        public override IEnumerable<Vector3> ActiveTargets() { foreach (var t in targets) if (!t.done) yield return t.pos; }

        public override void Tick(float dt)
        {
            if (Completed) return;
            bool touchedAny = false;
            if (PointerInput.Down && !assist)
                foreach (var t in targets)
                {
                    if (t.done) continue;
                    if (PointerInput.IsNear(ctx.Cam, t.pos, ctx.Config.radius, PointerInput.Position))
                    {
                        t.hold += dt; touchedAny = true; MarkProgress();
                        ctx.Animal.Performance.Touched(t.pos, 0.6f);
                        break; // ein Finger = ein Ziel zur Zeit
                    }
                }
            if (assist)
            {
                assistTimer += dt;
                foreach (var t in targets) if (!t.done) { t.hold += dt * (ctx.Config.holdSeconds / 1.6f); break; }
            }

            int done = 0;
            foreach (var t in targets)
            {
                float k = Mathf.Clamp01(t.hold / Mathf.Max(0.05f, ctx.Config.holdSeconds));
                if (!t.done && k >= 1f)
                {
                    t.done = true; Sfx("twinkle", t.pos, 0.5f);
                    if (t.halo != null) t.halo.baseAlpha *= 0.4f; // Licht wird sanfter
                }
                if (t.halo != null && !t.done) t.halo.emphasis = Mathf.Max(t.halo.emphasis, k);
                if (t.go != null) t.go.transform.localScale = Vector3.one * (t.done ? 0.8f : 1f + 0.2f * k);
                if (t.done) done++;
            }
            Progress = targets.Count > 0 ? done / (float)targets.Count : 1f;
            ctx.Animal.Performance.OnStepProgress(ctx.Index, ctx.Config, Progress, targets.Count > 0 ? targets[0].pos : Vector3.zero);
            if (done == targets.Count) Completed = true;
            if (!touchedAny && !assist) { /* kein Fortschritt – keine Strafe */ }
        }

        public override void BeginAssist() { assist = true; }
        public override IEnumerator Finish() { yield return new WaitForSeconds(0.7f); }
        public override void Cleanup() { base.Cleanup(); foreach (var t in targets) if (t.go != null && ctx.Config.prop == "guideOrb") Object.Destroy(t.go); }
    }

    // ------------------------------------------------------------------------------------------------
    /// <summary>Streicheln: Fingerbewegung über dem Tier sammelt "Streichel-Strecke" (Mila, Leni).</summary>
    public class StrokeStep : InteractionStep
    {
        float accum; bool assist; Vector3 focus; SoftPulse halo; float lastReact;

        public override void Begin(StepContext c)
        {
            base.Begin(c);
            focus = V(c.Config.target);
            halo = c.S.Props.AddHalo(c.Root, focus, Mathf.Max(0.6f, c.Config.radius), 0.35f, 0.3f);
            pulses.Add(halo);
        }

        public override IEnumerable<Vector3> ActiveTargets() { yield return focus; }

        public override void Tick(float dt)
        {
            if (Completed) return;
            float need = ctx.Config.strokeLength * Screen.height;
            if (!assist && PointerInput.Down && PointerInput.IsNear(ctx.Cam, focus, ctx.Config.radius * 1.4f, PointerInput.Position))
            {
                float d = PointerInput.Delta.magnitude;
                if (d > 1f) { accum += Mathf.Min(d, Screen.height * 0.05f); MarkProgress(); }
                if (Time.time - lastReact > 0.35f && d > 1f) { lastReact = Time.time; ctx.Animal.Performance.Touched(focus, Mathf.Clamp01(d / 20f)); }
            }
            if (assist) accum += need * dt / 4f;
            Progress = Mathf.Clamp01(accum / need);
            if (halo != null) halo.emphasis = Mathf.Max(0.2f, Progress);
            ctx.Animal.Performance.OnStepProgress(ctx.Index, ctx.Config, Progress, focus);
            if (Progress >= 1f) Completed = true;
        }

        public override void BeginAssist() { assist = true; }
        public override IEnumerator Finish() { yield return new WaitForSeconds(0.8f); }
    }
}
