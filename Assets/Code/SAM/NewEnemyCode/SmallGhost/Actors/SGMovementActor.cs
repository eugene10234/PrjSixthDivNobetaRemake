using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// SmallGhost movement actor — balloon-like floating behaviour.
    /// No pathfinding, no ground orientation, no NodeData dependency.
    ///
    /// Behaviours composited each FixedUpdate inside Float():
    ///   Drift          — slow organic random walk on XZ via Perlin noise,
    ///                    blended toward player when beyond ChaseRange
    ///   Bob            — gentle sine wave on Y around spawn height
    ///   Peer repulsion — soft push away from nearby SmallGhost siblings
    ///   Player repulsion — soft push away from player when too close
    ///   Obstacle avoidance — SpacingSystem writes to ApplyAvoidance() each tick
    ///
    /// Implements IMovementActor so shared infrastructure (SpacingSystem etc.)
    /// can call it without knowing it is a balloon ghost.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SGMovementActor : MonoBehaviour, IMovementActor
    {
        // ── Injected ──────────────────────────────────────────────────────────

        private SmallGhostStat _stat;
        private SGPerceptionActor _perception;
        private BaseEnemyData _data;

        [Inject]
        public void Construct(SmallGhostStat stat, SGPerceptionActor perception, BaseEnemyData data)
        {
            _stat = stat;
            _data = data;
            _perception = perception;
        }

        // ── Inspector (tuning) ────────────────────────────────────────────────

        [Header("Drift")]
        [SerializeField] private float _driftSpeed      = 0.8f;
        [SerializeField] private float _noiseTimeScale  = 0.4f;   // how fast direction changes

        [Header("Bob")]
        [SerializeField] private float _bobAmplitude    = 0.35f;  // height in world units
        [SerializeField] private float _bobSpeed        = 1.2f;   // cycles per second

        [Header("Peer repulsion")]
        [SerializeField] private float _peerRadius       = 2.5f;
        [SerializeField] private float _peerRepulseForce = 1.5f;

        [Header("Player repulsion")]
        [SerializeField] private float _playerRadius       = 4.0f;
        [SerializeField] private float _playerRepulseForce = 2.0f;

        // ── Runtime ───────────────────────────────────────────────────────────

        private Rigidbody _rb;
        private float     _spawnY;
        private float     _noiseOffsetX;   // unique per instance — ghosts don't move identically
        private float     _noiseOffsetZ;
        private float     _bobOffset;      // unique phase so ghosts don't bob in sync
        private Vector3   _avoidanceVector;
        private Vector3   _currentTarget;  // set by MoveToward each tick

        private int _enemyMask;
        private int _playerMask;

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            _rb.useGravity     = false;
            _rb.isKinematic    = false;
            _rb.constraints    = RigidbodyConstraints.FreezeRotation;
            _rb.sleepThreshold = 0f;
            _rb.linearDamping  = 2f; // gentle drag so it doesn't overshoot

            // Unique noise seeds — without this every SG drifts identically
            _noiseOffsetX = Random.Range(0f, 100f);
            _noiseOffsetZ = Random.Range(0f, 100f);
            _bobOffset    = Random.Range(0f, Mathf.PI * 2f);

            _enemyMask  = 1 << LayerMask.NameToLayer("Enemy");
            _playerMask = 1 << LayerMask.NameToLayer("Player");
        }

        private void Start()
        {
            _spawnY = transform.position.y;
        }

        // ── IMovementActor ────────────────────────────────────────────────────

        public Vector3 Position => transform.position;

        /// <summary>
        /// Pass player position as target so Float() can blend toward them
        /// when beyond ChaseRange. Pass Vector3.zero when no target.
        /// </summary>
        public void MoveToward(Vector3 target)
        {
            _currentTarget = target;
            Float();
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector3.zero;
        }

        public void LookAt(Vector3 target, bool requireVelocity = true)
        {
            if (requireVelocity && _rb.linearVelocity.sqrMagnitude <= 0.01f) return;

            Vector3 lookDir = requireVelocity
                ? _rb.linearVelocity        // face drift direction while floating
                : target - transform.position; // face player while stopped for attack

            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot,
                                                     Time.fixedDeltaTime * 2f);
        }

        public void DeathMovement()
        {
            // Drift gently upward — dissolve sequence plays simultaneously
            _rb.linearVelocity = new Vector3(0f, 0.8f, 0f);
        }

        // Written by SpacingSystem each FixedTick — obstacle avoidance
        public void ApplyAvoidance(Vector3 direction)
        {
            _avoidanceVector = direction;
        }

        public void ClearPath() { } // no path to clear

        // ── Core float logic ──────────────────────────────────────────────────

        private void Float()
        {
            float t = Time.fixedTime;

            // ── Drift — Perlin noise on XZ ────────────────────────────────────
            float nx = Mathf.PerlinNoise(_noiseOffsetX + t * _noiseTimeScale, _noiseOffsetX)
                       * 2f - 1f;
            float nz = Mathf.PerlinNoise(_noiseOffsetZ, _noiseOffsetZ + t * _noiseTimeScale)
                       * 2f - 1f;
            Vector3 noiseDrift = new Vector3(nx, 0f, nz) * _driftSpeed;

            // ── Distance-based player pull ────────────────────────────────────
            // Beyond ChaseRange: blend noise with a pull toward player so it
            // closes distance organically rather than beelining straight at them.
            Vector3 driftVel;
            if (_currentTarget != Vector3.zero)
            {
                Vector3 toPlayer   = _currentTarget - transform.position;
                toPlayer.y         = 0f;
                float distToPlayer = toPlayer.magnitude;

                if (distToPlayer > _stat.ChaseRange)
                {
                    Vector3 pullDir = toPlayer.normalized * _driftSpeed;
                    driftVel = Vector3.Lerp(noiseDrift, pullDir, _stat.ChaseWeight);
                }
                else
                {
                    driftVel = noiseDrift;
                }
            }
            else
            {
                driftVel = noiseDrift; // no target — pure drift
            }

            // ── Bob — sine wave pulling Y back toward spawn height ────────────
            float targetY = _spawnY
                          + Mathf.Sin(t * _bobSpeed * Mathf.PI * 2f + _bobOffset)
                          * _bobAmplitude;
            float bobVelY = (targetY - transform.position.y) * 5f; // spring pull

            // ── Peer repulsion ────────────────────────────────────────────────
            Vector3 peerRepulse = Vector3.zero;
            Collider[] peers = Physics.OverlapSphere(transform.position, _peerRadius, _enemyMask);
            foreach (Collider peer in peers)
            {
                if (peer.gameObject == gameObject) continue;
                if (!peer.TryGetComponent<SmallGhost>(out _)) continue; // SmallGhost siblings only

                Vector3 away = transform.position - peer.transform.position;
                float   dist = away.magnitude;
                if (dist < 0.01f) continue;

                float strength = 1f - Mathf.Clamp01(dist / _peerRadius);
                peerRepulse   += away.normalized * strength * _peerRepulseForce;
            }

            // ── Player repulsion ──────────────────────────────────────────────
            Vector3 playerRepulse = Vector3.zero;
            Collider[] players = Physics.OverlapSphere(transform.position, _playerRadius, _playerMask);
            if (players.Length > 0)
            {
                Vector3 away = transform.position - players[0].transform.position;
                away.y       = 0f; // XZ only — don't push vertically

                float dist = away.magnitude;
                if (dist > 0.01f)
                {
                    // Quadratic falloff — soft at range, stronger up close
                    float t01      = 1f - Mathf.Clamp01(dist / _playerRadius);
                    float strength = t01 * t01;
                    playerRepulse  = away.normalized * strength * _playerRepulseForce;
                }
            }

            // ── Obstacle avoidance (from SpacingSystem) ───────────────────────
            Vector3 obstacleAvoid = _avoidanceVector * _driftSpeed;

            // ── Compose and clamp ─────────────────────────────────────────────
            Vector3 desiredXZ = driftVel + peerRepulse + playerRepulse + obstacleAvoid;

            float maxSpeed = _driftSpeed + _peerRepulseForce + _playerRepulseForce;
            if (desiredXZ.magnitude > maxSpeed)
                desiredXZ = desiredXZ.normalized * maxSpeed;

            Vector3 finalVelocity = new Vector3(desiredXZ.x, bobVelY, desiredXZ.z);

            // Lerp toward desired — Rigidbody linear damping smooths the rest
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, finalVelocity,
                                               Time.fixedDeltaTime * 4f);

            if (_data.Target != null)
            {
                LookAt(_data.Target.transform.position);
            }
        }

        public void ReturnHome()
        {
            throw new System.NotImplementedException();
        }

        public void LookAt(Vector3 target)
        {
            Vector3 lookDir;
            lookDir = target - transform.position;
            if (lookDir.sqrMagnitude < 0.01f) return;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                       Time.fixedDeltaTime * 8f);
        }

        public bool CheckReturnHome()
        {
            throw new System.NotImplementedException();
        }
    }
}
