using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared chase state — used by every enemy type.
    /// Moves toward the target using IMovementActor (no knowledge of Rigidbody or pathfinding).
    ///
    /// Raises:
    ///   OnInAttackRange   — when distance drops below AttackRange (state machine → attack state)
    ///   OnOutOfChaseRange — when distance exceeds ChaseRange    (state machine → idle/home)
    /// </summary>
    public class ChaseState : IState
    {
        private readonly IMovementActor    _movement;
        private readonly IPerceptionActor  _perception;
        private readonly EnemyStat         _stat;
        private readonly BaseEnemyData     _data;

        public event Action OnInAttackRange;
        public event Action OnOutOfChaseRange;

        [Inject]
        public ChaseState(IMovementActor movement, IPerceptionActor perception,
                          EnemyStat stat, BaseEnemyData data)
        {
            _movement   = movement;
            _perception = perception;
            _stat       = stat;
            _data       = data;
            Debug.Log("Chase Initialization Success");
        }

        public void OnEnter() { }
        public void OnExit()  { }

        public void OnFixedTick()
        {
            if (_data.Target == null) { OnOutOfChaseRange?.Invoke(); return; }

            float dist = Vector3.Distance(_movement.Position, _data.Target.transform.position);

            if (dist <= _stat.AttackRange)
            {
                OnInAttackRange?.Invoke();
                return;
            }

            if (dist > _stat.ChaseRange)
            {
                OnOutOfChaseRange?.Invoke();
                return;
            }

            // Get jittered approach target so enemies don't all stack on same point
            var approachTarget = _perception.GetApproximateTargetPosition(
                _data.Target.transform.position, _stat.AttackRange);

            _movement.ApplyAvoidance(_data.AvoidanceDirection);
            _movement.MoveToward(approachTarget);
            _movement.LookAt(_data.Target.transform.position);
        }
    }
}
