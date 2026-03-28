using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost perception actor — OverlapSphere detection and LOS raycasting.
    /// Implements IPerceptionActor so states call clean methods rather than
    /// embedding Physics calls inside their own FixedTick.
    ///
    /// Registered via RegisterComponent().As&lt;IPerceptionActor&gt;() in GBGLifetimeScope.
    /// </summary>
    public class GBGPerceptionActor : MonoBehaviour, IPerceptionActor
    {
        private GiantBeastGhostStat _stat;
        private Vector3 spawnLoc;

        [Inject]
        public void Construct(GiantBeastGhostStat stat)
        {
            _stat = stat;
        }

        private void Start()
        {
            spawnLoc = transform.position;
        }

        // ── IPerceptionActor ──────────────────────────────────────────────────

        public GameObject ScanForPlayer(float detectionRadius)
        {
            int layerMask = 1 << LayerMask.NameToLayer("Player");
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, layerMask);
            return hits.Length > 0 ? hits[0].gameObject : null;
        }

        public bool HasLineOfSight(Vector3 target)
        {
            Vector3 origin    = transform.position + Vector3.up * 1.0f;
            Vector3 targetPos = target             + Vector3.up * 1.0f;
            Vector3 direction = (targetPos - origin).normalized;
            float   distance  = Vector3.Distance(origin, targetPos);

            int enemyMask    = 1 << LayerMask.NameToLayer("Enemy");
            int obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
            int combinedMask = enemyMask | obstacleMask;

            return !Physics.SphereCast(origin, 0.8f, direction, out _, distance, combinedMask);
        }

        // ── Cached jittered target ────────────────────────────────────────────
        // Updated on a fixed interval so all callers get a consistent value per tick
        // regardless of how many times GetApproximateTargetPosition is called.

        private Vector3 _cachedTarget;
        private float   _cacheTimer;
        private const float CacheInterval = 1.0f;

        public Vector3 GetApproximateTargetPosition(Vector3 exactTarget, float attackRange)
        {
            _cacheTimer -= Time.fixedDeltaTime;
            if (_cacheTimer > 0f) return _cachedTarget;

            _cacheTimer = CacheInterval;

            float   minRadius = 0.1f;
            float   maxRadius = attackRange * 0.5f;
            Vector2 dir       = Random.insideUnitCircle.normalized;
            float   dist      = Random.Range(minRadius, maxRadius);
            Vector2 offset    = dir * dist;

            _cachedTarget = exactTarget + new Vector3(offset.x, 0f, offset.y);
            return _cachedTarget;
        }

        public Vector3 InformSpawnLocation()
        {
            return spawnLoc;
        }
    }
}
