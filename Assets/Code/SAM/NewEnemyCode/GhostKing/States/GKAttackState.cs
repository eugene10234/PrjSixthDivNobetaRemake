using Cysharp.Threading.Tasks;
using Nobeta.Sam.Bullet;
using System;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King attack state.
    /// TODO: implement boss attack pattern (multi-phase, area attacks, etc.)
    /// </summary>
    public class GKAttackState : IState
    {
        public enum AttackMode
        { 
            fire,
            wind,
            darkness,
        }
        public AttackMode currentMode;
        private readonly GKMovementActor _movement;
        private readonly GKAnimationActor _animation;
        private readonly GKPerceptionActor _perception;
        private readonly EnemyStat _stat;
        private readonly BaseEnemyData _data;
        private readonly EnemyBulletSystem _bulletSystem;
        private readonly BulletData _spellData;
        private Transform _muzzlePoint;

        private bool _forceExited;
        private GameObject _player;
        public event Action OnTargetOutOfRange;
        public event Action OnAttackSequenceStarted;
        public event Action OnAttackFinished;
        public bool onCoolDown = false;
        private CancellationTokenSource _stateCts;   // lives for the whole state entry
        private CancellationTokenSource _sequenceCts; // per-sequence, for restarts

        [Inject]
        public GKAttackState(
            GKMovementActor movement,
            GKAnimationActor animation,
            GKPerceptionActor perception,
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
            _forceExited = false;
            _player = _perception.ScanForPlayer(50) ?? null;
            _stateCts = new CancellationTokenSource();
            _muzzlePoint = _movement.muzzlePoint;
            attackCoolDown = (1 / _stat.AttackSpeed);
            onCoolDown = true;
        }
        public void OnFixedTick() 
        {
            if (onCoolDown)
            {
                AttackCoolDownTick();
                //_movement.MoveToward(_player.transform.position);
                return;
            } 
            switch (currentMode)
            {
                case AttackMode.fire:
                    _movement.LookAt(_player.transform.position);
                    if (!isCasting)
                    { 
                        _data.isImmuneStagger = true;
                        StartCastingFireBall().Forget();
                    }
                    break;
                case AttackMode.wind:
                    _movement.LookAt(_player.transform.position);
                    if (!isWinding)
                    {
                        _data.isImmuneStagger = true;
                        StartMeleeSequence().Forget();
                    }
                    break;
                case AttackMode.darkness:
                    if (!isLasering)
                    {
                        BeginLaserSequence().Forget();
                    }
                    break;
            }
        }
        public void OnExit() 
        {
            _forceExited = true;
            _stateCts?.Cancel();
            _stateCts?.Dispose();
            _stateCts = null;
            isLasering = false;
            isCasting = false;
            isWinding = false;
            _animation.DeactivateLaser();
            _animation.AE_DeactivateWindFX();
            _animation.DeactivateFireRune();
            _data.isImmuneStagger = false;
        }

        private bool isPhaseTwo;
        private CancellationTokenSource _lightningLoopCts;
        public void ShiftGearTwo()
        {
            MaxFireBallCount *= 2;
            _animation.ShowAngryFace();
            isPhaseTwo = true;

            // Start the background lightning loop, independent of state transitions
            _lightningLoopCts = new CancellationTokenSource();
            var token = _lightningLoopCts.Token;
            StartLightning(token, delayMs: 0).Forget();
            StartLightning(token, delayMs: 666).Forget();
            StartLightning(token, delayMs: 1333).Forget();
        }


        public async UniTaskVoid StartLightning(CancellationToken token, int delayMs = 0)
        {
            if (delayMs > 0)
                await UniTask.Delay(delayMs, cancellationToken: token);
            while (!token.IsCancellationRequested)
            {
                Vector3 loc = _perception.GetApproximatePlayerPosition(_player, _data.AttackRange);
                await _animation.InstantiateLightning(loc);
                await UniTask.Delay(1000, cancellationToken: token); // tune interval
            }
        }

        public void StopLightning()
        {
            _lightningLoopCts?.Cancel();
            _lightningLoopCts?.Dispose();
            _lightningLoopCts = null;
        }




        //Wind
        public event Action OnFinishWindAttack;
        private bool isWinding;
        private async UniTask StartMeleeSequence()
        {
            isWinding = true;
            _sequenceCts?.Cancel();
            _sequenceCts?.Dispose();
            _sequenceCts = CancellationTokenSource.CreateLinkedTokenSource(_stateCts.Token);
            var token = _sequenceCts.Token;
            try
            {
                _movement.Stop();
                await _animation.StartPreMeleeAttackMotion(token);
                _animation.StartWindAttack();
                _animation.EndPreAttack();
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                onCoolDown = true;
                attackCoolDown = (1f / _stat.AttackSpeed);
                isWinding = false;
                _data.isImmuneStagger = false;
                if (!_forceExited)          // ← guard
                    OnFinishWindAttack?.Invoke();
            }
        }






        //FIRE

        private bool isCasting;
        private int FireBallDelayTime = 500;
        public int MaxFireBallCount = 3;
        private async UniTask CastFireBall(CancellationToken token)
        {
            for (int i = 0; i <= MaxFireBallCount; i++)
            {
                if (token.IsCancellationRequested || _player == null) return;
                _bulletSystem.FireAt(
                   origin: _muzzlePoint.position,
                   targetPosition: _player.transform.position,
                   data: _spellData,
                   instigator: _movement.gameObject,
                   homingTarget: _spellData.IsHoming ? _player.transform : null);

                await UniTask.Delay(FireBallDelayTime, cancellationToken: token);
            }

        }

        public event Action OnFinishedCasting;
        private async UniTaskVoid StartCastingFireBall()
        {
            isCasting = true;
            _sequenceCts?.Cancel();
            _sequenceCts?.Dispose();
            _sequenceCts = CancellationTokenSource.CreateLinkedTokenSource(_stateCts.Token);
            var token = _sequenceCts.Token;

            try
            {
                await _animation.ActivateFireRune(token);
                _movement.Stop();
                await CastFireBall(token);

                // EndLaserSequence cleans up FX — let it finish even if we're transitioning
                _animation.DeactivateFireRune();
            }
            catch (OperationCanceledException)
            {
               
            }
            finally
            {
                onCoolDown = true;
                _animation.DeactivateFireRune();
                isCasting = false;
                _data.isImmuneStagger = false;
                if (!_forceExited)          // ← guard
                    OnFinishedCasting?.Invoke();
            }
        }


        //DARKNESS
        private bool isLasering;
        private float _laserCountDown;
        private float _laserTime = 3;

        private async UniTask BeginLaserSequence()
        {
            isLasering = true;

            // Cancel previous sequence only, not the state token
            _sequenceCts?.Cancel();
            _sequenceCts?.Dispose();
            _sequenceCts = CancellationTokenSource.CreateLinkedTokenSource(_stateCts.Token);
            var token = _sequenceCts.Token;

            try
            {
                _laserCountDown = 0;
                //_movement.ResetMuzzlePoint();
                await _animation.ActivateLaserRune(token);
                await _movement.ReturnLaserPoint();
                await _movement.HorrizontalFollow(_player, token);

                _animation.ActivateLaser(token);
                if (isPhaseTwo)
                _movement.HorrizontalFollow(_player, token).Forget();
                while (_laserCountDown <= _laserTime)
                {
                    token.ThrowIfCancellationRequested();
                    if (!isPhaseTwo)
                    { 
                        _movement.Stop();
                    }
                    _laserCountDown += Time.fixedDeltaTime;
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Laser Sequence Interrupted!");
            }
            finally
            {
                onCoolDown = true;
                _animation.DeactivateLaser();
                isLasering = false;  // ← also reset here so OnFixedTick can retry if needed
                _data.isImmuneStagger = false;
            }
        }

        public void forceDeactivateLaser()
        {
            _animation.DeactivateLaser();
        }
        private float attackCoolDown;
        public void AttackCoolDownTick()
        { 
            attackCoolDown -= Time.fixedDeltaTime;
            if (attackCoolDown <= 0)
            {
                onCoolDown = false;
                attackCoolDown = (1f / _stat.AttackSpeed);
            }
        }

    }
}
