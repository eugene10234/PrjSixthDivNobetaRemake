using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
// 僅用於 Vector3, Quaternion 等數學類型
using UnityEngine;
// ========================================================================
// System Interface（系統接口）
// ========================================================================
namespace Nobeta.EUG.BulletSystem.ECSLike.Pure
{

    /// <summary>
    /// 系統接口（所有系統都實現這個）
    /// </summary>
    public interface ISystem
    {
        void Update(float deltaTime);
    }

}