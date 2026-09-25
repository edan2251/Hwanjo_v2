using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Hwanjo.ElementLab
{
    // Explicit opt-in rendered-build evidence, not a gameplay mode or OS input automation.
    // Uses the same Input System path as a keyboard, and labels its captures as automated.
    public sealed class LabEvidenceReplay : MonoBehaviour
    {
        Keyboard keyboard; LabWorld world; int assertions;
        IEnumerator Start()
        {
            world = GetComponent<LabWorld>();
            while (!Application.isFocused) yield return null;
            yield return new WaitForSeconds(.8f);
            world.ResetLab(); world.GameFeel = false; keyboard = InputSystem.AddDevice<Keyboard>();
            Send(Key.D); yield return new WaitForSeconds(.85f); Send();
            Check(world.Player.transform.position.x > 2.5f, "continuous run advances on solid ground");
            yield return new WaitForSeconds(.15f); world.ResetLab();
            Send(Key.Digit2); yield return null; Send(Key.J); yield return new WaitForSeconds(.7f);
            Check(world.ActionCount == 0, "hold does not attack"); Send(); yield return new WaitForSeconds(.19f);
            Check(world.ActionCount == 1 && world.Trace != null && world.Trace.Active, "one charge produces one trace");
            Check(Mathf.Abs(world.LastAttackReach - world.Tuning.Range) < .001f, "charge is 1R");
            float expiry = world.Trace.ExpiresAt;
            yield return new WaitForSeconds(.27f);
            Send(Key.Digit4, Key.J); yield return new WaitForSeconds(.06f); Send(); yield return new WaitForSeconds(.16f);
            Check(world.Trace.Active && world.Trace.Element == Element.Ice && world.Trace.Platform, "stationary rehit creates supported ice");
            Check(world.Trace.ExpiresAt == expiry, "ice keeps first expiry");
            yield return world.Capture("automated-ice-platform");
            yield return new WaitForSeconds(.20f); Send(Key.Digit1, Key.J); yield return new WaitForSeconds(.06f); Send(); yield return new WaitForSeconds(.15f);
            Check(world.Trace.Active && world.Trace.Element == Element.Water && !world.Trace.Platform, "heat restores water and collision");
            Check(world.Trace.ExpiresAt == expiry, "thaw keeps first expiry");
            yield return world.Capture("automated-thawed-water");
            yield return new WaitForSeconds(.3f); world.ResetLab();
            Send(Key.J); yield return new WaitForSeconds(.65f); Send(); yield return new WaitForSeconds(.45f);
            Send(Key.Digit3, Key.J); yield return new WaitForSeconds(.06f); Send(); yield return new WaitForSeconds(.14f);
            Check(world.Trace != null && world.Trace.Active && world.Trace.Flying, "fire rehit by wind is one transported trace");
            yield return world.Capture("automated-fire-transport");
            yield return new WaitForSeconds(.6f);
            Send(Key.LeftShift); yield return new WaitForSeconds(.1f); Send(); yield return new WaitForSeconds(.3f);
            var enemy = world.Targets.Find(t => t.Kind == TargetKind.Enemy);
            world.Player.Teleport((Vector2)enemy.transform.position - Vector2.right * .9f);
            Send(Key.Digit4, Key.J); yield return new WaitForSeconds(.06f); Send(); yield return new WaitForSeconds(.17f);
            Check(enemy.Model.State.Frozen, "enemy freezes directly with intrinsic moisture");
            yield return world.Capture("automated-enemy-freeze");
            world.Player.Teleport(Vector2.zero); yield return new WaitForSeconds(.7f);
            world.Player.Damage(100, 1); yield return new WaitForSeconds(.8f);
            Check(!world.Player.Alive, "death pose after lethal hit");
            yield return world.Capture("automated-death");
            Send(); InputSystem.RemoveDevice(keyboard); keyboard = null;
            Debug.Log("EVIDENCE_REPLAY_PASS assertions=" + assertions);
            world.Quit();
        }
        void Send(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        void Check(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException("EVIDENCE_REPLAY_FAIL: " + label);
            assertions++; Debug.Log("EVIDENCE_CHECK " + label);
        }
        void OnDestroy() { if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard); }
    }
}
