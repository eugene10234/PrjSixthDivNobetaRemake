using Cysharp.Threading.Tasks;
using Nobeta.Sam.Pathfinding;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King movement actor — mirrors BGMovementActor in structure.
    /// Owns the Rigidbody and all pathfinding / physics logic for the GK.
    /// Implements IMovementActor so shared and GK-specific states remain decoupled
    /// from Unity / physics details.
    ///
    /// Registered via RegisterComponent().AsSelf().As&lt;IMovementActor&gt;() in GKLifetimeScope.
    /// </summary>
    public class GKMovementActor : MonoBehaviour, IMovementActor
    {
        // ── Private fields ────────────────────────────────────────────────────
        private List<Vector3> Route;
        private int _currentPathIndex;
        private float _pathUpdateTimer;
        private const float PATH_REFRESH_RATE = 2f;
        private Vector3 muzzleOriPoint;
        private Vector3 _currentAvoidanceVector;
        private bool _hasLOS;

        public Transform muzzlePoint;
        private int _enemyMask;
        private int _obstacleMask;
        private int _losBlockMask;

        private Vector3 _collisionNormal = Vector3.zero;

        // Stuck detection
        private float _stuckTimer;
        private Vector3 _lastPos;

        // Node occupancy
        private float _nodeUpdateTimer;
        private Node _currentNode;

        // ── Injected ──────────────────────────────────────────────────────────

        private IPathFindingStrat _pfs;
        private NewGKStat _stat;
        private NodeData _nodeData;

        [Inject]
        public void Construct(IPathFindingStrat pfs, NewGKStat stat, NodeData nodeData)
        {
            _pfs = pfs;
            _stat = stat;
            _nodeData = nodeData;
            Route = new List<Vector3>();
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.sleepThreshold = 0f;

            _rb.constraints = RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezeRotationY;

            _enemyMask = 1 << LayerMask.NameToLayer("Enemy");
            _obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
            _losBlockMask = _enemyMask | _obstacleMask;
        }

        private void Start()
        {
        }

        // ── Collision normal tracking ─────────────────────────────────────────

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Obstacle")) return;

            Vector3 normal = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
                normal += contact.normal;
            _collisionNormal = normal.normalized;
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Obstacle")) return;
            _collisionNormal = Vector3.zero;
        }

        // ── LOS ───────────────────────────────────────────────────────────────

        private void UpdateLOS(Vector3 targetPos)
        {
            Vector3 originOffset = _collisionNormal != Vector3.zero
                ? _collisionNormal * 0.2f
                : Vector3.zero;

            Vector3 origin = transform.position + Vector3.up * 1.0f + originOffset;
            Vector3 targetUp = targetPos + Vector3.up * 1.0f;
            Vector3 dir = (targetUp - origin).normalized;
            float dist = Vector3.Distance(origin, targetUp);

            _hasLOS = !Physics.Raycast(origin, dir, dist, _losBlockMask);
            Debug.DrawLine(origin, targetUp, _hasLOS ? Color.green : Color.red);
        }

        // ── IMovementActor ────────────────────────────────────────────────────

        public Vector3 Position => transform.position;

        public void MoveToward(Vector3 target) => Move(target);

        public void DeathMovement()
        {
            _rb.linearVelocity = new Vector3(0, 0.5f, 0);
        }

        public void ApplyAvoidance(Vector3 direction)
        {
            _currentAvoidanceVector = direction;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector3.zero;
        }

        public void LookAt(Vector3 target)
        {
            Vector3 lookDir = _hasLOS
                ? target - transform.position
                : _rb.linearVelocity;

            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                   Time.fixedDeltaTime * 6f);
        }

        public void ClearPath()
        {
            Route.Clear();
        }

        // ── Public events ─────────────────────────────────────────────────────

        public Action OnReturnHome;

        public bool CheckReturnHome() => _isHome;
        private bool _isHome;

        // ── Movement methods ──────────────────────────────────────────────────

        public void ReturnHome()
        {
            if ((transform.position - TransLoc).sqrMagnitude < 1f)
            {
                _rb.linearVelocity = Vector3.zero;
                _isHome = true;
                return;
            }

            _isHome = false;

            Vector3 returnDir = (TransLoc - transform.position).normalized;
            float distFromHome = Vector3.Distance(TransLoc, transform.position);
            float homingRange = 5f;
            float returnWeight = 1f - distFromHome / homingRange;

            Vector3 targetNode = Seek(TransLoc);
            Vector3 targetNodeDir = (targetNode - transform.position).normalized;
            Vector3 desiredDir;

            if (CheckIfStuck())
            {
                Route.Clear();
                targetNode = Seek(TransLoc);
                targetNodeDir = (targetNode - transform.position).normalized;
                desiredDir = Vector3.Lerp(targetNodeDir, returnDir, returnWeight).normalized;
            }
            else
            {
                desiredDir = (targetNodeDir + returnDir).normalized;
            }

            ApplyVelocity(desiredDir);
        }

        public void Move(Vector3 targetPos)
        {
            NodeUpdate();
            UpdateLOS(targetPos);

            Vector3 dirToPlayer = (targetPos - transform.position).normalized;
            Vector3 targetNode = Seek(targetPos);
            Vector3 targetNodeDir = (targetNode - transform.position).normalized;
            Vector3 desiredDir;
            Vector3 sideDir;

            if (CheckIfStuck())
            {
                Route.Clear();
                targetNode = Seek(targetPos);
                targetNodeDir = (targetNode - transform.position).normalized;
                sideDir = Vector3.Cross(targetNodeDir, Vector3.up).normalized;
                if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                    sideDir = -sideDir;
                desiredDir = (targetNodeDir + sideDir).normalized;
            }
            else if (!_hasLOS)
            {
                desiredDir = targetNodeDir;
            }
            else
            {
                float distToPlayer = Vector3.Distance(transform.position, targetPos);
                float spiralRange = 10f;
                float spiralWeight = Mathf.Clamp01(1f - distToPlayer / spiralRange);
                sideDir = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
                if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                    sideDir = -sideDir;
                desiredDir = (dirToPlayer + sideDir * spiralWeight).normalized;
            }

            ApplyVelocity(desiredDir);
        }

        // ── Velocity ──────────────────────────────────────────────────────────

        private void ApplyVelocity(Vector3 desiredDir)
        {
            Vector3 avoidVelocity = _currentAvoidanceVector * _stat.SpacingSpeed;
            Vector3 moveVelocity = desiredDir * _stat.Speed;

            if (_collisionNormal != Vector3.zero)
            {
                moveVelocity = Vector3.ProjectOnPlane(moveVelocity, _collisionNormal);
                avoidVelocity = Vector3.ProjectOnPlane(avoidVelocity, _collisionNormal);
            }

            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity,
                                               moveVelocity + avoidVelocity,
                                               Time.fixedDeltaTime * 5f);
        }

        private void ApplySideVelocity(Vector3 desiredDir)
        {
            Vector3 moveVelocity = desiredDir * _stat.sideSpeed;

            if (_collisionNormal != Vector3.zero)
            {
                moveVelocity = Vector3.ProjectOnPlane(moveVelocity, _collisionNormal);
            }

            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity,
                                               moveVelocity,
                                               Time.fixedDeltaTime * 5f);
        }

        // ── Node occupancy ────────────────────────────────────────────────────

        public void NodeUpdate()
        {
            _nodeUpdateTimer -= Time.fixedDeltaTime;
            if (_nodeUpdateTimer > 0f) return;
            _nodeUpdateTimer = 0.5f;

            if (_nodeData.AllNodes.Count == 0) return;

            Node newNode = _nodeData.GetClosestValidNode(transform.position);
            if (newNode == null || newNode == _currentNode) return;

            if (_currentNode != null) _currentNode.enemiesOccupied--;
            newNode.enemiesOccupied++;
            _currentNode = newNode;
        }

        // ── Pathfinding ───────────────────────────────────────────────────────

        private Vector3 Seek(Vector3 targetPos)
        {
            _pathUpdateTimer += Time.fixedDeltaTime;
            if (Route.Count == 0 || _pathUpdateTimer >= PATH_REFRESH_RATE)
            {
                _pathUpdateTimer = 0f;
                Route = _pfs.CalculatePath(transform.position, targetPos)
                        ?? new List<Vector3>();
                _currentPathIndex = 0;
            }

            if (Route.Count > 0 && _currentPathIndex < Route.Count)
            {
                Vector3 node = Route[_currentPathIndex];
                if (Vector3.Distance(transform.position, node) < 1f)
                    _currentPathIndex++;
                return node;
            }

            return targetPos;
        }

        // ── Stuck detection ───────────────────────────────────────────────────

        private bool CheckIfStuck()
        {
            if (Vector3.Distance(transform.position, _lastPos) < 0.1f)
            {
                _stuckTimer += Time.fixedDeltaTime;
                if (_stuckTimer > 1.5f)
                {
                    Debug.Log("[GK] Stuck!");
                    return true;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }

            _lastPos = transform.position;
            return false;
        }


        //GKSpecifics

        [SerializeField] private Transform LaserPoint;
        [SerializeField] private Transform leftPoint;
        [SerializeField] private Transform rightPoint;
        [SerializeField] private Transform StagePoint;
        private Vector3 TransLoc;
        private float risingSpeed = 1.6f;

        public void ResetMuzzlePoint()
        {
            ResetMuzzlePointRotation();
        }
        
        public async UniTask<bool> Rise()
        {
            bool finished = false;
            _rb.linearVelocity = new Vector3(0, risingSpeed, 0);

            // Loop until the distance is small enough
            while (transform.position.y <= StagePoint.position.y)
            {
                // This is the "magic" part: it waits for the next frame before checking again
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                // Optional: Add a check to stop if the object is destroyed
                if (this == null) return false;
            }

            // Once the loop finishes, stop the movement and return true
            _rb.linearVelocity = Vector2.zero;
            finished = true;
            return finished;
        }

        private float _followTimer;
        private float _followThreshold =3f;
        public async UniTask<bool> HorrizontalFollow(GameObject player, CancellationToken token)
        {
            if (player == null) return false;

            _followTimer = 0f;

            while (_followTimer <= _followThreshold)
            {
                if (token.IsCancellationRequested) return false;

                // Safety check if player is destroyed mid-follow
                if (player == null) break;

                // Calculate horizontal direction
                float xDiff = player.transform.position.x - transform.position.x;
                Vector3 horizontalDir = new Vector3(Mathf.Sign(xDiff), 0, 0);

                // If very close, stop moving but keep the timer running
                if (Mathf.Abs(xDiff) < 0.8f) horizontalDir = Vector3.zero;

                ApplySideVelocity(horizontalDir);

                _followTimer += Time.fixedDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

                if (this == null) return false;
            }

            _rb.linearVelocity = Vector3.zero;
            return true;
        }
        public async UniTask<bool> ReturnStagePoint()
        {
            // 1. Calculate direction once or inside the loop? 
            // Inside is better if the transitionPoint moves.
            while (Vector3.SqrMagnitude(transform.position - StagePoint.position) > 0.01f)
            {
                // TARGET - SELF = Direction towards target
                Vector3 desireDir = (StagePoint.position - transform.position).normalized;

                ApplyVelocity(desireDir);

                Quaternion targetRot = Quaternion.LookRotation(Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                       Time.fixedDeltaTime * 6f);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                if (this == null) return false;
            }

            // 2. Full stop on all axes
            _rb.linearVelocity = Vector3.zero;
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
            return true;
        }
        public async UniTask<bool> ReturnLaserPoint()
        {
            // 1. Calculate direction once or inside the loop? 
            // Inside is better if the transitionPoint moves.
            while (Vector3.SqrMagnitude(transform.position - LaserPoint.position) > 0.05f)
            {
                // TARGET - SELF = Direction towards target
                Vector3 desireDir = (LaserPoint.position - transform.position).normalized;

                ApplyVelocity(desireDir);

                Quaternion targetRot = Quaternion.LookRotation(Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                       Time.fixedDeltaTime * 6f);
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

                if (this == null) return false;
            }

            // 2. Full stop on all axes
            _rb.linearVelocity = Vector3.zero;
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
            return true;
        }

        private const float ROTATION_SPEED = 6f;

        public void ResetMuzzlePointRotation()
        {
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward);
            muzzlePoint.rotation = Quaternion.Slerp(
                muzzlePoint.rotation, targetRot, Time.fixedDeltaTime * ROTATION_SPEED);
        }
        
        public void AimMuzzleAt(Vector3 target)
        {
            Vector3 aimDir = target - muzzlePoint.position;
            if (aimDir.sqrMagnitude < 0.25f) return;

            Quaternion targetRot = Quaternion.LookRotation(aimDir);
            muzzlePoint.rotation = Quaternion.Slerp(
                muzzlePoint.rotation, targetRot, Time.fixedDeltaTime * ROTATION_SPEED);
        }
    }
}
