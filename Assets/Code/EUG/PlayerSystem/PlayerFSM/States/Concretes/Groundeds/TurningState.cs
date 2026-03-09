using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
using static UnityEngine.Debug;
// ========================================================================
// IdleState - 待機狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class TurningState : GroundedState
    {
        private bool TurningTimerLock;
        private const float Turning_DURATION = 0.4f;

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
        }
        public override void Update()
        {
            if (TurningTimerLock) return;
            //base.Update();
            //if (ctx.InputService.MoveInput.magnitude > 0.1f)
            //{
                stateMachine.ChangeState<IdleState>();

            //}
            //stateMachine.ChangeStateByParent<GroundedState>(typeof(WalkingState));
            //PlayAnimation("MovementState", 0.1f);
            //AnimatorSetting();
        }
        private void AnimatorSetting()
        {
            Vector3 fff = ctx.Transform.forward;
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            var turnAngle = Vector3.SignedAngle(fff, movement, Vector3.up);
            ctx.AnimationService.SetFloat("Speed", ctx.CurrentSpeed);
            ctx.AnimationService.SetFloat("Rotation", turnAngle / 32f);
        }

        public override void FixedUpdate()
        {
            // 停止移動
            if (TurningTimerLock) ctx.MovementService.StopMovement(ctx.Config.acceleration*0.5f);
            //else ApplyMovementAndRotation(ctx.Config.walkSpeed);
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
