using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Hwanjo.ElementLab.Tests
{
    public class V02ExplorationTests : InputTestFixture
    {
        LabWorld w; Keyboard keyboard;
        [UnitySetUp] public IEnumerator SetUp() {w=new GameObject("exploration tests").AddComponent<LabWorld>();w.InputEnabled=false;w.GameFeel=false;w.SetInputFocus(true);yield return null;w.Session.StartExploration(true);yield return null;}
        [UnityTearDown] public IEnumerator Clean(){Object.Destroy(w.gameObject);yield return null;}
        [UnityTest] public IEnumerator ModesAndSessionResetPreserveExactlyTheSpecifiedProgress()
        {
            Assert.AreEqual(1,w.Session.Unlocked.Count);w.Select(Element.Ice);Assert.AreEqual(Element.Fire,w.Player.Element);
            w.Session.Unlocked.Add(Element.Ice);w.Session.Unlocked.Add(Element.Water);w.Session.ShortcutOpen=true;w.Session.Checkpoint="E05";w.Session.Discoveries.Add("S01");
            w.Session.Enter("E02",new Vector2(10.12f,-1.2f));var water=w.Targets.Find(t=>t.Kind==TargetKind.Water);
            w.ExecuteAttack(w.Player,AttackKind.Single,Element.Ice,new ActionContext(),Vector2.down);Assert.IsTrue(water.Model.State.Frozen);
            w.CreateTrace(Element.Fire,w.Player.AttackOrigin,Vector2.up,new ActionContext().Id);
            w.Session.RestartRoom();Assert.IsNull(w.Trace);Assert.AreEqual(3,w.Session.Unlocked.Count);Assert.IsTrue(w.Session.ShortcutOpen);
            Assert.IsFalse(w.Targets.Find(t=>t.Kind==TargetKind.Water).Model.State.Frozen);
            w.Player.Damage(100,1);yield return new WaitForSeconds(1.1f);Assert.AreEqual("E05",w.Session.Room);Assert.AreEqual(100,w.Player.Health);Assert.IsTrue(w.Session.ShortcutOpen);
            w.Session.StartLab();Assert.IsTrue(w.Session.Has(Element.Wind));Assert.AreEqual(4.2f,w.Tuning.MoveSpeed);
            w.Session.StartExploration();Assert.AreEqual(3,w.Session.Unlocked.Count);Assert.AreEqual(1,w.Session.Discoveries.Count);
            w.Session.StartExploration(true);Assert.AreEqual(1,w.Session.Unlocked.Count);Assert.IsFalse(w.Session.ShortcutOpen);Assert.AreEqual(0,w.Session.Discoveries.Count);
        }
        [UnityTest] public IEnumerator AllEightRoomsHaveSafeEntryAndRealTerrainWithNoSecretMapLeak()
        {
            Assert.IsFalse(w.Session.Visited.Contains("S01"));Assert.IsFalse(w.Session.Visited.Contains("S02"));
            foreach(var id in LabExploration.RoomIds)
            {
                w.Session.Enter(id,new Vector2(1.5f,0),true);yield return null;
                Assert.IsTrue(w.Player.Grounded,id);Assert.Greater(w.Session.Exits.Count,0,id);
                Assert.Greater(w.ContentRoot.GetComponentsInChildren<LabSurface>().Length,2,id);
                Assert.Less(w.Camera.transform.position.x,w.Session.Bounds.xMax);
            }
            Assert.AreEqual(2,w.Session.Discoveries.Count);
        }
        [UnityTest] public IEnumerator WaterGapCannotBeCrossedByMaximumJumpAndRepeatedDashBeforeIce()
        {
            keyboard=InputSystem.AddDevice<Keyboard>();
            w.Session.Enter("E02",new Vector2(9.9f,-1.2f));w.InputEnabled=true;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space,Key.D,Key.LeftShift));yield return new WaitForSeconds(.2f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));yield return new WaitForSeconds(.4f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D,Key.LeftShift));yield return new WaitForSeconds(.3f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            Assert.Less(w.Player.transform.position.x,23,"13-unit river must exceed jump/dash traversal");Assert.IsFalse(w.Session.Has(Element.Ice));
            yield return new WaitForSeconds(.7f);Assert.AreEqual("E01",w.Session.Room,"fall recovers safely");
        }
        [UnityTest] public IEnumerator UpRopeUnlockAndDownSurfaceUseActualKeysAndIndependentDurations()
        {
            keyboard=InputSystem.AddDevice<Keyboard>();
            w.Session.Enter("E03",new Vector2(14,0));w.InputEnabled=true;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.J));yield return new WaitForSeconds(.1f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return new WaitForSeconds(3.4f);
            Assert.IsTrue(w.Session.LadderOpen);
            w.Session.Unlocked.Add(Element.Ice);w.Session.Enter("E02",new Vector2(10.12f,-1.2f));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.S,Key.J,Key.Digit4));yield return new WaitForSeconds(.1f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.S));yield return new WaitForSeconds(.4f);
            var water=w.Targets.Find(t=>t.Kind==TargetKind.Water);Assert.IsTrue(water.Model.State.Frozen);
            // Leave the overhanging shore stair before making a trace above the frozen surface.
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));yield return new WaitForSeconds(.35f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.Greater(w.Player.transform.position.x,11);Assert.IsTrue(w.Player.Grounded);
            w.CreateTrace(Element.Fire,w.Player.AttackOrigin,Vector2.up,new ActionContext().Id);Assert.IsNotNull(w.Trace);yield return new WaitForSeconds(2.1f);
            Assert.IsFalse(w.Trace.Active);Assert.IsTrue(water.Model.State.Frozen);
        }
        [UnityTest] public IEnumerator MapResetAndModeMenuCancelPendingChargeAndDoNotLeakKeys()
        {
            keyboard=InputSystem.AddDevice<Keyboard>();
            w.InputEnabled=true;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.J));yield return new WaitForSeconds(.1f);
            w.Session.ToggleMap();Assert.IsFalse(w.Player.Input.Held);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.J,Key.Digit4,Key.R,Key.W));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Assert.AreEqual(0,w.ActionCount);Assert.AreEqual(Element.Fire,w.Player.Element);Assert.IsEmpty(w.Session.Confirmation);
            w.Session.ToggleMap();w.Session.RequestReset();Assert.AreEqual("room",w.Session.Confirmation);w.Session.Confirm();Assert.AreEqual("E01",w.Session.Room);
            w.Session.OpenMenu();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.J));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;Assert.AreEqual(0,w.ActionCount);
        }
    }
}
