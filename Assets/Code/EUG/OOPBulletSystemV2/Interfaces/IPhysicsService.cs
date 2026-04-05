using UnityEngine;

namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public interface IPhysicsService
    {
        CollisionHit OverlapSphere(Vector3 position, float radius, Vector3 direction, int layerMask);
        CollisionHit SphereCast(Vector3 position, float radius, Vector3 direction, float distance, int layerMask);
    }
}
