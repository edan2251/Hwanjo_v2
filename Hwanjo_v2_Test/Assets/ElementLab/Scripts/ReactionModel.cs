using System;
using System.Collections.Generic;
using System.Threading;

namespace Hwanjo.ElementLab
{
    [Flags] public enum Trait { None = 0, Flammable = 1, Wettable = 2, Freezable = 4, Meltable = 8, Extinguishable = 16, Pushable = 32, WindReactive = 64 }
    public enum Element { Fire, Water, Wind, Ice }
    public enum Effect { Heat, Moisture, WindImpulse, Cold }
    public enum Moisture { None, Dry, Wet }
    public enum FreezeCondition { WetOrLiquid, IntrinsicMoisture }
    public enum WindRoute { BodyPush, Device, TraceTransport }
    public enum Consequence { None, Thaw, Dry, Ignite, Extinguish, Wet, Freeze, Push, Rotate, Transport }
    public enum Preset { Custom, DryVine, WetVine, WoodenBox, Water, WindDevice, FreezableDevice }

    [Serializable] public struct TargetState
    {
        public Moisture Moisture;
        public bool Burning, Frozen, Liquid;
        public TargetState(Moisture moisture, bool burning = false, bool frozen = false, bool liquid = false)
        { Moisture = moisture; Burning = burning; Frozen = frozen; Liquid = liquid; }
        public override string ToString()
        {
            string value = Liquid ? "Liquid" : Moisture.ToString();
            return value + (Burning ? " + Burning" : "") + (Frozen ? " + Frozen" : "");
        }
    }

    [Serializable] public struct TargetProfile
    {
        public Trait Traits;
        public FreezeCondition FreezeCondition;
        public WindRoute WindRoute;
        public bool Fixed, LiquidBody, TimedStatuses, ConsumeOnBurnout;
        public bool Has(Trait trait) => (Traits & trait) != 0;
        public string Validate(TargetState state)
        {
            if (state.Burning && state.Frozen) return "연소와 동결을 동시에 적용할 수 없습니다.";
            if (state.Liquid && state.Frozen) return "물과 동결을 동시에 적용할 수 없습니다.";
            if (state.Burning && state.Moisture == Moisture.Wet) return "연소와 젖음을 동시에 적용할 수 없습니다.";
            if (state.Burning && !Has(Trait.Flammable)) return "연소에는 가연성 특성이 필요합니다.";
            if (state.Frozen && !Has(Trait.Freezable)) return "동결에는 동결 가능 특성이 필요합니다.";
            if (state.Liquid && !LiquidBody) return "물 상태에는 물 재질이 필요합니다.";
            if (state.Moisture != Moisture.None && !Has(Trait.Wettable)) return "건조/젖음에는 젖음 가능 특성이 필요합니다.";
            return "";
        }
        public static TargetProfile For(Preset preset)
        {
            var p = new TargetProfile { Fixed = true, WindRoute = WindRoute.BodyPush };
            switch (preset)
            {
                case Preset.Custom: case Preset.DryVine: case Preset.WetVine:
                    p.Traits = Trait.Flammable | Trait.Wettable | Trait.Extinguishable; break;
                case Preset.WoodenBox:
                    p.Traits = Trait.Flammable | Trait.Wettable | Trait.Extinguishable | Trait.Pushable; p.Fixed = false; break;
                case Preset.Water:
                    p.Traits = Trait.Freezable | Trait.Meltable; p.LiquidBody = true; break;
                case Preset.WindDevice:
                    p.Traits = Trait.WindReactive; p.WindRoute = WindRoute.Device; break;
                case Preset.FreezableDevice:
                    p.Traits = Trait.Wettable | Trait.Freezable | Trait.Meltable; break;
            }
            return p;
        }
        public static TargetState Initial(Preset preset) => preset == Preset.Water
            ? new TargetState(Moisture.None, liquid: true)
            : new TargetState(preset == Preset.WindDevice ? Moisture.None : preset == Preset.WetVine ? Moisture.Wet : Moisture.Dry);
    }

    public readonly struct Reaction
    {
        public readonly string Rule, Reason;
        public readonly TargetState Before, After;
        public readonly Consequence Consequence;
        public bool Applied => Consequence != Consequence.None;
        public Reaction(string rule, TargetState before, TargetState after, Consequence consequence, string reason = "")
        { Rule = rule; Before = before; After = after; Consequence = consequence; Reason = reason; }
        public static Reaction None(TargetState state, string reason) => new Reaction("EL00", state, state, Consequence.None, reason);
        public override string ToString() => Rule + " " + Before + " → " + After + (Reason.Length > 0 ? " (" + Reason + ")" : "");
    }

