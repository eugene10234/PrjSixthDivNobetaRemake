using Nobeta.EUG.Interfaces;
using System.Collections.Generic;
using UnityEngine;

// ============================================================================
// BulletManager v2
//
// 新增：IntervalSpawnStrategy 的計時管理
//   - 外部呼叫 RegisterIntervalFire() 登記一個持續定時發射的設定
//   - BulletManager.Update() 統一計時，到時間就觸發生成
//   - CancelIntervalFire() 取消
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public class BulletManager
    {
        private const int POOL_CAPACITY = 5000;

        // ── 對象池 + 活動列表 ─────────────────────────────────────────────
        private readonly Stack<Bullet> _bulletPool;
        private readonly List<Bullet> _activeBullets;
        private readonly BulletFactory _bulletFactory;
        private readonly IEventBus _eventBus;

        // ── IntervalFire 登記表 ───────────────────────────────────────────
        private readonly List<IntervalFireEntry> _intervalFires = new();

        public int ActiveBulletCount => _activeBullets.Count;
        public IReadOnlyList<Bullet> ActiveBullets => _activeBullets;

        // ─────────────────────────────────────────────────────────────────

        public BulletManager(BulletFactory bulletFactory, IEventBus eventBus)
        {
            _bulletFactory = bulletFactory;
            _eventBus = eventBus;
            _bulletPool = new Stack<Bullet>(POOL_CAPACITY);
            _activeBullets = new List<Bullet>(POOL_CAPACITY);
            PrewarmPool(50);
        }

        // ── Update ────────────────────────────────────────────────────────

        public void Update(float deltaTime)
        {
            // 1. 更新 IntervalFire 計時器
            UpdateIntervalFires(deltaTime);

            // 2. 更新所有活動子彈
            for (int i = _activeBullets.Count - 1; i >= 0; i--)
            {
                var bullet = _activeBullets[i];
                bullet.Update(deltaTime);

                if (!bullet.IsActive)
                {
                    ReturnToPool(bullet);
                    _activeBullets.RemoveAt(i);
                }
            }
        }

        // ── SpawnBullet（核心生成，支援 SpawnStrategy）───────────────────

        /// <summary>
        /// 根據 Config.SpawnStrategy 一次生成一批子彈
        /// </summary>
        public List<Bullet> SpawnBullet(BulletConfig config, Vector3 origin, Vector3 forward)
        {
            var spawnDatas = config.GetSpawnStrategy().GetSpawnDatas(origin, forward);
            var result = new List<Bullet>(spawnDatas.Count);

            foreach (var data in spawnDatas)
            {
                var bullet = GetFromPool();
                bullet.Initialize(config, data.Position, data.Direction);
                _activeBullets.Add(bullet);

                _eventBus.Publish(new BulletSpawnedEvent
                {
                    BulletState = bullet.State,
                    Position = data.Position,
                    Type = config.Type
                });

                result.Add(bullet);
            }

            return result;
        }

        // ── IntervalFire API ──────────────────────────────────────────────

        /// <summary>
        /// 登記一個定時發射設定（config 的 SpawnStrategy 必須是 IntervalSpawnStrategy）
        /// 回傳 handle，用於 CancelIntervalFire
        /// </summary>
        public int RegisterIntervalFire(
            BulletConfig config,
            System.Func<Vector3> originGetter,
            System.Func<Vector3> forwardGetter)
        {
            if (config.SpawnStrategy is not IntervalSpawnStrategy intervalStrategy)
            {
                Debug.LogWarning("[BulletManager] SpawnStrategy 不是 IntervalSpawnStrategy，請確認設定。");
                return -1;
            }

            var entry = new IntervalFireEntry
            {
                Handle = _nextIntervalHandle++,
                Config = config,
                OriginGetter = originGetter,
                ForwardGetter = forwardGetter,
                Interval = intervalStrategy.Interval,
                Timer = 0f
            };

            _intervalFires.Add(entry);
            return entry.Handle;
        }

        /// <summary>取消定時發射</summary>
        public void CancelIntervalFire(int handle)
        {
            _intervalFires.RemoveAll(e => e.Handle == handle);
        }

        // ── 原有 API（向後相容）──────────────────────────────────────────

        /// <summary>單顆子彈（不走 SpawnStrategy，向後相容）</summary>
        public Bullet SpawnSingleBullet(BulletConfig config, Vector3 position, Vector3 direction)
        {
            var bullet = GetFromPool();
            bullet.Initialize(config, position, direction);
            _activeBullets.Add(bullet);

            _eventBus.Publish(new BulletSpawnedEvent
            {
                BulletState = bullet.State,
                Position = position,
                Type = config.Type
            });

            return bullet;
        }

        public void DestroyBullet(Bullet bullet)
        {
            if (!_activeBullets.Contains(bullet)) return;
            bullet.IsActive = false;
            ReturnToPool(bullet);
        }

        public void ClearAll()
        {
            foreach (var b in _activeBullets)
                ReturnToPool(b);

            _activeBullets.Clear();
            _intervalFires.Clear();
        }

        // ── Batch（保留原有 SpawnPattern / SpawnCircleShotgun，用 Strategy 重實作）

        public List<Bullet> SpawnPattern(BulletConfig config, Vector3 origin, int count, float angleStep)
        {
            // 動態建立 CircleSpawnStrategy，不改原有呼叫方式
            var tempConfig = ShallowCopy(config);
            tempConfig.SpawnStrategy = new CircleSpawnStrategy(count);
            return SpawnBullet(tempConfig, origin, Vector3.forward);
        }

        public List<Bullet> SpawnCircleShotgunPattern(
            BulletConfig config, Vector3 origin, Vector3 forwardDirection, int count, float spreadAngle)
        {
            var tempConfig = ShallowCopy(config);
            tempConfig.SpawnStrategy = new ShotgunSpawnStrategy(count, spreadAngle);
            return SpawnBullet(tempConfig, origin, forwardDirection);
        }

        // ── Private ───────────────────────────────────────────────────────

        private int _nextIntervalHandle = 1;

        private void UpdateIntervalFires(float deltaTime)
        {
            foreach (var entry in _intervalFires)
            {
                entry.Timer += deltaTime;
                if (entry.Timer >= entry.Interval)
                {
                    entry.Timer -= entry.Interval;
                    SpawnBullet(entry.Config, entry.OriginGetter(), entry.ForwardGetter());
                }
            }
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
                _bulletPool.Push(_bulletFactory.Create());
        }

        private Bullet GetFromPool()
            => _bulletPool.Count > 0 ? _bulletPool.Pop() : _bulletFactory.Create();

        private void ReturnToPool(Bullet bullet)
        {
            bullet.Reset();
            _bulletPool.Push(bullet);
        }

        private static BulletConfig ShallowCopy(BulletConfig src)
        {
            // 建立淺複製，避免污染原始 Config
            return new BulletConfig
            {
                Id = src.Id,
                Speed = src.Speed,
                Damage = src.Damage,
                ManaCost = src.ManaCost,
                CoolTime = src.CoolTime,
                Lifetime = src.Lifetime,
                ColliderRadius = src.ColliderRadius,
                Type = src.Type,
                MovementStrategy = src.MovementStrategy,
                DamageStrategy = src.DamageStrategy,
            };
        }

        // ── Inner type ────────────────────────────────────────────────────

        private class IntervalFireEntry
        {
            public int Handle;
            public BulletConfig Config;
            public System.Func<Vector3> OriginGetter;
            public System.Func<Vector3> ForwardGetter;
            public float Interval;
            public float Timer;
        }
    }
}