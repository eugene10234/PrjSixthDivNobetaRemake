//using System.Collections.Generic;
//using Nobeta.EUG.BulletSystemV1.OOP;
////using Nobeta.EUG.Core.Events;
//using Nobeta.EUG.Interfaces;
//using Nobeta.EUG.Services;
//using UnityEngine;
//using VContainer;
//using VContainer.Unity;
//    // ========================================================================
//    // BulletSystemBridge (Unity 橋接層 - 唯一的 MonoBehaviour)
//    // ========================================================================
//namespace Nobeta.EUG.BulletSystemV1.OOP.UnityBridge
//{
//    using Nobeta.EUG.BulletSystemV1.OOP;
//    using Nobeta.EUG.BulletSystemV1.OOP.UnityBridge;
//    using Nobeta.EUG.Interfaces;
//    using UnityEngine;
//    using VContainer;

//    public class BulletSystemBridge : MonoBehaviour
//    {
//        [Header("VFX 配置")]
//        [SerializeField] private BulletVFXConfig[] bulletVFXConfigs;

//        private BulletManager bulletManager;
//        private BulletVisualService visualService;
//        private IEventBus eventBus;

//        [Inject]
//        public void Construct(BulletManager bulletManager, IEventBus eventBus)
//        {
//            this.bulletManager = bulletManager;
//            this.eventBus = eventBus;
//        }

//        private void Start()
//        {
//            // 創建視覺容器
//            var container = new GameObject("BulletVisuals").transform;
//            container.SetParent(transform);

//            // 創建完整的 VFX 服務
//            visualService = new BulletVisualService(bulletVFXConfigs, container);

//            // 訂閱事件
//            eventBus.Subscribe<BulletSpawnedEvent>(OnBulletSpawned);
//            eventBus.Subscribe<BulletHitEvent>(OnBulletHit);
//            eventBus.Subscribe<BulletExpiredEvent>(OnBulletExpired);
//        }

//        private void Update()
//        {
//            // 更新邏輯
//            bulletManager.Update(Time.deltaTime);

//            // 同步視覺
//            visualService.SyncVisualsToLogic(bulletManager.ActiveBullets);
//        }

//        private void OnBulletSpawned(BulletSpawnedEvent evt)
//        {
//            // AssignVisual 會自動播放槍口閃光
//        }

//        private void OnBulletHit(BulletHitEvent evt)
//        {
//            // 播放擊中特效
//            int type = (int)evt.Type;
//            visualService.PlayHitEffectAsync(type, evt.HitPosition, evt.HitNormal).Forget();

//            // 回收視覺（包含拖尾延遲回收）
//            visualService.RecycleVisual(evt.Bullet);
//        }

//        private void OnBulletExpired(BulletExpiredEvent evt)
//        {
//            // 回收視覺
//            visualService.RecycleVisual(evt.Bullet);
//        }

//        private void OnDestroy()
//        {
//            eventBus.Unsubscribe<BulletSpawnedEvent>(OnBulletSpawned);
//            eventBus.Unsubscribe<BulletHitEvent>(OnBulletHit);
//            eventBus.Unsubscribe<BulletExpiredEvent>(OnBulletExpired);
//        }
//        #region Public API

//        /// <summary>
//        /// 發射子彈
//        /// </summary>
//        public Bullet Fire(BulletConfig config, Vector3 position, Vector3 direction)
//        {
//            return bulletManager.SpawnBullet(config, position, direction);
//        }

//        /// <summary>
//        /// 發射彈幕
//        /// </summary>
//        public List<Bullet> FirePattern(
//            BulletConfig config,
//            Vector3 origin,
//            int count,
//            float angleStep)
//        {
//            return bulletManager.SpawnPattern(config, origin, count, angleStep);
//        }
//        public List<Bullet> FireCircleShotgunPattern(
//            BulletConfig config,
//            Vector3 origin,
//            Vector3 forwardDirection,
//            int count,
//            float spreadAngle)
//        {
//            return bulletManager.SpawnCircleShotgunPattern(config, origin, forwardDirection, count, spreadAngle);
//        }
//        /// <summary>
//        /// 清除所有子彈
//        /// </summary>
//        public void ClearAll()
//        {
//            bulletManager.ClearAll();
//        }

//        #endregion
//    }
//}


