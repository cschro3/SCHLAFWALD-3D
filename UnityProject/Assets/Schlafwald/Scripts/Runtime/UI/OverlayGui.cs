using System.Collections.Generic;
using UnityEngine;
using Schlafwald.Core;

namespace Schlafwald
{
    /// <summary>
    /// Minimalistische Oberfläche (IMGUI-Prototyp). Für Kinder: KEINE Menüs, keine Texte, nur ein fast unsichtbarer Eltern-Knopf
    /// (3 s halten + Rechenaufgabe). Dazu Überblendungen, Untertitel/Entwicklungs-Hinweis des Fallbacks und der Eltern-Bildschirmdimmer.
    /// IMGUI ist bewusst nur für diesen seltenen Elternbereich gewählt; vor Release durch UI Toolkit ersetzen (docs/QA.md).
    /// </summary>
    public class OverlayGui : MonoBehaviour
    {
        enum Gate { Closed, Holding, Challenge, Panel }
        AppServices s;
        Gate gate = Gate.Closed;
        HoldGate hold = new HoldGate(3f);
        MathChallenge challenge;
        System.Random rng = new System.Random();
        Texture2D vignetteTex;
        Vector2 scroll;
        GUIStyle label, button, toggle, title, small, box, banner, subtitle;
        bool stylesReady;
        int covHave, covTotal; bool covDone;
        public System.Action RestartRequested;

        public void Init(AppServices services) { s = services; }

        Rect GearRect(float u) { float w = Screen.width / u; return new Rect(w - 96f, 16f, 80f, 80f); }

        void Update()
        {
            if (s == null) return;
            float u = Screen.height / 720f;
            if (gate == Gate.Closed || gate == Gate.Holding)
            {
                // Unskalierte Zeit, damit auch während einer Pause gehalten werden kann
                var g = GearRect(u);
                Vector2 p = PointerInput.Position;
                bool inside = PointerInput.Down && p.x >= g.x * u && p.x <= (g.x + g.width) * u && (Screen.height - p.y) >= g.y * u && (Screen.height - p.y) <= (g.y + g.height) * u;
                if (hold.Tick(Time.unscaledDeltaTime, inside))
                {
                    challenge = ParentGate.Create(rng);
                    hold = new HoldGate(3f);
                    gate = Gate.Challenge;
                    PauseController.Request("parent");
                    PointerInput.Reset();
                }
                else gate = hold.Progress > 0.02f ? Gate.Holding : Gate.Closed;
            }
        }

