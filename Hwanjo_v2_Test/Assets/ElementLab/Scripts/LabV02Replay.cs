using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Hwanjo.ElementLab
{
    // Explicit opt-in evidence. Traversal uses keyboard events; no room teleport or unlock calls.
    // Separate isolated motion/physics setups below are labelled as such in the log.
    public sealed class LabV02Replay : MonoBehaviour
    {
        LabWorld w; Keyboard keyboard;int checks, sequence;float lastScroll;bool scrolling;
        readonly List<KeyValuePair<string,Texture2D>> rawFrames=new List<KeyValuePair<string,Texture2D>>();
        IEnumerator Start()
        {
            w=GetComponent<LabWorld>();while(!Application.isFocused)yield return null;
            keyboard=InputSystem.AddDevice<Keyboard>();yield return new WaitForSeconds(.3f);w.GameFeel=false;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-labPerformance")>=0){yield return Performance();Finish();yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-labCameraEvidence")>=0){yield return CameraComparison();Finish();yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-labTraversalOnly")<0)
            {
            w.Session.StartLab();yield return new WaitForSeconds(.3f);yield return Shot("lab-clean");
            // Isolated animation specimen, same starting coordinates for every directional family.
            foreach(bool left in new[]{false,true})
            foreach(Key direction in new[]{Key.None,Key.W,Key.S})
            foreach(bool charged in new[]{false,true})
            {
                w.Player.Teleport(new Vector2(0,0));Send(left?Key.A:Key.D);yield return new WaitForSeconds(.05f);Send();
                yield return Attack(Element.Water,direction,charged);
                Check(w.Player.transform.rotation==Quaternion.identity,"body stays upright "+direction);
            }
            w.Player.Teleport(Vector2.zero);Send(Key.Space);yield return new WaitForSeconds(.22f);Send();yield return Attack(Element.Ice,Key.S,false);
            w.Player.Teleport(Vector2.zero);w.Vfx=false;yield return Attack(Element.Fire,Key.W,false);yield return Shot("weapon-without-vfx");w.Vfx=true;
            }
            w.Session.StartExploration(true);yield return new WaitForSeconds(.2f);
            Check(w.Session.Unlocked.Count==1,"exploration begins with fire");yield return Shot("E01");
            yield return Walk(3);yield return Press(Key.E);Check(w.Session.Checkpoint=="E01","first checkpoint by E");
            yield return Walk(27,true);yield return Door("E02");yield return Shot("E02-before-ice");
            scrolling=true;yield return Walk(8.3f,true);scrolling=false;yield return Door("E03");yield return Shot("E03");
            yield return Walk(5.8f);yield return Attack(Element.Fire,Key.None,false);yield return new WaitForSeconds(3.1f);
            yield return Walk(14);yield return Attack(Element.Fire,Key.W,false);yield return new WaitForSeconds(3.1f);Check(w.Session.LadderOpen,"upward rope opens staircase");
            yield return Walk(22,true);yield return Press(Key.E);Check(w.Session.Has(Element.Water),"water acquired by E");
            yield return Walk(25);yield return Door("E04");yield return Shot("E04-burning");
            yield return Walk(9);yield return Attack(Element.Water,Key.None,false);Check(w.Session.GardenOpen,"water extinguishes garden gate");
            yield return Walk(27);yield return Door("E05");yield return Walk(3);yield return Press(Key.E);Check(w.Session.Checkpoint=="E05","second checkpoint by E");
            scrolling=true;yield return Walk(15,true);scrolling=false;yield return Press(Key.E);Check(w.Session.Has(Element.Ice),"ice acquired on upper altar");yield return Shot("E05-altar");
            yield return Walk(21.1f);yield return Attack(Element.Ice,Key.S,false);Check(w.Targets.Find(t=>t.Kind==TargetKind.Water).Model.State.Frozen,"ground down slash freezes real spring");yield return Shot("spring-frozen");
            yield return Walk(23.2f);Check(w.Player.transform.position.y>-.08f,"walk on frozen surface");yield return Shot("walk-on-ice");
            yield return Attack(Element.Fire,Key.S,false);Check(!w.Targets.Find(t=>t.Kind==TargetKind.Water).Model.State.Frozen,"heat contacts ice surface to thaw");yield return Shot("spring-thawed");
            yield return Walk(1,true);yield return Door("E04");yield return Walk(1);yield return Door("E03");yield return Walk(1);yield return Door("E02");
            yield return Walk(10.1f);yield return Attack(Element.Ice,Key.S,false);Check(w.Targets.Find(t=>t.Kind==TargetKind.Water).Model.State.Frozen,"revisit freezes long river");yield return Shot("E02-return-frozen");
            scrolling=true;yield return Walk(27);scrolling=false;yield return Door("E06");yield return Shot("E06");
            yield return Walk(8);yield return Press(Key.E);Check(w.Session.Has(Element.Wind),"wind acquired");
            yield return Walk(26,true);yield return Door("E01");Check(w.Session.ShortcutOpen,"inside shortcut opens to sanctuary");yield return Shot("E01-shortcut");
            // Return through the newly opened shortcut, then use the optional trace step.
            yield return Press(Key.E);Check(w.Session.Room=="E06","shortcut can be revisited");
            yield return Walk(13.1f);Send(Key.D);yield return new WaitForSeconds(.016f);Send();
            yield return Attack(Element.Water,Key.None,true);yield return Attack(Element.Ice,Key.None,false);
            Check(w.Trace!=null&&w.Trace.Platform,"optional supported water-ice step");yield return Shot("S02-trace-step");
            Send(Key.Space,Key.D);yield return new WaitForSeconds(.16f);Send();
            float limit=Time.realtimeSinceStartup+1.2f;while(!w.Player.Grounded && Time.realtimeSinceStartup<limit)yield return null;
            Check(w.Player.transform.position.y>.5f,"landed on temporary step");
            yield return Walk(16,true);yield return Door("S02");yield return Shot("S02");
            yield return Walk(2);yield return Door("E06");yield return Walk(1);yield return Door("E02");
            // The river resets on re-entry; freeze its exposed right bank before returning left.
            yield return Walk(22.9f);yield return Attack(Element.Ice,Key.S,false);yield return Walk(8.8f);yield return Door("E03");
            yield return Walk(4);yield return Attack(Element.Fire,Key.W,true);yield return Attack(Element.Wind,Key.W,false);yield return new WaitForSeconds(3.5f);
            Check(w.Session.SecretOpen,"vertical fire transport opens visible high connection");yield return Walk(3);yield return Door("S01");yield return Shot("S01");
            Check(w.Session.Discoveries.Count==2,"both optional discoveries recorded");yield return Press(Key.M);yield return Shot("map-complete");yield return Press(Key.M);
            yield return Press(Key.R);yield return Shot("room-reset-confirmation");Check(w.Session.Confirmation=="room","R requests room reset instead of erasing session");w.Session.Confirm();Check(w.Session.Unlocked.Count==4&&w.Session.ShortcutOpen,"room reset preserves progression");
            w.Player.Damage(100,1);yield return new WaitForSeconds(1.2f);Check(w.Session.Room=="E05"&&w.Player.Health==100,"death returns to activated checkpoint");yield return Shot("respawn");
            w.Session.StartLab();yield return Press(Key.F2);yield return Shot("korean-dummy");yield return Press(Key.F2);yield return Press(Key.F4);yield return Shot("korean-reaction-table");yield return Press(Key.F4);
            yield return Press(Key.Escape);yield return Shot("pause-menu");yield return Press(Key.Escape);
            w.Session.OpenMenu();yield return Shot("mode-menu");Finish();
        }
        IEnumerator Attack(Element e,Key direction,bool charge)
        {
            Send((Key)((int)Key.Digit1+(int)e));yield return null;
            if(direction==Key.None)Send(Key.J);else Send(Key.J,direction);
            yield return new WaitForSeconds(charge?.62f:.075f);
            if(charge && !w.Session.Active)yield return RawShot("charge-ready-"+direction+"-"+w.Player.Facing);
            if(direction==Key.None)Send();else Send(direction);
            float until=Time.realtimeSinceStartup+.44f;int prior=-1;
            while(Time.realtimeSinceStartup<until)
            {
                if(!w.Session.Active && w.Player.CurrentAttack!=AttackKind.None && w.Player.MotionFrame!=prior)
                {prior=w.Player.MotionFrame;yield return RawShot((w.Vfx?"":"no-vfx-")+"motion-"+w.Player.MotionRow+"-"+w.Player.Facing+"-"+prior);}
                else yield return null;
            }
            Send();yield return null;
            if(rawFrames.Count>0)
            {
                w.Paused=true;
                foreach(var frame in rawFrames){System.IO.File.WriteAllBytes(System.IO.Path.Combine(w.CaptureDirectory,frame.Key+".png"),frame.Value.EncodeToPNG());Destroy(frame.Value);yield return null;}
                rawFrames.Clear();w.Paused=false;
            }
        }
        IEnumerator Walk(float x,bool jump=false)
        {
            float until=Time.realtimeSinceStartup+14,nextJump=0;
            while(Mathf.Abs(w.Player.transform.position.x-x)>.075f && Time.realtimeSinceStartup<until)
            {
                Key horizontal=w.Player.transform.position.x<x?Key.D:Key.A;
                if(jump && w.Player.Grounded && Time.realtimeSinceStartup>=nextJump) {Send(horizontal,Key.Space);nextJump=Time.realtimeSinceStartup+.18f;}
                else Send(horizontal);
                if(scrolling && Time.realtimeSinceStartup-lastScroll>.12f) {lastScroll=Time.realtimeSinceStartup;yield return Shot("scroll-"+w.Session.Room);}
                else yield return null;
            }
            Send();Check(Mathf.Abs(w.Player.transform.position.x-x)<.14f,"walk "+w.Session.Room+" x="+x+" actual="+w.Player.transform.position);
            yield return new WaitForSeconds(.7f);
        }
        IEnumerator Door(string expected) {yield return Press(Key.E);Check(w.Session.Room==expected,"door to "+expected);}
        IEnumerator Press(Key key) {Send(key);yield return new WaitForSeconds(.07f);Send();yield return new WaitForSeconds(.16f);}
        IEnumerator Shot(string name) {yield return w.Capture((sequence++).ToString("D4")+"-"+name);}
        IEnumerator RawShot(string name)
        {
            yield return new WaitForEndOfFrame();
            rawFrames.Add(new KeyValuePair<string,Texture2D>((sequence++).ToString("D4")+"-"+name+"-"+DateTime.Now.ToString("HHmmssfff"),ScreenCapture.CaptureScreenshotAsTexture()));
        }
        IEnumerator Performance()
        {
            QualitySettings.vSyncCount=0;w.Session.StartLab();
            foreach(int target in new[]{30,60,120})
            {
                Application.targetFrameRate=target;w.ResetLab();yield return new WaitForSeconds(.5f);
                var times=new List<float>();float begin=Time.realtimeSinceStartup,end=begin+3;Send(Key.D);
                while(Time.realtimeSinceStartup<end){times.Add(Time.unscaledDeltaTime);yield return null;}
                Send();float elapsed=Time.realtimeSinceStartup-begin;float distance=w.Player.transform.position.x;
                // Stop before the laboratory dummy by using a clear segment for the logical sample.
                Debug.Log("PERFORMANCE target="+target+" actualFPS="+(times.Count/elapsed).ToString("F1")+" seconds="+elapsed.ToString("F3")+" frames="+times.Count+" position="+distance.ToString("F3"));
                times.Sort();Debug.Log("FRAME_MS p50="+(times[times.Count/2]*1000).ToString("F2")+" p95="+(times[(int)(times.Count*.95f)]*1000).ToString("F2"));
                w.Player.Teleport(new Vector2(29,0));float start=w.Player.transform.position.x;Send(Key.D);yield return new WaitForSeconds(.5f);Send();
                Check(Mathf.Abs(w.Player.transform.position.x-start-2.1f)<.3f,"movement at target "+target);
                yield return Press(Key.J);Check(w.ActionCount==1,"single input at target "+target);
            }
        }
        IEnumerator CameraComparison()
        {
            w.Session.StartExploration(true);
            foreach(bool damped in new[]{true,false})
            {
                if(w.CameraRig.Damping!=damped)yield return Press(Key.F5);
                Check(w.CameraRig.Damping==damped,"F5 damping="+damped);
                scrolling=true;yield return Walk(11,true);yield return Walk(2);scrolling=false;
                Debug.Log("CAMERA_COMPARISON damping="+damped+" camera="+w.Camera.transform.position+" player="+w.Player.transform.position);
                yield return Shot(damped?"damping-on-rest":"damping-off-rest");
            }
        }
        void Send(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        void Check(bool ok,string message){if(!ok){Debug.LogError("V02_EVIDENCE_FAIL "+message);Send();enabled=false;throw new InvalidOperationException(message);}checks++;Debug.Log("V02_CHECK "+message);}
        void Finish(){Send();Debug.Log("V02_EVIDENCE_PASS checks="+checks+" captures="+sequence);w.Quit();}
        void OnDestroy(){if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);}
    }
}
