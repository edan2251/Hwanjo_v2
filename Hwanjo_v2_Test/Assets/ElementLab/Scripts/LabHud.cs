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
        static readonly string[] presets = { "Custom", "Dry Vine", "Wet Vine", "Wooden Box", "Water", "Wind Device", "Freezable Device" };
        static readonly string[] materials = { "Plant", "Wood", "Metal", "Water", "Custom" };
        static readonly string[] elements = { "불", "물", "바람", "얼음" };
        static readonly string[] traitNames = { "Flammable", "Wettable", "Freezable", "Meltable", "Extinguishable", "Pushable", "WindReactive" };
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
            Label(new Rect(24, 14, 260, 32), "환조  ·  ELEMENT LAB", title, gold);
            Label(new Rect(26, 49, 264, 24), World.AutomatedEvidence ? "v0.1 / 자동 입력 검증 재생" : "v0.1   /   숲 유적의 속성 실험", small, muted);
            for (int i = 0; i < 4; i++)
            {
                Rect rect = new Rect(308 + i * 117, 19, 107, 44); Color c = LabSprites.ElementColor((Element)i);
                Fill(rect, World.Player.Element == (Element)i ? new Color(c.r * .32f, c.g * .32f, c.b * .32f, 1) : new Color(.1f, .16f, .18f, 1));
                Fill(new Rect(rect.x, rect.y, 3, rect.height), c);
                if (GUI.Button(rect, (i + 1) + "   " + elements[i], button) && !World.IsPaused) World.Select((Element)i);
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
            Label(new Rect(31, 644, width - 62, 22), "A/D 이동    SPACE 점프    SHIFT 대시    J 베기 / 차지    1–4 속성    R 초기화    ESC 일시정지", small, muted);
            if (GUI.Button(new Rect(31, 672, 135, 23), "F2  더미 편집", button)) World.SetPanel(!World.PanelOpen);
            if (GUI.Button(new Rect(178, 672, 130, 23), "F3  타격감 " + (World.GameFeel ? "ON" : "OFF"), button)) { World.GameFeel = !World.GameFeel; World.InputEvidence("UI feedback=" + World.GameFeel); }
            string[] zones = { "01 조작", "02 환경", "03 검흔", "04 적", "05 응용" }; float[] locations = { 0, 10, 29, 43, 54 };
            for (int i = 0; i < zones.Length; i++)
                if (GUI.Button(new Rect(width - 610 + i * 116, 672, 106, 23), zones[i], button) && !World.IsPaused)
                { World.Player.Teleport(new Vector2(locations[i], .02f)); World.InputEvidence("UI zone=" + zones[i]); }
            if (World.DebugView) DrawDiagnostics();
            if (World.PanelOpen) DrawPanel();
            else if (World.Paused || !World.Player.Alive) DrawPause();
            if (dropdown != null && World.PanelOpen) DrawDropdown();
            GUI.matrix = Matrix4x4.identity;
        }
        void DrawWorldLabels()
        {
            foreach (var target in World.Targets)
            {
                var point = World.Camera.WorldToScreenPoint(target.transform.position + Vector3.up * (target.Size.y + .32f));
                float x = point.x / scale, y = (Screen.height - point.y) / scale;
                if (x < -100 || x > width + 100 || y < 98 || y > 600) continue;
                Fill(new Rect(x - 68, y - 20, 136, 42), new Color(.04f, .12f, .13f, .83f));
                Label(new Rect(x - 64, y - 18, 130, 21), target.Label, small, gold);
                string state = !target.Model.Alive ? "소실 · R로 복원" : target.Model.State.ToString();
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
            Fill(new Rect(16, 98, 550, 65), new Color(.035f, .09f, .105f, .90f));
            Label(new Rect(25, 102, 530, 62), hint, text, new Color(.83f, .9f, .86f));
        }
        void DrawDiagnostics()
        {
            Fill(new Rect(22, 266, 740, 352), ink);
            Label(new Rect(36, 276, 710, 24), "F1  반응 기록 · 최근 8개   /   R=" + World.Tuning.Range + "  charge=" + World.Tuning.ChargeRange, small, gold);
            for (int i = 0; i < World.History.Count; i++) Label(new Rect(36, 306 + i * 38, 710, 38), World.History[i], small, muted);
            var a = World.Camera.WorldToScreenPoint(World.LastAttackOrigin); var b = World.Camera.WorldToScreenPoint(World.LastAttackEnd);
            if (World.ActionCount > 0) Fill(new Rect(Mathf.Min(a.x, b.x) / scale, (Screen.height - a.y) / scale, Mathf.Abs(a.x - b.x) / scale, 2), gold);
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
            if (DraftDry && DraftWet) ValidationMessage = "Dry + Wet 동시 선택은 불가합니다.";
            else if (DraftProfile.WindRoute == WindRoute.TraceTransport) ValidationMessage = "더미는 TraceTransport 결과를 지원하지 않습니다.";
            else
            {
                var state = new TargetState(DraftWet ? Moisture.Wet : DraftDry ? Moisture.Dry : Moisture.None, DraftBurning, DraftFrozen, DraftLiquid);
                ValidationMessage = DraftProfile.Validate(state);
                if (ValidationMessage.Length == 0)
                {
                    World.History.Clear(); World.ClearTransientEffects(); World.Dummy.Apply(DraftProfile, state);
                    ValidationMessage = DraftProfile.Has(Trait.Flammable) && !DraftProfile.Has(Trait.Extinguishable) ? "적용됨 · 경고: 가연성이지만 소화할 수 없습니다." : "적용됨 · 타이머 / 피해 / 이동 / VFX 초기화";
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
            Label(new Rect(x + 19, y + 47, 430, 22), "월드 정지 중 · Apply는 반응이 아닌 편집 동작입니다", small, muted);
            Drop("Preset", new Rect(x + 19, y + 76, 210, 29), presets[PresetIndex], presets, LoadPreset);
            Drop("Material", new Rect(x + 239, y + 76, 210, 29), materials[MaterialIndex], materials, i => { MaterialIndex = i; LoadPreset(i == 3 ? 4 : i == 1 ? 3 : i == 2 ? 6 : 1); MaterialIndex = i; });
            Label(new Rect(x + 19, y + 113, 430, 22), "Traits  ·  다중 선택", small, gold);
            for (int i = 0; i < 7; i++)
            {
                Trait trait = (Trait)(1 << i); bool old = DraftProfile.Has(trait);
                bool enabled = GUI.Toggle(new Rect(x + 20 + (i % 3) * 146, y + 141 + i / 3 * 25, 148, 25), old, traitNames[i], toggle);
                if (enabled != old) { DraftProfile.Traits = enabled ? DraftProfile.Traits | trait : DraftProfile.Traits & ~trait; PresetIndex = 0; }
            }
            Label(new Rect(x + 19, y + 218, 430, 20), "초기 상태  ·  모순 조합은 Apply에서 거부", small, gold);
            DraftDry = GUI.Toggle(new Rect(x + 20, y + 246, 83, 25), DraftDry, "Dry", toggle);
            DraftWet = GUI.Toggle(new Rect(x + 105, y + 246, 83, 25), DraftWet, "Wet", toggle);
            DraftBurning = GUI.Toggle(new Rect(x + 190, y + 246, 100, 25), DraftBurning, "Burning", toggle);
            DraftFrozen = GUI.Toggle(new Rect(x + 296, y + 246, 88, 25), DraftFrozen, "Frozen", toggle);
            DraftLiquid = GUI.Toggle(new Rect(x + 382, y + 246, 76, 25), DraftLiquid, "Liquid", toggle);
            Drop("Freeze", new Rect(x + 19, y + 278, 210, 29), DraftProfile.FreezeCondition.ToString(), new[] { "WetOrLiquid", "IntrinsicMoisture" }, i => DraftProfile.FreezeCondition = (FreezeCondition)i);
            Drop("Wind", new Rect(x + 239, y + 278, 210, 29), DraftProfile.WindRoute.ToString(), new[] { "BodyPush", "Device", "TraceTransport (미지원)" }, i => { if (i < 2) DraftProfile.WindRoute = (WindRoute)i; });
            DraftProfile.Fixed = GUI.Toggle(new Rect(x + 20, y + 318, 430, 24), DraftProfile.Fixed, "Fixed · 고정 몸체 (BodyPush 이동 차단)", toggle);
            Label(new Rect(x + 20, y + 347, 432, 42), "현재: " + World.Dummy.Model.State + "    HP " + World.Dummy.Model.Health.ToString("0") + "\nLast Effect: " + World.Dummy.LastEffect + "  |  " + World.Dummy.Model.Last.Rule, small, muted);
            if (GUI.Button(new Rect(x + 19, y + 400, 137, 34), "Apply 적용", button)) ApplyDraft();
            if (GUI.Button(new Rect(x + 165, y + 400, 138, 34), "Reset Target", button)) { World.History.Clear(); World.ClearTransientEffects(); World.Dummy.ResetTarget(); ReadDummy(); World.InputEvidence("UI Reset Target"); }
            if (GUI.Button(new Rect(x + 312, y + 400, 137, 34), "Default Preset", button)) { World.History.Clear(); World.ClearTransientEffects(); World.Dummy.DefaultPreset(); ReadDummy(); World.InputEvidence("UI Default Preset"); }
            Label(new Rect(x + 20, y + 448, 432, 60), ValidationMessage.Length > 0 ? ValidationMessage : "Reset Target = 마지막 Apply\nDefault Preset = 최초 설정 · F1에서 최근 8개 반응 확인", small, ValidationMessage.StartsWith("적용") ? muted : gold);
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
            Rect rect = new Rect(width / 2 - 180, 242, 360, 190); Fill(rect, ink);
            Label(new Rect(rect.x + 26, rect.y + 22, 310, 35), World.Player.Alive ? "잠시 쉬어가기" : "다시 시도할 수 있습니다", title, gold);
            if (GUI.Button(new Rect(rect.x + 27, rect.y + 79, 305, 36), World.Player.Alive ? "계속하기 · ESC" : "실험실 초기화 · R", button))
            { if (!World.Player.Alive) World.ResetLab(); World.Paused = false; }
            if (GUI.Button(new Rect(rect.x + 27, rect.y + 127, 305, 36), "게임 종료", button)) World.Quit();
        }
        static void Fill(Rect rect, Color c) { var old = GUI.color; GUI.color = c; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        static Texture2D Flat(Color color) { var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, color); tex.Apply(); return tex; }
        static void Label(Rect rect, string value, GUIStyle style, Color color)
        { var old = GUI.contentColor; var textColor = style.normal.textColor; GUI.contentColor = Color.white; style.normal.textColor = color; GUI.Label(rect, value, style); style.normal.textColor = textColor; GUI.contentColor = old; }
    }
}
