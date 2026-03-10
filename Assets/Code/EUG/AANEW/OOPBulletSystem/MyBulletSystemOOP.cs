// ============================================================================
// Pure C# OOP Bullet System - 完整實現
// 團隊友好版：熟悉的 OOP + VContainer + EventBus + ObjectPool
// ============================================================================

using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;

namespace Nobeta.BulletSystem.OOP
{
    // ========================================================================
    // Bullet (Pure C# Class - 核心子彈類)
    // ========================================================================

    /// <summary>
    /// 子彈類（Pure C#，完全可測試）
    /// 
    /// 職責：
    /// - 管理自身狀態（位置、速度、生命週期）
    /// - 處理移動邏輯
    /// - 觸發碰撞事件
    /// </summary>
    public class Bullet
    {
        #region Properties

        // 基礎屬性
        public int Id { get; private set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Velocity { get; set; }

        // 戰鬥屬性
        public int Damage { get; private set; }
        public BulletType Type { get; private set; }

        // 生命週期
        public float Lifetime { get; private set; }
        public bool IsActive { get; set; }

        // 碰撞
        public float ColliderRadius { get; private set; }

        #endregion

        #region Dependencies (通過構造函數注入)

        private readonly IEventBus eventBus;
        private readonly IPhysicsService physicsService;

        #endregion

        #region Constructor

        public Bullet(IEventBus eventBus, IPhysicsService physicsService)
        {
            this.eventBus = eventBus;
            this.physicsService = physicsService;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化子彈（從池中取出時調用）
        /// </summary>
        public void Initialize(BulletConfig config, Vector3 position, Vector3 direction)
        {
            Id = config.Id;
            Position = position;
            Velocity = direction.normalized * config.Speed;
            Rotation = Quaternion.LookRotation(direction);
            Damage = config.Damage;
            Type = config.Type;
            Lifetime = config.Lifetime;
            ColliderRadius = config.ColliderRadius;
            IsActive = true;
        }

        #endregion

        #region Update Logic

        /// <summary>
        /// 更新邏輯（每幀調用）
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            // 更新生命週期
            UpdateLifetime(deltaTime);

            // 更新移動
            UpdateMovement(deltaTime);

            // 檢查碰撞
            CheckCollision();
        }

        private void UpdateLifetime(float deltaTime)
        {
            Lifetime -= deltaTime;

            if (Lifetime <= 0)
            {
                OnLifetimeExpired();
            }
        }

        private void UpdateMovement(float deltaTime)
        {
            // 更新位置
            Position += Velocity * deltaTime;

            // 更新旋轉（朝向移動方向）
            if (Velocity != Vector3.zero)
            {
                Rotation = Quaternion.LookRotation(Velocity);
            }
        }

        private void CheckCollision()
        {
            // 委託給物理服務檢測
            var hit = physicsService.OverlapSphere(
                Position,
                ColliderRadius,
                LayerMask.GetMask("Enemy")
            );

            if (hit.HasHit)
            {
                OnCollision(hit);
            }
        }

        #endregion

        #region Event Handlers

        private void OnLifetimeExpired()
        {
            // 發布過期事件
            eventBus.Publish(new BulletExpiredEvent
            {
                Bullet = this,
                Position = Position
            });

            IsActive = false;
        }

        private void OnCollision(CollisionHit hit)
        {
            // 發布碰撞事件
            eventBus.Publish(new BulletHitEvent
            {
                Bullet = this,
                TargetGameObject = hit.GameObject,
                HitPosition = Position,
                HitNormal = hit.Normal,
                Damage = Damage,
                Type = Type
            });

            IsActive = false;
        }

        #endregion

        #region Reset (用於對象池)

        /// <summary>
        /// 重置狀態（歸還到池時調用）
        /// </summary>
        public void Reset()
        {
            Id = 0;
            Position = Vector3.zero;
            Velocity = Vector3.zero;
            Rotation = Quaternion.identity;
            IsActive = false;
        }

        #endregion
    }

    // ========================================================================
    // BulletConfig (配置類)
    // ========================================================================

    /// <summary>
    /// 子彈配置
    /// </summary>
    public class BulletConfig
    {
        public int Id { get; set; }
        public float Speed { get; set; }
        public int Damage { get; set; }
        public float Lifetime { get; set; }
        public float ColliderRadius { get; set; }
        public BulletType Type { get; set; }

        // 預定義配置
        public static BulletConfig Normal => new BulletConfig
        {
            Speed = 20f,
            Damage = 10,
            Lifetime = 5f,
            ColliderRadius = 0.1f,
            Type = BulletType.Normal
        };

        public static BulletConfig FireBall => new BulletConfig
        {
            Speed = 15f,
            Damage = 20,
            Lifetime = 4f,
            ColliderRadius = 0.2f,
            Type = BulletType.Fire
        };
    }

    public enum BulletType
    {
        Normal,
        Fire,
        Ice,
        Lightning
    }

    // ========================================================================
    // BulletFactory (工廠類 - Pure C#)
    // ========================================================================

    /// <summary>
    /// 子彈工廠
    /// 職責：創建和配置子彈
    /// </summary>
    public class BulletFactory
    {
        private readonly IObjectResolver container;
        private int nextId = 0;

        public BulletFactory(IObjectResolver container)
        {
            this.container = container;
        }

