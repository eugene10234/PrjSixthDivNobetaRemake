using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;
// ========================================================================
// GroundedState - 地面狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public abstract class GroundedState : BattleSetState, IUpdatable, IFixedUpdatable
    {
        protected float RunningTimer;
        protected const float RUNNING_DURATION = 1.4f;
        protected GroundedState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            // 重置垂直速度（貼地）
            ctx.MovementService.SetVerticalVelocity(-2f);

            // 恢復閃避次數
            ctx.DodgeCharges = ctx.Config.maxDodgeCharges;

        }

        public override void Update()
        {
            base.Update();
            DetectionHandler();
        }

        public virtual void FixedUpdate()
        {
            // 默認行為：應用重力
            ctx.MovementService.ApplyGravity(ctx.Config.gravity);

            // 執行移動
            ctx.MovementService.ExecuteMovement();
            //if (context.InputService.IsAimPressed)
            //{
            //    stateMachine.ChangeState<AimingState>();

            //}
        }

        public override void DetectionHandler()
        {
            base.DetectionHandler();
            //Vector3 direction = GetMoveDirection();

            //if (!(direction.magnitude > 0.1f)) stateMachine.ChangeState<IdleState>();
            if(ctx.ChargeRatio>0f)
            if (ctx.InputService.IsAimPressed) stateMachine.ChangeState<AimingState>();

            if (RunningTimer < 0.01)
            {
                if (!ctx.Detection.CheckIfMoving(ctx)) stateMachine.ChangeState<IdleState>();
            }
            //if (ctx.Detection.IsTurning) stateMachine.ChangeState<TurningState>();
        }
        #region Common Input Handlers

        public override void OnJumpInput()
        {
            if (ctx.CanJump)
            {
                stateMachine.ChangeState<JumpingState>();
            }
        }

        public override void OnDodgeInput()
        {
            if (ctx.CanDodge)
            {
                stateMachine.ChangeState<DodgingState>();
            }
        }
        public override void OnAimStartInput()
        {
            //Debug.LogWarning("OnAimStartInput");

            stateMachine.ChangeState<AimingState>();

        }
        public override void OnChantInput()
        {
            //Debug.LogWarning("OnAimStartInput");

            stateMachine.ChangeState<ChantingState>();

        }
        public override void OnAttackInput()
        {
            stateMachine.ChangeState<MeleeAttackState>();
        }
        //public override void OnFireStartInput()
        //{
        //    ctx.FireMagic();
        //}
        //public override void OnFireEndInput()
        //{
        //    ctx.FireMagic();
        //}

        #endregion

        #region Helper Methods

        /// <summary>
        /// 獲取移動方向（相對攝影機）
        /// </summary>
        protected Vector3 GetMoveDirection()
        {
            Vector2 input = ctx.InputService.MoveInput;
            return ctx.CameraService.GetCameraRelativeDirection(input);
        }

        /// <summary>
        /// 應用地面移動和旋轉
        /// </summary>
        protected void ApplyMovementAndRotation(float speed, bool startUpLock = false)
        {
            Vector3 direction = GetMoveDirection();

            if (direction.magnitude > 0.1f)
            {
                if (startUpLock)
                {
                    // 起步鎖定期間減速
                    speed *= 0.15f;
                }
                // 應用移動
                ctx.MovementService.ApplyGroundMovement(
                    direction,
                    speed,
                    ctx.Config.acceleration
                );

                // 應用旋轉
                ctx.MovementService.RotateTowards(
                    direction,
                    ctx.Config.rotationSpeed
                );
            }
            else
            {
                // 減速停止
                ctx.MovementService.StopMovement(ctx.Config.acceleration);
            }
        }

        #endregion
    }
}
