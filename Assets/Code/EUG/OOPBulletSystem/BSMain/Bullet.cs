using Assets.Code.EUG.AANEW.Temp;
using Nobeta.EUG.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
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
        public Vector3 Direction { get; set; }
        public float Speed { get; private set; }
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
            Direction = direction.normalized;
            Speed = config.Speed;
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
            //DefaultRotateTo();
            SinRotateTo();
        }

        private void DefaultRotateTo()
        {
            if (Velocity != Vector3.zero)
            {
                Rotation = Quaternion.LookRotation(Velocity);
            }
        }
        float spinSpeed = 720f * 1.5f;
        float currentSpinAngle = 0f;
        private void SinRotateTo()
        {
            if (Velocity != Vector3.zero)
            {
                // A. 計算基礎朝向 (指向飛行方向)
                Quaternion lookRotation = Quaternion.LookRotation(Velocity);

                // B. 更新累積的自轉角度
                currentSpinAngle += spinSpeed * Time.deltaTime;

                // C. 建立自轉四元數 (繞著自己的 Z 軸旋轉)
                Quaternion spinRotation = Quaternion.AngleAxis(currentSpinAngle, Vector3.forward);

                // D. 結合兩者：先對齊方向，再套用自轉
                // 注意：順序很重要，lookRotation * spinRotation 代表在 lookRotation 的基礎上進行局部旋轉
                Rotation = lookRotation * spinRotation;
            }
        }

        private void CheckCollision()
        {
            // 委託給物理服務檢測
            CollisionHit hit;
            hit = physicsService.OverlapSphere(
                Position,
                ColliderRadius,
                LayerMask.GetMask("Enemy", "Ground", "Obstacle", "Player")
            );
            if (!hit.HasHit)
            {
                hit = physicsService.SphereCast(
                    Position,
                    ColliderRadius,
                    Direction,
                    Speed * Time.deltaTime * 1.1f,
                    LayerMask.GetMask("Enemy", "Ground", "Obstacle", "Player")
                    );
            }

            //if (hit.GameObject.CompareTag("Enemy"))
            //{
            //    //eventBus?.Publish(new BulletHitEnemyEvent
            //    //{
            //    //    Bullet = bullet,
            //    //    Enemy = hitObj.gameObject,
            //    //    Damage = bullet.Damage,
            //    //    HitPosition = transform.position
            //    //});
            //}
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
            IDamageable target;
            if (EnemyDamageSystem.TryGetTarget(hit.GameObject, out target))
            {
                //hit.GameObject.transform.position = HitAway(hit);
                target.TakeDamage(Damage, Position);

                // AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{sfxName}");
                // AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);Assets/MasterMagicFX/ParticlesVer3/Cosmos/CosmoShoot2/Prefabs/Par_CosmoShootMuzzle.prefab

            }
            //if (hit.GameObject.CompareTag("Player"))
            //{
            //    //eventBus?.Publish(new BulletHitEnemyEvent
            //    //{
            //    //    Bullet = bullet,
            //    //    Enemy = hitObj.gameObject,
            //    //    Damage = bullet.Damage,
            //    //    HitPosition = transform.position
            //    //});
            //}
            IsActive = false;
        }

        private static Vector3 HitAway(CollisionHit hit)
        {
            var targetPos = hit.GameObject.transform.position;
            float originalY = targetPos.y; // 記錄原始 Y

            Vector3 moveDirection = hit.Normal;
            moveDirection.y = 0; // 確保推動方向沒有垂直分量

            targetPos -= moveDirection;
            targetPos.y = originalY; // 強制把 Y 設回原本的位置，防止任何誤差
            return targetPos;
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