// ============================================================================
// Concrete States - 具體狀態實現（使用服務）
// ============================================================================

using System;
using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;
    // ========================================================================
    // HitState  簡化版
    // ========================================================================

namespace Nobeta.EUG.FSM.States
{

    public class FellingState : ReactiveState, IUpdatable
    {
        private float hitTimer;
        private CameraReactStartEvent _cameraReactStartEvent;

        public FellingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
            reactionDuration = 2f;
            _cameraReactStartEvent = new CameraReactStartEvent()
            {
                TargetFOV = 52.5f,
                TargetDistance = 2.25f,
                TargetShoulderOffset = -0.45f,
                SmoothSpeed = 0.02f
            };
        }

        public override void Enter()
        {
            base.Enter();
            ctx.CurrentSpeedType = typeof(IdleState);
            PublishEvent(_cameraReactStartEvent);
            //ctx.MovementService.ResetVelocity();
            PlayAnimation("Felling");
            //PlaySound("Hit");
        }
        public override void FixedUpdate()
        {
            ctx.MovementService.StopMovement(ctx.Config.acceleration * 0.15f);
            base.FixedUpdate();
        }
        protected override void OnReactionComplete()
        {
                stateMachine.ChangeState<FellRecoveryState>();            
        }

        public override bool CanTransitionTo(Type stateType)
        {
            //// 受傷期間只能轉到閃避or死亡狀態
            //if (stateType == typeof(DyingState))
            //    return true;

            return reactionTimer <= 0;
        }

    }
}