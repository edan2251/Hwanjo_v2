using UnityEngine;

namespace Hwanjo.ElementLab
{
    public enum TargetKind { Dummy, Vine, Rope, Box, Water, WindDevice, FreezeDevice, Enemy, Trace }
    public sealed class LabTarget : MonoBehaviour
    {
        public LabWorld World { get; private set; }
        public TargetModel Model { get; private set; }
        public TargetKind Kind;
        public string Label;
        public Vector2 Size;
        public Vector3 Spawn;
        public SpriteRenderer Body;
        public BoxCollider2D HitCollider, SolidCollider;
        public LabMotor Motor;
        public TraceSlot Trace;
        public string LastEffect = "—";
        public bool PuzzleRope;
        public float PushVelocity, Spin, Flash;
        public int EnemyPhase { get; private set; }
        public float EnemyPhaseTime { get; private set; }
        public int EnemyHits { get; private set; }
        public float EnemyAnimationPhase => EnemyPhaseTime;
        TargetProfile initialProfile;
        TargetState initialState;
        SpriteRenderer stateIcon;
        public void Initialize(LabWorld world, TargetKind kind, string label, TargetProfile profile, TargetState state, Vector2 size)
        {
            World = world; Kind = kind; Label = label; Size = size; Spawn = transform.position; initialProfile = profile; initialState = state;
            Model = new TargetModel(GetInstanceID(), profile, state, world.Tuning.Status, kind == TargetKind.Enemy ? world.Tuning.EnemyHealth : 100, kind == TargetKind.Enemy || kind == TargetKind.Dummy || kind == TargetKind.Box);
            var graphic = new GameObject("Body"); graphic.transform.SetParent(transform, false); Body = graphic.AddComponent<SpriteRenderer>();
            string sprite = kind == TargetKind.Vine ? "vine" : kind == TargetKind.Rope ? "rope" : kind == TargetKind.Box ? "box" : kind == TargetKind.Water ? "water" : kind == TargetKind.WindDevice ? "wind" : kind == TargetKind.FreezeDevice ? "device" : kind == TargetKind.Enemy ? "enemy" : "dummy";
            Body.sprite = LabSprites.Prop(sprite); Body.sortingOrder = 15; Body.transform.localScale = new Vector3(size.x, size.y, 1);
            HitCollider = gameObject.AddComponent<BoxCollider2D>(); HitCollider.size = size; HitCollider.offset = new Vector2(0, size.y / 2); HitCollider.isTrigger = true;
            var solid = new GameObject("Body collision"); solid.transform.SetParent(transform, false); SolidCollider = solid.AddComponent<BoxCollider2D>(); SolidCollider.size = size; SolidCollider.offset = HitCollider.offset;
            var surface = solid.AddComponent<LabSurface>(); surface.PermanentSupport = kind == TargetKind.Water;
            Motor = new LabMotor(transform, size);
            stateIcon = LabSprites.Quad("State overlay", transform, new Vector2(0, size.y / 2), size + Vector2.one * .1f, Color.clear, 18);
            RefreshSolid();
        }
        public Reaction Receive(ActionContext action, Effect effect, float damage, float direction)
        {
            if (Trace != null && !Trace.CanReceive(action, effect, out string blocked))
            { var no = Reaction.None(Model.State, blocked); World.Record(this, action, effect, no); return no; }
            var r = Model.Receive(action, effect, damage); LastEffect = effect.ToString();
            if (r.Reason == "RehitBlocked") return r;
            World.Record(this, action, effect, r);
            if (damage > 0 && r.Reason != "WrongState:Inactive") { Flash = World.GameFeel ? World.Tuning.HitFlash : 0; World.Impact(action, transform.position + Vector3.up * .5f, r.Applied, effect); }
            if (r.Consequence == Consequence.Push) { PushVelocity = direction * World.Tuning.PushSpeed; if (Kind == TargetKind.Enemy) Motor.Velocity = new Vector2(0, World.Tuning.PushLift); }
            if (r.Consequence == Consequence.Rotate) Spin += 100;
            if (r.Consequence == Consequence.Freeze && Kind == TargetKind.Enemy) { EnemyPhase = 0; EnemyPhaseTime = 0; }
            Trace?.React(r, action, direction);
            RefreshSolid(); return r;
        }
        public void Tick(float dt)
        {
            if (Trace != null) return;
            bool wasAlive = Model.Alive; bool wasBurning = Model.State.Burning;
            Model.Tick(dt); Flash = Mathf.Max(0, Flash - dt);
            if (wasBurning && !Model.State.Burning && Model.Consumed) World.RecordTimer(this, "EL10 · fuel exhausted");
            if (PuzzleRope && wasAlive && !Model.Alive) World.OpenBridge();
            if (!Model.Alive) { Body.color = new Color(.35f, .45f, .4f, .2f); HitCollider.enabled = SolidCollider.enabled = false; stateIcon.color = Color.clear; return; }
            float self = 0;
            if (Kind == TargetKind.Enemy) self = TickEnemy(dt);
            if (!Model.Profile.Fixed && Kind != TargetKind.Water)
            {
                Motor.Move(dt, self, World.Tuning.Gravity, PushVelocity);
                PushVelocity = Mathf.MoveTowards(PushVelocity, 0, 12 * dt);
            }
            if (Kind == TargetKind.FreezeDevice && !Model.State.Frozen) Spin += dt * 75;
            if (Kind == TargetKind.WindDevice || Kind == TargetKind.FreezeDevice || Kind == TargetKind.Dummy && Model.Profile.WindRoute == WindRoute.Device)
                Body.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Spin * Mathf.Deg2Rad) * 20);
            DrawState(); RefreshSolid();
        }
        float TickEnemy(float dt)
        {
            if (Model.State.Frozen) { EnemyPhase = 0; EnemyPhaseTime = 0; return 0; }
            if (!World.Player.Alive) { EnemyPhase = 0; EnemyPhaseTime = 0; return 0; }
            float delta = World.Player.transform.position.x - transform.position.x;
            float slow = Model.State.Moisture == Moisture.Wet ? World.Tuning.Status.WetSpeed : 1;
            Body.flipX = delta < 0;
            if (EnemyPhase == 0)
            {
                if (Mathf.Abs(delta) < World.Tuning.EnemyRange && Mathf.Abs(World.Player.transform.position.y - transform.position.y) < 1)
                { EnemyPhase = 1; EnemyPhaseTime = 0; return 0; }
                return Mathf.Abs(delta) < 6 && Mathf.Abs(delta) > .8f ? Mathf.Sign(delta) * World.Tuning.EnemySpeed * slow : 0;
            }
            EnemyPhaseTime += dt * slow;
            if (EnemyPhase == 1 && EnemyPhaseTime >= World.Tuning.EnemyWindup)
            {
                EnemyPhase = 2; EnemyPhaseTime = 0;
                if (Mathf.Abs(delta) <= World.Tuning.EnemyRange && Mathf.Abs(World.Player.transform.position.y - transform.position.y) < 1.1f)
                { World.Player.Damage(World.Tuning.EnemyDamage, Mathf.Sign(delta)); EnemyHits++; }
            }
            else if (EnemyPhase == 2 && EnemyPhaseTime >= World.Tuning.EnemyActive) { EnemyPhase = 3; EnemyPhaseTime = 0; }
            else if (EnemyPhase == 3 && EnemyPhaseTime >= World.Tuning.EnemyRecovery) { EnemyPhase = 0; EnemyPhaseTime = 0; }
            // The same scaled phase drives the visible anticipation/contact/recovery and hit timing.
            Body.transform.localPosition = new Vector3(EnemyPhase == 2 ? Mathf.Sign(delta) * .15f : 0, EnemyPhase == 1 ? -.05f * Mathf.Clamp01(EnemyPhaseTime / World.Tuning.EnemyWindup) : 0, 0);
            return 0;
        }
        void DrawState()
        {
            Body.color = Flash > 0 ? Color.white : Model.State.Frozen ? new Color(.63f, .87f, 1) : Model.State.Moisture == Moisture.Wet ? new Color(.57f, .75f, .8f) : Color.white;
            stateIcon.color = Flash > 0 && World.GameFeel ? new Color(1, 1, 1, .65f) : Model.State.Frozen ? new Color(.6f, .9f, 1, .28f) : Color.clear;
            if (Model.State.Frozen && World.Clock % .24f < World.Delta) World.Particle(transform.position + new Vector3(Random.Range(-Size.x / 2, Size.x / 2), Size.y * .6f, 0), Element.Ice, .3f);
            if (Model.State.Burning && World.Clock % .18f < World.Delta) World.Particle(transform.position + new Vector3(Random.Range(-Size.x / 2, Size.x / 2), .4f, 0), Element.Fire, .45f);
            if (Model.State.Moisture == Moisture.Wet && !Model.State.Frozen && World.Clock % .35f < World.Delta) World.Particle(transform.position + new Vector3(Random.Range(-.3f, .3f), Size.y, 0), Element.Water, .45f);
        }
        public void RefreshSolid()
        {
            if (!SolidCollider) return;
            if (Trace != null) return;
            bool platform = Kind == TargetKind.Water && Model.State.Frozen || Kind == TargetKind.Box || Kind == TargetKind.Dummy && !Model.Profile.LiquidBody || Kind == TargetKind.Vine;
            SolidCollider.enabled = Trace == null && Model.Alive && platform;
            if (Kind == TargetKind.Water) { SolidCollider.size = new Vector2(Size.x, .15f); SolidCollider.offset = new Vector2(0, Size.y); }
        }
        public void Apply(TargetProfile profile, TargetState state)
        { Model.Apply(profile, state); ResetTransform(); World.RecordTimer(this, "Debug Apply · reset timers / motion / history"); }
        public void ResetTarget() { Model.Reset(); ResetTransform(); }
        public void DefaultPreset() { Model.Apply(initialProfile, initialState, Kind == TargetKind.Enemy ? World.Tuning.EnemyHealth : 100, Kind == TargetKind.Enemy || Kind == TargetKind.Dummy || Kind == TargetKind.Box); ResetTransform(); }
        void ResetTransform()
        {
            transform.position = Spawn; Motor.Clear(); PushVelocity = Spin = Flash = 0; EnemyPhase = 0; EnemyPhaseTime = 0; EnemyHits = 0;
            LastEffect = "—"; Body.color = Color.white; Body.transform.localRotation = Quaternion.identity; Body.transform.localPosition = Vector3.zero; stateIcon.color = Color.clear; HitCollider.enabled = true; RefreshSolid();
        }
    }
}
