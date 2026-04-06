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
            Speed = 35f,
            Damage = 40,
            ManaCost = 0,
            CoolTime = 0.5f,
            Lifetime = 5f,
            ColliderRadius = 0.2f,
            Type = BulletType.Normal,
            ChantManaCost = 100,
            ChantDuration = 3f,
            // Strategy：預設單顆直線單體（可省略，此處明確寫出）
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = ForwardMovementStrategy.Default,
            DamageStrategy = SingleDamageStrategy.Default,
        };

        public BulletConfig IceBall() => new BulletConfig
        {
            Speed = 45f,
            Damage = 25,
            ManaCost = 4,
            CoolTime = 0.15f,
            Lifetime = 5f,
            ColliderRadius = 0.15f,
            Type = BulletType.Ice,
            ChantManaCost = 20,
            ChantDuration = 5f,
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = ForwardMovementStrategy.Default,
            // 冰矢：單體 + DoT（凍傷每秒8點，持續3秒）
            DamageStrategy = new CompositeDamageStrategy(
                            SingleDamageStrategy.Default,
                            new DotDamageStrategy(_eventBus, tickCount: 3, tickInterval: 1f, damagePerTick: 8f)),
        };
        public BulletConfig FireBall() => new BulletConfig
        {
            Speed = 15f,
            Damage = 30,
            ManaCost = 40,
            CoolTime = 1f,
            Lifetime = 4f,
            ColliderRadius = 0.3f,
            Type = BulletType.Fire,
            ChantManaCost = 20,
            ChantDuration = 5f,
            // 火球：一次5顆散射 + 拋物線 + 範圍爆炸
            SpawnStrategy = new ShotgunSpawnStrategy(pellets: 9, spreadAngle: 16f),
            MovementStrategy = new ParabolicMovementStrategy(gravity: 0f, launchPitchUp: 0f),
            DamageStrategy = new AoeDamageStrategy(radius: 3f, falloff: 0.6f),
        };
        public BulletConfig LightningStrike() => new BulletConfig
        {
            Speed = 80f,
            Damage = 80,
            ManaCost = 50,
            CoolTime = 1.25f,
            Lifetime = 4f,
            ColliderRadius = 0.15f,
            Type = BulletType.Lightning,
            ChantManaCost = 20,
            ChantDuration = 5f,
            SpawnStrategy = new BurstSpawnStrategy(count: 3, spreadAngle: 0.2f),
            MovementStrategy = new SinWaveMovementStrategy(amplitude: 0.35f, frequency: 5f),   // 閃電帶點波動
            DamageStrategy = SingleDamageStrategy.Default,
        };
        public BulletConfig GrenadeBullet() => new BulletConfig
        {
            Speed = 18f,
            Damage = 80,
            ManaCost = 30,
            CoolTime = 2f,
            Lifetime = 3f,
            ColliderRadius = 0.25f,
            Type = BulletType.Normal,
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = new ParabolicMovementStrategy(gravity: 9.8f, launchPitchUp: 35f),
            DamageStrategy = new AoeDamageStrategy(radius: 5f, falloff: 0.4f),
        };
        public BulletConfig BurnBomb() => new BulletConfig
        {
            Speed = 12f,
            Damage = 20,
            ManaCost = 35,
            CoolTime = 1.5f,
            Lifetime = 4f,
            ColliderRadius = 0.3f,
            Type = BulletType.Fire,
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = new ParabolicMovementStrategy(gravity: 11f, launchPitchUp: 30f),
            // 燃燒彈：範圍 + 持續燃燒 DoT（每0.5秒10點，共10次）
            DamageStrategy = new CompositeDamageStrategy(
                new AoeDamageStrategy(radius: 4f, falloff: 0.3f),
                new DotDamageStrategy(_eventBus, tickCount: 10, tickInterval: 0.5f, damagePerTick: 10f)),
        };
    }
    public enum WeaponConfigType
    {
        Normal,
        IceBall,
        FireBall,
        BurnBomb,       // ✅ 獨立，不跟 BulletType 衝突
        LightningStrike,
    }
}