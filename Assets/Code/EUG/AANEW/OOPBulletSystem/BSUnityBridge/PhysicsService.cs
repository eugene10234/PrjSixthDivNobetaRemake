using System.Collections.Generic;
using Nobeta.EUG.BulletSystem.OOP;
//using Nobeta.EUG.Core.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// PhysicsService (物理服務實現)
// ========================================================================
namespace Nobeta.EUG.BulletSystem.OOP.UnityBridge
{

    /// <summary>
    /// 物理服務（封裝 Unity Physics）
    /// </summary>
    public class PhysicsService : IPhysicsService
    {
        public CollisionHit OverlapSphere(Vector3 position, float radius, int layerMask)
        {
            Collider[] hits = Physics.OverlapSphere(position, radius, layerMask);

            if (hits.Length > 0)
            {
                return new CollisionHit
                {
                    HasHit = true,
                    GameObject = hits[0].gameObject,
                    Normal = (position - hits[0].transform.position).normalized
                };
            }

            return new CollisionHit { HasHit = false };
        }
    }
}


