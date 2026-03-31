using System;
using UnityEngine;
using VContainer;
using static UnityEditor.Rendering.ShadowCascadeGUI;

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
        private MageGhostData        _data;
        private MGSleepState        _sleep;
        private IdleState            _idle;
        private StaggerState      _stagger;
        private MGAttackState      _attack;
        private MGSummonState      _summon;
        private Barrier  _barrier;
        

        [Inject]
        public void Construct(
            MageGhostData        data,
            IdleState            idle,
            StaggerState      stagger,
            MGSleepState        sleep,
            MGAttackState      attack,
            MGSummonState      summon,
            VictoryState      victory,
            DeathState          death,
            Barrier           barrier)
        {
            _data    = data;
            _sleep = sleep;
            _summon = summon;
            _stagger = stagger;
            _idle    = idle;
            _attack  = attack;
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
            // Scan for player on hit — if we don't have a target yet, find one now
            // (handled inside StaggerState.OnExit → OnStaggerFinished → back to chase)

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
            //summon transition
            _summon.OnSummonCompleted += () =>
            {
                _barrier.Show();
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
            };
        }

        public override void Dispose()
        {
            // Events are wired to lambdas that capture this machine — no manual unwire needed
            // since the entire scope is disposed together. Safe to leave empty.
        }
    }
}
