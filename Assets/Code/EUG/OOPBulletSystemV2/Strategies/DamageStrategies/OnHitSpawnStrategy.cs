using Assets.Code.EUG.AANEW.Temp;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.BulletSystemV2.OOP.UnityBridge;
using Nobeta.EUG.Interfaces;
using System;
using System.Threading;
using UnityEngine;
// ── 命中時生成子彈（連鎖用）────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class OnHitSpawnStrategy : DamageStrategyBase
    {
        private readonly IBulletSystem _bulletSystem;
        private readonly Func<CollisionHit, BulletConfig> _configGetter;
        private readonly int _maxChainDepth;
        private int _currentDepth;

        public override BulletHitType HitType => BulletHitType.Single;

        public OnHitSpawnStrategy(
            IBulletSystem bulletSystem,
            Func<CollisionHit, BulletConfig> configGetter,
            int maxChainDepth = 1)
        {
            _bulletSystem = bulletSystem;
            _configGetter = configGetter;
            _maxChainDepth = maxChainDepth;
        }

        public override UniTaskVoid ExecuteAsync(BulletState state, CollisionHit hit)
        {
            if (_currentDepth >= _maxChainDepth) return default; // ✅ 防無限遞迴

            _currentDepth++;
            var config = _configGetter(hit);
            _bulletSystem.Fire(config, hit.Point, Vector3.forward);
            return default;
        }

        public override IDamageStrategy Clone()
        {
            return new OnHitSpawnStrategy(_bulletSystem, _configGetter, _maxChainDepth);

        }
    }
}
