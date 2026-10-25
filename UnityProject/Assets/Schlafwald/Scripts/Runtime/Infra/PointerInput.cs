using UnityEngine;

namespace Schlafwald
{
    /// <summary>Quelle für Zeigereingaben. Hardware (Touch/Maus) oder Skript (PlayMode-Tests).</summary>
    public interface IPointerSource
    {
        bool IsDown { get; }
        Vector2 Position { get; }
    }

    public class HardwarePointerSource : IPointerSource
    {
        public bool IsDown
        {
            get { return Input.touchCount > 0 ? Input.GetTouch(0).phase != TouchPhase.Ended && Input.GetTouch(0).phase != TouchPhase.Canceled : Input.GetMouseButton(0); }
        }
        public Vector2 Position
        {
            get { return Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition; }
        }
    }

    public class ScriptedPointerSource : IPointerSource
    {
        public bool down;
        public Vector2 position;
        public bool IsDown { get { return down; } }
        public Vector2 Position { get { return position; } }
    }

    /// <summary>
    /// Einheitlicher Zeigerzustand pro Frame (nur der erste Finger – Mehrfachberührungen werden bewusst
    /// ignoriert, damit Kleinkinder nichts "falsch" machen können). Wird von <see cref="InputPump"/> aktualisiert.
    /// </summary>
    public static class PointerInput
    {
        public static IPointerSource Source = new HardwarePointerSource();
        public static bool Down, Pressed, Released;
        public static Vector2 Position, Delta;
        static bool wasDown;
        static Vector2 lastPos;

        public static void Poll()
        {
            Down = Source.IsDown;
            Position = Source.Position;
            Pressed = Down && !wasDown;
            Released = !Down && wasDown;
            Delta = (Down && !Pressed) ? Position - lastPos : Vector2.zero;
            wasDown = Down;
            lastPos = Position;
        }

        public static void Reset() { Down = Pressed = Released = wasDown = false; Delta = Vector2.zero; }

        /// <summary>Großzügige Touch-Zone: mindestens ~11 % der Bildschirmhöhe, mindestens der Weltradius in Pixeln.</summary>
        public static float TouchRadiusPx(Camera cam, Vector3 worldPos, float worldRadius)
        {
            float min = Screen.height * 0.11f;
            if (cam == null) return min;
            Vector3 a = cam.WorldToScreenPoint(worldPos);
            Vector3 b = cam.WorldToScreenPoint(worldPos + cam.transform.right * worldRadius);
            if (a.z <= 0f) return min;
            return Mathf.Max(min, Mathf.Abs(b.x - a.x));
        }

        public static bool IsNear(Camera cam, Vector3 worldPos, float worldRadius, Vector2 pointer)
        {
            Vector3 sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z <= 0f) return false;
            return Vector2.Distance(new Vector2(sp.x, sp.y), pointer) <= TouchRadiusPx(cam, worldPos, worldRadius);
        }
    }

    [DefaultExecutionOrder(-1000)]
    public class InputPump : MonoBehaviour
    {
        void Update() { PointerInput.Poll(); }
    }
}
