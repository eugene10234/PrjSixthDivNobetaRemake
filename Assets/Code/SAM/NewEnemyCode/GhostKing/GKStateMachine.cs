using Cysharp.Threading.Tasks;
using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Events;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King state machine — structure mirrors MGStateMachine.
    /// All transition wiring is left as TODO stubs; fill them in alongside the boss design.
    ///
    /// Implements IFixedTickable so VContainer drives OnFixedTick automatically.
    /// Implements IStartable so the initial state is entered after all injections complete.
    /// </summary>
    public class GKStateMachine : IEnemyStateMachine, IStartable, IFixedTickable, IDisposable
    {
        // ── States ────────────────────────────────────────────────────────────
        private readonly GKSleepState _sleepState;
        private readonly GKChaseState _chaseState;
        private readonly GKAttackState _attackState;
        private readonly GKSummonState _summonState;
        private readonly GKTransitionState _transitionState;
        private readonly GKStaggerState _staggerState;
        private readonly VictoryState _victoryState;
        private readonly DeathState _deathState;
        private readonly Barrier _barrier;
        private readonly GhostKingData _data;
        private bool canCountDownAttack;
        private IEventBus _eventBus;
        private CancellationTokenSource _cts;
        private IState _currentState;

        [Inject]
        public GKStateMachine(
            GKSleepState sleepState,
            GKChaseState chaseState,
            GKAttackState attackState,
            GKSummonState summonState,
            GKTransitionState transitionState,
            GKStaggerState staggerState,
            VictoryState victoryState,
            DeathState deathState,
            GhostKingData data,
            Barrier barrier)
        {
            _eventBus = EventBusProvider.GetEventBus();
            _sleepState = sleepState;
            _chaseState = chaseState;
            _attackState = attackState;
            _summonState = summonState;
            _transitionState = transitionState;
            _staggerState = staggerState;
            _victoryState = victoryState;
            _deathState = deathState;
            _barrier = barrier;
            _data = data;

            WireTransitions();
         }

        // ── Transition wiring ─────────────────────────────────────────────────

        private void WireTransitions()
        {
            //stagger
            _staggerState.OnStaggerFinished += () => TransitionTo(_chaseState);
            // Sleep → Chase when the player triggers the arena
            _sleepState.OnWakeUp += () => TransitionTo(_chaseState);
            _sleepState.OnAnnounced += () => TransitionTo(_summonState);
            _sleepState.OnSleepFinished += () => TransitionTo(_chaseState);
            _sleepState.OnCombatStart += () =>
            {
                _eventBus.Publish(new BossInfoEnableEvent
                {
                    BossName = _data.Name,
                    HealthRatio = _data.HealthPercent,
                });
            };
            //transition -> summon always
            _transitionState.OnTransitionFinished += () => TransitionTo(_summonState);
            _summonState.OnSummonCompleted += () =>
            {
                _attackState.currentMode = GKAttackState.AttackMode.darkness;
                TransitionTo(_attackState);
            };
            //AllSummonedDied
            _summonState.OnAllSummonedDied += () =>
            {
                _barrier.Hide();
                _attackState.OnExit();
                _sleepState.currentMode = GKSleepState.sleepmode.catatonic;
                TransitionTo(_sleepState);
            };
            //Chase -> Attack
            _chaseState.OnInMeleeAttackRange += () =>
            {
                _attackState.currentMode = GKAttackState.AttackMode.wind;
                TransitionTo(_attackState, requester: _chaseState);
            };
            _chaseState.OnOutofMeleeAttackRange += () =>
            {
                _attackState.currentMode = GKAttackState.AttackMode.fire;
                TransitionTo(_attackState, requester: _chaseState);
            };
            //attackstate
            _attackState.OnFinishedCasting += () =>
            {
                _attackState.currentMode = GKAttackState.AttackMode.wind;
                TransitionTo(_chaseState, requester: _attackState);
            };
            _attackState.OnFinishWindAttack += () =>
            {
                if (_currentState != _attackState) return;
                _attackState.currentMode = GKAttackState.AttackMode.fire;
            };
        }

      

        // ── IStartable ────────────────────────────────────────────────────────

        void IStartable.Start()
        {
            _sleepState.currentMode = GKSleepState.sleepmode.idle;
            TransitionTo(_sleepState);
        }

        // ── IFixedTickable ────────────────────────────────────────────────────

        private bool hasPhased;
        public void FixedTick()
        {
            if (_data.HealthPercent <= 0.5 && !hasPhased)
            { 
                hasPhased = true;
                _barrier.Show();
                _attackState.ShiftGearTwo();
                TransitionTo(_transitionState);
            }
            
            // TickGuard runs unconditionally so the 4-second window drains
            // even while the GK is chasing, attacking, etc.
            _staggerState.TickGuard();
            _currentState?.OnFixedTick();
        }

        // ── IDisposable ───────────────────────────────────────────────────────

        public void Dispose() { }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void TransitionTo(IState next, IState requester = null)
        {
            if (next == null) return;
            if (requester != null && _currentState != requester) return;
            _currentState?.OnExit();
            _currentState = next;
            _currentState.OnEnter();
        }

        public void OnCombatAlert(GameObject target)
        {
            //no one calls
            throw new NotImplementedException();
        }

        public void OnTargetLost()
        {
            ////no one calls
            throw new NotImplementedException();
        }

        public void OnHit(Vector3 hitPosition)
        {
            if (_data.IsDead) return;
            _eventBus.Publish(new BossHealthUpdateEvent
            {
                HealthRatio = _data.HealthPercent,
                BossName = _data.Name,
            });
            if (!_data.isImmuneStagger && _staggerState.TryStagger(hitPosition))
            {
                TransitionTo(_staggerState, requester:_chaseState);
            }
        }

        public void OnVictory()
        {
            _eventBus.Publish(new EnemyVictoryEvent { });
            TransitionTo(_victoryState);
        }

        public void OnDeath()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            DeathSequence().Forget();
        }

        private async UniTaskVoid DeathSequence ()
        { 
            TransitionTo(_deathState);
            _attackState.StopLightning();
            _eventBus.Publish(new BossInfoDisableEvent
            {
            });
            _eventBus.Publish(new EnemyDeathEvent { Reward = _data.Reward });
            await UniTask.Delay(6500, cancellationToken: default);
            _eventBus.Publish(new DialogEvent { ID = 4 });           
        }
    }
}