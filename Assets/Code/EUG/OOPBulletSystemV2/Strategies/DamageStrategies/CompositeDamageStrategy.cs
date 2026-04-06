using Assets.Code.EUG.AANEW.Temp;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Interfaces;
using System.Threading;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// CompositeDamageStrategy  ── 組合策略（先打單體，再觸發 DoT）
//   例：冰矢 = SingleDamage + DoT(減速)
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{ 
    public sealed class CompositeDamageStrategy : DamageStrategyBase
    {
        public override BulletHitType HitType => BulletHitType.Composite;
        private readonly IDamageStrategy[] _strategies;

        public CompositeDamageStrategy(params IDamageStrategy[] strategies)
        {
            _strategies = strategies;
        }

        public override UniTaskVoid ExecuteAsync(
            BulletState state,
            CollisionHit hit)
        {
            RunAllAsync(state, hit).Forget();
            return default;
        }

        private async UniTaskVoid RunAllAsync(
            BulletState state,
            CollisionHit hit)
        {
            foreach (var strategy in _strategies)
                strategy.ExecuteAsync(state, hit);

            await UniTask.CompletedTask;
        }
    }
}
