using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared victory state — plays the victory animation and halts movement.
    /// Terminal state: no transitions out.
    /// </summary>
    public class VictoryState : IState
    {
        private readonly IMovementActor  _movement;
        private readonly IAnimationActor _animation;

        [Inject]
        public VictoryState(IMovementActor movement, IAnimationActor animation)
        {
            _movement  = movement;
            _animation = animation;
        }

        public void OnEnter()
        {
            _movement.Stop();
            _animation.PlayVictory();
        }

        public void OnFixedTick() { }
        public void OnExit()      { }
    }
}
