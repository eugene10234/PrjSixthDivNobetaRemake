using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// SmallGhost state machine.
    /// Stripped down compared to other enemies — SG never chases or attacks melee.
    /// It floats, shoots, staggers, and dies.
    ///
    /// State set:
    ///   SGIdleFloatState  ← primary state, runs the entire time it is alive
    ///   StaggerState      ← shared
    ///   VictoryState      ← shared
    ///   DeathState        ← shared
    /// </summary>
    public class SGStateMachine : BaseStateMachine
    {
        private SmallGhostData   _data;
        private SGIdleFloatState _floatState;
        private StaggerState     _stagger;
        private SGRune           _sgRune;

        [Inject]
        public void Construct(
            SmallGhostData data,
            SGIdleFloatState floatState,
            StaggerState stagger,
            VictoryState victory,
            DeathState death,
            SGRune sgRune)
        {
            _data    = data;
            _floatState   = floatState;
            _stagger = stagger;
            _sgRune = sgRune;

            RegisterState(floatState);
            RegisterState(stagger);
            RegisterState(victory);
            RegisterState(death);

            WireTransitions();

            Debug.Log("SG StateMachine Ready");
        }

        // ── IStartable ────────────────────────────────────────────────────────

        public override void Start()
        {
            TransitionTo<SGIdleFloatState>();
        }

        // ── IEnemyStateMachine ────────────────────────────────────────────────

        public override void OnCombatAlert(GameObject target)
        {
            // SG doesn't chase but does need the target so it knows who to shoot
            _data.Target = target;
        }

        public override void OnTargetLost()
        {
            _data.Target = null;
        }

        public override void OnHit(Vector3 hitPosition)
        {
            if (_data.IsDead) return;
            if (_data.isImmuneStagger) return;
            _stagger.Activate(hitPosition);
            TransitionTo<StaggerState>();
        }

        public override void OnDeath()
        {
            _sgRune.PlayDeathSequence();
            TransitionTo<DeathState>();
        }

        public override void OnVictory()
        {
            TransitionTo<VictoryState>();
        }

        // ── Transitions ───────────────────────────────────────────────────────

        private void WireTransitions()
        {
            // Stagger → back to floating when recovery finishes
            _stagger.OnStaggerFinished += () => TransitionTo<SGIdleFloatState>();

            _floatState.OnPreWarming += () =>
            {
                _sgRune.Show();
            };

            _floatState.OnFinishedFiring += () =>
            {
                _sgRune.Hide();
            };
        }

        public override void Dispose() { }
    }
}
