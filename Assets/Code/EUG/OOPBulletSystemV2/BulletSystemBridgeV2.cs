using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;

// ============================================================================
// BulletSystemBridge v2  ── 唯一的 MonoBehaviour
//
// 主要變更：
//   - OnBulletHit 接收完整 BulletHitEvent（含 HitType）
//   - PlayHitEffectAsync 改傳 BulletHitEvent（而非 int type）
//   - 新增 FireWithStrategy() 讓外部直接用 config 的 strategy 發射
//   - RegisterIntervalFire / CancelIntervalFire
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2.OOP.UnityBridge
{
    public class BulletSystemBridgeV2 : MonoBehaviour
    {
        [Header("VFX 配置")]
        [SerializeField] private BulletVFXConfig[] bulletVFXConfigs;

        private BulletManager _bulletManager;
        private BulletVisualService _visualService;
        private IEventBus _eventBus;

        [Inject]
        public void Construct(BulletManager bulletManager, IEventBus eventBus)
        {
            _bulletManager = bulletManager;
            _eventBus = eventBus;
        }

        private void Start()
        {
            var container = new GameObject("BulletVisuals").transform;
            container.SetParent(transform);

            _visualService = new BulletVisualService(bulletVFXConfigs, container);

            _eventBus.Subscribe<BulletSpawnedEvent>(OnBulletSpawned);
            _eventBus.Subscribe<BulletHitEvent>(OnBulletHit);
            _eventBus.Subscribe<BulletExpiredEvent>(OnBulletExpired);
        }


        private void Update()
        {
            _bulletManager.Update(Time.deltaTime);
            _visualService.SyncVisualsToLogic(_bulletManager.ActiveBullets);
        }

        private void OnBulletSpawned(BulletSpawnedEvent evt) { /* AssignVisual 在 Sync 自動處理 */ }

        private void OnBulletHit(BulletHitEvent evt)
        {
            // 依 HitType 播放對應特效（核心：不需要另一個 VisualService）
            _visualService.PlayHitEffectAsync(evt).Forget();
            _visualService.RecycleVisual(evt.Bullet); // ✅ O(1)，不需要 foreach
        }

        private void OnBulletExpired(BulletExpiredEvent evt)
        {
            foreach (var b in _bulletManager.ActiveBullets)
            {
                    _visualService.RecycleVisual(b);
            }
        }

        private void OnDestroy()
        {
            _eventBus.Unsubscribe<BulletSpawnedEvent>(OnBulletSpawned);
            _eventBus.Unsubscribe<BulletHitEvent>(OnBulletHit);
            _eventBus.Unsubscribe<BulletExpiredEvent>(OnBulletExpired);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// 依 Config 的 SpawnStrategy 發射（可能一次生成多顆）
        /// </summary>
        public List<Bullet> Fire(BulletConfig config, Vector3 position, Vector3 direction)
            => _bulletManager.SpawnBullet(config, position, direction);

        /// <summary>
        /// 單顆子彈（向後相容）
        /// </summary>
        public Bullet FireSingle(BulletConfig config, Vector3 position, Vector3 direction)
            => _bulletManager.SpawnSingleBullet(config, position, direction);

        /// <summary>
        /// 彈幕（向後相容）
        /// </summary>
        public List<Bullet> FirePattern(BulletConfig config, Vector3 origin, int count, float angleStep)
            => _bulletManager.SpawnPattern(config, origin, count, angleStep);

        public List<Bullet> FireCircleShotgunPattern(
            BulletConfig config, Vector3 origin, Vector3 forwardDirection, int count, float spreadAngle)
            => _bulletManager.SpawnCircleShotgunPattern(config, origin, forwardDirection, count, spreadAngle);

        /// <summary>
        /// 登記定時發射（每 N 秒自動觸發，使用 IntervalSpawnStrategy 的 config）
        /// </summary>
        public int RegisterIntervalFire(
            BulletConfig config,
            System.Func<Vector3> originGetter,
            System.Func<Vector3> forwardGetter)
            => _bulletManager.RegisterIntervalFire(config, originGetter, forwardGetter);

        public void CancelIntervalFire(int handle)
            => _bulletManager.CancelIntervalFire(handle);

        public void ClearAll() => _bulletManager.ClearAll();
    }
}
