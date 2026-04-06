using Nobeta.EUG.Events;
using Nobeta.EUG.Interfaces;
using Nobeta.Sam.Events;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// GiantBeastGhost state machine.
    /// Extends BaseStateMachine — registers all states GBG uses and wires
    /// their transition events. Each state raises an event; this machine
    /// listens and calls TransitionTo&lt;T&gt;(). States never hold a reference
    /// back to the machine.
    ///
    /// GBG state set:
    ///   IdleState (Stop / GoHome sub-modes)
    ///   ChaseState
    ///   GBGAttackState   ← GBG-specific
    ///   StaggerState
    ///   VictoryState
    ///   DeathState
    /// </summary>
    public class GBGStateMachine : BaseStateMachine
    {
        private GiantBeastGhostData _data;
        private IdleState           _idle;
        private ChaseState          _chase;
        private GBGAttackState      _attack;
        private StaggerState        _stagger;
        private GBGDodgeState       _dodge;
        private IEventBus           _eventBus;

        [Inject]
        public void Construct(
            GiantBeastGhostData data,
            IdleState           idle,
            ChaseState          chase,
            GBGDodgeState       dodge,
            GBGAttackState      attack,
            StaggerState        stagger,
            VictoryState        victory,
            DeathState          death)
        {
            _eventBus = EventBusProvider.GetEventBus();
            _data    = data;
            _idle    = idle;
            _chase   = chase;
            _attack  = attack;
            _dodge   = dodge;
            _stagger = stagger;

            RegisterState(idle);
            RegisterState(chase);
            RegisterState(attack);
            RegisterState(stagger);
            RegisterState(victory);
            RegisterState(death);
            RegisterState(dodge);

            WireTransitions();

            Debug.Log("GBG StateMachine Ready");
        }

        // ── VContainer IStartable ─────────────────────────────────────────────

        public override void Start()
        {
            _idle.Mode = IdleState.IdleMode.Stop;
            TransitionTo<IdleState>();
        }
        // specific statemachine implementation

        public void OnDodge(Vector3 hitPosition, CancellationToken token)
        {
            if (_data.IsDead) return;
            TransitionTo<GBGDodgeState>();
            _dodge.SideStep(hitPosition, token).Forget();
        }

        // ── IEnemyStateMachine ────────────────────────────────────────────────

        public override void OnCombatAlert(GameObject target)
        {
            _data.Target = target;
            _idle.Mode   = IdleState.IdleMode.Stop; // reset in case we were going home
            TransitionTo<ChaseState>();
        }

        public override void OnTargetLost()
        {
            _data.Target = null;
            _idle.Mode   = IdleState.IdleMode.GoHome;
            TransitionTo<IdleState>();
        }

        public override void OnHit(Vector3 hitPosition)
        {
            if (_data.IsDead) return;
            _stagger.Activate(hitPosition);
            _eventBus.Publish(new BossHealthUpdateEvent
            {
                HealthRatio = _data.HealthPercent,
                BossName = _data.Name,
            });
            if (CurrentState == _stagger || _data.isImmuneStagger) return;
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
            //Dodge -> exit
            _dodge.OnFinishedDodging += () =>
            {
                if (_data.Target != null) TransitionTo<ChaseState>();
                else
                {
                    _idle.Mode = IdleState.IdleMode.GoHome;
                    TransitionTo<IdleState>();
                }
            };
            // Chase → attack when close enough
            _chase.OnInAttackRange += () => TransitionTo<GBGAttackState>();

            // Chase → idle/home when player escapes
            _chase.OnOutOfChaseRange += () =>
            {
                _idle.Mode = IdleState.IdleMode.GoHome;
                TransitionTo<IdleState>();
            };

            // Attack → chase when player moves out of attack range
            _attack.OnTargetOutOfRange += () => TransitionTo<ChaseState>();

            // Stagger → chase when recovery finishes (re-engage)
            _stagger.OnStaggerFinished += () =>
            {
                if (_data.Target != null) TransitionTo<ChaseState>();
                else
                {
                    _idle.Mode = IdleState.IdleMode.GoHome;
                    TransitionTo<IdleState>();
                }
            };

            // Idle GoHome → stop when arrived (player spotted mid-return is also handled inside IdleState)
            _idle.OnPlayerDetected += () =>
            {
                _eventBus.Publish(new BossInfoEnableEvent
                {
                    BossName = _data.Name,
                    HealthRatio = _data.HealthPercent,
                });
                // Player was spotted mid-return — jump straight into combat
                if (_data.Target != null) TransitionTo<ChaseState>();
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
