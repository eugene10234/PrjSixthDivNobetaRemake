using System.Linq;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// HomingMovementStrategy  ── 追尾導彈
//
// 每幀平滑地把速度方向轉向目標，turnSpeed 控制轉向靈敏度。
// 如果目標消失（死亡 / 超出範圍）則退化成直線飛行。
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class AutoHomingMovementStrategy : MovementStrategyBase
    {
        private readonly float _turnSpeed;       // 轉向速度（度/秒）
        private readonly float _maxHomingRange;  // 超過此距離放棄追蹤

        /// <param name="turnSpeed">轉向速度，越大越靈敏（建議 90~360）</param>
        /// <param name="maxHomingRange">放棄追蹤的距離（0 = 永不放棄）</param>
        public AutoHomingMovementStrategy(float turnSpeed = 180f, float maxHomingRange = 0f)
        {
            _turnSpeed = turnSpeed;
            _maxHomingRange = maxHomingRange;
        }

        public override void OnInitialize(BulletState state)
        {
            state.Velocity = state.Direction * state.Speed;
        }

        public override void UpdateMovement(BulletState state, float deltaTime)
        {
            GameObject target = SelectRandomTarget(state.Position);
            if (target != null)
            {
                Vector3 toTarget = target.transform.position - state.Position;
                float dist = toTarget.magnitude;

                // 超出追蹤範圍就退化為直線
                //bool inRange = _maxHomingRange <= 0f || dist <= _maxHomingRange;

                if (true)
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
        private float radius = 100f;
        private LayerMask targetLayer = LayerMask.GetMask("Enemy"); // 建議設置 Layer 以提升效能

        public GameObject SelectRandomTarget(Vector3 position)
        {
            // 1. 取得半徑內所有帶有 Collider 的物件
            Collider[] hitColliders = Physics.OverlapSphere(position, radius, targetLayer);

            // 2. 依照距離排序，並只取前 10 名
            // 使用 Linq 的 OrderBy 根據距離平方排序 (比直接算距離快)
            var targets = hitColliders
                .OrderBy(c => (c.transform.position - position).sqrMagnitude)
                .Take(10)
                .ToArray();

            // 3. 如果有找到目標，則隨機選取一位
            if (targets.Length > 0)
            {
                int randomIndex = Random.Range(0, targets.Length);
                GameObject selectedTarget = targets[randomIndex].gameObject;
                //Debug.Log($"找到 {targets.Length} 個近距離目標，隨機選中: {selectedTarget.name}");
                return selectedTarget;
            }
            else
            {
                //Debug.Log("半徑內沒有目標。");
                return null;
            }
        }
    }
}