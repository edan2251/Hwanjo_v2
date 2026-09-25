using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Hwanjo.ElementLab.Tests
{
    public class V02ContactTests : InputTestFixture
    {
        LabWorld world;
        [UnitySetUp] public IEnumerator SetUp() { world = new GameObject("V02 contacts").AddComponent<LabWorld>(); world.InputEnabled = false; world.GameFeel = false; world.SetInputFocus(true); yield return null; }
        [UnityTearDown] public IEnumerator Clean() { Object.Destroy(world.gameObject); yield return null; }
        [UnityTest] public IEnumerator FourDirectionsReachOnlyTheirOwnSurfaceAndOneR()
        {
            world.Player.Teleport(new Vector2(0, 4));
            Vector2 origin = world.Player.AttackOrigin;
            foreach (SlashDirection d in System.Enum.GetValues(typeof(SlashDirection)))
            {
                world.Player.Teleport(new Vector2(0, 4)); origin = world.Player.AttackOrigin;
                var inside = world.SpawnTarget(TargetKind.Rope,"near",Preset.DryVine,origin + d.Vector()*1.58f - Vector2.up*.015f,new Vector2(.03f,.03f));
                var outside = world.SpawnTarget(TargetKind.Rope,"far",Preset.DryVine,origin + d.Vector()*1.65f - Vector2.up*.015f,new Vector2(.03f,.03f));
                var behind = world.SpawnTarget(TargetKind.Rope,"back",Preset.DryVine,origin - d.Vector()*.7f - Vector2.up*.015f,new Vector2(.03f,.03f));
                foreach (var kind in new[] { AttackKind.Single, AttackKind.Charged })
                {
                    inside.DefaultPreset(); world.ExecuteAttack(world.Player,kind,Element.Fire,new ActionContext(),d.Vector());
                    Assert.IsTrue(inside.Model.State.Burning,d.ToString()); Assert.AreEqual(0,outside.Model.Reactions); Assert.AreEqual(0,behind.Model.Reactions);
                    Assert.AreEqual(1.6f,world.LastAttackReach,.001f);
                }
                world.Targets.Remove(inside);world.Targets.Remove(outside);world.Targets.Remove(behind);
                Object.Destroy(inside.gameObject);Object.Destroy(outside.gameObject);Object.Destroy(behind.gameObject);yield return null;
            }
        }
        [UnityTest] public IEnumerator GroundDownFreezeWalkAndHeatThawUsesExposedWaterSurface()
        {
            var water=world.Targets.Find(t=>t.Kind==TargetKind.Water);
            world.Player.Teleport(new Vector2(24.12f,0));
            Assert.IsTrue(world.Player.Grounded);
            world.ExecuteAttack(world.Player,AttackKind.Single,Element.Ice,new ActionContext(),Vector2.down);
            Assert.IsTrue(water.Model.State.Frozen); Assert.IsTrue(water.SolidCollider.enabled);
            world.Player.Motor.Move(.3f,4.2f,world.Tuning.Gravity);
            Assert.Greater(world.Player.transform.position.x,25); Assert.GreaterOrEqual(world.Player.transform.position.y,-.04f);
            world.ExecuteAttack(world.Player,AttackKind.Single,Element.Fire,new ActionContext(),Vector2.down);
            Assert.IsFalse(water.Model.State.Frozen);Assert.IsFalse(water.SolidCollider.enabled);yield return null;
        }
        [UnityTest] public IEnumerator SolidFloorOccludesWaterBelowButMissIsDifferentFromBlocked()
        {
            var water=world.SpawnTarget(TargetKind.Water,"hidden water",Preset.Water,new Vector2(0,-.6f),new Vector2(1,.2f));
            world.ExecuteAttack(world.Player,AttackKind.Single,Element.Ice,new ActionContext(),Vector2.down);
            Assert.IsFalse(water.Model.State.Frozen);Assert.That(string.Join(" ",world.History),Does.Contain("지형에 가려져"));yield return null;
        }
        [UnityTest] public IEnumerator FourTracesRehitAtRestStayInWorldAndFailureKeepsSlot()
        {
            foreach(SlashDirection d in System.Enum.GetValues(typeof(SlashDirection)))
            {
                world.Player.Teleport(new Vector2(0,3)); var o=world.Player.AttackOrigin;
                world.CreateTrace(Element.Fire,o,d.Vector(),new ActionContext().Id);var trace=world.Trace;
                Assert.IsTrue(trace.Active);Assert.Less((trace.Position-o).magnitude,1.6f);
                world.ExecuteAttack(world.Player,AttackKind.Single,Element.Wind,new ActionContext(),d.Vector());Assert.IsTrue(trace.Flying);
                trace.Tick(.05f);Assert.Greater(Vector2.Dot(trace.Position-o,d.Vector()),.96f);
            }
            world.CreateTrace(Element.Water,new Vector2(0,.62f),1,new ActionContext().Id);var old=world.Trace;var position=old.Position;
            world.Player.Teleport(new Vector2(10,0));Assert.AreEqual(position,old.Position);
            world.CreateTrace(Element.Fire,new Vector2(0,-.8f),1,new ActionContext().Id);Assert.AreSame(old,world.Trace);Assert.IsTrue(old.Active);yield return null;
        }
        [UnityTest] public IEnumerator ColdTraceNeverMakesAFreePlatformAndHeatEndsIt()
        {
            world.CreateTrace(Element.Ice,world.Player.AttackOrigin,1,new ActionContext().Id);var cold=world.Trace;
            Assert.IsFalse(cold.Platform);Assert.IsFalse(cold.SolidIce);cold.Target.Receive(new ActionContext(),Effect.Heat,0,1);Assert.IsFalse(cold.Active);yield return null;
        }
        [UnityTest] public IEnumerator PreparedMaterialAndPlayerTraceBothReachTheSameLabRope()
        {
            var rope=world.Targets.Find(t=>t.PuzzleRope);world.Player.Teleport(new Vector2(56.7f,0));
            world.ExecuteAttack(world.Player,AttackKind.Single,Element.Wind,new ActionContext(),Vector2.right);yield return new WaitForSeconds(.6f);
            Assert.IsTrue(rope.Model.State.Burning);Assert.IsNull(world.Trace);
            world.ResetLab();world.SetPreparedMaterial(false);world.Player.Teleport(new Vector2(56.7f,0));
            world.ExecuteAttack(world.Player,AttackKind.Charged,Element.Fire,new ActionContext(),Vector2.right);
            world.ExecuteAttack(world.Player,AttackKind.Single,Element.Wind,new ActionContext(),Vector2.right);yield return new WaitForSeconds(.7f);
            Assert.IsTrue(rope.Model.State.Burning);Assert.IsFalse(world.Trace.Active);
        }
        [UnityTest] public IEnumerator ReleaseLocksDirectionAndElementAndAirLongHoldIsCancelled()
        {
            var k=InputSystem.AddDevice<Keyboard>();world.InputEnabled=true;
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.J,Key.W));yield return new WaitForSeconds(.1f);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.W));yield return null;yield return null;
            Assert.AreEqual(SlashDirection.Up,world.Player.Direction);
            InputSystem.QueueStateEvent(k,new KeyboardState(Key.S,Key.Digit2));yield return new WaitForSeconds(.1f);
            Assert.AreEqual(SlashDirection.Up,world.Player.Direction);Assert.AreEqual(Element.Fire,world.Player.LockedElement);
            Assert.Greater(world.LastAttackEnd.y,world.LastAttackOrigin.y);
            world.Player.CancelAttack();world.Player.Teleport(new Vector2(0,3));InputSystem.QueueStateEvent(k,new KeyboardState(Key.J));yield return new WaitForSeconds(.7f);
            int count=world.ActionCount;InputSystem.QueueStateEvent(k,new KeyboardState());yield return new WaitForSeconds(.3f);Assert.AreEqual(count,world.ActionCount);
        }
    }
}
