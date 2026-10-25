using System.Collections;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Suris Animation: peristaltische Kriechwelle, neugierige Fühler, freundlicher Blick zum Kind, Fühler einziehen,
    /// langsam ins Schneckenhaus gleiten, Haus wackelt noch einmal, dann Ruhe. Alles prozedural und über Cues
    /// mit der Erzählung synchronisiert (siehe story_de.json: retractFeelers, enterShell, enterShellDeep, shellWobble).
    /// </summary>
    public class SnailPerformance : AnimalPerformance
    {
        Transform body, head, neck, shell, eyeL, eyeR, feelerL, feelerL2, feelerR, feelerR2, mantle, opening;
        Transform[] feet;

        readonly Channel feelerRetract = new Channel(0f);
        readonly Channel retract = new Channel(0f);
        readonly Channel curiosity = new Channel(0.35f);
        readonly Channel lookWeight = new Channel(0f);
        readonly Channel faceChild = new Channel(0f);
        readonly Channel eyeOpen = new Channel(1f);

        float stageBase;          // wächst mit jedem gelösten Schritt (Müdigkeit)
        Vector3 targetPos; bool hasTarget;
        Vector3 vel; float yVel;
        float crawl, crawlPhase, lastTrail;
        float yaw;
        float blinkTimer = 3f, blinkT = -1f;
        float wobbleT = -1f, wobbleAmp;
        float time;
        Material trailMat;
        Vector3 lastPos;

        public override void Init(AnimalRig rig, AnimalConfig config, AppServices services)
        {
            base.Init(rig, config, services);
            body = rig.Get("Body"); head = rig.Get("Head"); neck = rig.Get("Neck"); shell = rig.Get("Shell"); opening = rig.Get("ShellOpening");
            eyeL = rig.Get("EyeL"); eyeR = rig.Get("EyeR");
            feelerL = rig.Get("FeelerL"); feelerL2 = rig.Get("FeelerL2"); feelerR = rig.Get("FeelerR"); feelerR2 = rig.Get("FeelerR2");
            mantle = rig.Get("Mantle");
            feet = new Transform[SnailBuilder.FootSegments];
            for (int i = 0; i < feet.Length; i++) feet[i] = rig.Get("Foot" + i);
            yaw = transform.eulerAngles.y;
            lastPos = transform.position;
            trailMat = MaterialLibrary.Glow(ProceduralTextures.SoftDot(), new Color(0.92f, 0.96f, 1f, 0.22f), 1f, false);
            targetPos = transform.position;
        }

        public override Vector3 FocusPoint { get { return transform.position + transform.TransformDirection(new Vector3(0, 0.2f, -0.05f)); } }

        // ---------- Story-Hooks ----------
        public override void OnStepBegin(int index, StepConfig step)
        {
            if (step.type == "FollowPath") { hasTarget = true; curiosity.Go(0.7f, 1.5f); }
        }

        public override void OnStepProgress(int index, StepConfig step, float progress, Vector3 worldPoint)
        {
            if (step.type == "FollowPath") { targetPos = worldPoint; hasTarget = true; }
        }

        public override void OnStepComplete(int index, StepConfig step)
        {
            stageBase = Mathf.Max(stageBase, 0.15f * (index + 1));
            if (index == 0)
            {
                lookWeight.Go(1f, 1.5f); faceChild.Go(1f, 2.2f); curiosity.Go(0.9f, 1f);
                if (Services != null && Services.Audio != null) Services.Audio.PlaySfx("squeak", transform.position, 0.5f);
            }
            else if (index == 1)
            {
                curiosity.Go(1f, 0.6f);
                StartCoroutine(Settle(0.45f, 5f));
                if (Services != null && Services.Audio != null) Services.Audio.PlaySfx("twinkle", transform.position, 0.6f);
            }
            else curiosity.Go(0.3f, 2f);
        }

        IEnumerator Settle(float to, float after) { yield return new WaitForSeconds(after); curiosity.Go(to, 2f); }

        public override void Touched(Vector3 worldPoint, float strength)
        {
            if (retract.Value > 0.3f) { if (wobbleT < 0f) { wobbleT = 0f; wobbleAmp = 2f * strength; } return; }
            curiosity.Go(1f, 0.4f);
            if (Services != null && Services.Audio != null && Time.time - lastTouchSfx > 2f) { lastTouchSfx = Time.time; Services.Audio.PlaySfx("squeak", transform.position, 0.35f); }
        }
        float lastTouchSfx = -10f;

        public override void Cue(string cue)
        {
            switch (cue)
            {
                case "feelersCurious": curiosity.Go(1f, 1.2f); StartCoroutine(Settle(0.5f, 6f)); break;
                case "lookAtChild": lookWeight.Go(1f, 1.5f); faceChild.Go(1f, 2.2f); break;
                case "retractFeelers": feelerRetract.Go(1f, 2.4f); curiosity.Go(0f, 1f); break;
                case "enterShell":
                    retract.Go(0.55f, 4.0f); eyeOpen.Go(0.45f, 3.5f); lookWeight.Go(0f, 2f); faceChild.Go(0.5f, 2f);
                    if (Services != null && Services.Audio != null) Services.Audio.PlaySfx("yawn", transform.position, 0.4f);
                    break;
                case "enterShellDeep": retract.Go(1f, 4.0f); eyeOpen.Go(0f, 3.0f); break;
                case "shellWobble": StartCoroutine(WobbleAfter(0f)); break;
            }
        }

        IEnumerator WobbleAfter(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            wobbleT = 0f; wobbleAmp = 4.5f;
            while (wobbleT >= 0f) yield return null;
        }

        bool wobbledOnce;

        public override IEnumerator FinishSleep()
        {
            if (feelerRetract.Target < 1f) feelerRetract.Go(1f, 2f);
            if (retract.Target < 1f) { retract.Go(1f, 5f); eyeOpen.Go(0f, 4f); }
            while (!retract.Done || !feelerRetract.Done) yield return null;
            if (!wobbledOnce) yield return WobbleAfter(0.2f);
            while (wobbleT >= 0f) yield return null;
            yield return new WaitForSeconds(2.0f);
            IsAsleep = true; Sleepiness = 1f;
            if (Services != null && Services.Audio != null) Services.Audio.PlaySfx("snore", transform.position, 0.25f);
        }

        public override void SnapToAsleep()
        {
            feelerRetract.Set(1f); retract.Set(1f); eyeOpen.Set(0f); curiosity.Set(0f); lookWeight.Set(0f);
            faceChild.Set(0.5f); stageBase = 1f; IsAsleep = true; Sleepiness = 1f; hasTarget = false; wobbledOnce = true;
        }

        // ---------- Frame-Animation ----------
        void Update()
        {
            float dt = Time.deltaTime; if (dt <= 0f) return;
            time += dt;
            feelerRetract.Tick(dt); retract.Tick(dt); curiosity.Tick(dt); lookWeight.Tick(dt); faceChild.Tick(dt); eyeOpen.Tick(dt);

            UpdateMovement(dt);
            UpdateFoot();
            UpdateHeadAndFeelers();
            UpdateEyes(dt);
            UpdateRetract();
            UpdateShell(dt);

            float targetSleep = Mathf.Lerp(stageBase, 1f, retract.Value);
            if (!IsAsleep) Sleepiness = Mathf.MoveTowards(Sleepiness, targetSleep, dt * 0.5f);
        }

        void UpdateMovement(float dt)
        {
            Vector3 pos = transform.position;
            if (hasTarget && retract.Value < 0.01f)
            {
                Vector3 flatTarget = new Vector3(targetPos.x, pos.y, targetPos.z);
                Vector3 np = Vector3.SmoothDamp(pos, flatTarget, ref vel, 0.45f, 0.30f, dt);
                pos.x = np.x; pos.z = np.z;
                float gy = ForestTerrain.Height(pos.x, pos.z);
                pos.y = Mathf.SmoothDamp(pos.y, gy, ref yVel, 0.25f, 1f, dt);
                transform.position = pos;
            }
            Vector3 move = transform.position - lastPos; move.y = 0f;
            float speed = move.magnitude / dt;
            lastPos = transform.position;
            crawl = Mathf.Lerp(crawl, Mathf.Clamp01(speed / 0.10f), 1f - Mathf.Exp(-6f * dt));

            if (move.sqrMagnitude > 1e-8f && speed > 0.02f)
                yaw = Mathf.LerpAngle(yaw, Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg, 1f - Mathf.Exp(-3f * dt));
            if (faceChild.Value > 0.001f && Camera.main != null)
            {
                Vector3 toCam = Camera.main.transform.position - transform.position; toCam.y = 0f;
                if (toCam.sqrMagnitude > 0.01f)
                {
                    // Der Blick zum Kind ist leicht seitlich (3/4-Ansicht), damit man die Schnecke nicht frontal "plattdrückt".
                    float camYaw = Mathf.Atan2(toCam.x, toCam.z) * Mathf.Rad2Deg - 28f;
                    yaw = Mathf.LerpAngle(yaw, camYaw, faceChild.Value * (1f - Mathf.Exp(-2f * dt)));
                }
            }
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            if (crawl > 0.3f && Vector3.Distance(transform.position, trailPos) > 0.2f) { SpawnTrail(); }
        }
        Vector3 trailPos = new Vector3(9999, 0, 9999);

        void SpawnTrail()
        {
            trailPos = transform.position;
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());
            go.name = "SlimeTrail";
            Vector3 p = transform.position - transform.forward * 0.12f;
            p.y = ForestTerrain.Height(p.x, p.z) + 0.006f;
            go.transform.position = p;
            go.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
            go.transform.localScale = new Vector3(0.10f, 0.26f, 1f);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = trailMat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            var f = go.AddComponent<FadeAndDie>(); f.duration = 22f; f.startAlpha = 0.22f; f.tint = new Color(0.92f, 0.96f, 1f);
        }

        void UpdateFoot()
        {
            float retractAmt = retract.Value;
            for (int i = 0; i < feet.Length; i++)
            {
                if (feet[i] == null) continue;
                var rest = Rig.Rest("Foot" + i);
                float wave = Mathf.Sin(crawlPhase - i * 0.85f);
                float s = Mathf.Clamp01(retractAmt * 1.5f - i * 0.07f); s = s * s * (3f - 2f * s);
                float breath = 1f + 0.012f * Mathf.Sin(time * (IsAsleep ? 0.9f : 1.4f) + i * 0.3f);
                Vector3 sc = new Vector3(rest.scale.x * (1f - 0.08f * crawl * wave), rest.scale.y * (1f + 0.14f * crawl * wave) * breath, rest.scale.z * (1f + 0.20f * crawl * wave));
                Vector3 p = rest.position + new Vector3(0, 0, 0.010f * crawl * wave);
                Vector3 inPos = new Vector3(0f, 0.10f, -0.035f);
                feet[i].localPosition = Vector3.Lerp(p, inPos, s);
                feet[i].localScale = Vector3.Lerp(sc, sc * 0.02f, s);
            }
            if (mantle != null)
            {
                var m = Rig.Rest("Mantle");
                float sm = Mathf.Clamp01(retractAmt * 1.5f - 0.35f); sm = sm * sm * (3f - 2f * sm);
                mantle.localPosition = Vector3.Lerp(m.position, new Vector3(0f, 0.10f, -0.035f), sm);
                mantle.localScale = Vector3.Lerp(m.scale * (1f + 0.012f * Mathf.Sin(time * 1.4f)), m.scale * 0.02f, sm);
            }
            crawlPhase += Time.deltaTime * (1.2f + 7f * crawl);
        }

        void UpdateHeadAndFeelers()
        {
            float r = retract.Value;
            // Kopf: Blick zum Kind + sanftes Nicken; beim Einziehen in die Öffnung gleiten
            var headRest = Rig.Rest("Head");
            Quaternion look = Quaternion.identity;
            if (lookWeight.Value > 0.001f && Camera.main != null)
            {
                Vector3 local = transform.InverseTransformPoint(Camera.main.transform.position) - headRest.position;
                float yawL = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -50f, 50f);
                float pitchL = Mathf.Clamp(-Mathf.Atan2(local.y, new Vector2(local.x, local.z).magnitude) * Mathf.Rad2Deg, -10f, 25f);
                look = Quaternion.Euler(pitchL * lookWeight.Value, yawL * lookWeight.Value, 0f);
            }
            float nod = Mathf.Sin(time * 0.9f) * 2.2f * (1f - r);
            float s = Mathf.Clamp01(r * 1.7f); s = s * s * (3f - 2f * s);
            head.localRotation = headRest.rotation * look * Quaternion.Euler(nod + 14f * stageBase * (1f - r), 0, 0);
            head.localPosition = Vector3.Lerp(headRest.position, new Vector3(0f, 0.105f, -0.03f), s);
            head.localScale = Vector3.Lerp(headRest.scale, headRest.scale * 0.02f, s);
            var neckRest = Rig.Rest("Neck");
            neck.localPosition = Vector3.Lerp(neckRest.position, new Vector3(0f, 0.10f, -0.03f), s);
            neck.localScale = Vector3.Lerp(neckRest.scale, neckRest.scale * 0.02f, s);

            // Fühler: neugierig schwingen, dann zuerst einziehen (Skalieren + Zurücklehnen)
            float cur = curiosity.Value;
            float fr = feelerRetract.Value;
            Sway(feelerL, feelerL2, "FeelerL", "FeelerL2", 0.0f, cur, fr);
            Sway(feelerR, feelerR2, "FeelerR", "FeelerR2", 1.7f, cur, fr);
        }

        void Sway(Transform f1, Transform f2, string n1, string n2, float phase, float curiosityAmt, float retractAmt)
        {
            var a = Rig.Rest(n1); var b = Rig.Rest(n2);
            float swayX = Mathf.Sin(time * 1.3f + phase) * (4f + 12f * curiosityAmt);
            float swayZ = Mathf.Sin(time * 0.9f + phase * 1.7f) * (3f + 9f * curiosityAmt);
            float bend = Mathf.Sin(time * 1.9f + phase) * (6f + 12f * curiosityAmt);
            float back = retractAmt;
            f1.localRotation = a.rotation * Quaternion.Euler(swayX - 28f * back, 0, swayZ);
            f2.localRotation = b.rotation * Quaternion.Euler(bend - 20f * back, 0, 0);
            float k = Mathf.Lerp(1f, 0.04f, retractAmt * retractAmt * (3f - 2f * retractAmt));
            f1.localScale = new Vector3(a.scale.x, a.scale.y * k, a.scale.z);
            f2.localScale = new Vector3(b.scale.x, b.scale.y * k, b.scale.z);
        }

        void UpdateEyes(float dt)
        {
            blinkTimer -= dt;
            if (blinkTimer <= 0f && blinkT < 0f) { blinkT = 0f; blinkTimer = Random.Range(2.5f, 6f); }
            float blink = 0f;
            if (blinkT >= 0f) { blinkT += dt; blink = Mathf.Sin(Mathf.Clamp01(blinkT / 0.22f) * Mathf.PI); if (blinkT > 0.22f) blinkT = -1f; }
            float droop = 0.40f * stageBase * (1f - retract.Value);
            float close = Mathf.Max(blink, 1f - eyeOpen.Value, droop);
            float sy = Mathf.Lerp(1f, 0.07f, close);
            var rl = Rig.Rest("EyeL"); var rr = Rig.Rest("EyeR");
            float shrink = Mathf.Clamp01(retract.Value * 1.7f); shrink = shrink * shrink * (3f - 2f * shrink);
            eyeL.localScale = new Vector3(rl.scale.x, rl.scale.y * sy, rl.scale.z);
            eyeR.localScale = new Vector3(rr.scale.x, rr.scale.y * sy, rr.scale.z);
        }

        void UpdateRetract()
        {
            // Wangen/Lächeln sind Kinder des Kopfes und skalieren mit – nichts weiter nötig.
            if (opening != null)
            {
                var o = Rig.Rest("ShellOpening");
                opening.localScale = o.scale * (1f + 0.04f * Mathf.Sin(time * (IsAsleep ? 0.9f : 1.4f)));
            }
        }

        void UpdateShell(float dt)
        {
            var rest = Rig.Rest("Shell");
            Quaternion wob = Quaternion.identity;
            if (wobbleT >= 0f)
            {
                wobbleT += dt;
                float a = Mathf.Sin(wobbleT * 12f) * wobbleAmp * Mathf.Exp(-2.4f * wobbleT);
                wob = Quaternion.Euler(0f, 0f, a) * Quaternion.Euler(a * 0.4f, 0f, 0f);
                if (wobbleT > 2.2f) { wobbleT = -1f; wobbledOnce = true; }
            }
            float breath = 1f + (IsAsleep ? 0.010f : 0.004f) * Mathf.Sin(time * (IsAsleep ? 0.9f : 1.4f));
            shell.localRotation = rest.rotation * wob;
            shell.localScale = rest.scale * breath;
        }
    }
}
