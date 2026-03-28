// ============================================================================
// Concrete States - 具體狀態實現（使用服務）
// ============================================================================

using Nobeta.EUG.Events;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;
// ========================================================================
//DyingState - 簡化版
// ========================================================================

namespace Nobeta.EUG.FSM.States
{
    public class DyingState : BaseState
    {
        public DyingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ctx.MovementService.ResetVelocity();

            PlayAnimation("Dying");
            PlaySound("Die");
            ctx.EventBus.Publish(new PlayerLiveStateEvent {IsLive = false });
        }
        public override void Exit()
        {
            base.Exit();
            ctx.EventBus.Publish(new PlayerLiveStateEvent { IsLive = true });
        }
    }
}