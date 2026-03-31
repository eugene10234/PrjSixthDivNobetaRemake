using UnityEngine;

namespace Nobeta.Sam.Bullet
{
    /// <summary>
    /// Per-spell configuration asset.
    /// Create one per spell type: FireBolt, IceShard, BossOrb, etc.
    /// BulletSystem.Fire() takes one of these to configure the pooled bullet.
    /// </summary>
    [CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Objects/BulletData")]
    public class BulletData : ScriptableObject
    {
        [Header("Movement")]
        public float Speed         = 12f;
        public float Lifetime      = 5f;  // seconds before auto-return to pool
        public bool  IsHoming      = false;
        public float HomingStrength = 5f; // degrees per second turn rate when homing

        [Header("Damage")]
        public float Damage        = 10f;
        public float AoERadius     = 0f;  // 0 = no AoE, direct hit only
        public float AoEFalloff    = 1f;  // damage multiplier at edge of AoE radius

        [Header("Pool")]
        public int   PoolSize      = 20;  // how many of this type to pre-warm

        [Header("VFX")]
        public GameObject ImpactVFXPrefab;   // spawned at hit point, then auto-destroyed
        public GameObject ProjectilePrefab;  // the visual on the bullet itself (optional override)
    }
}
