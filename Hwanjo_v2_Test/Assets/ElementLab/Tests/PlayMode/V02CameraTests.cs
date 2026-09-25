using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Hwanjo.ElementLab.Tests
{
    public class V02CameraTests
    {
        LabWorld w;
        [UnitySetUp]public IEnumerator Setup(){w=new GameObject("camera checks").AddComponent<LabWorld>();w.InputEnabled=false;w.GameFeel=false;yield return null;}
        [UnityTearDown]public IEnumerator Clean(){Object.Destroy(w.gameObject);yield return null;}
        [UnityTest]public IEnumerator DampingStartsImmediatelyConvergesAndDoesNotMovePhysics()
        {
            w.Player.Teleport(new Vector2(20,0));Vector3 start=w.Camera.transform.position;w.Player.transform.position+=Vector3.right*3;
            Vector3 p=w.Player.transform.position;var next=w.CameraRig.Tick(.016f);Assert.Greater(next.x,start.x);Assert.Less(next.x,w.CameraGoal().x);Assert.AreEqual(p,w.Player.transform.position);
            for(int i=0;i<60;i++)w.CameraRig.Tick(.016f);Assert.AreEqual(w.CameraGoal().x,w.CameraRig.Position.x,.02f);
            w.CameraRig.Damping=false;Assert.AreEqual(w.CameraGoal(),w.CameraRig.Tick(.016f));yield return null;
        }
        [UnityTest]public IEnumerator RoomTransitionAndRespawnClearCameraVelocityAndRespectHalfViewBounds()
        {
            w.Player.transform.position=new Vector3(50,2);w.CameraRig.Tick(.03f);Assert.AreNotEqual(Vector2.zero,w.CameraRig.Velocity);
            w.Session.StartExploration(true);Assert.AreEqual(Vector2.zero,w.CameraRig.Velocity);Assert.AreEqual(w.CameraGoal(),w.Camera.transform.position);
            float half=w.Camera.orthographicSize*w.Camera.aspect;Assert.GreaterOrEqual(w.Camera.transform.position.x,w.Session.Bounds.xMin+half-.01f);
            w.Session.Checkpoint="E05";w.Session.Respawn();Assert.AreEqual(Vector2.zero,w.CameraRig.Velocity);Assert.AreEqual("E05",w.Session.Room);yield return null;
        }
        [UnityTest]public IEnumerator LowestLandingSurfaceStaysAboveFooterInEveryRoom()
        {
            w.Session.StartExploration(true);
            foreach(string id in LabExploration.RoomIds)
            {
                float floor=id=="E02"?-1.2f:id=="S02"?-.85f:0;
                w.Session.Enter(id,new Vector2(10,floor));
                float screenY=w.Camera.WorldToViewportPoint(w.Player.transform.position).y;
                Assert.Greater(screenY,.18f,id+" landing surface must clear the footer");
            }
            yield return null;
        }
    }
}
