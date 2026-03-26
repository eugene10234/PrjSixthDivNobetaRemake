using System;
using System.Collections.Generic;
using Nobeta.EUG.Interfaces;
using UnityEngine;
using VContainer;
// ========================================================================
// Services (服務接口)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP
{
    /// <summary>
    /// 物理服務接口
    /// </summary>
    public interface IPhysicsService
    {
        CollisionHit OverlapSphere(Vector3 position, float radius, int layerMask);
        CollisionHit SphereCast(Vector3 position, float radius, Vector3 direction, float distance, int layerMask);
    }

    /// <summary>
    /// 碰撞結果
    /// </summary>
    public struct CollisionHit
    {
        public bool HasHit;
        public GameObject GameObject;
        public Vector3 Normal;
    }
}