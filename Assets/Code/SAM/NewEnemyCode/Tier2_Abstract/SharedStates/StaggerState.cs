using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared stagger state — plays the hit reaction then raises OnStaggerFinished.
    /// SmallGhost does NOT register this state, so it is effectively skipped for that type.
    ///
    /// The state machine stores the hitPosition from OnHit and passes it via Activate()
    /// before transitioning here.
    /// </summary>
    public class StaggerState : IState
    {
        private readonly IMovementActor   _movement;
        private readonly IAnimationActor  _animation;

        private Vector3 _hitPosition;
        private float   _staggerTimer;
        private const float StaggerDuration = 0.4f;

        public event Action OnStaggerFinished;

        [Inject]
        public StaggerState(IMovementActor movement, IAnimationActor animation)
        {
            _movement  = movement;
            _animation = animation;
        }

        public void Activate(Vector3 hitPosition)
        {
            _hitPosition = hitPosition;
        }

        public void OnEnter()
        {
            _staggerTimer = StaggerDuration;
            _movement.Stop();
            _animation.PlayHit();
        }

        public void OnFixedTick()
        {
            _staggerTimer -= Time.fixedDeltaTime;
            if (_staggerTimer <= 0f)
                OnStaggerFinished?.Invoke();
        }

        public void OnExit() { }
    }
}
