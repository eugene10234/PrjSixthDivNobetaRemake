using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
    // ========================================================================
    // BulletSystemBridge (Unity 橋接層 - 唯一的 MonoBehaviour)
    // ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{

    /// <summary>
    /// 子彈系統橋接層
    /// 職責：連接 Pure C# 邏輯和 Unity
    /// </summary>
    public class BulletSystemBridge : MonoBehaviour
    {
        #region Inspector Fields

        [Header("視覺預製體")]
        [SerializeField] private GameObject normalBulletPrefab;
        [SerializeField] private GameObject fireBallPrefab;
        [SerializeField] private GameObject iceBallPrefab;
        [SerializeField] private GameObject lightningBallPrefab;

        [Header("除錯")]
        [SerializeField] private bool showDebugUI = true;

        #endregion

        #region Private Fields

        // Pure C# 管理器
        private BulletManager bulletManager;

        // 視覺服務
        private BulletVisualService visualService;

        // 事件總線
        private IEventBus eventBus;

        #endregion

        #region Dependency Injection

        [Inject]
        public void Construct(
            BulletManager bulletManager,
            IEventBus eventBus)
        {
            this.bulletManager = bulletManager;
            this.eventBus = eventBus;
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // 初始化視覺服務
            GameObject[] prefabs = new[]
            {
                normalBulletPrefab,
                fireBallPrefab,
                iceBallPrefab,
                lightningBallPrefab
            };

            var container = new GameObject("BulletVisuals").transform;
            container.SetParent(transform);

            visualService = new BulletVisualService(prefabs, container);

            // 訂閱事件
            eventBus.Subscribe<BulletSpawnedEvent>(OnBulletSpawned);
            eventBus.Subscribe<BulletHitEvent>(OnBulletHit);
            eventBus.Subscribe<BulletExpiredEvent>(OnBulletExpired);

            Debug.Log("[BulletSystem] 初始化完成（Pure C# OOP）");
        }

        private void Update()
        {
            // 更新 Pure C# 邏輯
            bulletManager.Update(Time.deltaTime);

            // 同步視覺
            visualService.SyncVisualsToLogic(bulletManager.ActiveBullets);
        }

        private void OnDestroy()
        {
            // 清理
            bulletManager.ClearAll();

            // 取消訂閱
            eventBus.Unsubscribe<BulletSpawnedEvent>(OnBulletSpawned);
            eventBus.Unsubscribe<BulletHitEvent>(OnBulletHit);
            eventBus.Unsubscribe<BulletExpiredEvent>(OnBulletExpired);
        }

        #endregion

        #region Event Handlers

        private void OnBulletSpawned(BulletSpawnedEvent evt)
        {
            // 視覺服務會在 SyncVisualsToLogic 中處理
        }

        private void OnBulletHit(BulletHitEvent evt)
        {
            // 回收視覺
            visualService.RecycleVisual(evt.Bullet);

            Debug.Log($"子彈擊中：傷害 {evt.Damage}");
        }

        private void OnBulletExpired(BulletExpiredEvent evt)
        {
            // 回收視覺
            visualService.RecycleVisual(evt.Bullet);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 發射子彈
        /// </summary>
        public Bullet Fire(BulletConfig config, Vector3 position, Vector3 direction)
        {
            return bulletManager.SpawnBullet(config, position, direction);
        }

        /// <summary>
        /// 發射彈幕
        /// </summary>
        public List<Bullet> FirePattern(
            BulletConfig config,
            Vector3 origin,
            int count,
            float angleStep)
        {
            return bulletManager.SpawnPattern(config, origin, count, angleStep);
        }

        /// <summary>
        /// 清除所有子彈
        /// </summary>
        public void ClearAll()
        {
            bulletManager.ClearAll();
        }

        #endregion

        #region Debug UI

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Box("=== Pure C# OOP Bullet System ===");

            GUILayout.Label($"<b>Active Bullets:</b> {bulletManager.ActiveBulletCount}");
            GUILayout.Label($"<b>FPS:</b> {1f / Time.deltaTime:F0}");

            GUILayout.Space(10);

            if (GUILayout.Button("Fire Normal"))
            {
                Fire(BulletConfig.Normal, transform.position, transform.forward);
            }

            if (GUILayout.Button("Fire FireBall"))
            {
                Fire(BulletConfig.FireBall, transform.position, transform.forward);
            }

            if (GUILayout.Button("Fire Pattern (16)"))
            {
                FirePattern(BulletConfig.Normal, transform.position, 16, 22.5f);
            }

            if (GUILayout.Button("Clear All"))
            {
                ClearAll();
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}


