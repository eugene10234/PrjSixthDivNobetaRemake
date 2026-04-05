using Assets.Code.EUG.AANEW.Temp;
using Cysharp.Threading.Tasks;

using Nobeta.EUG.Interfaces;
using System.Threading;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// SingleDamageStrategy  ── 單體傷害（原有行為）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class SingleDamageStrategy : IDamageStrategy
    {
        public static readonly SingleDamageStrategy Default = new();

        public UniTaskVoid ExecuteAsync(
            BulletState state,
            CollisionHit hit,
            IEventBus eventBus)
        {
            // 取得 IDamageable 並套用傷害
            if (EnemyDamageSystem.TryGetTarget(hit.GameObject, out IDamageable target))
                target.TakeDamage(state.Damage, state.Position);

            // 通知 VFX / Audio（由 BulletVisualService 訂閱 BulletHitEvent）
            eventBus.Publish(new BulletHitEvent
            {
                BulletState = state,
                TargetGameObject = hit.GameObject,
                HitPosition = state.Position,
                HitNormal = hit.Normal,
                Damage = state.Damage,
                Type = state.Type,
                HitType = BulletHitType.Single
            });

            return default;
        }
    }
}
