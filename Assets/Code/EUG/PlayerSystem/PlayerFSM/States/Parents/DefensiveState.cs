using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using System;
using UnityEngine;
// ========================================================================
// GroundedState - 地面狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public abstract class DefensiveState : BaseState, IUpdatable, IFixedUpdatable
    {


        protected DefensiveState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }



        public virtual void Update()
        {
            // 子類實現
        }

        public virtual void FixedUpdate()
        {
            // 默認行為：應用重力
            ctx.MovementService.ApplyGravity(ctx.Config.gravity);

            // 執行移動
            ctx.MovementService.ExecuteMovement();
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
        protected void ApplyMovementAndRotation(float speed)
        {
            Vector3 direction = GetMoveDirection();

            if (direction.magnitude > 0.1f)
            {
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
//// ========================================================================
//// DefensiveState - 防禦狀態父類
//// ========================================================================
//namespace Nobeta.EUG.FSM.Player.States
//{
//    /// <summary>
//    /// 防禦狀態 - 防禦動作的父類
//    /// 包含：Dodging
//    /// </summary>
//    public abstract class DefensiveState : BaseState, IUpdatable, IFixedUpdatable
//    {
//        protected float defenseDuration;
//        protected float defenseTimer;

//        protected DefensiveState(StateContext context, StateMachine stateMachine)
//            : base(context, stateMachine) { }

//        public override void Enter()
//        {
//            defenseTimer = defenseDuration;
//        }

//        public virtual void Update()
//        {
//            UpdateDefenseTimer();
//        }

//        public abstract void FixedUpdate();

//        /// <summary>
//        /// 更新防禦計時器
//        /// </summary>
//        protected virtual void UpdateDefenseTimer()
//        {
//            if (defenseTimer > 0)
//            {
//                defenseTimer -= Time.deltaTime;

//                if (defenseTimer <= 0)
//                {
//                    OnDefenseComplete();
//                }
//            }
//        }

//        /// <summary>
//        /// 防禦完成時調用
//        /// </summary>
//        protected virtual void OnDefenseComplete()
//        {
//            // 返回到待機狀態
//            if (context.MoveDirection.magnitude > 0.1f)
//            {
//                stateMachine.ChangeState<WalkingState>();
//            }
//            else
//            {
//                stateMachine.ChangeState<IdleState>();
//            }
//        }

//        /// <summary>
//        /// 檢查是否在無敵幀內
//        /// </summary>
//        protected virtual bool IsInvincible()
//        {
//            return true; // 默認整個防禦期間都無敵
//        }

//        public override bool CanTransitionTo(Type stateType)
//        {
//            // 防禦期間不能被打斷（除非是死亡）
//            if (stateType == typeof(DyingState))
//                return true;

//            return defenseTimer <= 0;
//        }
//    }