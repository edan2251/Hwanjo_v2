using UnityEngine;

namespace Hwanjo.ElementLab
{
    public sealed class LabSurface : MonoBehaviour { public bool PermanentSupport = true; }

    public sealed class LabMotor
    {
        public readonly Transform Transform;
        public Vector2 Size;
        public Vector2 Velocity;
        public bool Grounded { get; private set; }
        const float Skin = .015f;
        public LabMotor(Transform transform, Vector2 size) { Transform = transform; Size = size; }
        public void Clear()
        {
            Velocity = Vector2.zero; Physics2D.SyncTransforms();
            Grounded = Distance(Vector2.down, .06f) < .055f;
        }
        public void Move(float dt, float desiredX, float gravity, float externalX = 0)
        {
            if (dt <= 0) return;
            Physics2D.SyncTransforms();
            Velocity = new Vector2(desiredX + externalX, Mathf.Max(Velocity.y - gravity * dt, -16));
            MoveAxis(Vector2.right, Velocity.x * dt);
            Grounded = false;
            MoveAxis(Vector2.up, Velocity.y * dt);
            if (!Grounded && Velocity.y <= 0) Grounded = Distance(Vector2.down, .06f) < .055f;
            if (Grounded && Velocity.y < 0) Velocity = new Vector2(Velocity.x, 0);
        }
        void MoveAxis(Vector2 axis, float amount)
        {
            if (Mathf.Abs(amount) < .000001f) return;
            Vector2 direction = axis * Mathf.Sign(amount);
            float distance = Distance(direction, Mathf.Abs(amount) + Skin);
            float travel = Mathf.Min(Mathf.Abs(amount), Mathf.Max(0, distance - Skin));
            Transform.position += (Vector3)(direction * travel);
            if (travel + .00001f < Mathf.Abs(amount) && axis.y > 0)
            { if (amount < 0) Grounded = true; Velocity = new Vector2(Velocity.x, 0); }
            Physics2D.SyncTransforms();
        }
        float Distance(Vector2 direction, float max)
        {
            Vector2 center = (Vector2)Transform.position + Vector2.up * Size.y / 2;
            float nearest = max;
            foreach (var hit in Physics2D.BoxCastAll(center, Size - Vector2.one * Skin * 2, 0, direction, max))
            {
                if (!hit.collider || hit.collider.isTrigger || hit.collider.transform.IsChildOf(Transform)) continue;
                if (!hit.collider.GetComponent<LabSurface>()) continue;
                if (hit.distance < .0001f && Vector2.Dot(hit.normal, direction) > -.5f) continue;
                nearest = Mathf.Min(nearest, hit.distance);
            }
            return nearest;
        }
    }
}
