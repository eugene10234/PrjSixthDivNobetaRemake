using Nobeta.EUG.FSM.Core;
using Nobeta.EUG.FSM.Interfaces;
using System;
using UnityEngine;
using VContainer;
// ========================================================================
// CameraService - 攝影機服務
// ========================================================================

namespace Nobeta.EUG.FSM.Services
{
    public class CameraService : ICameraService
    {
        private readonly Camera mainCamera;

        [Inject]
        public CameraService()
        {
            mainCamera = Camera.main;
        }

        public Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (mainCamera == null) return new Vector3(input.x, 0, input.y);

            Vector3 forward = mainCamera.transform.forward;
            Vector3 right = mainCamera.transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        public Vector3 GetCameraForward()
        {
            if (mainCamera == null) return Vector3.forward;

            Vector3 forward = mainCamera.transform.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        public Vector3 GetCameraRight()
        {
            if (mainCamera == null) return Vector3.right;

            Vector3 right = mainCamera.transform.right;
            right.y = 0f;
            return right.normalized;
        }
    }
}
