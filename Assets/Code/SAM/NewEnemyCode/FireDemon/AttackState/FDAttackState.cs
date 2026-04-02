
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public class FDAttackState : IState
    {
        private readonly FDMovementActor _movement;
        private readonly FDAnimationActor _animation;
        private readonly FDPerceptionActor _perception;
        private readonly EnemyStat _stat;
        private readonly BaseEnemyData _data;

        private float _attackCooldown;

        public event Action OnTargetOutOfRange;

        [Inject]
        public FDAttackState(
            FDMovementActor movement,
            FDAnimationActor animation,
            FDPerceptionActor perception,
            EnemyStat stat,
            BaseEnemyData data)
        {
            _movement = movement;
            _animation = animation;
            _perception = perception;
            _stat = stat;
            _data = data;
        }

        public void OnEnter()
        {
            _movement.LookAt(_data.Target.transform.position);
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

            _movement.LookAt(_data.Target.transform.position);

            _attackCooldown -= Time.fixedDeltaTime;
            if (_attackCooldown <= 0f)
            {
                _movement.Stop();
                StartBreatheFire();
            }
        }

        private bool _isAttacking;
        private async void StartBreatheFire()
        {
            if (_isAttacking) return;
            _isAttacking = true;
            await _animation.PlayAttack();
            _attackCooldown = 1f / _stat.AttackSpeed;
            _isAttacking = false;
        }
        public void OnExit()
        {
            _attackCooldown = 0;
        }
    }
}



