using UnityEngine;

namespace Nobeta.Sam.Bullet
{
    /// <summary>
    /// Payload raised by Bullet on impact.
    /// Kept as a struct — no heap allocation per hit.
    /// </summary>
    public struct BulletHitEvent
    {
        public Vector3    HitPosition;
        public float      Damage;
        public float      AoERadius;
        public float      AoEFalloff;
        public GameObject Instigator;  // the enemy that fired this bullet
        public BulletData Data;
    }

    /// <summary>
    /// Implement on anything that can receive bullet damage:
    /// player, destructible props, shields, etc.
    /// Bullet calls this on the first collider it finds that implements it.
    /// </summary>
    public interface IBulletHitHandler
    {
        void OnBulletHit(BulletHitEvent hitEvent);
    }
}
