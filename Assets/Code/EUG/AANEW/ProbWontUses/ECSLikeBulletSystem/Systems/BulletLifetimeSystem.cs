using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Bullet Lifetime System（子彈生命週期系統 - Pure C#）
// ========================================================================
namespace Nobeta.EUG.BulletSystemV1.ECSLike.Pure
{
    /// <summary>
    /// 子彈生命週期系統
    /// 職責：管理子彈的生命週期，自動銷毀過期子彈
    /// </summary>
    public class BulletLifetimeSystem : ISystem
    {
        private readonly BulletData[] bullets;
        private readonly int maxBullets;
        private readonly Action<int> onBulletExpired;

        public BulletLifetimeSystem(
            BulletData[] bulletArray,
            int maxCount,
            Action<int> expiredCallback)
        {
            bullets = bulletArray;
            maxBullets = maxCount;
            onBulletExpired = expiredCallback;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < maxBullets; i++)
            {
                if (!bullets[i].isActive)
                    continue;

                // 減少生命週期
                bullets[i].lifetime -= deltaTime;

                // 檢查是否過期
                if (bullets[i].lifetime <= 0)
                {
                    // 通知管理器銷毀
                    onBulletExpired?.Invoke(bullets[i].id);
                }
            }
        }
    }

}