using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using UnityEngine;
using VContainer;
// ========================================================================
// DetectionService - 檢測服務
// ========================================================================
namespace Nobeta.EUG.FSM.Services
{
    public class DetectionService : IDetectionService
    {
        private readonly CharacterController controller;
        private readonly ITimeService timeService;

        private bool isGrounded;
        private float lastGroundedTime;
        private float _turnAngle;
        private Vector3 moveInput;


        public bool IsGrounded => isGrounded;
        public float LastGroundedTime => lastGroundedTime;
        public bool IsMoving => moveInput.magnitude > 0.1f;
        public bool IsTurning => _turnAngle >= 165f;
        [Inject]
        public DetectionService(
            CharacterController controller,
            ITimeService timeService)
        {
            this.controller = controller;
            this.timeService = timeService;
        }

        public void UpdateGlobalDetection(StateContext ctx)
        {
            UpdateGroundDetection();
            UpdateMoveDetection(ctx);
            UpdateTurnDetection(ctx);
        }
        private void UpdateTurnDetection(StateContext ctx)
        {
            Vector3 movement = ctx.CameraService.GetCameraRelativeDirection(ctx.InputService.MoveInput);
            _turnAngle = Vector3.SignedAngle(ctx.Transform.forward, movement, Vector3.up);
        }
        private void UpdateMoveDetection(StateContext ctx)
        {
            moveInput = ctx.InputService.MoveInput;
        }

        public void UpdateGroundDetection()
        {
            //bool wasGrounded = isGrounded;
            isGrounded = controller.isGrounded;

            if (isGrounded)
            {
                lastGroundedTime = timeService.GameTime;
            }
        }
        
    }
}
