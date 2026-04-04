using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Bullet Manager Config（配置）
// ========================================================================
namespace Nobeta.EUG.BulletSystemV1.ECSLike.Pure
{
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