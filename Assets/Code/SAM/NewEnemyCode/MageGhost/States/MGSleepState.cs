using System;
using TMPro;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public class MGSleepState : IState
    {
        private float _sleepTimer;
        private const float sleepDuration = 3;

        private MGAnimationActor _animation;

        [Inject]
        public void construct(MGAnimationActor animation)
        { 
            _animation = animation; 
        }
        
        public void OnEnter()
        {
            _animation.PlaySleep();
        }

        public void OnExit()
        {
            _animation.PlayAwake();
        }

        public event Action OnSleepFinished;
        public void OnFixedTick()
        {
            _sleepTimer += Time.fixedDeltaTime;
            if (_sleepTimer >= sleepDuration)
            { 
                OnSleepFinished?.Invoke();
                _sleepTimer = 0;
            }
        }
    }

}
