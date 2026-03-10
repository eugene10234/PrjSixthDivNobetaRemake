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
            Vector3 targetDir = GetMoveDirection();
            // 2. 計算當前物體的前方
            Vector3 currentForward = ctx.Transform.forward;
            // 3. 計算叉積
            Vector3 crossProduct = Vector3.Cross(currentForward, targetDir);

            if (crossProduct.y >= 0)
            {
                // 目標在右側，需要右轉
                PlayAnimation("Turn_R", 0.1f);
            }
            else if (crossProduct.y < 0)
            {
                // 目標在左側，需要左轉
                PlayAnimation("Turn_L", 0.1f);
            }
            //Debug.Log($"進入轉向狀態{direction.x}");
            //if (direction.x > 0) PlayAnimation("Turn_R", 0.1f);
            //else PlayAnimation("Turn_L", 0.1f);
            ctx.TimeService.TaskDelayLock(Turning_DURATION, (b) => TurningTimerLock = b);
            // 煞車
            ctx.MovementService.StopMovement(ctx.Config.acceleration * 0.5f);
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
            if (TurningTimerLock) ctx.MovementService.StopMovement(ctx.Config.acceleration*0.45f);
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
