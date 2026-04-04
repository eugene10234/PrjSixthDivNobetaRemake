using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// HomingMovementStrategy  ── 追尾導彈
//
// 每幀平滑地把速度方向轉向目標，turnSpeed 控制轉向靈敏度。
// 如果目標消失（死亡 / 超出範圍）則退化成直線飛行。
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class HomingMovementStrategy : IMovementStrategy
    {
        private readonly float _turnSpeed;       // 轉向速度（度/秒）
        private readonly float _maxHomingRange;  // 超過此距離放棄追蹤

        /// <param name="turnSpeed">轉向速度，越大越靈敏（建議 90~360）</param>
        /// <param name="maxHomingRange">放棄追蹤的距離（0 = 永不放棄）</param>
        public HomingMovementStrategy(float turnSpeed = 180f, float maxHomingRange = 0f)
        {
            _turnSpeed = turnSpeed;
            _maxHomingRange = maxHomingRange;
        }

        public void OnInitialize(BulletState state)
        {
            state.Velocity = state.Direction * state.Speed;
        }

        public void Update(BulletState state, float deltaTime)
        {
            if (state.Target != null && state.Target.gameObject.activeInHierarchy)
            {
                Vector3 toTarget = state.Target.position - state.Position;
                float dist = toTarget.magnitude;

                // 超出追蹤範圍就退化為直線
                bool inRange = _maxHomingRange <= 0f || dist <= _maxHomingRange;

                if (inRange && dist > 0.01f)
                {
                    // 用 RotateTowards 讓轉向有限速，避免瞬間鎖定
                    Vector3 desiredDir = toTarget.normalized * state.Speed;
                    state.Velocity = Vector3.RotateTowards(
                        state.Velocity,
                        desiredDir,
                        _turnSpeed * Mathf.Deg2Rad * deltaTime,
                        0f
                    );
                }
            }

            state.Position += state.Velocity * deltaTime;

            if (state.Velocity != Vector3.zero)
                state.Rotation = Quaternion.LookRotation(state.Velocity);
        }
    }  
}