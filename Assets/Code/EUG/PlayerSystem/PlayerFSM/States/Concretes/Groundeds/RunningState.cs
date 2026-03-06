using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// RunningState - 奔跑狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class RunningState : GroundedState
    {
        public RunningState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("Run");
        }
        public override void Update()
        {
            base.Update();
            //AnimatorSetting();
        }
        public override void FixedUpdate()
        {
            ApplyMovementAndRotation(context.Config.runSpeed);
            base.FixedUpdate();
        }
        private void AnimatorSetting()
        {
            Vector3 Velocity = context.MovementService.CurrentVelocity;
            context.AnimationService.SetFloat("Rotation",0f);

            //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
            //animator.SetBool("isRunning", playRunAnimation);
        }
        //public override void Update()
        //{
        //    base.Update();
        //    var moveInputMagnitude = context.InputService.MoveInput.magnitude;
        //    if (moveInputMagnitude > 0.1f && moveInputMagnitude <= context.Config.walkSpeed)
        //    {
        //        //stateMachine.ChangeState<WalkingState>();
        //    }
        //    else
        //    {
        //        stateMachine.ChangeState<IdleState>();
        //    }

        //}

        public override void OnStopInput()
        {
            stateMachine.ChangeState<IdleState>();
        }
    }

}
