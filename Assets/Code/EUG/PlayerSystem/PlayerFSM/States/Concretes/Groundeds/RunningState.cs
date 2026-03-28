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

        private float _turnAngle;

        public RunningState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            ctx.CurrentSpeedType = typeof(RunningState);
            RunningTimer = RUNNING_DURATION;
            base.Enter();
            PlayAnimation("MovementState");
        }
        public override void Update()
        {
            RunningTimer -= Time.deltaTime;
            //ctx.ConsumeStamina(3);
            base.Update();
            AnimatorSetting();
        }
        public override void FixedUpdate()
        {
            caculate();
            ctx.Detection.UpdateTurnDetection(ctx);
            ApplyMovementAndRotation(ctx.Config.runSpeed);
            base.FixedUpdate();
        }
        private void caculate()
        {
            Vector3 fff = ctx.Transform.forward;
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            _turnAngle = Vector3.SignedAngle(fff, movement, Vector3.up);
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
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            float turnAngle = Vector3.SignedAngle(ctx.Transform.forward, movement, Vector3.up);
            ctx.AnimationService.SetFloat("Speed", ctx.CurrentSpeed);
            ctx.AnimationService.SetFloat("Rotation", turnAngle / 32f);
            //debugServiceMb.DrawDebugText($"turnAngle: {turnAngle/Time.deltaTime}");
            //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
            //animator.SetBool("isRunning", playRunAnimation);
        }
        public override void DetectionHandler()
        {
                base.DetectionHandler();
            if (ctx.CurrentSpeed < 1.5f) stateMachine.ChangeState<IdleState>();
            var absTurnAngle = Mathf.Abs(_turnAngle);
            if (absTurnAngle > 0)
                //Debug.Log($"turnAngle: {absTurnAngle}");
            if (absTurnAngle > 150f)
            {
                stateMachine.ChangeState<TurningState>();
            }
        }

        public override void OnStopInput()
        {
            //stateMachine.ChangeState<IdleState>();
        }
        public override void OnMoveInput()
        {
            RunningTimer = RUNNING_DURATION;
        }
    }

}
