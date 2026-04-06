using Cysharp.Threading.Tasks;
using Mono.Cecil;
using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King sleep / dormant state — mirrors MGSleepState.
    ///
    /// The GK stands completely still and plays its sleep animation.
    /// The state machine wakes it by calling <see cref="OnWake"/> (e.g. when the
    /// player enters the arena trigger or the interaction point fires).
    ///
    /// Raises:
    ///   OnWakeUp — when external code calls Wake(), transitioning to chase/intro.
    /// </summary>
    public class GKSleepState : IState
    {
        public enum sleepmode 
        { 
            catatonic,
            rising,
            idle,
        }
        public sleepmode currentMode;
        private readonly GKMovementActor _movement;
        private readonly GKAnimationActor _animation;
        private readonly GKPerceptionActor _perception;
        private float _sleepTimer;
        private const float sleepDuration = 5;
        public event Action OnWakeUp;
        private bool _woken;
        private GameObject player;

        [Inject]
        public GKSleepState(GKMovementActor movement, GKAnimationActor animation, GKPerceptionActor perception)
        {
            _movement = movement;
            _animation = animation;
            _perception = perception;
        }

        /// <summary>
        /// Called from the arena interaction point / spawner to wake the boss.
        /// </summary>
        public void Wake()
        {
            _woken = true;
        }

        public void OnEnter()
        {
            _woken = false;
            _movement.Stop();
        }
        public event Action OnCombatStart;
        public event Action OnSleepFinished;
        public event Action OnAnnounced;
        public void OnFixedTick()
        {
            switch (currentMode)
            { 
                case sleepmode.catatonic:
                    _movement.Stop();
                    _animation.SetCatatonic();
                    _sleepTimer += Time.fixedDeltaTime;
                    if (_sleepTimer >= sleepDuration)
                    {
                        _animation.ResetCatatonic();
                        OnSleepFinished?.Invoke();
                        _sleepTimer = 0;
                    }
                    break;
                case sleepmode.rising:
                    if (!isRising)
                    { 
                        WaitRise().Forget();
                    }
                    break;
                case sleepmode.idle:
                    _movement.Stop();
                    player = _perception.ScanForPlayer(35);
                    if (player != null) { 
                        currentMode = sleepmode.rising;
                        OnCombatStart?.Invoke();    
                    }
                    break;
            }
            if (_woken)
            OnWakeUp?.Invoke();
            _woken = false;
        }

        public void OnExit()
        {
            _animation.ResetCatatonic();
            _woken = false;
        }

        private bool isRising;
        public async UniTask WaitRise()
        {
            isRising = true;
            await _movement.Rise();
            OnAnnounced?.Invoke();
            isRising = false;
        }
    }
}
