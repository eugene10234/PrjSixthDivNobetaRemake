// ============================================================================
// Pure C# Bullet Manager System - 完整實現（第1部分）
// 完全脫離 MonoBehaviour，SOLID + ECS-Like 設計
// ============================================================================

using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;  // 僅用於 Vector3, Quaternion 等數學類型

namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{
    // ========================================================================
    // Bullet Manager（子彈管理器 - Pure C#，核心）
    // ========================================================================

    /// <summary>
    /// 子彈管理器（Pure C#，完全脫離 MonoBehaviour）
    /// 
    /// 職責：
    /// - 管理所有子彈數據
    /// - 協調所有系統
    /// - 提供公共 API
    /// - 管理對象池
    /// </summary>
    public class BulletManager
    {
        #region Constants

        private const int MAX_BULLETS = 2000;  // 最大子彈數量

        #endregion

        #region Private Fields

        // 數據存儲（Array of Structs）
        private readonly BulletData[] bullets;

        // ID 池
        private readonly IdPool idPool;

        // 工廠
        private readonly BulletFactory bulletFactory;

        // 系統列表
        private readonly List<ISystem> systems;

        // 事件總線
        private readonly IEventBus eventBus;

        // 活動子彈計數
        private int activeBulletCount = 0;

        // 物理檢測回調（由 Unity 橋接層提供）
        private Func<Vector3, float, int, CollisionResult> physicsCheckCallback;

        #endregion

        #region Properties

        public int ActiveBulletCount => activeBulletCount;
        public int MaxBullets => MAX_BULLETS;

        #endregion

        #region Constructor

        public BulletManager(IEventBus eventBus)
        {
            this.eventBus = eventBus;

            // 初始化數據數組（預分配，零 GC）
            bullets = new BulletData[MAX_BULLETS];

            // 初始化 ID 池
            idPool = new IdPool(MAX_BULLETS);

            // 初始化工廠
            bulletFactory = new BulletFactory();

            // 初始化系統列表
            systems = new List<ISystem>();

            // 註冊系統
            RegisterSystems();
        }

        private void RegisterSystems()
        {
            // 移動系統
            systems.Add(new BulletMovementSystem(bullets, MAX_BULLETS));

            // 生命週期系統
            systems.Add(new BulletLifetimeSystem(
                bullets,
                MAX_BULLETS,
                OnBulletExpired
            ));

            // 碰撞系統會在設置物理回調後添加
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 設置物理檢測回調（由 Unity 橋接層調用）
        /// </summary>
        public void SetPhysicsCallback(Func<Vector3, float, int, CollisionResult> callback)
        {
            physicsCheckCallback = callback;

            // 添加碰撞系統
            systems.Add(new BulletCollisionSystem(
                bullets,
                MAX_BULLETS,
                eventBus,
                physicsCheckCallback
            ));
        }

        #endregion

        #region Update

        /// <summary>
        /// 更新所有系統（每幀調用）
        /// </summary>
        public void Update(float deltaTime)
        {
            // 更新活動子彈計數
            UpdateActiveBulletCount();

            // 更新所有系統
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Update(deltaTime);
            }
        }

        private void UpdateActiveBulletCount()
        {
            int count = 0;
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                if (bullets[i].isActive)
                    count++;
            }
            activeBulletCount = count;
        }

        #endregion

        #region Bullet Lifecycle

        /// <summary>
        /// 生成子彈
        /// </summary>
        public int SpawnBullet(BulletConfig config, Vector3 position, Vector3 direction)
        {
            // 從 ID 池獲取索引
            int index = idPool.Get();

            // 使用工廠創建子彈數據
            bullets[index] = bulletFactory.CreateBullet(config, position, direction);

            // 發布生成事件
            eventBus.Publish(new BulletSpawnedEvent
            {
                bulletId = bullets[index].id,
                position = position,
                type = config.type
            });

            return bullets[index].id;
        }

        /// <summary>
        /// 銷毀子彈
        /// </summary>
        public void DestroyBullet(int bulletId)
        {
            // 查找子彈
            int index = FindBulletIndex(bulletId);
            if (index == -1)
                return;

            // 發布銷毀事件
            eventBus.Publish(new BulletDestroyedEvent
            {
                bulletId = bulletId,
                position = bullets[index].position,
                type = bullets[index].type
            });

            // 標記為非活動
            bullets[index].isActive = false;

            // 歸還 ID
            idPool.Return(index);
        }

        /// <summary>
        /// 子彈過期回調
        /// </summary>
        private void OnBulletExpired(int bulletId)
        {
            DestroyBullet(bulletId);
        }

        #endregion

        #region Query

        /// <summary>
        /// 獲取所有活動子彈（用於視覺同步）
        /// </summary>
        public BulletData[] GetActiveBullets()
        {
            return bullets;  // 返回整個數組，外部自行檢查 isActive
        }

        /// <summary>
        /// 獲取單個子彈數據
        /// </summary>
        public bool TryGetBullet(int bulletId, out BulletData bulletData)
        {
            int index = FindBulletIndex(bulletId);
            if (index == -1)
            {
                bulletData = default;
                return false;
            }

            bulletData = bullets[index];
            return true;
        }

        /// <summary>
        /// 查找子彈索引
        /// </summary>
        private int FindBulletIndex(int bulletId)
        {
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                if (bullets[i].isActive && bullets[i].id == bulletId)
                    return i;
            }
            return -1;
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// 批量生成子彈（彈幕）
        /// </summary>
        public int[] SpawnBulletPattern(
            BulletConfig config,
            Vector3 origin,
            int count,
            float angleStep)
        {
            int[] bulletIds = new int[count];

            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                bulletIds[i] = SpawnBullet(config, origin, direction);
            }

            return bulletIds;
        }

        /// <summary>
        /// 清除所有子彈
        /// </summary>
        public void ClearAllBullets()
        {
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                if (bullets[i].isActive)
                {
                    DestroyBullet(bullets[i].id);
                }
            }

            idPool.Clear();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// 獲取統計信息
        /// </summary>
        public BulletManagerStats GetStatistics()
        {
            return new BulletManagerStats
            {
                ActiveCount = activeBulletCount,
                MaxCapacity = MAX_BULLETS,
                UtilizationRate = (float)activeBulletCount / MAX_BULLETS
            };
        }

        #endregion
    }

    // ========================================================================
    // Supporting Structures（輔助結構）
    // ========================================================================

    /// <summary>
    /// 統計信息
    /// </summary>
    public struct BulletManagerStats
    {
        public int ActiveCount;
        public int MaxCapacity;
        public float UtilizationRate;
    }

    // ========================================================================
    // Bullet Manager Config（配置）
    // ========================================================================

    /// <summary>
    /// 子彈管理器配置
    /// </summary>
    public class BulletManagerConfig
    {
        // 預定義的子彈配置
        public static readonly BulletConfig NormalBullet = new BulletConfig
        {
            speed = 20f,
            damage = 10,
            lifetime = 5f,
            colliderRadius = 0.1f,
            type = BulletType.Normal,
            visualIndex = 0
        };

        public static readonly BulletConfig FireBall = new BulletConfig
        {
            speed = 15f,
            damage = 20,
            lifetime = 4f,
            colliderRadius = 0.2f,
            type = BulletType.Fire,
            visualIndex = 1
        };

        public static readonly BulletConfig IceBall = new BulletConfig
        {
            speed = 12f,
            damage = 15,
            lifetime = 4f,
            colliderRadius = 0.2f,
            type = BulletType.Ice,
            visualIndex = 2
        };

        public static readonly BulletConfig LightningBall = new BulletConfig
        {
            speed = 25f,
            damage = 30,
            lifetime = 3f,
            colliderRadius = 0.15f,
            type = BulletType.Lightning,
            visualIndex = 3
        };
    }
}