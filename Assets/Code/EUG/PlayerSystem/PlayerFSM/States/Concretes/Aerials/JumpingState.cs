
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
            context.MovementService.ApplyJump(
                context.Config.jumpHeight,
                context.Config.gravity
            );

            PlayAnimation("Jump");
            PlaySound("Jump");
        }

        public override void Update()
        {
            base.Update();

            // 當開始下落時，轉換到下落狀態
            if (context.MovementService.CurrentVelocity.y < 0)
            {
                stateMachine.ChangeState<FallingState>();
            }
        }
    }
}
