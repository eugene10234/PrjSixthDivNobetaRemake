using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Events;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// MageGhost state machine.
    /// Extends BaseStateMachine — registers all states MG uses and wires
    /// their transition events. Each state raises an event; this machine
    /// listens and calls TransitionTo&lt;T&gt;(). States never hold a reference
    /// back to the machine.
    ///
    /// MG state set:
    ///   IdleState (Stop / GoHome sub-modes)
    ///   ChaseState
    ///   MGAttackState   ← MG-specific
    ///   StaggerState
    ///   VictoryState
    ///   DeathState
    /// </summary>
    public class MGStateMachine : BaseStateMachine
    {
        private MageGhostData _data;
        private MGSleepState _sleep;
        private IdleState _idle;
        private StaggerState _stagger;
        private MGAttackState _attack;
        private MGSummonState _summon;
        private Barrier _barrier;
        private IEventBus _eventBus;


        [Inject]
        public void Construct(
            MageGhostData data,
            IdleState idle,
            StaggerState stagger,
            MGSleepState sleep,
            MGAttackState attack,
            MGSummonState summon,
            VictoryState victory,
            DeathState death,
            Barrier barrier)
        {
            _eventBus = EventBusProvider.GetEventBus();
            _data = data;
            _sleep = sleep;
            _summon = summon;
            _stagger = stagger;
            _idle = idle;
            _attack = attack;
            _barrier = barrier;


            RegisterState(idle);
            RegisterState(stagger);
            RegisterState(attack);
            RegisterState(summon);
            RegisterState(sleep);
            RegisterState(victory);
            RegisterState(death);



            WireTransitions();

            Debug.Log("MG StateMachine Ready");
        }

        // ── VContainer IStartable ─────────────────────────────────────────────

        public override void Start()
        {
            _idle.Mode = IdleState.IdleMode.Stop;
            TransitionTo<MGSummonState>();
        }



        // ── IEnemyStateMachine ────────────────────────────────────────────────

        public override void OnCombatAlert(GameObject target)
        {
            _data.Target = target;
            _idle.Mode = IdleState.IdleMode.Stop; // reset in case we were going home
            TransitionTo<ChaseState>();
        }

        public override void OnTargetLost()
        {
            _data.Target = null;
            _idle.Mode = IdleState.IdleMode.GoHome;
            TransitionTo<IdleState>();
        }

        public override void OnHit(Vector3 hitPosition)
        {
            if (_data.IsDead) return;
            if (CurrentState is MGSummonState) return; // summon is uninterruptible
            _stagger.Activate(hitPosition);
            _eventBus.Publish(new BossHealthUpdateEvent
            {
                HealthRatio = _data.HealthPercent,
                BossName = _data.Name,
            });
            if (CurrentState == _stagger) return;
            TransitionTo<StaggerState>();
        }

        public override void OnDeath()
        {
            _eventBus.Publish(new BossInfoDisableEvent
            {
            });
            _eventBus.Publish(new EnemyDeathEvent { Reward = _data.Reward });
            TransitionTo<DeathState>();
        }

        public override void OnVictory()
        {
            _eventBus.Publish(new EnemyVictoryEvent { });
            TransitionTo<VictoryState>();
        }


        // ── Transition wiring ─────────────────────────────────────────────────

        private void WireTransitions()
        {
            //summon transition
            _summon.OnSummonCompleted += () =>
            {
                //_barrier.Show();
                TransitionTo<IdleState>();
            };
            _summon.OnAllSummonedDied += () =>
            {
                _barrier.Hide();
                TransitionTo<MGSleepState>();
            };

            //sleep

            _sleep.OnSleepFinished += () =>
            {
                _barrier.Show();
                TransitionTo<MGSummonState>();
            };


            // Attack → chase when player moves out of attack range
            _attack.OnTargetOutOfRange += () => TransitionTo<IdleState>();
            _attack.OnLaserFinished += () => TransitionTo<IdleState>();
            //stagger
            _stagger.OnStaggerFinished += () =>
            {
                if (_data.Target != null) TransitionTo<MGSleepState>();
                else
                {
                    _idle.Mode = IdleState.IdleMode.GoHome;
                    TransitionTo<IdleState>();
                }
            };

            // Idle GoHome → stop when arrived (player spotted mid-return is also handled inside IdleState)
            _idle.OnPlayerDetected += () =>
            {
                // Player was spotted mid-return — jump straight into combat
                _eventBus.Publish(new BossInfoEnableEvent
                {
                    BossName = _data.Name,
                    HealthRatio = _data.HealthPercent,
                });
                if (_data.Target != null) TransitionTo<MGAttackState>();
                else
                {
                    _idle.Mode = IdleState.IdleMode.Stop;
                    TransitionTo<IdleState>();
                }
            };

            _idle.OnReturnedHome += () =>
            {
                _idle.Mode = IdleState.IdleMode.Stop;
                _eventBus.Publish(new BossInfoDisableEvent
                {
                });
            };
        }

        public override void Dispose()
        {
            // Events are wired to lambdas that capture this machine — no manual unwire needed
            // since the entire scope is disposed together. Safe to leave empty.
        }
    }
}
