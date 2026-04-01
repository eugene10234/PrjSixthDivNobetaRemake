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
    public class DyingState : ReactiveState
    {
        public DyingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            //ctx.MovementService.ResetVelocity();

            PlayAnimation("Dying");
            PlaySound("Die");
            ctx.EventBus.Publish(new PlayerLiveStateEvent {IsLive = false });
        }
        public override void Exit()
        {
            base.Exit();
            ctx.EventBus.Publish(new PlayerLiveStateEvent { IsLive = true });
        }
        protected override void OnReactionComplete()
        {
            // 死亡狀態不會自動完成，需要外部觸發重生
        }

        public override bool CanTransitionTo(System.Type stateType)
        {
            // 只能轉換到待機（重生）
            return stateType == typeof(IdleState);
        }
    }
}