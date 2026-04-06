using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King chase state.
    /// TODO: implement pursuit logic (teleport dash, phase-through-walls, etc.)
    /// </summary>
    public class GKChaseState : IState
    {
        private readonly GKMovementActor _movement;
        private readonly GKPerceptionActor _perception;
        private readonly NewGKStat _stat;
        private readonly GhostKingData _data;
        private GameObject _player;
        public event Action OnInMeleeAttackRange;
        public event Action OnOutofMeleeAttackRange;

        [Inject]
        public GKChaseState(GKMovementActor movement, GKPerceptionActor perception,
                          NewGKStat stat, GhostKingData data)
        {
            _movement = movement;
            _perception = perception;
            _stat = stat;
            _data = data;
            Debug.Log("Chase Initialization Success");
        }

        public void OnEnter() 
        {
            _player = _perception.ScanForPlayer(50);
            outofRangeTimer = 0;
        }
        public void OnExit() { }

        private float outofRangeTimer;
        private float outofRangeMaxTime = 2;
        public void OnFixedTick()
        {
          

            float dist = Vector3.Distance(_movement.Position, _player.transform.position);

            if (dist <= _stat.AttackRange)
            {
                OnInMeleeAttackRange?.Invoke();
                return;
            }

            if (dist > _stat.ChaseRange)
            {
                outofRangeTimer += Time.fixedDeltaTime;
            }

            if (outofRangeTimer >= outofRangeMaxTime)
            {
                OnOutofMeleeAttackRange?.Invoke();
                return;
            }
            // Get jittered approach target so enemies don't all stack on same point
            _movement.MoveToward(_player.transform.position);
            _movement.LookAt(_player.transform.position);
        }
    }
}
