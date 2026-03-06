using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// GroundedState - 地面狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public abstract class GroundedState : BaseState, IUpdatable, IFixedUpdatable
    {
        protected GroundedState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // 重置垂直速度（貼地）
            context.MovementService.SetVerticalVelocity(-2f);

            // 恢復閃避次數
            context.DodgeCharges = context.Config.maxDodgeCharges;

        }

        public virtual void Update()
        {
            // 子類實現
        }

        public virtual void FixedUpdate()
        {
            // 默認行為：應用重力
            context.MovementService.ApplyGravity(context.Config.gravity);

            // 執行移動
            context.MovementService.ExecuteMovement();
            //if (context.InputService.IsAimPressed)
            //{
            //    stateMachine.ChangeState<AimingState>();

            //}
        }

        #region Common Input Handlers

        public override void OnJumpInput()
        {
            if (context.CanJump)
            {
                stateMachine.ChangeState<JumpingState>();
            }
        }

        public override void OnDodgeInput()
        {
            if (context.CanDodge)
            {
                stateMachine.ChangeState<DodgingState>();
            }
        }
        public override void OnAimStartInput()
        {
            Debug.LogWarning("OnAimStartInput");
            //if (context.CanDodge)
            //{
            //    stateMachine.ChangeState<AimingState>();
            //}
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// 獲取移動方向（相對攝影機）
        /// </summary>
        protected Vector3 GetMoveDirection()
        {
            Vector2 input = context.InputService.MoveInput;
            return context.CameraService.GetCameraRelativeDirection(input);
        }

        /// <summary>
        /// 應用地面移動和旋轉
        /// </summary>
        protected void ApplyMovementAndRotation(float speed)
        {
            Vector3 direction = GetMoveDirection();

            if (direction.magnitude > 0.1f)
            {
                // 應用移動
                context.MovementService.ApplyGroundMovement(
                    direction,
                    speed,
                    context.Config.acceleration
                );

                // 應用旋轉
                context.MovementService.RotateTowards(
                    direction,
                    context.Config.rotationSpeed
                );
            }
            else
            {
                // 減速停止
                context.MovementService.StopMovement(context.Config.acceleration);
            }
        }

        #endregion
    }
}
