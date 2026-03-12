using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
// ========================================================================
// BulletManager (管理器 - Pure C#)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP
{
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

        private const int POOL_CAPACITY = 5000;

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
}