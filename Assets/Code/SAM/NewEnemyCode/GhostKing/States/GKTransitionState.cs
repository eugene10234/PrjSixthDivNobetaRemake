using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King phase-transition state.
    /// TODO: implement cinematic lock, invincibility window, phase-change VFX.
    /// </summary>
    public class GKTransitionState : IState
    {
        private GhostKingData _gkData;
        private GKAnimationActor _animation;
        private GKMovementActor _movement;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(GhostKingData mgData, SmallGhostFactory smallGhostFactory, GKAnimationActor animation, GKMovementActor movement)
        {
            _gkData = mgData;
            _animation = animation;
            _movement = movement;
        }

        public void OnEnter() { }
        public void OnFixedTick() 
        {
            if (!isTransitioning)
            { 
                StartTransitioning().Forget();
            }


        }
        

        public void OnExit() 
        {
            isTransitioning = false;
        }

        private bool isTransitioning;
        public event Action OnTransitionFinished;
        private async UniTask StartTransitioning()
        {
            isTransitioning = true;
            var success = await _movement.ReturnStagePoint();
            if (success)
            {
                OnTransitionFinished?.Invoke();
            }
        }
    }
}
