using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Hwanjo.ElementLab.Tests
{
    public class LabRuntimeTests : InputTestFixture
    {
        LabWorld world;
        Keyboard keyboard;
        [UnitySetUp] public IEnumerator SetUp()
        {
            world = new GameObject("Runtime test lab").AddComponent<LabWorld>(); world.InputEnabled = false; world.GameFeel = false; world.SetInputFocus(true);
            yield return null; yield return null;
        }
        [UnityTearDown] public IEnumerator DestroyWorld()
        {
            // InputTestFixture restores the device set before UnityTearDown runs.
            keyboard = null;
            if (world) Object.Destroy(world.gameObject); yield return null;
        }
        [UnityTest] public IEnumerator FloorAndWallStopMotorWithoutTunneling()
        {
            world.Player.Teleport(new Vector2(0, 2)); yield return new WaitForSeconds(.7f);
            Assert.That(world.Player.transform.position.y, Is.EqualTo(0).Within(.04f)); Assert.IsTrue(world.Player.Grounded);
            var wall = new GameObject("Test wall"); wall.transform.SetParent(world.transform); wall.transform.position = new Vector3(1, 1);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.2f, 2); wall.AddComponent<LabSurface>();
            world.Player.Motor.Move(.3f, 20, 0); Assert.Less(world.Player.transform.position.x, .7f);
        }
        [UnityTest] public IEnumerator MultipleCollidersOnlyOnePhysicalHitAndReaction()
        {
            world.Player.Teleport(new Vector2(3.4f, 0));
            var child = new GameObject("Second hurt collider"); child.transform.SetParent(world.Dummy.transform, false); child.transform.localPosition = Vector3.up * .5f; child.AddComponent<CircleCollider2D>().isTrigger = true;
            world.ExecuteAttack(world.Player, AttackKind.Single, Element.Fire, new ActionContext()); yield return null;
            Assert.AreEqual(1, world.Dummy.Model.PhysicalHits); Assert.AreEqual(90, world.Dummy.Model.Health); Assert.AreEqual(1, world.Dummy.Model.Reactions);
        }
        [UnityTest] public IEnumerator ReachUsesCommonOriginAndTraceCanBeRehitAtRest()
        {
            world.Player.Teleport(Vector2.zero);
            world.ExecuteAttack(world.Player, AttackKind.Single, Element.Water, new ActionContext());
            Assert.That(world.LastAttackReach, Is.EqualTo(world.Tuning.Range).Within(.0001));
            world.ExecuteAttack(world.Player, AttackKind.Charged, Element.Water, new ActionContext());
            Assert.That(world.LastAttackReach, Is.EqualTo(world.Tuning.Range * 1.75f).Within(.0001));
            Assert.LessOrEqual(world.Trace.Target.HitCollider.bounds.max.x, world.Player.AttackOrigin.x + world.Tuning.Range + .001f);
            world.ExecuteAttack(world.Player, AttackKind.Single, Element.Ice, new ActionContext()); yield return null;
            Assert.AreEqual(Element.Ice, world.Trace.Element); Assert.IsTrue(world.Trace.Platform); Assert.IsTrue(world.Trace.Target.SolidCollider.enabled);
        }
        [UnityTest] public IEnumerator CreatingActionAndSameElementDoNotChangeTrace()
        {
            var action = new ActionContext(); world.CreateTrace(Element.Water, world.Player.AttackOrigin, 1, action.Id);
            float expiry = world.Trace.ExpiresAt;
            var r = world.Trace.Target.Receive(action, Effect.Cold, 0, 1); Assert.That(r.Reason, Does.Contain("CreatingAction"));
            r = world.Trace.Target.Receive(new ActionContext(), Effect.Moisture, 0, 1); Assert.That(r.Reason, Does.Contain("SameElement"));
            yield return null; Assert.AreEqual(Element.Water, world.Trace.Element); Assert.AreEqual(expiry, world.Trace.ExpiresAt);
        }
        [UnityTest] public IEnumerator TransformationRestoresWaterProfileWithoutRefreshingExpiry()
        {
            world.CreateTrace(Element.Water, world.Player.AttackOrigin, 1, new ActionContext().Id); float expiry = world.Trace.ExpiresAt;
            world.Trace.Target.Receive(new ActionContext(), Effect.Cold, 0, 1); Assert.IsTrue(world.Trace.Target.SolidCollider.enabled);
            world.Trace.Target.Receive(new ActionContext(), Effect.Heat, 0, 1); yield return null;
            Assert.AreEqual(Element.Water, world.Trace.Element); Assert.IsTrue(world.Trace.Target.Model.State.Liquid);
            Assert.IsTrue(world.Trace.Target.Model.Profile.Has(Trait.WindReactive)); Assert.IsFalse(world.Trace.Target.SolidCollider.enabled); Assert.AreEqual(expiry, world.Trace.ExpiresAt);
        }
        [UnityTest] public IEnumerator UnsupportedAirIceDoesNotCreateAPlatform()
        {
            var fakeSupport = new GameObject("Temporary ice support"); fakeSupport.transform.SetParent(world.transform); fakeSupport.transform.position = new Vector3(1.4f, 2.2f);
            fakeSupport.AddComponent<BoxCollider2D>().size = new Vector2(1.2f, .2f); fakeSupport.AddComponent<LabSurface>().PermanentSupport = false;
            world.CreateTrace(Element.Water, new Vector2(0, 2.8f), 1, new ActionContext().Id);
            world.Trace.Target.Receive(new ActionContext(), Effect.Cold, 0, 1); yield return null;
            Assert.IsFalse(world.Trace.Platform); Assert.IsFalse(world.Trace.Target.SolidCollider.enabled); Assert.That(world.Trace.Note, Does.Contain("지지면 없음"));
        }
        [UnityTest] public IEnumerator NewChargeReplacesFlyingProductInSameSlot()
        {
            world.CreateTrace(Element.Fire, world.Player.AttackOrigin, 1, new ActionContext().Id); var old = world.Trace;
            old.Target.Receive(new ActionContext(), Effect.WindImpulse, 0, 1); Assert.IsTrue(old.Flying);
            world.CreateTrace(Element.Water, world.Player.AttackOrigin, 1, new ActionContext().Id); yield return null;
            Assert.IsFalse(old.Active); Assert.IsTrue(world.Trace.Active); Assert.AreEqual(1, world.GetComponentsInChildren<LabTarget>().Length - world.Targets.Count);
        }
        [UnityTest] public IEnumerator TransportDriesWetTargetExactlyOnceAndStops()
        {
            var target = world.SpawnTarget(TargetKind.Vine, "Wet transport target", Preset.WetVine, new Vector2(3.1f, 0), new Vector2(.4f, 1));
            world.CreateTrace(Element.Fire, world.Player.AttackOrigin, 1, new ActionContext().Id);
            world.Trace.Target.Receive(new ActionContext(), Effect.WindImpulse, 0, 1);
            yield return new WaitForSeconds(.45f);
            Assert.AreEqual(Moisture.Dry, target.Model.State.Moisture); Assert.IsFalse(target.Model.State.Burning); Assert.AreEqual(1, target.Model.Reactions); Assert.IsFalse(world.Trace.Active);
        }
        [UnityTest] public IEnumerator WallStopsTransportBeforeTarget()
        {
            var wall = new GameObject("Transport wall"); wall.transform.SetParent(world.transform); wall.transform.position = new Vector3(2.2f, .8f); wall.AddComponent<BoxCollider2D>().size = new Vector2(.2f, 2); wall.AddComponent<LabSurface>();
            var target = world.SpawnTarget(TargetKind.Vine, "Beyond wall", Preset.WetVine, new Vector2(3.1f, 0), new Vector2(.4f, 1));
            world.CreateTrace(Element.Fire, world.Player.AttackOrigin, 1, new ActionContext().Id); world.Trace.Target.Receive(new ActionContext(), Effect.WindImpulse, 0, 1);
            yield return new WaitForSeconds(.4f);
            Assert.AreEqual(Moisture.Wet, target.Model.State.Moisture); Assert.AreEqual(0, target.Model.Reactions); Assert.IsFalse(world.Trace.Active);
        }
        [UnityTest] public IEnumerator LeftwardTraceAndTransportRespectDirection()
        {
            world.CreateTrace(Element.Water, new Vector2(2, .62f), -1, new ActionContext().Id);
            Assert.Less(world.Trace.Position.x, 2); float start = world.Trace.Position.x;
            world.Trace.Target.Receive(new ActionContext(), Effect.WindImpulse, 0, -1); yield return new WaitForSeconds(.1f);
            Assert.Less(world.Trace.Position.x, start); Assert.LessOrEqual(world.Trace.Traveled, world.Tuning.Range * 3);
        }
        [UnityTest] public IEnumerator TraceActuallyExpiresAndDoesNotDamageNearbyTargetEveryFrame()
        {
            var target = world.SpawnTarget(TargetKind.Dummy, "Near trace", Preset.DryVine, new Vector2(1.4f, 0), new Vector2(.3f, 1));
            world.CreateTrace(Element.Fire, world.Player.AttackOrigin, 1, new ActionContext().Id); yield return new WaitForSeconds(2.15f);
            Assert.IsFalse(world.Trace.Active); Assert.AreEqual(100, target.Model.Health); Assert.IsFalse(target.Model.State.Burning);
        }
        [UnityTest] public IEnumerator PanelPausesWorldAndClearsPendingCharge()
        {
            world.Player.Input.Press(true); world.Player.Input.Tick(.6f); world.SetPanel(true); float clock = world.Clock;
            yield return new WaitForSeconds(.2f);
            Assert.AreEqual(clock, world.Clock); Assert.IsFalse(world.Player.Input.Held); Assert.AreEqual(0, world.ActionCount);
            world.SetPanel(false); yield return null; Assert.Greater(world.Clock, clock);
        }
        [UnityTest] public IEnumerator DummyApplyRejectsContradictionsAndResetRemembersLastApply()
        {
            world.Hud.LoadPreset((int)Preset.WetVine); world.Hud.DraftDry = true;
            Assert.IsFalse(world.Hud.ApplyDraft()); Assert.That(world.Hud.ValidationMessage, Does.Contain("Dry + Wet"));
            world.Hud.DraftDry = false; Assert.IsTrue(world.Hud.ApplyDraft());
            world.Dummy.Receive(new ActionContext(), Effect.Heat, 10, 1); Assert.AreEqual(Moisture.Dry, world.Dummy.Model.State.Moisture);
            world.Dummy.ResetTarget(); Assert.AreEqual(Moisture.Wet, world.Dummy.Model.State.Moisture); Assert.AreEqual(100, world.Dummy.Model.Health);
            world.Dummy.DefaultPreset(); Assert.AreEqual(Moisture.Dry, world.Dummy.Model.State.Moisture); yield return null;
        }
        [UnityTest] public IEnumerator ResetRestoresConsumedObjectsEnemyMotionAndTrace()
        {
            var vine = world.Targets.Find(t => t.Kind == TargetKind.Vine); vine.Receive(new ActionContext(), Effect.Heat, 0, 1); vine.Model.Tick(3.1f);
            var box = world.Targets.Find(t => t.Kind == TargetKind.Box); box.transform.position += Vector3.right * 2; box.PushVelocity = 4;
            var enemy = world.Targets.Find(t => t.Kind == TargetKind.Enemy); enemy.Receive(new ActionContext(), Effect.Cold, 10, 1);
            world.CreateTrace(Element.Fire, world.Player.AttackOrigin, 1, new ActionContext().Id); world.OpenBridge(); world.ResetLab(); yield return null;
            Assert.IsTrue(vine.Model.Alive); Assert.AreEqual(box.Spawn.x, box.transform.position.x, .01f); Assert.AreEqual(0, box.PushVelocity);
            Assert.AreEqual(100, enemy.Model.Health); Assert.IsFalse(enemy.Model.State.Frozen); Assert.IsNull(world.Trace); Assert.IsFalse(world.BridgeOpen); Assert.AreEqual(0, world.History.Count);
        }
        [UnityTest] public IEnumerator FrozenEnemyCancelsAttackAndMustWindUpAgain()
        {
            var enemy = world.Targets.Find(t => t.Kind == TargetKind.Enemy); world.Player.Teleport((Vector2)enemy.transform.position - Vector2.right * .8f); yield return null;
            Assert.AreEqual(1, enemy.EnemyPhase); enemy.Receive(new ActionContext(), Effect.Cold, 0, 1);
            yield return new WaitForSeconds(.3f); Assert.AreEqual(0, enemy.EnemyHits); Assert.AreEqual(0, enemy.EnemyPhase);
            enemy.Receive(new ActionContext(), Effect.Heat, 0, 1); yield return new WaitForSeconds(.2f);
            Assert.AreEqual(0, enemy.EnemyHits); Assert.AreEqual(1, enemy.EnemyPhase);
        }
        [UnityTest] public IEnumerator WetAttackPhaseAndHitUseSameSlowerClock()
        {
            var enemy = world.Targets.Find(t => t.Kind == TargetKind.Enemy); enemy.Receive(new ActionContext(), Effect.Moisture, 0, 1);
            world.Player.Teleport((Vector2)enemy.transform.position - Vector2.right * .8f); yield return new WaitForSeconds(.7f);
            Assert.AreEqual(0, enemy.EnemyHits); Assert.AreEqual(1, enemy.EnemyPhase); Assert.Less(enemy.EnemyAnimationPhase, world.Tuning.EnemyWindup);
            yield return new WaitForSeconds(.2f); Assert.AreEqual(1, enemy.EnemyHits);
        }
        [UnityTest] public IEnumerator InputSystemTapAndHoldReallyReachAttackController()
        {
            keyboard = InputSystem.AddDevice<Keyboard>(); world.InputEnabled = true;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J)); yield return new WaitForSeconds(.1f);
            Assert.IsTrue(world.Player.Input.Held); Assert.AreEqual(0, world.ActionCount);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSeconds(.38f);
            Assert.AreEqual(1, world.ActionCount); Assert.IsNull(world.Trace);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J)); yield return new WaitForSeconds(.7f);
            Assert.AreEqual(1, world.ActionCount);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSeconds(.2f);
            Assert.AreEqual(2, world.ActionCount); Assert.IsNotNull(world.Trace); Assert.IsTrue(world.Trace.Active);
        }
        [UnityTest] public IEnumerator WetSlowDoesNotChangeWindLiftOrGravity()
        {
            var enemy = world.Targets.Find(t => t.Kind == TargetKind.Enemy);
            enemy.Receive(new ActionContext(), Effect.Moisture, 0, 1);
            enemy.Receive(new ActionContext(), Effect.WindImpulse, 0, 1);
            Assert.AreEqual(world.Tuning.PushLift, enemy.Motor.Velocity.y);
            float velocity = enemy.Motor.Velocity.y; enemy.Tick(.05f);
            Assert.That(enemy.Motor.Velocity.y, Is.EqualTo(velocity - world.Tuning.Gravity * .05f).Within(.001f));
            Assert.That(enemy.PushVelocity, Is.EqualTo(world.Tuning.PushSpeed - 12 * .05f).Within(.001f));
            float lifted = enemy.Motor.Velocity.y;
            var repeat = enemy.Receive(new ActionContext(), Effect.WindImpulse, 0, 1);
            Assert.That(repeat.Reason, Does.Contain("Cooldown")); Assert.AreEqual(lifted, enemy.Motor.Velocity.y);
            yield return null;
        }
        [UnityTest] public IEnumerator JumpDashHitAndDeathCancelPendingCharge()
        {
            keyboard = InputSystem.AddDevice<Keyboard>(); world.InputEnabled = true;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J)); yield return new WaitForSeconds(.1f);
            Assert.IsTrue(world.Player.Input.Held);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J, Key.Space)); yield return null; yield return null;
            Assert.IsFalse(world.Player.Input.Held); Assert.Greater(world.Player.Motor.Velocity.y, 0);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return new WaitForSeconds(.1f);
            Assert.AreEqual(0, world.ActionCount);
            world.Player.Teleport(Vector2.zero); yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J)); yield return new WaitForSeconds(.1f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J, Key.LeftShift)); yield return null; yield return null;
            Assert.IsFalse(world.Player.Input.Held); InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSeconds(.25f); Assert.AreEqual(0, world.ActionCount);
            world.Player.Input.Press(true); world.Player.Damage(100, 1);
            Assert.IsFalse(world.Player.Alive); Assert.IsFalse(world.Player.Input.Held);
            world.ResetLab(); Assert.IsTrue(world.Player.Alive); Assert.AreEqual(100, world.Player.Health);
        }
        [UnityTest] public IEnumerator FeelToggleDisablesFlashAndDotNeverRestartsIt()
        {
            world.GameFeel = false; world.Dummy.Receive(new ActionContext(), Effect.Heat, 10, 1); Assert.AreEqual(0, world.Dummy.Flash);
            world.ResetLab(); world.GameFeel = true;
            world.Dummy.Receive(new ActionContext(), Effect.Heat, 10, 1); Assert.Greater(world.Dummy.Flash, 0);
            yield return new WaitForSeconds(.65f); Assert.AreEqual(0, world.Dummy.Flash); Assert.AreEqual(1, world.Dummy.Model.DotTicks);
        }
        [UnityTest] public IEnumerator ResetAndTeleportRefreshGroundBeforeTheNextInput()
        {
            world.Player.Teleport(new Vector2(0, 3)); Assert.IsFalse(world.Player.Grounded);
            world.ResetLab(); Assert.IsTrue(world.Player.Grounded);
            world.Player.Input.Press(world.Player.Grounded); world.Player.Input.Tick(.65f);
            Assert.AreEqual(AttackKind.Charged, world.Player.Input.Release(world.Player.Grounded));
            yield return null;
        }
        [UnityTest] public IEnumerator WindowKeyMessagesReachControllerAndDoNotDuplicateRawInput()
        {
            keyboard = InputSystem.AddDevice<Keyboard>(); world.InputEnabled = true;
            LabKeyboardInterop.Forward(Key.J, true); yield return new WaitForSeconds(.1f);
            Assert.IsTrue(world.Player.Input.Held); LabKeyboardInterop.Forward(Key.J, false);
            yield return new WaitForSeconds(.4f); Assert.AreEqual(1, world.ActionCount);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J)); yield return new WaitForSeconds(.1f);
            LabKeyboardInterop.Forward(Key.J, true); yield return new WaitForSeconds(.1f);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; yield return null;
            LabKeyboardInterop.Forward(Key.J, false); yield return new WaitForSeconds(.4f);
            Assert.AreEqual(2, world.ActionCount); Assert.IsNull(world.Trace);
        }
        [UnityTest] public IEnumerator InputFocusAndPanelBlockHeldJNumberAndReset()
        {
            keyboard = InputSystem.AddDevice<Keyboard>(); world.InputEnabled = true; world.Player.Element = Element.Water;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J)); yield return new WaitForSeconds(.1f);
            Assert.IsTrue(world.Player.Input.Held);
            world.SetInputFocus(false); InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; world.SetInputFocus(true);
            yield return new WaitForSeconds(.15f); Assert.AreEqual(0, world.ActionCount);
            world.SetPanel(true); world.Player.transform.position = new Vector3(2, 0);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J, Key.Digit1, Key.R)); yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
            Assert.AreEqual(Element.Water, world.Player.Element); Assert.AreEqual(2, world.Player.transform.position.x); Assert.AreEqual(0, world.ActionCount);
        }
    }
}
