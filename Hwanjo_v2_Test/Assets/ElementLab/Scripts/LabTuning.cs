using UnityEngine;

namespace Hwanjo.ElementLab
{
    [CreateAssetMenu(menuName = "Hwanjo/Element Lab Tuning")]
    public sealed class LabTuning : ScriptableObject
    {
        [Header("Temporary scale and movement")]
        public float Range = 1.6f, MoveSpeed = 4.2f, JumpSpeed = 8.5f, Gravity = 23, DashSpeed = 11, DashSeconds = .16f, DashCooldown = .55f;
        [Header("Temporary attack phases: windup / active / recovery")]
        public float SingleWindup = .06f, ActiveSeconds = .06f, SingleRecovery = .20f, ChargeWindup = .08f, ChargeRecovery = .28f;
        public float SwordDamage = 10, PlayerHealth = 100, EnemyHealth = 100;
        [Header("Temporary trace values (same slot, original expiry)")]
        public float TraceSeconds = 2, TraceCenterR = .6f, TraceWidthR = .25f, TraceHeight = .8f, TransportRangeR = 3, TransportSpeed = 8;
        public float IceSupportDistanceR = .5f, IceThicknessR = .15f;
        [Header("Enemy and feedback")]
        public float EnemySpeed = 1.2f, EnemyWindup = .65f, EnemyActive = .1f, EnemyRecovery = .75f, EnemyRange = 1.15f, EnemyDamage = 8;
        public float PushSpeed = 4.8f, PushLift = 2.4f, HitStop = .045f, HitFlash = .10f, CameraShake = .045f;
        public StatusTuning Status = new StatusTuning();
        [Header("Camera only: seconds, independent of movement")]
        public float CameraDampingX = .12f, CameraDampingY = .08f;
        public float ChargeRange => Range * AttackInput.ChargeRatio;
        public float SingleDuration => SingleWindup + ActiveSeconds + SingleRecovery;
        public float ChargeDuration => ChargeWindup + ActiveSeconds + ChargeRecovery;
    }
}
