using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.Interfaces;
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
        //private readonly StateContext ctx;
        private readonly CharacterController controller;
        private readonly ITimeService timeService;
        private readonly Transform transform;

        private bool isGrounded;
        private float lastGroundedTime;
        private float _turnAngle;
        private Vector3 moveInput;


        public bool IsGrounded => isGrounded;
        public float LastGroundedTime => lastGroundedTime;
        public bool IsMoving => moveInput.sqrMagnitude > 0.01f;
        public bool IsTurning => _turnAngle >= 160f;
        [Inject]
        public DetectionService(
           // StateContext stateContext,
            CharacterController controller,
            Transform transform,
            ITimeService timeService)
        {
            //ctx = stateContext;
            this.controller = controller;
            this.timeService = timeService;
            this.transform = transform;
        }

        public void UpdateGlobalDetection(StateContext ctx)
        {
            UpdateGroundDetection();
            //UpdateMoveDetection(ctx);
            UpdateTurnDetection(ctx);
        }
        public void UpdateTurnDetection(StateContext ctx)
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
        public bool CheckGroundBelow(float distance)
        {
            // 取得角色的中心點與碰撞體半徑（假設是 CharacterController 或 CapsuleCollider）
            float radius = controller.radius * 0.9f; // 稍微縮小一點半徑，避免摩擦牆壁
            Vector3 origin = transform.position + Vector3.up * radius;

            // 向下發射球體
            return Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out RaycastHit hit,
                distance,
                LayerMask.GetMask("Ground","Obstacle","Default")
            );
        }
        public bool CheckIfMoving(StateContext ctx)
        {
            UpdateMoveDetection(ctx);
            return moveInput.sqrMagnitude > 0.01f;
        }
        public bool CheckIfTurning(StateContext ctx)
        {
            UpdateTurnDetection(ctx);
            return Mathf.Abs(_turnAngle)>= 165f;
        }
    }
}
