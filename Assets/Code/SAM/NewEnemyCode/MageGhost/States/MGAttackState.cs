using Cysharp.Threading.Tasks;
using Nobeta.Sam.Bullet;
using System;
using System.Threading;
using UnityEngine;
using VContainer;
using static Coffee.UIExtensions.UIParticleAttractor;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// MageGhost-specific attack state.
    /// Faces the target, plays the attack animation on a cooldown timer,
    /// and raises OnTargetOutOfRange when the player moves away.
    ///
    /// Not shared — MageGhost has CastState, SmallGhost has its own lunge.
    /// </summary>
    public class MGAttackState : IState
    {
        private readonly MGMovementActor _movement;
        private readonly MGAnimationActor _animation;
        private readonly MGPerceptionActor _perception;
        private readonly EnemyStat _stat;
        private readonly BaseEnemyData _data;
        private readonly EnemyBulletSystem _bulletSystem;
        private readonly BulletData _spellData;
        private Transform _muzzlePoint;
        private float _attackCooldown;
        private bool _isAttacking;
        private CancellationTokenSource _cts;

        public event Action OnTargetOutOfRange;
        public event Action OnLaserFinished;

        [Inject]
        public MGAttackState(
            MGMovementActor movement,
            MGAnimationActor animation,
            MGPerceptionActor perception,
            EnemyBulletSystem bulletSystem,
            BulletData spellData,
            EnemyStat stat,
            BaseEnemyData data)
        {
            _movement = movement;
            _animation = animation;
            _perception = perception;
            _bulletSystem = bulletSystem;
            _spellData = spellData;
            _stat = stat;
            _data = data;
        }

        public void OnEnter()
        {
            _cts = new CancellationTokenSource();
            _isAttacking = false;
            // Start cooldown at full so the first attack isn't instant
            _attackCooldown = 1f / _stat.AttackSpeed;
            _data.isStartAttacking = true;
            _muzzlePoint = _movement.muzzle;
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

            //_movement.Stop();
            _movement.LookAt(_data.Target.transform.position);

            _attackCooldown -= Time.fixedDeltaTime;
            if (_attackCooldown <= 0f && !_isAttacking)
            {
                _movement.AimMuzzleAt(_data.Target.transform.position);
                StartShootingLaser().Forget();
            }
        }

        public void OnExit()
        {
            // Cancels the laser mid-flight if the state is interrupted
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _isAttacking = false;
            _data.isStartAttacking = false;
        }

        private async UniTask FireLaser(CancellationToken token)
        {
            for (int i = 0; i <= 9; i++)
            {
                _bulletSystem.FireAt(
                   origin: _muzzlePoint.position,
                   targetPosition: _data.Target.transform.position,
                   data: _spellData,
                   instigator: _movement.gameObject,
                   homingTarget: _spellData.IsHoming ? _data.Target.transform : null);

                await UniTask.Delay(100, cancellationToken: token);
            }
        
        }
        private async UniTaskVoid StartShootingLaser()
        {
            _isAttacking = true;
            var token = _cts.Token;

            try
            {
                await _animation.PlayPreAttack().AttachExternalCancellation(token);

                _movement.LookAt(_data.Target.transform.position);

                await FireLaser(token);

                // EndLaserSequence cleans up FX — let it finish even if we're transitioning
                _animation.HideRune();

                _attackCooldown = Mathf.Floor(1f / _stat.AttackSpeed);
                OnLaserFinished?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // State was exited mid-laser (stagger, death, etc.)
                // OnExit already cleaned up _data and flags — nothing to do here
            }
            finally
            {
                _isAttacking = false;
            }
        }
    }
}
