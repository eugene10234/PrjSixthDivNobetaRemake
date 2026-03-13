using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// Supporting Structures（輔助結構）
// ========================================================================
namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{
    /// <summary>
    /// 統計信息
    /// </summary>
    public struct BulletManagerStats
    {
        public int ActiveCount;
        public int MaxCapacity;
        public float UtilizationRate;
    }
}