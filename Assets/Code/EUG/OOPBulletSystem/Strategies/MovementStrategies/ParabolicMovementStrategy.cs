using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// ParabolicMovementStrategy  ── 拋物線（手榴彈 / 投擲火球）
//
// 物理公式：
//   P(t) = Origin + Horizontal * t + 0.5 * gravity * t²
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class ParabolicMovementStrategy : IMovementStrategy
    {
        private readonly float _gravity;       // 重力加速度（正值 = 向下）
        private readonly float _launchPitchUp; // 初始仰角（度），讓拋物線更漂亮

        /// <param name="gravity">重力加速度（建議 9.8 ~ 20）</param>
        /// <param name="launchPitchUp">初始仰角補正（度），用來讓水平瞄準的子彈也能畫出拋物線</param>
        public ParabolicMovementStrategy(float gravity = 9.8f, float launchPitchUp = 25f)
        {
            _gravity = gravity;
            _launchPitchUp = launchPitchUp;
        }

        public void OnInitialize(BulletState state)
        {
            state.ElapsedTime = 0f;
            state.LaunchPosition = state.Position;

            // 對發射方向加上仰角讓軌跡更自然
            Quaternion pitchUp = Quaternion.AngleAxis(-_launchPitchUp, Vector3.Cross(state.Direction, Vector3.up));
            Vector3 adjustedDir = pitchUp * state.Direction;
            state.Velocity = adjustedDir.normalized * state.Speed;
        }

        public void Update(BulletState state, float deltaTime)
        {
            state.ElapsedTime += deltaTime;

            float t = state.ElapsedTime;

            // 水平分量（保持速度不衰減）
            Vector3 horizontal = new Vector3(state.Velocity.x, 0f, state.Velocity.z);

            // 垂直分量（自由落體）
            float vy = state.Velocity.y - _gravity * t;

            // 新位置
            Vector3 newPos = state.LaunchPosition
                            + horizontal * t
                            + new Vector3(0f, state.Velocity.y * t - 0.5f * _gravity * t * t, 0f);

            // 速度（朝向下一幀的移動方向，讓子彈頭指向飛行方向）
            Vector3 currentVel = horizontal + Vector3.up * vy;
            state.Velocity = currentVel;
            state.Position = newPos;

            if (currentVel != Vector3.zero)
                state.Rotation = Quaternion.LookRotation(currentVel);
        }
    }
}