using UnityEngine;

namespace Hwanjo.ElementLab
{
    public sealed class TraceSlot
    {
        public LabTarget Target { get; private set; }
        public Element Element { get; private set; }
        public float ExpiresAt { get; private set; }
        public bool Flying { get; private set; }
        public bool Platform { get; private set; }
        public bool SolidIce { get; private set; }
        public string Note { get; private set; } = "다른 속성으로 재타격";
        public bool Active => Target && Target.gameObject.activeSelf;
        public Vector2 Position => Target ? (Vector2)Target.transform.position + Vector2.up * Target.Size.y / 2 : Vector2.zero;
        public float Remaining => Mathf.Max(0, ExpiresAt - world.Clock);
        public float Traveled { get; private set; }
        readonly LabWorld world;
        readonly long createdBy;
        ActionContext lineage;
        Vector2 direction;
        SpriteRenderer symbol;
        public TraceSlot(LabWorld world, Element element, Vector2 center, long createdBy, Vector2 direction)
        {
            this.world = world; this.createdBy = createdBy; Element = element; ExpiresAt = world.Clock + world.Tuning.TraceSeconds;
            this.direction = direction;
            Vector2 size = AttackDirections.AreaSize(direction, world.Tuning.TraceWidthR * world.Tuning.Range, world.Tuning.TraceHeight);
            var go = new GameObject("Trace · one shared slot"); go.transform.SetParent(world.transform); go.transform.position = center - Vector2.up * size.y / 2;
            var profile = Profile(element); var state = State(element);
            Target = go.AddComponent<LabTarget>(); Target.Initialize(world, TargetKind.Trace, "검흔", profile, state, size); Target.Trace = this;
            Target.Body.enabled = false; Target.SolidCollider.enabled = false;
            symbol = LabSprites.Quad("Trace core", go.transform, new Vector2(0, Target.Size.y / 2), new Vector2(.08f, .92f), LabSprites.ElementColor(element), 22);
            symbol.transform.localRotation = Quaternion.Euler(0, 0, -17);
            symbol.sprite=LabSprites.TraceGlyph(Element,false);
            if (element == Element.Ice) Note = "냉기 흔적 · 발판 아님 · 열을 가하면 소멸";
        }
        static TargetProfile Profile(Element element)
        {
            var p = new TargetProfile { Fixed = true };
            if (element == Element.Fire) { p.Traits = Trait.Flammable | Trait.Extinguishable | Trait.WindReactive; p.WindRoute = WindRoute.TraceTransport; }
            else if (element == Element.Water || element == Element.Ice)
            { p.Traits = Trait.Freezable | Trait.Meltable | (element == Element.Water ? Trait.WindReactive : Trait.None); p.LiquidBody = true; p.WindRoute = element == Element.Water ? WindRoute.TraceTransport : WindRoute.BodyPush; }
            return p;
        }
        static TargetState State(Element element) => new TargetState(Moisture.None, burning: element == Element.Fire, frozen: element == Element.Ice, liquid: element == Element.Water);
        public bool CanReceive(ActionContext action, Effect effect, out string reason)
        {
            reason = "";
            if (action.Id == createdBy) reason = "RehitBlocked:CreatingAction";
            else if (Flying) reason = "Unsupported:InFlight";
            else if (ReactionRules.Of(Element) == effect) reason = "Unsupported:SameElement";
            else if (Element == Element.Wind) reason = "Unsupported:WindCombination";
            else if (TraceSupport.Get(Element, SolidIce, TraceSupport.FromEffect(effect)) == TraceResponse.Keep) reason = "Keep";
            else if (TraceSupport.Get(Element, SolidIce, TraceSupport.FromEffect(effect)) == TraceResponse.Unsupported) reason = "Unsupported:TraceCombination";
            return reason.Length == 0;
        }
        public void React(Reaction r, ActionContext action, Vector2 hitDirection)
        {
            if(r.Applied)world.Session?.TraceRehit();
            if (r.Consequence == Consequence.Thaw && !SolidIce) { End(); return; }
            if (r.Consequence == Consequence.Extinguish && Element == Element.Fire) { End(); return; }
            if (r.Consequence == Consequence.Transport)
            {
                Flying = true; Platform = false; direction = hitDirection; lineage = action; Traveled = 0; Target.HitCollider.enabled = false; Target.SolidCollider.enabled = false;
                Note = "바람 전달 · 첫 접촉에서 종료";
            }
            if (r.Consequence == Consequence.Freeze)
            { Element = Element.Ice; SolidIce = true; Target.Model.ChangeProfilePreservingState(Profile(Element)); UpdatePlatform(); }
            if (r.Consequence == Consequence.Thaw)
            { Element = Element.Water; SolidIce = false; Target.Model.ChangeProfilePreservingState(Profile(Element)); Platform = false; Target.SolidCollider.enabled = false; Note = "해동 · 최초 만료시각 유지"; }
            if (symbol) symbol.color = LabSprites.ElementColor(Element);
        }
        void UpdatePlatform()
        {
            Physics2D.SyncTransforms(); Platform = false;
            foreach (var hit in Physics2D.RaycastAll(Position, Vector2.down, world.Tuning.IceSupportDistanceR * world.Tuning.Range))
            {
                if (!hit.collider || hit.collider.transform.IsChildOf(Target.transform)) continue;
                var surface = hit.collider.GetComponent<LabSurface>();
                var body = hit.collider.GetComponentInParent<LabTarget>();
                if (surface && surface.PermanentSupport || body && body.Kind == TargetKind.Water) { Platform = true; break; }
            }
            Target.SolidCollider.size = new Vector2(.65f * world.Tuning.Range, world.Tuning.IceThicknessR * world.Tuning.Range);
            Target.SolidCollider.offset = new Vector2(0, Target.Size.y / 2);
            Target.SolidCollider.enabled = Platform;
            Target.SolidCollider.GetComponent<LabSurface>().PermanentSupport = false;
            Note = Platform ? "얼음 발판 · 최초 만료시각 유지" : "지지면 없음 · 시각 동결만 (공중 계단 금지)";
        }
        public void Tick(float dt)
        {
            if (!Active) return;
            if (world.Clock >= ExpiresAt) { End(); return; }
            symbol.color = new Color(LabSprites.ElementColor(Element).r, LabSprites.ElementColor(Element).g, LabSprites.ElementColor(Element).b, Mathf.Clamp01(Remaining * 3));
            symbol.sprite=LabSprites.TraceGlyph(Element,SolidIce&&Platform);
            if (SolidIce && Platform) { symbol.transform.localRotation = Quaternion.identity; symbol.transform.localScale = new Vector3(.65f * world.Tuning.Range, world.Tuning.IceThicknessR * world.Tuning.Range, 1); }
            else { symbol.transform.localRotation = Quaternion.identity;Vector2 size=AttackDirections.AreaSize(direction,.38f,.65f);symbol.transform.localScale = new Vector3(size.x,size.y,1); }
            if (world.Clock % .09f < dt) world.Particle(Position, Element, .35f);
            if (!Flying || dt <= 0) return;
            float step = Mathf.Min(world.Tuning.TransportSpeed * dt, world.Tuning.TransportRangeR * world.Tuning.Range - Traveled);
            var hits = Physics2D.CircleCastAll(Position, .10f, direction, step);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (!hit.collider || hit.collider.transform.IsChildOf(Target.transform) || hit.collider.GetComponentInParent<LabPlayer>()) continue;
                var target = hit.collider.GetComponentInParent<LabTarget>();
                if (target && target.Model.Alive && target.Trace == null)
                { lineage.Source = "TraceTransport"; target.Receive(lineage, ReactionRules.Of(Element), 0, direction); End(); return; }
                if (!hit.collider.isTrigger && hit.collider.GetComponent<LabSurface>()) { End(); return; }
            }
            Target.transform.position += (Vector3)(direction * step); Traveled += step; Physics2D.SyncTransforms();
            if (Traveled >= world.Tuning.TransportRangeR * world.Tuning.Range - .0001f) End();
        }
        public void End()
        {
            if (!Target) return;
            Target.gameObject.SetActive(false); Object.Destroy(Target.gameObject); Target = null;
        }
    }
}
