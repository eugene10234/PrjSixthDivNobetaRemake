using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Bullet Movement System（子彈移動系統 - Pure C#）
// ========================================================================
namespace Nobeta.EUG.BulletSystemV1.ECSLike.Pure
{

    /// <summary>
    /// 子彈移動系統
    /// 職責：更新子彈位置和旋轉
    /// </summary>
    public class BulletMovementSystem : ISystem
    {
        private readonly BulletData[] bullets;
        private readonly int maxBullets;

        public BulletMovementSystem(BulletData[] bulletArray, int maxCount)
        {
            bullets = bulletArray;
            maxBullets = maxCount;
        }

        public void Update(float deltaTime)
        {
            // 遍歷所有子彈
            for (int i = 0; i < maxBullets; i++)
            {
                // 跳過不活動的子彈
                if (!bullets[i].isActive)
                    continue;

                // 更新位置
                bullets[i].position += bullets[i].velocity * deltaTime;

                // 更新旋轉（朝向移動方向）
                if (bullets[i].velocity != Vector3.zero)
                {
                    bullets[i].rotation = Quaternion.LookRotation(bullets[i].velocity);
                }
            }
        }
    }
}