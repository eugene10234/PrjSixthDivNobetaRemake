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

        public bool IsGrounded => isGrounded;
        public float LastGroundedTime => lastGroundedTime;

        [Inject]
        public DetectionService(
            CharacterController controller,
            ITimeService timeService)
        {
            this.controller = controller;
            this.timeService = timeService;
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
