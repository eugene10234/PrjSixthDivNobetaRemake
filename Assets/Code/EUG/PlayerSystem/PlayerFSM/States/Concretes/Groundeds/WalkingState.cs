using Nobeta.EUG.FSM.Core;
using System;
using UnityEngine;
// ========================================================================
// WalkingState - 行走狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class WalkingState : GroundedState
    {
        public WalkingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            PlayAnimation("Walk");
        }
        public override void Update()
        {
            base.Update();
            //AnimatorSetting();
        }
        public override void FixedUpdate()
        {
            ApplyMovementAndRotation(context.Config.walkSpeed);
            base.FixedUpdate();
        }
        //private void AnimatorSetting()
        //{
        //    Vector3 Velocity = context.MovementService.CurrentVelocity;
        //    context.AnimationService.SetFloat("Rotation", 0f);

        //    //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
        //    //animator.SetBool("isRunning", playRunAnimation);
        //}
        public override void OnStopInput()
        {
            stateMachine.ChangeState<IdleState>();
        }

        public override void OnRunInput()
        {
            stateMachine.ChangeState<RunningState>();
        }
    }
}
