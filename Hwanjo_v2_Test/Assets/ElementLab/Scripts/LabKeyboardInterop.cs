using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Hwanjo.ElementLab
{
    // Some Windows accessibility/remote input sends window key messages without Raw Input.
    // Queue only transitions missing from the current Input System state. Native physical
    // input already reflected in that state is never sent a second time.
    public static class LabKeyboardInterop
    {
        static readonly Dictionary<Key, bool> pending = new Dictionary<Key, bool>();
        public static void BeginFrame() { pending.Clear(); }
        public static void Forward(Key key, bool down)
        {
            var keyboard = Keyboard.current; if (keyboard == null) return;
            bool current = pending.TryGetValue(key, out bool value) ? value : keyboard[key].isPressed;
            if (current == down) return;
            pending[key] = down;
            var held = new HashSet<Key>();
            foreach (var control in keyboard.allKeys) if (control.isPressed) held.Add(control.keyCode);
            foreach (var change in pending) { if (change.Value) held.Add(change.Key); else held.Remove(change.Key); }
            var keys = new Key[held.Count]; held.CopyTo(keys);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }
        public static void OnGui(Event e)
        {
            if (e.type != EventType.KeyDown && e.type != EventType.KeyUp) return;
            Key key;
            switch (e.keyCode)
            {
                case KeyCode.A: key = Key.A; break; case KeyCode.D: key = Key.D; break;
                case KeyCode.LeftArrow: key = Key.LeftArrow; break; case KeyCode.RightArrow: key = Key.RightArrow; break;
                case KeyCode.J: key = Key.J; break; case KeyCode.R: key = Key.R; break;
                case KeyCode.Space: key = Key.Space; break; case KeyCode.LeftShift: key = Key.LeftShift; break; case KeyCode.RightShift: key = Key.RightShift; break;
                case KeyCode.Alpha1: key = Key.Digit1; break; case KeyCode.Alpha2: key = Key.Digit2; break;
                case KeyCode.Alpha3: key = Key.Digit3; break; case KeyCode.Alpha4: key = Key.Digit4; break;
                case KeyCode.F1: key = Key.F1; break; case KeyCode.F2: key = Key.F2; break; case KeyCode.F3: key = Key.F3; break; case KeyCode.F9: key = Key.F9; break;
                case KeyCode.Escape: key = Key.Escape; break; default: return;
            }
            Forward(key, e.type == EventType.KeyDown);
        }
    }
}
