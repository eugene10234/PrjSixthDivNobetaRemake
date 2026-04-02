using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Bullet
{
    /// <summary>
    /// Singleton registered in EnemyManager.
    /// The only entry point enemies and bosses use to fire projectiles.
    ///
    /// Usage from an animation actor or state:
    ///     _bulletSystem.Fire(muzzlePoint.position, direction, _fireBoltData, gameObject);
    ///
    /// Manages one BulletPool per distinct BulletData asset.
    /// Pools are created on first use (lazy) so unused spell types cost nothing.
    /// </summary>
    public class EnemyBulletSystem
    {
        private readonly Dictionary<BulletData, EnemyBulletPool> _pools = new();
        private readonly Transform _poolRoot; // organisational root in hierarchy

        [Inject]
        public EnemyBulletSystem(EnemyBulletSystemRoot root)
        {
            _poolRoot = root.transform;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Fire a straight or homing bullet.</summary>
        /// <param name="origin">Muzzle world position.</param>
        /// <param name="direction">Direction the bullet travels (normalized).</param>
        /// <param name="data">Which spell/bullet type to fire.</param>
        /// <param name="instigator">The enemy GameObject that fired this (ignored in collision).</param>
        /// <param name="homingTarget">Optional transform to home toward.</param>
        public Bullet Fire(Vector3 origin, Vector3 direction, BulletData data,
                           GameObject instigator, Transform homingTarget = null)
        {
            EnemyBulletPool pool = GetOrCreatePool(data);
            return pool.Get(origin, direction, instigator, homingTarget);
        }

        /// <summary>
        /// Fire toward a world position rather than a raw direction.
        /// Convenience wrapper used by most cast states.
        /// </summary>
        public Bullet FireAt(Vector3 origin, Vector3 targetPosition, BulletData data,
                             GameObject instigator, Transform homingTarget = null)
        {
            Vector3 direction = (targetPosition+new Vector3(0,1.3f,0) - origin).normalized;
            return Fire(origin, direction, data, instigator, homingTarget);
        }

        /// <summary>Return all active bullets of a given type to the pool immediately.</summary>
        public void ClearAll(BulletData data)
        {
            if (_pools.TryGetValue(data, out EnemyBulletPool pool))
                pool.ReturnAll();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private EnemyBulletPool GetOrCreatePool(BulletData data)
        {
            if (!_pools.TryGetValue(data, out EnemyBulletPool pool))
            {
                // Create an organisational child under the pool root
                var poolParent = new GameObject($"Pool_{data.name}").transform;
                poolParent.SetParent(_poolRoot);

                pool = new EnemyBulletPool(data, poolParent);
                _pools[data] = pool;
            }

            return pool;
        }
    }
}
