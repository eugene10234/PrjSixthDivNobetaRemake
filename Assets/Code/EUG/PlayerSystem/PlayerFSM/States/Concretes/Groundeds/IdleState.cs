using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// IdleState - 待機狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class IdleState : GroundedState
    {
        public IdleState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("IdleState",0.1f);
        }
        public override void Update()
        {
            base.Update();
            if (ctx.InputService.MoveInput.magnitude > 0.1f)
            {
                stateMachine.ChangeState<WalkingState>();

            }
        }

        public override void FixedUpdate()
        {
            // 停止移動
            ctx.MovementService.StopMovement(ctx.Config.acceleration);

            base.FixedUpdate();
        }

        // 輸入事件處理
        public override void OnMoveInput()
        {
            // 開始移動
            stateMachine.ChangeState<WalkingState>();
        }
    }

}
