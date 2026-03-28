using System;
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
        private IdleState           _idle;
        private ChaseState          _chase;
        private MGAttackState      _attack;
        private StaggerState        _stagger;

        [Inject]
        public void Construct(
            MageGhostData data,
            IdleState           idle,
            ChaseState          chase,
            MGAttackState      attack,
            StaggerState        stagger,
            VictoryState        victory,
            DeathState          death)
        {
            _data    = data;
            _idle    = idle;
            _chase   = chase;
            _attack  = attack;
            _stagger = stagger;

            RegisterState(idle);
            RegisterState(chase);
            RegisterState(attack);
            RegisterState(stagger);
            RegisterState(victory);
            RegisterState(death);

            WireTransitions();

            Debug.Log("MG StateMachine Ready");
        }

        // ── VContainer IStartable ─────────────────────────────────────────────

        public override void Start()
        {
            _idle.Mode = IdleState.IdleMode.Stop;
            TransitionTo<IdleState>();
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

            // Scan for player on hit — if we don't have a target yet, find one now
            // (handled inside StaggerState.OnExit → OnStaggerFinished → back to chase)
            TransitionTo<StaggerState>();
        }

        public override void OnDeath()
        {
            TransitionTo<DeathState>();
        }

        public override void OnVictory()
        {
            TransitionTo<VictoryState>();
        }

        // ── Transition wiring ─────────────────────────────────────────────────

        private void WireTransitions()
        {
            // Chase → attack when close enough
            _chase.OnInAttackRange += () => TransitionTo<MGAttackState>();

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
            };
        }

        public override void Dispose()
        {
            // Events are wired to lambdas that capture this machine — no manual unwire needed
            // since the entire scope is disposed together. Safe to leave empty.
        }
    }
}
