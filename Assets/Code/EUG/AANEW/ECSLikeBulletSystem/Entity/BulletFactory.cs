using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Bullet Factory（子彈工廠 - Pure C#）
// ========================================================================
namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{
    /// <summary>
    /// 子彈工廠
    /// 職責：創建和初始化子彈數據
    /// </summary>
    public class BulletFactory
    {
        private int nextId = 0;

        public BulletData CreateBullet(
            BulletConfig config,
            Vector3 position,
            Vector3 direction)
        {
            return new BulletData
            {
                id = nextId++,
                isActive = true,

                // Transform
                position = position,
                rotation = Quaternion.LookRotation(direction),

                // Velocity
                velocity = direction.normalized * config.speed,

                // Damage
                damage = config.damage,
                type = config.type,

                // Lifetime
                lifetime = config.lifetime,
                maxLifetime = config.lifetime,

                // Collision
                colliderRadius = config.colliderRadius,
                layerMask = LayerMask.GetMask("Enemy"),

                // Visual
                visualIndex = config.visualIndex
            };
        }
    }
}