using Assets.Code.EUG.AANEW.Temp;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Interfaces;
using System.Threading;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// AoeDamageStrategy  ── 範圍爆炸傷害（手榴彈 / 火球著地）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class AoeDamageStrategy : IDamageStrategy
    {
        private readonly float _radius;
        private readonly float _falloff; // 距離衰減：0 = 無衰減，1 = 線性衰減到邊緣為0

        private static readonly Collider[] _overlapBuffer = new Collider[32];

        /// <param name="radius">爆炸半徑</param>
        /// <param name="falloff">距離衰減係數（0~1）</param>
        public AoeDamageStrategy(float radius, float falloff = 0.5f)
        {
            _radius = radius;
            _falloff = falloff;
        }

        public UniTaskVoid ExecuteAsync(
            BulletState state,
            CollisionHit hit,
            IEventBus eventBus)
        {
            // OverlapSphere 找出範圍內所有 Collider
            int count = Physics.OverlapSphereNonAlloc(
                state.Position, _radius, _overlapBuffer,
                LayerMask.GetMask("Enemy"));

            for (int i = 0; i < count; i++)
            {
                float dist = Vector3.Distance(state.Position, _overlapBuffer[i].transform.position);
                float ratio = 1f - Mathf.Clamp01(dist / _radius * _falloff);
                int dmg = Mathf.RoundToInt(state.Damage * ratio);

                if (EnemyDamageSystem.TryGetTarget(_overlapBuffer[i].gameObject, out IDamageable target))
                    target.TakeDamage(dmg, state.Position);
            }

            // 發布爆炸事件（VFX 監聽此事件播放爆炸特效）
            eventBus.Publish(new BulletHitEvent
            {
                BulletState = state,
                TargetGameObject = hit.GameObject,
                HitPosition = state.Position,
                HitNormal = hit.Normal,
                Damage = state.Damage,
                Type = state.Type,
                HitType = BulletHitType.Aoe,
                AoeRadius = _radius
            });

            return default;
        }
    }
}
