using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hwanjo.ElementLab
{
    public sealed class LabExit
    {
        public string To, Label, Gate;
        public Vector2 At, Arrival;
        public LabExit(string to, string label, Vector2 at, Vector2 arrival, string gate = "") { To=to;Label=label;At=at;Arrival=arrival;Gate=gate; }
    }
    // One session, eight authored rooms. No disk save and no changes to the player motor.
    public sealed class LabExploration : MonoBehaviour
    {
        public static readonly string[] RoomIds = { "E01","E02","E03","E04","E05","E06","S01","S02" };
        public static readonly string[] RoomNames = { "입구 성소","물가 갈림길","마른 회랑","불붙은 정원","샘터 제단","깊은 연결 통로","틈새 기록실","낮은 숨은 샘" };
        public bool Active, MenuOpen, MapOpen;
        public string Confirmation = "", Room = "E01", Checkpoint = "E01", Hint = "";
        public readonly HashSet<Element> Unlocked = new HashSet<Element> { Element.Fire };
        public readonly HashSet<string> Visited = new HashSet<string>(), SeenExits = new HashSet<string>(), Discoveries = new HashSet<string>();
        public bool LadderOpen, ShortcutOpen, SecretOpen, GardenOpen;
        public Rect Bounds { get; private set; } = new Rect(-9,-2,94,12);
        public readonly List<LabExit> Exits = new List<LabExit>();
        public bool Modal => MenuOpen || MapOpen || Confirmation.Length > 0;
        public int TraceAttempts { get; private set; }
        public int TraceRehits { get; private set; }
        LabWorld world;
        Transform labRoot, roomRoot;
        List<LabTarget> labTargets;
        LabTarget vine, rope, source, pool, secretRope;
        GameObject gate, gardenGate;
        readonly List<GameObject> ladder = new List<GameObject>();
        Vector2 pickupAt, checkpointAt;
        Element? pickup;
        SpriteRenderer pickupIcon;
        float deathClock;
        public void Initialize(LabWorld owner)
        {
            world=owner; labRoot=world.ContentRoot; labTargets=new List<LabTarget>(world.Targets);
            MenuOpen = world.Art != null; // Test fixtures without art start in the laboratory.
        }
        public static string Name(string id) { int i=Array.IndexOf(RoomIds,id); return i>=0 ? RoomNames[i] : id; }
        public bool Has(Element e) => !Active || Unlocked.Contains(e);
        public void StartLab()
        {
            ClearRoom(); Active=false; MenuOpen=MapOpen=false;Confirmation="";world.Paused=world.PanelOpen=world.HelpOpen=false;
            world.ContentRoot=labRoot;labRoot.gameObject.SetActive(true);world.Targets.Clear();world.Targets.AddRange(labTargets);
            Bounds=new Rect(-9,-2,94,12);world.ResetLab();world.SnapCamera();
        }
        public void StartExploration(bool fresh = false)
        {
            if(fresh) { Unlocked.Clear();Unlocked.Add(Element.Fire);Visited.Clear();SeenExits.Clear();Discoveries.Clear();LadderOpen=ShortcutOpen=SecretOpen=GardenOpen=false;Checkpoint="E01";Room="E01";TraceAttempts=TraceRehits=0; }
            Active=true;MenuOpen=MapOpen=false;Confirmation="";world.Paused=world.PanelOpen=world.HelpOpen=false;
            labRoot.gameObject.SetActive(false);Enter(Checkpoint,new Vector2(2,0),true);
        }
        public void OpenMenu() { MenuOpen=true;MapOpen=false;Confirmation="";world.PanelOpen=world.HelpOpen=false;world.Player.CancelAttack(); }
        public void ToggleMap() { if(!Active || MenuOpen) return;MapOpen=!MapOpen;world.Player.CancelAttack(); }
        public void RequestReset(bool fresh=false) { Confirmation=fresh?"fresh":"room";world.Player.CancelAttack(); }
        public void Confirm()
        {
            var what=Confirmation;Confirmation="";world.Paused=false;
            if(what=="fresh") StartExploration(true);else RestartRoom();
        }
        public void RestartRoom() { if(Active) Enter(Room,new Vector2(2,0),true);else world.ResetLab(); }
        public void Respawn() { if(Active) Enter(Checkpoint,new Vector2(2,0),true);else world.ResetLab(); }
        public void TraceCreated() { if(Active) TraceAttempts++; }
        public void TraceRehit() { if(Active) TraceRehits++; }
        void ClearRoom()
        {
            if(roomRoot) {roomRoot.gameObject.SetActive(false);Destroy(roomRoot.gameObject);roomRoot=null;}
            world.ClearTrace();world.ClearTransientEffects();
        }
        public void Enter(string room, Vector2 arrival, bool heal=false)
        {
            if(Array.IndexOf(RoomIds,room)<0) throw new ArgumentException("Unknown room");
            ClearRoom();Room=room;Visited.Add(room);Exits.Clear();ladder.Clear();world.Targets.Clear();
            vine=rope=source=pool=secretRope=null;gate=gardenGate=null;pickup=null;pickupIcon=null;checkpointAt=new Vector2(-99,-99);
            roomRoot=new GameObject(room+" "+Name(room)).transform;roomRoot.SetParent(world.transform);world.ContentRoot=roomRoot;
            // Leave lower viewport room for the HUD when standing on the -1.2 river bank.
            Bounds=new Rect(0,-3.7f,30,13.2f);world.PanelOpen=world.HelpOpen=world.Paused=MapOpen=false;Confirmation="";deathClock=0;
            Build();
            world.Ground(-1,0,8,12);world.Ground(30,31,8,12);
            var selected=world.Player.Element;
            if(heal) world.Player.ResetPlayer(arrival);else world.Player.Teleport(arrival);
            world.Player.Element=Unlocked.Contains(selected)?selected:Element.Fire;
            Physics2D.SyncTransforms();world.Player.Motor.Clear();world.SnapCamera();
            Hint=Name(Room)+" · 출구/제단 가까이에서 E";world.RecordTimer(null,"입장 · "+Name(Room));
        }
        void Exit(string to,string label,float x,float y,float arriveX=2,float arriveY=0,string condition="")
        {
            Exits.Add(new LabExit(to,label,new Vector2(x,y),new Vector2(arriveX,arriveY),condition));
            var marker=LabSprites.Quad("Door · "+label,roomRoot,new Vector2(x,y+.72f),new Vector2(.8f,1.4f),new Color(.16f,.31f,.32f,.9f),8);
            LabSprites.Quad("Lit doorway edge",marker.transform,new Vector2(-.45f,0),new Vector2(.08f,1),LabSprites.Hex("d9bb77"),9);
        }
        void Pickup(Element element,float x,float y)
        {
            pickup=element;pickupAt=new Vector2(x,y);
            pickupIcon=LabSprites.Quad("권능 제단",roomRoot,pickupAt+Vector2.up*.7f,new Vector2(.28f,.4f),LabSprites.ElementColor(element),19);
            pickupIcon.transform.localRotation=Quaternion.Euler(0,0,45);
            world.Ground(x-.6f,x+.6f,y-.03f);
        }
        void CheckpointAt(float x,float y)
        {
            checkpointAt=new Vector2(x,y);
            LabSprites.Quad("체크포인트 등",roomRoot,checkpointAt+Vector2.up*.8f,new Vector2(.18f,1.5f),LabSprites.Hex("b8c68f"),9);
        }
        LabTarget Target(TargetKind kind,string name,Preset preset,float x,float y,float w,float h,bool burn=false) => world.SpawnTarget(kind,name,preset,new Vector2(x,y),new Vector2(w,h),burn);
        void Steps(float start,float top,int count,bool filled=true)
        { for(int i=0;i<count;i++) world.Ground(start+i*1.65f,start+i*1.65f+1.8f,top+i*.85f,filled?top+i*.85f+2:1.6f); }
        void Build()
        {
            switch(Room)
            {
                case "E01":
                    world.Ground(0,30,0);CheckpointAt(3,0);Steps(12,.85f,3);world.Ground(17,21,2.55f);
                    world.Ground(21,22.8f,1.7f,3.7f);world.Ground(22.8f,24.6f,.85f,2.85f);
                    Exit("E02","물가로",27,0); if(ShortcutOpen) Exit("E06","열린 지름길",19,2.55f,23,2.55f);
                    break;
                case "E02":
                    world.Ground(0,7,0);world.Ground(7,10,-1.2f);world.Ground(23,30,-1.2f);
                    Steps(3,.85f,4,false);world.Ground(7.2f,9.5f,3.4f);
                    pool=Target(TargetKind.Water,"돌아올 물길",Preset.Water,16.5f,-1.65f,13,.45f);
                    Exit("E01","입구 성소",1,0,26,0);Exit("E03","위쪽 육로",8.4f,3.4f);
                    Exit("E01","아래쪽 성소 복귀로",7.4f,-1.2f,26,0);
                    Exit("E03","회랑으로 이어진 사다리",8.8f,-1.2f,2,0,"ladder");
                    Exit("E06","동결한 물길 너머",27,-1.2f,2,0,"ice");
                    break;
                case "E03":
                    world.Ground(0,30,0);Exit("E02","물가 갈림길",1,0,8.3f,3.4f);
                    vine=Target(TargetKind.Vine,"마른 덩굴 문",Preset.DryVine,7,0,.7f,3.4f,true);
                    rope=Target(TargetKind.Rope,"위의 밧줄 · 위 베기",Preset.DryVine,14,1.75f,.5f,.6f,true);
                    for(int i=0;i<3;i++) ladder.Add(world.Ground(16+i*1.7f,17.8f+i*1.7f,.85f+i*.85f));
                    world.Ground(21,27,2.55f);Pickup(Element.Water,22,2.55f);
                    foreach(var step in ladder)step.SetActive(LadderOpen);
                    if(LadderOpen) {vine.gameObject.SetActive(false);rope.gameObject.SetActive(false);}
                    Exit("E04","정원으로",25,2.55f,2,0,"water");
                    secretRope=Target(TargetKind.Rope,"높은 불씨 연결부",Preset.DryVine,4,4.4f,.5f,.65f,true);
                    Exit("S01","빛이 새는 틈",3,0,2,0,"secret");
                    break;
                case "E04":
                    world.Ground(0,30,0);Exit("E03","마른 회랑",1,0,24,2.55f);
                    source=Target(TargetKind.FreezeDevice,"타오르는 뿌리 · 물로 소화",Preset.DryVine,10,0,.8f,1.2f);
                    source.Apply(source.Model.Profile,new TargetState(GardenOpen?Moisture.Wet:Moisture.Dry,burning:!GardenOpen));
                    gardenGate=world.Ground(12,12.7f,3.5f,3.5f);
                    gardenGate.SetActive(!GardenOpen);
                    var ep=TargetProfile.For(Preset.WoodenBox);ep.Traits|=Trait.Freezable|Trait.Meltable;ep.FreezeCondition=FreezeCondition.IntrinsicMoisture;ep.TimedStatuses=true;
                    world.SpawnTarget(TargetKind.Enemy,"정원의 파수꾼",ep,new TargetState(Moisture.Dry),new Vector2(20,0),new Vector2(.85f,.9f));
                    Exit("E05","샘터 제단",27,0);break;
                case "E05":
                    world.Ground(0,21,0);world.Ground(24,30,0);world.Ground(21,24,-.65f);CheckpointAt(3,0);
                    Steps(8,.85f,3);world.Ground(13,17,2.55f);Pickup(Element.Ice,15,2.55f);
                    world.Ground(17,18.8f,1.7f,3.7f);world.Ground(18.8f,20.6f,.85f,2.85f);
                    pool=Target(TargetKind.Water,"물가와 같은 문양의 샘",Preset.Water,22.5f,-.4f,3,.4f);
                    Exit("E04","물가로 돌아가는 길",1,0,26,0);break;
                case "E06":
                    world.Ground(0,30,0);Pickup(Element.Wind,8,0);Exit("E02","물가 갈림길",1,0,26,-1.2f);
                    Steps(19,.85f,3);world.Ground(24,28,2.55f);Exit("E01","성소 지름길 개방",26,2.55f,19,2.55f,"shortcut");
                    world.Ground(14.5f,17.5f,2.15f,1);Exit("S02","아래로 흐르는 빛",16,2.15f,2,0,"spring");
                    break;
                case "S01":
                    world.Ground(0,30,0);Steps(8,.85f,3);world.Ground(13,19,2.55f);
                    Exit("E03","회랑으로 복귀",2,0,3,0);break;
                case "S02":
                    world.Ground(0,7,0);world.Ground(7,23,-.85f);world.Ground(23,30,0);world.Ground(11,19,-1.6f);
                    Target(TargetKind.Water,"숨은 맑은 샘",Preset.Water,15,-1.15f,6,.3f);Exit("E06","연결 통로로 복귀",2,0,12,0);break;
            }
            if(Room.StartsWith("S")) { Discoveries.Add(Room);LabSprites.Quad("발견 기록 비석",roomRoot,new Vector2(17,1),new Vector2(.5f,1.4f),LabSprites.Hex("bdc69a"),10); }
        }
        public bool CanExit(LabExit exit)
        {
            if(exit.Gate=="ice")return Unlocked.Contains(Element.Ice) && pool && pool.Model.State.Frozen;
            if(exit.Gate=="water")return Unlocked.Contains(Element.Water);
            if(exit.Gate=="ladder")return LadderOpen;
            if(exit.Gate=="secret")return SecretOpen;
            if(exit.Gate=="spring")return Unlocked.Contains(Element.Wind);
            return true;
        }
        public void Tick(float dt)
        {
            if(!Active || Modal || world.IsPaused)return;
            if(!world.Player.Alive) {deathClock+=Time.unscaledDeltaTime;if(deathClock>.9f)Respawn();return;}
            var pos=(Vector2)world.Player.transform.position;
            if(pos.y < -3.6f) {Respawn();return;}
            if(rope && !rope.Model.Alive && !LadderOpen) {LadderOpen=true;foreach(var step in ladder)step.SetActive(true);world.RecordTimer(null,"밧줄 해제 · 돌계단 개방");}
            if(secretRope && !secretRope.Model.Alive)SecretOpen=true;
            if(source)
            {
                bool wet=source.Model.State.Moisture==Moisture.Wet;
                if(wet)GardenOpen=true;
                if(gardenGate)gardenGate.SetActive(!GardenOpen);
                if(!wet && !source.Model.State.Burning)source.Model.Reset();
                if(!wet && Vector2.Distance(pos,(Vector2)source.transform.position)<.85f)world.Player.Damage(8,Mathf.Sign(pos.x-source.transform.position.x));
            }
            if(pickupIcon)pickupIcon.enabled=pickup.HasValue && !Unlocked.Contains(pickup.Value);
            Hint="E · 가까운 출구 / 제단    M · 방문 지도";
            bool interact=world.InputEnabled && Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame;
            if(Vector2.Distance(pos,checkpointAt)<1.2f)
            {
                Hint=Checkpoint==Room?"활성 체크포인트 · E로 체력 회복":"E · 체크포인트 활성화";
                if(interact){Checkpoint=Room;var selected=world.Player.Element;world.Player.ResetPlayer(checkpointAt);world.Player.Element=selected;world.RecordTimer(null,"체크포인트 활성 · "+Name(Room));}
            }
            if(pickup.HasValue && !Unlocked.Contains(pickup.Value) && Vector2.Distance(pos,pickupAt)<1.3f)
            {
                Hint="E · "+LabKorean.Elements[(int)pickup.Value]+" 권능 획득";
                if(interact){Unlocked.Add(pickup.Value);world.Player.Element=pickup.Value;world.Player.CancelCharge();world.RecordTimer(null,LabKorean.Elements[(int)pickup.Value]+" 권능 획득");}
            }
            foreach(var exit in Exits)
            {
                if(Vector2.Distance(pos,exit.At)>2)continue;
                // Hidden destinations are never added to the map before actual discovery.
                if(!exit.To.StartsWith("S") || Visited.Contains(exit.To))SeenExits.Add(Room+"→"+exit.To);
                if(Vector2.Distance(pos,exit.At)>1.1f)continue;
                Hint=CanExit(exit)?"E · "+exit.Label:"닫힌 길 · "+(exit.Gate=="secret"?"높은 가연성 연결부에 불씨를 보내세요":exit.Gate=="ice"?"얼음 권능으로 수면을 얼리세요":"먼저 이곳의 권능을 찾아보세요");
                if(interact && CanExit(exit)){if(exit.Gate=="shortcut")ShortcutOpen=true;Enter(exit.To,exit.Arrival);return;}
            }
        }
    }
}
