using System;
using System.Collections.Generic;

namespace Schlafwald.Core
{
    /// <summary>Serialisierbarer Vektor, damit Core ohne UnityEngine auskommt.</summary>
    [Serializable]
    public class Vec3
    {
        public float x, y, z;
        public Vec3() { }
        public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    /// <summary>Eine Erzählerzeile. Alle Texte der App liegen als bearbeitbare Daten in story_de.json.</summary>
    [Serializable]
    public class NarrationLine
    {
        public string id;
        public string text;
        /// <summary>intro, step0..n, hint(n), assist(n), success(n), outro</summary>
        public string phase = "";
        public float pauseAfter = 1.2f;
        /// <summary>warm | soft | whisper – steuert Lautstärke/Klangfarbe des Fallbacks und Hinweise für Sprecher.</summary>
        public string tone = "warm";
        /// <summary>Optionale feste Erzählerstimme; leer = im Elternbereich gewählte Stimme.</summary>
        public string narrator = "";
        /// <summary>Animations-Cue, der beim Start der Zeile ausgelöst wird (Synchronisierung Sprache ↔ Animation).</summary>
        public string cue = "";
        public float cueDelay = 0f;
        /// <summary>Finale: Zeile nur sprechen, wenn dieses Tier teilnimmt.</summary>
        public string animalId = "";
        /// <summary>"" = immer, "all" = nur wenn alle 7 Tiere teilnehmen, "partial" = nur bei Auswahl einzelner Tiere (Elternbereich).</summary>
        public string when = "";
        /// <summary>Kameraeinstellung, die mit dieser Zeile beginnt ("wide", "moon" oder "@tierId.sleep").</summary>
        public string shotId = "";
        /// <summary>Schlüssel der Audiodatei in Resources/Audio/Narration/{narrator}/; leer = id.</summary>
        public string audioKey = "";

        public string AudioKeyOrId { get { return string.IsNullOrEmpty(audioKey) ? id : audioKey; } }
    }

    [Serializable]
    public class ShotConfig
    {
        public string id;
        public Vec3 position;
        public Vec3 lookAt;
        public float fov = 50f;
        public float duration = 8f;
    }

    [Serializable]
    public class SleepSpot
    {
        public Vec3 position;
        public float yaw;
    }

    /// <summary>Ein Interaktionsschritt. type: FollowPath | DragToTarget | Touch | Stroke.</summary>
    [Serializable]
    public class StepConfig
    {
        public string id;
        public string type;
        /// <summary>leaf | bigLeaf | blanket | acorn | mossPillow | firefly | guideOrb</summary>
        public string prop = "";
        /// <summary>Startpositionen der Objekte bzw. Pfadpunkte (bei FollowPath nur x/z, y folgt dem Gelände).</summary>
        public Vec3[] points;
        public Vec3 target;
        /// <summary>Großzügiger Trefferradius in Metern (Zielzone).</summary>
        public float radius = 0.6f;
        public float holdSeconds = 1.2f;
        /// <summary>Strichlänge in Bildschirmhöhen (Stroke).</summary>
        public float strokeLength = 1.5f;
        public float helpAfter = 14f;
        public float assistAfter = 32f;
        public string successTrigger = "";
    }

    [Serializable]
    public class AnimalAudio
    {
        public string idleSfx = "";
        public string sleepSfx = "";
        public string narrationFolder = "";
    }

    [Serializable]
    public class AnimalConfig
    {
        public string id;
        public string displayName;
        public string species;
        public int order;
        /// <summary>false = Story/Konfiguration vorhanden, aber 3D-Charakter noch nicht umgesetzt (Phase 3).</summary>
        public bool implemented;
        public string modelId;
        public SleepSpot sleepSpot;
        public List<StepConfig> steps = new List<StepConfig>();
        public string[] animationStates = new string[0];
        public List<NarrationLine> lines = new List<NarrationLine>();
        public AnimalAudio audio = new AnimalAudio();
        public List<ShotConfig> shots = new List<ShotConfig>();
        /// <summary>Beschreibt, wann das Tier als eingeschlafen gilt.</summary>
        public string successTrigger = "";

        public List<NarrationLine> GetLines(string phase)
        {
            var r = new List<NarrationLine>();
            foreach (var l in lines) if (l.phase == phase) r.Add(l);
            return r;
        }

        /// <summary>Zeilen für einen Schritt: bevorzugt "phase{n}", sonst generisch "phase".</summary>
        public List<NarrationLine> GetStepLines(string phase, int stepIndex)
        {
            var specific = GetLines(phase + stepIndex);
            return specific.Count > 0 ? specific : GetLines(phase);
        }

        public ShotConfig FindShot(string shotId)
        {
            foreach (var s in shots) if (s.id == shotId) return s;
            return null;
        }
    }

    [Serializable]
    public class ChapterConfig
    {
        public string id;
        public string title;
        /// <summary>intro | animal | finale</summary>
        public string kind;
        public string animalId = "";
        public List<NarrationLine> lines = new List<NarrationLine>();
        public List<ShotConfig> shots = new List<ShotConfig>();
    }

    [Serializable]
    public class NarratorConfig
    {
        public string id;
        public string displayName;
        public string description;
    }

    [Serializable]
    public class WorldConfig
    {
        /// <summary>Waldpfad (Mittellinie), entlang dessen die Kamera reist.</summary>
        public Vec3[] path;
    }

    [Serializable]
    public class StoryData
    {
        public int version;
        public string language;
        public string title;
        public List<NarratorConfig> narrators = new List<NarratorConfig>();
        public WorldConfig world = new WorldConfig();
        public List<ChapterConfig> chapters = new List<ChapterConfig>();
        public List<AnimalConfig> animals = new List<AnimalConfig>();

        public AnimalConfig FindAnimal(string id)
        {
            foreach (var a in animals) if (a.id == id) return a;
            return null;
        }

        public ChapterConfig FindChapter(string kind)
        {
            foreach (var c in chapters) if (c.kind == kind) return c;
            return null;
        }
    }
}
