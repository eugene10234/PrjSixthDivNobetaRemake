using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// ForwardMovementStrategy  ── 直線向前（原有行為）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class ForwardMovementStrategy : IMovementStrategy
    {
        public static readonly ForwardMovementStrategy Default = new();

        public void OnInitialize(BulletState state)
        {
            state.Velocity = state.Direction * state.Speed;
        }

        public void Update(BulletState state, float deltaTime)
        {
            state.Position += state.Velocity * deltaTime;
            UpdateRotation(state);
        }

        private static void UpdateRotation(BulletState state)
        {
            if (state.Velocity == Vector3.zero) return;

            // 保留原本的 SinRotate 效果
            Quaternion look = Quaternion.LookRotation(state.Velocity);
            state.Rotation = look;
        }
    }
}