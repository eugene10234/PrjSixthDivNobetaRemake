using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// AerialState - 空中狀態父類（更新版）
// ========================================================================
namespace Nobeta.EUG.FSM.States
{

    public abstract class AerialState : BaseState, IUpdatable, IFixedUpdatable
    {
        private float _previousSpeed;
        private Vector3 _previousMoveDirection;
        protected AerialState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }
        public override void Enter()
        {
            base.Enter();
            _previousSpeed = context.CurrentSpeed;
            _previousMoveDirection = GetMoveDirection();
        }
        public virtual void Update()
        {
            // 檢查著地
            if (context.IsGrounded && context.MovementService.CurrentVelocity.y <= 0)
            {
                stateMachine.ChangeState<LandingState>();
            }
        }

        public virtual void FixedUpdate()
        {
            // 應用空中移動
            Vector3 direction = GetMoveDirection();

            context.MovementService.ApplyAirMovement(
                _previousSpeed,
                _previousMoveDirection,
                direction,
                context.Config.walkSpeed,
                0.5f // 空中控制係數
            );

            // 應用重力
            context.MovementService.ApplyGravity(context.Config.gravity);

            // 執行移動
            context.MovementService.ExecuteMovement();
        }

        #region Input Handlers

        public override void OnDodgeInput()
        {
            // 空中閃避
            if (context.CanDodge)
            {
                stateMachine.ChangeState<DodgingState>();
            }
        }

        #endregion

        #region Helper Methods

        protected Vector3 GetMoveDirection()
        {
            Vector2 input = context.InputService.MoveInput;
            return context.CameraService.GetCameraRelativeDirection(input);
        }

        #endregion
    }
}