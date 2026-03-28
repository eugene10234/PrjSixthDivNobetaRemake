using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost-specific attack state.
    /// Faces the target, plays the attack animation on a cooldown timer,
    /// and raises OnTargetOutOfRange when the player moves away.
    ///
    /// Not shared — MageGhost has CastState, SmallGhost has its own lunge.
    /// </summary>
    public class GBGAttackState : IState
    {
        private enum HeavyAttackMode
        {
            startwinding,
            finishwinding,
            fire,
        }
        private HeavyAttackMode _currentMode;
        private readonly GBGMovementActor  _movement;
        private readonly GBGAnimationActor _animation;
        private readonly GBGPerceptionActor _perception;
        private readonly EnemyStat         _stat;
        private readonly BaseEnemyData     _data;

        private float _attackCooldown;
        private bool _isAttacking;
        public event Action OnTargetOutOfRange;

        [Inject]
        public GBGAttackState(
            GBGMovementActor  movement,
            GBGAnimationActor animation,
            GBGPerceptionActor perception,
            EnemyStat          stat,
            BaseEnemyData      data)
        {
            _movement   = movement;
            _animation  = animation;
            _perception = perception;
            _stat       = stat;
            _data       = data;
            animation.OnWindingFinished += () => 
            {
                _currentMode = HeavyAttackMode.finishwinding;
            };
        }

        public void OnEnter()
        {
            _currentMode = HeavyAttackMode.startwinding;
            _attackDelayTimer = 0f;
            _isAttacking = false;
            _data.isStartAttacking = true;
        }

        private float _attackDelayTimer;
        public void OnFixedTick()
        {
            if (_data.Target == null) { OnTargetOutOfRange?.Invoke(); return; }

            float dist = Vector3.Distance(_movement.Position, _data.Target.transform.position);
            if (dist > _stat.AttackRange && !_isAttacking)
            {
                OnTargetOutOfRange?.Invoke();
                return;
            }

            _movement.SlowMove();
            _movement.SlowTurn(_data.Target.transform.position);

            if (_attackCooldown <= 0)
            {
                _data.isImmuneStagger = true;
                switch (_currentMode)
                {
                    case HeavyAttackMode.startwinding:
                        StartWinding();
                        break;
                    case HeavyAttackMode.finishwinding:
                        _attackDelayTimer += Time.fixedDeltaTime;
                        if (_attackDelayTimer >= 0.8)
                        { 
                            _animation.ActivateGlowHorn();
                        }
                        if (_attackDelayTimer >= 1)
                        {
                            _currentMode = HeavyAttackMode.fire;
                            _attackDelayTimer = 0;
                        }
                        break;
                    case HeavyAttackMode.fire:
                        Fire();
                        _attackCooldown = (1 / _stat.AttackSpeed);
                        break;
                }
            }
            _attackCooldown -= Time.fixedDeltaTime;
        }

        public void OnExit()
        {
            _isAttacking = false;
            _attackDelayTimer = 0f;
            _currentMode = HeavyAttackMode.startwinding;
            _data.isStartAttacking = false;
        }

        private void StartWinding()
        {
          if (_isAttacking) return;
          _animation.PlayAttack();
          _isAttacking = true;
        }

        private void Fire()
        {
            _animation.StartFire();
            _isAttacking = false;
            _currentMode = HeavyAttackMode.startwinding;
            _data.isImmuneStagger = false;
            _data.isStartAttacking = false;
        }
    }
}
