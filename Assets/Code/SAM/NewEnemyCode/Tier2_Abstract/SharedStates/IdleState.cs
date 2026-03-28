using System;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Shared idle state — used by every enemy type.
    /// Delegates to IMovementActor so it works for BigGhost, MageGhost, SmallGhost, etc.
    ///
    /// Two sub-behaviours controlled by IdleMode:
    ///   Stop     — stand still at current position (default on spawn)
    ///   GoHome   — navigate back to spawn point; raises OnArrived when close enough
    /// </summary>
    public class IdleState : IState
    {
        public enum IdleMode { Stop, GoHome }

        private readonly IMovementActor _movement;
        private readonly IPerceptionActor _perception;
        private readonly EnemyStat _stat;
        private readonly BaseEnemyData _data;

        public IdleMode Mode { get; set; } = IdleMode.Stop;

        /// <summary>Raised when GoHome completes. State machine listens and transitions to Stop.</summary>
        public event Action OnPlayerDetected;
        public event Action OnReturnedHome;

        [Inject]
        public IdleState(IMovementActor movement, IPerceptionActor perception,
                         EnemyStat stat, BaseEnemyData data)
        {
            _movement   = movement;
            _perception = perception;
            _stat       = stat;
            _data       = data;
        }

        public void OnEnter()  { }
        public void OnExit()   
        {
            if (Mode == IdleMode.GoHome)
            {
                _movement.ClearPath();
            }
        }

        public void OnFixedTick()
        {
            // Always scan — idle enemy should notice the player in both modes
            var player = _perception.ScanForPlayer(_stat.AggroRange);
            if (player != null)
            {
                _data.Target = player;
                OnPlayerDetected?.Invoke();
                return;
            }

            // Then handle movement sub-behaviour
            switch (Mode)
            {
                case IdleMode.Stop:
                    _movement.Stop();
                    break;

                case IdleMode.GoHome:
                    Debug.Log("Going Home");
                    _movement.ReturnHome();
                    _movement.LookAt(_perception.InformSpawnLocation());
                    if (_movement.CheckReturnHome())
                    {
                        Debug.Log("Home Sweet Home");
                        _movement.Stop(); 
                        OnReturnedHome?.Invoke();
                    }
                    break;
            }
        }
    }
}
