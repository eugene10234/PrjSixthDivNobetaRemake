// ============================================================================
// Services Implementation & Unity Bridge
// 服務實現和 Unity 橋接層
// ============================================================================

using System.Collections.Generic;
using Nobeta.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.BulletSystem.Unity
{
    // ========================================================================
    // PhysicsService (物理服務實現)
    // ========================================================================

    /// <summary>
    /// 物理服務（封裝 Unity Physics）
    /// </summary>
    public class PhysicsService : IPhysicsService
    {
        public CollisionHit OverlapSphere(Vector3 position, float radius, int layerMask)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, layerMask);

            if (hits.Length > 0)
            {
                return new CollisionHit
                {
                    HasHit = true,
                    GameObject = hits[0].gameObject,
                    Normal = (position - hits[0].transform.position).normalized
                };
            }

            return new CollisionHit { HasHit = false };
        }
    }

    // ========================================================================
    // VisualService (視覺服務)
    // ========================================================================

    /// <summary>
    /// 視覺服務（管理子彈視覺效果）
    /// </summary>
    public class BulletVisualService
    {
        private readonly GameObject[] visualPrefabs;
        private readonly Stack<GameObject>[] visualPools;
        private readonly Transform visualContainer;

        // 活動視覺：Bullet → GameObject
        private readonly Dictionary<Bullet, GameObject> activeVisuals;

        public BulletVisualService(GameObject[] prefabs, Transform container)
        {
            visualPrefabs = prefabs;
            visualContainer = container;

            activeVisuals = new Dictionary<Bullet, GameObject>();

            // 初始化每種類型的池
            visualPools = new Stack<GameObject>[prefabs.Length];
            for (int i = 0; i < prefabs.Length; i++)
            {
                visualPools[i] = new Stack<GameObject>();
                PrewarmVisuals(i, 20);
            }
        }

        private void PrewarmVisuals(int type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var visual = Object.Instantiate(visualPrefabs[type], visualContainer);
                visual.SetActive(false);
                visualPools[type].Push(visual);
            }
        }

        /// <summary>
        /// 為子彈分配視覺
        /// </summary>
        public void AssignVisual(Bullet bullet)
        {
            int type = (int)bullet.Type;

            // 從池獲取
            GameObject visual;
            if (visualPools[type].Count > 0)
            {
                visual = visualPools[type].Pop();
            }
            else
            {
                visual = Object.Instantiate(visualPrefabs[type], visualContainer);
            }

            // 激活並設置位置
            visual.SetActive(true);
            visual.transform.position = bullet.Position;
            visual.transform.rotation = bullet.Rotation;

            // 記錄映射
            activeVisuals[bullet] = visual;
        }

        /// <summary>
        /// 回收視覺
        /// </summary>
        public void RecycleVisual(Bullet bullet)
        {
            if (!activeVisuals.TryGetValue(bullet, out var visual))
                return;

            // 停用
            visual.SetActive(false);

            // 歸還到池
            int type = (int)bullet.Type;
            visualPools[type].Push(visual);

            // 移除映射
            activeVisuals.Remove(bullet);
        }

        /// <summary>
        /// 同步視覺到邏輯
        /// </summary>
        public void SyncVisualsToLogic(IReadOnlyList<Bullet> bullets)
        {
            foreach (var bullet in bullets)
            {
                if (!activeVisuals.TryGetValue(bullet, out var visual))
                {
                    // 新子彈，分配視覺
                    AssignVisual(bullet);
                    continue;
                }

                // 同步位置和旋轉
                visual.transform.position = bullet.Position;
                visual.transform.rotation = bullet.Rotation;
            }
        }
    }

    // ========================================================================
    // BulletSystemBridge (Unity 橋接層 - 唯一的 MonoBehaviour)
    // ========================================================================

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

    // ========================================================================
    // VContainer Scope Configuration
    // ========================================================================

    /// <summary>
    /// 子彈系統 Lifetime Scope
    /// </summary>
    public class BulletSystemScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 註冊 EventBus（如果還沒註冊）
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // 註冊服務
            builder.Register<IPhysicsService, PhysicsService>(Lifetime.Singleton);

            // 註冊工廠
            builder.Register<BulletFactory>(Lifetime.Singleton);

            // 註冊管理器
            builder.Register<BulletManager>(Lifetime.Singleton);

            // 註冊 Bullet（Transient - 每次創建新的）
            builder.Register<Bullet>(Lifetime.Transient);

            Debug.Log("[BulletSystem] VContainer 註冊完成");
        }
    }

    // ========================================================================
    // Usage Example (使用示例)
    // ========================================================================

    /// <summary>
    /// 武器系統（使用示例）
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] private BulletSystemBridge bulletSystem;
        [SerializeField] private Transform firePoint;

        [Inject] private IPlayerInputService inputService;

        private void Start()
        {
            inputService.OnFireStart += Fire;
        }

        private void OnDestroy()
        {
            inputService.OnFireStart -= Fire;
        }

        private void Fire()
        {
            bulletSystem.Fire(
                BulletConfig.Normal,
                firePoint.position,
                firePoint.forward
            );
        }
    }
}


