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

    public class HitState : ReactiveState, IUpdatable
    {
        private float hitTimer;

        public HitState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
            reactionDuration = ctx.Config.hitStunDuration;
        }

        public override void Enter()
        {
            base.Enter();
            //ctx.MovementService.ResetVelocity();
            PlayAnimation("Head_Hit_R");
            PlaySound("Hit");
        }

        protected override void OnReactionComplete()
        {
            stateMachine.ChangeState<IdleState>();
        }
        public override void OnDodgeInput()
        {
            if (ctx.CanDodge)
            {
                stateMachine.ChangeState<DodgingState>(force: true);
            }
        }
        public override bool CanTransitionTo(Type stateType)
        {
            // 受傷期間只能轉到閃避or死亡狀態
            if (stateType == typeof(DodgingState) || stateType == typeof(DyingState))
                return true;

            return reactionTimer <= 0;
        }

    }
}