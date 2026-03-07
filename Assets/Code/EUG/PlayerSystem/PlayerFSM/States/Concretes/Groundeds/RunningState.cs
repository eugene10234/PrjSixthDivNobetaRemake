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
            PlayAnimation("MovementState");
        }
        public override void Update()
        {
            base.Update();
            AnimatorSetting();
        }
        public override void FixedUpdate()
        {
            ApplyMovementAndRotation(ctx.Config.runSpeed);
            base.FixedUpdate();
        }
        private void AnimatorSetting()
        {
            //var endPoint = context.Transform.position + context.CameraService.GetCameraRelativeDirection(context.InputService.MoveInput) * 2f;
            //_debugServiceMb.AddGiz(() =>
            //{
            //    Gizmos.color = Color.red; // 甚至可以順便設顏色
            //    Gizmos.DrawLine(context.Transform.position,
            //        context.Transform.position + context.CameraService.GetCameraRelativeDirection(context.InputService.MoveInput) * 2f);
            //    //_debugServiceMb.ClearGiz();
            //});
            Vector3 currentVelocity = ctx.MovementService.CurrentVelocity;
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            float turnAngle = Vector3.SignedAngle(ctx.Transform.forward, movement, Vector3.up);
            Debug.Log($"Speed: {ctx.CurrentSpeed}");
            if (Mathf.Abs(turnAngle) >= 60f)
            {
                //Debug.Log($"Speed: {context.CurrentSpeed}");
                Debug.Log($"turnAngle: {turnAngle / Time.deltaTime}");
            }

            ctx.AnimationService.SetFloat("Speed", ctx.CurrentSpeed);
            ctx.AnimationService.SetFloat("Rotation", turnAngle / 165f);
            //debugServiceMb.DrawDebugText($"turnAngle: {turnAngle/Time.deltaTime}");
            //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
            //animator.SetBool("isRunning", playRunAnimation);
        }

        public override void OnStopInput()
        {
            stateMachine.ChangeState<IdleState>();
        }
    }

}
