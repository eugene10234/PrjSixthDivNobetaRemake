using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// IdleState - 待機狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class TurningState : GroundedState
    {
        private bool TurningTimerLock;
        private const float Turning_DURATION = 0.7f;
        public TurningState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("TurningState", 0.1f);
            ctx.TimeService.TaskDelayLock(Turning_DURATION, (b) => TurningTimerLock = b);
            // 煞車
            ctx.MovementService.StopMovement(ctx.Config.acceleration * 1.5f);
            PlayAnimation("Turning", 0.15f);
        }
        public override void Update()
        {
            if (TurningTimerLock) return;
            //base.Update();
            //if (ctx.InputService.MoveInput.magnitude > 0.1f)
            //{
            //    stateMachine.ChangeState<WalkingState>();

            //}
                stateMachine.ChangeStateByParent<GroundedState>(typeof(WalkingState));
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
            //stateMachine.ChangeState<WalkingState>();
        }
    }

}
