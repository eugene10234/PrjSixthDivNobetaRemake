using Assets.Code.EUG.AANEW.Temp;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.BulletSystemV2.OOP.UnityBridge;
using Nobeta.EUG.Interfaces;
using System;
using System.Threading;
using UnityEngine;
// ── 到期時生成子彈（核心爆炸用）──────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public  class OnExpireSpawnStrategy : DamageStrategyBase
    {
        private readonly IBulletSystem _bulletSystem;
        private readonly Func<BulletState, Vector3> _originGetter;
        private readonly Func<BulletState, BulletConfig> _configGetter;

        public override BulletHitType HitType => BulletHitType.Single;


        public OnExpireSpawnStrategy(
            IBulletSystem bulletSystem,
            Func<BulletState, Vector3> originGetter,
            Func<BulletState, BulletConfig> configGetter)
        {
            _bulletSystem = bulletSystem;
            _originGetter = originGetter;
            _configGetter = configGetter;
        }

        public override UniTaskVoid ExecuteAsync(BulletState state, CollisionHit hit)
        {
            var origin = _originGetter(state);
            var config = _configGetter(state);
            // 向四面八方爆炸，forward 不重要
            _bulletSystem.Fire(config, origin, Vector3.forward);
            return default;
        }

        public override IDamageStrategy Clone() => new OnExpireSpawnStrategy(
            _bulletSystem, _originGetter, _configGetter);
    }
}
