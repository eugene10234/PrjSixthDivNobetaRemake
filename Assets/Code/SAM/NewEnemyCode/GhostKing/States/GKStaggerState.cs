using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King stagger state with stagger guard.
    ///
    /// Stagger Guard rules:
    ///   - The GK absorbs up to <see cref="MaxStaggerHits"/> hits within <see cref="GuardWindow"/> seconds.
    ///   - On each absorbed hit the counter increments but no stagger animation plays.
    ///   - Once the counter reaches <see cref="MaxStaggerHits"/>, the guard breaks:
    ///     the real stagger plays, counter resets, window resets.
    ///   - If the window expires before threshold is reached the counter resets silently.
    ///
    /// IMPORTANT: The state machine must call <see cref="TickGuard"/> every FixedTick
    /// regardless of which state is currently active, so the window drains correctly
    /// while the GK is chasing, attacking, etc.
    ///
    /// Only transition INTO this state when <see cref="TryStagger"/> returns true.
    /// </summary>
    public class GKStaggerState : IState
    {
        // ── Guard config ──────────────────────────────────────────────────────
        private const int MaxStaggerHits = 10;
        private const float GuardWindow = 3f;
        private const float StaggerDuration = 0.4f;

        // ── Dependencies ──────────────────────────────────────────────────────
        private readonly IMovementActor _movement;
        private readonly IAnimationActor _animation;

        // ── Runtime state ─────────────────────────────────────────────────────
        private int _hitCounter;
        private float _windowTimer;
        private float _staggerTimer;

        public event Action OnStaggerFinished;

        [Inject]
        public GKStaggerState(IMovementActor movement, IAnimationActor animation)
        {
            _movement = movement;
            _animation = animation;
        }

        // ── Guard API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Call this from GKStateMachine.FixedTick() every frame regardless of
        /// current state so the guard window drains while the GK is not staggered.
        /// </summary>
        public void TickGuard()
        {
            if (_windowTimer > 0f)
            {
                _windowTimer -= Time.fixedDeltaTime;
                if (_windowTimer <= 0f)
                {
                    _hitCounter = 0;
                    _windowTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Returns true when the guard breaks and the real stagger should begin.
        /// Call this from your hit handler before transitioning into GKStaggerState.
        /// </summary>
        public bool TryStagger(Vector3 hitPosition)
        {
            if (_windowTimer <= 0f)
                _hitCounter = 0;

            _windowTimer = GuardWindow;
            _hitCounter++;

            if (_hitCounter >= MaxStaggerHits)
            {
                _hitCounter = 0;
                _windowTimer = 0f;
                return true;
            }

            return false;
        }

        // ── IState ────────────────────────────────────────────────────────────

        public void OnEnter()
        {
            _staggerTimer = StaggerDuration;
            _movement.Stop();
            _animation.PlayHit();
        }

        public void OnFixedTick()
        {
            // Only stagger timer here — guard window is handled by TickGuard()
            _staggerTimer -= Time.fixedDeltaTime;
            if (_staggerTimer <= 0f)
                OnStaggerFinished?.Invoke();
        }

        public void OnExit() { }
    }
}
