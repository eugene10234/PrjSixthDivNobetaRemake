using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Core Data Structures（核心數據結構 - Pure Struct）
// ========================================================================
namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{
    /// <summary>
    /// 子彈數據（Pure Struct，無引用類型，零 GC）
    /// </summary>
    public struct BulletData
    {
        // ID 和狀態
        public int id;
        public bool isActive;

        // Transform 組件
        public Vector3 position;
        public Quaternion rotation;

        // Velocity 組件
        public Vector3 velocity;

        // Damage 組件
        public int damage;
        public BulletType type;

        // Lifetime 組件
        public float lifetime;
        public float maxLifetime;

        // Collision 組件
        public float colliderRadius;
        public int layerMask;

        // Visual 組件（索引）
        public int visualIndex;
    }

    /// <summary>
    /// 子彈配置（Pure Struct）
    /// </summary>
    public struct BulletConfig
    {
        public float speed;
        public int damage;
        public float lifetime;
        public float colliderRadius;
        public BulletType type;
        public int visualIndex;
    }

    /// <summary>
    /// 子彈類型
    /// </summary>
    public enum BulletType
    {
        Normal,
        Fire,
        Ice,
        Lightning,
        Poison
    }   
}