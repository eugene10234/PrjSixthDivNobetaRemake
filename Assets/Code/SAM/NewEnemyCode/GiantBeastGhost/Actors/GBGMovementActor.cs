using Cysharp.Threading.Tasks;
using Nobeta.Sam.Pathfinding;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using VContainer;
using static UnityEngine.UI.Image;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost movement actor — owns the Rigidbody and all pathfinding/physics logic.
    /// Implements IMovementActor so shared states (ChaseState, IdleState) can call it
    /// without knowing it is a MonoBehaviour.
    ///
    /// Registered via RegisterComponent().As&lt;IMovementActor&gt;() in GBGLifetimeScope.
    ///
    /// Fixes applied:
    ///   1. Rotation constraints — X and Z frozen in Awake so physics can never tip the ghost
    ///   2. Collision normal projection — velocity redirected along wall surface on contact
    ///   3. LookAt — fixed to use a direction vector not raw world coordinates
    /// </summary>
    public class GBGMovementActor : MonoBehaviour, IMovementActor
    {
        // ── Private fields ────────────────────────────────────────────────────

        private List<Vector3> Route;
        private int _currentPathIndex;
        private float _pathUpdateTimer = 0f;
        private const float PATH_REFRESH_RATE = 2f;

        private Vector3 _currentAvoidanceVector;
        private bool hasLOS;
        private bool isHome;
        private Vector3 spawnLoc;
        int enemyMask;
        int obstacleMask;
        int _losBlockMask;

        // FIX 2 — collision normal written by OnCollisionStay, read in ApplyVelocity
        private Vector3 _collisionNormal = Vector3.zero;

        // Stuck detection
        private float _stuckTimer;
        private Vector3 _lastPos;

        // Node occupancy
        private float _nodeUpdateTimer;
        private Node _currentNode;

        // ── Injected ──────────────────────────────────────────────────────────

        private IPathFindingStrat _pfs;
        private GiantBeastGhostStat _stat;
        private NodeData _nodeData;

        [Inject]
        public void Construct(IPathFindingStrat pfs, GiantBeastGhostStat stat, NodeData nodeData)
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

            // FIX 1 — freeze X and Z rotation so physics can never tip or tilt the ghost.
            // Y rotation is left free so LookAt / Slerp can still turn the ghost.
            _rb.constraints = RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationZ
                            | RigidbodyConstraints.FreezeRotationY;

            enemyMask = 1 << LayerMask.NameToLayer("Enemy");
            obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
            _losBlockMask = enemyMask | obstacleMask;
        }

        private void Start()
        {
            spawnLoc = transform.position;
        }

        // ── FIX 2 — collision normal tracking ────────────────────────────────
        // OnCollisionStay accumulates contact normals while the ghost touches a surface.
        // ApplyVelocity projects movement onto that plane so the ghost slides along
        // walls instead of fighting into them.

        private void OnCollisionStay(Collision collision)
        {
            // Only redirect against obstacles — not other enemies
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

        private void UpdateLOS(Vector3 targetPos)
        {
            // Push origin slightly away from any wall we're currently touching
            // so the ray starts in clear space rather than inside the surface
            Vector3 originOffset = _collisionNormal != Vector3.zero
                ? _collisionNormal * 0.2f
                : Vector3.zero;

            Vector3 origin = transform.position + Vector3.up * 1.0f + originOffset;
            Vector3 targetUp = targetPos + Vector3.up * 1.0f;
            Vector3 dir = (targetUp - origin).normalized;
            float dist = Vector3.Distance(origin, targetUp);

            hasLOS = !Physics.Raycast(origin, dir, dist, _losBlockMask);

            Debug.DrawLine(origin, targetUp, hasLOS ? Color.green : Color.red);
        }

        // ── IMovementActor ────────────────────────────────────────────────────

        public Vector3 Position => transform.position;

        public void MoveToward(Vector3 target)
        {
            Move(target);
        }

        public void SlowMove()
        {
            Vector3 currentDirection = _rb.linearVelocity.normalized;
            Vector3 desiredVelocity = currentDirection * 0.1f;
            _rb.linearVelocity = desiredVelocity;
        }

        public void DodgeRotation(Vector3 hitposition)
        {
            // 1. Calculate direction and velocity (Keep your flattened logic)
            Vector3 myPos = transform.position;
            hitposition.y = myPos.y;
            Vector3 dodgeDirection = (myPos - hitposition).normalized;

            // 2. Handle Rotation (Face the threat)
            if (dodgeDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-dodgeDirection, Vector3.up);
            }

        }
        
        public async UniTask StartDodgeAsync(Vector3 hitposition, CancellationToken token)
        {
            // 1. Calculate direction and velocity (Keep your flattened logic)
            Vector3 myPos = transform.position;
            hitposition.y = myPos.y;
            Vector3 dodgeDirection = (myPos - hitposition).normalized;
            Vector3 sideDir = Vector3.Cross(dodgeDirection, Vector3.up).normalized;
            if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                sideDir = -sideDir;
            // Apply the initial burst
            _rb.linearVelocity = (dodgeDirection + sideDir).normalized * _stat.dodgeSpeed+ _currentAvoidanceVector*_stat.SpacingSpeed;

            //// 2. Handle Rotation (Face the threat)
            //if (dodgeDirection != Vector3.zero)
            //{
            //    transform.rotation = Quaternion.LookRotation(-dodgeDirection, Vector3.up);
            //}

            // 3. Wait for exactly 0.5 seconds
            // Using DelayFrame or Delay depending on if you want fixed time or scaled time
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), delayTiming: PlayerLoopTiming.FixedUpdate);

            // 4. Clean up: Stop the momentum so the enemy doesn't drift
            _rb.linearVelocity = Vector3.zero;
        }

        public void SlowTurn(Vector3 target)
        {
            Vector3 lookDir = target - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                   Time.fixedDeltaTime * 6f);
        }
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

        // FIX 3 — LookAt was passing raw world coords as a direction.
        // A direction must be (target - current position), not just target.xyz.
        public void LookAt(Vector3 target)
        {
            if (_rb.linearVelocity.sqrMagnitude <= 0.1f) return;

            Vector3 lookDir;
            if (hasLOS)
            {
                // Face the player directly when we can see them
                lookDir = target - transform.position;
            }
            else
            {
                // Face the direction we are actually moving when navigating around obstacles
                lookDir = _rb.linearVelocity;
            }

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

        // ── Public event ──────────────────────────────────────────────────────

        public Action OnReturnHome;

        public bool CheckReturnHome() => isHome;

        // ── Movement methods ──────────────────────────────────────────────────

        public void ReturnHome()
        {
            if ((spawnLoc - transform.position).sqrMagnitude < 2f)
            {
                isHome = true;
                return;
            }

            isHome = false;

            Vector3 returnSpawnDirection = (spawnLoc - transform.position).normalized;
            float distanceFromHome = Vector3.Distance(spawnLoc, transform.position);
            float homingRange = 5f;
            float returnWeight = 1f - distanceFromHome / homingRange;

            Vector3 targetNode = Seek(spawnLoc);
            Vector3 targetNodeDirection = (targetNode - transform.position).normalized;
            Vector3 desiredDirection;

            if (CheckIfStuck())
            {
                Route.Clear();
                targetNode = Seek(spawnLoc);
                targetNodeDirection = (targetNode - transform.position).normalized;
                desiredDirection = Vector3.Lerp(targetNodeDirection,
                                                   returnSpawnDirection,
                                                   returnWeight).normalized;
            }
            else
            {
                desiredDirection = (targetNodeDirection + returnSpawnDirection).normalized;
            }

            ApplyVelocity(desiredDirection);
        }

        public void Move(Vector3 targetPos)
        {
            NodeUpdate();

            // LOS check
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 targetUp = targetPos + Vector3.up * 1.5f;
            Vector3 direction = (targetUp - origin).normalized;
            float distance = Vector3.Distance(origin, targetUp);

            UpdateLOS(targetPos);

            // Direction selection
            Vector3 directionToPlayer = (targetPos - transform.position).normalized;
            Vector3 targetNode = Seek(targetPos);
            Vector3 targetNodeDirection = (targetNode - transform.position).normalized;
            Vector3 desiredDirection;
            Vector3 sideDir;

            if (CheckIfStuck())
            {
                Route.Clear();
                targetNode = Seek(targetPos);
                targetNodeDirection = (targetNode - transform.position).normalized;
                sideDir = Vector3.Cross(targetNodeDirection, Vector3.up).normalized;
                if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                    sideDir = -sideDir;
                desiredDirection = (targetNodeDirection + sideDir).normalized;
            }
            else if (!hasLOS)
            {
                desiredDirection = targetNodeDirection;
            }
            else
            {
                float distanceToPlayer = Vector3.Distance(transform.position, targetPos);
                float spiralRange = 10f;
                float spiralWeight = Mathf.Clamp01(1f - distanceToPlayer / spiralRange);
                sideDir = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
                if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                    sideDir = -sideDir;
                desiredDirection = (directionToPlayer + sideDir * spiralWeight).normalized;
            }

            ApplyVelocity(desiredDirection);
        }

        // ── Shared velocity application ───────────────────────────────────────

        private void ApplyVelocity(Vector3 desiredDir)
        {
            Vector3 avoidVelocity = _currentAvoidanceVector * _stat.SpacingSpeed;
            Vector3 moveVelocity = desiredDir * _stat.Speed;

            // FIX 2 — project velocity onto collision surface so ghost slides
            // along walls instead of pushing into them. Has no effect when
            // _collisionNormal is zero (no active wall contact).
            if (_collisionNormal != Vector3.zero)
            {
                moveVelocity = Vector3.ProjectOnPlane(moveVelocity, _collisionNormal);
                avoidVelocity = Vector3.ProjectOnPlane(avoidVelocity, _collisionNormal);
            }

            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity,
                                               moveVelocity + avoidVelocity,
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
                Vector3 targetNode = Route[_currentPathIndex];
                if (Vector3.Distance(transform.position, targetNode) < 1f)
                    _currentPathIndex++;
                return targetNode;
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
                    Debug.Log("Stuck!");
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
    }
}
