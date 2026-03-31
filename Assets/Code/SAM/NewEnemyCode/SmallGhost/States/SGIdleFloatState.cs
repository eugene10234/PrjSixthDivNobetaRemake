using Nobeta.Sam.Bullet;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// SmallGhost's only active state — floats and fires.
    ///
    /// Phase flow:
    ///   Drifting  — floats freely, closes distance if beyond ChaseRange,
    ///               ticks cooldown, attempts attack when in AttackRange with LOS
    ///   Stopping  — halts briefly to aim before firing
    ///   Firing    — fires bullet if LOS still clear, resets cooldown, back to Drifting
    ///
    /// All range thresholds read from SmallGhostStat so designers can tune
    /// ChaseRange, AttackRange, and AttackSpeed without touching code.
    /// </summary>
    public class SGIdleFloatState : IState
    {
        private readonly SGMovementActor   _movement;
        private readonly SGAnimationActor  _animation;
        private readonly SGPerceptionActor _perception;
        private readonly BaseEnemyData     _data;
        private readonly SmallGhostStat    _stat;
        private readonly EnemyBulletSystem      _bulletSystem;
        private readonly BulletData        _spellData;
        private readonly Transform         _muzzlePoint;

        // ── Attack sub-state ──────────────────────────────────────────────────

        private enum AttackPhase { Drifting, Stopping, Firing }
        private AttackPhase _phase;
        private float       _attackCooldown;
        private float       _stopTimer;
        private const float StopDuration = 0.5f; // seconds the ghost halts before firing
        private CancellationToken _token;
        // ── LOS ───────────────────────────────────────────────────────────────

        private int _losBlockMask;

        [Inject]
        public SGIdleFloatState(
            SGMovementActor   movement,
            SGAnimationActor  animation,
            SGPerceptionActor perception,
            BaseEnemyData     data,
            SmallGhostStat    stat,         // concrete type — reads ChaseRange
            EnemyBulletSystem bulletSystem,
            BulletData        spellData,
            Transform         muzzlePoint)
        {
            _movement     = movement;
            _animation    = animation;
            _perception = perception;
            _data         = data;
            _stat         = stat;
            _bulletSystem = bulletSystem;
            _spellData    = spellData;
            _muzzlePoint  = muzzlePoint;
        }

        public void ActivateToken(CancellationToken token)
        {
            _token = token;
        }
        public void OnEnter()
        {
            _phase          = AttackPhase.Drifting;
            _stopTimer      = 0f;

            // Stagger initial shot across instances so three SGs don't fire simultaneously
            _attackCooldown = UnityEngine.Random.Range(0f, 1f / _stat.AttackSpeed);

            _losBlockMask = (1 << LayerMask.NameToLayer("Enemy"))
                          | (1 << LayerMask.NameToLayer("Obstacle"));
        }

        public void OnFixedTick()
        {
            if (_data.Target != null && !_data.Target)
                _data.Target = null;

            if (_data.Target == null)
            {
                var player = _perception.ScanForPlayer(_stat.AggroRange);
                if (player != null)
                {
                    _data.Target = player;
                }
                else
                {
                    _movement.MoveToward(Vector3.zero);
                    return;
                }
            }

            float dist = Vector3.Distance(_movement.Position,
                                           _data.Target.transform.position);

            switch (_phase)
            {
                case AttackPhase.Drifting:
                    DriftingTick(dist);
                    break;

                case AttackPhase.Stopping:
                    StoppingTick();
                    break;

                case AttackPhase.Firing:
                    FiringTick();
                    break;
            }
        }

        public void OnExit()
        {
            _phase     = AttackPhase.Drifting;
            _stopTimer = 0f;
        }

        // ── Phase handlers ────────────────────────────────────────────────────

        private void DriftingTick(float dist)
        {
            // Pass real target so SGMovementActor blends toward player
            // when beyond ChaseRange (_stat.ChaseRange read inside actor)
            _movement.MoveToward(_data.Target.transform.position);

            _attackCooldown -= Time.fixedDeltaTime;
            if (_attackCooldown > 0f) return;

            if (dist <= _stat.AttackRange && HasLOS(_data.Target.transform.position))
            {
                // In range, LOS clear — commit to attack
                _phase     = AttackPhase.Stopping;
                _stopTimer = 0f;
            }
            else
            {
                // Reset cooldown and keep drifting
                _attackCooldown = 1f / _stat.AttackSpeed;

                if (dist <= _stat.AttackRange)
                {
                    // Inside attack range but LOS blocked by enemy or obstacle —
                    // nudge sideways to find a clear angle
                    Vector3 toTarget = (_data.Target.transform.position
                                      - _movement.Position).normalized;
                    Vector3 sideDir  = Vector3.Cross(toTarget, Vector3.up).normalized;

                    // Alternate left/right using noise so it doesn't always go the same way
                    float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0f);
                    sideDir    *= noise > 0.5f ? 1f : -1f;

                    _movement.ApplyAvoidance(sideDir);
                }
                // else: beyond AttackRange — SGMovementActor already closing distance
                // via ChaseRange blend, nothing extra needed here
            }
        }

        public event Action OnPreWarming;
        private bool isPreWarming;
        private void StoppingTick()
        {
            if (!isPreWarming)
            { 
                OnPreWarming?.Invoke();
                isPreWarming = true;
            }
            _movement.Stop();
            _movement.LookAt(_data.Target.transform.position, requireVelocity: false);
            _stopTimer += Time.fixedDeltaTime;
            if (_stopTimer >= StopDuration)
                _phase = AttackPhase.Firing;
        }

        public event Action OnFinishedFiring;
        private void FiringTick()
        {
            // Second LOS check — player may have moved behind cover during stop
            if (HasLOS(_data.Target.transform.position))
            {
                _movement.LookAt(_data.Target.transform.position);
                _animation.PlayAttack();
                _bulletSystem.FireAt(
                    origin:         _muzzlePoint.position,
                    targetPosition: _data.Target.transform.position,
                    data:           _spellData,
                    instigator:     _movement.gameObject,
                    homingTarget:   _spellData.IsHoming ? _data.Target.transform : null);
            }

            _attackCooldown = 1f / _stat.AttackSpeed;
            OnFinishedFiring?.Invoke();
            isPreWarming = false;
            _phase          = AttackPhase.Drifting;
        }

        // ── LOS ───────────────────────────────────────────────────────────────

        private bool HasLOS(Vector3 targetPos)
        {
            Vector3 origin    = _muzzlePoint.position;
            Vector3 targetUp  = targetPos + Vector3.up * 1f;
            Vector3 direction = (targetUp - origin).normalized;
            float   distance  = Vector3.Distance(origin, targetUp);

            // SphereCast gives tolerance so thin cover doesn't always block
            return !Physics.SphereCast(origin, 0.3f, direction, out _,
                                        distance, _losBlockMask);
        }
    }
}
