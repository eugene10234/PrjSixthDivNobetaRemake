using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Bullet Collision System（子彈碰撞系統 - Pure C#）
// ========================================================================
namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{
    /// <summary>
    /// 子彈碰撞系統
    /// 職責：檢測碰撞並觸發事件
    /// </summary>
    public class BulletCollisionSystem : ISystem
    {
        private readonly BulletData[] bullets;
        private readonly int maxBullets;
        private readonly IEventBus eventBus;
        private readonly Func<Vector3, float, int, CollisionResult> physicsCheck;

        public BulletCollisionSystem(
            BulletData[] bulletArray,
            int maxCount,
            IEventBus eventBus,
            Func<Vector3, float, int, CollisionResult> physicsCallback)
        {
            bullets = bulletArray;
            maxBullets = maxCount;
            this.eventBus = eventBus;
            physicsCheck = physicsCallback;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < maxBullets; i++)
            {
                if (!bullets[i].isActive)
                    continue;

                // 執行物理檢測（委託給 Unity 橋接層）
                var result = physicsCheck(
                    bullets[i].position,
                    bullets[i].colliderRadius,
                    bullets[i].layerMask
                );

                if (result.hasHit)
                {
                    // 發布碰撞事件
                    eventBus.Publish(new BulletHitEvent
                    {
                        bulletId = bullets[i].id,
                        targetId = result.targetId,
                        hitPosition = result.hitPosition,
                        hitNormal = result.hitNormal,
                        damage = bullets[i].damage,
                        type = bullets[i].type
                    });

                    // 標記子彈為非活動（將被銷毀）
                    bullets[i].isActive = false;
                }
            }
        }
    }

    /// <summary>
    /// 碰撞結果（Pure Struct）
    /// </summary>
    public struct CollisionResult
    {
        public bool hasHit;
        public int targetId;
        public Vector3 hitPosition;
        public Vector3 hitNormal;
    }

   
}