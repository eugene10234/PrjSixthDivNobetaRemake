using System;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.Interfaces;
using UnityEngine;
// ========================================================================
// AerialState - 空中狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{

    public abstract class AerialState : BattleSetState, IUpdatable, IFixedUpdatable
    {
        protected float stateStartStamp;
        private float _previousSpeed;
        private Vector3 _previousMoveDirection;
        protected AerialState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
            stateStartStamp =Time.time;
        }
        public override void Enter()
        {
            base.Enter();
            _previousSpeed = ctx.CurrentSpeed;
            _previousMoveDirection = GetMoveDirection();
        }
        public override void Update()
        {
            base.Update();
            // 檢查著地
            if ((Time.time-stateStartStamp)>0.3f)
            {
                if (ctx.IsGrounded && ctx.MovementService.CurrentVelocity.y <= 0)
                {
                    stateMachine.ChangeState<LandingState>();
                }
            }

        }

        public virtual void FixedUpdate()
        {
            // 應用空中移動
            Vector3 direction = GetMoveDirection();

            ctx.MovementService.ApplyAirMovement(
                _previousSpeed,
                _previousMoveDirection,
                direction,
                ctx.Config.walkSpeed,
                0.5f // 空中控制係數
            );

            // 應用重力
            ctx.MovementService.ApplyGravity(ctx.Config.gravity);

            // 執行移動
            ctx.MovementService.ExecuteMovement();
        }

        #region Input Handlers
        public override void OnJumpInput()
        {
            if (ctx.CanAirJump)
            {
                stateMachine.ChangeState<JumpingState>(force: true);
            }
        }
        //public override void OnDodgeInput()
        //{
        //    // 空中閃避
        //    if (ctx.CanDodge)
        //    {
        //        stateMachine.ChangeState<DodgingState>(force:true);
        //    }
        //}
        #endregion

        #region Helper Methods

        protected Vector3 GetMoveDirection()
        {
            Vector2 input = ctx.InputService.MoveInput;
            return ctx.CameraService.GetCameraRelativeDirection(input);
        }

        #endregion
    }
}