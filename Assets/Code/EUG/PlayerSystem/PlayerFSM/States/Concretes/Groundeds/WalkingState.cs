using Nobeta.EUG.FSM.Core;
using System;
using System.Net;
using Unity.Mathematics;

using UnityEngine;
using VContainer.Unity;
// ========================================================================
// WalkingState - 行走狀態
// ========================================================================
namespace Nobeta.EUG.FSM.States
{
    public class WalkingState : GroundedState
    {
        DebugServiceMb _debugServiceMb;
        private float _turnAngle;

        public WalkingState(StateContext context, StateMachine stateMachine, DebugServiceMb debugServiceMb)
            : base(context, stateMachine)
        {
            _debugServiceMb = debugServiceMb;
            Initialize();
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
            caculate();

            ApplyMovementAndRotation(ctx.Config.walkSpeed);
            base.FixedUpdate();
        }
        private void AnimatorSetting()
        {
            //var endPoint = context.Transform.position + context.CameraService.GetCameraRelativeDirection(context.InputService.MoveInput) * 2f;
            //_debugServiceMb.Giz.DrawLine(ctx.Transform.position,
            //        ctx.Transform.position + ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput) * 2f, Color.red);

            //caculate();
            //Debug.Log($"Speed: {context.CurrentSpeed}");
            if (Mathf.Abs(_turnAngle) >= 160f)
            {
                //_turnAngle100 = _turnAngle ;
                //Debug.Log($"Speed: {context.CurrentSpeed}");
                Debug.Log($"turnAngle: {_turnAngle }");
            }

            ctx.AnimationService.SetFloat("Speed", ctx.CurrentSpeed);
            ctx.AnimationService.SetFloat("Rotation", _turnAngle / 165f);
            //debugServiceMb.DrawDebugText($"turnAngle: {turnAngle/Time.deltaTime}");
            //bool playRunAnimation = isRunning & movementDirection.magnitude > 0;
            //animator.SetBool("isRunning", playRunAnimation);
        }

        private void caculate()
        {
            Vector3 fff = ctx.Transform.forward;
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            _turnAngle = Vector3.SignedAngle(fff, movement, Vector3.up);
        }

        public override void OnStopInput()
        {
            stateMachine.ChangeState<IdleState>();
        }

        public override void OnRunInput()
        {
            stateMachine.ChangeState<RunningState>();
        }

        public void Initialize()
        {
            //_debugServiceMb.Giz.Add(() =>
            //{
            //    Gizmos.color = Color.red; // 甚至可以順便設顏色
            //    Gizmos.DrawLine(ctx.Transform.position,
            //        ctx.Transform.position + ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput) * 2f);
            //    //_debugServiceMb.ClearGiz();
            //});
            //_debugServiceMb.Giz.Add(() =>
            //{
            //    Gizmos.color = Color.blue; // 甚至可以順便設顏色
            //    Gizmos.DrawLine(ctx.Transform.position,
            //        ctx.Transform.position + ctx.Transform.forward);
            //    //_debugServiceMb.ClearGiz();
            //});
            //_debugServiceMb.GUI.Add(DrawDebugInfo);
        }
        private void DrawDebugInfo()
        {
            GUILayout.Box("=== AngleTest ===");
            GUILayout.Label("<b>turnAngle>165:</b>");
            GUILayout.Label($"turnAngle: {_turnAngle}");
        }
    }
}
