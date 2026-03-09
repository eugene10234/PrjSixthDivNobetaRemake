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
        private float _turnAngle;
        private bool StartUpLock;
        private object _yaw;
        private const float StartUp_DURATION = 0.15f;
        public WalkingState(StateContext context, StateMachine stateMachine)
            : base(context, stateMachine)
        {
        }

        public override void Enter()
        {
            //ctx.DebugService.GUI.Add(DrawDebugInfo);

            base.Enter();
            PlayAnimation("MovementState",0.1f);
            ctx.TimeService.TaskDelayLock(StartUp_DURATION, (b) => StartUpLock = b);
        }
        public override void Update()
        {

            base.Update();
            AnimatorSetting();
      
        }
        public override void FixedUpdate()
        {
            //AngleCaculate();
            ApplyMovementAndRotation(ctx.Config.walkSpeed, StartUpLock);
            base.FixedUpdate();
        }
        private void AnimatorSetting()
        {
            ctx.AnimationService.SetFloat("Speed", ctx.CurrentSpeed);
            ctx.AnimationService.SetFloat("Rotation",_turnAngle/32f );
        }
        public override void DetectionHandler()
        {
            base.DetectionHandler();
            if (ctx.InputService.IsRunPressed) stateMachine.ChangeState<RunningState>();
        }

        private void AngleCaculate()
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

        //private void DrawDebugInfo()
        //{
        //    GUILayout.Box("=== AngleTest ===");
        //    GUILayout.Label("<b>YAW:</b>");
        //    GUILayout.Label($"YAW: {_yaw}");
        //}
    }
}
