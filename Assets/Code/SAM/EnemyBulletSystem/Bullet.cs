using System;
using UnityEngine;

namespace Nobeta.Sam.Bullet
{
    /// <summary>
    /// Lives on each pooled bullet GameObject.
    /// Configured by BulletPool.Get() — never constructed directly.
    ///
    /// Responsibilities:
    ///   - Move straight or home toward a target each FixedUpdate
    ///   - Auto-return to pool after lifetime expires
    ///   - On collision: raise OnHit event, spawn impact VFX, return to pool
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class Bullet : MonoBehaviour
    {
        // ── Runtime config (set by BulletPool.Get each time) ─────────────────

        private BulletData   _data;
        private GameObject   _instigator;
        private Transform    _homingTarget;
        private float        _lifetimeTimer;
        private bool         _isActive;
        private Rigidbody    _rb;
        private SphereCollider _col;
        private TrailRenderer[] allTrails;
        private bool _hasTrails;

        // ── Events ────────────────────────────────────────────────────────────

        /// Raised on impact — BulletPool listens to this to return the bullet.
        public event Action<Bullet, BulletHitEvent> OnHit;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _rb  = GetComponent<Rigidbody>();
            _col = GetComponent<SphereCollider>();
            allTrails = GetComponentsInChildren<TrailRenderer>();
            _rb.useGravity  = false;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void OnEnable()
        {
            if (allTrails != null && allTrails.Length > 0)
            {
                foreach (var t in allTrails)
                {
                    t.Clear();
                }
            }
        }
        private void FixedUpdate()
        {
            if (!_isActive) return;

            // ── Lifetime ──────────────────────────────────────────────────────
            _lifetimeTimer -= Time.fixedDeltaTime;
            if (_lifetimeTimer <= 0f)
            {
                ReturnToPool(transform.position, didHit: false);
                return;
            }

            // ── Movement ──────────────────────────────────────────────────────
            if (_data.IsHoming && _homingTarget != null)
            {
                Vector3 desired = (_homingTarget.position - transform.position).normalized;
                Vector3 current = _rb.linearVelocity.normalized;
                Vector3 steered = Vector3.RotateTowards(
                    current, desired,
                    _data.HomingStrength * Mathf.Deg2Rad * Time.fixedDeltaTime,
                    0f);
                _rb.linearVelocity = steered * _data.Speed;
            }
            else
            {
                // Straight — velocity set once in Activate, just keep it
                _rb.linearVelocity = transform.forward * _data.Speed;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            // Ignore the instigator
            if (_instigator != null && other.gameObject == _instigator) return;
            if (_instigator != null && other.transform.IsChildOf(_instigator.transform)) return;

            Vector3 hitPos = transform.position;

            // ── Direct hit ────────────────────────────────────────────────────
            var directHandler = other.GetComponentInParent<IBulletHitHandler>();
            if (directHandler != null)
            {
                var evt = BuildEvent(hitPos, _data.Damage);
                directHandler.OnBulletHit(evt);
                ReturnToPool(hitPos, didHit: true);
                return;
            }

            // ── AoE hit ───────────────────────────────────────────────────────
            if (_data.AoERadius > 0f)
            {
                ApplyAoE(hitPos);
                ReturnToPool(hitPos, didHit: true);
                return;
            }

            // Hit something with no handler (wall, terrain) — still return
            ReturnToPool(hitPos, didHit: true);
        }

        // ── Public API (called by BulletPool) ─────────────────────────────────

        public void Activate(BulletData data, Vector3 position, Vector3 direction,
                             GameObject instigator, Transform homingTarget = null)
        {
            _data          = data;
            _instigator    = instigator;
            _homingTarget  = homingTarget;
            _lifetimeTimer = data.Lifetime;
            _isActive      = true;

            transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            _rb.linearVelocity = direction.normalized * data.Speed;

            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            _isActive = false;
            _rb.linearVelocity = Vector3.zero;
            gameObject.SetActive(false);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private void ApplyAoE(Vector3 center)
        {
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            Collider[] hits = Physics.OverlapSphere(center, _data.AoERadius, playerMask);

            foreach (Collider hit in hits)
            {
                var handler = hit.GetComponentInParent<IBulletHitHandler>();
                if (handler == null) continue;

                // Falloff: full damage at center, AoEFalloff multiplier at edge
                float dist       = Vector3.Distance(center, hit.transform.position);
                float t          = Mathf.Clamp01(dist / _data.AoERadius);
                float multiplier = Mathf.Lerp(1f, _data.AoEFalloff, t);
                float damage     = _data.Damage * multiplier;

                handler.OnBulletHit(BuildEvent(center, damage));
            }
        }

        private void ReturnToPool(Vector3 hitPos, bool didHit)
        {
            if (!_isActive) return; // guard double-return

            // Spawn impact VFX
            if (didHit && _data.ImpactVFXPrefab != null)
            {
                var vfx = GameObject.Instantiate(_data.ImpactVFXPrefab, hitPos, Quaternion.identity);
                GameObject.Destroy(vfx, 3f); // auto-cleanup
            }

            var evt = BuildEvent(hitPos, _data.Damage);
            OnHit?.Invoke(this, evt);

            Deactivate();
        }

        private BulletHitEvent BuildEvent(Vector3 pos, float damage) => new BulletHitEvent
        {
            HitPosition = pos,
            Damage      = damage,
            AoERadius   = _data.AoERadius,
            AoEFalloff  = _data.AoEFalloff,
            Instigator  = _instigator,
            Data        = _data,
        };
    }
}
