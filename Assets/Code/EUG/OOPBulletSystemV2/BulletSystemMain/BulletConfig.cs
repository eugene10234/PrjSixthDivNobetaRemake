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
    /// <summary>
    /// 子彈設定。
    /// v2 新增：SpawnStrategy / MovementStrategy / DamageStrategy 三個可插拔策略。
    /// 保持與原版 100% 向後相容（不填則使用預設行為）。
    /// </summary>
    public class BulletConfig
    {
        // ── 原有欄位（不動）──────────────────────────────────────────────

        public static IEventBus _eventBus;
        public BulletConfig()
        {
            _eventBus = EventBusProvider.GetEventBus();

            //// 預設使用全局事件總線（如果有的話）
            //if (_eventBus == null && EventBusProvider.GetEventBus != null)
            //{
            //    _eventBus = EventBusProvider.GlobalEventBus;
            //}
        }

        public void SetIEventBus(IEventBus eventBus) => _eventBus = eventBus;

        public int Id { get; set; }
        public float Speed { get; set; }
        public int Damage { get; set; }
        public int ManaCost { get; set; }
        public float CoolTime { get; set; }
        public float Lifetime { get; set; }
        public float ColliderRadius { get; set; }
        public BulletType Type { get; set; }
        public int ChantManaCost { get; set; }
        public float ChantDuration { get; set; }
        public bool ChantFlag { get; set; }
        public bool IsChantComplete { get; set; }
        public float ChantRatio { get; set; }
        public float NextFireTime { get; private set; }

        // ── v2 新增：Strategy 插槽（null = 使用預設） ─────────────────────

        /// <summary>決定「發射幾顆、往哪飛」（null → SingleSpawnStrategy）</summary>
        public ISpawnStrategy SpawnStrategy { get; set; }

        /// <summary>決定「每幀怎麼移動」（null → ForwardMovementStrategy）</summary>
        public IMovementStrategy MovementStrategy { get; set; }

        /// <summary>決定「打到目標後怎麼結算傷害」（null → SingleDamageStrategy）</summary>
        public IDamageStrategy DamageStrategy { get; set; }

        // ── Strategy 快速存取器（帶預設值，Bullet 直接使用）────────────────

        public ISpawnStrategy GetSpawnStrategy() => SpawnStrategy ?? SingleSpawnStrategy.Default;
        public IMovementStrategy GetMovementStrategy() => MovementStrategy ?? ForwardMovementStrategy.Default;
        public IDamageStrategy GetDamageStrategy() => DamageStrategy ?? SingleDamageStrategy.Default;

        // ── TryFire / Chant 邏輯（原版不動）─────────────────────────────────

        public void TryFire(Action<PlayerWeaponFireEvent> fireAction)
        {
            if (Time.time < NextFireTime) return;

            if (ChantFlag)
            {
                if (IsChantComplete)
                {
                    fireAction?.Invoke(new PlayerWeaponFireEvent
                    {
                        bulletConfig = this,
                        IsSpecial = true,
                        FinalManaCost = ChantManaCost
                    });
                }
                else
                {
                    _eventBus.Publish(new PlayerWeaponFireEvent
                    {
                        bulletConfig = this,
                        IsSpecial = false,
                        FinalManaCost = ManaCost
                    });
                }

                ChantRatio = 0f;
                IsChantComplete = false;
                ChantFlag = false;
                KillChant(evt => _eventBus.Publish(evt));
            }
            else
            {
                fireAction?.Invoke(new PlayerWeaponFireEvent
                {
                    bulletConfig = this,
                    IsSpecial = false,
                    FinalManaCost = ManaCost
                });
            }

            NextFireTime = Time.time + CoolTime;
        }

        public void TryChant()
        {
            if (ChantFlag) return;
            StartChant(evt => _eventBus.Publish(evt));
        }

        public void StartChant(Action<PlayerChantStartEvent> startAct)
        {
            ChantRatio = 0f;
            ChantFlag = true;
            startAct?.Invoke(new PlayerChantStartEvent { });
            AsyncChantProgess(evt => _eventBus.Publish(evt), evt => _eventBus.Publish(evt));
        }

        public void PauseChant()
        {
            if (DOTween.IsTweening(this)) DOTween.Kill(this);
        }

        public void ContinueChant()
        {
            AsyncChantProgess(evt => _eventBus.Publish(evt), evt => _eventBus.Publish(evt));
        }

        public void KillChant(Action<PlayerChantKillEvent> act)
        {
            PauseChant();
            act.Invoke(new PlayerChantKillEvent { });
            ChantFlag = false;
        }

        private float _lastInvokeTime = 0f;

        private void AsyncChantProgess(
            Action<PlayerChantUpdateEvent> updateAct,
            Action<PlayerChantCompleteEvent> completeAct)
        {
            if (DOTween.IsTweening(this)) DOTween.Kill(this);

            DOTween.To(() => ChantRatio, x => ChantRatio = x, 1f, ChantDuration * (1 - ChantRatio))
                   .SetEase(Ease.Linear)
                   .SetUpdate(UpdateType.Normal, false)
                   .SetId(this)
                   .OnStart(() => { _lastInvokeTime = 0f; })
                   .OnUpdate(() =>
                   {
                       if (Time.time - _lastInvokeTime >= 0.05f)
                       {
                           updateAct?.Invoke(new PlayerChantUpdateEvent { TargetRatio = ChantRatio });
                           _lastInvokeTime = Time.time;
                       }
                   })
                   .OnComplete(() =>
                   {
                       IsChantComplete = true;
                       completeAct?.Invoke(new PlayerChantCompleteEvent { });
                   });
        }

        // ── 預定義配置（v2：加入 Strategy）──────────────────────────────────

        public static BulletConfig Normal => new BulletConfig
        {
            Speed = 35f,
            Damage = 40,
            ManaCost = 0,
            CoolTime = 0.5f,
            Lifetime = 5f,
            ColliderRadius = 0.2f,
            Type = BulletType.Normal,
            NextFireTime = Time.time,
            ChantManaCost = 100,
            ChantDuration = 3f,
            // Strategy：預設單顆直線單體（可省略，此處明確寫出）
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = ForwardMovementStrategy.Default,
            DamageStrategy = SingleDamageStrategy.Default,
        };

        public static BulletConfig IceBall => new BulletConfig
        {
            Speed = 45f,
            Damage = 25,
            ManaCost = 4,
            CoolTime = 0.15f,
            Lifetime = 5f,
            ColliderRadius = 0.15f,
            Type = BulletType.Ice,
            NextFireTime = Time.time,
            ChantManaCost = 20,
            ChantDuration = 5f,
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = ForwardMovementStrategy.Default,
            // 冰矢：單體 + DoT（凍傷每秒8點，持續3秒）
            DamageStrategy = new CompositeDamageStrategy(
                SingleDamageStrategy.Default,
                new DotDamageStrategy(_eventBus,tickCount: 3, tickInterval: 1f, damagePerTick: 8f)),
        };

        public static BulletConfig FireBall => new BulletConfig
        {
            Speed = 15f,
            Damage = 30,
            ManaCost = 40,
            CoolTime = 1f,
            Lifetime = 4f,
            ColliderRadius = 0.3f,
            Type = BulletType.Fire,
            NextFireTime = Time.time,
            ChantManaCost = 20,
            ChantDuration = 5f,
            // 火球：一次5顆散射 + 拋物線 + 範圍爆炸
            SpawnStrategy = new ShotgunSpawnStrategy(pellets: 9, spreadAngle: 16f),
            MovementStrategy = new ParabolicMovementStrategy(gravity: 0f, launchPitchUp: 0f),
            DamageStrategy = new AoeDamageStrategy(radius: 3f, falloff: 0.6f),
        };

        public static BulletConfig LightningStrike => new BulletConfig
        {
            Speed = 80f,
            Damage = 80,
            ManaCost = 50,
            CoolTime = 1.25f,
            Lifetime = 4f,
            ColliderRadius = 0.15f,
            Type = BulletType.Lightning,
            NextFireTime = Time.time,
            ChantManaCost = 20,
            ChantDuration = 5f,
            SpawnStrategy = new BurstSpawnStrategy(count: 3, spreadAngle: 0.2f), 
            MovementStrategy = new SinWaveMovementStrategy(amplitude: 0.35f, frequency: 5f),   // 閃電帶點波動
            DamageStrategy = SingleDamageStrategy.Default,
        };

        public static BulletConfig GrenadeBullet => new BulletConfig
        {
            Speed = 18f,
            Damage = 80,
            ManaCost = 30,
            CoolTime = 2f,
            Lifetime = 3f,
            ColliderRadius = 0.25f,
            Type = BulletType.Normal,
            NextFireTime = Time.time,
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = new ParabolicMovementStrategy(gravity: 9.8f, launchPitchUp: 35f),
            DamageStrategy = new AoeDamageStrategy(radius: 5f, falloff: 0.4f),
        };

        public static BulletConfig BurnBomb => new BulletConfig
        {
            Speed = 12f,
            Damage = 20,
            ManaCost = 35,
            CoolTime = 1.5f,
            Lifetime = 4f,
            ColliderRadius = 0.3f,
            Type = BulletType.Fire,
            NextFireTime = Time.time,
            SpawnStrategy = SingleSpawnStrategy.Default,
            MovementStrategy = new ParabolicMovementStrategy(gravity: 11f, launchPitchUp: 30f),
            // 燃燒彈：範圍 + 持續燃燒 DoT（每0.5秒10點，共10次）
            DamageStrategy = new CompositeDamageStrategy(
                new AoeDamageStrategy(radius: 4f, falloff: 0.3f),
                new DotDamageStrategy(_eventBus, tickCount: 10, tickInterval: 0.5f, damagePerTick: 10f)),
        };
    }

    // ── BulletType（不動）───────────────────────────────────────────────────

    public enum BulletType
    {
        Normal, Ice, Fire, Lightning, SpecialNormal,
    }

    // ── DoT Tick 事件（v2 新增）──────────────────────────────────────────────

    public struct BulletDotTickEvent
    {
        public GameObject TargetGameObject;
        public int Damage;
        public int TickIndex;
        public int TotalTicks;
        public BulletType Type;
    }
}