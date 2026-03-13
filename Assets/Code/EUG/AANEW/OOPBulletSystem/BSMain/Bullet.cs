using Assets.Code.EUG.AANEW.Temp;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
// ========================================================================
// Bullet (Pure C# Class - 核心子彈類)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP
{

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

}