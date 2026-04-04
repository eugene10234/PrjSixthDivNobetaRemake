using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// SinWaveMovementStrategy  ── 正弦波飛行（波浪子彈）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class SinWaveMovementStrategy : IMovementStrategy
    {
        private readonly float _amplitude; // 振幅（Units）
        private readonly float _frequency; // 頻率（Hz）

        public SinWaveMovementStrategy(float amplitude = 1f, float frequency = 2f)
        {
            _amplitude = amplitude;
            _frequency = frequency;
        }

        public void OnInitialize(BulletState state)
        {
            state.ElapsedTime = 0f;
            state.LaunchPosition = state.Position;
            state.Velocity = state.Direction * state.Speed;
        }

        public void Update(BulletState state, float deltaTime)
        {
            state.ElapsedTime += deltaTime;

            // 主軸前進
            Vector3 forward = state.Direction * state.Speed * state.ElapsedTime;

            // 側向正弦偏移（以初始 up/right 決定擺動平面）
            Vector3 right = Vector3.Cross(state.Direction, Vector3.up).normalized;
            float sideOff = Mathf.Sin(state.ElapsedTime * _frequency * Mathf.PI * 2f) * _amplitude;

            state.Position = state.LaunchPosition + forward + right * sideOff;

            // 速度方向（切線方向，讓子彈頭正確轉向）
            float sideVel = Mathf.Cos(state.ElapsedTime * _frequency * Mathf.PI * 2f)
                           * _amplitude * _frequency * Mathf.PI * 2f;
            Vector3 tangent = (state.Direction * state.Speed + right * sideVel).normalized;
            state.Velocity = tangent * state.Speed;

            if (tangent != Vector3.zero)
                state.Rotation = Quaternion.LookRotation(tangent);
        }
    }
}