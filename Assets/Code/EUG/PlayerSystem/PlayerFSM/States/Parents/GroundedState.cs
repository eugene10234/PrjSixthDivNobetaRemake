using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.MBFunctions.PlayerUI;
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
            ctx.AirJumpCharges = ctx.Config.maxAirJumpCharges;

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

            // 1. 如果在地面，重置垂直速度並保持貼地
            GroundDectionRoutine();
            if (ctx.InputService.IsAimPressed) stateMachine.ChangeState<AimingState>();
            if (ctx.CurrentBulletConfig.ChantFlag) stateMachine.ChangeState<ChantingState>();
            if (RunningTimer < 0.01)
            {
                if (!ctx.Detection.CheckIfMoving(ctx)) stateMachine.ChangeState<IdleState>();
            }
            //if (ctx.Detection.IsTurning) stateMachine.ChangeState<TurningState>();
        }

        private void GroundDectionRoutine()
        {
            //PublishEvent(new InteractEreaEnterEvent
            //{
            //    //InteractObj = this,
            //    Message = $"timeSinceG{timeSinceGrounded>0.15} || isFFGrd{isFarFromGround}"
            //});
            if (ctx.Detection.IsGrounded)
            {
                // 給予一個微小的向下力，確保檢測器能持續觸碰地面
                ctx.MovementService.SetVerticalVelocity(-2f);
            }
            else
            {
                float timeSinceGrounded = Time.time - ctx.Detection.LastGroundedTime;

                // 關鍵：除了時間，還要檢查下方是否真的「空無一物」
                bool isFarFromGround = !ctx.Detection.CheckGroundBelow(1f);
                // 2. 如果不在地面，判斷是「真的在掉落」還是「只是下坡」

                if (timeSinceGrounded > 0.15f && isFarFromGround)
                {
                    // 只有當垂直速度明顯向下，且下方沒有近距離地面時，才進入 FallingState
                    if (ctx.MovementService.CurrentVelocity.y < -2.5f)
                    {
                        stateMachine.ChangeState<FallingState>();
                    }
                }
            }
        }
        #region Common Input Handlers

        public override void OnJumpInput()
        {
            if (ctx.CanJump)
            {
                stateMachine.ChangeState<JumpingState>(force:true);
            }
        }

        //public override void OnDodgeInput()
        //{
        //    if (ctx.CanDodge)
        //    {
        //        stateMachine.ChangeState<DodgingState>();
        //    }
        //}
        public override void OnAimStartInput()
        {
            //Debug.LogWarning("OnAimStartInput");

            stateMachine.ChangeState<AimingState>();

        }
        public override void OnChantInput()
        {
            //Debug.LogWarning("OnAimStartInput");

            stateMachine.ChangeState<ChantingState>();
            //ctx.ChantMagic();
            //ctx.StartAsyncChant();

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
