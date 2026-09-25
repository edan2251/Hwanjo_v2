using NUnit.Framework;
using UnityEngine;

namespace Hwanjo.ElementLab.Tests
{
    public class ReactionTests
    {
        static TargetModel Vine(bool wet = false) => new TargetModel(1, TargetProfile.For(Preset.DryVine), new TargetState(wet ? Moisture.Wet : Moisture.Dry));
        static TargetModel Enemy()
        {
            var p = TargetProfile.For(Preset.WoodenBox);
            p.Traits |= Trait.Freezable | Trait.Meltable; p.FreezeCondition = FreezeCondition.IntrinsicMoisture; p.TimedStatuses = true;
            return new TargetModel(7, p, new TargetState(Moisture.Dry));
        }
        [TestCase(Preset.DryVine)] [TestCase(Preset.WetVine)] [TestCase(Preset.WoodenBox)]
        public void SameTraitsShareIgnitionRegardlessOfPreset(Preset preset)
        {
            var p = TargetProfile.For(preset);
            Assert.AreEqual("EL03", ReactionRules.Evaluate(p, new TargetState(Moisture.Dry), Effect.Heat).Rule);
        }
        [Test] public void OneInputDriesWithoutIgnitingAndNextInputIgnites()
        {
            var target = Vine(true); var a = new ActionContext();
            Assert.AreEqual("EL02", target.Receive(a, Effect.Heat, 10).Rule);
            Assert.IsFalse(target.State.Burning); Assert.AreEqual(Moisture.Dry, target.State.Moisture);
            Assert.AreEqual("RehitBlocked", target.Receive(a, Effect.Heat, 10).Reason);
            Assert.AreEqual(90, target.Health); Assert.AreEqual(1, target.PhysicalHits);
            Assert.AreEqual("EL03", target.Receive(new ActionContext(), Effect.Heat).Rule);
        }
        [Test] public void DistinctContextsCannotRepeatSameActionId()
        {
            var target = Vine(true); target.Receive(new ActionContext(900), Effect.Heat, 10);
            Assert.AreEqual("RehitBlocked", target.Receive(new ActionContext(900), Effect.Heat, 10).Reason);
            Assert.AreEqual(1, target.PhysicalHits); Assert.IsFalse(target.State.Burning);
        }
        [Test] public void OneActionCanReachSeveralTargetsOnceEach()
        {
            var a = new ActionContext(); var x = Vine(); var y = new TargetModel(2, x.Profile, x.State);
            Assert.AreEqual("EL03", x.Receive(a, Effect.Heat).Rule); Assert.AreEqual("EL03", y.Receive(a, Effect.Heat).Rule);
        }
        [Test] public void TransportLineageCannotDryThenIgniteVisitedTarget()
        {
            var target = Vine(true); var lineage = new ActionContext();
            target.Receive(lineage, Effect.Heat); target.Receive(lineage, Effect.Heat);
            Assert.AreEqual(Moisture.Dry, target.State.Moisture); Assert.IsFalse(target.State.Burning);
        }
        [Test] public void HeatThawsBeforeDryingOrIgniting()
        {
            var target = Enemy(); target.Receive(new ActionContext(), Effect.Cold);
            Assert.IsTrue(target.State.Frozen);
            Assert.AreEqual("EL01", target.Receive(new ActionContext(), Effect.Heat).Rule);
            Assert.AreEqual(Moisture.Wet, target.State.Moisture); Assert.IsFalse(target.State.Frozen || target.State.Burning);
        }
        [Test] public void MoistureExtinguishesIntoWetAndClearsDamageTimer()
        {
            var target = Vine(); target.Receive(new ActionContext(), Effect.Heat); target.Tick(.6f);
            Assert.AreEqual(98, target.Health);
            Assert.AreEqual("EL04", target.Receive(new ActionContext(), Effect.Moisture).Rule);
            target.Tick(9); Assert.AreEqual(98, target.Health); Assert.AreEqual(Moisture.Wet, target.State.Moisture);
        }
        [Test] public void ColdExtinguishesBeforeFreezing()
        {
            var target = Enemy(); target.Receive(new ActionContext(), Effect.Heat);
            Assert.AreEqual("EL06", target.Receive(new ActionContext(), Effect.Cold).Rule);
            Assert.IsFalse(target.State.Frozen || target.State.Burning);
        }
        [Test] public void DryEnvironmentNeedsMoistureButEnemyProfileFreezesDirectly()
        {
            var p = TargetProfile.For(Preset.FreezableDevice);
            Assert.IsFalse(ReactionRules.Evaluate(p, new TargetState(Moisture.Dry), Effect.Cold).Applied);
            Assert.AreEqual("EL07", ReactionRules.Evaluate(p, new TargetState(Moisture.Wet), Effect.Cold).Rule);
            Assert.AreEqual("EL07", Enemy().Receive(new ActionContext(), Effect.Cold).Rule);
        }
        [Test] public void WaterFreezesAndThawsToLiquid()
        {
            var t = new TargetModel(3, TargetProfile.For(Preset.Water), TargetProfile.Initial(Preset.Water));
            t.Receive(new ActionContext(), Effect.Cold); Assert.IsFalse(t.State.Liquid); Assert.IsTrue(t.State.Frozen);
            t.Receive(new ActionContext(), Effect.Heat); Assert.IsTrue(t.State.Liquid); Assert.IsFalse(t.State.Frozen);
        }
        [Test] public void PhysicalDamageDoesNotRequireElementReaction()
        {
            var p = new TargetProfile { Traits = Trait.Pushable };
            var t = new TargetModel(3, p, new TargetState(Moisture.None));
            Assert.IsFalse(t.Receive(new ActionContext(), Effect.Heat, 10).Applied); Assert.AreEqual(90, t.Health);
        }
        [TestCase(WindRoute.BodyPush, "EL08", Consequence.Push)]
        [TestCase(WindRoute.Device, "EL09", Consequence.Rotate)]
        [TestCase(WindRoute.TraceTransport, "EL09", Consequence.Transport)]
        public void WindWithBothTraitsTakesExactlyOneConfiguredRoute(WindRoute route, string rule, Consequence consequence)
        {
            var p = new TargetProfile { Traits = Trait.Pushable | Trait.WindReactive, WindRoute = route };
            var r = ReactionRules.Evaluate(p, new TargetState(Moisture.None), Effect.WindImpulse);
            Assert.AreEqual(rule, r.Rule); Assert.AreEqual(consequence, r.Consequence);
        }
        [Test] public void FixedBodyDoesNotMoveButFixedDeviceRotates()
        {
            var p = TargetProfile.For(Preset.WoodenBox); p.Fixed = true;
            Assert.AreEqual("Immovable", ReactionRules.Evaluate(p, new TargetState(Moisture.Dry), Effect.WindImpulse).Reason);
            Assert.AreEqual(Consequence.Rotate, ReactionRules.Evaluate(TargetProfile.For(Preset.WindDevice), new TargetState(Moisture.None), Effect.WindImpulse).Consequence);
        }
        [Test] public void BurnHasSixTicksAndHeatDoesNotExtendIt()
        {
            var t = Enemy(); t.Receive(new ActionContext(), Effect.Heat);
            for (int i = 0; i < 6; i++) { t.Tick(.5f); if (i < 5) t.Receive(new ActionContext(), Effect.Heat); if (i == 4) Assert.LessOrEqual(t.BurnRemaining, .5f); }
            Assert.AreEqual(88, t.Health); Assert.IsFalse(t.State.Burning); Assert.AreEqual(6, t.DotTicks);
        }
        [Test] public void DotDoesNotEmitHeatOrExtraPhysicalHits()
        {
            var t = Enemy(); t.Receive(new ActionContext(), Effect.Heat, 10); t.Tick(3);
            Assert.AreEqual(6, t.DotTicks); Assert.AreEqual(1, t.PhysicalHits); Assert.AreEqual(1, t.Reactions); Assert.AreEqual(78, t.Health);
        }
        [Test] public void RefreezeAndWindCooldownPreventContinuousControl()
        {
            var t = Enemy(); t.Receive(new ActionContext(), Effect.Cold); t.Tick(.8f);
            Assert.IsFalse(t.State.Frozen); Assert.That(t.Receive(new ActionContext(), Effect.Cold).Reason, Does.Contain("FreezeCooldown"));
            t.Tick(.61f); Assert.IsTrue(t.Receive(new ActionContext(), Effect.Cold).Applied);
            Assert.IsTrue(t.Receive(new ActionContext(), Effect.WindImpulse).Applied);
            Assert.That(t.Receive(new ActionContext(), Effect.WindImpulse).Reason, Does.Contain("WindCooldown"));
        }
        [Test] public void WetRefreshesWithoutStackingAndEventuallyExpires()
        {
            var t = Enemy(); t.Receive(new ActionContext(), Effect.Moisture); t.Tick(1.5f); t.Receive(new ActionContext(), Effect.Moisture);
            Assert.AreEqual(2, t.WetRemaining); t.Tick(2.01f); Assert.AreEqual(Moisture.Dry, t.State.Moisture);
        }
        [Test] public void FrozenDoesNotRefreshOnRepeatedCold()
        {
            var t = Enemy(); t.Receive(new ActionContext(), Effect.Cold); t.Tick(.4f); t.Receive(new ActionContext(), Effect.Cold);
            Assert.That(t.FreezeRemaining, Is.EqualTo(.4f).Within(.0001));
        }
        [Test] public void ApplyAndResetClearPreviousTimersAndRememberAppliedState()
        {
            var t = Vine(); t.Receive(new ActionContext(), Effect.Heat, 10);
            var p = TargetProfile.For(Preset.Water); var s = TargetProfile.Initial(Preset.Water);
            t.Apply(p, s); Assert.AreEqual(0, t.BurnRemaining); Assert.AreEqual(100, t.Health);
            t.Receive(new ActionContext(), Effect.Cold); t.Reset(); Assert.IsTrue(t.State.Liquid); Assert.IsFalse(t.State.Frozen);
        }
        [Test] public void InvalidStatesAreRejectedInsteadOfSilentlyCorrected()
        {
            var p = TargetProfile.For(Preset.Water);
            Assert.IsNotEmpty(p.Validate(new TargetState(Moisture.None, frozen: true, liquid: true)));
            Assert.IsNotEmpty(Enemy().Profile.Validate(new TargetState(Moisture.Dry, burning: true, frozen: true)));
        }
        [Test] public void BurnedEnvironmentIsRestoredByReset()
        {
            var p = TargetProfile.For(Preset.DryVine); p.ConsumeOnBurnout = true;
            var t = new TargetModel(4, p, TargetProfile.Initial(Preset.DryVine)); t.Receive(new ActionContext(), Effect.Heat); t.Tick(3);
            Assert.IsTrue(t.Consumed); t.Reset(); Assert.IsTrue(t.Alive);
        }
        [TestCase(.49f, AttackKind.Single)] [TestCase(.5f, AttackKind.Charged)] [TestCase(8f, AttackKind.Charged)]
        public void ReleaseProducesOneAttackAtExactThreshold(float duration, AttackKind expected)
        {
            var input = new AttackInput(); input.Press(true); input.Tick(duration);
            Assert.IsTrue(input.Held); Assert.AreEqual(expected, input.Release(true)); Assert.AreEqual(AttackKind.None, input.Release(true));
        }
        [Test] public void CancelDiscardsHeldInputAndAirCannotCharge()
        {
            var input = new AttackInput(); input.Press(true); input.Tick(1); input.Cancel(); Assert.AreEqual(AttackKind.None, input.Release(true));
            input.Press(false); input.Tick(1); Assert.AreEqual(AttackKind.Single, input.Release(false));
        }
        [Test] public void ChargeRangeIsWholeReachFromSameOrigin()
        {
            var tuning = ScriptableObject.CreateInstance<LabTuning>(); Assert.AreEqual(1.75f * tuning.Range, tuning.ChargeRange);
            Assert.LessOrEqual((tuning.TraceCenterR + tuning.TraceWidthR / 2) * tuning.Range, tuning.Range);
            Object.DestroyImmediate(tuning);
        }
    }
}
