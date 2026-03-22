using Cysharp.Threading.Tasks;
using Nobeta.Sam.Pathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{


    public class BigGhostActor : MonoBehaviour
    {
        private Animator _animator;
        private IPathFindingStrat _pfs;
        private List<Vector3> Route;
        private BigGhostStat _bgs;
        private Rigidbody _rb;
        private int _currentPathIndex;
        private float _pathUpdateTimer = 0f;
        private const float PATH_REFRESH_RATE = 2f;
        public event Action OnSpacingFinished;
        private Vector3 _currentAvoidanceVector;
        private GameObject _player;
        private bool hasLOS;
        private NodeData _nodeData;
        private Vector3 spawnLoc;
        private SkinnedMeshRenderer _smr;
        [Inject]

        public void Construct(IPathFindingStrat pfs, BigGhostStat bgs, NodeData nodeData)
        {
            _bgs = bgs;
            _pfs = pfs;
            _nodeData = nodeData;
            Route = new List<Vector3>();
        }
        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _rb = GetComponent<Rigidbody>();
            _smr = GetComponentInChildren<SkinnedMeshRenderer>();
        }
        private void Start()
        {
            spawnLoc = transform.position;
        }

        private static readonly int DissolveAmount = Shader.PropertyToID("_Dissolve");
        private MaterialPropertyBlock _propBlock;

        public async UniTask PlayDeathSequence(float duration)
        {
            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                // This applies to the RENDERER, regardless of which material is swapped in
                _smr.GetPropertyBlock(_propBlock);
                _propBlock.SetFloat(DissolveAmount, progress);
                _smr.SetPropertyBlock(_propBlock);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }

        private float _stuckTimer;
        private Vector3 _lastPos;
        private bool CheckIfStuck()
        {
            if (Vector3.Distance(transform.position, _lastPos) < 0.1f)
            {
                _stuckTimer += Time.fixedDeltaTime;
                if (_stuckTimer > 1.5f)
                {
                    return true;
                }
            }
            else
            {
                _stuckTimer = 0;
            }
            _lastPos = transform.position;
            return false;
        }

        private Vector3 SeekHome()
        {
            _pathUpdateTimer += Time.fixedDeltaTime;
            if (Route.Count == 0 || _pathUpdateTimer >= PATH_REFRESH_RATE)
            {
                _pathUpdateTimer = 0f;
                Route = _pfs.CalculatePath(transform.position, spawnLoc) ?? new List<Vector3>();
                _currentPathIndex = 0;
            }
            if (Route != null && Route.Count > 0 && _currentPathIndex < Route.Count)
            {
                Vector3 targetNode = Route[_currentPathIndex];
                if (Vector3.Distance(transform.position, targetNode) < 1f)
                {
                    _currentPathIndex++;
                }
                return targetNode;
            }
            if (Route == null)
            {
                Debug.Log("Route is Null");
            }
            return spawnLoc;
        }

        public Action OnReturnHome;
        public void ReturnHome()
        {
            if ((spawnLoc - transform.position).sqrMagnitude < 2f)
            {
                OnReturnHome?.Invoke();
                return;
            }
            Vector3 avoidVelocity = _currentAvoidanceVector * _bgs.SpacingSpeed;
            Vector3 returnSpawnDirection = (spawnLoc - transform.position).normalized;
            float distanceFromHome = Vector3.Distance(spawnLoc, transform.position);
            float homingRange = 5f;
            Vector3 movementVelocity;
            Vector3 DesiredDirection;
            Vector3 playerlocation = GetPlayerLocation();
            Vector3 directionToPlayer = (playerlocation - transform.position).normalized;
            Vector3 targetNode = SeekHome();
            Vector3 targetNodeDirection = (targetNode - transform.position).normalized;
            bool isStuck = CheckIfStuck();
            float returnWeight = (1 - distanceFromHome / homingRange);

            if (isStuck)
            {
                Route.Clear();
                targetNode = SeekHome();
                targetNodeDirection = (targetNode - transform.position).normalized;
                DesiredDirection = Vector3.Lerp(targetNodeDirection, returnSpawnDirection, returnWeight).normalized;
            }
            else
            {
                DesiredDirection = (targetNodeDirection + returnSpawnDirection).normalized;
            }
            movementVelocity = DesiredDirection * _bgs.Speed;
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, movementVelocity + avoidVelocity, Time.fixedDeltaTime * 5f);
            if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                Vector3 lookDir = new Vector3(movementVelocity.x, 0, movementVelocity.z);
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 6f);
            }
        }
        public void Move()
        {
            if (_player == null)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 1.0f;
            Vector3 targetPos = _player.transform.position + Vector3.up * 1.0f;
            Vector3 direction = (targetPos - origin).normalized;
            float distance = Vector3.Distance(origin, targetPos);
            int EnemyLayerMask = 1 << LayerMask.NameToLayer("Enemy");
            int ObstacleLayerMask = 1 << LayerMask.NameToLayer("Obstacle");
            int CombinedMask = EnemyLayerMask | ObstacleLayerMask;

            hasLOS = !Physics.SphereCast(
                origin,
                0.5f,
                direction,
                out RaycastHit hit,
                distance,
                CombinedMask
            );

            Vector3 avoidVelocity = _currentAvoidanceVector * _bgs.SpacingSpeed;
            Vector3 movementVelocity;
            Vector3 DesiredDirection;
            Vector3 playerlocation = GetPlayerLocation();
            Vector3 directionToPlayer = (playerlocation - transform.position).normalized;
            Vector3 targetNode = Seek();
            Vector3 targetNodeDirection = (targetNode - transform.position).normalized;
            Vector3 sideDir;
            bool isStuck = CheckIfStuck();

            if (isStuck)
            {
                Route.Clear();
                targetNode = Seek();
                targetNodeDirection = (targetNode - transform.position).normalized;
                sideDir = Vector3.Cross(targetNodeDirection, Vector3.up).normalized;
                if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                {
                    sideDir = -sideDir;
                }
                DesiredDirection = (targetNodeDirection + sideDir).normalized;
            }
            else if (!hasLOS)
            {
                targetNode = Seek();
                targetNodeDirection = (targetNode - transform.position).normalized;
                DesiredDirection = targetNodeDirection.normalized;
            }
            else
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerlocation);
                float spiralRange = 10f;
                sideDir = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
                if (Vector3.Dot(sideDir, _currentAvoidanceVector) > 0)
                {
                    sideDir = -sideDir;
                }
                float spiralWeight = Mathf.Clamp01(1 - distanceToPlayer / spiralRange);
                DesiredDirection = (directionToPlayer + sideDir * spiralWeight).normalized;
            }

            movementVelocity = DesiredDirection * _bgs.Speed;
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, movementVelocity + avoidVelocity, Time.fixedDeltaTime * 5f);

            if (_rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 lookDir;
                if (hasLOS) { lookDir = new Vector3(directionToPlayer.x, 0, directionToPlayer.z); }
                else { lookDir = new Vector3(movementVelocity.x, 0, movementVelocity.z); }
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 6f);
            }
            origin = transform.position + Vector3.up * 1.5f;
            targetPos = _player.transform.position + Vector3.up * 1.5f;
            Debug.DrawLine(origin, targetPos, hasLOS ? Color.green : Color.red);
        }


        private float nodeUpdateTimer;
        private Node currentNode;
        public void NodeUpdate()
        {
            nodeUpdateTimer -= Time.fixedDeltaTime;
            if (nodeUpdateTimer > 0) return;

            nodeUpdateTimer = 0.5f;
            Node newNode;
            if (_nodeData.AllNodes.Count != 0)
            {
                newNode = _nodeData.GetClosestValidNode(transform.position);


                if (newNode == null || newNode == currentNode) return;

                // Leave old node
                if (currentNode != null)
                {
                    currentNode.enemiesOccupied--;
                }

                // Enter new node
                if (newNode != null)
                {
                    newNode.enemiesOccupied++;
                }
                currentNode = newNode;
            }
        }


        public void Idle()
        {
            _rb.linearVelocity = Vector3.zero;
        }
        private Vector3 Seek()
        {
            if (_player == null)
            {
                return Vector3.zero;
            }
            _pathUpdateTimer += Time.fixedDeltaTime;
            if (Route.Count == 0 || _pathUpdateTimer >= PATH_REFRESH_RATE)
            {
                _pathUpdateTimer = 0f;
                Route = _pfs.CalculatePath(transform.position, GetPlayerLocation()) ?? new List<Vector3>();
                _currentPathIndex = 0;
            }
            if (Route != null && Route.Count > 0 && _currentPathIndex < Route.Count)
            {
                Vector3 targetNode = Route[_currentPathIndex];
                if (Vector3.Distance(transform.position, targetNode) < 1f)
                {
                    _currentPathIndex++;
                }
                return targetNode;
            }
            if (Route == null)
            {
                Debug.Log("Route is Null");
            }
            return GetPlayerLocation();
        }
        public void OnHit(Vector3 position)
        {
            _animator.SetTrigger("tHit");
            _rb.linearVelocity = Vector3.zero;
            Vector3 direction = (position - gameObject.transform.position).normalized;
            direction.y = 0;
        }
        public void GivePlayerInfo(GameObject target)
        {
            _player = target;
        }

        private Vector3 _cachedTarget;
        private float _updatePlayertimer;

        public Vector3 GetPlayerLocation()
        {
            _updatePlayertimer -= Time.deltaTime;

            if (_updatePlayertimer <= 0f)
            {
                float minRadius = 0.1f;
                float maxRadius = _bgs.AttackRange * 0.5f;

                Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
                float distance = UnityEngine.Random.Range(minRadius, maxRadius);

                Vector2 offset = dir * distance;

                _cachedTarget = _player.transform.position + new Vector3(offset.x, 0, offset.y);

                _updatePlayertimer = 1.0f; // update every second
            }

            return _cachedTarget;
        }
        public void GiveSpacingDirection(Vector3 direction)
        {
            _currentAvoidanceVector = direction;
        }

        public GameObject Look()
        {
            float detectionRadius = 60f;
            int layermask = 1 << LayerMask.NameToLayer("Player");
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, layermask);

            if (hitColliders.Length > 0)
            {
                if (_player == null)
                {
                    _player = hitColliders[0].gameObject;
                }
                return hitColliders[0].gameObject;
            }
            return null;
        }

        public void OnAttack(GameObject target)
        {
            _rb.linearVelocity = Vector3.zero;
            Vector3 lookDir = (_player.transform.position - transform.position);
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f);
            _animator.SetTrigger("tAttack");
        }
        public async void OnDeath()
        {
            _animator.SetBool("isDead", true);
            _rb.linearVelocity = new Vector3(0, 0.5f, 0);
            await PlayDeathSequence(5);
            Destroy(gameObject);
        }
    }
}
