using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared death state — plays the dissolve sequence then destroys the GameObject.
    /// Terminal state: no transitions out.
    ///
    /// Uses async/await (UniTask) so the dissolve animation can finish before Destroy().
    /// The state machine must never transition away from DeathState once entered.
    /// </summary>
    public class DeathState : IState
    {
        private readonly IMovementActor  _movement;
        private readonly IAnimationActor _animation;
        private readonly GameObject      _gameObject;
        private const float DissolveDuration = 5f;

        [Inject]
        public DeathState(IMovementActor movement, IAnimationActor animation, GameObject gameObject)
        {
            _movement   = movement;
            _animation  = animation;
            _gameObject = gameObject;
        }

        public void OnEnter()
        {
            _movement.Stop();
            _animation.PlayDeath();
            _movement.DeathMovement();
            RunDeathSequence();
        }

        public void OnFixedTick() { }
        public void OnExit()      { }

        private async void RunDeathSequence()
        {
            await _animation.PlayDeathSequence(DissolveDuration);
            Object.Destroy(_gameObject);
        }
    }
}
