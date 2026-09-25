using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hwanjo.ElementLab
{
    public sealed class LabWorld : MonoBehaviour
    {
        public LabTuning Tuning;
        public LabArtLibrary Art;
        public LabPlayer Player { get; private set; }
        public LabTarget Dummy { get; private set; }
        public readonly List<LabTarget> Targets = new List<LabTarget>();
        public readonly List<string> History = new List<string>();
        public TraceSlot Trace { get; private set; }
        public Camera Camera { get; private set; }
        public LabHud Hud { get; private set; }
        public float Clock { get; private set; }
        public float Delta { get; private set; }
        public bool DebugView, GameFeel = true, Paused, PanelOpen;
        public bool InputEnabled = true;
        public bool IsPaused => Paused || PanelOpen || !focused;
        public int ActionCount { get; private set; }
        public Vector2 LastAttackOrigin, LastAttackEnd;
        public float LastAttackReach;
        public int LastAttackTargetCount;
        public GameObject Bridge;
        public bool BridgeOpen => Bridge && Bridge.activeSelf;
        public string CaptureDirectory;
        readonly List<LabFx> effects = new List<LabFx>();
        float stopRemaining, shakeRemaining;
        long lastFeedbackAction = -1;
        bool focused = true;
        bool quitPending, quitAllowed;
        public bool AutomatedEvidence { get; private set; }
        Transform background;
        void Awake()
        {
            if (!Tuning) Tuning = ScriptableObject.CreateInstance<LabTuning>();
            Application.targetFrameRate = 60;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "-labCaptureDir") CaptureDirectory = args[i + 1];
            BuildRoom();
            Hud = gameObject.AddComponent<LabHud>(); Hud.World = this;
            Debug.Log("ELEMENT_LAB_READY | single sword | 0.5 sec | 1.75 R | one trace slot");
            Application.wantsToQuit += WantsToQuit;
            AutomatedEvidence = Array.IndexOf(Environment.GetCommandLineArgs(), "-labReplayEvidence") >= 0;
            if (AutomatedEvidence) gameObject.AddComponent<LabEvidenceReplay>();
        }
        void Update()
        {
            LabKeyboardInterop.BeginFrame();
            var kb = Keyboard.current;
            if (InputEnabled && kb != null && focused)
            {
                if (kb.f1Key.wasPressedThisFrame) { DebugView = !DebugView; InputEvidence("F1 diagnostics=" + DebugView); }
                if (kb.f2Key.wasPressedThisFrame) SetPanel(!PanelOpen);
                if (kb.f3Key.wasPressedThisFrame) { GameFeel = !GameFeel; InputEvidence("F3 feedback=" + GameFeel); }
                if (kb.escapeKey.wasPressedThisFrame) { if (PanelOpen) SetPanel(false); else { Paused = !Paused; Player.CancelCharge(); InputEvidence("Pause=" + Paused); } }
                if (kb.f9Key.wasPressedThisFrame) StartCoroutine(Capture());
                if (!IsPaused)
                {
                    if (kb.rKey.wasPressedThisFrame) ResetLab();
                    if (kb.digit1Key.wasPressedThisFrame) Select(Element.Fire);
                    if (kb.digit2Key.wasPressedThisFrame) Select(Element.Water);
                    if (kb.digit3Key.wasPressedThisFrame) Select(Element.Wind);
                    if (kb.digit4Key.wasPressedThisFrame) Select(Element.Ice);
                }
            }
            stopRemaining = Mathf.Max(0, stopRemaining - Time.unscaledDeltaTime);
            Delta = IsPaused || GameFeel && stopRemaining > 0 ? 0 : Time.deltaTime;
            Clock += Delta;
            Player.Tick(Delta, InputEnabled && !IsPaused);
            foreach (var target in Targets) target.Tick(Delta);
            Trace?.Tick(Delta);
            for (int i = effects.Count - 1; i >= 0; i--) if (!effects[i].Tick(Delta)) effects.RemoveAt(i);
            shakeRemaining = Mathf.Max(0, shakeRemaining - Time.unscaledDeltaTime);
            Vector3 cam = new Vector3(Mathf.Clamp(Player.transform.position.x + 3.3f, 5, 75), 3.4f, -10);
            if (shakeRemaining > 0 && GameFeel && !IsPaused) cam += new Vector3(Mathf.Sin(Time.unscaledTime * 93), Mathf.Cos(Time.unscaledTime * 79), 0) * Tuning.CameraShake;
            Camera.transform.position = cam;
            if (background) background.position = new Vector3(cam.x, 4.5f, 2);
        }
        void OnApplicationFocus(bool value) { SetInputFocus(value); }
        public void SetInputFocus(bool value) { focused = value; if (Player) Player.CancelCharge(); Debug.Log("INPUT_FOCUS " + value + " keyboard=" + (Keyboard.current != null) + " enabled=" + (Keyboard.current != null && Keyboard.current.enabled)); }
        void OnApplicationPause(bool value) { if (value && Player) Player.CancelCharge(); }
        void OnDestroy() { Application.wantsToQuit -= WantsToQuit; Trace?.End(); }
        public void Select(Element element) { Player.Element = element; InputEvidence("Element=" + element); }
        public void SetPanel(bool open) { PanelOpen = open; Player.CancelCharge(); InputEvidence("F2 panel=" + open); if (Hud && open) Hud.ReadDummy(); }
        public void BuildRoom()
        {
            var cameraGo = new GameObject("Element Lab Camera"); cameraGo.transform.SetParent(transform); Camera = cameraGo.AddComponent<Camera>(); cameraGo.tag = "MainCamera";
            Camera.orthographic = true; Camera.orthographicSize = 5.4f; Camera.clearFlags = CameraClearFlags.SolidColor; Camera.backgroundColor = LabSprites.Hex("11292e");
            cameraGo.AddComponent<AudioListener>(); Camera.transform.position = new Vector3(5, 3.4f, -10);
            if (Art && Art.Background)
            {
                var go = new GameObject("Generated forest backdrop"); go.transform.SetParent(transform); background = go.transform;
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Art.Background; sr.sortingOrder = -50; sr.color = new Color(.72f, .80f, .80f);
                float height = 12; background.localScale = Vector3.one * height / sr.sprite.bounds.size.y;
            }
            else
            {
                for (int i = 0; i < 36; i++)
                {
                    float x = -10 + i * 2.8f;
                    LabSprites.Quad("Distant forest pillar", transform, new Vector2(x, 4), new Vector2(.45f, 10), LabSprites.Hex(i % 2 == 0 ? "24474a" : "1a383f"), -35);
                    LabSprites.Quad("Forest canopy", transform, new Vector2(x + .4f, 7.6f), new Vector2(4, 2), LabSprites.Hex("203e3c"), -30);
                }
            }
            Ground(-9, 24, 0); Ground(24, 28, -.65f); Ground(28, 57, 0); Ground(57, 64, -2.2f); Ground(64, 67, 0); Ground(67, 71, -.65f); Ground(71, 84, 0);
            Ground(60, 61, -.9f); Ground(62, 64, -.35f);
            Ground(-10, -9, 4); Ground(84, 85, 4);
            for (int i = 0; i < (Art && Art.Background ? 0 : 28); i++)
            {
                float x = -6 + i * 3.2f;
                LabSprites.Quad("Mossy ruin", transform, new Vector2(x, 1.9f), new Vector2(.26f, 3.8f), LabSprites.Hex("335653"), -20);
                LabSprites.Quad("Stone cap", transform, new Vector2(x, 3.8f), new Vector2(.9f, .18f), LabSprites.Hex("52756a"), -19);
                if (i % 3 == 0) LabSprites.Quad("Lantern", transform, new Vector2(x + .45f, 1.8f), new Vector2(.12f, .24f), LabSprites.Hex("c7ad70"), -15);
            }
            var playerGo = new GameObject("Same human protagonist"); playerGo.transform.SetParent(transform); Player = playerGo.AddComponent<LabPlayer>(); Player.Initialize(this); Player.ResetPlayer(Vector2.zero);
            Dummy = SpawnTarget(TargetKind.Dummy, "01  편집 더미", Preset.DryVine, new Vector2(4.7f, 0), new Vector2(.95f, 1.15f));
            SpawnTarget(TargetKind.Vine, "덩굴", Preset.DryVine, new Vector2(12, 0), new Vector2(.9f, 1.4f), true);
            SpawnTarget(TargetKind.Rope, "밧줄", Preset.DryVine, new Vector2(17, .1f), new Vector2(.45f, 1.4f), true);
            SpawnTarget(TargetKind.Box, "나무상자", Preset.WoodenBox, new Vector2(21, 0), new Vector2(.95f, .95f), true);
            SpawnTarget(TargetKind.Water, "물웅덩이", Preset.Water, new Vector2(26, -.4f), new Vector2(4, .4f));
            SpawnTarget(TargetKind.WindDevice, "풍차", Preset.WindDevice, new Vector2(33, 0), new Vector2(1.2f, 1.4f));
            SpawnTarget(TargetKind.FreezeDevice, "결빙 장치", Preset.FreezableDevice, new Vector2(38, 0), new Vector2(1.1f, 1.3f));
            var enemyProfile = TargetProfile.For(Preset.WoodenBox); enemyProfile.Traits |= Trait.Freezable | Trait.Meltable; enemyProfile.FreezeCondition = FreezeCondition.IntrinsicMoisture; enemyProfile.TimedStatuses = true;
            SpawnTarget(TargetKind.Enemy, "숲의 파수꾼", enemyProfile, new TargetState(Moisture.Dry), new Vector2(45, 0), new Vector2(.85f, .9f));
            var rope = SpawnTarget(TargetKind.Rope, "불을 보내 다리 열기", Preset.DryVine, new Vector2(62, 0), new Vector2(.4f, 1.3f), true); rope.PuzzleRope = true;
            Bridge = Ground(57, 64, .02f); Bridge.name = "Rope released bridge"; Bridge.SetActive(false);
            SpawnTarget(TargetKind.Water, "수면을 얼려 건너기", Preset.Water, new Vector2(69, -.4f), new Vector2(4, .4f));
            Physics2D.SyncTransforms();
        }
        GameObject Ground(float left, float right, float top)
        {
            var go = new GameObject("Permanent moss stone"); go.transform.SetParent(transform); go.transform.position = new Vector3((left + right) / 2, top - .8f);
            var box = go.AddComponent<BoxCollider2D>(); box.size = new Vector2(right - left, 1.6f); go.AddComponent<LabSurface>();
            LabSprites.Quad("Stone mass", go.transform, Vector2.zero, box.size, LabSprites.Hex("233a3d"), 5);
            LabSprites.Quad("Moss rim", go.transform, new Vector2(0, .78f), new Vector2(right - left, .16f), LabSprites.Hex("6e9266"), 7);
            for (float x = -(right - left) / 2 + .2f; x < (right - left) / 2; x += .6f)
                LabSprites.Quad("Stone grain", go.transform, new Vector2(x, .25f + Mathf.Sin(x * 7) * .22f), new Vector2(.45f, .09f), LabSprites.Hex("3e5753"), 6);
            return go;
        }
        public LabTarget SpawnTarget(TargetKind kind, string label, Preset preset, Vector2 at, Vector2 size, bool consume = false)
        { var p = TargetProfile.For(preset); p.ConsumeOnBurnout = consume; return SpawnTarget(kind, label, p, TargetProfile.Initial(preset), at, size); }
        public LabTarget SpawnTarget(TargetKind kind, string label, TargetProfile profile, TargetState state, Vector2 at, Vector2 size)
        {
            var go = new GameObject(label); go.transform.SetParent(transform); go.transform.position = at;
            var target = go.AddComponent<LabTarget>(); target.Initialize(this, kind, label, profile, state, size); Targets.Add(target); return target;
        }
        public float ClipReach(Vector2 origin, float direction, float reach)
        {
            foreach (var hit in Physics2D.BoxCastAll(origin, new Vector2(.01f, .85f), 0, Vector2.right * direction, reach))
                if (hit.collider && !hit.collider.isTrigger && hit.collider.GetComponent<LabSurface>() && !hit.collider.GetComponentInParent<LabTarget>()) reach = Mathf.Min(reach, hit.distance);
            return Mathf.Max(0, reach);
        }
        public void ExecuteAttack(LabPlayer player, AttackKind kind, Element element, ActionContext action)
        {
            Physics2D.SyncTransforms(); ActionCount++;
            var origin = player.AttackOrigin; float direction = player.Facing; float range = kind == AttackKind.Charged ? Tuning.ChargeRange : Tuning.Range;
            float reach = ClipReach(origin, direction, range);
            LastAttackOrigin = origin; LastAttackEnd = origin + Vector2.right * direction * reach; LastAttackReach = reach; LastAttackTargetCount = 0;
            var targets = new HashSet<LabTarget>();
            foreach (var collider in Physics2D.OverlapBoxAll(origin + Vector2.right * direction * reach / 2, new Vector2(reach, 1.05f), 0))
            {
                var target = collider.GetComponentInParent<LabTarget>(); if (!target || !targets.Add(target)) continue;
                target.Receive(action, ReactionRules.Of(element), Tuning.SwordDamage, direction); LastAttackTargetCount++;
            }
            Slash(origin, direction, reach, element, kind == AttackKind.Charged);
            if (kind == AttackKind.Charged) CreateTrace(element, origin, direction, action.Id);
            Debug.Log("ATTACK " + action.Id + " " + kind + " " + element + " origin=" + origin.ToString("F3") + " end=" + LastAttackEnd.ToString("F3") + " reach=" + reach.ToString("F3"));
        }
        public void CreateTrace(Element element, Vector2 origin, float direction, long actionId)
        {
            Trace?.End();
            float center = Mathf.Min(Tuning.TraceCenterR * Tuning.Range, ClipReach(origin, direction, Tuning.TraceCenterR * Tuning.Range + Tuning.TraceWidthR * Tuning.Range / 2) - Tuning.TraceWidthR * Tuning.Range / 2);
            if (center <= .12f) { Trace = null; RecordTimer(null, "검흔 생성 불가 · 벽 여유 없음"); return; }
            Trace = new TraceSlot(this, element, origin + Vector2.right * direction * center, actionId);
        }
        public void ResetLab()
        {
            Trace?.End(); Trace = null; Clock = 0; ActionCount = 0; stopRemaining = shakeRemaining = 0; lastFeedbackAction = -1;
            foreach (var fx in effects) fx.Destroy(); effects.Clear();
            foreach (var target in Targets) target.DefaultPreset();
            Player.ResetPlayer(Vector2.zero); Bridge.SetActive(false); History.Clear(); Hud?.ReadDummy();
            Physics2D.SyncTransforms(); InputEvidence("R full reset");
        }
        public void ClearTransientEffects()
        {
            foreach (var fx in effects) fx.Destroy(); effects.Clear();
            stopRemaining = shakeRemaining = 0;
        }
        public void OpenBridge() { Bridge.SetActive(true); RecordTimer(null, "응용 1 해결 · 밧줄 해제 / 다리 개방"); }
        public void Record(LabTarget target, ActionContext action, Effect effect, Reaction reaction)
        { RecordTimer(target, "#" + action.Id + " " + action.Source + " " + effect + " | " + reaction); }
        public void RecordTimer(LabTarget target, string text)
        {
            string line = (target ? target.Label + " [" + target.Model.Id + "] " : "") + text;
            History.Insert(0, line); if (History.Count > 8) History.RemoveAt(History.Count - 1);
            Debug.Log("REACTION " + line);
        }
        public void InputEvidence(string value) { Debug.Log("INPUT " + value); }
        public void Impact(ActionContext action, Vector2 at, bool reacted, Effect effect)
        {
            if (GameFeel && lastFeedbackAction != action.Id) { stopRemaining = Tuning.HitStop; shakeRemaining = .12f; lastFeedbackAction = action.Id; }
            Element element = effect == Effect.Heat ? Element.Fire : effect == Effect.Moisture ? Element.Water : effect == Effect.Cold ? Element.Ice : Element.Wind;
            for (int i = 0; i < (reacted ? 5 : 2); i++) Particle(at, element, .25f);
        }
        public void Particle(Vector2 at, Element element, float life)
        {
            var color = LabSprites.ElementColor(element);
            Vector2 velocity = element == Element.Water ? new Vector2(UnityEngine.Random.Range(-.4f, .4f), -1.6f) : element == Element.Wind ? new Vector2(Player.Facing * 2.3f, .2f) : new Vector2(UnityEngine.Random.Range(-.8f, .8f), 1.4f);
            var size = element == Element.Wind ? new Vector2(.24f, .025f) : element == Element.Water ? new Vector2(.035f, .10f) : new Vector2(.05f, .05f);
            var sr = LabSprites.Quad("Element particle", transform, at, size, color, 32);
            if (element == Element.Ice) sr.transform.localRotation = Quaternion.Euler(0, 0, 45);
            effects.Add(new LabFx(sr, velocity, life));
        }
        void Slash(Vector2 origin, float direction, float reach, Element element, bool charged)
        {
            var color = LabSprites.ElementColor(element);
            int segments = charged ? 18 : 12;
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)(segments - 1); float x = Mathf.Sin(t * Mathf.PI) * reach; float y = Mathf.Lerp(.62f, -.5f, t);
                Vector2 at = origin + new Vector2(direction * x, y);
                float width = element == Element.Wind ? .18f : element == Element.Water ? .13f : .16f;
                var sr = LabSprites.Quad("Single slash arc", transform, at, new Vector2(width, element == Element.Wind ? .025f : .055f), Color.Lerp(Color.white, color, t), 31);
                sr.transform.localRotation = Quaternion.Euler(0, 0, element == Element.Ice ? 45 : -t * 100 * direction);
                effects.Add(new LabFx(sr, Vector2.zero, charged ? .24f : .17f));
            }
        }
        public IEnumerator Capture(string label = "game")
        {
            if (string.IsNullOrEmpty(CaptureDirectory)) CaptureDirectory = Path.Combine(Application.persistentDataPath, "Captures");
            Directory.CreateDirectory(CaptureDirectory);
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(CaptureDirectory, label + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".png");
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(path, image.EncodeToPNG()); Destroy(image); Debug.Log("CAPTURE " + path);
        }
        bool WantsToQuit() { if (quitAllowed) return true; Quit(); return false; }
        public void Quit() { if (!quitPending) StartCoroutine(FinishQuit()); }
        IEnumerator FinishQuit()
        {
            quitPending = true; Paused = true; Player.CancelCharge(); Debug.Log("ELEMENT_LAB_USER_QUIT");
            // Route OS close requests through the same frame boundary as the in-game menu.
            yield return null; yield return null; quitAllowed = true; Application.Quit(0);
        }
    }

    public sealed class LabFx
    {
        readonly SpriteRenderer renderer; readonly Vector2 velocity; readonly float lifetime; float age;
        public LabFx(SpriteRenderer renderer, Vector2 velocity, float lifetime) { this.renderer = renderer; this.velocity = velocity; this.lifetime = lifetime; }
        public bool Tick(float dt)
        {
            if (!renderer) return false; age += dt; if (age >= lifetime) { Destroy(); return false; }
            renderer.transform.position += (Vector3)(velocity * dt); Color c = renderer.color; c.a = 1 - age / lifetime; renderer.color = c; return true;
        }
        public void Destroy() { if (renderer) UnityEngine.Object.Destroy(renderer.gameObject); }
    }
}
