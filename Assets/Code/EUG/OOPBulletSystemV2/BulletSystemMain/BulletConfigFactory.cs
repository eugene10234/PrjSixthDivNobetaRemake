using DG.Tweening;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;

// ============================================================================
// BulletConfig v2  ── 加入三個 Strategy 欄位
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class BulletConfigFactory
    {
        private readonly IEventBus _eventBus;

        public BulletConfigFactory(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public BulletConfig Normal() => new BulletConfig
        {
            Speed = 20f,
            Damage = 10,
            Type = BulletType.Normal,
            DamageStrategy = SingleDamageStrategy.Default
        };

        public BulletConfig IceDot() => new BulletConfig
        {
            Speed = 50f,
            Damage = 15,
            Type = BulletType.Ice,
            DamageStrategy = new CompositeDamageStrategy(
                SingleDamageStrategy.Default,
                new DotDamageStrategy(_eventBus, tickCount: 3, tickInterval: 1f, damagePerTick: 8f)
            )
        };
    }
}