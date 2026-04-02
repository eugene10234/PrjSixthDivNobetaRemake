using Nobeta.Sam.Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public class FDMovementActor : MonoBehaviour, IMovementActor
    {
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

        // FIX 2 ¡X collision normal written by OnCollisionStay, read in ApplyVelocity
        private Vector3 _collisionNormal = Vector3.zero;

        // Stuck detection
        private float _stuckTimer;
        private Vector3 _lastPos;

        // Node occupancy
        private float _nodeUpdateTimer;
        private Node _currentNode;

        //Injected
        private FDPerceptionActor _perception;
        private IPathFindingStrat _pfs;
        private FDStat _stat;
        private NodeData _nodeData;

        [Inject]
        public void Construct(IPathFindingStrat pfs, FDStat stat, NodeData nodeData, FDPerceptionActor perception)
        {
            _pfs = pfs;
            _stat = stat;
            _nodeData = nodeData;
            _perception = perception;
            Route = new List<Vector3>();
        }

        //Unity lifecycle 

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.sleepThreshold = 0f;
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

       

        private void OnCollisionStay(Collision collision)
        {
            // Only redirect against obstacles ¡X not other enemies
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



        //IMovementActor

        public Vector3 Position => transform.position;

        public void MoveToward(Vector3 target)
        {
            Move(target);
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

        public void LookAt(Vector3 target)
        {
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

        

        public Action OnReturnHome;

        public bool CheckReturnHome() => isHome;

        

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

        // ¢w¢w Shared velocity application ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w

        private void ApplyVelocity(Vector3 desiredDir)
        {
            Vector3 avoidVelocity = _currentAvoidanceVector * _stat.SpacingSpeed;
            Vector3 moveVelocity = desiredDir * _stat.Speed;

            // FIX 2 ¡X project velocity onto collision surface so ghost slides
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

        // ¢w¢w Node occupancy ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w

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

        // ¢w¢w Pathfinding ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w

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

        // ¢w¢w Stuck detection ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w

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
