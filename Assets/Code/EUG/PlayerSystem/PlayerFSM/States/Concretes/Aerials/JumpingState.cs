
using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// JumpingState - 跳躍狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class JumpingState : AerialState
    {
        public JumpingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // 應用跳躍速度
            ctx.MovementService.ApplyJump(
                ctx.Config.jumpHeight,
                ctx.Config.gravity
            );

            PlayAnimation("Jump");
            PlaySound("Jump");
        }

        public override void Update()
        {
            base.Update();

            // 當開始下落時，轉換到下落狀態
            if (ctx.MovementService.CurrentVelocity.y < 0)
            {
                stateMachine.ChangeState<FallingState>();
            }
        }
    }
}