    public static class ReactionRules
    {
        public static Effect Of(Element element) => element == Element.Fire ? Effect.Heat : element == Element.Water ? Effect.Moisture : element == Element.Wind ? Effect.WindImpulse : Effect.Cold;
        public static Reaction Evaluate(TargetProfile p, TargetState before, Effect effect)
        {
            var after = before;
            switch (effect)
            {
                case Effect.Heat:
                    if (before.Frozen)
                    {
                        if (!p.Has(Trait.Meltable)) return Reaction.None(before, "NoTrait:Meltable");
                        after.Frozen = false; after.Liquid = p.LiquidBody;
                        after.Moisture = p.Has(Trait.Wettable) ? Moisture.Wet : Moisture.None;
                        return new Reaction("EL01", before, after, Consequence.Thaw);
                    }
                    if (before.Burning) return Reaction.None(before, "WrongState:AlreadyBurning");
                    if (before.Moisture == Moisture.Wet && p.Has(Trait.Wettable))
                    { after.Moisture = Moisture.Dry; return new Reaction("EL02", before, after, Consequence.Dry); }
                    if (before.Liquid) return Reaction.None(before, "Unsupported:Evaporation");
                    if (p.Has(Trait.Flammable))
                    { after.Burning = true; return new Reaction("EL03", before, after, Consequence.Ignite); }
                    return Reaction.None(before, "NoTrait:Flammable");
                case Effect.Moisture:
                    if (before.Burning)
                    {
                        if (!p.Has(Trait.Extinguishable)) return Reaction.None(before, "NoTrait:Extinguishable");
                        after.Burning = false;
                        after.Moisture = p.Has(Trait.Wettable) ? Moisture.Wet : before.Moisture;
                        return new Reaction("EL04", before, after, Consequence.Extinguish);
                    }
                    if (before.Frozen) return Reaction.None(before, "WrongState:Frozen");
                    if (p.Has(Trait.Wettable))
                    { after.Moisture = Moisture.Wet; return new Reaction("EL05", before, after, Consequence.Wet); }
                    return Reaction.None(before, "NoTrait:Wettable");
                case Effect.Cold:
                    if (before.Burning)
                    {
                        if (!p.Has(Trait.Extinguishable)) return Reaction.None(before, "NoTrait:Extinguishable");
                        after.Burning = false; return new Reaction("EL06", before, after, Consequence.Extinguish);
                    }
                    if (before.Frozen) return Reaction.None(before, "WrongState:AlreadyFrozen");
                    if (!p.Has(Trait.Freezable)) return Reaction.None(before, "NoTrait:Freezable");
                    if (before.Moisture != Moisture.Wet && !before.Liquid && p.FreezeCondition != FreezeCondition.IntrinsicMoisture)
                        return Reaction.None(before, "WrongState:NeedsMoisture");
                    after.Frozen = true; after.Liquid = false;
                    return new Reaction("EL07", before, after, Consequence.Freeze);
                case Effect.WindImpulse:
                    if (p.Has(Trait.WindReactive) && p.WindRoute != WindRoute.BodyPush)
                        return new Reaction("EL09", before, after, p.WindRoute == WindRoute.Device ? Consequence.Rotate : Consequence.Transport);
                    if (!p.Has(Trait.Pushable)) return Reaction.None(before, "NoTrait:Pushable");
                    if (p.Fixed) return Reaction.None(before, "Immovable");
                    return new Reaction("EL08", before, after, Consequence.Push);
                default: return Reaction.None(before, "Unsupported");
            }
        }
    }

    public sealed class ActionContext
    {
        static long next;
        public readonly long Id;
        public string Source = "Sword";
        public readonly HashSet<int> Visited = new HashSet<int>();
        public ActionContext() { Id = Interlocked.Increment(ref next); }
        public ActionContext(long id) { Id = id; }
    }

    [Serializable] public class StatusTuning
    {
        public float BurnSeconds = 3, BurnTick = .5f, WetSeconds = 2, FreezeSeconds = .8f, RefreezeDelay = .6f, WindDelay = .5f, WetSpeed = .8f;
        public int BurnDamage = 2;
    }

