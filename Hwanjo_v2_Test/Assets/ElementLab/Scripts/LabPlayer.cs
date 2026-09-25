using UnityEngine;
using UnityEngine.InputSystem;

namespace Hwanjo.ElementLab
{
    public sealed class LabPlayer : MonoBehaviour
    {
        public LabWorld World;
        public LabMotor Motor;
        public readonly AttackInput Input = new AttackInput();
        public Element Element { get; set; }
        public float Health { get; private set; }
        public bool Alive => Health > 0;
        public int Facing { get; private set; } = 1;
        public int AttackCount { get; private set; }
        public AttackKind CurrentAttack { get; private set; }
        public float AttackProgress { get; private set; }
        public string Pose { get; private set; } = "Idle";
        public Vector2 AttackOrigin => (Vector2)transform.position + Vector2.up * .62f;
        public bool Grounded => Motor.Grounded;
        public SpriteRenderer Body, Sword;
        float attackTime, dashTime, dashCooldown, hitTime, immunity, externalX, idleClock;
        bool activated;
        Element attackElement;
        ActionContext action;
        bool capturePoses;
        readonly System.Collections.Generic.HashSet<string> capturedPoses = new System.Collections.Generic.HashSet<string>();
        public void Initialize(LabWorld world)
        {
            World = world; Health = world.Tuning.PlayerHealth; Motor = new LabMotor(transform, new Vector2(.48f, .88f));
            capturePoses = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-labCaptureActions") >= 0;
            var go = new GameObject("Character body 48px"); go.transform.SetParent(transform, false); Body = go.AddComponent<SpriteRenderer>(); Body.sortingOrder = 25;
            Sword = LabSprites.Quad("Single sword", transform, new Vector2(.32f, .40f), new Vector2(.70f, .045f), new Color(.85f, .88f, .83f), 26);
            var hilt = LabSprites.Quad("Sword hilt", Sword.transform, new Vector2(-.36f, 0), new Vector2(.14f, 3), LabSprites.Hex("c2a466"), 27);
            var collider = gameObject.AddComponent<BoxCollider2D>(); collider.size = Motor.Size; collider.offset = Vector2.up * Motor.Size.y / 2; collider.isTrigger = true;
        }
        public void Tick(float dt, bool allowInput)
        {
            var kb = Keyboard.current;
            if (!allowInput) Input.Cancel();
            dashCooldown = Mathf.Max(0, dashCooldown - dt); hitTime = Mathf.Max(0, hitTime - dt); immunity = Mathf.Max(0, immunity - dt);
            float move = allowInput && kb != null ? (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1 : 0) - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1 : 0) : 0;
            if (!Alive) move = 0;
            if (move != 0 && CurrentAttack == AttackKind.None && dashTime <= 0) Facing = move > 0 ? 1 : -1;
            if (Alive && allowInput && kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame && Motor.Grounded)
                { CancelCharge(); Motor.Velocity = new Vector2(0, World.Tuning.JumpSpeed); World.InputEvidence("Jump"); }
                if ((kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame) && dashCooldown <= 0)
                { CancelCharge(); dashTime = World.Tuning.DashSeconds; dashCooldown = World.Tuning.DashCooldown; World.InputEvidence("Dash"); }
                if (kb.jKey.wasPressedThisFrame && CurrentAttack == AttackKind.None && dashTime <= 0 && hitTime <= 0)
                { Input.Press(Motor.Grounded); World.InputEvidence("J pressed"); }
                // A release uses elapsed held frames, then increments neither duration nor damage again.
                if (kb.jKey.wasReleasedThisFrame)
                {
                    AttackKind kind = Input.Release(Motor.Grounded);
                    if (kind != AttackKind.None && CurrentAttack == AttackKind.None && dashTime <= 0 && hitTime <= 0) BeginAttack(kind);
                    World.InputEvidence("J released");
                }
            }
            Input.Tick(dt);
            if (CurrentAttack != AttackKind.None)
            {
                attackTime += dt;
                float windup = CurrentAttack == AttackKind.Charged ? World.Tuning.ChargeWindup : World.Tuning.SingleWindup;
                float duration = CurrentAttack == AttackKind.Charged ? World.Tuning.ChargeDuration : World.Tuning.SingleDuration;
                AttackProgress = Mathf.Clamp01(attackTime / duration);
                if (!activated && attackTime >= windup) { activated = true; World.ExecuteAttack(this, CurrentAttack, attackElement, action); }
                if (attackTime >= duration) CurrentAttack = AttackKind.None;
            }
            float speed = move * World.Tuning.MoveSpeed * (Input.Held ? .5f : 1);
            if (CurrentAttack != AttackKind.None) speed *= .35f;
            if (dashTime > 0) { speed = Facing * World.Tuning.DashSpeed; dashTime = Mathf.Max(0, dashTime - dt); }
            Motor.Move(dt, speed, World.Tuning.Gravity, externalX); externalX = Mathf.MoveTowards(externalX, 0, 14 * dt);
            if (transform.position.y < -5) ResetPlayer(new Vector2(0, 0));
            idleClock += dt; Render(move);
        }
        public void BeginAttack(AttackKind kind)
        {
            if (!Alive || kind == AttackKind.None || CurrentAttack != AttackKind.None) return;
            CurrentAttack = kind; attackTime = AttackProgress = 0; activated = false; attackElement = Element; action = new ActionContext(); AttackCount++;
            World.InputEvidence(kind + " attack / " + attackElement + " / ActionId=" + action.Id);
        }
        void Render(float move)
        {
            float phase;
            if (!Alive) { Pose = "Death"; phase = Mathf.Clamp01(idleClock / .5f); }
            else if (hitTime > 0) { Pose = "Hit"; phase = 1 - hitTime / .18f; }
            else if (dashTime > 0) { Pose = "Dash"; phase = 1 - dashTime / World.Tuning.DashSeconds; }
            else if (CurrentAttack != AttackKind.None)
            {
                Pose = CurrentAttack == AttackKind.Charged ? "ChargeRelease" : "Slash";
                float windup = CurrentAttack == AttackKind.Charged ? World.Tuning.ChargeWindup : World.Tuning.SingleWindup;
                float duration = CurrentAttack == AttackKind.Charged ? World.Tuning.ChargeDuration : World.Tuning.SingleDuration;
                phase = attackTime < windup ? attackTime / windup / 3 : attackTime < windup + World.Tuning.ActiveSeconds
                    ? 1f/3 + (attackTime - windup) / World.Tuning.ActiveSeconds / 3
                    : 2f/3 + (attackTime - windup - World.Tuning.ActiveSeconds) / (duration - windup - World.Tuning.ActiveSeconds) / 3;
            }
            else if (Input.Held && Grounded) { Pose = "Charge"; phase = Mathf.Clamp01(Input.HeldSeconds / AttackInput.ChargeThreshold); }
            else if (!Grounded) { Pose = Motor.Velocity.y > .1f ? "Jump" : "Fall"; phase = .5f; }
            else if (Mathf.Abs(move) > .1f) { Pose = "Run"; phase = Mathf.Repeat(idleClock * 9 / 6, 1); }
            else { Pose = "Idle"; phase = Mathf.Repeat(idleClock, 1); }
            var frame = World.Art ? World.Art.Frame(Pose, phase) : null;
            Body.sprite = frame ? frame : LabSprites.HeroPlaceholder(Pose, phase); Body.flipX = Facing < 0;
            if (capturePoses && frame && capturedPoses.Add(Pose + "-" + frame.name)) World.StartCoroutine(World.Capture(Pose + "-" + frame.name));
            Body.color = immunity > 0 && (int)(Time.unscaledTime * 18) % 2 == 0 ? new Color(1, .65f, .65f, .7f) : Color.white;
            Sword.enabled = Alive && Pose != "Death";
            float angle = Input.Held ? 110 : CurrentAttack != AttackKind.None ? Mathf.Lerp(105, -32, Mathf.Clamp01(AttackProgress * 2)) : -24;
            Sword.transform.localPosition = new Vector3(Facing * .37f, Input.Held ? .58f : .42f, 0);
            Sword.transform.localRotation = Quaternion.Euler(0, 0, Facing > 0 ? angle : 180 - angle);
            Sword.color = Color.Lerp(new Color(.88f, .89f, .79f), LabSprites.ElementColor(CurrentAttack != AttackKind.None ? attackElement : Element), .4f);
        }
        public void CancelCharge() { Input.Cancel(); }
        public void Teleport(Vector2 at) { transform.position = at; Motor.Clear(); CancelCharge(); CurrentAttack = AttackKind.None; dashTime = externalX = 0; }
        public void Damage(float damage, float direction)
        {
            if (!Alive || immunity > 0 || dashTime > 0) return;
            Health = Mathf.Max(0, Health - damage); CancelCharge(); CurrentAttack = AttackKind.None;
            immunity = .6f; hitTime = .18f; externalX = direction * 3; idleClock = 0;
            World.Particle(AttackOrigin, Element.Fire, .25f); World.InputEvidence("Player hit / HP=" + Health);
        }
        public void ResetPlayer(Vector2 at)
        {
            transform.position = at; Health = World.Tuning.PlayerHealth; Element = Element.Fire; Facing = 1; AttackCount = 0;
            Input.Cancel(); Motor.Clear(); CurrentAttack = AttackKind.None;
            attackTime = dashTime = dashCooldown = hitTime = immunity = externalX = idleClock = 0;
        }
    }
}
