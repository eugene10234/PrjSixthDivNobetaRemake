using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Bullet
{
    /// <summary>
    /// Object pool for a single BulletData type.
    /// BulletSystem owns one of these per distinct BulletData asset.
    /// Pre-warms PoolSize bullets on creation.
    /// </summary>
    public class EnemyBulletPool
    {
        private readonly BulletData      _data;
        private readonly Transform       _parent;       // organisational parent in hierarchy
        private readonly Stack<Bullet>   _free  = new();
        private readonly HashSet<Bullet> _active = new();

        public EnemyBulletPool(BulletData data, Transform parent)
        {
            _data   = data;
            _parent = parent;
            Prewarm(data.PoolSize);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public Bullet Get(Vector3 position, Vector3 direction,
                          GameObject instigator, Transform homingTarget = null)
        {
            Bullet bullet = _free.Count > 0 ? _free.Pop() : CreateBullet();

            bullet.OnHit += HandleBulletHit;
            _active.Add(bullet);

            bullet.Activate(_data, position, direction, instigator, homingTarget);
            return bullet;
        }

        public void ReturnAll()
        {
            foreach (Bullet b in _active)
            {
                b.OnHit -= HandleBulletHit;
                b.Deactivate();
                _free.Push(b);
            }
            _active.Clear();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                _free.Push(CreateBullet());
        }

        private Bullet CreateBullet()
        {
            // Use the data's projectile prefab if provided, otherwise a plain GameObject
            GameObject go = _data.ProjectilePrefab != null
                ? GameObject.Instantiate(_data.ProjectilePrefab, _parent)
                : new GameObject($"Bullet_{_data.name}");

            go.transform.SetParent(_parent);
            go.SetActive(false);

            // Ensure required components exist (Bullet.Awake handles Rigidbody/Collider)
            if (!go.TryGetComponent<Bullet>(out var bullet))
                bullet = go.AddComponent<Bullet>();

            return bullet;
        }

        private void HandleBulletHit(Bullet bullet, BulletHitEvent evt)
        {
            if (!_active.Contains(bullet)) return;

            bullet.OnHit -= HandleBulletHit;
            _active.Remove(bullet);
            _free.Push(bullet);
            // Bullet.Deactivate() was already called inside ReturnToPool
        }
    }
}
