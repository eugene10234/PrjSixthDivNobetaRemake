
using Cysharp.Threading.Tasks;
using Nobeta.EUG.FSM.Core;
using System;
using System.Threading;
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
            stateStartStamp = Time.time;
            ctx.TimeService.DelayAction(ApplyJump, 0.2f).Forget();

            PlayAnimation("Jump", 0.2f);
            PlaySound("Jump");
        }

        private void ApplyJump()
        {
            // 應用跳躍速度
            ctx.MovementService.ApplyJump(
                ctx.Config.jumpHeight,
                ctx.Config.gravity
            );
        }

        public override void Update()
        {
            base.Update();
            if ((Time.time - stateStartStamp) > 0.3f)
            {
                // 當開始下落時，轉換到下落狀態
                if (ctx.MovementService.CurrentVelocity.y < 0)
                {
                    stateMachine.ChangeState<FallingState>();
                }
            }

        }
    }
}