    public sealed class TargetModel
    {
        public readonly int Id;
        public TargetProfile Profile { get; private set; }
        public TargetState State { get; private set; }
        public TargetState InitialState { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool Damageable { get; private set; }
        public bool Consumed { get; private set; }
        public float BurnRemaining, WetRemaining, FreezeRemaining, FreezeCooldown, WindCooldown;
        public int DotTicks { get; private set; }
        public int PhysicalHits { get; private set; }
        public int Reactions { get; private set; }
        public Reaction Last { get; private set; }
        public readonly StatusTuning Tuning;
        readonly HashSet<long> handled = new HashSet<long>();
        readonly Queue<long> history = new Queue<long>();
        float burnClock;
        public bool Alive => !Consumed && Health > 0;
        public TargetModel(int id, TargetProfile profile, TargetState state, StatusTuning tuning = null, float hp = 100, bool damageable = true)
        { Id = id; Tuning = tuning ?? new StatusTuning(); Apply(profile, state, hp, damageable); }
        public void Apply(TargetProfile profile, TargetState state, float hp = 100, bool damageable = true)
        {
            string error = profile.Validate(state); if (error.Length > 0) throw new ArgumentException(error);
            Profile = profile; InitialState = state; MaxHealth = hp; Damageable = damageable; Reset();
        }
        public void Reset()
        {
            State = InitialState; Health = MaxHealth; Consumed = false;
            BurnRemaining = State.Burning ? Tuning.BurnSeconds : 0; burnClock = 0;
            WetRemaining = State.Moisture == Moisture.Wet ? Tuning.WetSeconds : 0;
            FreezeRemaining = State.Frozen ? Tuning.FreezeSeconds : 0;
            FreezeCooldown = WindCooldown = 0; DotTicks = PhysicalHits = Reactions = 0;
            handled.Clear(); history.Clear(); Last = Reaction.None(State, "Reset");
        }
        public void ChangeProfilePreservingState(TargetProfile profile)
        {
            string error = profile.Validate(State); if (error.Length > 0) throw new ArgumentException(error);
            Profile = profile;
        }
        public Reaction Receive(ActionContext action, Effect effect, float damage = 0)
        {
            if (handled.Contains(action.Id) || !action.Visited.Add(Id)) return Reaction.None(State, "RehitBlocked");
            handled.Add(action.Id); history.Enqueue(action.Id); if (history.Count > 256) handled.Remove(history.Dequeue());
            if (!Alive) return Last = Reaction.None(State, "WrongState:Inactive");
            if (damage > 0) { PhysicalHits++; if (Damageable) Health = Math.Max(0, Health - damage); }
            Reaction reaction = ReactionRules.Evaluate(Profile, State, effect);
            if (reaction.Consequence == Consequence.Freeze && FreezeCooldown > 0) reaction = Reaction.None(State, "RehitBlocked:FreezeCooldown");
            if (reaction.Consequence == Consequence.Push && WindCooldown > 0) reaction = Reaction.None(State, "RehitBlocked:WindCooldown");
            if (reaction.Applied)
            {
                Reactions++;
                State = reaction.After;
                if (reaction.Consequence == Consequence.Ignite) { BurnRemaining = Tuning.BurnSeconds; burnClock = 0; DotTicks = 0; }
                if (!State.Burning) { BurnRemaining = 0; burnClock = 0; }
                if (State.Moisture == Moisture.Wet && (reaction.Consequence == Consequence.Wet || reaction.Consequence == Consequence.Thaw || reaction.Consequence == Consequence.Extinguish)) WetRemaining = Tuning.WetSeconds;
                if (State.Moisture != Moisture.Wet) WetRemaining = 0;
                if (reaction.Consequence == Consequence.Freeze) FreezeRemaining = Tuning.FreezeSeconds;
                if (reaction.Consequence == Consequence.Thaw) { FreezeRemaining = 0; FreezeCooldown = Tuning.RefreezeDelay; }
                if (reaction.Consequence == Consequence.Push) WindCooldown = Tuning.WindDelay;
            }
            return Last = reaction;
        }
        public void Tick(float dt)
        {
            if (dt <= 0 || !Alive) return;
            FreezeCooldown = Math.Max(0, FreezeCooldown - dt); WindCooldown = Math.Max(0, WindCooldown - dt);
            var state = State;
            if (state.Burning)
            {
                float active = Math.Min(dt, BurnRemaining); burnClock += active; BurnRemaining = Math.Max(0, BurnRemaining - dt);
                while (burnClock + .00001f >= Tuning.BurnTick && DotTicks < (int)Math.Round(Tuning.BurnSeconds / Tuning.BurnTick))
                { burnClock -= Tuning.BurnTick; DotTicks++; if (Damageable) Health = Math.Max(0, Health - Tuning.BurnDamage); }
                if (BurnRemaining <= .00001f) { state.Burning = false; Consumed = Profile.ConsumeOnBurnout; Last = new Reaction("EL10", State, state, Consequence.None, "FuelExpired"); }
            }
            if (Profile.TimedStatuses)
            {
                if (state.Frozen)
                {
                    FreezeRemaining -= dt;
                    if (FreezeRemaining <= .00001f)
                    {
                        state.Frozen = false; state.Liquid = Profile.LiquidBody;
                        state.Moisture = Profile.Has(Trait.Wettable) ? Moisture.Wet : Moisture.None;
                        WetRemaining = Tuning.WetSeconds; FreezeCooldown = Tuning.RefreezeDelay;
                    }
                }
                else if (state.Moisture == Moisture.Wet)
                { WetRemaining -= dt; if (WetRemaining <= .00001f) state.Moisture = Moisture.Dry; }
            }
            State = state;
        }
    }

    public enum AttackKind { None, Single, Charged }
    public sealed class AttackInput
    {
        public const float ChargeThreshold = .5f, ChargeRatio = 1f;
        public bool Held { get; private set; }
        public float HeldSeconds { get; private set; }
        bool groundStart;
        public void Press(bool grounded) { Held = true; HeldSeconds = 0; groundStart = grounded; }
        public void Tick(float dt) { if (Held) HeldSeconds += Math.Max(0, dt); }
        public void Cancel() { Held = false; HeldSeconds = 0; }
        public AttackKind Release(bool grounded)
        {
            if (!Held) return AttackKind.None;
            var result = HeldSeconds + .000001f >= ChargeThreshold
                ? (groundStart && grounded ? AttackKind.Charged : AttackKind.None) : AttackKind.Single;
            Cancel(); return result;
        }
    }
}
