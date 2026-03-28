using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// BigGhost-specific attack state.
    /// Faces the target, plays the attack animation on a cooldown timer,
    /// and raises OnTargetOutOfRange when the player moves away.
    ///
    /// Not shared — MageGhost has CastState, SmallGhost has its own lunge.
    /// </summary>
    public class BGAttackState : IState
    {
        private readonly BGMovementActor  _movement;
        private readonly BGAnimationActor _animation;
        private readonly BGPerceptionActor _perception;
        private readonly EnemyStat       _stat;
        private readonly BaseEnemyData   _data;

        private float _attackCooldown;

        public event Action OnTargetOutOfRange;

        [Inject]
        public BGAttackState(
            BGMovementActor movement,
            BGAnimationActor animation,
            BGPerceptionActor perception,
            EnemyStat        stat,
            BaseEnemyData    data)
        {
            _movement   = movement;
            _animation  = animation;
            _perception = perception;
            _stat       = stat;
            _data       = data;
        }

        public void OnEnter()
        {
            _attackCooldown = 0f; // attack immediately on entry
        }

        public void OnFixedTick()
        {
            if (_data.Target == null) { OnTargetOutOfRange?.Invoke(); return; }

            float dist = Vector3.Distance(_movement.Position, _data.Target.transform.position);
            if (dist > _stat.AttackRange)
            {
                OnTargetOutOfRange?.Invoke();
                return;
            }

            _movement.Stop();
            _movement.LookAt(_data.Target.transform.position);

            _attackCooldown -= Time.fixedDeltaTime;
            if (_attackCooldown <= 0f)
            {
                _animation.PlayAttack();
                _attackCooldown = 1f / _stat.AttackSpeed;
            }
        }

        public void OnExit()
        {
            _attackCooldown = 0f;
        }
    }
}