        /// <summary>
        /// 創建子彈（從 VContainer 解析）
        /// </summary>
        public Bullet Create()
        {
            return container.Resolve<Bullet>();
        }

        /// <summary>
        /// 創建並初始化子彈
        /// </summary>
        public Bullet CreateAndInitialize(
            BulletConfig config,
            Vector3 position,
            Vector3 direction)
        {
            var bullet = Create();

            config.Id = nextId++;
            bullet.Initialize(config, position, direction);

            return bullet;
        }
    }

    // ========================================================================
    // BulletManager (管理器 - Pure C#)
    // ========================================================================

    /// <summary>
    /// 子彈管理器（Pure C#）
    /// 
    /// 職責：
    /// - 管理所有活動子彈
    /// - 管理對象池
    /// - 協調更新
    /// - 提供公共 API
    /// </summary>
    public class BulletManager
    {
        #region Constants

        private const int POOL_CAPACITY = 500;

        #endregion

        #region Private Fields

        // 對象池
        private readonly Stack<Bullet> bulletPool;

        // 活動子彈列表
        private readonly List<Bullet> activeBullets;

        // 工廠
        private readonly BulletFactory bulletFactory;

        // 事件總線
        private readonly IEventBus eventBus;

        #endregion

        #region Properties

        public int ActiveBulletCount => activeBullets.Count;
        public IReadOnlyList<Bullet> ActiveBullets => activeBullets;

        #endregion

        #region Constructor

        public BulletManager(
            BulletFactory bulletFactory,
            IEventBus eventBus)
        {
            this.bulletFactory = bulletFactory;
            this.eventBus = eventBus;

            bulletPool = new Stack<Bullet>(POOL_CAPACITY);
            activeBullets = new List<Bullet>(POOL_CAPACITY);

            // 預熱池
            PrewarmPool(50);
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var bullet = bulletFactory.Create();
                bulletPool.Push(bullet);
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// 更新所有活動子彈
        /// </summary>
        public void Update(float deltaTime)
        {
            // 反向遍歷（可以安全移除）
            for (int i = activeBullets.Count - 1; i >= 0; i--)
            {
                var bullet = activeBullets[i];

                // 更新子彈
                bullet.Update(deltaTime);

                // 如果非活動，歸還到池
                if (!bullet.IsActive)
                {
                    ReturnToPool(bullet);
                    activeBullets.RemoveAt(i);
                }
            }
        }

        #endregion

        #region Spawn/Destroy

        /// <summary>
        /// 生成子彈
        /// </summary>
        public Bullet SpawnBullet(
            BulletConfig config,
            Vector3 position,
            Vector3 direction)
        {
            // 從池獲取
            Bullet bullet = GetFromPool();

            // 初始化
            bullet.Initialize(config, position, direction);

            // 添加到活動列表
            activeBullets.Add(bullet);

            // 發布事件
            eventBus.Publish(new BulletSpawnedEvent
            {
                Bullet = bullet,
                Position = position,
                Type = config.Type
            });

            return bullet;
        }

        /// <summary>
        /// 銷毀子彈
        /// </summary>
        public void DestroyBullet(Bullet bullet)
        {
            if (!activeBullets.Contains(bullet))
                return;

            bullet.IsActive = false;
            activeBullets.Remove(bullet);
            ReturnToPool(bullet);
        }

        /// <summary>
        /// 清除所有子彈
        /// </summary>
        public void ClearAll()
        {
            foreach (var bullet in activeBullets)
            {
                ReturnToPool(bullet);
            }

            activeBullets.Clear();
        }

        #endregion

        #region Object Pool

        private Bullet GetFromPool()
        {
            if (bulletPool.Count > 0)
            {
                return bulletPool.Pop();
            }

            // 池空了，創建新的
            return bulletFactory.Create();
        }

        private void ReturnToPool(Bullet bullet)
        {
            bullet.Reset();
            bulletPool.Push(bullet);
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// 批量生成（彈幕）
        /// </summary>
        public List<Bullet> SpawnPattern(
            BulletConfig config,
            Vector3 origin,
            int count,
            float angleStep)
        {
            var bullets = new List<Bullet>(count);

            for (int i = 0; i < count; i++)
            {
                float angle = angleStep * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

                var bullet = SpawnBullet(config, origin, direction);
                bullets.Add(bullet);
            }

            return bullets;
        }

        #endregion
    }

    // ========================================================================
    // Services (服務接口)
    // ========================================================================

    /// <summary>
    /// 物理服務接口
    /// </summary>
    public interface IPhysicsService
    {
        CollisionHit OverlapSphere(Vector3 position, float radius, int layerMask);
    }

    /// <summary>
    /// 碰撞結果
    /// </summary>
    public struct CollisionHit
    {
        public bool HasHit;
        public GameObject GameObject;
        public Vector3 Normal;
    }

    // ========================================================================
    // Events (事件)
    // ========================================================================

    public struct BulletSpawnedEvent
    {
        public Bullet Bullet;
        public Vector3 Position;
        public BulletType Type;
    }

    public struct BulletHitEvent
    {
        public Bullet Bullet;
        public GameObject TargetGameObject;
        public Vector3 HitPosition;
        public Vector3 HitNormal;
        public int Damage;
        public BulletType Type;
    }

    public struct BulletExpiredEvent
    {
        public Bullet Bullet;
        public Vector3 Position;
    }
}