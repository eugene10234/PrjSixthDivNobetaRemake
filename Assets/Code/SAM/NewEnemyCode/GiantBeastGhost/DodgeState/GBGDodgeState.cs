using Cysharp.Threading.Tasks;
using Nobeta.Sam.Enemy;
using System;
using System.Threading;
using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    public class GBGDodgeState:IState
    {
        private readonly GBGMovementActor _movement;
        private readonly GBGAnimationActor _animation;
        private readonly GBGPerceptionActor _perception;
        private readonly EnemyStat _stat;
        private readonly BaseEnemyData _data;

        [Inject]
        public GBGDodgeState(
            GBGMovementActor movement,
            GBGAnimationActor animation,
            GBGPerceptionActor perception,
            EnemyStat stat,
            BaseEnemyData data)
        {
            _movement = movement;
            _animation = animation;
            _perception = perception;
            _stat = stat;
            _data = data;
        }

        public void OnEnter()
        {
            _data.isInVinsible = true;
            _data.isImmuneStagger = true;
        }

        public void OnFixedTick()
        {
           
        }

        private bool isDodging;
        public async UniTaskVoid SideStep(Vector3 hitposition, CancellationToken token)
        {
            if (isDodging) return;

            isDodging = true;

            try
            {
                // Wait for the movement actor to finish the 0.5s burst
                _movement.DodgeRotation(hitposition);
                _animation.PlayDodgeFX();
                await _movement.StartDodgeAsync(hitposition, token);
            }
            catch (OperationCanceledException)
            {
                // Handle if the task is cancelled (e.g., enemy died)
                return;
            }
            finally
            {
                // This ensures isDodging is reset even if the task is cancelled or fails
                isDodging = false;
            }

            OnFinishedDodging?.Invoke();
        }

        public event Action OnFinishedDodging;
        public void OnExit()
        {
            _data.isInVinsible = false;
            _data.isImmuneStagger= false;
        }
    }
}
