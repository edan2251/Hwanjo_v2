using System;
using UnityEngine;

namespace Hwanjo.ElementLab
{
    public sealed class LabHud : MonoBehaviour
    {
        public LabWorld World;
        public TargetProfile DraftProfile;
        public bool DraftDry, DraftWet, DraftBurning, DraftFrozen, DraftLiquid;
        public string ValidationMessage = "";
        public int PresetIndex, MaterialIndex;
        GUIStyle text, small, title, button, toggle;
        Font font;
        string dropdown;
        Rect dropdownRect;
        string[] dropdownItems;
        Action<int> dropdownChoose;
        float scale, width;
        static readonly string[] presets = { "직접 설정", "마른 덩굴", "젖은 덩굴", "나무상자", "물", "바람 장치", "결빙 장치" };
        static readonly string[] materials = { "식물", "나무", "금속", "물", "직접 설정" };
        static readonly string[] elements = { "불", "물", "바람", "얼음" };
        static readonly string[] traitNames = { "가연성", "젖음 가능", "동결 가능", "해동 가능", "소화 가능", "이동 가능", "바람 반응" };
        static readonly Color ink = new Color(.035f, .075f, .09f, .95f), muted = new Color(.58f, .72f, .71f), gold = new Color(.88f, .76f, .49f);
        void Start() { ReadDummy(); }
        void EnsureStyles()
        {
            if (text != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 18);
            text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 16, wordWrap = true, richText = false };
            text.normal.textColor = new Color(.87f, .92f, .86f);
            small = new GUIStyle(text) { fontSize = 13 };
            title = new GUIStyle(text) { fontSize = 23, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 14, alignment = TextAnchor.MiddleCenter };
            button.normal.background = Flat(new Color(.10f, .19f, .21f)); button.normal.textColor = new Color(.88f, .94f, .91f);
            button.hover.background = Flat(new Color(.20f, .35f, .35f)); button.hover.textColor = Color.white;
            button.active.background = Flat(new Color(.38f, .41f, .29f)); button.active.textColor = Color.white;
            toggle = new GUIStyle(GUI.skin.toggle) { font = font, fontSize = 14 }; toggle.normal.textColor = text.normal.textColor;
        }
        void OnGUI()
        {
            if (!World || !World.Player) return;
            LabKeyboardInterop.OnGui(Event.current);
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-labCaptureActions") >= 0 && (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp))
                Debug.Log("NATIVE_GUI_KEY " + Event.current.type + " " + Event.current.keyCode);
            EnsureStyles(); scale = Screen.height / 720f; width = Screen.width / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            Fill(new Rect(0, 0, width, 84), ink); Fill(new Rect(0, 82, width, 2), new Color(.31f, .54f, .48f));
            Label(new Rect(24, 14, 260, 32), "환조  ·  속성 실험실", title, gold);
            Label(new Rect(26, 49, 264, 24), World.AutomatedEvidence ? "v0.2 / 자동 입력 검증 재생" : "v0.2   /   숲 유적의 속성 실험", small, muted);
            for (int i = 0; i < 4; i++)
            {
                Rect rect = new Rect(308 + i * 117, 19, 107, 44); Color c = LabSprites.ElementColor((Element)i);
                Fill(rect, World.Player.Element == (Element)i ? new Color(c.r * .32f, c.g * .32f, c.b * .32f, 1) : new Color(.1f, .16f, .18f, 1));
                Fill(new Rect(rect.x, rect.y, 3, rect.height), c);
                if (GUI.Button(rect, (i + 1) + "   " + (World.Session.Has((Element)i) ? elements[i] : "미획득"), button) && !World.IsPaused) World.Select((Element)i);
                Fill(new Rect(rect.x, rect.y, 3, rect.height), c);
                if (World.Player.Element == (Element)i) Fill(new Rect(rect.x, rect.yMax - 3, rect.width, 3), c);
            }
            float statusX = Mathf.Max(790, width - 470);
            Label(new Rect(statusX, 17, 155, 23), "HP  " + World.Player.Health.ToString("0") + " / 100", text, text.normal.textColor);
            Fill(new Rect(statusX, 48, 120, 4), new Color(.2f, .28f, .29f)); Fill(new Rect(statusX, 48, 120 * World.Player.Health / World.Tuning.PlayerHealth, 4), gold);
            Label(new Rect(statusX + 150, 17, 280, 23), World.Trace != null && World.Trace.Active ? "검흔  " + elements[(int)World.Trace.Element] + "  ·  " + World.Trace.Remaining.ToString("0.0") + "s" : "검흔  —", text, muted);
            string charge = World.Player.Input.Held ? (World.Player.Input.HeldSeconds >= .5f ? "차지 완료 · J를 놓아 베기" : "차지  " + World.Player.Input.HeldSeconds.ToString("0.00") + " / 0.50s") : "J 짧게 · 단타     J 길게 · 차지";
            Label(new Rect(statusX + 150, 45, 300, 24), charge, small, World.Player.Input.Held ? gold : muted);
            DrawWorldLabels();
            Fill(new Rect(16, 634, width - 32, 70), ink);
            Label(new Rect(31, 644, width - 62, 22), "A/D 이동    SPACE 점프    SHIFT 대시    J 베기 · W/S 위/아래    1–4 속성    R 초기화    ESC 메뉴", small, muted);
            if (GUI.Button(new Rect(31, 672, 135, 23), World.Session.Active ? "M  방문 지도" : "F2  더미 편집", button) && !World.Session.MenuOpen)
            { if(World.Session.Active)World.Session.ToggleMap();else World.SetPanel(!World.PanelOpen); }
            if (GUI.Button(new Rect(178, 672, 130, 23), "F3  타격감 " + (World.GameFeel ? "켜짐" : "꺼짐"), button)) { World.GameFeel = !World.GameFeel; World.InputEvidence("UI feedback=" + World.GameFeel); }
            string[] zones = { "01 조작", "02 환경", "03 검흔", "04 적", "05 응용" }; float[] locations = { 0, 10, 29, 43, 54 };
            for (int i = 0; i < (World.Session.Active ? 0 : zones.Length); i++)
                if (GUI.Button(new Rect(width - 610 + i * 116, 672, 106, 23), zones[i], button) && !World.IsPaused)
                { World.Player.Teleport(new Vector2(locations[i], .02f)); World.SnapCamera(); World.InputEvidence("UI zone=" + zones[i]); }
            if(World.Session.Active)Label(new Rect(340,672,560,24),World.Session.Room+" · "+LabExploration.Name(World.Session.Room)+"    발견 "+World.Session.Discoveries.Count+"/2",small,gold);
            if(GUI.Button(new Rect(320,600,145,26),"F4  반응 도움말",button) && !World.Session.MenuOpen) {World.HelpOpen=!World.HelpOpen;World.Player.CancelAttack();}
            if(World.DebugView)Label(new Rect(width-440,601,420,25),"F5 카메라 완화 "+(World.CameraRig.Damping?"켜짐":"꺼짐")+"    F6 속성 효과 "+(World.Vfx?"켜짐":"꺼짐"),small,muted);
            if(!World.Session.Active && !World.IsPaused && World.Player.transform.position.x>51 && GUI.Button(new Rect(width-400,563,376,30),"비교 재료 · 준비된 불씨 "+(World.UsePreparedMaterial?"있음 (클릭해 없애기)":"없음 (클릭해 복원)"),button))World.SetPreparedMaterial(!World.UsePreparedMaterial);
            if (World.DebugView) DrawDiagnostics();
            if (World.Session.MenuOpen) DrawStart();
            else if (World.Session.Confirmation.Length>0) DrawConfirmation();
            else if (World.Session.MapOpen) DrawMap();
            else if (World.PanelOpen) DrawPanel();
            else if (World.HelpOpen) DrawHelp();
            else if (World.Paused || !World.Player.Alive) DrawPause();
            if (dropdown != null && World.PanelOpen) DrawDropdown();
            GUI.matrix = Matrix4x4.identity;
        }
        void DrawWorldLabels()
        {
            foreach (var target in World.Targets)
            {
                if(!target || !target.gameObject.activeInHierarchy)continue;
                var point = World.Camera.WorldToScreenPoint(target.transform.position + Vector3.up * (target.Size.y + .32f));
                float x = point.x / scale, y = (Screen.height - point.y) / scale;
                if (x < -100 || x > width + 100 || y < 98 || y > 600) continue;
                Fill(new Rect(x - 68, y - 20, 136, 42), new Color(.04f, .12f, .13f, .83f));
                Label(new Rect(x - 64, y - 18, 130, 21), target.Label, small, gold);
                string state = !target.Model.Alive ? "소실 · R로 복원" : LabKorean.State(target.Model.State);
                if (target.Kind == TargetKind.Enemy && target.EnemyPhase == 1 && !target.Model.State.Frozen) state = "!  공격 예고";
                Label(new Rect(x - 64, y + 1, 130, 20), state, small, muted);
                if (target.Kind == TargetKind.Enemy)
                { Fill(new Rect(x - 48, y - 28, 96, 4), ink); Fill(new Rect(x - 48, y - 28, 96 * target.Model.Health / target.Model.MaxHealth, 4), new Color(.75f, .38f, .35f)); }
            }
            if (World.Trace != null && World.Trace.Active)
            {
                var point = World.Camera.WorldToScreenPoint(World.Trace.Position + Vector2.up * .8f);
                Label(new Rect(point.x / scale - 125, (Screen.height - point.y) / scale, 300, 37), "◇ " + World.Trace.Note, small, LabSprites.ElementColor(World.Trace.Element));
            }
            string hint = World.Player.transform.position.x < 9 ? "01  조작과 더미\n단타는 J를 놓을 때 한 번. F2로 특성을 바꿔 보세요." : World.Player.transform.position.x < 28 ? "02  같은 특성, 같은 반응\n물 → 불은 건조만. 한 번 더 불을 가하면 점화됩니다." : World.Player.transform.position.x < 41 ? "03  공간에 남긴 검흔\n차지 → 다른 속성 선택 → 제자리 단타로 재타격." : World.Player.transform.position.x < 51 ? "04  숲의 파수꾼\n물은 둔화, 얼음은 짧은 동결. 불·바람도 비교하세요." : "05  작은 응용\n건너편 밧줄에 불 검흔을 바람으로 보내세요. 수면은 얼릴 수 있습니다.";
            if(World.Session.Active)hint=World.Session.Room+"  "+LabExploration.Name(World.Session.Room)+"\n"+World.Session.Hint;
            Fill(new Rect(16, 98, 550, 65), new Color(.035f, .09f, .105f, .90f));
            Label(new Rect(25, 102, 530, 62), hint, text, new Color(.83f, .9f, .86f));
        }
        void DrawDiagnostics()
        {
            Fill(new Rect(22, 266, 740, 352), ink);
            Label(new Rect(36, 276, 710, 24), "F1  반응 기록 · 최근 8개   /   R=" + World.Tuning.Range + "  차지=" + World.Tuning.ChargeRange, small, gold);
            for (int i = 0; i < World.History.Count; i++) Label(new Rect(36, 306 + i * 38, 710, 38), World.History[i], small, muted);
            var a = World.Camera.WorldToScreenPoint(World.LastAttackOrigin); var b = World.Camera.WorldToScreenPoint(World.LastAttackEnd);
            if (World.ActionCount > 0) Fill(new Rect(Mathf.Min(a.x, b.x) / scale, (Screen.height - Mathf.Max(a.y,b.y)) / scale, Mathf.Max(2,Mathf.Abs(a.x - b.x) / scale), Mathf.Max(2,Mathf.Abs(a.y-b.y)/scale)), gold);
        }
        public void ReadDummy()
        {
            if (!World || !World.Dummy) return;
            DraftProfile = World.Dummy.Model.Profile; var s = World.Dummy.Model.State;
            DraftDry = s.Moisture == Moisture.Dry; DraftWet = s.Moisture == Moisture.Wet; DraftBurning = s.Burning; DraftFrozen = s.Frozen; DraftLiquid = s.Liquid;
            PresetIndex = 0; MaterialIndex = DraftProfile.LiquidBody ? 3 : 4; ValidationMessage = ""; dropdown = null;
        }
        public void LoadPreset(int index)
        {
            PresetIndex = index; DraftProfile = TargetProfile.For((Preset)index); var s = TargetProfile.Initial((Preset)index);
            DraftDry = s.Moisture == Moisture.Dry; DraftWet = s.Moisture == Moisture.Wet; DraftBurning = DraftFrozen = false; DraftLiquid = s.Liquid;
            MaterialIndex = index == 4 ? 3 : index == 3 ? 1 : index >= 5 ? 2 : 0; ValidationMessage = "";
        }
        public bool ApplyDraft()
        {
            if (DraftDry && DraftWet) ValidationMessage = "건조 + 젖음 동시 선택은 불가합니다.";
            else if (DraftProfile.WindRoute == WindRoute.TraceTransport) ValidationMessage = "더미는 검흔 전달 결과를 지원하지 않습니다.";
            else
            {
                var state = new TargetState(DraftWet ? Moisture.Wet : DraftDry ? Moisture.Dry : Moisture.None, DraftBurning, DraftFrozen, DraftLiquid);
                ValidationMessage = DraftProfile.Validate(state);
                if (ValidationMessage.Length == 0)
                {
                    World.History.Clear(); World.ClearTransientEffects(); World.Dummy.Apply(DraftProfile, state);
                    ValidationMessage = DraftProfile.Has(Trait.Flammable) && !DraftProfile.Has(Trait.Extinguishable) ? "적용됨 · 경고: 가연성이지만 소화할 수 없습니다." : "적용됨 · 타이머 / 피해 / 이동 / 효과 초기화";
                    World.InputEvidence("UI Apply accepted"); return true;
                }
            }
            World.InputEvidence("UI Apply rejected: " + ValidationMessage); return false;
        }
        void DrawPanel()
        {
            // Popups own pointer input. Underlying toggles must not steal their hot control.
            bool oldEnabled = GUI.enabled; if (dropdown != null) GUI.enabled = false;
            float x = width - 492, y = 98;
            Fill(new Rect(x, y, 468, 527), ink); Fill(new Rect(x, y, 3, 527), gold);
            Label(new Rect(x + 19, y + 13, 355, 28), "더미 실험 · 런타임 편집", title, gold);
            if (GUI.Button(new Rect(x + 420, y + 12, 28, 28), "×", button)) World.SetPanel(false);
            Label(new Rect(x + 19, y + 47, 430, 22), "월드 정지 중 · 적용은 반응이 아닌 편집 동작입니다", small, muted);
            Drop("프리셋", new Rect(x + 19, y + 76, 210, 29), presets[PresetIndex], presets, LoadPreset);
            Drop("재질", new Rect(x + 239, y + 76, 210, 29), materials[MaterialIndex], materials, i => { MaterialIndex = i; LoadPreset(i == 3 ? 4 : i == 1 ? 3 : i == 2 ? 6 : 1); MaterialIndex = i; });
            Label(new Rect(x + 19, y + 113, 430, 22), "특성  ·  다중 선택", small, gold);
            for (int i = 0; i < 7; i++)
            {
                Trait trait = (Trait)(1 << i); bool old = DraftProfile.Has(trait);
                bool enabled = GUI.Toggle(new Rect(x + 20 + (i % 3) * 146, y + 141 + i / 3 * 25, 148, 25), old, traitNames[i], toggle);
                if (enabled != old) { DraftProfile.Traits = enabled ? DraftProfile.Traits | trait : DraftProfile.Traits & ~trait; PresetIndex = 0; }
            }
            Label(new Rect(x + 19, y + 218, 430, 20), "초기 상태  ·  모순 조합은 적용 시 거부", small, gold);
            DraftDry = GUI.Toggle(new Rect(x + 20, y + 246, 83, 25), DraftDry, "건조", toggle);
            DraftWet = GUI.Toggle(new Rect(x + 105, y + 246, 83, 25), DraftWet, "젖음", toggle);
            DraftBurning = GUI.Toggle(new Rect(x + 190, y + 246, 100, 25), DraftBurning, "연소", toggle);
            DraftFrozen = GUI.Toggle(new Rect(x + 296, y + 246, 88, 25), DraftFrozen, "동결", toggle);
            DraftLiquid = GUI.Toggle(new Rect(x + 382, y + 246, 76, 25), DraftLiquid, "물", toggle);
            Drop("동결", new Rect(x + 19, y + 278, 210, 29), (DraftProfile.FreezeCondition == FreezeCondition.WetOrLiquid ? "젖음 / 물 필요" : "내재 수분"), new[] { "젖음 / 물 필요", "내재 수분" }, i => DraftProfile.FreezeCondition = (FreezeCondition)i);
            Drop("바람", new Rect(x + 239, y + 278, 210, 29), (DraftProfile.WindRoute == WindRoute.Device ? "장치 작동" : "몸체 밀기"), new[] { "몸체 밀기", "장치 작동", "검흔 전달 (미지원)" }, i => { if (i < 2) DraftProfile.WindRoute = (WindRoute)i; });
            DraftProfile.Fixed = GUI.Toggle(new Rect(x + 20, y + 318, 430, 24), DraftProfile.Fixed, "고정 몸체 · 바람으로 밀 수 없음", toggle);
            Label(new Rect(x + 20, y + 347, 432, 42), "현재: " + LabKorean.State(World.Dummy.Model.State) + "    HP " + World.Dummy.Model.Health.ToString("0") + "\n최근 효과: " + World.Dummy.LastEffect + "  |  " + LabKorean.ConsequenceName(World.Dummy.Model.Last.Consequence), small, muted);
            if (GUI.Button(new Rect(x + 19, y + 400, 137, 34), "설정 적용", button)) ApplyDraft();
            if (GUI.Button(new Rect(x + 165, y + 400, 138, 34), "마지막 적용 복원", button)) { World.History.Clear(); World.ClearTransientEffects(); World.Dummy.ResetTarget(); ReadDummy(); World.InputEvidence("UI Reset Target"); }
            if (GUI.Button(new Rect(x + 312, y + 400, 137, 34), "최초 프리셋 복원", button)) { World.History.Clear(); World.ClearTransientEffects(); World.Dummy.DefaultPreset(); ReadDummy(); World.InputEvidence("UI Default Preset"); }
            Label(new Rect(x + 20, y + 448, 432, 60), ValidationMessage.Length > 0 ? ValidationMessage : "마지막 적용 복원 = 직접 적용한 설정\n최초 프리셋 복원 = 시작 설정 · F1에서 최근 8개 반응 확인", small, ValidationMessage.StartsWith("적용") ? muted : gold);
            GUI.enabled = oldEnabled;
        }
        void Drop(string name, Rect rect, string value, string[] choices, Action<int> choose)
        {
            if (GUI.Button(rect, name + "  ·  " + value + "  ▾", button))
            { if (dropdown == name) dropdown = null; else { dropdown = name; dropdownRect = new Rect(rect.x, rect.yMax, rect.width, choices.Length * 27); dropdownItems = choices; dropdownChoose = choose; } }
        }
        void DrawDropdown()
        {
            Fill(dropdownRect, new Color(.12f, .20f, .22f, 1));
            for (int i = 0; i < dropdownItems.Length; i++)
            {
                GUI.enabled = !dropdownItems[i].Contains("미지원");
                if (GUI.Button(new Rect(dropdownRect.x, dropdownRect.y + i * 27, dropdownRect.width, 27), dropdownItems[i], button))
                { var choose = dropdownChoose; dropdown = null; choose(i); World.InputEvidence("UI dropdown=" + dropdownItems[i]); }
                GUI.enabled = true;
            }
            if (Event.current.type == EventType.MouseDown && !dropdownRect.Contains(Event.current.mousePosition)) dropdown = null;
        }
        void DrawPause()
        {
            Rect rect = new Rect(width / 2 - 210, 188, 420, 400); Fill(rect, ink);
            Label(new Rect(rect.x + 26, rect.y + 22, 370, 35), World.Player.Alive ? "잠시 쉬어가기" : "체크포인트에서 다시 시작", title, gold);
            if (GUI.Button(new Rect(rect.x + 27, rect.y + 76, 365, 36), "계속하기 · ESC", button)) {World.Paused=false;if(!World.Player.Alive)World.Session.Respawn();}
            if (GUI.Button(new Rect(rect.x + 27, rect.y + 122, 365, 36), World.Session.Active?"현재 방 다시 시작":"실험실 초기화", button)) {if(World.Session.Active)World.Session.RequestReset();else {World.ResetLab();World.Paused=false;}}
            if (World.Session.Active && GUI.Button(new Rect(rect.x+27,rect.y+168,365,36),"탐험 처음부터",button))World.Session.RequestReset(true);
            if (GUI.Button(new Rect(rect.x+27,rect.y+214,365,36),"모드 선택",button))World.Session.OpenMenu();
            if (GUI.Button(new Rect(rect.x + 27, rect.y + 260, 365, 36), "게임 종료", button)) World.Quit();
            Label(new Rect(rect.x+27,rect.y+315,365,66),"권능·발견·지름길은 이 실행 동안 유지됩니다.\n프로그램 종료 후에는 저장되지 않습니다.",small,muted);
        }
        void DrawStart()
        {
            float x=width/2-240;Fill(new Rect(x,188,480,370),ink);
            Label(new Rect(x+28,210,424,42),"환조 · 숲 유적의 네 가지 힘",title,gold);
            Label(new Rect(x+28,261,424,57),"같은 검, 네 방향. 물가에서 시작한 길이\n새 권능과 함께 다시 이어집니다.",text,muted);
            if(GUI.Button(new Rect(x+28,337,424,42),"속성 실험실 · 네 속성 모두 사용",button))World.Session.StartLab();
            if(GUI.Button(new Rect(x+28,391,424,42),World.Session.Visited.Count>0?"유적 탐험 · 마지막 체크포인트에서 계속":"유적 탐험 · 불의 권능으로 시작",button))World.Session.StartExploration();
            if(GUI.Button(new Rect(x+28,446,424,35),"게임 종료",button))World.Quit();
            Label(new Rect(x+28,495,424,42),"세션 내 진행 유지 · 종료 후 저장 없음",small,muted);
        }
        void DrawConfirmation()
        {
            float x=width/2-235;Fill(new Rect(x,246,470,235),ink);
            bool fresh=World.Session.Confirmation=="fresh";
            Label(new Rect(x+25,265,420,40),fresh?"탐험 진행을 처음부터 시작할까요?":"현재 방을 다시 시작할까요?",text,gold);
            Label(new Rect(x+25,310,420,66),fresh?"획득 권능·지도·발견·지름길을 초기화합니다.":"권능·발견·지름길은 보존하고\n이 방의 적·물체·검흔을 복구합니다.",text,muted);
            if(GUI.Button(new Rect(x+25,414,200,40),"취소",button))World.Session.Confirmation="";
            if(GUI.Button(new Rect(x+245,414,200,40),"확인 · 다시 시작",button))World.Session.Confirm();
        }
        void DrawMap()
        {
            float x=width/2-455;Fill(new Rect(x,185,910,400),ink);
            Label(new Rect(x+25,201,860,35),"방문 지도 · M 닫기",title,gold);
            Vector2[] points={new Vector2(35,80),new Vector2(235,80),new Vector2(435,80),new Vector2(635,80),new Vector2(635,185),new Vector2(235,270),new Vector2(435,185),new Vector2(435,270)};
            foreach(string link in World.Session.SeenExits)
            {
                var ends=link.Split('→');if(ends.Length!=2 || !World.Session.Visited.Contains(ends[0]) || !World.Session.Visited.Contains(ends[1]))continue;
                int a=Array.IndexOf(LabExploration.RoomIds,ends[0]),b=Array.IndexOf(LabExploration.RoomIds,ends[1]);if(a<0||b<0)continue;
                Vector2 p=points[a]+new Vector2(87,32),q=points[b]+new Vector2(87,32);
                Fill(new Rect(x+Mathf.Min(p.x,q.x),185+p.y,Mathf.Max(2,Mathf.Abs(p.x-q.x)),2),muted);
                Fill(new Rect(x+q.x,185+Mathf.Min(p.y,q.y),2,Mathf.Max(2,Mathf.Abs(p.y-q.y))),muted);
            }
            for(int i=0;i<LabExploration.RoomIds.Length;i++)
            {
                string id=LabExploration.RoomIds[i];if(!World.Session.Visited.Contains(id))continue;
                Vector2 p=points[i];Fill(new Rect(x+p.x,185+p.y,175,64),World.Session.Room==id?new Color(.24f,.38f,.33f):new Color(.1f,.2f,.22f));
                Label(new Rect(x+p.x+8,193+p.y,164,54),(World.Session.Room==id?"● ":"")+LabExploration.Name(id)+(World.Session.Checkpoint==id?"\n체크포인트":""),small,gold);
            }
            Label(new Rect(x+26,548,858,28),"방문한 장소만 표시 · 발견 "+World.Session.Discoveries.Count+"/2 · 성소 지름길 "+(World.Session.ShortcutOpen?"열림":"미개방"),small,muted);
        }
        void DrawHelp()
        {
            float x = width / 2 - 530;
            Fill(new Rect(x, 170, 1060, 405), ink);
            Label(new Rect(x + 24, 185, 960, 35), "검흔 반응표 · F4 닫기", title, gold);
            string[] rows = { "불 검흔", "물 검흔", "바람 검흔", "냉기 흔적", "변환된 얼음" };
            for (int col = 0; col < 4; col++) Label(new Rect(x + 196 + col * 207, 235, 207, 25), elements[col], text, LabSprites.ElementColor((Element)col));
            for (int row = 0; row < 5; row++)
            {
                Label(new Rect(x + 25, 277 + row * 43, 165, 35), rows[row], text, gold);
                for (int col = 0; col < 4; col++) Label(new Rect(x + 196 + col * 207, 277 + row * 43, 205, 35), TraceSupport.Description((Element)Mathf.Min(row, 3), row == 4, (Element)col), small, muted);
            }
            Label(new Rect(x + 25, 503, 990, 60), "차지와 기본 베기 모두 1R · 검흔 2초 · 새 검흔 1개가 이전 슬롯을 교체\n물 → 얼음은 지지면이 있을 때만 발판. 맵 수면 동결은 검흔과 별개입니다. 바람 검흔은 기능 검토 중입니다.", small, muted);
        }
        static void Fill(Rect rect, Color c) { var old = GUI.color; GUI.color = c; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        static Texture2D Flat(Color color) { var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, color); tex.Apply(); return tex; }
        static void Label(Rect rect, string value, GUIStyle style, Color color)
        { var old = GUI.contentColor; var textColor = style.normal.textColor; GUI.contentColor = Color.white; style.normal.textColor = color; GUI.Label(rect, value, style); style.normal.textColor = textColor; GUI.contentColor = old; }
    }
}
