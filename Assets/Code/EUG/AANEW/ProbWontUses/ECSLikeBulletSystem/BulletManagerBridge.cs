// ============================================================================
// Pure C# Bullet Manager System - 完整實現（第2部分）
// BulletManager 核心管理器
// ============================================================================

using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;  // 僅用於 Vector3, Quaternion 等數學類型

namespace Nobeta.EUG.BulletSystemV1.ECSLike.Pure
{
    /// <summary>
    /// 子彈管理器橋接層（唯一的 MonoBehaviour）
    /// 
    /// 職責：
    /// - 將 Pure C# 邏輯連接到 Unity
    /// - 提供物理檢測回調
    /// - 同步視覺效果
    /// - 處理 Unity 生命週期
    /// </summary>
    public class BulletManagerBridge : MonoBehaviour
    {
        #region Inspector Fields

        [Header("視覺預製體")]
        [SerializeField] private GameObject[] bulletVisualPrefabs;

        [Header("對象池設置")]
        [SerializeField] private int visualPoolSize = 100;

        [Header("除錯")]
        [SerializeField] private bool showDebugInfo = true;

        #endregion

        #region Private Fields

        // Pure C# 管理器
        private BulletManager bulletManager;

        // Unity 視覺對象池
        private GameObject[][] visualPools;
        private Transform[] visualContainers;

        // 事件總線
        private IEventBus eventBus;

        #endregion

        #region Dependency Injection

        [Inject]
        public void Construct(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 創建 Pure C# 管理器
            bulletManager = new BulletManager(eventBus);

            // 設置物理回調
            bulletManager.SetPhysicsCallback(PhysicsCheck);

            // 初始化視覺池
            InitializeVisualPools();

            Debug.Log($"[BulletManager] 初始化完成，最大容量: {bulletManager.MaxBullets}");
        }

        private void Update()
        {
            // 更新 Pure C# 邏輯
            bulletManager.Update(Time.deltaTime);

            // 同步視覺到數據
            SyncVisualsToData();
        }

        private void OnDestroy()
        {
            // 清理
            bulletManager.ClearAllBullets();
        }

        #endregion

        #region Visual Pool Initialization

        private void InitializeVisualPools()
        {
            int visualTypeCount = bulletVisualPrefabs.Length;

            visualPools = new GameObject[visualTypeCount][];
            visualContainers = new Transform[visualTypeCount];

            for (int type = 0; type < visualTypeCount; type++)
            {
                // 創建容器
                var container = new GameObject($"BulletVisuals_Type{type}").transform;
                container.SetParent(transform);
                visualContainers[type] = container;

                // 創建池
                visualPools[type] = new GameObject[visualPoolSize];

                for (int i = 0; i < visualPoolSize; i++)
                {
                    var visual = Instantiate(bulletVisualPrefabs[type], container);
                    visual.SetActive(false);
                    visualPools[type][i] = visual;
                }
            }

            Debug.Log($"[BulletManager] 視覺池初始化完成，類型數: {visualTypeCount}，每類容量: {visualPoolSize}");
        }

        #endregion

        #region Visual Synchronization

        /// <summary>
        /// 同步視覺到數據
        /// </summary>
        private void SyncVisualsToData()
        {
            var bullets = bulletManager.GetActiveBullets();

            // 首先停用所有視覺
            DeactivateAllVisuals();

            // 激活並同步活動子彈的視覺
            for (int i = 0; i < bulletManager.MaxBullets; i++)
            {
                if (!bullets[i].isActive)
                    continue;

                // 獲取視覺對象
                int visualType = bullets[i].visualIndex;

                if (visualType < 0 || visualType >= visualPools.Length)
                    continue;

                // 找到可用的視覺對象（簡化：使用 ID 映射）
                int visualIndex = i % visualPoolSize;
                var visual = visualPools[visualType][visualIndex];

                // 激活並設置位置/旋轉
                visual.SetActive(true);
                visual.transform.position = bullets[i].position;
                visual.transform.rotation = bullets[i].rotation;
            }
        }

        private void DeactivateAllVisuals()
        {
            for (int type = 0; type < visualPools.Length; type++)
            {
                for (int i = 0; i < visualPoolSize; i++)
                {
                    visualPools[type][i].SetActive(false);
                }
            }
        }

        #endregion

        #region Physics Callback

        /// <summary>
        /// 物理檢測回調（由 Pure C# 調用）
        /// </summary>
        private CollisionResult PhysicsCheck(Vector3 position, float radius, int layerMask)
        {
            // 使用 Unity Physics 進行檢測
            Collider[] hits = Physics.OverlapSphere(position, radius, layerMask);

            if (hits.Length > 0)
            {
                var hit = hits[0];

                return new CollisionResult
                {
                    hasHit = true,
                    targetId = hit.gameObject.GetInstanceID(),
                    hitPosition = position,
                    hitNormal = (position - hit.transform.position).normalized
                };
            }

            return new CollisionResult { hasHit = false };
        }

        #endregion

        #region Public API（暴露給外部使用）

        /// <summary>
        /// 發射子彈
        /// </summary>
        public int Fire(BulletConfig config, Vector3 position, Vector3 direction)
        {
            return bulletManager.SpawnBullet(config, position, direction);
        }

        /// <summary>
        /// 發射彈幕
        /// </summary>
        public int[] FirePattern(BulletConfig config, Vector3 origin, int count, float angleStep)
        {
            return bulletManager.SpawnBulletPattern(config, origin, count, angleStep);
        }

        /// <summary>
        /// 清除所有子彈
        /// </summary>
        public void ClearAll()
        {
            bulletManager.ClearAllBullets();
        }

        /// <summary>
        /// 獲取統計信息
        /// </summary>
        public BulletManagerStats GetStats()
        {
            return bulletManager.GetStatistics();
        }

        #endregion

        #region Debug GUI

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            var stats = bulletManager.GetStatistics();

            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Box("=== Bullet Manager (Pure C#) ===");

            GUILayout.Label($"<b>Active Bullets:</b> {stats.ActiveCount}/{stats.MaxCapacity}");
            GUILayout.Label($"<b>Utilization:</b> {stats.UtilizationRate:P1}");
            GUILayout.Label($"<b>FPS:</b> {1f / Time.deltaTime:F0}");

            GUILayout.Space(10);

            if (GUILayout.Button("Fire Normal"))
            {
                Fire(BulletManagerConfig.NormalBullet, transform.position, transform.forward);
            }

            if (GUILayout.Button("Fire Pattern (Circle)"))
            {
                FirePattern(BulletManagerConfig.FireBall, transform.position, 16, 22.5f);
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