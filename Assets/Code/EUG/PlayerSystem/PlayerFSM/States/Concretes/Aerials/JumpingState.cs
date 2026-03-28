
using Cysharp.Threading.Tasks;
using Nobeta.EUG.FSM.Core;

using UnityEngine;
// ========================================================================
// JumpingState - 跳躍狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class JumpingState : AerialState
    {
        private bool _isAirJumping;
        private Vector3 _TbufAirmoveDir;
        private string _aniName;
        private float _crossDuration;

        public JumpingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            BaseState prevState = stateMachine.PreviousState;
            if (typeof(AerialState).IsAssignableFrom(prevState.GetType()))
            {
                ctx.AirJumpCharges--;
                ctx.LastAirJumpTime = Time.time;
                _isAirJumping = true;
                _TbufAirmoveDir = GetMoveDirection();
                _aniName = "AirJump";
                _crossDuration = 0.15f;
            }
            else
            {
                _isAirJumping = false;
                _aniName = "Jump";
                _crossDuration = 0.2f;
            }
            stateStartStamp = Time.time;
            ctx.TimeService.DelayAction(ApplyJump, _crossDuration).Forget();

            PlayAnimation(_aniName, _crossDuration);
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
            if (_isAirJumping && (Time.time - stateStartStamp) < 0.15f)
            {
                ctx.MovementService.RotateTowards(
                    _TbufAirmoveDir,
                    ctx.Config.rotationSpeed*1.5f
                    );
            }
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