        void MakeStyles(float u)
        {
            if (stylesReady) return;
            stylesReady = true;
            label = new GUIStyle(GUI.skin.label) { fontSize = 26, wordWrap = true, normal = { textColor = Color.white } };
            small = new GUIStyle(label) { fontSize = 20, normal = { textColor = new Color(1, 1, 1, 0.8f) } };
            title = new GUIStyle(label) { fontSize = 34, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = 26, fixedHeight = 56 };
            toggle = new GUIStyle(GUI.skin.toggle) { fontSize = 26 };
            box = new GUIStyle(GUI.skin.box);
            banner = new GUIStyle(label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.4f, 0.95f) } };
            subtitle = new GUIStyle(label) { fontSize = 32, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 1f, 0.95f, 0.95f) } };
            GUI.skin.horizontalSlider.fixedHeight = 26;
            GUI.skin.horizontalSliderThumb.fixedHeight = 38; GUI.skin.horizontalSliderThumb.fixedWidth = 38;
        }

        void OnGUI()
        {
            if (s == null || s.Settings == null) return;
            float u = Screen.height / 720f;
            var prev = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(u, u, 1f));
            float W = Screen.width / u, H = 720f;
            MakeStyles(u);
            var set = s.Settings.Current;

            // Audiomodus: schwarzer Bildschirm
            float black = set.audioOnlyMode ? 1f : Mathf.Max(s.Transitions.FadeAlpha, 1f - s.Transitions.DimBrightness);
            if (s.Camera != null) s.Camera.enabled = !set.audioOnlyMode;

            if (s.Transitions.Vignette > 0.01f)
            {
                if (vignetteTex == null) vignetteTex = MakeVignette();
                GUI.color = new Color(0.04f, 0.03f, 0.08f, s.Transitions.Vignette);
                GUI.DrawTexture(new Rect(0, 0, W, H), vignetteTex);
            }
            if (black > 0.001f)
            {
                GUI.color = new Color(0, 0, 0, black);
                GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Untertitel (Fallback = immer, damit Eltern vorlesen können) & ausdrücklicher Hinweis
            string sub = s.Narration.Subtitle;
            if (!string.IsNullOrEmpty(sub) && !set.audioOnlyMode)
                GUI.Label(new Rect(W * 0.12f, H - 150f, W * 0.76f, 120f), sub, subtitle);
            if (s.Narration.UsingFallback && (Application.isEditor || Debug.isDebugBuild) && !set.audioOnlyMode)
                GUI.Label(new Rect(0, 8, W, 30), "ENTWICKLUNGS-FALLBACK: keine Sprachaufnahme vorhanden – Murmel-Platzhalter + Untertitel", banner);

            if (gate == Gate.Closed || gate == Gate.Holding) DrawGear(u);
            else if (gate == Gate.Challenge) DrawChallenge(W, H);
            else DrawPanel(W, H);
            GUI.matrix = prev;
        }

        void DrawGear(float u)
        {
            var g = GearRect(u);
            float a = gate == Gate.Holding ? 0.55f : 0.14f;
            GUI.color = new Color(1, 1, 1, a);
            GUI.Label(new Rect(g.x + 8, g.y + 14, g.width, g.height), "•••", title);
            if (gate == Gate.Holding)
            {
                GUI.color = new Color(1f, 0.9f, 0.5f, 0.8f);
                GUI.DrawTexture(new Rect(g.x, g.yMax - 8, g.width * hold.Progress, 8), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        void DrawChallenge(float W, float H)
        {
            var r = new Rect(W / 2 - 330, H / 2 - 190, 660, 380);
            GUI.color = new Color(0.08f, 0.08f, 0.16f, 0.96f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(new Rect(r.x + 30, r.y + 20, r.width - 60, 50), "Nur für Eltern", title);
            GUI.Label(new Rect(r.x + 30, r.y + 80, r.width - 60, 50), "Bitte lösen: " + challenge.Question, label);
            for (int i = 0; i < 3; i++)
                if (GUI.Button(new Rect(r.x + 30 + i * 200, r.y + 160, 180, 70), challenge.Answers[i].ToString(), button))
                {
                    if (ParentGate.Check(challenge, i)) { gate = Gate.Panel; covDone = false; }
                    else { gate = Gate.Closed; PauseController.Release("parent"); }
                }
            if (GUI.Button(new Rect(r.x + 30, r.y + 270, 200, 60), "Abbrechen", button)) { gate = Gate.Closed; PauseController.Release("parent"); }
        }

        void DrawPanel(float W, float H)
        {
            var set = s.Settings.Current;
            var r = new Rect(W / 2 - 400, 30, 800, H - 60);
            GUI.color = new Color(0.07f, 0.08f, 0.15f, 0.97f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(r.x + 24, r.y + 14, r.width - 48, r.height - 28));
            GUILayout.Label("Elternbereich", title);
            scroll = GUILayout.BeginScrollView(scroll);

            GUILayout.Label("Lautstärke gesamt: " + Mathf.RoundToInt(set.masterVolume * 100) + " %", label);
            set.masterVolume = GUILayout.HorizontalSlider(set.masterVolume, 0f, 1f);
            GUILayout.Label("Erzähler: " + Mathf.RoundToInt(set.narrationVolume * 100) + " %", label);
            set.narrationVolume = GUILayout.HorizontalSlider(set.narrationVolume, 0f, 1f);
            set.musicEnabled = GUILayout.Toggle(set.musicEnabled, " Musik an", toggle);
            GUILayout.Label("Musik: " + Mathf.RoundToInt(set.musicVolume * 100) + " %", label);
            set.musicVolume = GUILayout.HorizontalSlider(set.musicVolume, 0f, 1f);
            GUILayout.Label("Waldgeräusche: " + Mathf.RoundToInt(set.ambienceVolume * 100) + " %", label);
            set.ambienceVolume = GUILayout.HorizontalSlider(set.ambienceVolume, 0f, 1f);

            GUILayout.Space(10);
            GUILayout.Label("Erzählerstimme (gilt ab neuer Zeile)", label);
            GUILayout.BeginHorizontal();
            foreach (var n in s.Story.narrators)
            {
                bool sel = set.narratorId == n.id;
                if (GUILayout.Toggle(sel, " " + n.displayName, button) && !sel) { set.narratorId = n.id; covDone = false; }
            }
            GUILayout.EndHorizontal();
            if (!covDone) { NarrationManager.Coverage(s.Story, set.narratorId, out covHave, out covTotal); covDone = true; }
            GUILayout.Label("Sprachaufnahmen vorhanden: " + covHave + " / " + covTotal + (covHave == 0 ? "  (Entwicklungs-Fallback aktiv)" : ""), small);

            GUILayout.Space(10);
            float tm = set.targetMinutes <= 0f ? 0f : set.targetMinutes;
            GUILayout.Label("Gewünschte Dauer: " + (tm <= 0f ? "natürlicher Ablauf" : Mathf.RoundToInt(tm) + " Minuten") + " (gilt ab neuer Nacht)", label);
            tm = GUILayout.HorizontalSlider(tm, 0f, 45f);
            set.targetMinutes = tm < 7f ? 0f : Mathf.Round(tm / 5f) * 5f;
            GUILayout.Label("Bildschirm ausblenden nach Ende: " + (set.dimAfterMinutes <= 0f ? "nie" : Mathf.RoundToInt(set.dimAfterMinutes) + " Min."), label);
            float dm = GUILayout.HorizontalSlider(set.dimAfterMinutes, 0f, 30f);
            set.dimAfterMinutes = dm < 1f ? 0f : Mathf.Round(dm);
            set.audioOnlyMode = GUILayout.Toggle(set.audioOnlyMode, " Audiomodus ohne Bildschirm (Hilfen laufen schneller)", toggle);
            set.showSubtitles = GUILayout.Toggle(set.showSubtitles, " Untertitel zum Vorlesen", toggle);

            GUILayout.Space(10);
            GUILayout.Label("Tiere der Nacht (gilt ab neuer Nacht)", label);
            foreach (var a in s.Story.animals)
            {
                GUI.enabled = a.implemented;
                bool on = set.IsAnimalEnabled(a.id) && a.implemented;
                bool now = GUILayout.Toggle(on, " " + a.displayName + (a.implemented ? "" : "  (3D-Figur folgt in Phase 3)"), toggle);
                if (now != on) ToggleAnimal(set, a.id, now);
                GUI.enabled = true;
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            bool userPaused = PauseController.Has("user");
            if (GUILayout.Button(userPaused ? "Geschichte fortsetzen" : "Geschichte pausieren", button)) { if (userPaused) PauseController.Release("user"); else PauseController.Request("user"); }
            if (GUILayout.Button("Neue Nacht starten", button)) { s.Settings.Save(); CloseGate(); if (RestartRequested != null) RestartRequested(); return; }
            if (GUILayout.Button("Schließen", button)) { s.Settings.Save(); CloseGate(); }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void ToggleAnimal(ParentSettings set, string id, bool on)
        {
            if (set.enabledAnimals.Count == 0) foreach (var a in s.Story.animals) if (a.implemented) set.enabledAnimals.Add(a.id);
            if (on && !set.enabledAnimals.Contains(id)) set.enabledAnimals.Add(id);
            if (!on) set.enabledAnimals.Remove(id);
            if (set.enabledAnimals.Count == 0) foreach (var a in s.Story.animals) if (a.implemented) set.enabledAnimals.Add(a.id); // mindestens ein Tier
        }

        void CloseGate() { gate = Gate.Closed; PauseController.Release("parent"); }

        static Texture2D MakeVignette()
        {
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float a = Mathf.SmoothStep(0.45f, 1.25f, d);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255));
                }
            t.SetPixels32(px); t.Apply(false, true);
            return t;
        }
    }
}
